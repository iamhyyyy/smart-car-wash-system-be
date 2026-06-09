using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCarWash.Application.Interfaces
{
    public interface IServiceManager
    {
        IVehicleService VehicleService { get; }
        IFeedbackService FeedbackService { get; }
        IPromotionService PromotionService { get; }
        IBookingService BookingService { get; }
        IUserService UserService { get; }
        ITierService TierService { get; }
        ICustomerProfileService CustomerProfileService { get; }
        IPointLogService PointLogService { get; }
        IWashService WashServices { get; }
    }
}
