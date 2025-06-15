using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B.Models
{
    public class ProjectAccess
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string AccessLevel { get; set; } = "View"; // "View", "Edit", "Admin"

        [DataType(DataType.DateTime)]
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
} 