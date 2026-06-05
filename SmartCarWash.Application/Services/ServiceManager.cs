using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using SmartCarWash.Application.Interfaces;
using SmartCarWash.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCarWash.Application.Services
{
    public class ServiceManager : IServiceManager
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IVehicleService VehicleService => _serviceProvider.GetRequiredService<IVehicleService>();

        public IFeedbackService FeedbackService => _serviceProvider.GetRequiredService<IFeedbackService>();

        public IPromotionService PromotionService => _serviceProvider.GetRequiredService<IPromotionService>();
    }
}
