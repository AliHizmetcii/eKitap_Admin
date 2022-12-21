using System.ComponentModel.DataAnnotations.Schema;

namespace eKitap.Models
{
    [Table("ClassRooms")]
    public class ClassRoom : BaseEntity
    {
        public string Title { get; set; }
        public ICollection<Student> Students { get; set; }
        public ICollection<Book> Books { get; set; }
    }
}
