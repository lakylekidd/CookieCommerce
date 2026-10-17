# UI consistency rule

The Blazor application has three UI domains: **Web Shop**, **Backoffice**, and **Customer Dashboard**. Before implementing UI, identify its domain, inspect similar pages in that domain, and follow [the UI design system](ui-design-system.md). Search for existing components first; reuse shared components only when the underlying pattern is genuinely shared, and otherwise reuse domain-specific components and styles.

When available in Development, use the `/design-system` UI Brandbook to compare rendered examples and find their source components.

Preserve each domain's hover, focus, cursor, disabled, loading, animation, and responsive behavior. Do not copy a style from one domain into another without a reason grounded in the existing UI. Consistency is primarily required within the same UI domain.
