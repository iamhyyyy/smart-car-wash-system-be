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
            foreach (var role in new[] { "admin", "manager", "customer" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role, NormalizedName = role.ToUpper() });
                }
            }
        }

        private static readonly Guid DefaultTierId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        private static async Task SeedTiersAsync(AppDbContext context)
        {
            var now = DateTime.UtcNow;
            var tierTemplates = new List<Tier>
            {
                new() { Id = DefaultTierId, Name = "Bronze", MinPointsRequired = 0, BookingWindowDays = 3, PriorityLevel = 1, PointMultiplier = 1.0m, PerksDescription = "Hang mac dinh", IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Silver", MinPointsRequired = 200, BookingWindowDays = 4, PriorityLevel = 2, PointMultiplier = 1.10m, PerksDescription = "Giam nhe gio cho", IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Gold", MinPointsRequired = 500, BookingWindowDays = 5, PriorityLevel = 3, PointMultiplier = 1.20m, PerksDescription = "Uu tien gio cao diem", IsActive = true, CreatedAt = now, UpdatedAt = now }
            };

            var existingNames = await context.Tiers.Select(t => t.Name).ToListAsync();
            var toAdd = tierTemplates.Where(t => !existingNames.Contains(t.Name)).ToList();

            if (toAdd.Count > 0)
            {
                context.Tiers.AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedUsersAsync(UserManager<User> userManager, AppDbContext context)
        {
            await CreateUserAsync(userManager, context, "admin", "nguyenhuy3112005@gmail.com", "Admin@123", "Admin", "User", "admin", seedProfile: false);
            await CreateUserAsync(userManager, context, "manager", "kietdtse183938@fpt.edu.vn", "Manager@123", "Manager", "User", "manager", seedProfile: false);
            await CreateUserAsync(userManager, context, "customer", "huyndse184016@fpt.edu.vn", "Customer@123", "Customer", "User", "customer", seedProfile: true);
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
            bool seedProfile = true)
        {
            if (await userManager.FindByNameAsync(username) != null) return;

            var user = new User
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded) return;

            await userManager.AddToRoleAsync(user, role);

            if (seedProfile && role == "customer")
            {
                if (!await context.CustomerProfiles.AnyAsync(cp => cp.Id == user.Id))
                {
                    context.CustomerProfiles.Add(new CustomerProfile
                    {
                        Id = user.Id,
                        CurrentTierId = DefaultTierId,
                        LastTierReviewDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedWashServicesAsync(AppDbContext context)
        {
            var now = DateTime.UtcNow;
            var templates = new List<WashService>
            {
                new() { Name = "Rua co ban", Description = "Rua ngoai xe may", BasePrice = 30000, EstimatedDurationMinutes = 15, PointsPerTransaction = 10, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Rua cao cap", Description = "Rua + bao duong nhe", BasePrice = 80000, EstimatedDurationMinutes = 30, PointsPerTransaction = 25, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Combo toan dien", Description = "Goi tong hop", BasePrice = 250000, EstimatedDurationMinutes = 75, PointsPerTransaction = 60, IsActive = true, CreatedAt = now, UpdatedAt = now }
            };

            var existingNames = await context.WashServices.Select(s => s.Name).ToListAsync();
            var toAdd = templates.Where(s => !existingNames.Contains(s.Name)).ToList();

            if (toAdd.Count > 0)
            {
                context.WashServices.AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedVehiclesAsync(AppDbContext context)
        {
            var profile = await context.CustomerProfiles.OrderBy(cp => cp.CreatedAt).FirstOrDefaultAsync();
            if (profile == null) return;

            var now = DateTime.UtcNow;
            var vehicles = new List<Vehicle>
            {
                new() { CustomerId = profile.Id, LicensePlate = "30F-12345", VehicleType = VehicleType.Motorbike, Brand = "Honda", Color = "Black", CreatedAt = now, UpdatedAt = now },
                new() { CustomerId = profile.Id, LicensePlate = "51G-99999", VehicleType = VehicleType.Scooter, Brand = "Yamaha", Color = "Red", CreatedAt = now, UpdatedAt = now }
            };

            var existingPlates = await context.Vehicles.Select(v => v.LicensePlate).ToListAsync();
            var toAdd = vehicles.Where(v => !existingPlates.Contains(v.LicensePlate)).ToList();

            if (toAdd.Count > 0)
            {
                context.Vehicles.AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedPromotionsAsync(AppDbContext context, UserManager<User> userManager)
        {
            var admin = await userManager.FindByNameAsync("admin");
            var bronzeTierId = await context.Tiers
                .Where(t => t.Name == "Bronze")
                .Select(t => t.Id)
                .FirstOrDefaultAsync();
            if (bronzeTierId == Guid.Empty) return;

            var now = DateTime.UtcNow;
            var templates = new List<Promotion>
            {
                new() { PromoName = "Giam 10K dau tuan", Description = "Ap dung lich dau tuan", PromoType = PromoType.Discount, DiscountAmount = 10000, DiscountPercent = 0, PointsCost = 0, MinTierId = bronzeTierId, ValidFrom = now.AddDays(-15), ValidTo = now.AddMonths(2), MaxUsesTotal = 300, MaxUsesPerCustomer = 3, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { PromoName = "Doi 100 diem", Description = "Doi diem lay ma giam gia", PromoType = PromoType.Addon, DiscountAmount = 20000, DiscountPercent = 0, PointsCost = 100, MinTierId = null, ValidFrom = now.AddDays(-20), ValidTo = now.AddMonths(3), MaxUsesTotal = null, MaxUsesPerCustomer = 5, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now }
            };

            var existingNames = await context.Promotions.Select(p => p.PromoName).ToListAsync();
            var toAdd = templates.Where(p => !existingNames.Contains(p.PromoName)).ToList();

            if (toAdd.Count > 0)
            {
                context.Promotions.AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedBookingsAsync(AppDbContext context)
        {
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

            var toAdd = new List<Booking>();
            foreach (var b in templates)
            {
                var exists = await context.Bookings.AnyAsync(existing =>
                    existing.CustomerId == b.CustomerId && existing.ScheduledTime == b.ScheduledTime);
                if (!exists) toAdd.Add(b);
            }

            if (toAdd.Count > 0)
            {
                context.Bookings.AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedPointLogsAsync(AppDbContext context)
        {
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
                    ExpiresAt = now.AddMonths(6),
                    CreatedAt = booking.CompletedTime ?? now,
                    UpdatedAt = booking.CompletedTime ?? now
                });
            }

            var existingBookingIds = await context.PointLogs
                .Where(pl => pl.BookingId != null)
                .Select(pl => pl.BookingId!.Value)
                .ToListAsync();

            var toAdd = templates.Where(pl => pl.BookingId.HasValue && !existingBookingIds.Contains(pl.BookingId.Value)).ToList();

            if (toAdd.Count > 0)
            {
                context.PointLogs.AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedFeedbacksAsync(AppDbContext context)
        {
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

            var existingBookingIds = await context.Feedbacks.Select(f => f.BookingId).ToListAsync();
            var toAdd = templates.Where(f => !existingBookingIds.Contains(f.BookingId)).ToList();

            if (toAdd.Count > 0)
            {
                context.Feedbacks.AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }
    }
}
