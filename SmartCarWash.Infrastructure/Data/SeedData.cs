using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Infrastructure.Data;

namespace SmartCarWash.Infrastructure.Data
{
    public class SeedData
    {
        public async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            try
            {
                // Tự động chạy Migration nếu DB dưới Neon chưa được cập nhật
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await context.Database.MigrateAsync();
                }

                // 1. Nhóm Identity & Sản phẩm nền tảng
                await SeedRolesAsync(roleManager);
                await SeedUsersAsync(userManager);

                await SeedVehiclesAsync(context);

                Console.WriteLine("All Seed Completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during database seeding: {ex.Message}");
                throw;
            }
        }

        // 1. SEED ROLES - Đã sửa tham số nhận vào là IdentityRole<Guid>
        private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            var roles = new[] { "admin", "manager", "customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    // Đã sửa thành new IdentityRole<Guid>
                    await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role, NormalizedName = role.ToUpper() });
                }
            }
        }

        // 2. SEED USERS
        private static async Task SeedUsersAsync(UserManager<User> userManager)
        {
            if (userManager.Users.Any()) return;

            await CreateUserAsync(userManager, "carwash_admin", "admin.wash@gmail.com", "Admin@123", "Huy Admin", "admin");
            await CreateUserAsync(userManager, "station_manager", "kiet.manager@fpt.edu.vn", "Manager@123", "Tuấn Kiệt", "manager");
            await CreateUserAsync(userManager, "hoang_manager", "hoang.manager@example.com", "Manager@123", "Minh Hoàng", "manager");

            // Khách hàng rửa xe mẫu
            await CreateUserAsync(userManager, "lan_anh", "lananh@gmail.com", "Customer@123", "Nguyễn Lan Anh", "customer");
            await CreateUserAsync(userManager, "minh_quan", "minhquan@gmail.com", "Customer@123", "Trần Minh Quân", "customer");
            await CreateUserAsync(userManager, "thu_thao", "thuthao@gmail.com", "Customer@123", "Lê Thu Thảo", "customer");
            await CreateUserAsync(userManager, "quoc_bao", "quocbao@gmail.com", "Customer@123", "Phạm Quốc Bảo", "customer");
            await CreateUserAsync(userManager, "thanh_truc", "thanhtruc@gmail.com", "Customer@123", "Võ Thanh Trúc", "customer");
        }

        private static async Task CreateUserAsync(UserManager<User> userManager, string username, string email, string password, string fullName, string role)
        {
            if (await userManager.FindByNameAsync(username) == null)
            {
                var user = new User
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true,
                    DateOfBirth = DateTime.UtcNow.AddYears(-25)
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

        // ==========================================
        // 3. SEED VEHICLES (Gán ngẫu nhiên cho các Customer ở trên)
        // ==========================================
        private static async Task SeedVehiclesAsync(AppDbContext context)
        {
            if (context.Vehicles.Any()) return;

            // Lấy danh sách ID của tất cả người dùng hệ thống để làm chủ xe
            var customerIds = context.Users.Select(u => u.Id).ToList();
            if (!customerIds.Any()) return;

            var random = new Random();
            var vehicles = new List<Vehicle>
            {
                new Vehicle { LicensePlate = "30F-123.45", VehicleType = "Ô tô 4 chỗ (Sedan)", OwnerName = "Nguyễn Lan Anh", CreatedAt = DateTime.UtcNow },
                new Vehicle { LicensePlate = "51G-999.99", VehicleType = "Ô tô 7 chỗ (SUV)", OwnerName = "Trần Minh Quân", CreatedAt = DateTime.UtcNow },
                new Vehicle { LicensePlate = "29A-888.88", VehicleType = "Xe máy (Tay ga)", OwnerName = "Lê Thu Thảo", CreatedAt = DateTime.UtcNow },
                new Vehicle { LicensePlate = "30H-456.78", VehicleType = "Ô tô bán tải", OwnerName = "Phạm Quốc Bảo", CreatedAt = DateTime.UtcNow },
                new Vehicle { LicensePlate = "43C-111.11", VehicleType = "Ô tô 4 chỗ (Hatchback)", OwnerName = "Võ Thanh Trúc", CreatedAt = DateTime.UtcNow }
            };

            context.Vehicles.AddRange(vehicles);
            await context.SaveChangesAsync();
        }
    }
}