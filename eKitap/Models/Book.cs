using System.ComponentModel.DataAnnotations.Schema;

namespace eKitap.Models
{
    [Table("Books")]
    public class Book : BaseEntity
    {
        public string Name { get; set; }
        public ClassRoom ClassRoom  { get; set; }
        public int ClassRoomId { get; set; }
        public int DownlaodCount { get; set; }
        public string PdfName { get; set; }
        public string? ChapterName { get; set; }
        public ICollection<BookStudentConnection> BookStudentConnections { get; set; }
    }
}
