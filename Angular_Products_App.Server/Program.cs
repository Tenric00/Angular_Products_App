using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using MyApp.Data;
using MyApp.Repositories;
using MyApp.Services;

namespace Angular_Products_App.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // read config flag
            var useMock = builder.Configuration.GetValue<bool>("UseMockData");

            if (useMock)
            {
                // Register mock repository for demo/testing (no DB required)
                builder.Services.AddScoped<IProductRepository, MockProductRepository>();
            }
            else
            {
                // Update connection string in appsettings.json
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


                builder.Services.AddScoped<IProductRepository, ProductRepository>();
            }

            builder.Services.AddScoped<IProductService, ProductService>();

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();



            // register CORS (development only)
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("DevCors", policy =>
                    {
                        policy
                            .AllowAnyOrigin()   // development only; use specific origins for stricter policy
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
                });
            }


            /*
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularClient", policy =>
                {
                    policy.WithOrigins("https://localhost:55454")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                });
            });
             */

            var app = builder.Build();

            app.UseDefaultFiles();
            app.MapStaticAssets();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // Use CORS policy (development only)
            if (app.Environment.IsDevelopment())
            {
                //app.UseSwagger();
                //app.UseSwaggerUI();

                app.UseCors("DevCors");
            }

            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
