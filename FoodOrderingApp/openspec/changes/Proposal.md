# Proposal: CraveDash — Cross-Platform Food Ordering Application

- **Change ID:** `build-food-ordering-app`
- **Status:** Approved
- **Author:** Syncfusion Code Studio (OpenSpec `/opsx-propose`)
- **Target Stack:** .NET MAUI (`net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`)

---

## 1. Project Overview

**CraveDash** (application ID `com.cravedash.foodorderingapp`) is a feature-rich, cross-platform food ordering application built with **.NET MAUI** and the **MVVM architecture pattern**, using **Syncfusion .NET MAUI controls** for the presentation layer, a local **SQLite** database for data persistence, and **CommunityToolkit.Mvvm** for data binding and commands.

The application delivers a complete, end-to-end food-ordering experience on a single codebase targeting Android, iOS, macOS (Mac Catalyst), and Windows:

- **Restaurant discovery** — browse restaurants with ratings, cuisine types, and featured dishes; search and filter the catalog.
- **Interactive food catalog** — explore menu items with images, descriptions, ratings, and pricing.
- **Product detail view** — review detailed item information and customize order quantities (1–99).
- **Cart management** — add, remove, and update items with real-time price calculations (subtotal, 18% GST tax, flat delivery fee, total).
- **Multi-payment checkout** — complete purchases via UPI, Net Banking, Credit Card, or Debit Card, with a simulated payment gateway (including realistic latency and simulated failures).
- **Order lifecycle & tracking** — place orders, view order history, track status progression (Confirmed → Preparing → Out for Delivery → Delivered), and cancel orders while still in the Confirmed state.
- **Delivery tracking (simulated)** — view mock delivery-partner information, route locations, live location updates, and estimated delivery times.
- **Profile management** — update personal details, securely change passwords, manage multiple delivery addresses (add/edit/delete/set-default), and track rewards points.

The application is fully offline-first: all data (users, catalog, carts, orders, addresses) is persisted in a local SQLite database (`foodordering.db3`), authentication uses BCrypt password hashing with secure session storage, and external integrations (payment gateway, delivery tracking/maps, delivery partner network) are intentionally represented by **mock service implementations** so the sample runs standalone with no backend dependency.

---

## 2. Problem Statement

Building a production-quality food-delivery frontend that runs natively on Android, iOS, macOS, and Windows typically requires either four separate codebases or a web-based compromise. Teams evaluating .NET MAUI + Syncfusion controls need a reference application that demonstrates a **complete business workflow** — not isolated control demos — covering:

1. **Authentication with real security practices** — secure password storage (hashing, never plaintext), case-insensitive credential lookup, persistent secure sessions, and password change flows.
2. **Commercial transaction logic** — cart price math (GST tax, delivery fees), snapshot pricing at order time (so later catalog price changes don't corrupt historical orders), idempotent cart updates, and transactional order creation (order + line items + rewards + cart clear succeed or fail together).
3. **Responsive, adaptive UI** — layouts that reflow between phone and desktop (e.g., two-column login branding panel on WinUI/Mac Catalyst, adaptive grid spans), full light/dark theme support, and modal dialogs that work across idioms.
4. **Long-running operation UX** — async payment processing with busy overlays, disabled buttons, re-entrancy-safe commands, and clear success/error feedback (inline banners and confirmation dialogs).
5. **State that survives process restarts** — persisted carts per user, order history, remembered login sessions.

No existing sample demonstrates all of these together in a single .NET MAUI codebase using Syncfusion's `SfTextInputLayout`, `SfSwitch`, and shell-based navigation. CraveDash fills that gap.

---

## 3. Goals and Objectives

### 3.1 Goals

| # | Goal | Measured By |
|---|------|-------------|
| G1 | Deliver a complete food ordering workflow (browse → detail → cart → checkout → payment → order tracking) as one uninterrupted flow | End-to-end walkthrough on all 4 target platforms |
| G2 | Demonstrate Syncfusion .NET MAUI controls in real business contexts (`SfTextInputLayout` for forms, `SfSwitch` for filters, Syncfusion Core/Buttons/Inputs/TabView/Charts/DataGrid packages) | Every major screen uses at least one Syncfusion input or control |
| G3 | Enforce data integrity and security by default — BCrypt hashing, SQLite transactions, order price snapshots, user-scoped queries | No plaintext credentials; multi-table order creation is atomic |
| G4 | Provide MVVM reference architecture that is approachable and consistent — DI-registered services/viewmodels/pages, `INotifyPropertyChanged` with `SetProperty`, `AsyncRelayCommand` with re-entrancy guards, `QueryProperty` navigation parameters | All 9 viewmodels follow the same structure; no logic in code-behind beyond view wiring |
| G5 | Achieve responsive, themed UI across form factors and light/dark modes | `OnPlatform`/`OnIdiom`-adaptive modal and page layouts; `AppThemeBinding` throughout |
| G6 | Run fully offline with mock external integrations (payments, maps, delivery network) that mirror real API contracts (`IPaymentService`, `IMapService`) for easy backend swap | App functions with zero network connectivity |

### 3.2 Objectives (Non-Functional)

- **Startup resilience**: database initialization is time-bounded (30 s cancellation token) with graceful fallback to the login route; session restore reroutes the user to `//home` or `//login` without user effort.
- **Perceived performance**: all I/O is async; loading is communicated via `ActivityIndicator` bound to `IsLoading`; buttons disable during operations through `InvertedBoolConverter`.
- **Maintainability**: strictly layered architecture — Views → ViewModels → Services → Database — with every service behind an interface registered in DI.
- **Extensibility**: mock payment/map services implement the same interfaces a real gateway would, enabling backend integration without ViewModel changes.

---

## 4. Success Criteria

1. **Authentication lifecycle works end-to-end.** A new user can sign up (validated fields, ≥8-char password, duplicate-email rejection with "Email already registered"), and after logout can log in with case-insensitive email matching; credentials never stored in plaintext; session persists across app restarts via `SecureStorage`.
2. **Catalog is browsable, searchable, and filterable.** Home shows 8 seeded dishes across 8 restaurants with images, ratings, prices, cuisine, and vegetarian indicators; typing in the search box live-filters by dish/restaurant/cuisine; toggling "Veg Only" reduces results to `IsVeg` items; no matches shows the friendly empty state.
3. **Cart math is exact and itemized.** Cart shows per-item line totals, quantity, and unit price; the summary panel shows Subtotal, Tax (18% GST), Delivery Fee (₹50), and Total; adding the same dish again merges quantities instead of duplicating rows; removing an item updates totals immediately.
4. **Checkout creates orders atomically.** Successful (simulated) payment results in exactly one `Orders` row plus `OrderItems` snapshots at current unit prices, 5% rewards points credited to the user, and an empty cart — all inside one SQLite transaction; payment failure (simulated ~15%) blocks order creation and shows a retryable error banner.
5. **Order tracking reflects the real lifecycle.** Orders page lists history newest-first with emoji status chips in status-specific colors; order detail shows a 2-state timeline (Completed/Current per stage), delivery-partner card (once past `Confirmed`), estimated delivery time based on status, and a Cancel button that exists **only** while status is `Confirmed` and requires an explicit confirmation dialog.
6. **Profile is self-serviceable.** Users can edit name/email, change password (old password verified, ≥8 chars, confirm match required), and fully manage addresses (add, edit, delete with confirmation, set-default applied transactionally across all their addresses).
7. **Cross-platform UI quality.** Every screen adapts to phone/desktop (two-column login on WinUI/Mac Catalyst, responsive grid spans `Default=2 / WinUI=4 / MacCatalyst=3`, adaptive modal sizes) and renders correctly in light AND dark themes.
8. **Builds and runs on all four targets** — `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0` — as a single project (Multi-Target Frameworks in `FoodOrderingApp.csproj`).

---

## 5. Scope and Assumptions

### 5.1 In Scope

| Capability | Included |
|---|---|
| **Authentication** | Sign up, login, logout (with confirmation), session persistence/restoration, password change (old-password verification), case-insensitive email uniqueness |
| **Catalog** | Seeded 8-item catalog; live search (dish/restaurant/cuisine); vegetarian filter; item detail modal with 1–99 quantity stepper |
| **Cart & Checkout** | Per-user persistent cart; merge-on-add; remove; cost breakdown (subtotal + 18% GST + ₹50 delivery); 4-method payment UI (UPI, Net Banking, Credit Card, Debit Card); simulated gateway with latency, ~15% failure rate, `TXN_{timestamp}_{random}` transaction IDs |
| **Orders** | Atomic order creation (order + snapshot line items + 5% rewards + cart clear); history (newest first); status lifecycle Confirmed → Preparing → OutForDelivery → Delivered, plus Cancelled (from Confirmed only); status timeline UI; mock delivery partner, route, location polling, ETA |
| **Profile** | View/edit name & email; account stats (join date, total non-cancelled orders, rewards); address book CRUD + transactional set-default; logout |
| **Data** | Local SQLite (`foodordering.db3`) with 6 tables, 9 indexes, FK pragma, seed-on-empty, image-extension normalization migration (`.jpg/.jpeg → .png`) |
| **UI/UX** | Shell TabBar (Home/Cart/Orders/Profile) + modal routes; light/dark theming (`AppThemeBinding`); responsive layouts (`OnPlatform`, `OnIdiom`); empty states; error banners; success banners; confirmation alerts; loading indicators; re-entrancy-safe commands |

### 5.2 Out of Scope (v1.0)

- **Real backend services** — no actual payment gateway, no restaurant/POS integration, no real GPS/maps SDK, no push notifications. Mock implementations behind `IPaymentService`/`IMapService` stand in.
- **Multiple restaurants per order disambiguation** — cart may mix restaurants; no per-restaurant splitting or per-restaurant delivery fees.
- **Real-time order status from a server** — status advances via `SimulateStatusUpdateAsync` (mock), not SignalR/polling of a backend.
- **Delivery partner assignment persistence** — partner details are generated per view; `DeliveryPartnerId` column exists on `Order` but is not populated by the mock flow in v1.0.
- **Order item detail hydration on the order-detail screen** — v1.0 shows a placeholder line (`OrderDetailViewModel.LoadOrderItemsAsync`); the `OrderItems` table stores correct snapshots for future use.
- **Image upload / restaurant onboarding** — catalog is seed data only.
- **Automated test suite** — guided manual walkthrough testing (documented in Tasks.md phase 12).

### 5.3 Assumptions

1. **Single-user-device model.** One active session per install; the local DB stores multiple users but auth views are scoped to the securely-stored current user.
2. **Currency and locale.** All money is displayed as ₹ (INR, 2-decimal); GST is a flat 18%; delivery fee is a flat ₹50 — Indian market conventions. Phone validation (`[6-9]\d{9}`) and postal codes (6 digits) follow Indian formats.
3. **Seed catalog images** (`burger.png`, `pizza.png`, `sushi.png`, `noodles.png`, `bowl.png`, `bbq.png`, `butter_chicken.png`, `cake.png`) are bundled or resolvable as MAUI image resources.
4. **SecureStorage availability.** iOS Keychain / Android Keystore / Windows DPAPI(+) provide `SecureStorage`; on platforms where it throws, auth degrades gracefully (session cache empty → login route).
5. **Users have .NET 10 MAUI workloads** installed to build per-target; Windows runs unpackaged (`WindowsPackageType=None`) for development convenience.
6. **Spec-driven development context.** This proposal was produced with the OpenSpec workflow; requirements are traceable to `specs.md` capability specs and implemented via `tasks.md`.

---

*This proposal is the source of truth for intent. Technical detail: see `Design.md`. Requirements: see `Specs.md`. Implementation plan: see `Tasks.md`.*
