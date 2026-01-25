using System.Diagnostics.CodeAnalysis;

// Suppress style warnings for benchmark methods
// Benchmark method names use underscores for readability (e.g., Bind_SingleChain_Success)
[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Benchmark method names use underscores for clarity and readability", Scope = "namespaceanddescendants", Target = "~N:Benchmark")]

// Expression body suggestions - benchmark methods are intentionally written in block form for clarity
[assembly: SuppressMessage("Style", "IDE0022:Use expression body for method", Justification = "Benchmark methods use block bodies for consistency and clarity", Scope = "namespaceanddescendants", Target = "~N:Benchmark")]

// Lambda expression simplification - keeping explicit lambdas for benchmark clarity
[assembly: SuppressMessage("Style", "IDE0200:Remove unnecessary lambda expression", Justification = "Explicit lambdas improve benchmark readability", Scope = "namespaceanddescendants", Target = "~N:Benchmark")]

// ToString locale warnings - benchmarks measure performance, not localization
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Benchmarks focus on performance measurement, not localization", Scope = "namespaceanddescendants", Target = "~N:Benchmark")]

// Static method suggestions - benchmark methods should remain instance methods for BenchmarkDotNet
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "BenchmarkDotNet requires instance methods", Scope = "namespaceanddescendants", Target = "~N:Benchmark")]

// Array allocation suggestions - constant arrays in benchmarks are intentional for consistent testing
[assembly: SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "Constant arrays ensure consistent benchmark conditions", Scope = "namespaceanddescendants", Target = "~N:Benchmark")]

// Parentheses clarity suggestions - operator precedence is clear in benchmark context
[assembly: SuppressMessage("Style", "IDE0048:Add parentheses for clarity", Justification = "Operator precedence is clear in context", Scope = "namespaceanddescendants", Target = "~N:Benchmark")]

// Blank line suggestions
[assembly: SuppressMessage("Style", "IDE2003:Blank line required", Justification = "Formatting preferences vary", Scope = "namespaceanddescendants", Target = "~N:Benchmark")]

// Pattern matching suggestions
[assembly: SuppressMessage("Style", "IDE0078:Use pattern matching", Justification = "Explicit type checks are clearer in benchmarks", Scope = "namespaceanddescendants", Target = "~N:Benchmark")]

// Lambda expression body suggestions
[assembly: SuppressMessage("Style", "IDE0053:Use expression body for lambda expression", Justification = "Block bodies improve benchmark readability", Scope = "namespaceanddescendants", Target = "~N:Benchmark")]