using Microsoft.EntityFrameworkCore;

namespace OrderManagerApp.Models;

public class OrderContext() : DbContext
{
    private const string _db_name = "orders.db";
    private const string _project_name = "OrderManager";

    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        string db_path = Path.Combine(path, _project_name, _db_name);

        optionsBuilder.UseSqlite($"Data Source={db_path}");
    }

    public static IAsyncEnumerable<Order> GetOpenOrdersAsync(OrderContext ctx, ShippingMethod? method = null, bool descending = false)
    {
        var openOrders = ctx.Orders.AsNoTracking().Where(OrderFilterNotDeleted);
        if (method is not null)
            openOrders = openOrders.Where(order => order.Method == method);

        openOrders = descending ?
        openOrders.OrderByDescending(OrderNumberOrderingFunc):
        openOrders.OrderBy(OrderNumberOrderingFunc);

        return openOrders.ToAsyncEnumerable();
    }

    public static IAsyncEnumerable<Order> GetClosedOrdersAsync(OrderContext ctx, ShippingMethod? method = null, bool descending = false)
    {
        var closedOrders = ctx.Orders.AsNoTracking().Where(OrderFilterIsDeleted);

        if (method is not null)
            closedOrders = closedOrders.Where(order => order.Method == method);

        closedOrders = descending ?
        closedOrders.OrderByDescending(OrderNumberOrderingFunc):
        closedOrders.OrderBy(OrderNumberOrderingFunc);

        return closedOrders.ToAsyncEnumerable();
    }

    public static IAsyncEnumerable<Order> GetAllOrdersAsync(OrderContext ctx, bool descending = false)
    {
        var order = ctx.Orders.AsNoTracking();

        var orderedOrders = descending ?
        order.OrderByDescending(OrderNumberOrderingFunc):
        order.OrderBy(OrderNumberOrderingFunc);

        return orderedOrders.ToAsyncEnumerable();
    }

    public static IAsyncEnumerable<IGrouping<object, Order>> GetOrdersAsync(OrderContext ctx,  
                                            Func<Order, bool> orderFilter, 
                                            bool descending = true,
                                            bool groupByMethod = false)
    {
        var orders = ctx.Orders.AsNoTracking().OrderBy(orderFilter);

        var orderedOrders = descending ?
            orders.OrderByDescending(orderFilter):
            orders.OrderBy(orderFilter);

        Func<Order, object> selector =  groupByMethod 
                ? o => o.Method
                : o => 1;

        var grouped = orderedOrders.GroupBy(selector);

        return grouped.ToAsyncEnumerable();
    }

    public static IAsyncEnumerable<Order> PrintingOrders(OrderContext ctx)
    {
        var availableOrders = ctx.Orders.Where(o => o.isDeleted == false). AsNoTracking();

        var cpup = availableOrders.Where(o => o.Method == ShippingMethod.CPUP).OrderBy(OrderNumberOrderingFunc);
        var backorder = availableOrders.Where(o => o.Method == ShippingMethod.BACKORDER).OrderBy(OrderNumberOrderingFunc);
        var shipping = availableOrders.Where(o => o.Method == ShippingMethod.SHIPPING).OrderBy(OrderNumberOrderingFunc);

        var order = cpup.Concat(backorder).Concat(shipping);

        return order.ToAsyncEnumerable();
    }

    public static bool OrderFilterNotDeleted(Order order) => !order.isDeleted;
    public static bool OrderFilterIsDeleted(Order order) => order.isDeleted;
    public static uint OrderNumberOrderingFunc(Order order) => order.OrderNumber;
}