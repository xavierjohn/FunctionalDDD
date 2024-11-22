namespace FunctionalDdd;

using OpenTelemetry.Trace;

public static class CommonValueObjectTraceProviderBuilderExtensions
{
    public static TracerProviderBuilder AddFunctionalDddCommonValueObjectInstrumentation(this TracerProviderBuilder builder)
        => builder.AddSource(CommonValueObjectTrace.ActivitySourceName);
}
