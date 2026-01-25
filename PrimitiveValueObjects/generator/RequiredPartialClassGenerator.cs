namespace SourceGenerator;

using FunctionalDdd.PrimitiveValueObjectGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;

/// <summary>
/// C# source generator that automatically creates factory methods, validation logic, and parsing support
/// for value objects inheriting from <c>RequiredGuid</c> or <c>RequiredString</c>.
/// </summary>
/// <remarks>
/// <para>
/// This incremental source generator analyzes partial class declarations and generates complementary code
/// that provides a complete, production-ready value object implementation. It eliminates boilerplate while
/// maintaining type safety and validation consistency.
/// </para>
/// <para>
/// For each partial class inheriting from <c>RequiredGuid</c>, generates:
/// <list type="bullet">
/// <item><c>IScalarValue&lt;TSelf, Guid&gt;</c> - Interface for ASP.NET Core automatic validation</item>
/// <item><c>NewUnique()</c> - Creates a new instance with a unique GUID</item>
/// <item><c>TryCreate(Guid)</c> - Creates from non-nullable GUID (required by IScalarValue)</item>
/// <item><c>TryCreate(Guid?)</c> - Creates from nullable GUID with empty validation</item>
/// <item><c>TryCreate(string?)</c> - Parses from string with format and empty validation</item>
/// <item><c>Parse(string, IFormatProvider?)</c> - IParsable implementation</item>
/// <item><c>TryParse(...)</c> - IParsable try-parse pattern</item>
/// <item>Private constructor calling base class</item>
/// <item>JSON converter attribute for serialization</item>
/// <item>Explicit cast operator from GUID</item>
/// <item>OpenTelemetry activity tracing</item>
/// </list>
/// </para>
/// <para>
/// For each partial class inheriting from <c>RequiredString</c>, generates:
/// <list type="bullet">
/// <item><c>IScalarValue&lt;TSelf, string&gt;</c> - Interface for ASP.NET Core automatic validation</item>
/// <item><c>TryCreate(string)</c> - Creates from non-nullable string (required by IScalarValue)</item>
/// <item><c>TryCreate(string?)</c> - Creates from nullable string with null/empty/whitespace validation</item>
/// <item><c>Parse(string, IFormatProvider?)</c> - IParsable implementation</item>
/// <item><c>TryParse(...)</c> - IParsable try-parse pattern</item>
/// <item>Private constructor calling base class</item>
/// <item>JSON converter attribute for serialization</item>
/// <item>Explicit cast operator from string</item>
/// <item>OpenTelemetry activity tracing</item>
/// </list>
/// </para>
/// <para>
/// Benefits of using the source generator:
/// <list type="bullet">
/// <item><strong>Zero boilerplate</strong>: Write just the class declaration, get full implementation</item>
/// <item><strong>Consistency</strong>: All value objects have identical validation and factory patterns</item>
/// <item><strong>Type safety</strong>: Prevents primitive obsession with strongly-typed wrappers</item>
/// <item><strong>Performance</strong>: Generated code is compiled, no runtime reflection</item>
/// <item><strong>Observability</strong>: Built-in OpenTelemetry tracing for all operations</item>
/// <item><strong>Serialization</strong>: Automatic JSON support for APIs and persistence</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Minimal value object definition:
/// <code>
/// // Your code - just the declaration
/// public partial class CustomerId : RequiredGuid
/// {
/// }
/// 
/// public partial class FirstName : RequiredString
/// {
/// }
/// 
/// // Generated code provides everything else:
/// var id = CustomerId.NewUnique();
/// var nameResult = FirstName.TryCreate("John");
/// </code>
/// </example>
/// <example>
/// Generated code for RequiredGuid (CustomerId):
/// <code>
/// // &lt;auto-generated/&gt;
/// namespace MyApp.Domain;
/// using FunctionalDdd;
/// using System.Diagnostics.CodeAnalysis;
/// using System.Text.Json.Serialization;
///
/// [JsonConverter(typeof(ParsableJsonConverter&lt;CustomerId&gt;)]
/// public partial class CustomerId : IScalarValue&lt;CustomerId, Guid&gt;, IParsable&lt;CustomerId&gt;
/// {
///     private CustomerId(Guid value) : base(value) { }
///
///     public static explicit operator CustomerId(Guid customerId)
///         =&gt; TryCreate(customerId).Value;
///
///     public static CustomerId NewUnique() =&gt; new(Guid.NewGuid());
///
///     // Required by IScalarValue - enables automatic ASP.NET Core validation
///     public static Result&lt;CustomerId&gt; TryCreate(Guid value)
///         =&gt; TryCreate((Guid?)value, null);
///
///     public static Result&lt;CustomerId&gt; TryCreate(Guid? requiredGuidOrNothing, string? fieldName = null)
///     {
///         using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity("CustomerId.TryCreate");
///         var field = !string.IsNullOrEmpty(fieldName)
///             ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
///             : "customerId";
///         return requiredGuidOrNothing
///             .ToResult(Error.Validation("Customer Id cannot be empty.", field))
///             .Ensure(x =&gt; x != Guid.Empty, Error.Validation("Customer Id cannot be empty.", field))
///             .Map(guid =&gt; new CustomerId(guid));
///     }
///
///     public static Result&lt;CustomerId&gt; TryCreate(string? stringOrNull, string? fieldName = null)
///     {
///         // Parsing logic with validation...
///     }
///
///     public static CustomerId Parse(string s, IFormatProvider? provider) { /* ... */ }
///     public static bool TryParse(...) { /* ... */ }
/// }
/// </code>
/// </example>
[Generator(LanguageNames.CSharp)]
public class RequiredPartialClassGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the incremental generator, setting up the syntax provider and compilation pipeline.
    /// </summary>
    /// <param name="context">The initialization context provided by the compiler.</param>
    /// <remarks>
    /// <para>
    /// This method configures the incremental generator pipeline:
    /// <list type="number">
    /// <item>Creates a syntax provider that identifies candidate classes (partial classes with base types)</item>
    /// <item>Filters to only classes inheriting from RequiredGuid or RequiredString</item>
    /// <item>Combines syntax nodes with compilation for semantic analysis</item>
    /// <item>Registers the source output callback to generate code</item>
    /// </list>
    /// </para>
    /// <para>
    /// The incremental approach ensures:
    /// <list type="bullet">
    /// <item>Fast incremental builds - only regenerates changed files</item>
    /// <item>Minimal IDE impact - efficient background generation</item>
    /// <item>Correct dependency tracking - regenerates when base classes change</item>
    /// </list>
    /// </para>
    /// </remarks>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassDeclarationSyntax> requiredGuids = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (n, _) => IsSyntaxTargetForGeneration(n),
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node
                ).Where(m => m is not null);

        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndEnums
            = context.CompilationProvider.Combine(requiredGuids.Collect());

        context.RegisterSourceOutput(compilationAndEnums,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    /// <summary>
    /// Executes the source generation for all identified value object classes.
    /// </summary>
    /// <param name="compilation">The current compilation containing the user's code.</param>
    /// <param name="classes">The class declarations identified as value objects to generate.</param>
    /// <param name="context">The source production context for adding generated source.</param>
    /// <remarks>
    /// <para>
    /// This method:
    /// <list type="number">
    /// <item>Extracts metadata (namespace, class name, base type) from each class</item>
    /// <item>Determines which template to use based on base class (RequiredGuid vs RequiredString)</item>
    /// <item>Generates the complementary partial class source code</item>
    /// <item>Adds the generated source to the compilation</item>
    /// </list>
    /// </para>
    /// <para>
    /// Generated files are named "{ClassName}.g.cs" to clearly indicate they are generated
    /// and should not be manually edited.
    /// </para>
    /// </remarks>
    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
        IEnumerable<ClassDeclarationSyntax> distinctClasses = classes.Distinct();

        List<RequiredPartialClassInfo> classesToGenerate = GetTypesToGenerate(compilation, distinctClasses, context.CancellationToken);

        foreach (var g in classesToGenerate)
        {
            var camelArg = g.ClassName.ToCamelCase();
            var classType = g.ClassBase switch
            {
                "RequiredGuid" => "Guid",
                "RequiredString" => "string",
                "RequiredInt" => "int",
                "RequiredDecimal" => "decimal",
                _ => null
            };

            // Skip unsupported base types and emit a diagnostic to inform the user
            if (classType is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "FDDD001",
                        title: "Unsupported base type for RequiredPartialClassGenerator",
                        messageFormat: "Class '{0}' inherits from unsupported base type '{1}'. Supported bases: RequiredGuid, RequiredString, RequiredInt, RequiredDecimal.",
                        category: "SourceGenerator",
                        DiagnosticSeverity.Warning,
                        isEnabledByDefault: true),
                    location: null,
                    g.ClassName,
                    g.ClassBase));
                continue;
            }

            // Build up the source code
            // Note: The base class is already declared in the user's partial class.
            // We only generate the additional members and interfaces.
            var source = $@"// <auto-generated/>
    namespace {g.NameSpace};
    using FunctionalDdd;
    using FunctionalDdd.PrimitiveValueObjects;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json.Serialization;

    #nullable enable
    [JsonConverter(typeof(ParsableJsonConverter<{g.ClassName}>))]
    {g.Accessibility.ToCamelCase()} partial class {g.ClassName} : IScalarValue<{g.ClassName}, {classType}>, IParsable<{g.ClassName}>
    {{
        private {g.ClassName}({classType} value) : base(value)
        {{
        }}

        public static explicit operator {g.ClassName}({classType} {camelArg}) => TryCreate({camelArg}).Value;

        public static {g.ClassName} Parse(string s, IFormatProvider? provider)
        {{
            var r = TryCreate(s, null);
            if (r.IsFailure)
            {{
                var val = (ValidationError)r.Error;
                throw new FormatException(val.FieldErrors[0].Details[0]);
            }}
            return r.Value;
        }}

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out {g.ClassName} result)
        {{
            var r = TryCreate(s, null);
            if (r.IsFailure)
            {{
                result = default;
                return false;
            }}

            result = r.Value;
            return true;
        }}";

            if (g.ClassBase == "RequiredGuid")
            {
                source += $@"

        public static {g.ClassName} NewUnique() => new(Guid.NewGuid());

        /// <summary>
        /// Creates a validated instance from a Guid.
        /// Required by IScalarValue interface for model binding and JSON deserialization.
        /// </summary>
        /// <param name=""value"">The Guid value to validate.</param>
        /// <param name=""fieldName"">Optional field name for validation error messages.</param>
        /// <returns>Success with the value object, or Failure with validation errors.</returns>
        public static Result<{g.ClassName}> TryCreate(Guid value, string? fieldName = null)
        {{
            using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(""{g.ClassName}.TryCreate"");
            var field = !string.IsNullOrEmpty(fieldName)
                ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
                : ""{g.ClassName.ToCamelCase()}"";
            if (value == Guid.Empty)
                return Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be empty."", field);
            return new {g.ClassName}(value);
        }}

        public static Result<{g.ClassName}> TryCreate(Guid? requiredGuidOrNothing, string? fieldName = null)
        {{
            using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(""{g.ClassName}.TryCreate"");
            var field = !string.IsNullOrEmpty(fieldName)
                ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
                : ""{g.ClassName.ToCamelCase()}"";
            return requiredGuidOrNothing
                .ToResult(Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be empty."", field))
                .Ensure(x => x != Guid.Empty, Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be empty."", field))
                .Map(guid => new {g.ClassName}(guid));
        }}

        public static Result<{g.ClassName}> TryCreate(string? stringOrNull, string? fieldName = null)
        {{
            using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(""{g.ClassName}.TryCreate"");
            var field = !string.IsNullOrEmpty(fieldName)
                ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
                : ""{g.ClassName.ToCamelCase()}"";
            Guid parsedGuid = Guid.Empty;
            return stringOrNull
                .ToResult(Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be empty."", field))
                .Ensure(x => Guid.TryParse(x, out parsedGuid), Error.Validation(""Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)"", field))
                .Ensure(_ => parsedGuid != Guid.Empty, Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be empty."", field))
                .Map(guid => new {g.ClassName}(parsedGuid));
        }}
    }}";
            }

            if (g.ClassBase == "RequiredString")
            {
                source += $@"

        /// <summary>
        /// Creates a validated instance from a string.
        /// Required by IScalarValue interface for model binding and JSON deserialization.
        /// </summary>
        /// <param name=""value"">The string value to validate.</param>
        /// <param name=""fieldName"">Optional field name for validation error messages.</param>
        /// <returns>Success with the value object, or Failure with validation errors.</returns>
        public static Result<{g.ClassName}> TryCreate(string? value, string? fieldName = null)
        {{
            using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(""{g.ClassName}.TryCreate"");
            var field = !string.IsNullOrEmpty(fieldName)
                ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
                : ""{g.ClassName.ToCamelCase()}"";
            return value
                .EnsureNotNullOrWhiteSpace(Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be empty."", field))
                .Map(str => new {g.ClassName}(str));
        }}
    }}";
            }

            if (g.ClassBase == "RequiredInt")
            {
                source += $@"

        /// <summary>
        /// Creates a validated instance from an integer.
        /// Required by IScalarValue interface for model binding and JSON deserialization.
        /// </summary>
        /// <param name=""value"">The integer value to validate.</param>
        /// <param name=""fieldName"">Optional field name for validation error messages.</param>
        /// <returns>Success with the value object, or Failure with validation errors.</returns>
        public static Result<{g.ClassName}> TryCreate(int value, string? fieldName = null)
        {{
            using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(""{g.ClassName}.TryCreate"");
            var field = !string.IsNullOrEmpty(fieldName)
                ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
                : ""{g.ClassName.ToCamelCase()}"";
            if (value == 0)
                return Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be zero."", field);
            return new {g.ClassName}(value);
        }}

        public static Result<{g.ClassName}> TryCreate(int? valueOrNothing, string? fieldName = null)
        {{
            using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(""{g.ClassName}.TryCreate"");
            var field = !string.IsNullOrEmpty(fieldName)
                ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
                : ""{g.ClassName.ToCamelCase()}"";
            return valueOrNothing
                .ToResult(Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be empty."", field))
                .Ensure(x => x != 0, Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be zero."", field))
                .Map(val => new {g.ClassName}(val));
        }}

        public static Result<{g.ClassName}> TryCreate(string? stringOrNull, string? fieldName = null)
        {{
            using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(""{g.ClassName}.TryCreate"");
            var field = !string.IsNullOrEmpty(fieldName)
                ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
                : ""{g.ClassName.ToCamelCase()}"";
            int parsedInt = 0;
            return stringOrNull
                .ToResult(Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be empty."", field))
                .Ensure(x => int.TryParse(x, out parsedInt), Error.Validation(""Value must be a valid integer."", field))
                .Ensure(_ => parsedInt != 0, Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be zero."", field))
                .Map(_ => new {g.ClassName}(parsedInt));
        }}
    }}";
            }

            if (g.ClassBase == "RequiredDecimal")
            {
                source += $@"

        /// <summary>
        /// Creates a validated instance from a decimal.
        /// Required by IScalarValue interface for model binding and JSON deserialization.
        /// </summary>
        /// <param name=""value"">The decimal value to validate.</param>
        /// <param name=""fieldName"">Optional field name for validation error messages.</param>
        /// <returns>Success with the value object, or Failure with validation errors.</returns>
        public static Result<{g.ClassName}> TryCreate(decimal value, string? fieldName = null)
        {{
            using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(""{g.ClassName}.TryCreate"");
            var field = !string.IsNullOrEmpty(fieldName)
                ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
                : ""{g.ClassName.ToCamelCase()}"";
            if (value == 0m)
                return Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be zero."", field);
            return new {g.ClassName}(value);
        }}

        public static Result<{g.ClassName}> TryCreate(decimal? valueOrNothing, string? fieldName = null)
        {{
            using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(""{g.ClassName}.TryCreate"");
            var field = !string.IsNullOrEmpty(fieldName)
                ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
                : ""{g.ClassName.ToCamelCase()}"";
            return valueOrNothing
                .ToResult(Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be empty."", field))
                .Ensure(x => x != 0m, Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be zero."", field))
                .Map(val => new {g.ClassName}(val));
        }}

        public static Result<{g.ClassName}> TryCreate(string? stringOrNull, string? fieldName = null)
        {{
            using var activity = PrimitiveValueObjectTrace.ActivitySource.StartActivity(""{g.ClassName}.TryCreate"");
            var field = !string.IsNullOrEmpty(fieldName)
                ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
                : ""{g.ClassName.ToCamelCase()}"";
            decimal parsedDecimal = 0m;
            return stringOrNull
                .ToResult(Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be empty."", field))
                .Ensure(x => decimal.TryParse(x, out parsedDecimal), Error.Validation(""Value must be a valid decimal."", field))
                .Ensure(_ => parsedDecimal != 0m, Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be zero."", field))
                .Map(_ => new {g.ClassName}(parsedDecimal));
        }}
    }}";
            }

            context.AddSource($"{g.ClassName}.g.cs", source);
        }
    }

    /// <summary>
    /// Extracts metadata from class declarations to determine which classes need code generation.
    /// </summary>
    /// <param name="compilation">The compilation containing semantic information.</param>
    /// <param name="classes">The candidate class declarations.</param>
    /// <param name="cancellationToken">Cancellation token to observe.</param>
    /// <returns>
    /// A list of <see cref="RequiredPartialClassInfo"/> containing metadata for each class to generate.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method uses semantic analysis to extract:
    /// <list type="bullet">
    /// <item>Fully-qualified namespace</item>
    /// <item>Class name</item>
    /// <item>Base class name (RequiredGuid or RequiredString)</item>
    /// <item>Accessibility level (public, internal, etc.)</item>
    /// </list>
    /// </para>
    /// <para>
    /// The method respects cancellation to avoid blocking the IDE during long compilations.
    /// </para>
    /// </remarks>
    private static List<RequiredPartialClassInfo> GetTypesToGenerate(Compilation compilation, IEnumerable<ClassDeclarationSyntax> classes, CancellationToken cancellationToken)
    {
        var classToGenerate = new List<RequiredPartialClassInfo>();

        foreach (var classDeclarationSyntax in classes)
        {
            // stop if we're asked to
            cancellationToken.ThrowIfCancellationRequested();

            INamedTypeSymbol? classSymbol = compilation
                .GetSemanticModel(classDeclarationSyntax.SyntaxTree)
                .GetDeclaredSymbol(classDeclarationSyntax, cancellationToken) as INamedTypeSymbol;

            if (classSymbol == null) continue;

            string className = classSymbol.Name;
            string @namespace = classSymbol.ContainingNamespace.ToDisplayString();
            // Extract just the base class name without type parameters (e.g., "RequiredGuid" from "RequiredGuid<EmployeeId>")
            string @base = classSymbol.BaseType?.Name ?? "unknown";
            string accessibility = classSymbol.DeclaredAccessibility.ToString();
            classToGenerate.Add(new RequiredPartialClassInfo(@namespace, className, @base, accessibility));
        }

        return classToGenerate;
    }

    /// <summary>
    /// Fast syntax-only filter to identify potential value object class declarations.
    /// </summary>
    /// <param name="node">The syntax node to examine.</param>
    /// <returns>
    /// <c>true</c> if the node is a partial class inheriting from RequiredGuid or RequiredString;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This predicate is called for every syntax node in the compilation, so it must be fast.
    /// It uses only syntactic analysis (no semantic model) to quickly filter candidates.
    /// </para>
    /// <para>
    /// The method checks:
    /// <list type="number">
    /// <item>Is it a class declaration?</item>
    /// <item>Does it have a base type list?</item>
    /// <item>Is the first base type named "RequiredGuid" or "RequiredString"?</item>
    /// </list>
    /// </para>
    /// <para>
    /// Note: This is a syntactic check only. Semantic validation happens later in <see cref="GetTypesToGenerate"/>.
    /// </para>
    /// </remarks>
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        if (node is ClassDeclarationSyntax c && c.BaseList != null)
        {
            var baseType = c.BaseList.Types.FirstOrDefault();
            var nameOfFirstBaseType = baseType?.Type.ToString();

            // Support both old names and new generic names
            // RequiredString<ClassName> or RequiredString (for backwards compat during migration)
            if (nameOfFirstBaseType != null && nameOfFirstBaseType.StartsWith("RequiredString", StringComparison.Ordinal))
                return true;
            if (nameOfFirstBaseType != null && nameOfFirstBaseType.StartsWith("RequiredGuid", StringComparison.Ordinal))
                return true;
            if (nameOfFirstBaseType != null && nameOfFirstBaseType.StartsWith("RequiredInt", StringComparison.Ordinal))
                return true;
            if (nameOfFirstBaseType != null && nameOfFirstBaseType.StartsWith("RequiredDecimal", StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}
