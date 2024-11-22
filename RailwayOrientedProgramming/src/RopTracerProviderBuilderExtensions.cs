namespace FunctionalDdd;

using OpenTelemetry.Trace;

public static class RopTracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddFunctionalDddRopInstrumentation(this TracerProviderBuilder builder)
        => builder.AddSource(RopTrace.ActivitySourceName);
}
