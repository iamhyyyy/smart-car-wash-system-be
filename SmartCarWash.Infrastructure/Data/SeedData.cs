using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCarWash.Domain.Entities;
using SmartCarWash.Domain.Enums;

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
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await context.Database.MigrateAsync();
                }

                // Chạy theo đúng thứ tự phụ thuộc dữ liệu
                await SeedRolesAsync(roleManager);
                await SeedTiersAsync(context);
                await SeedUsersAsync(userManager, context);
                await SeedWashServicesAsync(context);
                await SeedVehiclesAsync(context);
                await SeedPromotionsAsync(context, userManager);
                await SeedBookingsAsync(context);
                await SeedPointLogsAsync(context);
                await SeedFeedbacksAsync(context);

                Console.WriteLine("All Seed Completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during database seeding: {ex.Message}");
                throw;
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            // Nếu đã có bất kỳ role nào trong hệ thống -> Bỏ qua
            if (await roleManager.Roles.AnyAsync()) return;

            foreach (var role in new[] { "admin", "manager", "customer" })
            {
                await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role, NormalizedName = role.ToUpper() });
            }
        }

        private static readonly Guid MemberTierId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        private static async Task SeedTiersAsync(AppDbContext context)
        {
            // Nếu bảng Tiers đã có data -> Bỏ qua
            if (await context.Tiers.AnyAsync()) return;

            var now = DateTime.UtcNow;
            var tierTemplates = new List<Tier>
            {
                new() { Id = MemberTierId, Name = "Member", MinPointsRequired = 0, BookingWindowDays = 7, PriorityLevel = 1, PointMultiplier = 1.0m, PerksDescription = "Dat lich truoc 7 ngay", IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Silver", MinPointsRequired = 200, BookingWindowDays = 10, PriorityLevel = 2, PointMultiplier = 1.10m, PerksDescription = "Dat lich truoc 10 ngay, uu tien xep hang", IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Gold", MinPointsRequired = 500, BookingWindowDays = 12, PriorityLevel = 3, PointMultiplier = 1.20m, PerksDescription = "Dat lich truoc 12 ngay, uu tien cao", IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Platinum", MinPointsRequired = 1000, BookingWindowDays = 14, PriorityLevel = 4, PointMultiplier = 1.30m, PerksDescription = "Dat lich truoc 14 ngay, uu tien hang dau", IsActive = true, CreatedAt = now, UpdatedAt = now }
            };

            context.Tiers.AddRange(tierTemplates);
            await context.SaveChangesAsync();
        }

        private static async Task SeedUsersAsync(UserManager<User> userManager, AppDbContext context)
        {
            // Nếu bảng Users đã có data -> Bỏ qua không tạo user lẫn profile nữa
            if (await userManager.Users.AnyAsync()) return;

            await CreateUserAsync(userManager, context, "admin", "nguyenhuy3112005@gmail.com", "Admin@123", "Admin", "User", "admin", seedProfile: false);
            await CreateUserAsync(userManager, context, "manager", "kietdtse183938@fpt.edu.vn", "Manager@123", "Manager", "User", "manager", seedProfile: false);
            await CreateUserAsync(userManager, context, "customer", "huyndse184016@fpt.edu.vn", "Customer@123", "Customer", "User", "customer", phoneNumber: "0901234567", seedProfile: true);
        }

        private static async Task CreateUserAsync(
            UserManager<User> userManager,
            AppDbContext context,
            string username,
            string email,
            string password,
            string firstName,
            string lastName,
            string role,
            string? phoneNumber = null,
            bool seedProfile = true)
        {
            var user = new User
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded) return;

            await userManager.AddToRoleAsync(user, role);

            if (seedProfile && role == "customer")
            {
                context.CustomerProfiles.Add(new CustomerProfile
                {
                    Id = user.Id,
                    CurrentTierId = MemberTierId,
                    LastTierReviewDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedWashServicesAsync(AppDbContext context)
        {
            // Nếu bảng WashServices đã có data -> Bỏ qua
            if (await context.WashServices.AnyAsync()) return;

            var now = DateTime.UtcNow;
            var templates = new List<WashService>
            {
                new() { Name = "Rua co ban", Description = "Rua ngoai xe may", BasePrice = 30000, EstimatedDurationMinutes = 15, PointsPerTransaction = 10, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Rua cao cap", Description = "Rua + bao duong nhe", BasePrice = 80000, EstimatedDurationMinutes = 30, PointsPerTransaction = 25, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Combo toan dien", Description = "Goi tong hop", BasePrice = 250000, EstimatedDurationMinutes = 75, PointsPerTransaction = 60, IsActive = true, CreatedAt = now, UpdatedAt = now }
            };

            context.WashServices.AddRange(templates);
            await context.SaveChangesAsync();
        }

        private static async Task SeedVehiclesAsync(AppDbContext context)
        {
            // Nếu bảng Vehicles đã có data -> Bỏ qua
            if (await context.Vehicles.AnyAsync()) return;

            var profile = await context.CustomerProfiles.OrderBy(cp => cp.CreatedAt).FirstOrDefaultAsync();
            if (profile == null) return;

            var now = DateTime.UtcNow;
            var vehicles = new List<Vehicle>
            {
                new() { CustomerId = profile.Id, LicensePlate = "30F-12345", VehicleType = VehicleType.Motorbike, Brand = "Honda", Color = "Black", CreatedAt = now, UpdatedAt = now },
                new() { CustomerId = profile.Id, LicensePlate = "51G-99999", VehicleType = VehicleType.Scooter, Brand = "Yamaha", Color = "Red", CreatedAt = now, UpdatedAt = now }
            };

            context.Vehicles.AddRange(vehicles);
            await context.SaveChangesAsync();
        }

        private static async Task SeedPromotionsAsync(AppDbContext context, UserManager<User> userManager)
        {
            // Nếu bảng Promotions đã có data -> Bỏ qua
            if (await context.Promotions.AnyAsync()) return;

            var admin = await userManager.FindByNameAsync("admin");
            var silverTierId = await context.Tiers
                .Where(t => t.Name == "Silver")
                .Select(t => t.Id)
                .FirstOrDefaultAsync();
            if (silverTierId == Guid.Empty) return;

            var now = DateTime.UtcNow;
            var templates = new List<Promotion>
            {
                new() { PromoName = "Giam 10K dau tuan", Description = "Ap dung lich dau tuan cho tat ca thanh vien", PromoType = PromoType.Discount, DiscountAmount = 10000, DiscountPercent = 0, PointsCost = 0, MinTierId = MemberTierId, ValidFrom = now.AddDays(-15), ValidTo = now.AddMonths(2), MaxUsesTotal = 300, MaxUsesPerCustomer = 3, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { PromoName = "Uu dai Silver+", Description = "Khuyen mai danh rieng cho hang Silver tro len", PromoType = PromoType.Discount, DiscountAmount = 0, DiscountPercent = 15, PointsCost = 0, MinTierId = silverTierId, ValidFrom = now.AddDays(-10), ValidTo = now.AddMonths(2), MaxUsesTotal = 100, MaxUsesPerCustomer = 2, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { PromoName = "Doi 100 diem", Description = "Doi diem lay addon giam gia", PromoType = PromoType.Addon, DiscountAmount = 20000, DiscountPercent = 0, PointsCost = 100, MinTierId = null, ValidFrom = now.AddDays(-20), ValidTo = now.AddMonths(3), MaxUsesTotal = null, MaxUsesPerCustomer = 5, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { PromoName = "Rua mien phi 200 diem", Description = "Doi diem lay luot rua mien phi", PromoType = PromoType.FreeWash, DiscountAmount = 0, DiscountPercent = 100, PointsCost = 200, MinTierId = null, ValidFrom = now.AddDays(-5), ValidTo = now.AddMonths(6), MaxUsesTotal = 50, MaxUsesPerCustomer = 1, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now }
            };

            context.Promotions.AddRange(templates);
            await context.SaveChangesAsync();
        }

        private static async Task SeedBookingsAsync(AppDbContext context)
        {
            // Nếu bảng Bookings đã có data -> Bỏ qua
            if (await context.Bookings.AnyAsync()) return;

            var customer = await context.CustomerProfiles.OrderBy(c => c.CreatedAt).FirstOrDefaultAsync();
            var vehicles = await context.Vehicles.Where(v => customer != null && v.CustomerId == customer.Id).OrderBy(v => v.CreatedAt).ToListAsync();
            var services = await context.WashServices.OrderBy(s => s.BasePrice).Take(2).ToListAsync();
            var promotion = await context.Promotions.OrderBy(p => p.CreatedAt).FirstOrDefaultAsync();

            if (customer == null || vehicles.Count == 0 || services.Count == 0) return;

            var now = DateTime.UtcNow;
            var templates = new List<Booking>();

            for (var i = 0; i < Math.Min(2, vehicles.Count); i++)
            {
                var service = services[i % services.Count];
                var vehicle = vehicles[i];
                var baseAmount = service.BasePrice;
                var discount = promotion is null
                    ? 0
                    : (promotion.DiscountAmount > 0 ? promotion.DiscountAmount : Math.Round(baseAmount * promotion.DiscountPercent / 100m, 2));
                discount = Math.Min(discount, baseAmount);
                var scheduled = now.AddDays(-5 + i).AddHours(10 + i);

                templates.Add(new Booking
                {
                    CustomerId = customer.Id,
                    VehicleId = vehicle.Id,
                    ServiceId = service.Id,
                    PromoId = i == 0 ? promotion?.Id : null,
                    ScheduledTime = scheduled,
                    CheckinTime = scheduled.AddMinutes(10),
                    CompletedTime = scheduled.AddMinutes(service.EstimatedDurationMinutes + 20),
                    BaseAmount = baseAmount,
                    DiscountAmount = discount,
                    FinalAmount = baseAmount - discount,
                    PointsEarned = service.PointsPerTransaction,
                    PointsRedeemed = i == 0 ? promotion?.PointsCost ?? 0 : 0,
                    Status = BookingStatus.Completed,
                    PaymentMethod = i == 0 ? PaymentMethod.Cash : PaymentMethod.Transfer,
                    StaffNotes = $"Booking seed {i + 1}",
                    CreatedAt = scheduled,
                    UpdatedAt = scheduled.AddMinutes(5)
                });
            }

            context.Bookings.AddRange(templates);
            await context.SaveChangesAsync();
        }

        private static async Task SeedPointLogsAsync(AppDbContext context)
        {
            // Nếu bảng PointLogs đã có data -> Bỏ qua
            if (await context.PointLogs.AnyAsync()) return;

            var bookings = await context.Bookings
                .Include(b => b.Customer)
                .Where(b => b.Status == BookingStatus.Completed)
                .OrderBy(b => b.ScheduledTime)
                .Take(2)
                .ToListAsync();
            if (bookings.Count == 0) return;

            var now = DateTime.UtcNow;
            var templates = new List<PointLog>();
            var balances = new Dictionary<Guid, int>();

            foreach (var booking in bookings)
            {
                var customerId = booking.CustomerId;
                var prevBalance = balances.TryGetValue(customerId, out var b) ? b : booking.Customer.AvailablePoints;
                var earned = booking.PointsEarned > 0 ? booking.PointsEarned : 10;
                var newBalance = prevBalance + earned;
                balances[customerId] = newBalance;

                templates.Add(new PointLog
                {
                    CustomerId = customerId,
                    BookingId = booking.Id,
                    PointsChanged = earned,
                    TransactionType = PointTransactionType.Earn,
                    BalanceAfter = newBalance,
                    Note = "Cong diem sau khi hoan tat booking",
                    ExpiresAt = now.AddYears(1),
                    CreatedAt = booking.CompletedTime ?? now,
                    UpdatedAt = booking.CompletedTime ?? now
                });
            }

            context.PointLogs.AddRange(templates);
            await context.SaveChangesAsync();
        }

        private static async Task SeedFeedbacksAsync(AppDbContext context)
        {
            // Nếu bảng Feedbacks đã có data -> Bỏ qua
            if (await context.Feedbacks.AnyAsync()) return;

            var completedBookings = await context.Bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .OrderBy(b => b.CompletedTime)
                .Take(2)
                .ToListAsync();
            if (completedBookings.Count == 0) return;

            var comments = new[]
            {
                "Dich vu nhanh va sach se.",
                "Nhan vien than thien, se quay lai."
            };

            var templates = completedBookings.Select((booking, index) => new Feedback
            {
                BookingId = booking.Id,
                CustomerId = booking.CustomerId,
                Rating = 4 + (index % 2),
                Comment = comments[index % comments.Length],
                CreatedAt = (booking.CompletedTime ?? DateTime.UtcNow).AddMinutes(30),
                UpdatedAt = (booking.CompletedTime ?? DateTime.UtcNow).AddMinutes(30)
            }).ToList();

            context.Feedbacks.AddRange(templates);
            await context.SaveChangesAsync();
        }
    }
}