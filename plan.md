# StreamVault — Admin Catalogue Plan

Shared working document for Matt + Claude. Tick items off as they land, add notes
under the session log at the bottom. If a decision changes, update the Decisions
section rather than leaving it stale.

## Brief (summary)

Internal admin area for the StreamVault catalogue. ASP.NET Core MVC (Razor),
SQLite created + seeded automatically on first run ("clone and press F5").
Four content types sharing common properties, each with type-specific fields.
List (filter by type, search by title), Create, Edit, Delete, server-side
validation. Scored on: readable code, good inheritance/polymorphism (no
scattered type-checks), separation of concerns, sensible schema and queries.

## Decisions

| Decision | Choice | Why |
|---|---|---|
| Framework | .NET 10 (current LTS), template as scaffolded | Brief says ".NET 8 or latest LTS"; net10.0 is the latest LTS. |
| Data access | **EF Core + SQLite** | Only option with native inheritance mapping; `EnsureCreated()` gives auto-create+seed for free; change tracking simplifies Edit. |
| Schema | **TPH (single table + discriminator)** | List page (hot path) is a single table scan — no joins/unions. Nullable subtype columns never leak past the data layer because EF materialises proper subclasses. EF's default and best-supported strategy. TPT rejected: recurring join cost to fix a cosmetic problem. TPC rejected: common columns ×4, cross-type sort/search over a UNION. |
| Domain model | Abstract `ContentItem` base + `Movie`, `Series`, `Audiobook`, `Album` subclasses | Common data on the base, specifics on subclasses. |
| Data layer | `ICatalogueRepository` interface, EF implementation, registered via DI | Swappable/mockable data services (Matt's requirement). |
| Controllers | **Generic base controller**: abstract `ContentEntryController<TContent, TViewModel>` with the shared Create/Edit/Delete flow; four thin subclasses. Separate `CatalogueController` for list/filter/search | Per-type endpoints remove the need for type-switching; base class removes the duplication that four plain controllers would have. |
| Auth | None | Per brief — admin assumed logged in. |

## Architecture outline (Phase 1)

```
StreamVault/
  Models/                      # domain entities
    ContentItem.cs             # abstract: Id, Title, Description, ReleaseDate,
                               #   AgeRating (enum), Genre, abstract TypeDisplayName
    Movie.cs                   # DurationMinutes, Director
    Series.cs                  # Seasons, TotalEpisodes
    Audiobook.cs               # Author, Narrator, DurationMinutes
    Album.cs                   # Artist, TrackCount, RecordLabel
    AgeRating.cs               # enum (U, PG, 12, 15, 18 or similar)
  Data/
    CatalogueDbContext.cs      # DbSet<ContentItem> + TPH config
    ICatalogueRepository.cs    # Search(query), Find<T>(id), Add, Update, Delete
    EfCatalogueRepository.cs
    CatalogueQuery.cs          # type filter + title search (+ paging later)
    DbSeeder.cs                # EnsureCreated + seed a few items per type
  ViewModels/
    ContentViewModel.cs        # abstract: common fields + validation attributes,
                               #   abstract ToEntity()/ApplyTo(entity)/LoadFrom(entity)
    MovieViewModel.cs / SeriesViewModel.cs / AudiobookViewModel.cs / AlbumViewModel.cs
    CatalogueListViewModel.cs  # rows + current filter/search state
  Controllers/
    CatalogueController.cs     # Index (list, filter by type, search by title)
    ContentEntryController.cs  # abstract generic base: GET/POST Create,
                               #   GET/POST Edit, POST Delete (with confirm)
    MoviesController.cs / SeriesController.cs
    AudiobooksController.cs / AlbumsController.cs
  Views/
    Catalogue/Index.cshtml     # table w/ type badge, filter dropdown, search box
    Shared/_CommonContentFields.cshtml   # partial for base-class fields
    Movies|Series|Audiobooks|Albums/Create.cshtml, Edit.cshtml
```

Notes:
- **No type-switches anywhere**: the list page renders `TypeDisplayName`
  polymorphically; per-row Edit/Delete links resolve controller names in one
  place (a small view helper / dictionary), not via `if (item is Movie)`.
- **Validation** lives on the view models via DataAnnotations (`[Required]`
  Title, `[Range(1, …)]` durations/counts/seasons, etc.). Controllers check
  `ModelState`; views show validation summary + per-field messages. Template's
  jQuery unobtrusive validation gives client-side echo for free.
- **Delete** is POST + anti-forgery with a confirm step (kept minimal).
- **Startup**: connection string in appsettings (`Data Source=streamvault.db`);
  `DbSeeder` runs on app start — clone/F5 requirement.
- Movie and Audiobook both have a duration; they stay separate properties on
  each subclass (may share one TPH column via config — decide during build).

## Phase 1 — MVP per the brief

- [ ] EF Core + SQLite packages, connection string, `CatalogueDbContext` with TPH mapping
- [ ] Domain entities: `ContentItem` + four subclasses, `AgeRating` enum
- [ ] `ICatalogueRepository` + `EfCatalogueRepository`, DI registration
- [ ] `DbSeeder`: EnsureCreated + seed data (a few items per type)
- [ ] View models with validation attributes + entity mapping
- [ ] `ContentEntryController<TContent, TViewModel>` base (Create/Edit/Delete)
- [ ] Four per-type controllers + Create/Edit views (shared partial for common fields)
- [ ] `CatalogueController` Index: list with type visible, filter by type, search by title
- [ ] Delete with confirm, POST + anti-forgery
- [ ] Layout/nav tidy-up (point template nav at Catalogue), remove leftover template cruft
- [ ] Manual test pass: all four types through create → list → filter → search → edit → delete
- [ ] README: run instructions + brief rationale for EF Core / TPH choices

## Phase 2 — Pagination

- [ ] Add page number/size to `CatalogueQuery`; repository returns a paged result (items + total count)
- [ ] Pager UI on the list page, preserving active filter/search in page links

## Phase 3 — Analytics (with simulator)

Goal: play/engagement stats per content item, as if a real consumer app were
driving traffic.

- Separate `AnalyticsEvents` (or aggregated `ContentStats`) table with FK to
  `ContentItem.Id` — first genuinely relational data in the schema (works fine
  against the TPH table).
- `IAnalyticsService` interface so implementations are swappable via DI:
  - [ ] Real implementation reading from the analytics table
  - [ ] Simulator implementation generating plausible plays/engagement over time
- [ ] Surface stats in the admin UI (list columns and/or details view)
- To discuss: event-level vs pre-aggregated storage; whether the simulator
  writes to the same table (background service) or fakes reads.

## Phase 4 — DuckDB-driven frontend filtering

Goal: ship catalogue data to the browser once, run filter/search/sort locally
(DuckDB-WASM) to cut backend round-trips on large datasets.

Open questions to talk through before building:
- **Initial load** without overwhelming the system: full snapshot vs chunked/
  paged hydration vs a columnar export endpoint (e.g. Parquet) DuckDB ingests directly.
- **Sync with multiple concurrent admins**: staleness tolerance? Options range
  from versioned snapshot + delta polling to push (SignalR) invalidation.
- **Split of responsibilities**: what stays server-rendered vs what becomes
  client-driven; how this coexists with Phase 2 server paging.
- Progressive enhancement: server filtering must keep working without it.

## Session log

- **2026-07-04** — Reviewed template (stock MVC, net10.0). Agreed decisions:
  EF Core, TPH, repository interface, generic base controller. Deferred
  pagination/analytics/DuckDB to later phases. Wrote this plan.
