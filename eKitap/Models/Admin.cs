using System.ComponentModel.DataAnnotations.Schema;

namespace eKitap.Models
{
    [Table("Admins")]
    public class Admin : BaseEntity
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }
}
