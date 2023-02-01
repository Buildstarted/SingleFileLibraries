ClassApplicatorTagHelper
------------------------

Inspired by Svelte class shortcuts

```razor
class:name="@Model.Value"
```

```razor
class:name
```

ClassApplicatorTagHelper provides a shorter way of toggling a class on an element.

```razor
<!-- These are equivalent -->
<div class="(@Model.Active ? 'active' : '')">...</div>
<div class:active="@Model.Active">...</div>

<!-- Shorthand, for when name and value match -->
<div class:active>...</div>

<!-- Multiple class toggles can be included -->
<div class:active class:inactive="@(!Model.Active)" class:isAdmin>...</div>
```

Setup
-----

In your `_ViewImports.cshtml` file add:
```razor
@addTagHelper *, SingleFileLibraries
```

Settings
--------

If you wish to change the default prefix from `class` to something else like `css` 
just add a configuration option to your `Startup.cs`

```csharp
builder.Services.Configure<ClassApplicatorConfiguration>(config => config.Prefix = "css");
```

Usage
-----

To set a class based on the boolean value of a property on your model

```razor
<span class:is-active="@Model.IsActive">Is active</span>
```

### Shortcut version

You can also use the shortcut version if the class name and the property name are the same. 
Class names to property names are case insensitive. Characters such as `-` and `_` are removed
from the class name and then compared against the property name.

If the value is not a boolean but non-null the class name will also be applied.

```razor
<span class:is-active>Is active</span>
<span class:name>Name</span>
```

#### Output

```html
<span class="is-active">Is active</span>
<span class="name">Name</span>
```

### Chaining class names

You can chain classnames to the same model value. Chaining classnames using the shortcut method
will check each classname against a property of the same name.

```razor
<span class:is-active|text-success="@Model.IsActive">Is Active</span>
```

#### Output

```html
<span class="is-active text-success">Is active</span>
```
