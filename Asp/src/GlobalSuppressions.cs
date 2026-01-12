using System.Diagnostics.CodeAnalysis;

// Suppress trimming warnings for model binding provider
// Model types are registered with MVC at compile time and known to the runtime
[assembly: UnconditionalSuppressMessage("Trimming", "IL2075",
    Justification = "Model types implementing ITryCreatable are registered with MVC at compile time",
    Scope = "member",
    Target = "~M:FunctionalDdd.Asp.ModelBinding.ValueObjectModelBinderProvider.GetBinder(Microsoft.AspNetCore.Mvc.ModelBinding.ModelBinderProviderContext)~Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinder")]

// Suppress AOT warnings for generic type instantiation in model binder
// Model binding occurs at startup before AOT compilation paths are executed
[assembly: UnconditionalSuppressMessage("AOT", "IL3050",
    Justification = "Model binding is resolved at application startup, not in AOT-compiled code paths",
    Scope = "member",
    Target = "~M:FunctionalDdd.Asp.ModelBinding.ValueObjectModelBinderProvider.GetBinder(Microsoft.AspNetCore.Mvc.ModelBinding.ModelBinderProviderContext)~Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinder")]

// Suppress all trimming/AOT warnings for ValueObjectJsonInputFormatter
// This is an optional feature that intentionally uses reflection for flexibility
// Users who need AOT can use manual validation or the model binder (non-JSON) approach
[assembly: UnconditionalSuppressMessage("Trimming", "IL2026",
    Justification = "ValueObjectJsonInputFormatter is optional and uses reflection by design. For AOT scenarios, use manual validation or model binder for route/query parameters.",
    Scope = "type",
    Target = "~T:FunctionalDdd.Asp.ModelBinding.ValueObjectJsonInputFormatter")]

[assembly: UnconditionalSuppressMessage("Trimming", "IL2067",
    Justification = "ValueObjectJsonInputFormatter is optional and uses reflection by design",
    Scope = "type",
    Target = "~T:FunctionalDdd.Asp.ModelBinding.ValueObjectJsonInputFormatter")]

[assembly: UnconditionalSuppressMessage("Trimming", "IL2070",
    Justification = "ValueObjectJsonInputFormatter is optional and uses reflection by design",
    Scope = "type",
    Target = "~T:FunctionalDdd.Asp.ModelBinding.ValueObjectJsonInputFormatter")]

[assembly: UnconditionalSuppressMessage("Trimming", "IL2072",
    Justification = "ValueObjectJsonInputFormatter is optional and uses reflection by design",
    Scope = "type",
    Target = "~T:FunctionalDdd.Asp.ModelBinding.ValueObjectJsonInputFormatter")]

[assembly: UnconditionalSuppressMessage("Trimming", "IL2075",
    Justification = "ValueObjectJsonInputFormatter is optional and uses reflection by design",
    Scope = "type",
    Target = "~T:FunctionalDdd.Asp.ModelBinding.ValueObjectJsonInputFormatter")]

[assembly: UnconditionalSuppressMessage("AOT", "IL3050",
    Justification = "ValueObjectJsonInputFormatter is optional and uses reflection by design",
    Scope = "type",
    Target = "~T:FunctionalDdd.Asp.ModelBinding.ValueObjectJsonInputFormatter")]
