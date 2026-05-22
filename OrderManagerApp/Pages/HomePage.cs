using System.ComponentModel;
using System.Drawing.Printing;
using System.Text;
using OrderManagerApp.Pages;
using Microsoft.EntityFrameworkCore;
using OrderManagerApp.Models;

namespace OrderManagerApp.Pages;

public partial class HomePage : Form
{
    private Order? selectedOrder = null;
    private string printData = string.Empty;
    private int printIndex = 0;
    private readonly Font printFont = new("Arial", 12);
    private BindingList<Order> openOrders = [];
    private BindingList<Order> closedOrders = [];

    public HomePage()
    {
        InitializeComponent();
        openGridView.DataSource = openOrders;
        closedGridView.DataSource = closedOrders;

    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);

        await CullOrders();
        await LoadOrders();

        UpdateTotalOrdersButton_Clicked(null,null);
    }


    // Orders marked for deletion will be culled after two weeks
    private static async Task CullOrders()
    {
        using OrderContext ctx = new();
        var markedOrders = OrderContext.GetClosedOrdersAsync(ctx);
        List<Order> deleteOrders = [];

        await foreach (var order in markedOrders)
        {
            if (DateTime.Now - order.UpdatedDate.Date > TimeSpan.FromDays(14))
                deleteOrders.Add(order);
        }

        if (deleteOrders.Count <= 0) return;

        var choice = MessageBox.Show("Delete two week old orders?", "Orders to be deleted", MessageBoxButtons.OKCancel);
        if (choice != DialogResult.OK) return;

        ctx.Orders.RemoveRange(deleteOrders);
        await ctx.SaveChangesAsync();
    }

    private async void ClearButton_Clicked(object sender, EventArgs e)
    {
        await LoadOrders();
        searchBox.Text = string.Empty;
    }

    private async Task LoadOrders()
    {
        using OrderContext ctx = new();
        await DisplayManyOpenOrders(OrderContext.GetOpenOrdersAsync(ctx));
        await DisplayManyClosedOrders(OrderContext.GetClosedOrdersAsync(ctx));
    }

    private async void PoButton_Clicked(object sender, EventArgs e)
    {
        var searchText = searchBox.Text;
        await FindPO(searchText);
    }

    private async void OnButton_Clicked(object sender, EventArgs e)
    {
        var success = uint.TryParse(searchBox.Text, out uint on);

        if (success) await FindSO(on);
    }

    private async Task FindSO(uint so)
    {
        using OrderContext ctx = new();

        Order? foundOrder = await ctx.Orders.FirstOrDefaultAsync(o => o.OrderNumber == so);
        if (foundOrder is null)
        {
            MessageBox.Show("Order Not Found", "Not Found", MessageBoxButtons.OK);
            return;
        }

        if (foundOrder.isDeleted)
        {
            DisplayOneClosedOrder(foundOrder);

            var selection = MessageBox.Show("Mark as not deleted?", "Order is Deleted", MessageBoxButtons.OKCancel);
            if (selection == DialogResult.OK)
                foundOrder.UndeleteOrder();

            await ctx.SaveChangesAsync();
        }
        else
        {
            DisplayOneOpenOrder(foundOrder);
        }
    }

    private async Task FindPO(string po)
    {
        using OrderContext ctx = new();

        IAsyncEnumerable<Order> foundOpenOrders = OrderContext.GetOpenOrdersAsync(ctx).Where(order => order.PoNumber.Contains(po));
        var foundClosedOrders = OrderContext.GetClosedOrdersAsync(ctx).Where(order => order.PoNumber.Contains(po));

        await DisplayManyOpenOrders(foundOpenOrders);
        await DisplayManyClosedOrders(foundClosedOrders);
    }
    private async void PrintButton_Clicked(object sender, EventArgs e)
    {
        if (new PrintDialog().ShowDialog() != DialogResult.OK) return;

        await LoadPrinterDataAsync();

        PrintDocument doc = new();
        doc.PrintPage += PrintPageHandler;
        printIndex = 0;

        try { doc.Print(); }
        catch (Exception error)
        {
            MessageBox.Show(
                $"Error while printing: {error.Message}",
                "Printing Error",
                MessageBoxButtons.OK
            );
        }
    }

    private async Task LoadPrinterDataAsync()
    {
        using var ctx = new OrderContext();
        var builder = new StringBuilder();
        await foreach (var order in OrderContext.GetOpenOrdersAsync(ctx))
            builder.Append(order.DisplayOrder());

        printData = builder.ToString();
    }

    private void PrintPageHandler(object sender, PrintPageEventArgs e)
    {
        string remaining = printData[printIndex..];
        int charsFitted = 0;

        e.Graphics?.MeasureString(
            remaining,
            printFont,
            e.MarginBounds.Size,
            StringFormat.GenericTypographic,
            out charsFitted,
            out _
        );

        e.Graphics?.DrawString(
            remaining,
            printFont,
            Brushes.Black,
            e.MarginBounds,
            StringFormat.GenericTypographic
        );

        printIndex += charsFitted;
        e.HasMorePages = printIndex < printData.Length;

        if (!e.HasMorePages) printIndex = 0;
    }

    private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        var searchText = searchBox.Text;

        if (e.KeyCode is not Keys.Enter)
            return;

        bool isOn = uint.TryParse(searchText, out uint searchNum);

        if (isOn) 
            await FindSO(searchNum);
        else
            await FindPO(searchText);
    }

    private void AddButton_Clicked(object sender, EventArgs e)
    {
        var orderPage = new OrderPage(null, this);
        orderPage.Show();

        orderPage.OrderSaved += async (obj, order) =>
        {
            using OrderContext ctx = new();
            ctx.Orders.Add(order);
            await ctx.SaveChangesAsync();
            openOrders.Clear();
            await LoadOrders();
            selectedOrder = null;
        };
    }

    private void UpdateButton_Clicked(object sender, EventArgs e)
    {
        if (selectedOrder is null) return;

        var orderPage = new OrderPage(selectedOrder, this);
        orderPage.Show();

        orderPage.OrderSaved += async (obj, order) =>
        {
            var context = new OrderContext();
            order.isDeleted = false;
            context.Orders.Update(order);
            await context.SaveChangesAsync();
            await LoadOrders();
            selectedOrder = null;
        };
    }

    private async void DeleteButton_Clicked(object sender, EventArgs e)
    {
        if (selectedOrder is not null)
        {
        using OrderContext ctx = new();
            selectedOrder.isDeleted = true;
            ctx.Orders.Update(selectedOrder);
            await ctx.SaveChangesAsync();
            await LoadOrders();
        }
        else
        {
            MessageBox.Show("There is no selected Order", "Select an Order", MessageBoxButtons.OK);
        }

    }

    private void OpenGridView_SelectionChanged(object sender, EventArgs e)
    {
        var selectedCellCount = openGridView.GetCellCount(DataGridViewElementStates.Selected);

        if (selectedCellCount <= 0) return;

        var selectedRow = openGridView.SelectedRows;

        if (selectedRow.Count <= 0) return;

        selectedOrder = selectedRow[0].DataBoundItem as Order;

        if (closedGridView.SelectedRows.Count > 0) closedGridView.ClearSelection();
    }

    private void ClosedGridView_SelectionChanged(object sender, EventArgs e)
    {
        var selectedCellCount = closedGridView.GetCellCount(DataGridViewElementStates.Selected);

        if (selectedCellCount <= 0) return;

        var selectedRow = closedGridView.SelectedRows;

        if (selectedRow.Count <= 0) return;

        selectedOrder = selectedRow[0].DataBoundItem as Order;

        if (openGridView.SelectedRows.Count > 0) openGridView.ClearSelection();
    }

    private void QuitButton_Clicked(object sender, EventArgs e) => Environment.Exit(0);

    private void UpdateTotalOrdersButton_Clicked(object? sender, EventArgs? e)
    {
        totalOrdersBox.Text = $"Number of Orders: {openOrders.Count}";
        totalClosedOrderBox.Text = $"Number of Closed Orders: {closedOrders.Count}";

    }

    private void DisplayOneOpenOrder(Order foundOrder)
    {
        openOrders.Clear();
        openOrders.Add(foundOrder);
        closedOrders.Clear();
    }

    private void DisplayOneClosedOrder(Order foundOrder)
    {
        closedOrders.Clear();
        closedOrders.Add(foundOrder);
        openOrders.Clear();
    }

    private async Task DisplayManyOpenOrders(IAsyncEnumerable<Order> orders)
    {
        openOrders.Clear();
        await foreach (var order in orders)
            openOrders.Add(order);
        
    }
    
    private async Task DisplayManyClosedOrders(IAsyncEnumerable<Order> orders)
    {
        closedOrders.Clear();
        await foreach(var order in orders)
            closedOrders.Add(order);
    }
}
