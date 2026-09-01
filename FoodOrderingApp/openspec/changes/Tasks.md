# Tasks: CraveDash — Cross-Platform Food Ordering Application

- **Change ID:** `build-food-ordering-app`
- **Status**: Approved
- **Related:** `Proposal.md` (intent), `Design.md` (architecture), `Specs.md` (requirements)
- **Legend:** ☐ = todo, ☑ = done. **Sections:** 14 phases + Testing & Deployment + Definition of Done.

---

## Phase 1: Project Initialization & Scaffolding

- [ ] 1.1 Create the .NET MAUI multi-target solution `FoodOrderingApp` (`FoodOrderingApp.slnx`) with project `FoodOrderingApp` targeting `net10.0-android`, and conditionally `net10.0-ios`/`net10.0-maccatalyst` (non-Linux) and `net10.0-windows10.0.19041.0` (Windows).
  - **Files:** `FoodOrderingApp.slnx`, `FoodOrderingApp/FoodOrderingApp.csproj`
  - **Depends on:** —
- [ ] 1.2 Set csproj properties: `UseMaui=true`, `SingleProject=true`, `ImplicitUsings=enable`, `Nullable=enable`, `MauiXamlInflator=SourceGen`, `ApplicationTitle=CraveDash`, `ApplicationId=com.cravedash.foodorderingapp`, versions `1.0`/`1`, `WindowsPackageType=None`, platform minimums (iOS/Mac 15.0, Android 21, Win 10.0.17763.0), `IncludeTransitiveFrameworkReferences=true`.
  - **Depends on:** 1.1
- [ ] 1.3 Add `PackageReference`s: `Microsoft.Maui.Controls` (`$(MauiVersion)`), `CommunityToolkit.Mvvm` 8.2.0, `Syncfusion.Maui.Core`, `Syncfusion.Maui.Buttons`, `Syncfusion.Maui.Inputs`, `Syncfusion.Maui.Charts`, `Syncfusion.Maui.DataGrid`, `Syncfusion.Maui.TabView` (latest), `sqlite-net-pcl` 1.9.172, `SQLitePCLRaw.bundle_green` 2.1.8, `BCrypt.Net-Core` 1.6.0, `Microsoft.Extensions.DependencyInjection` 10.0.0, `Microsoft.Extensions.Logging.Debug` 10.0.0. *(Specs CON-1/5.2)*
  - **Depends on:** 1.2
- [ ] 1.4 Verify resources: `Resources/AppIcon` (appicon.svg + foreground), `Resources/Splash` (splash.svg), `Resources/Fonts` (OpenSans-Regular.ttf, OpenSans-SemiBold.ttf), `Resources/Images` (8 seed dish PNGs + dotnet_bot.png), `Resources/Raw`, `Platforms/{Android,iOS,MacCatalyst,Windows}`, `Properties/launchSettings.json`. *(DEP-3, DEP-4)*
  - **Depends on:** 1.1
- [ ] 1.5 Smoke-build the empty project per available target (`dotnet build -f net10.0-windows10.0.19041.0` etc.).
  - **Depends on:** 1.3, 1.4

## Phase 2: Application Shell & Startup Infrastructure

- [ ] 2.1 Create `App.xaml` resource dictionary: light/dark color keys (`PrimaryColor #FF6B35`, `SecondaryColor #004E89`, `TertiaryColor #F7B801`, text/background/border/error `#DC2626`/success `#16A34A`/warning `#EA580C`), font-size + padding keys, named styles (`PageStyle`, `PrimaryButtonStyle`, `EntryStyle`, text styles), and the 4 converters as resources. *(NFR-13; REQ-11.5)*
  - **Files:** `App.xaml`, `Converters/{BoolToTextConverter,CountToVisibilityConverter,InvertedBoolConverter,StringToBoolConverter}.cs`
  - **Depends on:** Phase 1
- [ ] 2.2 Implement the 4 value converters per REQ-11.1–11.4 (including `CountToVisibilityConverter` `ShowEmpty` parameter and safe non-collection handling).
  - **Depends on:** 2.1
- [ ] 2.3 Create `App.xaml.cs`: constructor-injected `IAuthService`; `CreateWindow` builds `AppShell` + `Window`; `InitializeAppAsync` — resolve `IDatabaseService`, call `InitializeAsync` under a **30 s CancellationTokenSource**, load session cache, then route main-thread navigation `//home` if `IsSessionValid()` else `//login`, with nested login-fallback error handling and Debug logging throughout. *(REQ-1.8, REQ-2.7, NFR-21)*
  - **Files:** `App.xaml.cs`
  - **Depends on:** 2.1
- [ ] 2.4 Create `MauiProgram.cs`: `UseMauiApp<App>()`, font registration (OpenSansRegular/OpenSansSemiBold), `ConfigureSyncfusionCore()`, `AddDebug` logging (DEBUG), and `ConfigureServices` registering **7 services, 9 ViewModels, 10 Pages — all singletons**. *(REQ-12.6; Design D15)*
  - **Files:** `MauiProgram.cs`
  - **Depends on:** 2.3
- [ ] 2.5 Create `AppShell.xaml` (ShellContents: login, signup; TabBar: home/cart/orders/profile; global ShellContents: itemdetail/checkout/orderdetail/addressform; `FlyoutBehavior=Disabled`; themed background) and `AppShell.xaml.cs` registering the 4 modal routes via `Routing.RegisterRoute`. *(REQ-12.2, REQ-12.3)*
  - **Files:** `AppShell.xaml`, `AppShell.xaml.cs`
  - **Depends on:** 2.4

## Phase 3: Data Layer — Models & Database Service

- [ ] 3.1 Create SQLite entity models per Design §3.2: `Models/User.cs` (UserId PK, Email Unique/NotNull, FullName, PasswordHash, DOB?, JoinDate, RewardsPoints, audit timestamps), `Models/Item.cs` (ItemId, RestaurantName, ItemName, Description, Price, Image?, IsVeg, Cuisine, Rating, CreatedAt), `Models/CartItem.cs` (CartItemId, UserId/ItemId indexed, Quantity, AddedAt/UpdatedAt, `[Ignore] Item?`), `Models/Order.cs` (OrderId, UserId indexed, OrderDate, TotalAmount, Status default "Confirmed", EstimatedDelivery?, DeliveredAt?, DeliveryPartnerId?, audit, `[Ignore] Items`), `Models/OrderItem.cs` (OrderItemId, OrderId/ItemId indexed, Quantity, **UnitPrice** snapshot, audit, `[Ignore] Item?`), `Models/Address.cs` (AddressId, AddressLine1/2, PostalCode, UserId indexed, Street, City, State?, ZipCode?, Label "Home", IsDefault, audit). *(REQ-1.2; Specs §1 REQ-5/7/10 FKs)*
  - **Files:** `Models/*.cs`
  - **Depends on:** Phase 2
- [ ] 3.2 Create `Services/IDatabaseService.cs` interface: `InitializeAsync`, generic `GetByIdAsync<T>`, `GetAllAsync<T>`, `QueryAsync<T>`, `InsertAsync<T>`, `InsertAllAsync<T>`, `UpdateAsync<T>`, `DeleteAsync<T>` (entity + by id + `DeleteAllAsync<T>`), `ExecuteTransactionAsync`, `ClearUsersAsync`, `GetAllUsersAsync`. *(REQ-1.6, REQ-1.7)*
  - **Depends on:** 3.1
- [ ] 3.3 Implement `Database/DatabaseService.cs`: connection path `FileSystem.AppDataDirectory/foodordering.db3`; lazy init — FK pragma, 6 `CreateTableAsync`, 9 `CREATE INDEX IF NOT EXISTS` statements (REQ-1.3), image `.jpg/.jpeg→.png` normalization update (REQ-1.4), seed-if-empty with the exact 8 dishes of Design §3.3 (REQ-1.5); idempotency guard (REQ-1.8); async generic facade over `SQLiteAsyncConnection`; `ExecuteTransactionAsync` via `RunInTransactionAsync` with delegate sync-await, exception logging, false on failure.
  - **Files:** `Database/DatabaseService.cs`
  - **Depends on:** 3.2
- [ ] 3.4 Verification step: run app; confirm `foodordering.db3` created with 6 tables, 9 indexes, 8 seeded rows; second run does not duplicate seeds. *(AC-2.1)*
  - **Depends on:** 3.3

## Phase 4: Services — Validation & Auth

- [ ] 4.1 Create `Services/IValidationService.cs` + `Services/ValidationService.cs`: `IsValidEmail` (regex `^[^@\s]+@[^@\s]+\.[^@\s]+$`), `ValidatePassword` (min 8; optional upper/digit/special; `(bool, error)` return), `IsValidPhoneNumber` (`^[6-9]\d{9}$` after digit-strip), `IsValidPostalCode` (`^\d{6}$`), `ValidateRequired`, `ValidateLength`, `ValidatePasswordMatch`, `IsValidUrl`, `IsValidLatitude/Longitude/Coordinate`. *(REQ-2.1, NFR-11)*
  - **Files:** `Services/IValidationService.cs`, `Services/ValidationService.cs`
  - **Depends on:** Phase 3
- [ ] 4.2 Create `Services/IAuthService.cs` (login/signup/2× change-password, validate/update-profile, address CRUD + set-default, session API sync + async, `AuthResult` record class) and implement `Services/AuthService.cs`:
  - Login: required-field check → case-insensitive user lookup → BCrypt verify → SecureStorage session write + cache → `AuthResult`. *(REQ-2.4–2.5, NFR-7, NFR-9)*
  - SignUp: name required → email regex → password ≥8 → case-insensitive duplicate check → BCrypt hash → insert. *(REQ-2.1–2.3)*
  - ChangePassword(userId): current-password verify, ≥8 new, hash-rotate, `UpdatedAt`. *(REQ-2.9)*
  - Session: `IsSessionValid(Async)`, `ClearSession` (SecureStorage.RemoveAll + cache reset), `LogoutAsync`, `GetCurrentUserId/Email(Async)` with lazy SecureStorage hydration (`_sessionCacheLoaded`), `GetCurrentUserAsync`. *(REQ-2.5–2.8)*
  - Profile update + cached-email refresh. *(REQ-2.11)*
  - Address: get/add/update/delete + **transactional SetDefault** (all user addresses updated, exactly-one default). *(REQ-10.6)*
  - All paths return `AuthResult`/bool with exception capture — no thrown exceptions across boundary. *(REQ-2.10, NFR-20)*
  - **Files:** `Services/IAuthService.cs`, `Services/AuthService.cs`
  - **Depends on:** 4.1

## Phase 5: Services — Cart, Order, Payment, Map

- [ ] 5.1 Create `Services/ICartService.cs` + `Services/CartService.cs`: user-scoped `AddToCartAsync` with **merge-on-existing** semantics, `RemoveFromCartAsync`, `UpdateQuantityAsync` (reject <1 or >99), `ClearCartAsync`, `GetCartItemsAsync` (`ORDER BY AddedAt DESC`), `CalculateTotalAsync` (price×qty join), `GetCartCountAsync`; all auth-loss/exception-safe. *(REQ-5.1, 5.2, 5.7; NFR-5/20)*
  - **Files:** `Services/ICartService.cs`, `Services/CartService.cs`
  - **Depends on:** 4.2
- [ ] 5.2 Create `Services/IPaymentService.cs` (enum `PaymentMethod {UPI, NetBanking, CreditCard, DebitCard}`, `PaymentResult`) + `Services/PaymentService.cs` mock: 2–3.5 s delay, **15% deliberate failure**, `TXN_{unixMillis}_{random5}` ids, method whitelist validation. *(REQ-6.4, 6.6; Design D7)*
  - **Files:** `Services/IPaymentService.cs`, `Services/PaymentService.cs`
  - **Depends on:** 4.1
- [ ] 5.3 Create `Services/IOrderService.cs` + `Services/OrderService.cs`:
  - `CreateOrderAsync`: session guard → non-empty cart guard → **single transaction**: insert Order (Confirmed, ETA +45 min) → insert OrderItems with **UnitPrice snapshots** → rewards `total × 0.05` (`CalculateRewardsAsync`, simulated 50 ms) credited to user → `ClearCartAsync`. *(REQ-7.1, 7.2; NFR-22)*
  - `GetUserOrdersAsync` (user-scoped, newest-first), `GetOrderByIdAsync` (**UserId-verified** lookup — IDOR-safe). *(REQ-7.6; NFR-10)*
  - `UpdateOrderStatusAsync` (+`DeliveredAt` on Delivered) *(REQ-7.4)*; `CancelOrderAsync` (Confirmed-only) *(REQ-7.3)*; `SimulateStatusUpdateAsync` single-stage advance, terminal no-op. *(REQ-7.5)*
  - **Files:** `Services/IOrderService.cs`, `Services/OrderService.cs`
  - **Depends on:** 5.1
- [ ] 5.4 Create `Services/IMapService.cs` (DTOs: `DeliveryPartner`, `DeliveryRoute`, `LocationUpdate`, `Location` with Haversine `GetDistanceTo`) + `Services/MapService.cs` mock: partner generator (Indian names, `98…` phone, `DL/MH/KA/TN/GJ` vehicle numbers, rating 3.5–5.0, 500–5000 deliveries, active-delivery cache), route builder (Mumbai restaurant/customer mock coords, interpolated position), location poller (progress from `UtcNow.Second % 60`, statuses "Heading to your location" / "Almost there!"), ETA 10–25 min. *(REQ-8.4, 8.5; Design D7)*
  - **Files:** `Services/IMapService.cs`, `Services/MapService.cs`
  - **Depends on:** 5.3

## Phase 6: ViewModel Infrastructure & Commands

- [ ] 6.1 Implement the shared command layer in `ViewModels/LoginViewModel.cs` (bottom of file): `RelayCommand` (Action wrapper) and `AsyncRelayCommand` (Func<Task>, `MainThread.BeginInvokeOnMainThread` dispatch, `_isExecuting` re-entrancy guard, `CanExecuteChanged`, Debug exception logging) — plus the generic parameter variant used by Cart/Orders/Profile. *(NFR-2, NFR-3)*
  - **Depends on:** Phase 5
- [ ] 6.2 Adopt the canonical ViewModel skeleton (documented in Design §2.2): `INotifyPropertyChanged` + `SetProperty<T>` + command properties + `InitializeAsync()` — to be used by all 9 VMs in phases 7–11.

## Phase 7: Authentication UI — Login & SignUp

- [ ] 7.1 `ViewModels/LoginViewModel.cs`: Email/Password/ErrorMessage/IsLoading/IsPasswordVisible state; `LoginCommand` (required-field check → `LoginAsync` → success `GoToAsync("//home")`, else error banner; loading lifecycle), `NavigateToSignUpCommand` (reset fields → `//signup`), `TogglePasswordVisibilityCommand`, `ResetFields()`. *(REQ-2.4; US-2)*
  - **Depends on:** 6.1, 4.2
- [ ] 7.2 `Views/LoginPage.xaml` + code-behind: `Shell.NavBarIsVisible=false`; two-column Grid — branding panel (🍔 CraveDash, "Premium food at your fingertips") **visible only WinUI/MacCatalyst**; form column: welcome headers, error border (`StringToBoolConverter`), `SfTextInputLayout` email + password (`EnablePasswordVisibilityToggle`), "Forgot password?" label (inert in v1), Sign In button (disabled while loading via `InvertedBoolConverter`), ActivityIndicator, "Sign Up" tap-link. *(AC-1; AC-7.1; NFR-14, 19)*
  - **Files:** `Views/LoginPage.xaml(.cs)`
  - **Depends on:** 7.1
- [ ] 7.3 `ViewModels/SignUpViewModel.cs`: FullName/Email/Password/ConfirmPassword + error/loading/visibility flags; `SignUpCommand` — all-fields check → ≥8 password → match check → `SignUpAsync` → success `//login`, error banner otherwise; navigate-to-login with field reset; 2 visibility toggles. *(REQ-2.1–2.3, AC-1.1–1.4)*
  - **Depends on:** 6.1, 4.2
- [ ] 7.4 `Views/SignUpPage.xaml` + code-behind: SfTextInputLayout form, inline validation error banner, loading state, Sign Up + "Already have an account? Login" link.
  - **Files:** `Views/SignUpPage.xaml(.cs)`
  - **Depends on:** 7.3
- [ ] 7.5 Manual verification AC-1.1–1.8 (validation messages, duplicate email, case-insensitive login, session restore, logout).
  - **Depends on:** 7.2, 7.4

## Phase 8: Home — Discovery, Search & Filter

- [ ] 8.1 `ViewModels/HomeViewModel.cs`: `Items`/`FilteredItems` (ObservableCollection), `SearchQuery`, `ShowVegetarianOnly`, `IsLoading`, `CurrentUser`; `SearchQuery`/`ShowVegetarianOnly` setters trigger `ApplyFilters()` (case-insensitive substring on ItemName/RestaurantName/Cuisine; veg flag intersection); `LoadItemsCommand` (awaited in `InitializeAsync` + `LoadCurrentUserAsync`), `ItemSelectedCommand` → `itemdetail?itemId=`, `LogoutCommand` (clear session → `//login`). *(REQ-3.2, 3.3, 3.5, 3.6)*
  - **Depends on:** 6.1, 4.2
- [ ] 8.2 `Views/HomePage.xaml` + code-behind: themed header (primary-color bar with SearchBar bound to `SearchQuery`; "Veg Only :" label + `SfSwitch` bound to `ShowVegetarianOnly` with custom On/Off `SwitchSettings`); `CollectionView` with `GridItemsLayout Span={OnPlatform Default=2, WinUI=4, MacCatalyst=3}`; item card template (Border + Shadow, image 250h, 🌱 badge, restaurant, name, ⭐ rating `{0:F1}`, `₹{0:F0}` price, cuisine); EmptyView (😔, "No items found", helper); centered `ActivityIndicator`; `OnSelectionChanged` handler executing `ItemSelectedCommand` with the tapped `Item`. *(REQ-3.1, 3.4; AC-2; NFR-15)*
  - **Files:** `Views/HomePage.xaml(.cs)`
  - **Depends on:** 8.1

## Phase 9: Item Detail & Add-to-Cart

- [ ] 9.1 `ViewModels/ItemDetailViewModel.cs`: `[QueryProperty(ItemId)]` triggering `LoadItemAsync`; `Quantity` setter clamped 1–99; `Increment`/`DecrementQuantityCommand`; `AddToCartCommand` — guard Item + CartService present → `AddToCartAsync` → success banner "Added {n} {name}(s) to cart!" auto-hide 1 s; `CloseCommand` → `GoToAsync("..")`. *(REQ-4.1–4.5; AC-2.5–2.7)*
  - **Depends on:** 5.1, 6.1
- [ ] 9.2 `Views/ItemDetailPopup.xaml` + code-behind using the **modal card pattern** (Design §5.3): dim BoxView overlay (tap → Close), centered `Border` (`RoundRectangle 16`, shadow, `HeightRequest={OnIdiom Phone=500, Tablet=700, Desktop=700}`, `WidthRequest={OnPlatform Default=320, WinUI=500, MacCatalyst=600}`), ✕ close button, hero image, metadata labels, description, price card (`₹{0:F2}`), quantity stepper (− / boxed value / +), success banner, Add to Cart button (disabled while loading), ActivityIndicator. *(REQ-4.1–4.4; AC-7.1)*
  - **Files:** `Views/ItemDetailPopup.xaml(.cs)`
  - **Depends on:** 9.1

## Phase 10: Cart & Checkout

- [ ] 10.1 `ViewModels/CartViewModel.cs` + wrapper `CartItemViewModel` (composes CartItem+Item; computed DisplayName/RestaurantName/UnitPrice/Quantity/Total; parent RemoveCommand): state `CartItems`, `Subtotal`, `Tax=0.18m`, `DeliveryFee=50`, `Total` (recalc via `CalculateTotal()` on setters), `IsLoading`, `IsCartEmpty`; commands — `LoadCartCommand` (per-user load + item hydration + subtotal), `RemoveItemCommand<T>` (remove + collection delete + recompute), `CheckoutCommand` — recompute subtotal/tax/fee/total → `GoToAsync("checkout", {total: "F2" InvariantCulture})`, `ContinueShoppingCommand` → `//home`. *(REQ-5.3–5.6; AC-3)*
  - **Depends on:** 5.1, 6.1
- [ ] 10.2 `Views/CartPage.xaml` + code-behind: "Your Cart" header; line-item cards (rounded image, name/restaurant/unit-price, qty, line total, Remove) in vertical CollectionView; empty state (🛒 + Continue Shopping, `InvertedBoolConverter`-toggled via `IsCartEmpty`); bottom summary panel (Subtotal / "Tax (18% GST)" / Delivery Fee / Total, `₹{0:F2}`, separator rule) + "Proceed to Checkout"; ActivityIndicator. *(REQ-5.3–5.5; NFR-15)*
  - **Files:** `Views/CartPage.xaml(.cs)`
  - **Depends on:** 10.1
- [ ] 10.3 `ViewModels/CheckoutViewModel.cs` + `PaymentMethodOption` POCO: `[QueryProperty(TotalAmount, "total")]`; `SelectedPaymentMethod` (UPI default), 4 `PaymentMethodOption`s (💳 UPI / 🏦 Net Banking / 💰 Credit Card / 🏧 DebitCard with descriptions); `ConfirmPaymentCommand` — amount > 0 guard → `ValidatePaymentMethodAsync` → `PaymentProcessing=true` → `ProcessPaymentAsync` → success: `CreateOrderAsync(TotalAmount)` → success card (order id + transaction id) → 3 s → `//orders`; order-null → error; payment failure → error banner; full try/catch with `PaymentProcessing=false` in finally. `CancelCommand` → `GoToAsync("..")`. *(REQ-6.1–6.9; AC-4)*
  - **Depends on:** 5.2, 5.3, 6.1
- [ ] 10.4 `Views/CheckoutPopup.xaml` + code-behind: modal card pattern; card title + ✕; error/success borders; total-amount highlight card (`₹{0:F2}`, 32-pt); 4 exclusive payment method cards (UPI checkbox default-checked; `CheckedChanged` handlers in code-behind mutate `SelectedPaymentMethod` and clear the other three); T&C footnote; **processing overlay** (semi-opaque card overlay, ActivityIndicator, "Processing Payment…" — visible while `PaymentProcessing`); Cancel/Confirm grid (hidden/disabled during processing). `OnAppearing` resets: error/success messages, `PaymentProcessing`, `IsLoading`, `SelectedPaymentMethod = UPI`. *(REQ-6.2, 6.3, 6.5; AC-4.1–4.3, 4.6)*
  - **Files:** `Views/CheckoutPopup.xaml(.cs)`
  - **Depends on:** 10.3
- [ ] 10.5 Manual verification AC-3 (line math incl. merge-on-add & removal recalc) and AC-4.4–4.5 (payment failure leaves cart intact; success creates Order + snapshot OrderItems + rewards + empty cart, transactionally).
  - **Depends on:** 10.2, 10.4

## Phase 11: Orders, Tracking & Profile

- [ ] 11.1 `ViewModels/OrdersViewModel.cs` + `OrderViewModel` POCO: `Orders` collection with re-broadcast `CollectionChanged→PropertyChanged`; `EmptyMessage` ("No orders yet. Start ordering!"); `LoadOrdersCommand` (user orders newest-first → wrap with `StatusDisplayName` emoji chips + `StatusColor` mapping (`#FF6B35/#F7B801/#004E89/#16A34A/#999999`), `FormattedDate` `MMM dd, yyyy 'at' HH:mm`, `FormattedAmount` ₹); `OrderSelectedCommand` → `orderdetail?id=`. *(REQ-7.7, 7.8; US-16)*
  - **Depends on:** 5.3, 6.1
- [ ] 11.2 `Views/OrdersPage.xaml` + code-behind: "Your Orders" header + overlay ActivityIndicator; empty state (📦 + message + "Start Ordering" wired to code-behind navigation); order cards (Order #id, date, ₹ amount, colored status chip, separator). *(REQ-7.7; NFR-15)*
  - **Files:** `Views/OrdersPage.xaml(.cs)`
  - **Depends on:** 11.1
- [ ] 11.3 `ViewModels/OrderDetailViewModel.cs` + `TimelineItem`/`OrderItemDetail` POCOs: `[QueryProperty(OrderId)]` → `LoadOrderAsync` (user-verified order → formatted display fields, status color, **timeline build** (4 stages; stage `IsCompleted` per status; current orange/completed green/pending gray; line colors), `CanCancelOrder = (Status=="Confirmed")`; partner info + `RefreshLocationAsync` when status ∉ {Confirmed, Cancelled}; `CalculateEstimatedDeliveryTime` (30/20/10/0 minutes by status → "Expected by HH:mm" / "Delivered"); order items **placeholder row** (v1.0 scope); error path → "Failed to load order" alert). Commands: `CancelOrderCommand` (Yes/No `DisplayAlert` → `CancelOrderAsync` → success alert → `//orders`), `BackCommand` → `//orders`, `RefreshLocationCommand`. *(REQ-8.1–8.8; AC-5.2–5.5, AC-5.5 service rule)*
  - **Depends on:** 5.3, 5.4, 6.1
- [ ] 11.4 `Views/OrderDetailPage.xaml` + code-behind: modal card pattern; back header (‹ + `Order #id`); summary card (date + ₹ amount + colored status); timeline card (icon bubble Border + display name + connector BoxView per `TimelineItem`); two-column delivery-partner card (👤 name + ⭐ rating; 📞 phone; 🚗 vehicle; **Refresh** button) + delivery-details card (🕐 ETA; 📍 address placeholder); order-items card; Cancel Order button (visible while `CanCancelOrder`); tap-overlay dismiss bound to `CloseCommand`. *(REQ-8.1–8.8; AC-5)*
  - **Files:** `Views/OrderDetailPage.xaml(.cs)`
  - **Depends on:** 11.3
- [ ] 11.5 `ViewModels/ProfileViewModel.cs` + `AddressItem` POCO: state (CurrentUser, edit-mode flags, FullName/Email, JoinDate, TotalOrders, RewardsPoints, password trio, messages, `Addresses`, `CurrentAddress` + `[QueryProperty(AddressId)]` for edit-load); `LoadProfileAsync` (user + join date + rewards + **non-cancelled order count** + addresses + edit copy), `SaveProfileCommand` (required name/email → update + 3-s success banner), `DiscardChangesCommand`, `ChangePasswordCommand` (presence → ≥8 → match → `ValidatePasswordAsync` → `ChangePasswordAsync(email,new)` → banner + field clear), `LogoutCommand` (confirm → `LogoutAsync` → `//login`); address commands — `AddAddressCommand` (`//addressform`), `EditAddressCommand<T>` (`//addressform?id=`), `DeleteAddressCommand<T>` (confirm → delete → remove from list → banner), `SetDefaultAddressCommand<T>` (**transactional** exactly-one default → reload → banner), `SaveAddressCommand` (required Street/City/State/Postal validations → insert-or-update via `IAuthService` → reload + 2-s banner → `GoToAsync("..")`), `CancelCommand` (reset form → `//profile`). *(REQ-9.1–9.4, REQ-10.1–10.7; AC-6)*
  - **Depends on:** 4.2, 6.1
- [ ] 11.6 `Views/ProfilePage.xaml` + code-behind: two-column responsive card layout — profile info card (`SfTextInputLayout` name/email enabled only in edit mode, Save/Discard buttons shown in edit mode), stats card (Joined / Total Orders / 🎁 Rewards), address card (header + **+ Add**; per-address rows showing `Label (Default)` when default, address line, Edit/Delete buttons bound via `RelativeSource` to page BindingContext, **Set Default** only when not default), Security card (current/new/confirm `SfTextInputLayout` password fields with toggles + Change Password button), red Logout button; success/error banners; full-screen ActivityIndicator. *(REQ-9, 10; AC-6)*
  - **Files:** `Views/ProfilePage.xaml(.cs)`
  - **Depends on:** 11.5
- [ ] 11.7 `Views/AddressFormPopup.xaml` + code-behind: modal card pattern; adaptive title (Add New Address / Edit Address), ✕ cancel; error banner; fields — Label (free text), Street Address, City, State, Postal Code entries; Save + Cancel; validation via `SaveAddressAsync` rules. *(REQ-10.2–10.4)*
  - **Files:** `Views/AddressFormPopup.xaml(.cs)`
  - **Depends on:** 11.5
- [ ] 11.8 Manual verification AC-6.1–6.6 (profile math, password rotation, address CRUD + transactional default).
  - **Depends on:** 11.6, 11.7

## Phase 12: Cross-Cutting Hardening & UI Polish

- [ ] 12.1 Audit every awaited operation for `ActivityIndicator`/disabled-button feedback; confirm `InvertedBoolConverter` disables submit controls during `IsLoading`/`PaymentProcessing`/`IsSaving`/`IsChangingPassword`. *(NFR-4)*
- [ ] 12.2 Verify `AsyncRelayCommand` re-entrancy guard across double-tap-sensitive commands (Add to Cart, Confirm Payment, Cancel Order). *(NFR-3; AC-4.3)*
- [ ] 12.3 Light/dark sweep: every page/modal honors `AppThemeBinding` (backgrounds, text, borders, inputs). *(NFR-13; AC-7.2)*
- [ ] 12.4 Adaptive sweep: login two-column, home grid spans, modal `OnPlatform`/`OnIdiom` sizes on the widest available targets. *(NFR-18; AC-7.1)*
- [ ] 12.5 Empty-state sweep: Home (filtered to zero), Cart, Orders. *(NFR-15)*
- [ ] 12.6 Error-path sweep: simulated payment failure banner + retry; order-load failure alert; SecureStorage unavailability fallback. *(NFR-20, 23; AC-4.4)*

## Phase 13: Testing & Verification Activities

- [ ] 13.1 **Unit-level service checks** (manual/scripted, DEBUG output): duplicate-email rejection at signUp; BCrypt hash verify round-trip; case-insensitive login; quantity bounds (0, 1, 99, 100 rejected at service); merge-on-add row math; `CalculateRewardsAsync(966.85) = 48.34 → 48 points`; transaction rollback leaves cart when an inner insert fails.
  - **Depends on:** Phases 4–5
- [ ] 13.2 **Integration walkthroughs (per AC section)**: AC-1 Auth; AC-2 Catalog; AC-3 Cart math; AC-4 Payment (success + forced-failure retry loop); AC-5 Order lifecycle incl. `SimulateStatusUpdateAsync` progression, partner card at Preparing, cancel-at-Confirmed-only; AC-6 Profile/addresses incl. set-default exclusivity; AC-7 themes/adaptivity/offline; AC-8 four-target builds. *(All sections of Specs §4)*
  - **Depends on:** Phase 12
- [ ] 13.3 **Persistence checks**: kill/relaunch mid-session (route restore, cart contents, rewards balance); DB file inspection (6 tables, 9 indexes, 8 seeds, hashed `PasswordHash`, `UnitPrice` snapshots unchanged after catalog price edits).
  - **Depends on:** 13.2
- [ ] 13.4 **UI stress**: rapid double-taps on payment/add-to-cart; rapid tab switching during load; modal open/close spam; search typing storm (filter responsiveness).
  - **Depends on:** 13.2

## Phase 14: Deployment & Release Activities

- [ ] 14.1 Windows: run unpackaged (`WindowsPackageType=None`) via `dotnet build -f net10.0-windows10.0.19041.0` + F5/deploy; verify on min-OS 10.0.17763.
- [ ] 14.2 Android: deploy to device/emulator (API 21+ min); verify SecureStorage on Keystore, tab bar, modal sizing on phone idiom.
- [ ] 14.3 iOS/Mac Catalyst: build/deploy where toolchain available (Xcode; min iOS 15); verify Keychain session, light/dark, safe-area.
- [ ] 14.4 Release readiness: `ApplicationDisplayVersion 1.0` / `ApplicationVersion 1`; splash + app icons render; fonts packaged; DEBUG logging compiled out in Release.
- [ ] 14.5 Post-build hygiene: delete `bin`/`obj` in packaging; README describes build per target (path-length caveat for repo clones); optional store packaging follow-up (out of scope v1.0).
- [ ] 14.6 Archive the change: on completion, run `/opsx-archive` — final specs → `openspec/specs/`, change folder → `openspec/changes/archive/`.

---

## Task Dependency Graph (summary)

```
Phase 1 (scaffold)
   └── Phase 2 (shell, App, DI, converters)
          ├── Phase 3 (models + DatabaseService)      ── 3.4 DB smoke test
          │      └── Phase 4 (Validation + Auth services)
          │             ├── Phase 5 (Cart, Payment, Order, Map services)
          │             │        └── Phase 6 (command infrastructure)
          │             │               ├── Phase 7 (Login/SignUp UI)   ── AC-1
          │             │               ├── Phase 8 (Home)              ── AC-2 (browse part)
          │             │               ├── Phase 9 (ItemDetail)          ── AC-2 (detail part)
          │             │               ├── Phase 10 (Cart + Checkout)    ── AC-3, AC-4
          │             │               └── Phase 11 (Orders/Tracking/Profile/Address) ── AC-5, AC-6
          │             │                        └── Phase 12 (hardening/polish)
          │             │                                 └── Phase 13 (testing) ── AC-7, AC-8
          │             │                                          └── Phase 14 (deployment + archive)
```

**Critical path:** 1 → 2 → 3 → 4 → 5 → 6 → 10 → 11 → 12 → 13 → 14 (checkout/order creation exercises the deepest stack: UI → VM → payment mock → order transaction → DB).

**Parallelization:** Phases 7–9 screens are independent of Phase 10–11 once Phase 6 lands — they can be built concurrently by separate contributors.

---

## Definition of Done

- [ ] All requirements REQ-1…REQ-12.6 and NFR-1…NFR-27 in `Specs.md` implemented and traceable to a task above.
- [ ] All acceptance criteria AC-1…AC-8 pass on at least one reference platform, with Android + Windows as primary verification targets.
- [ ] App builds for all four target frameworks from the single project (AC-8).
- [ ] No service throws unhandled exceptions in normal + simulated-failure paths (NFR-20).
- [ ] Full order flow completes offline end-to-end (US-28 / AC-7.3).
- [ ] `openspec/changes/build-food-ordering-app/` contents (Proposal, Design, Specs, Tasks) reviewed and archived per Step 6 of the SDD workflow (task 14.6).
