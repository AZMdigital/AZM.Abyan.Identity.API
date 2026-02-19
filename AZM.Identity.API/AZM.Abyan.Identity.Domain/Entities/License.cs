using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities
{
    public class License : BaseEntity
    {
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public string LicenseKeyHash { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKeyEncrypted {  get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiryDate {  get; set; }
        public int? MaxUsers { get; set; }
        public bool IsRevoked { get; set; }
        public string PackageName {  get; set; }
        public string Domain { get; set; }
        public string ServerIps {  get; set; }
    }
}
