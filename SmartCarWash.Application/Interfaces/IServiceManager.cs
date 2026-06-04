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
    }
}
