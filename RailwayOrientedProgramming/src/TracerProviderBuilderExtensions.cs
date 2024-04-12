namespace FunctionalDdd;

using OpenTelemetry.Trace;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddFunctionalDddInstrumentation(this TracerProviderBuilder builder)
        => builder.AddSource(Trace.ActivitySourceName);
}
