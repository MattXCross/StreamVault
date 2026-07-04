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

- [x] EF Core + SQLite packages, connection string, `CatalogueDbContext` with TPH mapping
- [x] Domain entities: `ContentItem` + four subclasses, `AgeRating` enum
- [x] `ICatalogueRepository` + `EfCatalogueRepository`, DI registration
- [x] `DbSeeder`: EnsureCreated + seed data (a few items per type)
- [x] View models with validation attributes + entity mapping
- [x] `ContentEntryController<TContent, TViewModel>` base (Create/Edit/Delete)
- [x] Four per-type controllers + Create/Edit views (shared partial for common fields)
- [x] `CatalogueController` Index: list with type visible, filter by type, search by title
- [x] Delete with confirm, POST + anti-forgery
- [x] Layout/nav tidy-up (point template nav at Catalogue), remove leftover template cruft
- [ ] Manual test pass: all four types through create → list → filter → search → edit → delete
- [x] README: run instructions + brief rationale for EF Core / TPH choices

## Phase 2 — Pagination

- [x] Add page number/size to `CatalogueQuery`; repository returns a paged result (items + total count)
- [x] Pager UI on the list page, preserving active filter/search in page links

## Phase 3 — Analytics (with simulator)

Goal: play/engagement stats per content item, as if a real consumer app were
driving traffic.

**Storage — two tiers, because two questions have different lifetimes:**

- `ContentStats` (aggregate, one row per content item, **never expires**) —
  lifetime rollups that must survive event purging:
  - `ContentItemId` (FK to `ContentItem.Id`, one-to-one)
  - `TotalPlays` (lifetime)
  - `ThumbsUp`, `ThumbsDown` (lifetime counts)
  - `PositiveRatingPercent` — computed, not stored: `ThumbsUp / (ThumbsUp +
    ThumbsDown)`, shown Steam-style in the UI.
- `PlayEvent` (event-level, **30-day TTL**) — one row per play, so we can
  chart plays over time and see viewership drop off:
  - `Id`
  - `ContentItemId` (FK)
  - `PlayedAt` (UTC timestamp)
  - `Country` (country of origin the play came from)

Rationale: ratings and lifetime plays are cheap running totals we keep
forever on `ContentStats`; individual play events are the bulky, ephemeral
part, so they carry a 30-day TTL to cap storage cost. When an event is
purged, `TotalPlays` on the aggregate is unaffected — it was already
incremented when the play happened. `Country` lives on the event because it
is inherently per-play; geographic breakdown is therefore "last 30 days".

**TTL implementation:** SQLite has no native TTL, so a background
`IHostedService` periodically deletes `PlayEvent` rows older than 30 days.
Time-window queries also filter by date defensively, so a late purge never
shows stale data.

### Steps (build + stop for inspection after each)

- [ ] **Step 1 — Analytics domain models.** `ContentStats` and `PlayEvent`
  in `Models/`, mirroring the existing entity style (plain properties,
  computed `PositiveRatingPercent` marked `[NotMapped]`, no comments).
- [ ] **Step 2 — DbContext + schema.** Add `DbSet`s, configure the FK to
  `ContentItem`, one-to-one for stats, and an index on
  `PlayEvent(ContentItemId, PlayedAt)` for the time-series query.
- [ ] **Step 3 — `IAnalyticsService` + EF implementation.** Read side:
  stats for one item, plays-over-time (grouped by day within a window),
  country breakdown; write side: record a play (insert event + bump
  aggregate), record a rating (bump thumbs up/down). Registered via DI.
- [ ] **Step 4 — Simulator.** Seeds `ContentStats` for existing content and
  generates plausible historical `PlayEvent`s + ratings, so the UI has data
  on first run. (Decide: one-off seed vs ongoing background traffic.)
- [ ] **Step 5 — TTL purge.** `IHostedService` deleting `PlayEvent`s older
  than 30 days on an interval.
- [ ] **Step 6 — UI.** Surface `% positive` and total plays on the list,
  and a per-item analytics view with a plays-over-time filter (viewership
  trend) and country breakdown.
  - **Note:** stats creation is lazy — `GetStatsAsync` returns null and the
    play/country queries return empty for content never played or rated.
    Every read site must handle the no-stats case explicitly (Steam-style
    "No plays / No ratings yet" rather than a crash or a blank).

Open decisions to confirm at inspection: `Country` as a `string` (ISO code)
vs a fixed enum; whether the simulator is a one-off seed or a live
background feed; where analytics surfaces first (list columns vs a dedicated
details page).

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
- **2026-07-04 (build)** — Phase 1 built in staged commits: EF package →
  domain models → data layer (verified seeding creates the single Contents
  table) → view models → per-type controllers/views → catalogue list page.
  Smoke-tested via curl: list/filter/search, create (valid + invalid with
  server-side validation errors), type-scoped edit (wrong-type id 404s),
  delete. Remaining: manual browser pass by Matt + README.
- **2026-07-04 (later)** — Working style agreed: no explanatory comments in
  code (self-documenting), and Matt inspects + makes every commit himself.
  Removed remaining comments. Built Phase 2 pagination (PagedResult<T>,
  page/pageSize on CatalogueQuery, clamped page, Bootstrap pager preserving
  filter/search) — verified with 13 rows over 2 pages via curl. Pinned
  SQLitePCLRaw.bundle_e_sqlite3 3.0.3 to clear NU1903 advisory on the
  transitive 2.1.11; runtime smoke-tested.
