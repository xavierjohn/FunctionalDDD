namespace Trellis.EntityFrameworkCore;

using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
/// Extension methods for <see cref="ModelBuilder"/> that register EF Core value converters
/// for Trellis primitive value objects.
/// </summary>
public static class ModelBuilderExtensions
{
    private static readonly ConcurrentDictionary<Type, ValueConverter?> s_converterCache = new();

    /// <summary>
    /// Scans the EF Core model for scalar properties whose CLR types implement
    /// <see cref="IScalarValue{TSelf, TPrimitive}"/> or derive from <see cref="RequiredEnum{TSelf}"/>
    /// and sets the appropriate <see cref="ValueConverter"/> on any that lack one.
    /// Properties already configured with <c>HasConversion</c> are skipped.
    /// <para>
    /// <b>Preferred approach:</b> Use
    /// <see cref="ModelConfigurationBuilderExtensions.ApplyTrellisConventions"/>
    /// in <c>ConfigureConventions</c> instead. That registers type-level converters
    /// before EF Core's convention engine runs, eliminating all <c>HasConversion</c>
    /// boilerplate.
    /// </para>
    /// <para>
    /// This method remains useful as a safety net when mixing convention-based and
    /// manual property registration. Call it at the END of <c>OnModelCreating</c>
    /// so that manual <c>HasConversion</c> calls take precedence.
    /// </para>
    /// </summary>
    /// <param name="modelBuilder">The model builder to configure.</param>
    /// <returns>The same <see cref="ModelBuilder"/> for chaining.</returns>
    public static ModelBuilder ApplyTrellisValueConverters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                // Skip properties that already have a converter configured
                // (manual HasConversion takes precedence)
                if (property.GetValueConverter() is not null)
                    continue;

                var clrType = property.ClrType;
                var converter = GetOrCreateConverter(clrType);
                if (converter is not null)
                    property.SetValueConverter(converter);
            }
        }

        return modelBuilder;
    }

    private static ValueConverter? GetOrCreateConverter(Type clrType) =>
        s_converterCache.GetOrAdd(clrType, static type =>
        {
            var baseInfo = TrellisTypeScanner.FindTrellisBase(type);
            if (baseInfo is null)
                return null;

            var (providerType, isEnum) = baseInfo.Value;
            if (isEnum)
            {
                var converterType = typeof(TrellisEnumConverter<>).MakeGenericType(type);
                return (ValueConverter)Activator.CreateInstance(converterType)!;
            }
            else
            {
                var converterType = typeof(TrellisScalarConverter<,>).MakeGenericType(type, providerType);
                return (ValueConverter)Activator.CreateInstance(converterType)!;
            }
        });
}
