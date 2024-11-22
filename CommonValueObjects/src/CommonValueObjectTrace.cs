namespace FunctionalDdd;

using System.Diagnostics;
using System.Reflection;

public static class CommonValueObjectTrace
{
    internal static readonly AssemblyName AssemblyName = typeof(Trace).Assembly.GetName();
    internal static readonly string ActivitySourceName = "Functional DDD Common Value Object";
    internal static readonly Version Version = AssemblyName.Version!;
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName, Version.ToString());
}
