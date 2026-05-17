using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using GLMS.Web.Models.Enums;

namespace GLMS.Web.Models
{
    public class Contract
    {
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }

        public Client? Client { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public ContractStatus Status { get; set; }

        [Required]
        [StringLength(50)]
        public string ServiceLevel { get; set; } = string.Empty;

        public string? SignedAgreementPath { get; set; }
        [NotMapped]
        public IFormFile? SignedAgreementFile { get; set; }

        public List<ServiceRequest> ServiceRequests { get; set; } = new();
    }
}