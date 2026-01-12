using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities
{
    public class Client:BaseEntity
    {
        public string Name {  get; set; }
        public string Description { get; set; }
        public Guid RealmId { get; set; }
        [ForeignKey("RealmId")]
        public Tenant tenant { get; set; } = null!;
    }
}
