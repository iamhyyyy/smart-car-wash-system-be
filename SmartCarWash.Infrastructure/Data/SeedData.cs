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
                // Tự động chạy Migration nếu DB dưới Neon chưa được cập nhật
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await context.Database.MigrateAsync();
                }

                // 1. Nhóm Identity & dữ liệu nền tảng
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

        private static readonly Guid DefaultTierId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        private static readonly string[] CustomerNamePool =
        {
            "Nguyen Lan Anh", "Tran Minh Quan", "Le Thu Thao", "Pham Quoc Bao", "Vo Thanh Truc",
            "Do Gia Huy", "Bui Hai Dang", "Hoang Yen Nhi", "Pham Duc Manh", "Ngo Bao Chau",
            "Le Bao Ngan", "Tran Quynh Nhu", "Nguyen Hoang Long", "Phan Gia Bao", "Vu Tu Linh"
        };

        // 2. SEED TIERS - Lọc bỏ các hạng thành viên đã tồn tại dựa vào Name
        private static async Task SeedTiersAsync(AppDbContext context)
        {
            var now = DateTime.UtcNow;
            var tierTemplates = new List<Tier>
            {
                new() { Id = DefaultTierId, Name = "Bronze", MinPointsRequired = 0, BookingWindowDays = 3, PriorityLevel = 1, PointMultiplier = 1.0m, PerksDescription = "Hạng mặc định cho khách hàng mới", IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Silver", MinPointsRequired = 200, BookingWindowDays = 4, PriorityLevel = 2, PointMultiplier = 1.10m, PerksDescription = "Giảm nhẹ giờ chờ", IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Gold", MinPointsRequired = 500, BookingWindowDays = 5, PriorityLevel = 3, PointMultiplier = 1.20m, PerksDescription = "Ưu tiên giờ cao điểm", IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Platinum", MinPointsRequired = 900, BookingWindowDays = 6, PriorityLevel = 4, PointMultiplier = 1.30m, PerksDescription = "Ưu đãi combo định kỳ", IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Diamond", MinPointsRequired = 1400, BookingWindowDays = 7, PriorityLevel = 5, PointMultiplier = 1.40m, PerksDescription = "Ưu tiên tuyệt đối", IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Weekend Saver", MinPointsRequired = 80, BookingWindowDays = 3, PriorityLevel = 1, PointMultiplier = 1.05m, PerksDescription = "Ưu đãi cuối tuần", IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Student", MinPointsRequired = 50, BookingWindowDays = 3, PriorityLevel = 1, PointMultiplier = 1.08m, PerksDescription = "Hỗ trợ khách sinh viên", IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Family", MinPointsRequired = 350, BookingWindowDays = 4, PriorityLevel = 2, PointMultiplier = 1.15m, PerksDescription = "Áp dụng nhiều xe trong gia đình", IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Business", MinPointsRequired = 700, BookingWindowDays = 6, PriorityLevel = 3, PointMultiplier = 1.25m, PerksDescription = "Cho khách hàng doanh nghiệp", IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "VIP", MinPointsRequired = 1800, BookingWindowDays = 10, PriorityLevel = 6, PointMultiplier = 1.50m, PerksDescription = "Đặc quyền cao nhất", IsActive = true, CreatedAt = now, UpdatedAt = now }
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
            await CreateUserAsync(userManager, context, "carwash_admin", "admin.wash@gmail.com", "Admin@123", "Huy", "Admin", "admin", seedProfile: false);
            await CreateUserAsync(userManager, context, "station_manager", "kiet.manager@fpt.edu.vn", "Manager@123", "Tuan", "Kiet", "manager", seedProfile: false);
            await CreateUserAsync(userManager, context, "hoang_manager", "hoang.manager@example.com", "Manager@123", "Minh", "Hoang", "manager", seedProfile: false);

            // Seed danh sách 10 khách hàng cố định nếu tài khoản đó chưa có
            for (var i = 0; i < 10; i++)
            {
                var username = $"customer_{i + 1:D2}";
                var email = $"{username}@smartwash.local";

                var fullName = CustomerNamePool[i % CustomerNamePool.Length];
                var parts = fullName.Split(' ', 2, StringSplitOptions.TrimEntries);
                var firstName = parts[0];
                var lastName = parts.Length > 1 ? parts[1] : "Customer";

                await CreateUserAsync(userManager, context, username, email, "Customer@123", firstName, lastName, "customer");
            }
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
            var existingUser = await userManager.FindByNameAsync(username);
            if (existingUser != null) return; // Nếu có rồi thì bỏ qua không tạo nữa

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
                // Kiểm tra xem Profile đã tồn tại cho User này chưa (tránh lỗi khóa chính)
                var profileExists = await context.CustomerProfiles.AnyAsync(cp => cp.Id == user.Id);
                if (!profileExists)
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

        // 4. SEED WASH SERVICES - Check trùng theo tên dịch vụ
        private static async Task SeedWashServicesAsync(AppDbContext context)
        {
            var now = DateTime.UtcNow;
            var templates = new List<WashService>
            {
                new() { Name = "Rua co ban", Description = "Rua ngoai xe may", BasePrice = 30000, EstimatedDurationMinutes = 15, PointsPerTransaction = 10, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Rua cao cap", Description = "Rua + bao duong nhe", BasePrice = 80000, EstimatedDurationMinutes = 30, PointsPerTransaction = 25, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Ve sinh noi that", Description = "Lam sach noi that toan bo", BasePrice = 120000, EstimatedDurationMinutes = 45, PointsPerTransaction = 35, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Rua khong cham", Description = "Cong nghe rua bot tuyet", BasePrice = 90000, EstimatedDurationMinutes = 25, PointsPerTransaction = 28, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Danh bong vo xe", Description = "Lam moi vo xe va mam", BasePrice = 50000, EstimatedDurationMinutes = 20, PointsPerTransaction = 15, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Khu mui khoang xe", Description = "Khu mui va diet khuan", BasePrice = 60000, EstimatedDurationMinutes = 20, PointsPerTransaction = 18, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Phu ceramic nhanh", Description = "Bao ve son co ban", BasePrice = 180000, EstimatedDurationMinutes = 55, PointsPerTransaction = 45, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Rua dong co", Description = "Ve sinh khoang dong co", BasePrice = 110000, EstimatedDurationMinutes = 40, PointsPerTransaction = 30, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Cham soc da ghe", Description = "Duong am va lam sach ghe da", BasePrice = 150000, EstimatedDurationMinutes = 50, PointsPerTransaction = 40, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { Name = "Combo toan dien", Description = "Goi tong hop toan bo dich vu", BasePrice = 250000, EstimatedDurationMinutes = 75, PointsPerTransaction = 60, IsActive = true, CreatedAt = now, UpdatedAt = now }
            };

            var existingNames = await context.WashServices.Select(s => s.Name).ToListAsync();
            var toAdd = templates.Where(s => !existingNames.Contains(s.Name)).ToList();

            if (toAdd.Count > 0)
            {
                context.WashServices.AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }

        // 5. SEED VEHICLES - Check trùng theo biển số xe (LicensePlate)
        private static async Task SeedVehiclesAsync(AppDbContext context)
        {
            var profiles = await context.CustomerProfiles.OrderBy(cp => cp.CreatedAt).Take(10).ToListAsync();
            if (profiles.Count < 10) return; // Đảm bảo có đủ Customer để gán xe

            var now = DateTime.UtcNow;
            var vehicles = new List<Vehicle>
            {
                new() { CustomerId = profiles[0].Id, LicensePlate = "30F-12345", VehicleType = VehicleType.Motorbike, Brand = "Honda", Color = "Black", CreatedAt = now, UpdatedAt = now },
                new() { CustomerId = profiles[1].Id, LicensePlate = "51G-99999", VehicleType = VehicleType.Scooter, Brand = "Yamaha", Color = "Red", CreatedAt = now, UpdatedAt = now },
                new() { CustomerId = profiles[2].Id, LicensePlate = "29A-88888", VehicleType = VehicleType.Motorbike, Brand = "Suzuki", Color = "White", CreatedAt = now, UpdatedAt = now },
                new() { CustomerId = profiles[3].Id, LicensePlate = "30H-45678", VehicleType = VehicleType.Other, Brand = "VinFast", Color = "Blue", CreatedAt = now, UpdatedAt = now },
                new() { CustomerId = profiles[4].Id, LicensePlate = "43C-11111", VehicleType = VehicleType.Motorbike, Brand = "Honda", Color = "Gray", CreatedAt = now, UpdatedAt = now },
                new() { CustomerId = profiles[5].Id, LicensePlate = "59X-22222", VehicleType = VehicleType.Scooter, Brand = "Piaggio", Color = "Silver", CreatedAt = now, UpdatedAt = now },
                new() { CustomerId = profiles[6].Id, LicensePlate = "79B-33333", VehicleType = VehicleType.Motorbike, Brand = "Yamaha", Color = "Black", CreatedAt = now, UpdatedAt = now },
                new() { CustomerId = profiles[7].Id, LicensePlate = "88C-44444", VehicleType = VehicleType.Other, Brand = "SYM", Color = "Green", CreatedAt = now, UpdatedAt = now },
                new() { CustomerId = profiles[8].Id, LicensePlate = "92D-55555", VehicleType = VehicleType.Motorbike, Brand = "VinFast", Color = "White", CreatedAt = now, UpdatedAt = now },
                new() { CustomerId = profiles[9].Id, LicensePlate = "14E-66666", VehicleType = VehicleType.Scooter, Brand = "Honda", Color = "Orange", CreatedAt = now, UpdatedAt = now }
            };

            var existingPlates = await context.Vehicles.Select(v => v.LicensePlate).ToListAsync();
            var toAdd = vehicles.Where(v => !existingPlates.Contains(v.LicensePlate)).ToList();

            if (toAdd.Count > 0)
            {
                context.Vehicles.AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }

        // 6. SEED PROMOTIONS - Check trùng theo tên chiến dịch (PromoName)
        private static async Task SeedPromotionsAsync(AppDbContext context, UserManager<User> userManager)
        {
            var admin = await userManager.FindByNameAsync("carwash_admin");
            var tierIds = await context.Tiers.OrderBy(t => t.MinPointsRequired).Select(t => t.Id).Take(4).ToListAsync();
            if (tierIds.Count == 0) return;

            var now = DateTime.UtcNow;
            var templates = new List<Promotion>
            {
                new() { PromoName = "Giam 10K dau tuan", Description = "Ap dung cho lich dau tuan", PromoType = PromoType.Discount, DiscountAmount = 10000, DiscountPercent = 0, PointsCost = 0, MinTierId = tierIds[0], ValidFrom = now.AddDays(-15), ValidTo = now.AddMonths(2), MaxUsesTotal = 300, MaxUsesPerCustomer = 3, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { PromoName = "Giam 15 phan tram", Description = "Cho khach Silver tro len", PromoType = PromoType.Discount, DiscountAmount = 0, DiscountPercent = 15, PointsCost = 0, MinTierId = tierIds[Math.Min(1, tierIds.Count - 1)], ValidFrom = now.AddDays(-10), ValidTo = now.AddMonths(1), MaxUsesTotal = 200, MaxUsesPerCustomer = 2, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { PromoName = "Doi 100 diem", Description = "Doi diem lay ma giam gia", PromoType = PromoType.Addon, DiscountAmount = 20000, DiscountPercent = 0, PointsCost = 100, MinTierId = null, ValidFrom = now.AddDays(-20), ValidTo = now.AddMonths(3), MaxUsesTotal = null, MaxUsesPerCustomer = 5, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { PromoName = "Tang diem thu 6", Description = "Thu 6 nhan them diem", PromoType = PromoType.PointBonus, DiscountAmount = 0, DiscountPercent = 0, PointsCost = 0, MinTierId = null, ValidFrom = now.AddDays(-5), ValidTo = now.AddMonths(2), MaxUsesTotal = 500, MaxUsesPerCustomer = 4, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { PromoName = "Free wash mini", Description = "Mien phi rua co ban khi du diem", PromoType = PromoType.FreeWash, DiscountAmount = 30000, DiscountPercent = 0, PointsCost = 250, MinTierId = tierIds[0], ValidFrom = now.AddDays(-3), ValidTo = now.AddMonths(1), MaxUsesTotal = 120, MaxUsesPerCustomer = 1, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { PromoName = "Giam 25K xe moi", Description = "Khuyen mai khach moi", PromoType = PromoType.Discount, DiscountAmount = 25000, DiscountPercent = 0, PointsCost = 0, MinTierId = null, ValidFrom = now.AddDays(-7), ValidTo = now.AddMonths(1), MaxUsesTotal = 180, MaxUsesPerCustomer = 1, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { PromoName = "Combo noi that -20%", Description = "Khuyen mai cho goi noi that", PromoType = PromoType.Discount, DiscountAmount = 0, DiscountPercent = 20, PointsCost = 0, MinTierId = tierIds[0], ValidFrom = now.AddDays(-1), ValidTo = now.AddMonths(2), MaxUsesTotal = 160, MaxUsesPerCustomer = 2, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { PromoName = "Flash sale toi", Description = "Khung gio 18h-21h", PromoType = PromoType.Discount, DiscountAmount = 15000, DiscountPercent = 0, PointsCost = 0, MinTierId = null, ValidFrom = now.AddDays(-2), ValidTo = now.AddMonths(1), MaxUsesTotal = 220, MaxUsesPerCustomer = 2, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { PromoName = "Uu dai Gold", Description = "Khach Gold duoc uu tien", PromoType = PromoType.Addon, DiscountAmount = 30000, DiscountPercent = 0, PointsCost = 150, MinTierId = tierIds[Math.Min(2, tierIds.Count - 1)], ValidFrom = now.AddDays(-12), ValidTo = now.AddMonths(2), MaxUsesTotal = 130, MaxUsesPerCustomer = 3, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new() { PromoName = "Uu dai Platinum", Description = "Giam gia cho hang cao", PromoType = PromoType.Discount, DiscountAmount = 0, DiscountPercent = 25, PointsCost = 0, MinTierId = tierIds[Math.Min(3, tierIds.Count - 1)], ValidFrom = now.AddDays(-14), ValidTo = now.AddMonths(4), MaxUsesTotal = 90, MaxUsesPerCustomer = 2, CurrentUses = 0, CreatedBy = admin?.Id, IsActive = true, CreatedAt = now, UpdatedAt = now }
            };

            var existingNames = await context.Promotions.Select(p => p.PromoName).ToListAsync();
            var toAdd = templates.Where(p => !existingNames.Contains(p.PromoName)).ToList();

            if (toAdd.Count > 0)
            {
                context.Promotions.AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }

        // 7. SEED BOOKINGS - Check theo ScheduledTime và CustomerId
        private static async Task SeedBookingsAsync(AppDbContext context)
        {
            var customers = await context.CustomerProfiles.OrderBy(c => c.CreatedAt).Take(10).ToListAsync();
            var vehicles = await context.Vehicles.OrderBy(v => v.CreatedAt).Take(10).ToListAsync();
            var services = await context.WashServices.OrderBy(s => s.BasePrice).Take(10).ToListAsync();
            var promotions = await context.Promotions.OrderBy(p => p.CreatedAt).Take(10).ToListAsync();

            if (customers.Count < 10 || vehicles.Count < 10 || services.Count == 0) return;

            var now = DateTime.UtcNow;
            var templates = new List<Booking>();
            for (var i = 0; i < 10; i++)
            {
                var service = services[i % services.Count];
                var promo = promotions.Count > 0 ? promotions[i % promotions.Count] : null;
                var baseAmount = service.BasePrice;
                var discount = promo is null
                    ? 0
                    : (promo.DiscountAmount > 0 ? promo.DiscountAmount : Math.Round(baseAmount * promo.DiscountPercent / 100m, 2));
                discount = Math.Min(discount, baseAmount);
                var finalAmount = baseAmount - discount;
                var scheduled = now.AddDays(-10 + i).AddHours(i);

                templates.Add(new Booking
                {
                    CustomerId = customers[i].Id,
                    VehicleId = vehicles[i].Id,
                    ServiceId = service.Id,
                    PromoId = promo?.Id,
                    ScheduledTime = scheduled,
                    CheckinTime = scheduled.AddMinutes(10),
                    CompletedTime = scheduled.AddMinutes(service.EstimatedDurationMinutes + 20),
                    BaseAmount = baseAmount,
                    DiscountAmount = discount,
                    FinalAmount = finalAmount,
                    PointsEarned = service.PointsPerTransaction,
                    PointsRedeemed = promo?.PointsCost ?? 0,
                    Status = BookingStatus.Completed,
                    PaymentMethod = i % 3 == 0 ? PaymentMethod.Cash : (i % 3 == 1 ? PaymentMethod.Transfer : PaymentMethod.Points),
                    StaffNotes = $"Booking seed {i + 1}",
                    CreatedAt = scheduled,
                    UpdatedAt = scheduled.AddMinutes(5)
                });
            }

            // Check xem khách hàng này đã có đặt lịch vào đúng boong khung giờ này chưa
            var toAdd = new List<Booking>();
            foreach (var b in templates)
            {
                var exists = await context.Bookings.AnyAsync(existing => existing.CustomerId == b.CustomerId && existing.ScheduledTime == b.ScheduledTime);
                if (!exists)
                {
                    toAdd.Add(b);
                }
            }

            if (toAdd.Count > 0)
            {
                context.Bookings.AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }

        // 8. SEED POINT LOGS - Check theo BookingId (Mỗi booking chỉ có 1 log cộng điểm)
        private static async Task SeedPointLogsAsync(AppDbContext context)
        {
            var bookings = await context.Bookings
                .Include(b => b.Customer)
                .OrderBy(b => b.ScheduledTime)
                .Take(10)
                .ToListAsync();
            if (bookings.Count < 10) return;

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

        // 9. SEED FEEDBACKS - Check xem BookingId đó đã được đánh giá chưa
        private static async Task SeedFeedbacksAsync(AppDbContext context)
        {
            var completedBookings = await context.Bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .OrderBy(b => b.CompletedTime)
                .Take(10)
                .ToListAsync();
            if (completedBookings.Count < 10) return;

            var comments = new[]
            {
                "Dich vu nhanh va sach se.", "Nhan vien than thien.", "Gia hop ly, se quay lai.",
                "Xe duoc cham soc ky.", "Dat lich de dang, dung gio.", "Khu vuc cho doi thoai mai.",
                "Rat hai long ve chat luong.", "Khuyen mai tot, tiet kiem chi phi.",
                "Goi combo rat dang tien.", "Trai nghiem nhin chung rat on."
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