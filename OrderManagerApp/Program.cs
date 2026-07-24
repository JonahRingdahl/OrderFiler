using Microsoft.EntityFrameworkCore;
using OrderManagerApp.Models;
using OrderManagerApp.Pages;

namespace OrderManagerApp;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {

        using (var db = new OrderContext())
        {
            db.Database.Migrate(); 
        }
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new HomePage());
    }
}
