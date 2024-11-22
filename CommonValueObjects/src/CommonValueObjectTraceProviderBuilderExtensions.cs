namespace FunctionalDdd;

using OpenTelemetry.Trace;

public static class CommonValueObjectTraceProviderBuilderExtensions
{
    public static TracerProviderBuilder AddFunctionalDddCvoInstrumentation(this TracerProviderBuilder builder)
        => builder.AddSource(CommonValueObjectTrace.ActivitySourceName);
}
