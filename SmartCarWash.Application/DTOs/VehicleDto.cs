using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCarWash.Application.DTOs
{
    public class VehicleDto
    {
        public Guid Id { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
    }
    public class CreateVehicleDto
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
    }
}
