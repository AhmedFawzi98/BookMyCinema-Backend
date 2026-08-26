# Domain-Driven Design

This document records the DDD applying approach that BookMyCinema is intended to follow.

## Scope

This document focuses on what is adopted in BookMyCinema project:

- strategic DDD decisions 
- tactical DDD patterns 
- shape of domain entities, value objects, aggregates, identities, and domain services

## Relationship to the architecture docs

The architecture document explains project boundaries, dependency direction, placement rules, and the read/write split at the solution level.
This document goes one level deeper into how the domain model itself should be designed inside those boundaries.

Use this together with:

- [Clean Architecture](../architecture/clean-architecture.md)
- [Logical Database Schema](../persistence/logical-schema.md)
- [Results Pattern](../error-handling/results-pattern.md)

## Repository stance on DDD

BookMyCinema is intended to be modeled using DDD because the project is centered on business rules, behavior, lifecycle, permissions, scheduling, booking, ticketing, and other concepts that should be expressed in domain terms rather than as a collection of technical CRUD tables.

Some of the main targeted points are:

- business terminology should drive code and documentation names
- the model should be explicit about boundaries, ownership, and invariants
- aggregates should protect consistency boundaries
- entities and value objects should be rich and behavior-oriented, not anemic data bags
- repositories should serve aggregate behavior, not become one-per-table CRUD wrappers
- application use cases should orchestrate the model rather than replace it

## Strategic design

## Subdomains

The business domain is expected to be analyzed and split into subdomains based on business meaning, rules, goals, and change patterns.
Not all subdomains are equally important, and the repository should treat that as a design concern, not just a naming exercise.

The standard DDD distinction used in this repository is:

- `Core subdomain`
  - where the business differentiates itself
  - usually the place with the highest business sensitivity and change frequency
- `Supportive subdomain`
  - built to support the business's own operation
  - important, but not the main competitive edge
- `Generic subdomain`
  - common capability that many systems need and often share similar patterns

This classification matters because it affects:

- how much modeling effort is justified
- where the most precise ubiquitous language is needed
- where buying/adopting vs building is reasonable
- how carefully boundaries should be protected

TODO:


## Bounded contexts

Each bounded context should define:

- one cohesive domain model
- its own ubiquitous language
- its own rules, behaviors, and meanings
- its own internal consistency boundary

Bounded contexts should be:

- narrow enough to stay cohesive
- large enough to avoid artificial fragmentation
- aligned with the language and consistency needs of the business


TODO:


## Ubiquitous language

The repository is intended to use business language consistently in:

- code
- documentation
- use-case names
- aggregate names
- error names
- DTO and contract names where appropriate

This means naming should be driven by business meaning, not by framework vocabulary or generic technical labels.

For example:

- prefer business-specific names over generic `Manager`, `Helper`, or `Data`
- prefer explicit domain names like `BookingReference` or `TicketReference` rather than `PublicId` when the concept has real domain meaning
- avoid generic names like `SomeDomainService` when the service is actually a policy, calculator, guard, or rule evaluator

## Context relationships

This repository is expected to apply standard bounded-context relationship thinking when multiple contexts are introduced, including:

- cooperation
- customer/supplier
- separate ways

Where contexts share a common domain concept, that should be a deliberate choice, not accidental coupling.
Where a downstream model needs protection from another model's terminology or semantics, translation should be explicit.

The exact cross-context integration patterns for BookMyCinema are still to be implemented and should be decided per context boundary rather than assumed globally.

## Tactical design

## Aggregates and consistency boundaries

The repository is intended to model aggregates as consistency boundaries, not as arbitrary object graphs.

That means:

- aggregate roots are the entry points for behavior
- invariants that must stay consistent together belong inside the same aggregate boundary
- internal aggregate entities should not be manipulated directly from application use cases
- repositories should load and persist whole aggregates where domain behavior needs to run
- domain events are controlled by the aggregate root, which owns when they are raised as part of aggregate behavior and consistency enforcement

The architecture document already explains the repository's write path vs read path split. See [Clean Architecture](../architecture/clean-architecture.md), section `Domain-model path vs direct persistence/query path`.
This document adds the DDD interpretation of that split:
- aggregates matter on the write path because business behavior and invariants are being executed
- aggregates do not need to be materialized on the read path when a projected query is enough

## Entities

Entities in this repository are intended to be behavior-oriented objects with identity.
They should not be modeled as anemic models.

The general style to follow is:

- expose behavior through methods
- keep setters private or otherwise restricted
- use access modifiers intentionally to protect invariants
- keep identity immutable after creation
- avoid allowing application handlers to mutate internal entity state directly

The current `Entity<TId>` base in [Entity.cs](/C:/Users/ahmed/source/repos/BookMyCinema/Backend/BookMyCinema/src/BookMyCinema.SharedKernel/Entity.cs) and `AggregateRoot<TId>` base in [AggregateRoot.cs](/C:/Users/ahmed/source/repos/BookMyCinema/Backend/BookMyCinema/src/BookMyCinema.SharedKernel/AggregateRoot.cs) already establish part of that foundation.
The generic entity base also overrides equality and hash code behavior so entities use identity-based equality semantics rather than value semantics across all fields or based on reference.


## Strong identities

Strong identities should be modeled as `readonly record struct`.

That applies to identity types introduced for aggregates, entities, or other domain concepts where wrapping a primitive adds clarity and compile time protection.
The repository uses `readonly record struct` specifically for strong identities because their fields are expected to be value types only, so they can stay lightweight and avoid unnecessary heap allocation.

The goal is to:

- avoid primitive obsession
- make identity meaning explicit in code
- prevent accidental mixing of unrelated identifiers
- keep identity types lightweight and immutable


## Public identifiers and domain references

A distinction is made between:

- public opaque identifiers that exist mainly for external exposure and non-sequential identity concerns (i.e `PublicId`)
- domain references that carry actual domain meaning 

At minimum, these domain references are intended to have strong identity types:

- `BookingReference`
- `TicketReference`

The reason is that they are not merely opaque exposure identifiers. They carry explicit domain meaning in the business language and deserve to be modeled as such.

For other externally exposed identifiers, the current repository stance is intentionally still flexible:

- some may remain simple `PublicId` properties without an additional strong type when a wrapper around `Guid` adds little value
- some may later receive strong identity types if doing so adds real clarity and protection rather than ceremony

If a strong type is introduced for one of those public identifiers, the property in the model may still be named `PublicId` while the strong identity type itself can be domain-specific, such as `MoviePublicId` or `CinemaPublicId`.

### Internal aggregate entities

When an entity is internal to an aggregate, its methods should be designed so that use-case handlers do not bypass the aggregate root and manipulate that entity directly.
In practice, the repository intends to use domain-layer visibility and aggregate-root methods to enforce that style.

The rule to follow is:

- application code should tell the aggregate root what business operation to perform
- the aggregate root coordinates any internal entity changes needed to perform that operation

## Value objects

Value objects are intended to be immutable and behavior-oriented.
They should model validated domain concepts, not just grouped primitives.
Because they represent value semantics, the repository uses `record` type for value objects so immutability and value-based equality comes out of the box.

The repository's existing value objects already show the intended style:

- [Email.cs](/C:/Users/ahmed/source/repos/BookMyCinema/Backend/BookMyCinema/src/BookMyCinema.SharedKernel/ValueObjects/Email.cs)
- [Country.cs](/C:/Users/ahmed/source/repos/BookMyCinema/Backend/BookMyCinema/src/BookMyCinema.SharedKernel/ValueObjects/Country.cs)
- [Currency.cs](/C:/Users/ahmed/source/repos/BookMyCinema/Backend/BookMyCinema/src/BookMyCinema.SharedKernel/ValueObjects/Currency.cs)

Common characteristics of the intended approach:

- no public setters
- creation through validation-aware public factory methods when invalid input is possible and results can be returned
- normalization inside creation
- failure returned as `Result<T>` when validation is part of normal flow
- immutability after creation
- value semantics out of the box through record types

### Constructor vs factory method

The repository intends to follow this modeling rule:

- use a constructor directly when construction is trivial or invalid input is impossible by design
- use a constructor when invalid input should be exceptional and represented by exceptions
- use a factory method when construction performs validation / essential logic and returns `Result<T>`
- use a separate factory class when creation requires dependencies or significant decision logic

This applies to both entities and value objects.

## SharedKernel usage

Shared kernel usage in this repository should stay small and deliberate.
The Clean Architecture document (See [Clean Architecture](../architecture/clean-architecture.md)) already covers the project boundary in more detail, so the DDD rule here is brief:

- shared kernel is for concepts that are genuinely shared and stable across domain areas
- it should not become a dumping ground for anything that feels reusable
- domain-specific concepts should stay in their own bounded context or domain area unless there is a real reason to share them

The current repository already uses `BookMyCinema.SharedKernel` for:

- base entity and aggregate types
- result and error primitives
- audit capability interfaces
- domain abstractions such as `IDomainEvent` and `IAggregateRoot`
- shared value objects such as `Email`, `Country`, and `Currency`


## Country and Currency

`Country` and `Currency` are important concrete examples of how this repository separates domain meaning from persistence representation.

Current code already shows both sides:

- domain-side value objects:
  - [Country.cs](/C:/Users/ahmed/source/repos/BookMyCinema/Backend/BookMyCinema/src/BookMyCinema.SharedKernel/ValueObjects/Country.cs)
  - [Currency.cs](/C:/Users/ahmed/source/repos/BookMyCinema/Backend/BookMyCinema/src/BookMyCinema.SharedKernel/ValueObjects/Currency.cs)
- persistence-side reference-data entities:
  - [Country.cs](/C:/Users/ahmed/source/repos/BookMyCinema/Backend/BookMyCinema/src/BookMyCinema.Persistance/Country/Country.cs)
  - [Currency.cs](/C:/Users/ahmed/source/repos/BookMyCinema/Backend/BookMyCinema/src/BookMyCinema.Persistance/Currency/Currency.cs)

The intended rule is:

- `Country` and `Currency` are modeled as value objects for domain use
- they also exist as normalized persistence reference data
- they do not require separate domain entities just because there are persisted `Countries` and `Currencies` tables as they have no business lifecycle or domain meaningful operations.

This is important because the repository does not equate "table exists" with "domain entity must exist."

The reasons, as reflected in the project notes and existing persistence design, are:

- they are domain concepts when used as validated values in the model
- they are also reference data when used for normalization, referential integrity, and querying
- a persistence entity for normalized reference data does not automatically imply aggregate or entity behavior in the domain model

This is the same broader rule described in the architecture document's query and persistence sections:

- some persisted structures exist only for persistence or query purposes
- the domain model should only introduce entities where identity, lifecycle, and behavior justify them

## Domain services

Domain services are intended for domain logic that does not naturally belong to one entity or value object.
They are not a fallback for business logic that could live inside an aggregate but has been pushed out for convenience.

The repository's intended stance is:

- prefer entity and value-object behavior first
- use a domain service when the logic is genuinely domain logic and does not sit naturally on one model object
- keep domain services focused and explicit in naming

Preferred naming should reflect the job being done, for example:

- `PriceCalculator`
- `DeactivationPolicy`
- `OverdraftLimitGuard`

Avoid generic names like `SomethingDomainService` when a more precise business name exists.

### Dependencies in domain services

The repository intends to keep domain services as pure as practical.
Use-case handlers should usually fetch the needed data and pass it into the domain service rather than turning the domain service into a data-access orchestrator.

Injecting repositories into a domain service is allowed only when the fetch is inseparable from the domain rule or when centralizing that logic is clearly the least confusing option.

## Rich models and encapsulation

The repository intends to prefer rich models over anemic models.

That means:

- domain objects encapsulate their own behavior and rule enforcement
- public setters should be avoided for domain state
- private fields with read-only exposure are preferred for collections
- methods should represent business operations, not low-level property mutation

### Collections

Collections should be exposed carefully.
The repository intends to use private mutable collections with read-only exposure to callers.

Where collection behavior becomes substantial, first-class collection wrappers are acceptable and often desirable.
This is especially useful when:

- collection rules are non-trivial
- multiple behaviors belong to the collection itself
- keeping logic on the parent entity would make it noisy or scattered

The goal is not to wrap every list mechanically, but to give collection behavior an explicit home when that improves the model.

## Construction and persistence compatibility

The repository intends to model domain objects for correctness first, while still remaining compatible with EF Core materialization.

The standard approach is:

- identity should stay immutable after creation
- constructors can be private or protected when that helps protect invariants
- parameterless constructors may exist when needed for EF Core materialization
- factory methods can create validated objects while persistence is still allowed to materialize stored state

This is one reason the repository keeps EF Core concerns in the Persistence project rather than leaking ORM constraints into Domain design decisions more than necessary.

## Results and domain creation

The repository uses result-based failure handling for expected failures.
That has direct modeling consequences in the domain:

- creation and behavioral operations may return `Result` or `Result<T>` when invalid input or violated rules are expected business outcomes
- exceptions are reserved for truly exceptional situations rather than normal rule enforcement

This is already visible in current shared value objects such as `Country`, `Currency`, and `Email`, which validate and return `Result<T>` on creation.


## TODO

- define subdomains
- define bounded contexts 
- define the concrete bounded-context map for the main subdomains
- document aggregate-by-aggregate modeling decisions, invariants, events, etc.
- define domain-event handling approach beyond the current `IDomainEvent` marker
