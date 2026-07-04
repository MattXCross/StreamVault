# StreamVault Admin

Internal admin area for managing the StreamVault catalogue: movies, series,
audiobooks and music albums.

## Running it

Requirements: .NET 10 SDK.

```
dotnet run --project StreamVault
```

Or open `StreamVault.sln` in Rider / Visual Studio and press F5. On first run
the SQLite database (`streamvault.db`) is created and seeded automatically —
no setup steps.

## Design notes

**Data access — EF Core.** The domain is an inheritance hierarchy
(`ContentItem` with four subclasses) and EF Core is the only option of the
three (EF Core / Dapper / ADO.NET) that maps one natively — with Dapper we
would be hand-writing a discriminator switch in the data layer, which is
exactly the type-checking this codebase avoids. `EnsureCreated()` also gives
the clone-and-F5 database creation for free. Data access sits behind
`ICatalogueRepository` (registered via DI), so the implementation can be
swapped or mocked without touching controllers.

**Schema — table-per-hierarchy (TPH).** All content lives in one `Contents`
table with a `ContentType` discriminator. The hottest query — the mixed-type
catalogue list — is a single table scan with no joins or unions; filtering by
type is a `WHERE` on the discriminator. The nullable subtype columns never
leak out of the data layer because EF materialises proper `Movie` /
`Audiobook` / etc. instances. Table-per-type was rejected because it pays a
recurring join cost on every read to fix what is only a cosmetic issue, and
table-per-concrete-type because it duplicates the common columns four times
and pushes cross-type search/sort onto a `UNION ALL`.

**Controllers — one generic base, four thin subclasses.**
`ContentEntryController<TContent, TViewModel>` implements the Create / Edit /
Delete flow once; `MoviesController` and friends are one-line subclasses that
give each type its own routes, views and form. There are no `if (item is
Movie)`-style checks anywhere: per-type behaviour comes from overrides
(`TypeDisplayName`, view-model `ApplyTo`/`LoadFrom`), and the single place
that knows which controller serves which type is the `ContentTypes` registry
used by the list view to build links.

**Validation.** DataAnnotations on the form view models (required title,
positive durations/counts, plus a cross-field rule that a series can't have
fewer episodes than seasons). Controllers check `ModelState`; errors render
per-field with client-side echo via unobtrusive validation.

## Project layout

```
StreamVault/
  Models/       domain entities (ContentItem hierarchy, AgeRating)
  Data/         DbContext, repository interface + EF implementation, seeder
  ViewModels/   form view models with validation + entity mapping
  Controllers/  Catalogue (list) + generic base + four per-type controllers
  Views/        list page, per-type Create/Edit forms, shared field partials
```

`plan.md` tracks progress and the roadmap (pagination, analytics with a
simulated traffic source, DuckDB-assisted client-side filtering).
