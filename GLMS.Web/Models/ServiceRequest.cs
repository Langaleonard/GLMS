using System.ComponentModel.DataAnnotations;
using GLMS.Web.Models.Enums;

namespace GLMS.Web.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }

        public Contract? Contract { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal CostUsd { get; set; }

        public decimal ExchangeRate { get; set; }

        public decimal CostZar { get; set; }

        [Required]
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
    }
}