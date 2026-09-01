# Specs: CraveDash — Cross-Platform Food Ordering Application

- **Change ID:** `build-food-ordering-app`
- **Status:** Approved
- **Related:** `Proposal.md`, `Design.md`, `Tasks.md`
- **Spec format:** Adherence: Strict

---

## 1. Capabilities (Functional Requirements)

### REQ-1: Database & Persistence

| ID | Requirement |
|---|---|
| REQ-1.1 | The app shall persist data in a local SQLite database `foodordering.db3` under `FileSystem.AppDataDirectory` via a single lazily-created `SQLiteAsyncConnection`. |
| REQ-1.2 | On first initialization, the app shall enable `PRAGMA foreign_keys = ON` and create tables `Users`, `Items`, `CartItem`, `OrderItems`, `Orders`, `Addresses` (`CREATE TABLE IF NOT EXISTS` semantics via ORM). |
| REQ-1.3 | The app shall create indexes: `idx_users_email(Email)`, `idx_items_veg(IsVeg)`, `idx_items_cuisine(Cuisine)`, `idx_cartitems_userid(UserId)`, `idx_cartitems_itemid(ItemId)`, `idx_orders_userid(UserId)`, `idx_orders_status(Status)`, `idx_orderitems_orderid(OrderId)`, `idx_addresses_userid(UserId)`. |
| REQ-1.4 | A data-normalization migration shall rewrite item image references ending in `.jpg` or `.jpeg` to `.png`. |
| REQ-1.5 | When the `Items` table is empty, the app shall seed exactly 8 menu items across 8 restaurants covering American, Italian, Japanese, Thai, Healthy, Indian, and Dessert cuisines (per Design §3.3). |
| REQ-1.6 | The data layer shall expose generic async operations — `GetByIdAsync<T>`, `GetAllAsync<T>`, `QueryAsync<T>`, `InsertAsync<T>`, `InsertAllAsync<T>`, `UpdateAsync<T>`, `DeleteAsync<T>` (entity and by id), `DeleteAllAsync<T>` — behind `IDatabaseService`. |
| REQ-1.7 | The data layer shall expose `ExecuteTransactionAsync(Func<Task>)` wrapping multi-write operations in `RunInTransactionAsync`; on exception it shall log and return `false` (operations roll back). |
| REQ-1.8 | Initialization shall be idempotent (no-op when already initialized). |

### REQ-2: Authentication & Session

| ID | Requirement |
|---|---|
| REQ-2.1 | **Sign-Up** shall accept Full Name, Email, Password, Confirm Password; enforce: all fields non-empty, password ≥ 8 characters, password == confirm password. |
| REQ-2.2 | Sign-Up shall reject an already-registered email with "Email already registered", using **case-insensitive** duplicate detection (`LOWER(Email) = LOWER(?)`) plus the DB `Unique` constraint. |
| REQ-2.3 | Passwords shall be stored **only** as BCrypt hashes (`BCrypt.Net.BCrypt.HashPassword`); never plaintext. |
| REQ-2.4 | **Login** shall require email + password; look up the user case-insensitively; verify with `BCrypt.Verify`; return "Invalid email or password" for either wrong email or wrong password (no user-enumeration hinting). |
| REQ-2.5 | On successful login, the app shall store `session_userid` and `session_email` in `SecureStorage` and mirror them into an in-memory session cache. |
| REQ-2.6 | **Session validity** shall be determined by presence of both session keys (`IsSessionValid` / `IsSessionValidAsync`); the async version must tolerate SecureStorage exceptions (returns false). |
| REQ-2.7 | On app launch, the app shall navigate to `//home` when a valid session exists, otherwise `//login`; DB initialization is guarded by a 30-second cancellation token with login-fallback on any routing error. |
| REQ-2.8 | **Logout** shall clear all SecureStorage entries and reset the session cache; the Profile logout flow must confirm with a Yes/No dialog before clearing. |
| REQ-2.9 | **Change Password** shall validate: current password present, new password ≥ 8 characters, new == confirm; verify the current password hash before re-hashing the new one; report "Current password is incorrect" on mismatch. |
| REQ-2.10 | Failed auth operations shall return `AuthResult { IsSuccessful=false, ErrorMessage }` — exceptions shall not escape the service. |
| REQ-2.11 | **Profile update** (name/email) shall update `UpdatedAt` and refresh the cached session email when editing the logged-in user. |

### REQ-3: Home / Catalog Browsing

| ID | Requirement |
|---|---|
| REQ-3.1 | Home shall display all items as an adaptive card grid (`Span`: 2 default, 4 WinUI, 3 Mac Catalyst) with image, restaurant, dish name, rating (1-decimal), price (`₹{0:F0}`), cuisine, and a "🌱 Vegetarian" badge when `IsVeg`. |
| REQ-3.2 | The search box shall live-filter items where the query matches (case-insensitive substring) dish name, restaurant name, or cuisine. |
| REQ-3.3 | The Veg-Only switch shall restrict displayed items to `IsVeg == true`; it shall apply in combination with an active search query. |
| REQ-3.4 | When (filtered) results are empty, Home shall show the empty-state view (icon, "No items found", helper text). |
| REQ-3.5 | Selecting an item card shall navigate to `itemdetail?itemId={id}`. |
| REQ-3.6 | Data (re)load occurs on every `OnAppearing` via `InitializeAsync`; loading state drives a centered `ActivityIndicator`. |

### REQ-4: Item Detail & Add to Cart

| ID | Requirement |
|---|---|
| REQ-4.1 | The item detail modal shall load the item by `itemId` `[QueryProperty]` and display image, restaurant, name, veg badge, rating, cuisine, description, price per item (`₹{0:F2}`). |
| REQ-4.2 | Quantity shall be constrained to **1–99**; "+" increments to max 99, "−" decrements to min 1. |
| REQ-4.3 | Add to Cart shall call `ICartService.AddToCartAsync(itemId, quantity)` and, on success, show a green success banner ("Added {n} {dish}(s) to cart!") that auto-hides after 1 second. |
| REQ-4.4 | The modal shall be dismissible via ✕ button or tapping the dimmed overlay; dismissal navigates back one route. |
| REQ-4.5 | Auth-loss or service failure shall fail silently with Debug logging — no crash. |

### REQ-5: Cart Management

| ID | Requirement |
|---|---|
| REQ-5.1 | Adding an existing (user,item) pair shall **merge**: increment `Quantity` by the added amount (update `UpdatedAt`) — not create a duplicate row. |
| REQ-5.2 | `UpdateQuantityAsync` shall reject quantities < 1 or > 99. |
| REQ-5.3 | The cart page shall list items (newest-added first) with image, name, restaurant, unit price, quantity, line total, and a Remove action that deletes the row and refreshes totals. |
| REQ-5.4 | The summary panel shall display: **Subtotal** (`Σ price × qty`), **Tax = Subtotal × 0.18 (18% GST)**, **Delivery Fee = ₹50**, **Total = Subtotal + Tax + Delivery Fee**; all formatted `₹{0:F2}`; totals recalc on every item add/remove. |
| REQ-5.5 | With an empty cart, the page shall show the empty state (🛒, "Your cart is empty", "Continue Shopping" → `//home`) and hide the summary panel. |
| REQ-5.6 | "Proceed to Checkout" shall be enabled only with a non-empty cart and shall navigate to `checkout` passing `total` as an invariant-culture "F2" string. |
| REQ-5.7 | All cart queries shall be scoped to the current logged-in user's `UserId`. |

### REQ-6: Checkout & Payment

| ID | Requirement |
|---|---|
| REQ-6.1 | The checkout modal shall display the payable total (₹, 2-dp) received via the `total` `[QueryProperty]`. |
| REQ-6.2 | The modal shall offer exactly 4 payment methods — UPI (default-selected), Net Banking, Credit Card, Debit Card — as mutually exclusive options. |
| REQ-6.3 | On open (and on cancel), the modal shall reset error/success messages, `PaymentProcessing`, `IsLoading`, and re-select UPI. |
| REQ-6.4 | Confirm Payment shall first validate the method (`ValidatePaymentMethodAsync`) and reject totals ≤ 0 with "Invalid amount". |
| REQ-6.5 | Payment processing shall show a blocking overlay ("Processing Payment…" + `ActivityIndicator`); the Confirm button shall be disabled and non-reentrant during processing. |
| REQ-6.6 | The simulated gateway shall take 2–3.5 s; **fail ~15% of attempts** with "Payment failed. Please try again."; successes shall return `TransactionId` formatted `TXN_{unixMillis}_{random5}`. |
| REQ-6.7 | On payment failure, order creation shall NOT occur; the error banner shall remain and the user may retry or cancel. |
| REQ-6.8 | On success, the app shall show a success card containing Order ID and Transaction ID, then auto-navigate to `//orders` after 3 seconds. |
| REQ-6.9 | Tapping the dim overlay or ✕ or "Cancel" shall dismiss checkout without side effects (`GoToAsync("..")`). |

### REQ-7: Order Lifecycle

| ID | Requirement |
|---|---|
| REQ-7.1 | Order creation shall be **atomic within one DB transaction**: insert `Order` (Status "Confirmed", `EstimatedDelivery = OrderDate + 45 min`), insert `OrderItems` **snapshotting current `Item.Price` as `UnitPrice`**, credit rewards = `⌊total × 0.05⌋` points to the user, and clear the cart. |
| REQ-7.2 | Order creation shall short-circuit (return null, no transaction) when the cart is empty or the user session is missing; transaction failure shall leave the cart intact. |
| REQ-7.3 | Order status domain: `Confirmed → Preparing → OutForDelivery → Delivered`, plus `Cancelled` reachable **only from `Confirmed`**. |
| REQ-7.4 | `UpdateOrderStatusAsync` shall stamp `DeliveredAt = UtcNow` when transitioning to "Delivered". |
| REQ-7.5 | `SimulateStatusUpdateAsync` shall advance one stage per call along the progression and be a no-op at `Delivered`. |
| REQ-7.6 | Order reads (`GetUserOrdersAsync`, `GetOrderByIdAsync`) shall be scoped to the current user's `UserId`; history ordered `OrderDate DESC`. |
| REQ-7.7 | Orders list shall render per order: "Order #{id}", formatted date (`MMM dd, yyyy 'at' HH:mm`), amount (`₹{0:F2}`), and an emoji status chip in its status color (Confirmed `#FF6B35`, Preparing `#F7B801`, OutForDelivery `#004E89`, Delivered `#16A34A`, other `#999999`). |
| REQ-7.8 | Selecting an order shall navigate to `orderdetail?id={orderId}`. |

### REQ-8: Order Tracking Detail

| ID | Requirement |
|---|---|
| REQ-8.1 | The order detail modal shall show "Order #{id}", date (`dddd, MMMM dd, yyyy 'at' HH:mm`), amount, and colored status display name. |
| REQ-8.2 | The status timeline shall always show the 4 stages (Confirmed 📋, Preparing 👨‍🍳, Out for Delivery 🚗, Delivered 📦) with per-stage completed/current state: completed stages green, the current stage orange, pending stages gray; connector line green when completed else `#E0E0E0`. |
| REQ-8.3 | Estimated delivery shall be computed from status — Confirmed: `OrderDate + 30 min`; Preparing: `+20 min`; OutForDelivery: `+10 min`; Delivered: "Delivered" — displayed as "Expected by {HH:mm}". |
| REQ-8.4 | A delivery-partner card shall be visible once status ∉ {Confirmed, Cancelled}, populated from `IMapService.GetDeliveryPartnerAsync` (name, rating e.g. `⭐ 4.2/5.0 (n deliveries)`, phone `📞 …`, vehicle `🚗 type (number)`). |
| REQ-8.5 | A **Refresh** button shall re-poll `IMapService.GetLocationUpdateAsync` for the partner's current coordinates. |
| REQ-8.6 | A **Cancel Order** button shall be visible only while `Status == "Confirmed"`; it must confirm via Yes/No dialog, set status "Cancelled", alert success, and return to `//orders`. |
| REQ-8.7 | Load failures shall surface a "Failed to load order" alert rather than crash. |
| REQ-8.8 | The modal shall be dismissible via ‹ Back to Orders or dim-overlay tap. |

### REQ-9: Profile & Account

| ID | Requirement |
|---|---|
| REQ-9.1 | Profile shall display Full Name, Email, Joined date (`MMMM dd, yyyy`), **Total Orders** = count of non-Cancelled orders, and **Rewards Points** (🎁 display). |
| REQ-9.2 | Profile edit mode shall enable name/email entry with **Save Changes** and **Discard**; Discard restores current values; Save requires non-empty name and email and persists with `UpdatedAt` refresh, showing an auto-hiding (3 s) success banner. |
| REQ-9.3 | Change Password shall enforce REQ-2.9 rules, verify current password server-side, hash-rotate, and show a 3-s success banner; on success the three password fields are cleared. |
| REQ-9.4 | Logout (REQ-2.8) shall be on the Profile page and navigate to `//login` after confirmation. |

### REQ-10: Address Book

| ID | Requirement |
|---|---|
| REQ-10.1 | Profile shall list the user's addresses (default first, then newest first) with label (Home/Work/Other — default label "Home") and summarized line (`AddressLine1, City`). |
| REQ-10.2 | Add Address shall open the `addressform` modal empty; Edit Address shall open it with `?id=` and pre-fill the record. |
| REQ-10.3 | The address form shall require Street (AddressLine1), City, State, and Postal Code; label is free text; errors display in the inline error banner. |
| REQ-10.4 | Save shall insert (add) or update (edit) and reload the address list, showing a 2-s success banner, then return to profile; Cancel clears form state and returns. |
| REQ-10.5 | Delete Address shall confirm via Yes/No dialog, delete the row, remove it from the UI, and toast success. |
| REQ-10.6 | **Set Default** shall, inside one DB transaction, set `IsDefault = true` for the chosen address and `false` for every other address of the user (exactly one default), then reload the list. |
| REQ-10.7 | The default address row shall render its label with "(Default)" and hide its Set Default button. |

### REQ-11: Value Converters

| ID | Requirement |
|---|---|
| REQ-11.1 | `StringToBoolConverter` — returns `true` for non-empty/non-whitespace strings. |
| REQ-11.2 | `InvertedBoolConverter` — logical negation. |
| REQ-11.3 | `BoolToTextConverter` — maps booleans to configured text values. |
| REQ-11.4 | `CountToVisibilityConverter` — default: visible when collection count > 0; with parameter `ShowEmpty`: visible when count == 0; non-collection/non-int input handled safely. |
| REQ-11.5 | All four shall be registered in `App.xaml` application resources. |

### REQ-12: Startup & Navigation Wiring

| ID | Requirement |
|---|---|
| REQ-12.1 | `App.CreateWindow` shall construct `AppShell` and start async initialization without blocking window creation. |
| REQ-12.2 | `AppShell` shall define ShellContents for `login`, `signup`; a TabBar of `home`, `cart`, `orders`, `profile`; and global ShellContents for `itemdetail`, `checkout`, `orderdetail`, `addressform`. |
| REQ-12.3 | `AppShell` constructor shall register routes `itemdetail`, `checkout`, `orderdetail`, `addressform` via `Routing.RegisterRoute`. |
| REQ-12.4 | Every page shall receive its ViewModel via constructor DI and set `BindingContext`; every page (except login/signup) shall call `viewModel.InitializeAsync()` from `OnAppearing`. |
| REQ-12.5 | All auth-transition navigations shall use `//`-prefixed global routes to reset the stack. |
| REQ-12.6 | DI registrations shall be singletons for all services, ViewModels, and Pages (MauiProgram.ConfigureServices). |

---

## 2. Non-Functional Requirements

### Performance
| ID | Requirement |
|---|---|
| NFR-1 | All database and service operations shall be asynchronous (`async/await`); no blocking calls on the UI thread. |
| NFR-2 | Command execution shall re-enter the UI thread via `MainThread.BeginInvokeOnMainThread` where commands mutate UI-bound state. |
| NFR-3 | `AsyncRelayCommand` shall implement a re-entrancy guard (`_isExecuting`) raising `CanExecuteChanged` — double-taps must not enqueue duplicate operations (critical for payment & add-to-order). |
| NFR-4 | Loading indicators (`ActivityIndicator`) shall be visible during any awaited data operation; primary buttons shall disable while their operation runs. |
| NFR-5 | Query result sets (cart, orders, addresses) are user-scoped to keep data volume and latency constant per account. |
| NFR-6 | Hot path queries shall hit the indexes listed in REQ-1.3 (Email lookups, per-user cart/orders/addresses, order-line fetch). |

### Security
| ID | Requirement |
|---|---|
| NFR-7 | Passwords shall exist in memory only transiently and at rest ** exclusively** as BCrypt hashes. |
| NFR-8 | Sessions shall live in platform `SecureStorage` (Keychain/Keystore/DPAPI-backed), never in the SQLite DB or preferences. |
| NFR-9 | Login failures shall be indistinguishable ("Invalid email or password") to prevent account enumeration. |
| NFR-10 | All order/address/cart reads shall be scoped to the authenticated `UserId` — cross-user record access by ID must be impossible (`GetOrderByIdAsync` verifies UserId). |
| NFR-11 | Email validation regex `^[^@\s]+@[^@\s]+\.[^@\s]+$` shall gate sign-up/profile; password minimum shall be 8 characters. |
| NFR-12 | Destructive actions (logout, delete address, cancel order) shall require an explicit confirmation dialog. |

### Usability & Accessibility
| ID | Requirement |
|---|---|
| NFR-13 | Full **light and dark theme** support via `AppThemeBinding` on all backgrounds, text, and borders. |
| NFR-14 | Minimum touch targets 44×44 via global styles (`MinimumHeight/WidthRequest` on Button/Entry/CheckBox/Editor). |
| NFR-15 | Every list/collection view shall have a designed empty state (icon + message + CTA). |
| NFR-16 | Recoverable input errors shall show inline banners (never modal); system-level failures may use alerts. |
| NFR-17 | Success feedback shall be visible (green banner) and auto-dismiss (1–3 s) to avoid permanent clutter. |
| NFR-18 | Layouts shall adapt: two-column login on WinUI/Mac Catalyst; grid spans per platform; modal sizes via `OnPlatform`/`OnIdiom`. |
| NFR-19 | Form inputs shall use `SfTextInputLayout` floating hints with clear placeholders and email keyboards where appropriate; password fields offer visibility toggles. |

### Reliability
| ID | Requirement |
|---|---|
| NFR-20 | Service methods shall catch exceptions, log to `Debug.WriteLine`, and return safe defaults (`false`/empty/`null`) — no unhandled exceptions from services. |
| NFR-21 | App startup shall tolerate DB initialization failure/timeout (30 s CTS) and session-read failure, always landing the user on a usable route. |
| NFR-22 | Multi-row writes (order creation, set-default address) shall be transactional — all-or-nothing. |
| NFR-23 | SecureStorage unavailability shall degrade gracefully to logged-out state. |
| NFR-24 | Historical orders shall be immune to catalog price changes (UnitPrice snapshots). |

### Portability
| ID | Requirement |
|---|---|
| NFR-25 | The single project shall build for `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`. |
| NFR-26 | Platform variation shall use XAML markup (`OnPlatform`, `OnIdiom`, `AppThemeBinding`) rather than code branches wherever possible. |
| NFR-27 | Currency display shall be `₹` with 2 decimals; formatting of persisted totals uses `CultureInfo.InvariantCulture` in navigation parameters. |

---

## 3. User Stories

| ID | As a… | I want to… | So that… |
|---|---|---|---|
| US-1 | new customer | sign up with name, email, and a strong password | I can securely access my own account |
| US-2 | returning customer | log in with my email (any capitalization) and password | I quickly resume ordering without case-sensitivity friction |
| US-3 | customer | stay logged in after reopening the app | I don't have to re-enter credentials every launch |
| US-4 | hungry customer | browse dishes with photos, ratings, price, and cuisine | I can decide what to order at a glance |
| US-5 | customer | search by dish, restaurant, or cuisine as I type | I find what I crave without scrolling everything |
| US-6 | vegetarian | flip a "Veg Only" switch | non-vegetarian items never distract me |
| US-7 | customer | tap a dish and see full details plus a quantity stepper | I can confirm the choice and buy the right amount |
| US-8 | customer | add an item to my cart and see a confirmation banner | I get immediate proof my selection was saved |
| US-9 | customer | add the same dish again later | quantities merge into one line instead of duplicates |
| US-10 | customer | see subtotal, 18% GST, ₹50 delivery fee, and total before paying | the final charge is never a surprise |
| US-11 | customer | remove an item from the cart | my totals update instantly |
| US-12 | customer | choose my payment method (UPI default) | I pay the way I prefer |
| US-13 | customer | see a "Processing Payment…" overlay while paying | I know the tap registered and don't double-pay |
| US-14 | customer | get a clear error and retry when payment fails | a transient failure doesn't lose my cart |
| US-15 | customer | see my Order ID and Transaction ID on success | I have proof of purchase and can reference support |
| US-16 | customer | see my order history newest-first with status chips | I can track every order in one place |
| US-17 | customer | open an order and see a 4-stage timeline | I understand exactly where my food is |
| US-18 | customer | see my delivery partner, their rating, and vehicle, and refresh their location | I feel confident the delivery is really coming |
| US-19 | customer | cancel an order only while it is still "Confirmed" (with a confirmation dialog) | I can back out of mistakes without abusing cancellations |
| US-20 | customer | edit my name and email in my profile | my account stays current |
| US-21 | customer | change my password by verifying the current one | my account stays secure even if a password leaks |
| US-22 | customer | save multiple addresses labeled Home/Work/Other | checkout delivery is flexible to where I actually am |
| US-23 | customer | set one address as the default | future orders go where I usually want them |
| US-24 | customer | delete an address after confirming | my address book stays clean |
| US-25 | rewards member | earn 5% of my order value as points and see them on my profile | loyalty is visible and motivating |
| US-26 | desktop user (Windows/macOS) | get a two-column branded login and wider grids/modals | the app feels native to my large screen, not like a stretched phone |
| US-27 | dark-mode user | have every screen adapt automatically | I can order comfortably at night |
| US-28 | user on the go | use the app fully offline | flaky connectivity never blocks browsing or ordering |

---

## 4. Acceptance Criteria

### AC-1 — Authentication (US-1, US-2, US-3)
1. Sign-up with any empty field → "All fields are required" banner; no navigation.
2. 7-character password → "Password must be at least 8 characters long"; mismatched confirm → "Passwords do not match".
3. Duplicate email (any case) → "Email already registered".
4. Valid sign-up → user row inserted with BCrypt hash (no plaintext anywhere in `foodordering.db3`); navigates to login.
5. Login with wrong password or unknown email → identical "Invalid email or password".
6. `John@X.com` registered → `john@x.com` logs in (case-insensitive).
7. Kill + relaunch app while logged in → lands on `//home` (session from SecureStorage).
8. Logout confirm → session keys removed → relaunch lands on `//login`.

### AC-2 — Catalog & Detail (US-4–US-8)
1. First run seeds exactly 8 dishes; Home grid shows all 8 cards with image, veg badge (on the 4 veg items), rating, ₹ price, cuisine.
2. Typing "pizza" in search → only Margherita Pizza remains; clearing search restores all.
3. Veg-Only on → only `IsVeg` items (4); combined with search "bowl" → Buddha Bowl only.
4. Search "zzz" → empty state ("No items found" + helper text).
5. Tapping a card opens item detail populated from the `itemId` query param.
6. Stepper: "−" at quantity 1 does nothing; "+" stops at 99; Add to Cart shows "Added 1 Margherita Pizza(s) to cart!" and hides after ~1 s.
7. Re-adding a dish already in cart increases that cart line's quantity (no duplicate row).

### AC-3 — Cart (US-9, US-10, US-11)
1. Cart with items: each line shows name, restaurant, unit price (`₹{0:F2} per item`), Qty, line total; summary shows Subtotal, "Tax (18% GST)", "Delivery Fee" (₹50.00), Total — all mathematically exact (e.g., one Pad Thai 449.50 → Tax 80.91, Total 580.41).
2. Remove on any line deletes it and Subtotal/Tax/Total decrease immediately.
3. Empty cart → 🛒 empty state with "Continue Shopping" → `//home`; summary hidden.
4. Proceed to Checkout → checkout modal shows the same total (`₹{0:F2}`) received via query param.

### AC-4 — Checkout & Payment (US-12–US-15)
1. Checkout opens with UPI pre-selected, no stale error/success banners, Confirm enabled.
2. Selecting a different method deselects all others (exclusivity).
3. Confirm → overlay "Processing Payment…" appears; tapping Confirm again during processing does nothing (re-entrancy guard).
4. Payment failure (~15% of attempts) → red banner "Payment failed. Please try again."; NO order row created; cart unchanged; retry works.
5. Payment success → success card with Order ID and `TXN_…` transaction ID; DB gains 1 Order + matching OrderItems at snapshot prices; user rewards increased by ⌊total×0.05⌋; cart is empty; auto-navigation to `//orders` after ~3 s.
6. Overlay tap / ✕ / Cancel → returns with no side effects.

### AC-5 — Orders & Tracking (US-16–US-18)
1. Orders tab lists history newest-first with correct chips (✓ Confirmed orange, 🍳 Preparing yellow, 🚗 blue, ✅ green).
2. Fresh order shows chip "✓ Confirmed" and detail timeline has stage 1 orange-current, 2–4 gray; ETA "Expected by {OrderDate+30min}".
3. After simulated advancement to Preparing → detail shows partner card (name, ⭐ rating, 📞, 🚗 vehicle) and ETA +20 min; Refresh updates coordinates via mock service.
4. Cancel button visible only at Confirmed; flow: Yes on dialog → status Cancelled → success alert → back on `//orders` list showing it excluded from Total Orders count.
5. Attempting cancel of a non-Confirmed order (service-level) returns false — no state change.

### AC-6 — Profile & Addresses (US-19, US-20–US-24)
1. Profile shows name, email, Joined, Total Orders (non-cancelled count), Rewards (e.g., 48 for a ₹966.85 first order: ⌊966.85×0.05⌋).
2. Edit → change email → Save → success banner (3 s); email persists across relaunch; Discard restores previous values with no save.
3. Change Password with wrong current → "Current password is incorrect"; correct flow (≥8 chars, match) → banner + fields cleared; old password no longer logs in, new one does.
4. Add Address with missing Street/City/State/Postal → corresponding required-field banner; valid save appears in list with label; edit pre-fills and updates in place.
5. Delete → Yes on dialog → row removed from list and DB.
6. Set Default on address B → B marked "(Default)"; A's default flag cleared (exactly one default; verified transactionally across restart).

### AC-7 — UI Quality (US-26, US-27, US-28)
1. Windows build: login shows left branding panel + right form; Home grid is 4 columns; modals ~500 px wide. Phone build: single column, 2-span grid, 320–340 px modals.
2. Toggle OS dark mode → every screen (home, cart, orders, profile, all modals) renders dark backgrounds with light text without restart.
3. Airplane mode / no network → entire flow (browse → order) still works (all local/mock).

### AC-8 — Platform Builds
`dotnet build` succeeds for all four TargetFrameworks from the single FoodOrderingApp.csproj; app launches on each available target.

---

## 5. Constraints and Dependencies

### 5.1 Technical Constraints
| ID | Constraint |
|---|---|
| CON-1 | .NET MAUI on **.NET 10** (`net10.0-*`); C# with `ImplicitUsings` and `Nullable` **enabled** (`string?`/`T?` used deliberately); XAML source generation (`MauiXamlInflator=SourceGen`). |
| CON-2 | Single multi-target project (`SingleProject=true`); app ID `com.cravedash.foodorderingapp`; display name "CraveDash"; Windows targets `10.0.19041.0` with min `10.0.17763.0` and `WindowsPackageType=None` (unpackaged dev run). |
| CON-3 | Minimum OS versions: iOS/Mac Catalyst 15.0, Android 21 (API 21), Windows 10 17763. |
| CON-4 | Money is `decimal` end-to-end; GST fixed 18%; delivery ₹50; rewards 5% — Indian-market constants. |
| CON-5 | Shell navigation only (no third-party nav); modal data passes via `QueryProperty` strings. |
| CON-6 | All external services (payment, maps, delivery network) are **mocked** in v1.0 — no real network calls. |
| CON-7 | MVVM discipline: no service/database access from code-behind beyond DI + BindingContext. |

### 5.2 Package Dependencies
| Package | Version | Purpose |
|---|---|---|
| `Microsoft.Maui.Controls` | `$(MauiVersion)` (net10) | Core MAUI framework |
| `CommunityToolkit.Mvvm` | 8.2.0 | MVVM helpers (ObservableObject, AsyncRelayCommand support) |
| `Syncfusion.Maui.Core` | * | SfTextInputLayout; `ConfigureSyncfusionCore` |
| `Syncfusion.Maui.Buttons` | * | SfSwitch |
| `Syncfusion.Maui.Inputs` | * | Input controls |
| `Syncfusion.Maui.Charts` | * | Charting capability (referenced) |
| `Syncfusion.Maui.DataGrid` | * | Data grid capability (referenced) |
| `Syncfusion.Maui.TabView` | * | Tab view control (referenced) |
| `sqlite-net-pcl` | 1.9.172 | SQLite ORM |
| `SQLitePCLRaw.bundle_green` | 2.1.8 | Native SQLite bundles |
| `BCrypt.Net-Core` | 1.6.0 | Password hashing |
| `Microsoft.Extensions.DependencyInjection` | 10.0.0 | DI container |
| `Microsoft.Extensions.Logging.Debug` | 10.0.0 | Debug logging |

### 5.3 Platform/Environment Dependencies
| ID | Dependency |
|---|---|
| DEP-1 | .NET 10 MAUI workloads per target platform (android/ios/maccatalyst/windows). |
| DEP-2 | `SecureStorage` backed by iOS Keychain, Android Keystore, Windows DPAPI-protection — with graceful degradation per NFR-23. |
| DEP-3 | Bundled/registered seed images (`burger.png`, `pizza.png`, `sushi.png`, `noodles.png`, `bowl.png`, `bbq.png`, `butter_chicken.png`, `cake.png`) resolvable as MAUI image resources. |
| DEP-4 | OpenSans-Regular & OpenSans-SemiBold fonts packaged in Resources/Fonts. |
| DEP-5 | (Mock-only) No runtime network connectivity required. |

### 5.4 Assumptions & Risks
- **Assumption:** single-window, single-active-session usage; singleton lifetimes are safe (Design D12).
- **Assumption:** users run per-platform toolchain (Xcode/Android SDK/VS workloads) to build non-Windows targets.
- **Risk:** Shell singleton pages can show stale state → mitigated by `OnAppearing` Initialize + explicit resets (Checkout REQ-6.3).
- **Risk:** SecureStorage absent on some emulators → degrades to login (NFR-23).
- **Risk:** "Forgot password?" label exists on Login without a wired flow — v1.0 leaves the tap gesture intentionally inert (out of scope, see Proposal §5.2).
