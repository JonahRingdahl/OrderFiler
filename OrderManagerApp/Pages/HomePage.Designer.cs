namespace OrderManagerApp.Pages;

partial class HomePage
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    private const int GRID_WIDTH = 500;
    private const int GRID_HEIGHT = 600;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1280, 720);
        this.Text = "Order Manager";

        searchLabel = new()
        {
            Name = "searchLabel",
            Location = new Point(20, 10),
            Text = "Search Box",
            AutoSize = true
        };
        searchBox = new()
        {
            Name = "searchBox",
            Location = new Point(20, 30)
        };
        searchBox.KeyDown += SearchBox_KeyDown;
        onButton = new()
        {
            Name = "onButton",
            Location = new Point(20, 60),
            Text = "Search Order Number",
            AutoSize = true
        };
        onButton.Click += OnButton_Clicked;

        poButton = new()
        {
            Name = "poButton",
            Location = new Point(20, 90),
            Text = "Search Purchase Order",
            AutoSize = true
        };
        poButton.Click += PoButton_Clicked;

        clearButton= new()
        {
            Name = "clearButton",
            Location = new Point(20, 120),
            Text = "Clear Search",
            AutoSize = true
        };
        clearButton.Click += ClearButton_Clicked;

        printButton = new()
        {
            Name = "printButton",
            Text = "Print Database",
            AutoSize = true,
            Location = new Point(20, 200)
        };
        printButton.Click += PrintButton_Clicked;

        addButton = new()
        {
            Name = "addButton",
            Location = new Point(20, 230),
            Text = "Add Order",
            AutoSize = true,
        };
        addButton.Click += AddButton_Clicked;

        updateButton = new()
        {
            Name = "updateButton",
            Location = new Point(20, 260),
            Text = "Update Order",
            AutoSize = true,
        };
        updateButton.Click += UpdateButton_Clicked;

        deleteButton = new()
        {
            Name = "deleteButton",
            Location = new Point(20, 290),
            Text = "Delete Button",
            AutoSize = true,
        };
        deleteButton.Click += DeleteButton_Clicked;

        quitButton = new()
        {
            Name = "quitButton",
            Location = new Point(20, 320),
            Text = "Quit",
            AutoSize = true
        };
        quitButton.Click += QuitButton_Clicked;

        updateTotalOrdersButton = new()
        {
            Name = "updateTotalOrdersButton",
            Location = new Point(20, 360),
            Text = "Update Total",
            AutoSize = true
        };
        updateTotalOrdersButton.Click += UpdateTotalOrdersButton_Clicked;

        totalOrdersBox = new()
        {
            Name = "totalOrdersBox",
            Location = new Point(20,400),
            Text = "",
            AutoSize = true
        };
        totalClosedOrderBox = new()
        {
            Name = "totalClosedOrderBox",
            Location = new Point(20, 430),
            Text = "",
            AutoSize = true
        };


        openGridView = new()
        {
            Name = "orderGridView",
            Location = new Point(200, 10),
            Size = new Size(GRID_WIDTH, GRID_HEIGHT)
        };
        openGridView.SelectionChanged += OpenGridView_SelectionChanged;

        closedGridView = new()
        {
            Name = "closedGridView",
            Location = new Point(200 + GRID_WIDTH + 50, 10),
            Size = new Size(GRID_WIDTH, GRID_HEIGHT)
        };
        closedGridView.SelectionChanged += ClosedGridView_SelectionChanged;



        Controls.Add(printButton);
        Controls.Add(searchLabel);
        Controls.Add(searchBox);
        Controls.Add(onButton);
        Controls.Add(poButton);
        Controls.Add(clearButton);
        Controls.Add(addButton);
        Controls.Add(updateButton);
        Controls.Add(deleteButton);
        Controls.Add(quitButton);

        Controls.Add(totalOrdersBox);
        Controls.Add(totalClosedOrderBox);

        Controls.Add(openGridView);
        Controls.Add(closedGridView);

        Controls.Add(updateTotalOrdersButton);
    }


    Button printButton;

    Label searchLabel;
    TextBox searchBox;
    Button onButton;
    Button poButton;
    Button clearButton;

    Button addButton;
    Button updateButton;
    Button deleteButton;
    Button quitButton;

    DataGridView openGridView;
    DataGridView closedGridView;

    Button updateTotalOrdersButton;
    Label totalOrdersBox;
    Label totalClosedOrderBox;
}
