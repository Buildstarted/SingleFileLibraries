ClassApplicatorTagHelper
------------------------

ClassApplicatorTagHelper provides a shorter way of toggling a class on an element.

```razor
    class:name="@Model.Value"
```

```razor
    class:name
```

```razor
    <!-- These are equivalent -->
    <div class="(@Model.Active ? 'active' : ''}">...</div>
    <div class:active={@Model.Active}>...</div>

    <!-- Shorthand, for when name and value match -->
    <div class:active>...</div>

    <!-- Multiple class toggles can be included -->
    <div class:active class:inactive={!@Model.Active} class:isAdmin>...</div>
```
