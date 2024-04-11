namespace FunctionalDdd;

using System.Diagnostics;
using System.Reflection;

internal class Trace
{
    internal static readonly AssemblyName AssemblyName = typeof(Trace).Assembly.GetName();
    internal static readonly string ActivitySourceName = AssemblyName.Name!;
    internal static readonly Version Version = AssemblyName.Version!;
    internal static readonly ActivitySource ActivitySource = new(ActivitySourceName, Version.ToString());
}
