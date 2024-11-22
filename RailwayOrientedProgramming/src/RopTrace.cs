namespace FunctionalDdd;

using System.Diagnostics;
using System.Reflection;

internal static class RopTrace
{
    internal static readonly AssemblyName AssemblyName = typeof(RopTrace).Assembly.GetName();
    internal static readonly string ActivitySourceName = "Functional DDD ROP";
    internal static readonly Version Version = AssemblyName.Version!;
    internal static readonly ActivitySource ActivitySource = new(ActivitySourceName, Version.ToString());
}
