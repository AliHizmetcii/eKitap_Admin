using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eKitap.Models
{
    [Table("BookStudentConnections")]
    public class BookStudentConnection : BaseEntity
    {
        public int BookId { get; set; }
        public Book Book { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
        public int CurrentPage { get; set; }
        public List<Comment> Comments { get; set; }
        public short? Rate { get; set; }
    }
}
