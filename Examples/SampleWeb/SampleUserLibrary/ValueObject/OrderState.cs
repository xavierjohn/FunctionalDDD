namespace SampleUserLibrary;

using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// Enum value object for order state demonstrating RequiredEnum with ASP.NET Core.
/// </summary>
public partial class OrderState : RequiredEnum<OrderState>
{
    public static readonly OrderState Draft = new(canModify: true, canCancel: true);
    public static readonly OrderState Confirmed = new(canModify: false, canCancel: true);
    public static readonly OrderState Shipped = new(canModify: false, canCancel: false);
    public static readonly OrderState Delivered = new(canModify: false, canCancel: false);
    public static readonly OrderState Cancelled = new(canModify: false, canCancel: false);

    public bool CanModify { get; }
    public bool CanCancel { get; }

    private OrderState(bool canModify, bool canCancel)
    {
        CanModify = canModify;
        CanCancel = canCancel;
    }
}
