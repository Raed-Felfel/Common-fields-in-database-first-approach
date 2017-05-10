using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommonTableFiels.Models
{
    public class EntityBase
    {
        public bool? Deleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    public partial class Entities
    {
        public string UserId { get; set; }

        public override int SaveChanges()
        {
            //changes on tables derived from EntityBase
            var changesSet = ChangeTracker.Entries<EntityBase>();
            if(changesSet != null)
            {
                //new added records
                foreach (var entry in changesSet.Where(e => e.State == System.Data.Entity.EntityState.Added))
                {
                    entry.Entity.Deleted = false;
                    entry.Entity.CreatedBy = UserId;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                //modified records
                foreach (var entry in changesSet.Where(e => e.State == System.Data.Entity.EntityState.Modified))
                {
                    entry.Entity.ModifiedBy = UserId;
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    if (entry.CurrentValues.GetValue<bool?>("Deleted") == true)
                    {
                        entry.Entity.DeletedBy = UserId;
                        entry.Entity.DeletedAt = DateTime.UtcNow;
                    }
                }
            } //if(changesSet != null)
            return base.SaveChanges();
        }
    }

}