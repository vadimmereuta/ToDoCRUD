using ToDoCRUD.Client.Pages;
using ToDoCRUD.Components;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace ToDoCRUD
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=app.db"));

            // Make HttpContext available and register an HttpClient whose BaseAddress
            // is set from the current request (so server-side activation/prerendering
            // uses the correct origin and won't try to call a hard-coded port).
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped(sp =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var request = httpContextAccessor.HttpContext?.Request;
                var baseAddress = request is not null
                    ? new Uri($"{request.Scheme}://{request.Host.Value}/")
                    : new Uri(builder.Configuration["ServerBaseAddress"] ?? "https://localhost:5001/");

                return new HttpClient { BaseAddress = baseAddress };
            });

            // Register MVC controllers so API endpoints (e.g. /api/TodoItems) are available
            builder.Services.AddControllers();

            // Register antiforgery services (required for interactive components endpoints)
            builder.Services.AddAntiforgery();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveWebAssemblyComponents();

            var app = builder.Build();

            // Apply migrations at startup
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Routing + auth middleware
            app.UseRouting();
            app.UseAuthorization();

            // Antiforgery middleware must be registered after routing/auth and before endpoint mapping
            app.UseAntiforgery();

            // Map controllers and component endpoints
            app.MapControllers();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.Run();
        }
    }
}
