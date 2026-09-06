# Persistence Data Access

This document explains the current data-access shape used by BookMyCinema persistence:

- aggregate repositories
- reader and writer services
- unit of work
- transaction abstraction

## Purpose

BookMyCinema does not use one universal data-access style for every operation.
Instead, it uses the access style that best matches the operation's responsibility:

- aggregate repositories for aggregate lifecycle and invariant-preserving reads/writes
- readers for query-focused read only use cases and optimized projections
- writers for direct persistence operations that do not need aggregate materialization (that bypass domain) whether for operational reference data or other concerns.
- unit of work and transactions for coordinating persistence work at application use-case level

This fits the repository's Clean Architecture and logical CQRS approach:

- Domain and Application own the abstractions they need
- Persistence owns the concrete implementation details
- use cases choose the right access path and abstraction for their needs

## Repositories

Repositories in BookMyCinema follow domain driven design guidelines, so repositories are aggregate-oriented, not table-oriented CRUD wrappers.

Their job is to support aggregate loading and persistence when a use case needs the Domain model to enforce business rules, state transitions, and invariants.
they are not to be treated as generic query services.

## Why the generic repository is in SharedKernel

The minimal generic repository contract lives in `BookMyCinema.SharedKernel` because it is a shared domain building block that all bounded contexts can rely on.

That is a deliberate commitment for the current architecture direction:

- multiple bounded contexts are expected to need the same minimal aggregate-add contract
- keeping it in SharedKernel avoids redefining the same base abstraction in each bounded context
- it stays safe to share because it is intentionally small and aggregate-oriented 

Note: the generic repository base contract could instead be defined separately inside each domain layer that actually needs it (if modular monolith architecture was used, specially if not all bounded contexts needs it).

The generic repository stays minimal, it does not expose generic update, delete, or arbitrary querying operations.
That is intentional because most other operations are aggregate-specific, not safely generic.
and in particular, deletion is not always treated as a generic repository concern, it might be soft delete(have activation/deactivation semantics rather than deletion.)
so sometimes it needs:
- aggregate-specific invariant enforcement
- domain rules about lifecycle and allowed transitions
- logic that spans the aggregate root and its internal entities

The same reasoning applies to many updates and lookups:

- some writes must go through aggregate behavior
- some reads are better handled as direct projections
- forcing them into a generic repository would either weaken the model or create leaky abstractions

also it does not expose any ORM specific features(persistance ignorant), and it does not expose expressions so query building is not leaked to the application layer.


## Readers and writers

Not every operation should materialize a Domain aggregate.

When an operation does not need aggregate behavior or invariant enforcement, or deal with reference data (look ups, operational data) that has no domain entities,
Application may define a dedicated persistence-facing abstraction and Persistence implements it using the most suitable data-access approach.

examples:

- readers for query-side projections and optimized reads
- writers for focused direct write operations that do not justify aggregate loading or has no domain entity

## Unit of work

`IUnitOfWork` is an Application concern.

It exists to let Application use cases coordinate persistence changes and transactions.

The current abstraction is intentionally small:

- `SaveChangesAsync`
- `BeginTransactionAsync`

This reflects its real responsibility: coordinating persistence work and transactions while keeping EF Core transaction details hidden behind the abstraction.

## Isolation level in the abstraction

Using `IsolationLevel` in `IUnitOfWork.BeginTransactionAsync(...)` is acceptable in application layer IUnitOfWork abstraction because it is a BCL type(`IsolationLevel` comes from the BCL (`System.Data`)), not an ORM type.

So Application can express the transaction isolation requirement explicitly without leaking persistence implementation details.

## Transaction abstraction

Although `IUnitOfWork` can start a transaction, the transaction itself is represented by a separate abstraction: `ITransaction`.
This abstraction matters because:

- transaction control is not the same concern as unit-of-work coordination.
- ORM specific transaction implmentation such as EF Core's `IDbContextTransaction` should stay hidden behind the transaction abstraction.

`ITransaction` currently owns:

- `CommitAsync`
- `RollbackAsync`
- `CreateSavepointAsync`
- `RollbackToSavepointAsync`

Those responsibilities intentionally belong to the transaction abstraction rather than to `IUnitOfWork`.

- `IUnitOfWork` the transaction and saves changes
- `ITransaction` controls the lifetime and behavior of that transaction

Moving commit, rollback, and savepoint operations onto `ITransaction` is intentional for responsibility clarity.

## Transaction boundary ownership

Application use-case handlers own the transaction boundary.

That means the use case handler decides when to:

- create a transaction
- commit it
- roll it back
- create savepoints
- roll back to savepoints

This gives flexibility per use case.

Different use cases may need different transactional behavior, such as: no transaction at all or one simple transaction around a command or more complex transaction management.

The abstraction still hides persistence details, while the use case keeps control over orchestration and transaction flow.

## Async disposal

Use cases that begin a transaction should respect that transactions are asynchronously disposable.

The expected usage shape is:

```csharp
await using var transaction =
    await unitOfWork.BeginTransactionAsync(...);
```

This keeps transaction lifetime explicit and aligned with the abstraction contract.
