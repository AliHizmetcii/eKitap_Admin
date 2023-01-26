using System.ComponentModel.DataAnnotations.Schema;

namespace eKitap.Models
{
    [Table("Comments")]
    public class Comment : BaseEntity
    {
        public int PageNo { get; set; }
        public string Text { get; set; }
        public BookStudentConnection Connection { get; set; }
        public int  ConnectionId { get; set; }
    }
}
