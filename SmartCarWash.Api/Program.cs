
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Application.Mappings;
using SmartCarWash.Application.Services;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Interfaces;
using SmartCarWash.Infrastructure.Data;
using SmartCarWash.Infrastructure.Repositories;
using System;

namespace SmartCarWash.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đăng ký Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Đăng ký Repository
            builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();

            // Đăng ký Service
            builder.Services.AddScoped<IVehicleService, VehicleService>();

            // Add services to the container.
            builder.Services.AddControllers();

            //Add db
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


            //Add Identity services, chỗ này là đăng ký để ASP.Net tự DI dùm ở chỗ AuthService
            builder.Services.AddIdentity<User, IdentityRole<Guid>>()
                            .AddEntityFrameworkStores<AppDbContext>()
                            .AddDefaultTokenProviders();

            //Add AutoMapper
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            builder.Services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();

            //seed data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();

                    // 1. Tự động chạy Migration (Tạo bảng trên Neon nếu chưa có)
                    await context.Database.MigrateAsync();

                    // 2. Chạy SeedData
                    var seedData = new SeedData();
                    await seedData.InitializeAsync(services);

                    Console.WriteLine("Database Migration & Seed completed successfully!");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
                }
            }

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            //environment variable for port, default to 8080 if not set
            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            app.Run($"http://0.0.0.0:{port}");

            //chạy test local thì dùng cái này cho nhanh, chạy trên server thì dùng cái trên
            //app.Run();
        }
    }
}
