# Create a Modern Food Ordering Experience with Syncfusion® .NET MAUI

CraveDash is a modern food ordering application built with .NET MAUI, MVVM, and Syncfusion .NET MAUI controls. It demonstrates restaurant discovery, menu browsing, cart management, secure checkout, order tracking, and profile management in a responsive cross-platform app.

This repository now includes OpenSpec files and follows a spec-driven workflow so the feature scope, design decisions, and implementation tasks are captured before and during development.

## Getting Started with OpenSpec

The project uses OpenSpec to keep the implementation aligned with the documented product intent. The recommended flow is:

1. Define the feature request in the spec-first workflow.
2. Review the generated OpenSpec files.
3. Implement the app changes against the approved plan.
4. Keep the spec files updated if the design or scope changes.

Common OpenSpec commands used in Code Studio:

- `/opsx-propose` — create or draft a new proposal for the requested change.
- `/opsx-apply` — apply the approved proposal and move it into implementation.
- `/opsx-archive` — archive completed or superseded OpenSpec work.

For the full walkthrough, see Syncfusion's guide: [Build Voice Notes App with Spec-Driven Development](https://help.syncfusion.com/code-studio/getting-started/build-voice-notes-app-with-spec-driven-development).

## OpenSpec Files in This Repository

The OpenSpec folder is located at [FoodOrderingApp/openspec](FoodOrderingApp/openspec) and contains the following documents:

- [Proposal.md](FoodOrderingApp/openspec/Proposal.md) — the product proposal and problem statement.
- [Design.md](FoodOrderingApp/openspec/Design.md) — the solution design, architecture choices, and implementation approach.
- [Specs.md](FoodOrderingApp/openspec/Specs.md) — the detailed requirements and success criteria.
- [Tasks.md](FoodOrderingApp/openspec/Tasks.md) — the implementation task breakdown.

These files act as the source of truth for the application scope and should be updated together when requirements change.

## Suggested Workflow

When starting a new feature or revisiting the current sample, follow this sequence:

1. **Read the proposal** — confirm the user problem and expected outcome.
2. **Review the design** — understand how the solution is structured.
3. **Check the specs** — verify the requirements, acceptance criteria, and boundaries.
4. **Work through the tasks** — implement the app incrementally.
5. **Update documentation** — keep the README and OpenSpec files aligned with the final behavior.

## Key Features

- **Restaurant Discovery:** Browse restaurants with ratings, cuisine types, and featured dishes.
- **Interactive Food Catalog:** Explore menu items with images, descriptions, ratings, and pricing.
- **Product Detail View:** Review detailed item information and customize order quantities.
- **Cart Management:** Add, update, and remove items with real-time price calculations.
- **Multi-Payment Support:** Complete purchases using UPI, Net Banking, Credit Card, or Debit Card.
- **Order Tracking:** Access order history and review previously purchased items.
- **Profile Management:** Update personal details, manage addresses, and track user rewards.
- **Secure Authentication:** Password update and logout confirmation workflows.
- **Responsive Design:** Optimized layout for desktop and mobile devices.
- **MVVM-Based Architecture:** Clean separation of business logic, presentation, and data layers.

## Usage Scenarios

CraveDash supports a variety of food delivery and restaurant ordering workflows.

### Restaurant Browsing

- Browse restaurant listings with cuisine information and user ratings.
- Discover popular dishes and featured menu items.
- Search and explore food options across multiple restaurants.

### Food Ordering

- View detailed food descriptions, pricing, and ratings.
- Select desired quantities before adding items to the cart.
- Build customized orders from multiple restaurants.

### Cart & Checkout Management

- Modify item quantities directly from the cart.
- Review subtotal, taxes, delivery charges, and final payable amount.
- Remove items before checkout.
- Proceed through a streamlined payment workflow.

### Secure Payment Processing

- Choose preferred payment methods.
- Review order totals before confirmation.
- Complete transactions with an intuitive payment interface.

### User Account Management

- Manage personal details and account settings.
- Update passwords securely.
- Add and maintain delivery addresses.
- Monitor rewards and order history.

## Technologies Used

- **.NET MAUI:** Cross-platform application development.
- **Syncfusion .NET MAUI Toolkit:** Advanced UI and data visualization controls.
- **C# & XAML:** Core development technologies.
- **MVVM Architecture:** Structured application design with ViewModels and Models.
- **ObservableCollection:** Dynamic data management.
- **Data Binding & Commands:** Efficient UI interaction handling.

## Syncfusion Controls Highlighted

- **[SfTabView](https://help.syncfusion.com/maui/tabview/overview)**  

- **[SfTextInputLayout](https://help.syncfusion.com/maui/textinputlayout/overview)**

- **[SfButton](https://help.syncfusion.com/maui-toolkit/button/overview)** 

- **[SfPopup](https://help.syncfusion.com/maui/popup/overview)**  

- **[SfNumericEntry](https://help.syncfusion.com/maui/numericentry/overview)**

## Output

![FoodOrderingApplication]()

## Troubleshooting

### Path Too Long Exception

If you encounter a path too long exception when building this example project, close Visual Studio and rename the repository to short and build the project.

For a step-by-step procedure, refer to the [A Food Ordering Application Blog]().