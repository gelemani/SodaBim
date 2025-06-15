using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B.Models
{
    public class IfcCollision
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int FileId { get; set; }
        [Required]
        public string ElementA { get; set; } = string.Empty;
        [Required]
        public string ElementB { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [ForeignKey("FileId")]
        public ProjectFile? File { get; set; }
    }
} 