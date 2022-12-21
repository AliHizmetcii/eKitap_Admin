using System.ComponentModel.DataAnnotations.Schema;

namespace eKitap.Models
{
    [Table("Students")]
    public class Student : BaseEntity
    {
        public string Name { get; set; }
        public string TcId { get; set; }
        public string Password { get; set; }
        public ClassRoom ClassRoom { get; set; }
        public int ClassRoomId { get; set; }
        public bool ApproveStatus { get; set; }
        public ICollection<BookStudentConnection> BookStudentConnections { get; set; }
    }
}
