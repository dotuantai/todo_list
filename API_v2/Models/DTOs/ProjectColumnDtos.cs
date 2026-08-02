using System.ComponentModel.DataAnnotations;

namespace API_v2.Models.DTOs
{
    public class ProjectColumnResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsCompletedStage { get; set; }
    }

    public class CreateProjectColumnRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name must not exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Range(0, int.MaxValue, ErrorMessage = "Order must be 0 or greater.")]
        public int Order { get; set; }
        public bool IsCompletedStage { get; set; }
    }

    public class UpdateProjectColumnRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name must not exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Range(0, int.MaxValue, ErrorMessage = "Order must be 0 or greater.")]
        public int Order { get; set; }
        public bool IsCompletedStage { get; set; }
    }
}
