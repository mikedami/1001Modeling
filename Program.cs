using _1001;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Authorization;

namespace TrackListApp{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Enable legacy timestamp behavior to handle DateTime kinds automatically
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>();

            builder.Services.AddControllersWithViews();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            
            app.MapControllers();

            app.Run();
        }
    }
}
        