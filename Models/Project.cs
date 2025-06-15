using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B.Models
{
    public class Project
    {
        public int Id { get; set; } // Project ID

        [Required]
        public int CreatorId { get; set; } // ID of the user who created the project

        [ForeignKey("CreatorId")]
        public User? Creator { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Date of creation

        [DataType(DataType.DateTime)]
        public DateTime LastModified { get; set; } = DateTime.UtcNow; // Date of last modification

        public string AccessLevel { get; set; } = "View"; // Access level of the current user (e.g., "View", "Edit", "Admin")

        public ICollection<ProjectAccess> ProjectAccesses { get; set; } = new List<ProjectAccess>();
        public ICollection<ProjectFile> ProjectFiles { get; set; } = new List<ProjectFile>();
    }
}
