<h1 align="center">RezervisiMe</h1>

<p align="center">
  <strong>An online accommodation reservation system</strong>
</p>

<p align="center">
  A web application simulating an Airbnb-style booking platform, built for the Applied Software Engineering course at FTN, University of Novi Sad.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET%20Framework-4.7-512BD4?logo=dotnet" alt=".NET Framework" />
  <img src="https://img.shields.io/badge/ASP.NET%20Web%20API-2-512BD4" alt="ASP.NET Web API 2" />
  <img src="https://img.shields.io/badge/Storage-JSON%20Files-yellow" alt="JSON File Storage" />
  <img src="https://img.shields.io/badge/Frontend-jQuery%20%2B%20Vanilla%20JS-0769AD?logo=jquery" alt="jQuery" />
  <img src="https://img.shields.io/badge/Maps-Leaflet-199900?logo=leaflet" alt="Leaflet" />
  <img src="https://img.shields.io/badge/Auth-Custom%20Bearer%20Tokens-orange" alt="Custom Auth" />
</p>


<p align="center">
<img width="1080" height="608" alt="2026-06-27 22-01-40" src="https://github.com/user-attachments/assets/eddcc8c8-0508-485d-abda-a88730a5ee34" />

</p>


---

## What is RezervisiMe?

RezervisiMe simulates an online accommodation booking platform along the lines of Airbnb or Booking.com. Hosts list accommodations, guests search and reserve them, and an administrator moderates the whole system: approving reservations, moderating reviews, and managing accounts.

The project was built to the specification of the Web Programming course's regular exam project: a three-role system (Guest, Host, Administrator) backed entirely by flat JSON files instead of a database, with all the search, sort, and business-rule constraints that come with a real booking flow.

---

## Tech Stack

### Backend

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Web API 2 (.NET Framework 4.7) |
| Architecture | Layered (Controllers → Services → Repositories → Storage) |
| Data Storage | Flat JSON files per entity, read/written through a generic file-backed store |
| Authentication | Custom bearer tokens, issued and validated in-memory |
| Authorization | Role-based, enforced via a custom `AuthorizeRoleAttribute` |
| Error Handling | Result pattern (`Result<T>` / `Error`) instead of exceptions |
| Image Uploads | Multipart form handling, written to `Content/uploads` |
| Password Storage | Salted hashing via a dedicated `PasswordHasher` |
| Admin Provisioning | Administrators seeded from `App_Data/admins.json` at startup, never created at runtime |

### Frontend

| Layer | Technology |
|-------|-----------|
| Structure | Static HTML pages served from `Client/` (index, accommodations, accommodation, login, register, profile, reservations, admin) |
| Scripting | Vanilla JS + jQuery, split per page under `Client/js/pages` |
| Maps | Leaflet, with a Serbia-shaped overlay for location-based browsing |
| Styling | Modular per-concern stylesheets under `Client/css` |
| API Access | A shared `api.js` wrapper handling auth headers and JSON requests |

---

## Architecture

```
RezervisiMe/                   # Single ASP.NET Web Application project (RezervisiMe.csproj)
├── RezervisiMe.API/            # Backend code, organized as a namespace inside the project
│   ├── Controllers/             # AuthController, UsersController, AccommodationsController,
│   │                            # ReservationsController, ReviewsController
│   ├── Services/                # Business logic per aggregate (UserService, AccommodationService, ...)
│   │   └── Interfaces/
│   ├── Repositories/             # Thin data-access layer wrapping JsonFileStore<T>
│   │   └── Interfaces/
│   ├── Models/                   # Domain entities (User, Accommodation, Reservation, Review)
│   │   ├── Dto/                   # Outbound shapes returned to the client
│   │   └── Requests/               # Inbound shapes accepted from the client
│   ├── Auth/                      # TokenStore (issue/validate/revoke), AuthorizeRoleAttribute
│   └── Infrastructure/             # JsonFileStore<T>, Composition root, Result/Error,
│                                    # AdminBootstrapper, SeedBootstrapper, ImageUploader
│
├── Client/                     # Static frontend, served directly by the same web app
│   ├── *.html                   # index, accommodations, accommodation, login, register,
│   │                             # profile, reservations, admin
│   ├── css/                      # One stylesheet per concern (hero, cards, header, auth, ...)
│   ├── js/pages/                  # One script per page
│   ├── js/lib/                    # Vendored jQuery and Leaflet
│   └── js/serbia-shape.js         # Serbia border geometry used by the home page map
│
├── Content/uploads/            # Accommodation images saved by ImageUploader
│
├── App_Data/                   # users.json, accommodations.json, reservations.json,
│                                # reviews.json, admins.json, plus seed_*.json fixtures
│
├── Global.asax(.cs)            # Application startup: runs AdminBootstrapper + SeedBootstrapper
└── Web.config                  # Routing, compilation, and Web API configuration
```

There is no separate frontend or API project, the whole solution is one `RezervisiMe.csproj`. `RezervisiMe.API` is purely a namespace/folder convention for the backend code, while `Client/` holds the static pages the same app serves.

### Key Design Patterns

**Layered architecture.** Controllers only translate HTTP into service calls and map `Result<T>` to an `IHttpActionResult`. Services hold all business rules. Repositories are a thin pass-through to storage. Nothing above the repository layer knows that data lives in a JSON file.

**Result pattern instead of exceptions.** Every service method returns `Result<T>` (or `Result` for void operations) rather than throwing. A `Result<T>` carries either a value or an `Error`, with implicit conversions from both, so a service method can simply `return someValue;` or `return Error.Validation("...");` and let the caller branch on `IsSuccess`. Controllers convert this to an HTTP response in one line via a `ToHttpResult()` extension.

**Generic JSON-backed store.** `JsonFileStore<T>` is a single generic class that knows how to load, add, update, and soft-delete any entity deriving from `EntityBase`, serialized to its own file under `App_Data`. All four repositories (`User`, `Accommodation`, `Reservation`, `Review`) are thin wrappers around an instance of this store, so adding a fifth entity type means writing one file, not a new persistence layer.

**Soft delete everywhere.** `EntityBase` carries an `IsDeleted` flag. `GetAll()` filters deleted rows out automatically; `GetAllIncludingDeleted()` is available where the seed/admin logic needs to see everything. Nothing is ever physically removed from a JSON file, per the spec's requirement.

**Composition root.** `Composition.cs` wires up every store, repository, and service as static singletons in one place. There's no DI container; this single class is the application's entire object graph, instantiated once at startup.

**Custom token authentication.** `TokenStore` is an in-memory `ConcurrentDictionary` issuing opaque GUID tokens with an 8-hour TTL on login, and discarding them on logout. `AuthorizeRoleAttribute` is a custom `AuthorizeAttribute` that validates the bearer token against `TokenStore` and optionally checks the caller's role against an allow-list passed into the attribute, e.g. `[AuthorizeRole("Domacin", "Administrator")]`.

**Admin accounts are provisioned, not registered.** `AdminBootstrapper` reads `App_Data/admins.json` on startup and creates any administrator accounts that don't already exist by username. There is no `/api/auth/register` path that can produce an Administrator; the only way to get one is to be seeded from that file, as required by the spec.

---

## A Serbia-Shaped Map, Not Just a Map Pin

The home page's location picker isn't a generic Leaflet widget dropped on the page, it's clipped into the actual outline of Serbia.

`serbia-shape.js` stores three things extracted from Serbia's real border geometry: a normalized SVG clip path (`clipD`, in `objectBoundingBox` units so it scales with any container), a separate outline path (`outlineD`) for drawing the border as a visible line, and the lat/lng bounding box the shape corresponds to (`bounds`).

On page load:

1. An invisible `<svg>` defines a `clipPath` from `clipD` and injects it into the page once.
2. The Leaflet map container (`#serbia-map`) gets `clip-path: url(#serbia-clip)` in CSS, so the rectangular tile layer underneath is masked down to Serbia's exact silhouette instead of a box.
3. A second `<svg>` traces `outlineD` on top as a thin white stroke, so the country's border reads clearly against the map tiles.
4. `map.fitBounds()` uses the stored `bounds` so the clipped shape and the tile layer line up perfectly regardless of viewport size.
5. Panning, zooming, and all other interaction are disabled (`dragging: false`, `scrollWheelZoom: false`, etc.), since this is a fixed decorative/selection map, not a free-roam one.

City selection drops a custom pin built from a Leaflet `divIcon` (`rs-pin` / `rs-pin__shape` in CSS) rather than the default marker image, and `map.flyTo()` animates to the chosen city's coordinates. Typing a city name filters against a small `CITIES` lookup table with diacritic-insensitive matching, so "Beograd" and "beograd" resolve the same way.

---

## Roles & Permissions

| Role | Can do |
|------|--------|
| **Unauthenticated visitor** | Browse and search available accommodations on the home page |
| **Guest** | Everything a visitor can, plus: reserve accommodations, cancel their own reservations, view/filter their reservations by status, and create/edit/delete reviews for stays that reached status `ZAVRSENA` |
| **Host** | Manage their own accommodations: create, edit, delete (only when not currently reserved), filter by availability, sort by name/price/posting date |
| **Administrator** | Search and sort all users; manage hosts and guests (with cascading cancellation of a removed guest's active reservations); approve/reject reservations and reviews; edit or soft-delete any accommodation; view every reservation in the system |

Role enforcement happens entirely server-side through `AuthorizeRoleAttribute` on each controller action, so the same token that authenticates a request also determines what that request is allowed to do.

---

## Features

### Implemented

- Registration and login with bearer-token issuance, logout with token revocation
- Profile viewing and editing for the logged-in user
- Accommodation listing, search (name, city, type, price range), combined multi-parameter search, and sorting (name, price, date posted, both directions)
- Image upload required on accommodation creation, validated by extension and size
- Reservation creation with automatic total-price calculation, guest-count validation against the accommodation's capacity, and overlap checking against existing `ODOBRENA` reservations
- Reservation lifecycle: `KREIRANA → ODOBRENA / OTKAZANA → ZAVRSENA`, with cancellation locked out inside the 24-hour pre-check-in window
- Review creation restricted to guests with a completed (`ZAVRSENA`) stay at that accommodation, with admin approval/rejection before a review becomes publicly visible
- Administrator user management with cascading reservation cancellation when a guest is deleted
- Logical (soft) delete across all entities, with deleted accommodations excluded from listings and reservations
- Administrators loaded exclusively from a seed file, never user-creatable
- Leaflet map on the home page, clipped to Serbia's actual border shape with a custom pin and city-name search

### Planned / Out of Scope

- Payment processing
- Real-time notifications
- Multi-language support beyond the Serbian-language domain status values required by the spec

---

## API Overview

| Endpoint Group | Description |
|---------------|-------------|
| `POST /api/auth/register`, `/login`, `/logout`, `GET/PUT /me` | Registration, login/logout, profile read and update |
| `GET /api/accommodations`, `/mine`, `/{id}` | Public listing/search/sort, a host's own accommodations, single accommodation detail |
| `POST /api/accommodations`, `PUT/DELETE /{id}`, `POST /upload-image` | Host (and admin) create/edit/delete, image upload |
| `GET /api/reservations/mine`, `/` (admin) | A guest's own reservations, or every reservation for admins |
| `POST /api/reservations`, `/{id}/cancel`, `/{id}/approve`, `/{id}/reject` | Create, cancel, and admin approve/reject |
| `GET /api/reviews/for-accommodation/{id}`, `/mine`, `/` (admin) | Public approved reviews for a listing, a guest's own reviews, all reviews for admins |
| `POST /api/reviews`, `PUT/DELETE /{id}`, `/{id}/approve`, `/{id}/reject` | Create/edit/delete by the author, admin moderation |
| `GET/POST/PUT/DELETE /api/users/*` (admin only) | Search/sort users, create hosts, edit, soft-delete |

All endpoints follow the same pattern: a successful `Result<T>` returns the value with a 200, a failed one returns a JSON error body with the appropriate status code, set by the `ToHttpResult()` extension.

---

## Data Persistence

Per the project specification, there is no database. Every entity is persisted as a JSON array in its own file under `App_Data` (`users.json`, `accommodations.json`, `reservations.json`, `reviews.json`), with administrators seeded separately from `admins.json`. A parallel set of `seed_*.json` files provides the test data the spec requires for demonstrating every feature, loaded once by `SeedBootstrapper` (tracked via a `.seeded` marker file so seeding doesn't repeat on every restart). `JsonFileStore<T>` serializes and deserializes through Newtonsoft.Json and guards every read/write with a lock, since IIS Express can serve multiple concurrent requests against the same file.

---

## Getting Started

### Prerequisites

- Visual Studio 2019+ with the ASP.NET and web development workload
- .NET Framework 4.7 (or compatible) targeting pack

### Setup

1. **Clone the repository**
   ```bash
   git clone https://gitlab.com/ignjatradojicic/rezervisime.git
   cd rezervisime
   ```
2. **Open `RezervisiMe.sln` in Visual Studio.**
3. **Restore NuGet packages** (Visual Studio does this automatically on build, or run `nuget restore` from the Package Manager Console).
4. **Run the project** (F5 / IIS Express). On first run, `SeedBootstrapper` and `AdminBootstrapper` populate `App_Data` with the seed and admin accounts, so the app is immediately usable with demo data.
5. Open the served `Client/index.html` to browse accommodations as a visitor, or log in with one of the seeded accounts to try the Guest, Host, or Administrator flows.

---

## Author

**Ignjat Radojicic**

- GitHub: [@IgnjatRadojicic](https://github.com/IgnjatRadojicic)

---

## License

This project was built for academic purposes as part of the Web Programming course at FTN, University of Novi Sad.
