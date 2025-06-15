using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FileId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Text { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string ElementName { get; set; } = string.Empty;

        [Required]
        public int ElementId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        [ForeignKey("FileId")]
        public ProjectFile? File { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
} 