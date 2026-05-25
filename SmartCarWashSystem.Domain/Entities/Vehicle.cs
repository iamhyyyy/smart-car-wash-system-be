using SmartCarWash.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCarWash.Domain.Entities
{
    public class Vehicle : BaseEntity
    {
        public string LicensePlate { get; set; } = string.Empty; // Biển số xe (Quan trọng nhất)
        public string VehicleType { get; set; } = string.Empty;  // Loại xe: Ô tô 4 chỗ, 7 chỗ, Xe máy...
        public string OwnerName { get; set; } = string.Empty;   // Tên chủ xe
    }

}
