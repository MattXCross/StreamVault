# StreamVault Admin

Internal admin area for managing the StreamVault catalogue: movies, series,
audiobooks and music albums. An admin can list, search, filter, create, edit
and delete content, and inspect per-title analytics inline.

## Running it

Requirements: .NET 10 SDK.

```
dotnet run --project StreamVault
```

Or open `StreamVault.sln` in Visual Studio and press F5. On first run.

The dev connection string has an in-code default (`Data Source=streamvault.db`),
so nothing needs to be configured to run locally. For a real deployment the
connection string would be supplied through user secrets or environment
variables so that no live credentials are ever committed.

## Stretch goal: per-content analytics

The optional feature I added is analytics for each piece of content. The data
is split into two layers:

- **`ContentStats`** a long-term overview model that keeps a general lifetime
  summary of a few core stats (total plays, thumbs up, thumbs down). This is
  cheap to store and never expires, so lifetime figures survive forever.
- **`PlayEvent`** a detailed, event-level record (content id, timestamp,
  country) written for every play. This is what lets us report the more
  detailed recent-behaviour stats: plays over time and plays by country. It is
  far heavier to store, so we only keep the most recent window of it.

The play events are cleaned up by a time to live service in lieu of the ability
to set a TTL declaratively as we could on a managed SQL instance such as Azure
SQL. Because a managed database would handle expiry natively, this service is
only spun up when we are connected to the local SQLite database. Its behaviour
is configured in `appsettings.json` under `Analytics`:

- `PlayEventRetentionDays` how far back to keep play events (default 30).
- `EnableSimulator` whether to generate simulated traffic.
- `EventsPerSecond` the rate of the live simulator.

The provider is selected by `Database:Provider`. Only `"Sqlite"` is
implemented; any other value fails fast at startup rather than silently doing
the wrong thing.

## Service and repository layer

Catalogue data is reached through a DI service layer rather than touching the
`DbContext` from controllers directly. The catalogue is implemented as an
interface, `ICatalogueRepository`, with an EF Core implementation
(`EfCatalogueRepository`) registered against it. Exposing it as an interface
means we can add further implementations later, so migrating away from the
locally running SQLite database to a cloud-hosted or even a non-SQL store
is a smoother change that does not ripple out into the controllers.

The analytics side follows the same pattern. `IAnalyticsService` defines the
contract; `EfAnalyticsService` is the current implementation. In the future we
would build out the real front end and controllers for recording plays, and
those would record plays by calling this same `IAnalyticsService`. The traffic
simulator is deliberately written the same way, it records plays through
`IAnalyticsService` exactly as a real controller would, and nothing else in the
app depends on it. That is what makes it replaceable: it is a stand-in for the
real recording flow, not an abstraction of its own, so once that flow exists the
simulator is simply switched off (`EnableSimulator = false`) or deleted and
nothing else has to change.

## How the database is created and seeded

The schema is created on startup with `EnsureCreated` in `Data/DbSeeder.cs`.

We deliberately do **not** auto-run migrations. Auto-migrating on startup is a
bad idea for production deployment, and for local development I personally
prefer to run them manually as well. This makes it an explicit decision gives clear
visibility when something goes wrong, and it keeps local development aligned
with the production process.

Seeding runs only when the tables are empty, so restarting the app never
duplicates data. There are two parts to it:

- **Catalogue data** a fixed set of movies, series, audiobooks and albums is
  inserted from `DbSeeder` if the `Contents` table is empty. This gives enough
  rows to show off search, filtering and pagination.
- **Analytics data** when the simulator is enabled and no stats exist yet,
  `AnalyticsSimulator` backfills a plausible history of `PlayEvent`s and an
  aggregate `ContentStats` row for every content item. This gives the analytics
  panels something realistic to show on first run including deliberately
  varied trends (declining, steady, rising) so viewership drop-off is visible.

Beyond the initial backfill, `AnalyticsSimulationService` keeps generating live
plays in the background at the configured rate, standing in for the real play
traffic a production system would receive.

## Inheritance and polymorphism in the data model

All content types derive from an abstract `ContentItem` base that holds the
fields common to everything title, description, release date, age rating and
genre. Each concrete type (`Movie`, `Series`, `Audiobook`, `Album`) adds only
its own fields and overrides `TypeDisplayName`. Putting the shared shape on the
base class removes duplicate code and means a change to a common field is made
in exactly one place, which makes the app easier to maintain.

## Data schema: table-per-hierarchy

The hierarchy is persisted as **table-per-hierarchy (TPH)**: every content type
lives in a single `Contents` table with a `ContentType` discriminator column
telling us which type each row is. TPH keeps the hot path the full catalogue
lookup, which is the most common query as a single indexed table scan with no
joins. A table-per-type design would force an expensive join for that read on
every request, so TPH is the better fit for a read-heavy admin list.

## The generic content controller

Create/edit/delete logic is written once in a generic base controller,
`ContentEntryController<TContent, TViewModel>`. The concrete controllers
(`MoviesController`, `SeriesController`, and so on) are one-line subclasses that
simply bind the base to their content and view-model types.

The type parameters are how we discern the type of the incoming request data:
MVC model-binds the request straight onto the correct strongly-typed view
model, while we keep only one implementation of the actual controller logic.
That single implementation is far easier to maintain, and adding a new content
type is trivial implement the new model and view model, then declare a new
controller class that inherits from the template. No existing logic has to
change.

## Other uses of inheritance, polymorphism and separation of concerns

- **View-model hierarchy.** Form view models mirror the content hierarchy and
  override `ApplyTo`/`LoadFrom` to map themselves to and from their entity.
  Mapping lives on the polymorphic type rather than in branching controller
  code, so the base controller never needs to know which concrete type it holds.
- **No type-checks scattered through the code.** The one place that maps a
  content type to its controller and display name is the `ContentTypes`
  registry. There is no `if`/`switch` on content type anywhere in the
  controllers or views.
- **Separation of concerns.** Controllers are thin.
  Querying and persistence live in the repository class, analytics logic lives in the
  analytics service, and background concerns (TTL purge, live simulation) live
  in their own hosted services. Each layer has a single responsibility.

## Testing

The core services are covered by unit tests (`StreamVault.Tests`). These verify
that the repository and analytics logic behave correctly, and, just as
importantly, they act as a contract that any future implementation can be
tested against. If we later add an implementation backed by a cloud-hosted
database, or even a non-SQL store, the same tests protect us from breaking
existing functionality as we build the new version of the service.

## What I'd do next with more time
- Integrate DuckDB to power a much more robust analytics view. DuckDB is a
  columnar database, meaning it stores each column of data together rather than
  each row together. Analytics queries almost always read a few columns across a
  very large number of rows (for example summing plays by day or by country), so
  storing the data column-by-column lets DuckDB read only the columns it needs
  and skip the rest. This makes querying large stat sets far more efficient than
  a row-based store like SQLite, which has to walk whole rows even when it only
  wants one or two fields. On top of that we could build a richer analytics
  dashboard, which would give an admin much better information to make decisions
  with when managing the content of the app.
- Protect against two admins editing the same content at once. Right now if two
  people open the same item and both save, the second save silently overwrites
  the first. The simplest fix is a warning: when someone opens an item's edit
  screen, show a message to anyone else who opens it that another admin is
  already editing this content, so they know their changes might clash. A
  stronger option is a checkout system, where opening an item for editing marks
  it as locked and puts everyone else into a read only view until the first
  person is finished, which removes the risk of a clash entirely at the cost of
  making people wait their turn.
