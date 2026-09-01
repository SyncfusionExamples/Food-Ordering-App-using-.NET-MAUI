# Design: CraveDash — Cross-Platform Food Ordering Application

- **Change ID:** `build-food-ordering-app`
- **Status:** Approved
- **Related:** `Proposal.md` (intent), `Specs.md` (requirements), `Tasks.md` (implementation plan)

---

## 1. System Architecture

### 1.1 Architectural Style

**MVVM (Model–View–ViewModel)** over a **strictly layered, dependency-injected** service architecture, hosted on **.NET MAUI Shell navigation**. The solution contains a single multi-target project, `FoodOrderingApp` (`FoodOrderingApp.slnx`), targeting `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, and `net10.0-windows10.0.19041.0`.

### 1.2 Layer Diagram

```
┌───────────────────────────────────────────────────────────────────────┐
│  PRESENTATION LAYER                                                    │
│  Views (XAML ContentPages + code-behind)                               │
│  LoginPage, SignUpPage, HomePage, ItemDetailPopup, CartPage,           │
│  CheckoutPopup, OrdersPage, OrderDetailPage, ProfilePage,             │
│  AddressFormPopup  +  Converters (4 IValueConverter classes)           │
├───────────────────────────────────────────────────────────────────────┤
│  VIEWMODEL LAYER (9 VMs — INotifyPropertyChanged + ICommand)           │
│  LoginVM, SignUpVM, HomeVM, ItemDetailVM, CartVM, CheckoutVM,          │
│  OrdersVM, OrderDetailVM, ProfileVM                                    │
│  (bindable wrapper models: CartItemViewModel, OrderViewModel,          │
│   PaymentMethodOption, TimelineItem, OrderItemDetail, AddressItem)     │
├───────────────────────────────────────────────────────────────────────┤
│  SERVICE LAYER (interface + implementation, all singletons)            │
│  IAuthService→AuthService        ICartService→CartService              │
│  IOrderService→OrderService       IPaymentService→PaymentService       │
│  IMapService→MapService           IValidationService→ValidationService│
├───────────────────────────────────────────────────────────────────────┤
│  DATA LAYER                                                            │
│  IDatabaseService → DatabaseService (SQLiteAsyncConnection)             │
│  Models (6 SQLite-annotated POCOs): User, Item, CartItem, Order,        │
│  OrderItem, Address                                                    │
├───────────────────────────────────────────────────────────────────────┤
│  PLATFORM / INFRASTRUCTURE                                             │
│  Microsoft.Maui Shell, SecureStorage, FileSystem.AppDataDirectory,     │
│  MainThread, BCrypt.Net-Core, sqlite-net-pcl + SQLitePCLRaw bundle_green│
└───────────────────────────────────────────────────────────────────────┘
```

**Key rules:**
- Views never touch the database or services directly; ViewModels orchestrate services only.
- Code-behind is restricted to view wiring: constructor DI of the ViewModel, `BindingContext` assignment, `OnAppearing` → `InitializeAsync()`, and pure-presentation handlers (e.g., payment method checkbox exclusivity in `CheckoutPopup.xaml.cs`).
- Every service is consumed through its interface; all are registered singletons.

### 1.3 Composition Root & Dependency Injection

`MauiProgram.CreateMauiApp()` is the single composition root:

```csharp
builder.UseMauiApp<App>()
    .ConfigureFonts(...)          // OpenSans-Regular, OpenSans-SemiBold
    .ConfigureSyncfusionCore()    // Syncfusion.Maui.Core.Hosting
    .ConfigureServices();        // extension method below
```

`ConfigureServices` registers, all as **singletons**:
- 7 services: `IDatabaseService→DatabaseService`, `IAuthService→AuthService`, `ICartService→CartService`, `IOrderService→OrderService`, `IPaymentService→PaymentService`, `IMapService→MapService`, `IValidationService→ValidationService`
- 9 viewmodels (one per screen)
- 10 pages (constructor-injected viewmodels)

**Rationale — singletons:** the app is single-window and single-session; singleton pages keep Shell navigation state and cart/order lists warm across tab switches at trivial memory cost. The tradeoff (stale singleton page state) is handled in each page's `OnAppearing` through `InitializeAsync()` and explicit view-state resets in `OnAppearing` (see `CheckoutPopup.OnAppearing` resetting error/success/payment-flags).

### 1.4 Application Startup Flow

`App.xaml.cs` (constructor-injected with `IAuthService`) overrides `CreateWindow`:

1. Creates `AppShell`, wraps it in `Window`, and kicks off `InitializeAppAsync(shell)` fire-and-forget.
2. `InitializeAppAsync`:
   - Resolves `IDatabaseService` from `IPlatformApplication.Current.Services` and calls `InitializeAsync()` inside a **30-second `CancellationTokenSource`** guard.
   - Loads session cache via `IsSessionValidAsync()` → `IsSessionValid()`.
   - On the main thread, routes: logged-in → `shell.GoToAsync("//home")`, otherwise `//login`. Both navigation attempts have nested fallback-to-login error handling.
3. `DatabaseService.InitializeAsync()` (idempotent): creates the connection at `FileSystem.AppDataDirectory/foodordering.db3`, enables `PRAGMA foreign_keys = ON`, creates all 6 tables, creates 9 indexes, runs the `.jpg/.jpeg → .png` image normalization migration, then seeds the 8-item catalog **only when the Items table is empty**.

**Rationale for time-bounded init:** a hung first-run DB migration must not freeze the app on the default login route silently; the 30 s CTS + try/catch chain keeps startup robust while logging to Debug output.

---

## 2. Component Design

### 2.1 Views (Presentation)

| View | Type | Purpose |
|---|---|---|
| `LoginPage` | ContentPage (Shell root candidate) | Email/password sign-in; two-column branded layout on WinUI/MacCatalyst; SfTextInputLayout inputs with password visibility toggle; inline error banner; loading state |
| `SignUpPage` | ContentPage | Full name, email, password + confirm; client-side validation; navigates back to login on success |
| `HomePage` | Tab (Home) | Primary-color header with `SearchBar` + Veg-Only `SfSwitch`; `CollectionView` grid of item cards (span 2 / WinUI 4 / Mac 3); empty state; selection → `itemdetail?itemId=` |
| `ItemDetailPopup` | Modal (route `itemdetail`) | Item image hero, veg/rating/cuisine metadata, description, price card, quantity stepper (−/value/+), Add to Cart with success banner, close via overlay tap or ✕ |
| `CartPage` | Tab (Cart) | Line-item cards (image, name, restaurant, unit price, qty, line total, Remove); summary panel (Subtotal, 18% GST, ₹50 Delivery, Total); Proceed to Checkout; empty state with Continue Shopping |
| `CheckoutPopup` | Modal (route `checkout`) | Semi-transparent overlay + centered card; total display; 4 exclusive payment method cards (UPI default); T&C footnote; processing overlay with ActivityIndicator; success card (order + transaction IDs) then auto-navigate to orders after 3 s |
| `OrdersPage` | Tab (Orders) | Order history list (newest first) with date, amount, emoji status chips; empty state with "Start Ordering" |
| `OrderDetailPage` | Modal (route `orderdetail`) | Order summary card; status timeline (4 stages, current highlighted); delivery partner card (name, rating, phone, vehicle + Refresh); delivery details card (ETA, address placeholder); order items placeholder; Cancel (only when Confirmed, with confirmation dialog, → Orders) |
| `ProfilePage` | Tab (Profile) | Profile info card (editable in edit mode + Save/Discard); stats card (Joined, Total Orders, Rewards 🎁); address book (list + Add, per-address Edit/Delete/Set Default); Security card (Change Password); Logout |
| `AddressFormPopup` | Modal (route `addressform`) | Address form (Label, Street, City, State, Postal Code, IsDefault) used for both add and edit (`?id=` query); validation errors inline |

**Converters** (registered in `App.xaml` resources):
- `StringToBoolConverter` — non-empty string → `true` (error banner visibility).
- `InvertedBoolConverter` — boolean negation (disables buttons while loading, hides content while loading).
- `BoolToTextConverter` — status/text mapping.
- `CountToVisibilityConverter` — collection count ↔ visibility, with `ShowEmpty` parameter for empty states.

### 2.2 ViewModels (State & Commands)

All 9 ViewModels follow one identical pattern:

```csharp
public class XViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private bool SetProperty<T>(ref T storage, T value,
        [CallerMemberName] string propertyName = "") { ... }

    public XViewModel(deps...) {
        SomeCommand = new AsyncRelayCommand(SomeAsync);
    }
    public async Task InitializeAsync() { ... }   // invoked from OnAppearing
}
```

| ViewModel | Key State | Commands |
|---|---|---|
| `LoginViewModel` | Email, Password, ErrorMessage, IsLoading, IsPasswordVisible | `LoginCommand` (async), `NavigateToSignUpCommand`, `TogglePasswordVisibilityCommand` |
| `SignUpViewModel` | FullName, Email, Password, ConfirmPassword, ErrorMessage, IsLoading, visibility flags (8-char min password const) | `SignUpCommand` (async), `NavigateToLoginCommand`, 2 toggle commands |
| `HomeViewModel` | `Items` (full), `FilteredItems` (displayed), SearchQuery, ShowVegetarianOnly, IsLoading, CurrentUser | `LoadItemsCommand`, `ItemSelectedCommand` (→ `itemdetail?itemId=`), `LogoutCommand` |
| `ItemDetailViewModel` | `ItemId` `[QueryProperty]` → auto-load, Item, Quantity (clamped 1–99), SuccessMessage/ShowSuccessMessage, IsLoading | `IncrementQuantityCommand`, `DecrementQuantityCommand`, `AddToCartCommand`, `CloseCommand` |
| `CartViewModel` | `CartItems` (of `CartItemViewModel`), Subtotal, Tax (0.18), DeliveryFee (50), Total, IsLoading, IsCartEmpty | `LoadCartCommand`, `RemoveItemCommand`, `CheckoutCommand` (→ `checkout` with `total` query param), `ContinueShoppingCommand` |
| `CheckoutViewModel` | `TotalAmount` `[QueryProperty("total")]`, `SelectedPaymentMethod`, 4 `PaymentMethodOption`s, IsLoading, ErrorMessage/SuccessMessage/ShowSuccessMessage, PaymentProcessing | `ConfirmPaymentCommand` (async), `CancelCommand` |
| `OrdersViewModel` | `Orders` (of `OrderViewModel`), IsLoading, EmptyMessage | `LoadOrdersCommand`, `OrderSelectedCommand` (→ `orderdetail?id=`) |
| `OrderDetailViewModel` | `OrderId` `[QueryProperty("id")]` → auto-load, formatted display fields, status color, `Timeline`, `OrderItems`, `CanCancelOrder`, delivery partner fields | `CancelOrderCommand`, `BackCommand`, `RefreshLocationCommand` |
| `ProfileViewModel` | CurrentUser, edit-mode state, stats, `Addresses`, `CurrentAddress` + `AddressId` `[QueryProperty]`, password fields, messages, IsSaving/IsLoading/IsChangingPassword | `SaveProfileCommand`, `DiscardChangesCommand`, `ChangePasswordCommand`, `LogoutCommand`, `AddAddressCommand`, `EditAddressCommand`, `DeleteAddressCommand`, `SetDefaultAddressCommand`, `SaveAddressCommand`, `CancelCommand` |

**Bindable wrapper models** (all defined alongside their VMs, all POCOs with computed display properties):
- `CartItemViewModel` — composes `CartItem` + `Item` + parent `RemoveCommand`; exposes `DisplayName`, `RestaurantName`, `UnitPrice`, `Quantity`, `Total` (computed).
- `OrderViewModel` — adds `StatusDisplayName` (emoji), `StatusColor` (hex per status), `FormattedDate`, `FormattedAmount`.
- `PaymentMethodOption` — enum + `DisplayName` + `Description`.
- `TimelineItem` / `OrderItemDetail` / `AddressItem` — display shape for order timeline / line items / addresses respectively.

**Command infrastructure (`LoginViewModel.cs`, shared app-wide):**
- `RelayCommand` — wraps `Action`, always executable.
- `AsyncRelayCommand` — wraps `Func<Task>`; fires via `MainThread.BeginInvokeOnMainThread`; has an `_isExecuting` re-entrancy guard that suppresses double-taps and raises `CanExecuteChanged`; catches all exceptions with Debug logging. (A generic `AsyncRelayCommand<T>` variant is used where command parameters are needed, e.g., cart remove, order select.)
- **Rationale:** hand-rolled `ICommand` with a UI-thread dispatcher and re-entrancy guard gives deterministic double-tap protection for payment/ordering actions (avoiding duplicate orders) without pulling observers into `CommunityToolkit.Mvvm.Input` attributes for every VM (the Toolkit's `AsyncRelayCommand` is used in some VMs as well, and both patterns coexist).

### 2.3 Services (Business Logic)

#### `AuthService : IAuthService`
- **Login**: required-field checks → case-insensitive email lookup (`LOWER(Email) = LOWER(?)`) → BCrypt verify → persist session to `SecureStorage` (`session_userid`, `session_email`) and populate an in-memory session cache → `AuthResult`.
- **SignUp**: full-name required, email regex, password policy (≥8 chars via `ValidationService.ValidatePassword`) → duplicate-email check (case-insensitive) → BCrypt hash → insert.
- **ChangePassword** (2 overloads): by userId+old/new (verifies old hash, ≥8 char new) or by email+new.
- **ValidatePassword**, **UpdateProfile** (name/email + refresh cached session email if self).
- **Address management** (delegated into `AuthService`): get/add/update/delete; **SetDefault** loads all user addresses then, inside a DB transaction, sets `IsDefault = (AddressId == target)` for each — guaranteeing exactly one default.
- **Session**: `IsSessionValid` (sync, cached), `IsSessionValidAsync` (reads SecureStorage), `ClearSession` (SecureStorage.RemoveAll + cache reset), `LogoutAsync`, `GetCurrentUserId/Email` (sync cached), `GetCurrentUserIdAsync/EmailAsync/UserAsync` (lazy-load SecureStorage once via `_sessionCacheLoaded`).
- Returns `AuthResult { IsSuccessful, ErrorMessage, User }` — no exceptions cross the service boundary.

#### `CartService : ICartService`
- `AddToCartAsync(itemId, qty)` — **merge semantics**: existing `CartItem` for the (user,item) pair increments quantity; otherwise inserts new row. Both stamp `UpdatedAt`.
- `RemoveFromCartAsync(cartItemId)`, `UpdateQuantityAsync` (rejects qty < 1 or > 99), `ClearCartAsync` (deletes all rows for user).
- `GetCartItemsAsync` (per-user, `ORDER BY AddedAt DESC`), `CalculateTotalAsync` (joins item prices × qty in memory), `GetCartCountAsync` (sum of quantities).
- All operations resolve the current user via `IAuthService.GetCurrentUserIdAsync()` and return false/empty on auth loss or exceptions (Debug-logged).

#### `OrderService : IOrderService`
- `CreateOrderAsync(totalAmount, addressId?)` — the app's critical transaction:
  1. Resolve user; require non-empty cart (short-circuits otherwise).
  2. Inside `IDatabaseService.ExecuteTransactionAsync`:
     - Insert `Order` (Status = "Confirmed", EstimatedDelivery = now+45 min).
     - For each cart item, look up the `Item` and insert an `OrderItem` **snapshotting `UnitPrice` at order time**.
     - `CalculateRewardsAsync(total)` = total × **0.05** (simulated 50 ms latency) and add to `user.RewardsPoints`.
     - `ClearCartAsync()`.
  3. Return the created order; any failure → null (transaction rolls back, cart is preserved).
- `GetUserOrdersAsync` (per-user, newest first), `GetOrderByIdAsync` (**also verifies UserId** — prevents IDOR).
- `UpdateOrderStatusAsync` (sets `DeliveredAt` when status becomes "Delivered").
- `CancelOrderAsync` — only allowed from status `Confirmed`.
- `SimulateStatusUpdateAsync` — advances one step along Confirmed → Preparing → OutForDelivery → Delivered (no-op at terminal state).
- **Rationale — snapshot pricing:** `OrderItem.UnitPrice` is frozen at purchase time so catalog price changes never alter historical orders.

#### `PaymentService : IPaymentService` (mock)
- `ProcessPaymentAsync(amount, method)` — 2–3 s simulated latency, then **deliberate 15% failure** ("Payment failed. Please try again.") for realistic error-path testing; success returns `TXN_{unixMillis}_{5-digit random}`.
- `ValidatePaymentMethodAsync` — whitelist (UPI/NetBanking/CreditCard/DebitCard).
- `GenerateTransactionIdAsync` — timestamp+random format.
- **Rationale — mock with failure rate:** exercising the failure path is mandatory for a checkout UI; a 100%-success mock cannot validate error banners, so the design bakes failure in.

#### `MapService : IMapService` (mock)
- `GetDeliveryPartnerAsync(orderId)` — random Indian partner profile (names, `98XXXXXXXX` phones, state-coded vehicle numbers `DL/MH/KA/TN/GJ + district + letters + digits`), rating 3.5–5.0, 500–5000 deliveries; caches in `_activeDeliveries` for location tracking.
- `GetDeliveryRouteAsync(orderId)` — mock Mumbai restaurant + customer locations, Haversine distance, interpolated partner position.
- `GetLocationUpdateAsync(orderId)` — progress derived from `DateTime.UtcNow.Second % 60` (0–0.95 interpolation) with statuses "Heading to your location" / "Almost there!".
- `GetEstimatedDeliveryTimeAsync` — 10–25 minutes remaining.
- `Location.GetDistanceTo` — Haversine (earth radius 6371 km).
- **Rationale:** simulates the contracts of a real logistics API (partner, route, poll-updates, ETA) — a real implementation replaces the class behind `IMapService` with zero ViewModel change.

#### `ValidationService : IValidationService`
- `IsValidEmail` — regex `^[^@\s]+@[^@\s]+\.[^@\s]+$` (case-insensitive).
- `ValidatePassword(pwd, requireUppercase?, requireDigits?, requireSpecialChar?)` — min 8 chars + optional complexity; returns `(bool, error)`.
- `IsValidPhoneNumber` — strips non-digits, `^[6-9]\d{9}$` (Indian 10-digit).
- `IsValidPostalCode` — `^\d{6}$` (Indian 6-digit PIN).
- `ValidateRequired`, `ValidateLength(min, max)`, `ValidatePasswordMatch`, `IsValidUrl`, `IsValidLatitude/Longitude/Coordinate`.

#### `DatabaseService : IDatabaseService`
- Path: `FileSystem.AppDataDirectory/foodordering.db3`; lazy single `SQLiteAsyncConnection`.
- `InitializeAsync` — FK pragma, create 6 tables, 9 named indexes (`idx_users_email`, `idx_items_veg`, `idx_items_cuisine`, `idx_cartitems_userid`, `idx_cartitems_itemid`, `idx_orders_userid`, `idx_orders_status`, `idx_orderitems_orderid`, `idx_addresses_userid`), image normalization update statements, seed-if-empty (8 dishes).
- Generic CRUD facade: `GetByIdAsync<T>`, `GetAllAsync<T>`, `QueryAsync<T>(sql, params)`, `InsertAsync<T>`, `InsertAllAsync<T>`, `UpdateAsync<T>`, `DeleteAsync<T>(entity|id)`, `DeleteAllAsync<T>`.
- `ExecuteTransactionAsync(Func<Task>)` — wraps the action in `RunInTransactionAsync` (synchronously awaited inside the transaction delegate); returns success bool, logs failures.
- Helpers: `ClearUsersAsync`, `GetAllUsersAsync`.
- **Rationale — repository-facade over SQLite:** isolates persistence behind `IDatabaseService` so ViewModels/Services never touch `SQLiteAsyncConnection`, keeping data access testable and swap-friendly.

---

## 3. Data Models

Local SQLite database `foodordering.db3`, 6 tables, all via `sqlite-net-pcl` annotations. Navigation properties are marked `[Ignore]` (client-side hydration, no FK enforcement at ORM level).

### 3.1 Entity-Relationship Overview

```
Users (UserId PK) ──┬──< CartItems (UserId FK˟, ItemId FK˟) >── Items (ItemId PK)
                    ├──< Orders   (UserId FK˟)  ──< OrderItems (OrderId FK˟, ItemId FK˟) >── Items
                    └──< Addresses(UserId FK˟)
FK˟ = logical foreign key (indexed int column; relations enforced at query level + PRAGMA foreign_keys)
```

### 3.2 Table Schemas

**Users** (`[Table("Users")]`)
| Column | Type | Constraints |
|---|---|---|
| UserId | int | PK, AutoIncrement |
| Email | string | Unique, NotNull |
| FullName | string | NotNull |
| PasswordHash | string | NotNull |
| DOB | string? | — |
| JoinDate | DateTime | default UtcNow |
| RewardsPoints | int | default 0 |
| CreatedAt / UpdatedAt | DateTime | default UtcNow |

**Items**
| Column | Type | Constraints |
|---|---|---|
| ItemId | int | PK, AutoIncrement |
| RestaurantName | string | NotNull |
| ItemName | string | NotNull |
| Description | string | — |
| Price | decimal | NotNull |
| Image | string? | — |
| IsVeg | bool | default false (indexed) |
| Cuisine | string | (indexed) |
| Rating | double | — |
| CreatedAt | DateTime | — |

**CartItems** — CartItemId PK; `UserId` NotNull+Indexed; `ItemId` NotNull+Indexed; `Quantity` NotNull (default 1); `AddedAt`/`UpdatedAt` (default UtcNow); `[Ignore] Item?` navigation.

**Orders** — OrderId PK; `UserId` NotNull+Indexed; `OrderDate` (default UtcNow); `TotalAmount` NotNull decimal; `Status` NotNull (default "Confirmed"; domain: Confirmed, Preparing, OutForDelivery, Delivered, Cancelled); `EstimatedDelivery`/`DeliveredAt` nullable DateTime; `DeliveryPartnerId` nullable int (reserved); audit timestamps; `[Ignore] List<OrderItem>? Items`.

**OrderItems** — OrderItemId PK; `OrderId` NotNull+Indexed; `ItemId` NotNull+Indexed; `Quantity` NotNull; `UnitPrice` NotNull decimal (**price snapshot**); audit timestamps; `[Ignore] Item?`.

**Addresses** — AddressId PK; `AddressLine1`/`AddressLine2`/`PostalCode`; `UserId` NotNull+Indexed; `Street` NotNull; `City` NotNull; `State?`/`ZipCode?`; `Label` default "Home" (Home/Work/Other); `IsDefault` bool default false; audit timestamps.

### 3.3 Seed Data (8 dishes, inserted when Items is empty)

| Restaurant | Dish | Price | Veg | Cuisine | Rating |
|---|---|---|---|---|---|
| The Burger Loft | Classic Burger | 299.50 | No | American | 4.8 |
| Pizzeria Artisan | Margherita Pizza | 399.99 | Yes | Italian | 4.6 |
| Sakura Sushi Bar | Salmon Poke Bowl | 549.25 | No | Japanese | 4.9 |
| Noodle Theory | Pad Thai | 449.50 | Yes | Thai | 4.5 |
| Green Garden | Buddha Bowl | 349.99 | Yes | Healthy | 4.7 |
| Smoke & Fire BBQ | Pulled Pork Sandwich | 399.50 | No | American | 4.4 |
| Indus Spice | Butter Chicken | 449.99 | No | Indian | 4.6 |
| Sugar Rush | Chocolate Lava Cake | 249.00 | Yes | Dessert | 4.9 |

### 3.4 Data Access Patterns

- **Case-insensitive uniqueness** for email via `LOWER(Email) = LOWER(?)` queries plus the DB `Unique` constraint.
- **User scoping** for all per-user queries (cart, orders, addresses) resolved through `IAuthService.GetCurrentUserIdAsync()`.
- **Transactions** for multi-write operations: order creation; set-default address.
- **Snapshot immutability** for historical prices in `OrderItems.UnitPrice`.
- **Timestamps** everywhere (`CreatedAt`/`UpdatedAt`) withUtcNow defaults.

---

## 4. Application Flow

### 4.1 Navigation Architecture — Shell

`AppShell.xaml` defines global routes; `AppShell.xaml.cs` registers modal routes via `Routing.RegisterRoute`:

| Route | Page | Registration | Presentation |
|---|---|---|---|
| `login` | LoginPage | ShellContent (root) | Push |
| `signup` | SignUpPage | ShellContent | Push |
| `home` / `cart` / `orders` / `profile` | TabBar ShellContents | TabBar | Tabs |
| `itemdetail` | ItemDetailPopup | `Routing.RegisterRoute` | `ModalAnimated` |
| `checkout` | CheckoutPopup | `Routing.RegisterRoute` | `ModalAnimated` |
| `orderdetail` | OrderDetailPage | `Routing.RegisterRoute` | `ModalAnimated` |
| `addressform` | AddressFormPopup | `Routing.RegisterRoute` | `ModalAnimated` |

**Startup routing** (App.xaml.cs): session valid → `//home`; else `//login` (both under try/catch with login fallback).
**Global route resets** use the `//route` prefix (clears the stack — e.g., `//home` after login, `//login` after logout, `//orders` after cancel).
**Data passing:** `[QueryProperty]` — `itemdetail?itemId=`, `checkout?total=₹…` (invariant-culture "F2"), `orderdetail?id=`, `addressform?id=` (edit) or bare (add).
**Rationale:** Shell gives declarative tabs + URI-driven modal registration in one place; `//` route resets avoid layering modals across logout/login.

### 4.2 Primary User Flow — Order a Meal

```
Launch ──▶ (session?) ──▶ //login or //home
   │
   ├─ Sign Up ──▶ validations ──▶ BCrypt hash ──▶ insert ──▶ back to //login
   │
   └─ Log In ──▶ verify hash ──▶ SecureStorage session ──▶ //home
                        │
        ┌───────────────┘
        ▼
   //home  [search | veg-only toggle] ──▶ filtered cards
        │  tap card
        ▼
   itemdetail?itemId=  ──▶ stepper 1–99 ──▶ Add to Cart ──▶ banner ──▶ ✕/overlay close
        │
        ▼
   //cart ──▶ [Subtotal + 18% GST + ₹50] = Total ──▶ Proceed to Checkout
        │
        ▼
   checkout?total=… ──▶ pick method (UPI default) ──▶ Confirm Payment
        │      ├─ processing overlay (2–3 s simulated)
        │      ├─ ~15% failure ──▶ error banner ──▶ retry
        │      └─ success ──▶ TXN id
        ▼                (atomic: Order + OrderItems + 5% rewards + cart clear)
   order created (Status=Confirmed, ETA +45 min) ──▶ success card 3 s ──▶ //orders
        │  tap order
        ▼
   orderdetail?id= ──▶ summary / timeline / partner card / ETA / [Cancel?]
        │  Cancel (Confirmed only) ──▶ confirm dialog ──▶ Status=Cancelled ──▶ //orders
        ▼
   //orders / //profile (stats, addresses, password change, logout ──▶ confirm ──▶ //login)
```

### 4.3 State Management

- **Session state** — `AuthService` in-memory cache (`_cachedUserIdStr`, `_cachedEmail`, `_sessionCacheLoaded`) lazily hydrated from `SecureStorage`; survives page lifecycles in the singleton.
- **Cart & list state** — `ObservableCollection<T>` in singleton VMs (`CartItems`, `FilteredItems`, `Orders`, `Addresses`, `Timeline`, `OrderItems`); `CollectionChanged` is re-broadcast as `PropertyChanged` where UI needs it (OrdersVM).
- **Computed display state** — recalculated on set: `CartViewModel.Subtotal → CalculateTotal()`, `HomeViewModel.SearchQuery/ShowVegetarianOnly → ApplyFilters()`, `ItemDetailViewModel.ItemId → LoadItemAsync()`.
- **Transient view state** — `IsLoading`, `PaymentProcessing`, `IsSaving`, `ShowSuccessMessage` bound directly to XAML (banners auto-dismiss after 1–3 s via `Task.Delay`).
- **Persistence** — SQLite is the source of truth across restarts; SecureStorage owns identity across restarts.

---

## 5. UI/UX Considerations

### 5.1 Theme & Design System (App.xaml resources)

- **Palette:** Primary `#FF6B35` (food-appetite orange), Secondary `#004E89`, Tertiary `#F7B801`; text `#1E1E1E`/`#FFFFFF`; surfaces `#FFFFFF`/`#1E1E1E`; error `#DC2626`, success `#16A34A`, warning `#EA580C`; card borders `#E0E0E0`/`#333333`.
- **Dark mode everywhere** via `AppThemeBinding Light=…, Dark=…` on every background/text/border (including `AppShell.BackgroundColor`).
- **Fonts:** OpenSans Regular + SemiBold, registered in `MauiProgram`.
- **Typography scale resource keys:** FontSizeSmall(12)/Normal(14)/Large(18)/XLarge(24)/Title(28); named styles (`PrimaryButtonStyle`, `EntryStyle`, `PageStyle`, …); shared `Styles.xaml` global styles for Button (44-min touch target), CheckBox, Entry, etc.

### 5.2 Adaptive & Responsive Layout

- **Login page:** two-column Grid — branding panel visible only `{OnPlatform WinUI=true, MacCatalyst=true}`; form auto-moves to column 1 on desktop, full-width on phone.
- **Home grid:** `GridItemsLayout Span="{OnPlatform Default=2, WinUI=4, MacCatalyst=3}"`.
- **Modals:** fixed overlay pattern (see 5.3) with `WidthRequest="{OnPlatform Default=340–320, WinUI=500–700, MacCatalyst=600–700}"` and `HeightRequest="{OnIdiom Phone=500, Tablet=700, Desktop=700}"`.
- **Profile:** two-column card grid that naturally stacks on narrow windows via wrapping `Grid ColumnDefinitions="*,*"` within ScrollViewer.

### 5.3 Modal Pattern (consistent across ItemDetail / Checkout / OrderDetail / AddressForm)

1. Full-screen `Grid` with semi-transparent black `BoxView` overlay (Opacity 0.5) — tap-to-dismiss bound to `CancelCommand`/`CloseCommand`.
2. Centered `Border` card: `StrokeShape="RoundRectangle 16"`, drop shadow, page-background fill, fixed/adaptive size.
3. `Shell.PresentationMode="ModalAnimated"` on the page; scrollable `VerticalStackLayout` content.
**Rationale:** Shell modal pages have transparent backgrounds, so the app implements its own overlay (dim + tap-outside-to-cancel + shadowed card) to get a popup look with native Shell routing and swipe-safe dismissal.

### 5.4 Feedback & Interaction Patterns

- **Error banners:** red-tinted Border (`#FF6B6B` stroke, `#FFE0E0` fill, `#CC0000` text) bound via `StringToBoolConverter` — inline, never modal for recoverable input errors.
- **Success banners:** green (`#16A34A`/`#E8F5E9`) with auto-dismiss (Task.Delay) — "Added to cart", "Profile updated", "Address added", "Payment successful".
- **Destructive confirmations:** `DisplayAlert` modals for Logout, Delete Address, Cancel Order — explicit Yes/No.
- **Loading:** `ActivityIndicator` bound to `IsLoading` (overlay in modals; inline on tabs); all submit buttons disabled during async work via `InvertedBoolConverter` + `AsyncRelayCommand` re-entrancy guard.
- **Payment processing overlay:** `PaymentProcessing` shows semi-opaque full-card overlay "Processing Payment…" with ActivityIndicator; Confirm/Cancel buttons hidden or disabled during processing.
- **Empty states:** icon + title + helper + CTA on Home ("😔 No items found — Try adjusting your search or filters"), Cart ("🛒 Your cart is empty — Continue Shopping"), Orders ("📦 No orders yet. Start ordering! — Start Ordering").
- **Status visualization:** emoji chips per status (✓ Confirmed 🍳 Preparing 🚗 Out for Delivery ✅ Delivered) with per-status colors (`#FF6B35`, `#F7B801`, `#004E89`, `#16A34A`); order timeline with icon bubbles, colored connector lines (green completed / `#E0E0E0` pending), orange current.
- **Micro-affordances:** SfSwitch with custom On/Off `SwitchSettings` (green vs orange-red thumb/track); password visibility toggles on all password inputs (`EnablePasswordVisibilityToggle`); quantity stepper with −/+ buttons and boxed value.

### 5.5 Syncfusion MAUI Controls Utilized

| Control | Namespace/Package | Used For |
|---|---|---|
| `SfTextInputLayout` | Syncfusion.Maui.Core | Login/SignUp/Profile/Address form inputs — floating hints, filled/outlined containers, password toggle |
| `SfSwitch` | Syncfusion.Maui.Buttons | Veg-Only filter on Home header |
| `SfTabView` | Syncfusion.Maui.TabView | (Referenced tab-view capability for sectioned layouts) |
| Core/Charts/DataGrid packages | Syncfusion.Maui.* | Referenced for extended UI/chart/data-grid capabilities |
| `ConfigureSyncfusionCore()` | Syncfusion.Maui.Core.Hosting | Mandatory MAUI startup initialization |

---

## 6. Technical Decisions and Rationale

| # | Decision | Rationale / Tradeoff |
|---|---|---|
| D1 | **.NET MAUI multi-target single project** | One codebase → Android/iOS/Mac Catalyst/Windows; net10.0 with XAML source-gen inflator (`<MauiXamlInflator>SourceGen</MauiXamlInflator>`) for faster builds/runtime perf. Tradeoff: platform build requires respective workloads. |
| D2 | **MVVM with hand-rolled command layer + CommunityToolkit.Mvvm** | ViewModels own an identical `INotifyPropertyChanged`+`SetProperty` pattern (explicit, educational, zero magic); `AsyncRelayCommand` with main-thread dispatch + re-entrancy guard prevents double-tap duplicate orders — the highest-risk race in a commerce app. Toolkit `AsyncRelayCommand` (`CommunityToolkit.Mvvm.Input`) is used where it simplifies parameterized commands. |
| D3 | **SQLite via `sqlite-net-pcl` behind `IDatabaseService` facade** | Zero-config offline persistence on all platforms; generic repository facade keeps data access swappable and mockable; tradeoff: no LINQ navigation — `[Ignore]` navigation properties are hydrated explicitly by services. |
| D4 | **BCrypt.Net-Core for password hashing** | Adaptive, salted, industry-standard; never store plaintext. Cost is negligible for single-user auth flows. |
| D5 | **SecureStorage for session persistence** | OS-backed (Keychain/Keystore/DPAPI) storage for `session_userid`/`session_email` — sessions survive restarts without a token infra. Tradeoff: not available on all emulators — code degrades gracefully to login. |
| D6 | **Shell navigation with URI routes + QueryProperty data passing** | Declarative TabBar + registered modal routes; `//route` resets guarantee a clean stack across auth transitions; strings-only query params are Shell-native (tradeoff: totals passed as invariant "F2" strings; IDs as ints). |
| D7 | **Mock Payment/Map services with realistic behaviors** | The app must demo/fail-test payment errors and delivery tracking standalone; mocks mirror real API shapes (`PaymentResult`, `DeliveryPartner`, `LocationUpdate`), with intentional latency (2–3 s payment), 15% failure rate, Haversine distance, and status interpolation — swap to real backends without touching VMs. |
| D8 | **Transaction-wrapped order creation** | Order + snapshot line items + rewards update + cart clear must be atomic; `RunInTransactionAsync` rolls back on any failure, preserving the cart for retry. |
| D9 | **Price snapshots in OrderItems + per-order statuses on the Order row** | Historical integrity in the face of mutable catalog prices; status string domain (Confirmed/Preparing/OutForDelivery/Delivered/Cancelled) is simple, indexed, and renderable. |
| D10 | **App-level resource dictionary theming (AppThemeBinding)** | One design token set drives every screen; light/dark support is a binding convention, not per-page code. |
| D11 | **Modal-as-page overlay pattern** | Uniform "popup card" UX (dim overlay, tap-outside dismiss, shadowed card) reusing Shell's `ModalAnimated` presentation — works on all idioms without platform popup APIs. |
| D12 | **Singleton DI for pages/VMs/services** | Simple lifetime model matching the single-window app; tab data stays warm; stale-state risk is mitigated with `OnAppearing → InitializeAsync()` + explicit resets (CheckoutPopup). Tradeoff: acceptable for the sample; a multi-window app would move pages transient. |
| D13 | **Startup DB init with 30 s CTS + error-tolerant session routing** | First run must never hard-deadlock the UI; timeouts and fallbacks log to Debug and route to login rather than crash. |
| D14 | **Indian-market business constants** | ₹ currency, 18% GST, ₹50 flat delivery, 5% rewards, 6-digit PINs, 10-digit `[6-9]` phones, Mumbai mock coordinates — localizes the sample's commerce math; constants are isolated in VM/service fields for easy retargeting. |
| D15 | **Syncfusion Core startup + control set** | `ConfigureSyncfusionCore()` is required for Syncfusion MAUI inputs/buttons/etc.; selecting `SfTextInputLayout`/`SfSwitch` demonstrates production form/filter UI with built-in floating labels, assistive labels, and password toggles. |
