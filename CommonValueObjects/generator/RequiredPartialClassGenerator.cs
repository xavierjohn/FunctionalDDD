﻿namespace SourceGenerator
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading;
    using FunctionalDDD;
    using FunctionalDDD.CommonValueObjectGenerator;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [Generator(LanguageNames.CSharp)]
    public class RequiredPartialClassGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<ClassDeclarationSyntax> requiredGuids = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (n, _) => IsSyntaxTargetForGeneration(n),
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx));

            IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndEnums
                = context.CompilationProvider.Combine(requiredGuids.Collect());

            context.RegisterSourceOutput(compilationAndEnums,
                static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
        {
            if (classes.IsDefaultOrEmpty)
            {
                // nothing to do yet
                return;
            }

            // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
            IEnumerable<ClassDeclarationSyntax> distinctClasses = classes.Distinct();

            List<RequiredPartialClassInfo> classesToGenerate = GetTypesToGenerate(compilation, distinctClasses, context.CancellationToken);

            foreach (var g in classesToGenerate)
            {
                var camelArg = g.ClassName.ToCamelCase();
                // Build up the source code
                var source = $@"// <auto-generated/>
namespace {g.NameSpace};
using FunctionalDDD;
{g.Accessibility.ToCamelCase()} partial class {g.ClassName} : Required{g.ClassType}<{g.ClassName}>
{{
    protected static readonly Error CannotBeEmptyError = Error.Validation(""{g.ClassName.SplitPascalCase()} cannot be empty"", ""{g.ClassName.ToCamelCase()}"");

    private {g.ClassName}({g.ClassType} value) : base(value)
    {{
    }}

    public static explicit operator {g.ClassName}({g.ClassType} {camelArg}) => Create({camelArg}).Value;
";

                if (g.ClassType == "Guid")
                {
                    source += $@"
    public static {g.ClassName} CreateUnique() => new(Guid.NewGuid());

    public static Result<{g.ClassName}> Create(Maybe<Guid> requiredGuidOrNothing)
    {{
        return requiredGuidOrNothing
            .ToResult(CannotBeEmptyError)
            .Ensure(x => x != Guid.Empty, CannotBeEmptyError)
            .Map(guid => new {g.ClassName}(guid));
    }}
}}
";
                }

                if (g.ClassType == "String")
                {
                    source += $@"
    public static Result<{g.ClassName}> Create(Maybe<string> requiredStringOrNothing)
    {{
        return requiredStringOrNothing
            .EnsureNotNullOrWhiteSpace(CannotBeEmptyError)
            .Map(str => new {g.ClassName}(str));
    }}
}}
";
                }

                context.AddSource($"{g.ClassName}.g.cs", source);
            }
        }

        private static List<RequiredPartialClassInfo> GetTypesToGenerate(Compilation compilation, IEnumerable<ClassDeclarationSyntax> classes, CancellationToken cancellationToken)
        {
            var requiredGuidStr = "FunctionalDDD.RequiredGuid`1";
            var requiredStringStr = "FunctionalDDD.RequiredString`1";
            var classToGenerate = new List<RequiredPartialClassInfo>();
            bool nothingToDo = compilation.GetTypeByMetadataName(requiredGuidStr) == null && compilation.GetTypeByMetadataName(requiredStringStr) == null;
            if (nothingToDo)
                return classToGenerate;

            foreach (var classDeclarationSyntax in classes)
            {
                // stop if we're asked to
                cancellationToken.ThrowIfCancellationRequested();

                SemanticModel semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax, cancellationToken) is not INamedTypeSymbol classSymbol)
                {
                    // something went wrong
                    continue;
                }

                string className = classSymbol.Name;
                string @namespace = classSymbol.ContainingNamespace.ToString();
                string @base = classSymbol.BaseType?.Name ?? "unknown";
                string classType = classSymbol.BaseType?.BaseType?.TypeArguments[0].Name ?? "unknown";
                string accessibility = classSymbol.DeclaredAccessibility.ToString();
                classToGenerate.Add(new RequiredPartialClassInfo(@namespace, className, @base, classType, accessibility));
            }

            return classToGenerate;
        }

        static ClassDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            // we know the node is a ClassDeclarationSyntax thanks to IsSyntaxTargetForGeneration
            return (ClassDeclarationSyntax)context.Node;
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        {
            if (node is ClassDeclarationSyntax c && c.BaseList != null)
            {
                var baseType = c.BaseList.Types.FirstOrDefault();
                var nameOfFirstBaseType = baseType?.Type.ToString();

                if (nameOfFirstBaseType == "RequiredString<" + c.Identifier.ValueText + ">")
                    return true;
                if (nameOfFirstBaseType == "RequiredGuid<" + c.Identifier.ValueText + ">")
                    return true;
            }

            return false;
        }
    }
}
