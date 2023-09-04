using LittleBitHelperExpenseTracker.Data;
using LittleBitHelperExpenseTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static LittleBitHelperExpenseTracker.Models.JsonOperations;



namespace LittleBitHelperExpenseTracker
{
    internal class Program
    {
        protected Program()
        {

        }

        public static Settings? Default { get; set; }

        public static async Task Main(string[] args)
        {
            Console.Title = "LittleBitHelperExpenseTrackerApp";
            var builder = WebApplication.CreateBuilder(args);
            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddRazorPages();
            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapRazorPages();
            Default = builder.Configuration.Get<Settings>();
            if (Default != null)
            {
                if (Default.InitialConsoleOutputColor == "Red")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if (Default.InitialConsoleOutputColor == "Green")
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                await Console.Out.WriteLineAsync(Default.InitialConsoleOutputColor);
            }

            await JsonCheckAndUpdate();
            app.Run();
        }
    }
}