# BookMyCinema Logical Database Schema

## 1. Purpose and source of truth

This document explains the initial logical database schema for BookMyCinema: a multi-tenant cinema-management and ticket-booking platform.

The schema is maintained in DBML and rendered in dbdiagram. The DBML file is the authoritative structural source of truth.
This document records the intent behind the structure, relationships, keys, indexes, constraints, application-enforced invariants, lifecycle decisions, and any extra notes.

This document and the related DBML will be revised and aligned with entities and models as they are implemented in the ORM.
The DBML is the source of truth for the logical schema, and the EF Core model is the source of truth for the application model.
Both are aligned (columns naming, lengths, nullability) and tables/entities relationships
so dbml, and this MD file are always kept aligned for each colum nullabilty, length, naming.


### Schema artifacts

- **DBML source of truth:** [`logical-schema.final.dbml`](./logical-schema.final.dbml)
- **Static diagram snapshot: [to be added].
- **Live dbdiagram:** https://dbdiagram.io/d/BookMyCinema-6a085a82697f99c1678c1c90
- **ERD diagram:** https://app.diagrams.net/ (will be replaced with live link for ERD drawn using imported XMLs based on DBML) will be add as a separate artifact and will be referenced in this document.
- **Physical Schema:** a physical schema generated from the DBMS will be added as a separate artifact and will be referenced in this document.

---

## 2. Scope and modeling principles

### 2.1 Shared-schema multi-tenancy

BookMyCinema uses one database and one shared set of tables. A cinema is the tenant boundary, represented by `CinemaId`. A cinema branch is the main operational subdivision, represented by `CinemaBranchId`.

Tenant and branch ownership columns are intentionally duplicated on entities that need direct tenant/branch filtering or database-enforced ownership consistency. Composite foreign keys then prove that the referenced child belongs to the same tenant and branch.

Canonical ownership key order:

- Cinema-owned alternate key: `(CinemaId, Id)`
- Branch-owned alternate key: `(CinemaId, CinemaBranchId, Id)`

This ordering keeps tenant and branch prefixes usable for common filtering and relationship validation (support better composite index usage for expected access patterns).

### 2.2 Surrogate keys and identifying keys

Most independent entities use a numeric surrogate primary key:

- `int` for normal-volume entities
- `bigint` for lifetime high-volume transactional entities

association entities and identifying dependents may use composite or shared primary keys instead.

A typical independently identified branch-owned entity therefore has:

- `PK CLUSTERED (Id)`
- `AK UNIQUE (CinemaId, CinemaBranchId, Id)` when composite child foreign keys need ownership enforcement

This provides narrow immutable row identity while retaining database-enforced tenant and branch consistency.

### 2.3 Public identifiers

Externally exposed entities use UUIDv7 as public identifiers, persisted as `binary(16)` to preserve sortability. This prevents SQL Server's uniqueidentifier 
byte-reordering from breaking UUIDv7 ordering. obscures internal entity counts, and mitigates enumeration attacks.

Domain-specific names are used instead of a generic `PublicId` where appropriate:
i.e:
    - `BookingReference`
    - `TicketReference`

Association Entities, internal lookups, owned dependents, or any entitiy that has no public exposure operations do not have public UUIDs.

### 2.4 Delete behavior

All foreign keys use `NO ACTION`/`RESTRICT` initially. Cascading deletion is intentionally avoided because entities have historical and operational significance.
so entities are available to be reactivated or referenced even after they are no longer operationally relevant.

Physical deletion is permitted only for entities whose lifecycle explicitly treats them as replaceable associations or derived configuration rows, such as selected junction rows or generated catalogue relations.

### 2.5 Audit and activation

Audit timestamps use SQL Server `datetime2` and .NET `DateTime`, always in UTC.

The domain uses independent capability interfaces rather than an audit inheritance hierarchy:

- `ICreationAuditable`
- `IModificationAuditable`
- `IActivationAuditable`

Typical fields:

- `CreatedAtUtc`
- `CreatedByUserId`
- `ModifiedAtUtc`
- `ModifiedByUserId`
- `IsActive`
- `ActivationChangedAtUtc`
- `ActivationChangedByUserId`

The audit actor columns are nullable because work can be performed by background jobs, imports, deployment seeders, or other system processes.

For diagram clarity , audit actor relationships is omitted from the schema diagram. although in the physical SQL Server implementation, every `CreatedByUserId`, `ModifiedByUserId`, and `ActivationChangedByUserId` column is intended to be an explicit nullable foreign key to `ApplicationUsers.Id`, with `NO ACTION` delete and update behavior.

### 2.6 Activation is not soft deletion

`IsActive` means the row remains visible for history, administration, and possible reactivation but is not eligible for relevant operational use.

It does not mean hidden-by-default deletion. Therefore:

- no global query filter is used for activation;
- public and operational queries explicitly require active scope;
- historical and administrative queries may include inactive rows;

### 2.7 Concurrency

No generic concurrency token or `rowversion` is included initially.
When optimistic concurrency is needed later, selected entities will implement an appropriate dedicated interface.
Pessimistic concurrency remains operation-specific and should be implemented through focused repository methods using explicit SQL locking hints and transaction boundaries.

---

## 3. Type and storage conventions

| Concern                               | Initial convention             |
| ------------------------------------- | ------------------------------ |
| Normal entity identifiers             | `int`                          |
| High-volume transactional identifiers | `bigint`                       |
| Public UUIDv7 identifiers             | `binary(16)`                   |
| Audit timestamps                      | `datetime2`, UTC               |
| Resolved local showtime timestamps    | `datetimeoffset`               |
| Local schedule dates                  | `date`                         |
| Local schedule times                  | `time`                         |
| Money                                 | `decimal(18,2)` initially      |
| Currency Code                         | ISO 4217 alpha-3 `char(3)`     |
| Country code                          | ISO 3166-1 alpha-2, `char(2)`  |
| Payment Provider references           | Provider-appropriate `varchar` |

---

## 4. Functional entity groups

## 4.1 Platform, tenant, branch, and employee scope

### `Countries`

A small normalized lookup for countries.

Key fields:

- `Code char(2)`
- `Name nvarchar(100)`

Primary Key - Constraints - Indexes:

- Primary key on `Code`

Only the country level is normalized initially. Lower geographic levels remain descriptive branch data (free text).

### `TimeZones`

Stores supported IANA time-zone identifiers and platform display labels.

Key fields:

- `Id smallint`
- `IanaId`
- `Name nvarchar(100)`

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Unique index on `IanaId`

`IanaId` is authoritative. `Name` is a platform-managed label. This lookup is deployment- or administrator-seeded and does not require normal audit fields.


### `Currencies`

Reference-data lookup containing supported currencies.

Key fields:
- `Code char(3)`
- `Name nvarchar(50)`

Primary Key - Constraints - Indexes:

- Primary key on `Code`


### `ApplicationUsers`

A deliberately minimal user placeholder.

Key fields:

- `Id`
- `PublicId`
- audit fields
- activation fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Unique index on `PublicId`

Username, email, password, external identity provider data, claims, and authentication-specific fields are deferred until Auth module is implemented.

When a user is deactivated, all active `CinemaEmployees` memberships should be deactivated within the same business operation and transaction.

### `Cinemas`

Represents the tenant and cinema brand.

Key fields:

- `Id`
- `PublicId`
- `BrandName`
- `Slug`
- audit fields
- activation fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Unique `PublicId`
- Globally unique `Slug`

The slug remains unique(globally) even when a cinema is inactive. Inactive cinemas remain resolvable for administration and historical data.

### `CinemaBranches`

Represents an operational branch owned by a cinema.

Key fields:

- `Id`
- `CinemaId`
- `PublicId`
- `Name`
- `Slug`
- `CurrencyCode`
- `TimeZoneId`
- `CountryId`
- `AdministrativeArea`
- `Locality`
- `AddressLine1`
- `AddressLine2`
- audit fields
- activation fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Alternate key `(CinemaId, Id)`
- Unique `PublicId`
- Unique `(CinemaId, Slug)`

Foregin keys:

- `CinemaId` → `Cinemas(Id)`
- `TimeZoneId` → `TimeZones(Id)`
- `CountryId` → `Countries(Id)`
- `CurrencyCode` → `Currencies(Code)`


Location strategy:

- `CountryId` is normalized and integrity-controlled.
- `AdministrativeArea` is free text for state, governorate, province, region, or equivalent.
- `Locality` is free text for city, town, district, municipality, area, or equivalent.
- `AddressLine1` is street name, building number, venue name, or primary address line.
- `AddressLine2` floor, mall unit, district detail, landmark, or secondary line.

This intentionally avoids maintaining a global geographic hierarchy. It permits text filtering while accepting that spelling and categorization correctness are administrator responsibilities.

At the expected scale, text filtering on branch location is acceptable without dedicated text indexes. Latitude, longitude, map display, and closest-branch search are deferred until a concrete feature requires them.

### `CinemaEmployees`

A rich association between a user and a cinema.

Key fields:

- `CinemaId`
- `UserId`
- `ScopeType`
- audit fields
- activation fields

`ScopeType` distinguishes organization-wide access from access limited to selected branches. It is a domain enum.

Primary Key - Constraints - Indexes:

- Composite primary key `(CinemaId, UserId)`
- Index on `UserId`

Foreign keys:

- `CinemaId` → `Cinemas(Id)`
- `UserId` → `ApplicationUsers(Id)`


### `CinemaEmployeeBranchAssignments`

Associates a cinema employee with permitted branches when `ScopeType` is branch-specific.

Key fields:

- `CinemaId`
- `UserId`
- `CinemaBranchId`
- creation audit fields

Primary Key - Constraints - Indexes:

- Composite primary key `(CinemaId, UserId, CinemaBranchId)`
- Index on `UserId`


Foreign keys:

- composite `(CinemaId, UserId)` → `CinemaEmployees(CinemaId, UserId)`
- composite `(CinemaId, CinemaBranchId)` → `CinemaBranches(CinemaId, Id)`

Composite foreign keys ensure that the employee membership and assigned branch belong to the same cinema. Rows may be physically inserted and deleted as assignments change.

---
> **Foreign-key convention:** Unless stated otherwise, all foreign keys use `ON DELETE NO ACTION`. Audit actor columns are nullable because writes may be performed by system processes, imports, seeders, or background jobs. Their relationships may be omitted from the visual DBML diagram for clarity, but they are still part of the intended physical schema.


## 4.2 Hall, seat type, seat, and layout configuration

### `CinemaSeatTypes`

Defines cinema-owned seat classifications such as Standard, Premium, VIP, etc.

Key fields:

- `Id`
- `CinemaId`
- `Name`
- `DisplayOrder`
- audit fields
- activation fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Unique `(CinemaId, Name)`

Foreign keys:

- `CinemaId` → `Cinemas(Id)`
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`
- nullable `ModifiedByUserId` → `ApplicationUsers(Id)`
- nullable `ActivationChangedByUserId` → `ApplicationUsers(Id)`

### `Halls`

Represents a screening hall owned by a branch.

Key fields:

- `Id`
- `CinemaId`
- `CinemaBranchId`
- `PublicId`
- `Name`
- audit fields
- activation fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Alternate key `(CinemaId, CinemaBranchId, Id)`
- Unique `PublicId`
- Unique `(CinemaId, CinemaBranchId, Name)`

Foreign keys:

- composite `(CinemaId, CinemaBranchId)` → `CinemaBranches(CinemaId, Id)`
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`
- nullable `ModifiedByUserId` → `ApplicationUsers(Id)`
- nullable `ActivationChangedByUserId` → `ApplicationUsers(Id)`


### `Seats`

Represents a physical seat within a hall.

Key fields:

- `Id`
- `HallId`
- `CinemaSeatTypeId`
- `RowNumber`
- `SeatNumber`
- `SeatLabel`
- audit fields
- activation fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Alternate key `(HallId, Id)`
- Unique `(HallId, RowNumber, SeatNumber)`

Foreign keys:

- `HallId` → `Halls(Id)`
- `CinemaSeatTypeId` → `CinemaSeatTypes(Id)`
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`
- nullable `ModifiedByUserId` → `ApplicationUsers(Id)`
- nullable `ActivationChangedByUserId` → `ApplicationUsers(Id)`

`CinemaId` and `CinemaBranchId` are intentionally not duplicated on `Seats`. Seat access is resolved through its hall, and authorization is checked against the hall scope.

`SeatLabel` is persisted and generated centrally during hall configuration. It is not a computed column.

Application-enforced invariant:

> The selected `CinemaSeatTypeId` must belong to the same cinema as the seat's hall and must be available for use.

The administration UI should allow selection of valid active seat types only. This is one of the intentionally application-enforced ownership rules.

### `HallLayoutCells`

Represents every coordinate in a hall layout, including seat cells and gap cells.

Key fields:

- `HallId`
- `RowNumber`
- `ColumnNumber`
- nullable `SeatId`
- creation and modification audit fields

Primary Key - Constraints - Indexes:

- Composite primary key `(HallId, RowNumber, ColumnNumber)`
- Filtered unique index `(HallId, SeatId) WHERE SeatId IS NOT NULL`

Foreign keys:

- `HallId` → `Halls(Id)`
- composite nullable `(HallId, SeatId)` → `Seats(HallId, Id)`
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`
- nullable `ModifiedByUserId` → `ApplicationUsers(Id)`

A null `SeatId` represents a gap or non-seat cell. Physical removal is allowed when the layout is redesigned.

---

## 4.3 Movie catalogue and metadata

### `Movies`

Represents a movie independently of any specific cinema or branch.

Important fields:

- `Id`
- `PublicId`
- nullable `TmdbId`
- title and original-title metadata
- original language
- overview
- runtime and runtime-estimation marker
- release date
- poster and trailer provider paths/references
- TMDB vote values
- adult-content marker
- provider synchronization timestamp
- audit and activation fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Unique `PublicId`
- Filtered unique index on `TmdbId` when non-null

Foreign keys:

- nullable `CreatedByUserId` → `ApplicationUsers(Id)`
- nullable `ModifiedByUserId` → `ApplicationUsers(Id)`
- nullable `ActivationChangedByUserId` → `ApplicationUsers(Id)`

TMDB identifiers are optional provider-specific identifiers. They are not primary keys or internal foreign keys, allowing future manual records or multiple metadata providers.

When provider runtime is missing or invalid, the application stores the configured fallback runtime and sets `IsRuntimeEstimated = 1`.

### `Genres`

A reusable genre catalogue.

Key fields:

- `Id`
- nullable `TmdbId`
- `Name`
- synchronization timestamp
- creation and modification audit fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Filtered unique index on `TmdbId` when non-null

Foreign keys:

- nullable `CreatedByUserId` → `ApplicationUsers(Id)`
- nullable `ModifiedByUserId` → `ApplicationUsers(Id)`

`Name` is not unique because provider naming, localization, or future catalogue sources may create legitimate variants.

### `MovieGenres`

Pure many-to-many junction between movies and genres.

Primary Key - Constraints - Indexes:

- Composite primary key `(MovieId, GenreId)`
- Reverse-lookup index on `GenreId`

Foreign keys:

- `MovieId` → `Movies(Id)`
- `GenreId` → `Genres(Id)`
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`

The primary key already supports movie-first genre lookup. Rows may be physically synchronized with the provider catalogue.

### `Persons`

Represents cast and crew persons independently of a movie.

Key fields:

- `Id`
- `PublicId`
- nullable `TmdbId`
- `Name`
- nullable `OriginalName`
- nullable profile path
- synchronization timestamp
- creation and modification audit fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Unique `PublicId`
- Filtered unique index on `TmdbId` when non-null

Foreign keys:

- nullable `CreatedByUserId` → `ApplicationUsers(Id)`
- nullable `ModifiedByUserId` → `ApplicationUsers(Id)`

`OriginalName` remains provisional until real provider responses confirm that it provides distinct value.

### `MovieCredits`

Represents a cast or crew credit connecting one person to one movie.

Important fields:

- `MovieId`
- `PersonId`
- optional opaque `TmdbCreditId`
- `CreditCategory`
- cast-specific `CharacterName` and `CastOrder`
- crew-specific `Department` and `Job`
- synchronization timestamp
- creation audit fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Filtered unique index on `TmdbCreditId` when non-null
- Index on `MovieId`
- Index on `PersonId`
- Check constraint:
    - Cast credit: `CastOrder` required; `Department` and `Job` null; `CharacterName` may be null.
    - Crew credit: `CharacterName` and `CastOrder` null; `Department` and `Job` required.

Foreign keys:

- `MovieId` → `Movies(Id)`
- `PersonId` → `Persons(Id)`
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`

`TmdbCreditId` is stored as an opaque provider random string and will not be parsed into a binary or numeric representation.

### `MoviePosterMirrors`

Stores metadata for a locally mirrored movie poster.

It is a weak shared-primary-key one-to-one dependent of `Movies`:

- `MovieId` is both PK and FK.
- `StorageId` is unique.

Foreign keys:

- `MovieId` → `Movies(Id)`; `MovieId` is also the primary key
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`
- nullable `ModifiedByUserId` → `ApplicationUsers(Id)`

---

## 4.4 Branch movie availability, schedules, and generated showtimes

### `CinemaBranchMovies`

Represents a stable association between a branch and a movie available for scheduling.

Key fields:

- `Id`
- `CinemaId`
- `CinemaBranchId`
- `MovieId`
- creation audit fields
- activation audit fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Alternate key `(CinemaId, CinemaBranchId, Id)`
- Unique `(CinemaBranchId, MovieId)`

Foreign keys:

- composite `(CinemaId, CinemaBranchId)` → `CinemaBranches(CinemaId, Id)`
- `MovieId` → `Movies(Id)`
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`
- nullable `ActivationChangedByUserId` → `ApplicationUsers(Id)`

The entity uses insert-or-reactivate semantics:

- missing association: insert active row;
- existing inactive association: reactivate the same row;
- existing active association: treat as idempotent or reject as already active, depending on command semantics.

The stable row is preserved because historical schedules may continue to reference it.

### `ShowtimeSchedules`

Defines a recurring local schedule from which concrete showtimes are generated.

Important fields:

- `Id`
- `PublicId`
- `CinemaId`
- `CinemaBranchId`
- `HallId`
- `CinemaBranchMovieId`
- `LocalEffectiveFrom`,
- `LocalEffectiveTo`,
- `LocalStartTime`,
- `LocalEndTime`,
- `LocalGeneratedThroughDate`
- `Status`
- creation and modification audit fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Alternate key `(CinemaId, CinemaBranchId, Id)`
- Unique `PublicId`
- Index `(CinemaId, CinemaBranchId, HallId)`
- Index `(CinemaId, CinemaBranchId, CinemaBranchMovieId)`

Foreign keys:

- composite `(CinemaId, CinemaBranchId, HallId)` → `Halls(CinemaId, CinemaBranchId, Id)`
- composite `(CinemaId, CinemaBranchId, CinemaBranchMovieId)` → `CinemaBranchMovies(CinemaId, CinemaBranchId, Id)`
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`
- nullable `ModifiedByUserId` → `ApplicationUsers(Id)`

Composite foreign keys prove that the selected hall and branch movie belong to the same cinema and branch as the schedule.

Schedule overlap validation is application logic because it depends on business rules, effective periods, and hall availability rather than a database check constraint.

Display state is an application projection/helper based on stored status, effective dates, and the supplied branch-local current date. It is not persisted or computed in SQL.

### `ShowtimeScheduleSeatTypePrices`

Associative entity with attributes connecting a schedule to each priced cinema seat type.

Key fields:

- `ShowtimeScheduleId`
- `CinemaSeatTypeId`
- `Amount`
- `CurrencyCode`
- creation audit fields

Primary Key - Constraints - Indexes:

- Composite primary key `(ShowtimeScheduleId, CinemaSeatTypeId)`

Foreign keys:

- `ShowtimeScheduleId` → `ShowtimeSchedules(Id)`
- `CinemaSeatTypeId` → `CinemaSeatTypes(Id)`
- `CurrencyCode` → `Currencies(Code)`
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`

Application-enforced invariants:

- the seat type belongs to the schedule hall's cinema;
- the seat type is active;
- it is actually used by active seats in the hall;
- every active seat type used by the hall receives one price;
- irrelevant seat types cannot be priced;
- currency matches the branch currency (populated automatically from the branch configuration not user input.)
  just shown in UI for user reference.

This is another intentionally application-enforced ownership rule.

### `Showtimes`

Represents a concrete generated occurrence of a schedule.

Key fields:

- `Id`
- `PublicId`
- `ShowtimeScheduleId`
- cinema and branch ownership
- resolved `LocalStartsAt` and `LocalEndsAt` as `datetimeoffset`
- `LocalOccurrenceDate`
- creation audit fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Alternate key `(CinemaId, CinemaBranchId, Id)`
- Unique `PublicId`
- Unique `(ShowtimeScheduleId, LocalOccurrenceDate)`

Foreign keys:

- composite `(CinemaId, CinemaBranchId, ShowtimeScheduleId)` → `ShowtimeSchedules(CinemaId, CinemaBranchId, Id)`
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`

The resolved offsets retain the actual local occurrence in the presence of time zones and daylight-saving transitions. Conversion, ambiguous local times, and invalid local times are handled during generation using the branch time zone.

No speculative branch/start-time listing index is defined initially because the principal discovery flow is movie-first and branch-movie information is reached through the schedule.

### `ShowtimeSeats`

Represents the availability state of a physical seat for one showtime and snapshots relevant seat data.

Key fields:

- `ShowtimeId`
- `SeatId`
- `CinemaSeatTypeId` snapshot
- `SeatLabel` snapshot
- `Status`
- nullable `BookingReference`
- nullable `HoldExpiresAtUtc`

Primary Key - Constraints - Indexes:

- Composite primary key `(ShowtimeId, SeatId)`
- Filtered cleanup index on `HoldExpiresAtUtc WHERE Status = Held`

Foreign keys:

- `ShowtimeId` → `Showtimes(Id)`
- `SeatId` → `Seats(Id)`
- `CinemaSeatTypeId` → `CinemaSeatTypes(Id)`

State consistency check constraint:

- Available: no booking reference and no expiry.
- Held: booking reference and expiry required.
- PendingPayment: booking reference required; no hold expiry.
- Booked: booking reference required; no hold expiry.

No standalone booking-reference index is initially required because relevant flows already know the showtime and each showtime has a small seat population.

Application-enforced invariant:

> Generated seats must originate from the hall selected by the showtime schedule.

The database intentionally does not duplicate hall ownership into every generated seat relationship. Controlled generation logic enforces it.

---

## 4.5 Booking, payment, and ticketing

### `Bookings`

Represents the business booking and payment boundary.

Important fields:

- `Id bigint`
- `BookingReference binary(16)`
- optional authenticated `UserId`
- `CinemaId`
- `CinemaBranchId`
- `ShowtimeId`
- `CustomerName`
- `CustomerEmail`,
- `CustomerPhone`,
- `SeatsCount`
- `CurrencyCode`
- `TotalPriceAmount`
- `Status`
- creation and modification audit fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Unique `BookingReference`
- Alternate key `(Id, ShowtimeId)` for the `BookingSeats` composite FK
- Index `(CinemaId, CinemaBranchId, ShowtimeId)`
- Filtered index on `UserId` when non-null

Foreign keys:

- nullable `UserId` → `ApplicationUsers(Id)`
- composite `(CinemaId, CinemaBranchId, ShowtimeId)` → `Showtimes(CinemaId, CinemaBranchId, Id)`
- `CurrencyCode` → `Currencies(Code)`
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`
- nullable `ModifiedByUserId` → `ApplicationUsers(Id)`

Customer name, email, and phone are persisted booking-time snapshots for both guests and authenticated users.

`SeatsCount` and `TotalPriceAmount` are persisted even though they can initially be derived from booking-seat rows. They remain available if `BookingSeats` are removed during cancellation, expiry, or failure handling and preserve historical and reconciliation meaning.

`TotalPriceAmount` is the final payable amount snapshot. Calculation belongs to Application logic.

`CurrencyCode` is from the ShowtimeSchedule of that Showtime.


### `BookingSeats`

Owned transactional dependent representing seats selected for a booking.

Important fields:

- `Id bigint`
- `BookingId`
- `ShowtimeId`
- `SeatId`
- price and currency snapshots
- creation audit fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Unique `(BookingId, SeatId)`
- Unique `(ShowtimeId, SeatId)`

Foreign keys:

- composite `(BookingId, ShowtimeId)` → `Bookings(Id, ShowtimeId)`
- composite `(ShowtimeId, SeatId)` → `ShowtimeSeats(ShowtimeId, SeatId)`
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`

Relationships:

- Composite FK `(BookingId, ShowtimeId)` to `Bookings(Id, ShowtimeId)`
- Composite FK `(ShowtimeId, SeatId)` to `ShowtimeSeats`

The first constraint keeps the seat under the booking's showtime. The second ensures one booking-seat row owns a specific showtime seat at most once.

`BookingSeats` store the price actually assigned at booking time. They do not independently control availability; `ShowtimeSeats` own the inventory state machine.

### `Payments`

Represents one local payment journey for a booking.

Important fields:

- `Id bigint`
- `BookingId`
- `PaymentProvider`,
- `ProviderSessionReference`
- `ProviderSessionExpiresAtUtc`
- `Status`
- `Amount`
- `CurrencyCode`
- `CompletedAtUtc`
- `FailureCode`,
- `FailureMessage`
- creation and modification audit fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Unique `BookingId`
- Filtered unique `(PaymentProvider, ProviderSessionReference)` when the reference is non-null

Foreign keys:

- `BookingId` → `Bookings(Id)`
- `CurrencyCode` → `Currencies(Code)`
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`
- nullable `ModifiedByUserId` → `ApplicationUsers(Id)`

The unique booking relationship implements one local payment record per booking.
Retryable card declines remain within the same provider payment journey rather than creating new local booking/payment rows for each attempt.

Provider-specific states and events are translated into the shared local lifecycle. Ordinary retryable declines must not release seats while the provider session remains open.
Provider session expiry closes abandoned unpaid journeys.

### `Tickets`

Represents the credential issued for one confirmed booking seat.

Important fields:

- `Id bigint`
- `BookingSeatId`
- `TicketReference`
- `Status`
- `UsedAtUtc`,
- `CancelledAtUtc`,
- `ValidationToken`,
- `ValidationTokenHash`,
- `EncryptedValidationToken`,
- creation audit fields

Primary Key - Constraints - Indexes:

- Primary key on `Id`
- Unique `BookingSeatId`
- Unique `TicketReference`

Foreign keys:

- `BookingSeatId` → `BookingSeats(Id)`
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`

State consistency check constraint:

- Issued: `UsedAtUtc` and `CancelledAtUtc` null.
- Used: `UsedAtUtc` required and `CancelledAtUtc` null.
- Cancelled: `CancelledAtUtc` required and `UsedAtUtc` null.

The QR credential strategy remains deferred. `ValidationToken`, `ValidationTokenHash`, and `EncryptedValidationToken` are provisional alternatives; only the selected strategy should remain in the final physical model.

---

## 4.6 Media metadata

### `UserProfileImages`

Weak shared-primary-key one-to-one dependent of `ApplicationUsers`.

- `UserId`
- `StorageId`.
- `OriginalFileName`,
- `StorageId`,
- `FileExtension`,
- `StoredContentType`,
- `SizeBytes`,
- `Width`,
- `Height`,
- creation and modification audit fields


Primary Key - Constraints - Indexes:
- Primary key on `UserId`
- Unique `StorageId`

Foreign keys:

- `UserId` → `ApplicationUsers(Id)`; `UserId` is also the primary key (shared pk)
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`
- nullable `ModifiedByUserId` → `ApplicationUsers(Id)`

### `CinemaLogos`

Weak shared-primary-key one-to-one dependent of `Cinemas`.

- `CinemaId`
- `OriginalFileName`,
- `StorageId`,
- `FileExtension`,
- `StoredContentType`,
- `SizeBytes`,
- `Width`,
- `Height`,
- Creation and modification audit fields are included.

Primary Key - Constraints - Indexes:
- Primary key on `CinemaId`
- Unique `StorageId`

Foreign keys:

- `CinemaId` → `Cinemas(Id)`; `CinemaId` is also the primary key (shared pk)
- nullable `CreatedByUserId` → `ApplicationUsers(Id)`
- nullable `ModifiedByUserId` → `ApplicationUsers(Id)`

### Shared media conventions

- `OriginalFileName` contains the sanitized original filename, including its original extension.
- `FileExtension` and `StoredContentType` describe the actual stored/transformed file.
- `StorageId` is currently `binary(16)`; its exact storage-provider semantics remain deferred.

---

## 5. Weak dependents

### These depend on their owner for identity and/or lifecycle:

- `HallLayoutCells`
- `UserProfileImages`
- `CinemaLogos`
- `MoviePosterMirrors`

Shared-primary-key dependents model optional one-to-one data without introducing unnecessary independent identity(Surrogate key).

---

## 6. Database-enforced invariants

The initial schema directly enforces the following important invariants:

1. Branches belong to a cinema.
2. Halls belong to the stated cinema and branch.
3. Employee branch assignments remain within the employee's cinema membership.
4. Hall layout seats belong to the same hall as their layout cell.
5. A seat coordinate is unique inside a hall.
6. A seat can occupy at most one layout coordinate in its hall.
7. A branch/movie association is unique and stable.
8. Schedules reference halls and branch movies in the same tenant/branch scope.
9. A generated showtime occurrence is unique for a schedule and local occurrence date.
10. Showtime-seat status, booking ownership, and hold-expiry nullability are consistent.
11. Booking seats belong to the booking's showtime.
12. A showtime seat can belong to at most one booking-seat row.
13. There is one local payment row per booking.
14. Provider session references are unique within a provider when available.
15. There is at most one ticket per booking seat.
16. Ticket status and terminal timestamps remain consistent.
17. Cast and crew credit fields remain category-consistent.

---

## 7. Intentionally application-enforced invariants

Not every cross-parent rule is duplicated into composite database keys. The following are intentionally validated in application/domain logic:

### 7.1 Seat type ownership

A seat's (Seats table) `CinemaSeatTypeId` must belong to the same cinema as its hall.

Reason for application enforcement:

- `Seats` intentionally avoid duplicated tenant/branch columns that will add no benefit in filteration as Seat access pattern is by hall not by tenant or branch directly.
- The administration workflow already resolves the hall and restricts seat-type choices. (knowing hall -> it's branch -> it's cinema -> valid seat types for that cinema)

### 7.2 Generated showtime-seat hall ownership

Each `ShowtimeSeat.SeatId` must belong to the hall selected by the showtime schedule.

Reason for application enforcement:

- Rows are generated by controlled backend logic. based on the selected hall to create showtime for, and since each seat belong to a hall and each seat have a type
  so when creating showtime -> hall is selected -> seat types are loaded to be configured configured(price per seat type for this showtime schedule) which is for this hall.
  then ShowtimeSeats records are created by backend knowing the hall, its seats, and every seat type
- Duplicating hall and tenant ownership on the generated association would widen a high-volume table and its keys with no benefit for filteration using them as ShowtimeSeats access pattern is based on showtime not tenant or branch or hall.

### 7.3 Schedule seat-type pricing ownership and completeness

Priced seat types must belong to the schedule hall's cinema and match the seat types actively used in that hall. Currency must match the branch currency.

Reason for application enforcement:

- same as `7.2` The rule spans schedule belonging to a hall that belong to a branch and a cinema with allowed seat types and configured currency
- Application logic validates that each seat type is available for that hall that the schedule is for and they are active seat types, and each active seat type is provided a price for that schedule.

### 7.4 Schedule overlap

Schedule overlap and effective-period rules remain domain/application validation because they require interval and business-state reasoning.

### 7.5 Free-text branch geography

`AdministrativeArea` and `Locality` are descriptive text. Their correctness, spelling, and consistency are administrator responsibilities.

This approach was chosen to avoid modeling and maintaining inconsistent geographic hierarchies across countries, such as states, governorates, provinces, cities, towns, districts, and municipalities.
CountryId remains the only normalized lookup table referencing column for reliable country-level grouping, while the lower address levels remain flexible enough to represent different countries without forcing them into a fixed hierarchy.
i.e
    Egypt -> Governorate -> City -> District
    United States -> State -> City -> County (or different based on state)

The trade-off does not prevent practical filtering by location. as branch queries may still filter Locality and AdministrativeArea using text matching. At the expected number of branches is not large enough to need an index, specially if applying these filters after restricting by cinema or country is sufficiently inexpensive.
Future nearest-branch or map-based discovery is independent of this hierarchy. When required, latitude and longitude can be added to CinemaBranches, and distance-based searching can operate directly on those coordinates without requiring normalized city, district, or administrative-area tables.

---

## 8. Indexing strategy

The schema defines initial essential indexes only.

### 8.1 General principles

- Primary and alternate keys cover required identity and ownership paths.
- Composite child-side indexes are added where needed for important composite FK joins.
- Standalone indexes are added only when the leftmost prefix of another index does not support the expected access pattern.
- Low-selectivity activation flags are not indexed alone.
- Filtered indexes are used where they directly support a narrow operational subset, such as active holds or nullable provider references.

### 8.2 Post-implementation tuning

After representative data and query plans exist, evaluation(based on static query analysis for frequent access patterns and hot paths and execution plans) will be done for index tuning that may:

- add covering indexes with `INCLUDE` columns;
- add indexes for right-side composite-key lookup patterns;
- extending indexes with filter or ordering columns;
- removing overlapping or unused indexes;
- 
---

## 9. Extra Notes:

---

## 10. Final considerations

- the DBML file is to be kept in sync with migrations and domain changes and treated as source of truth
- any static asset such as the schema SVG is to be re-generated after material schema or layout changes.
- this document is to be kept updated when a design decision changes or application-enforcement change.
