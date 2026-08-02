using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_v2.Models
{
    public class ProjectColumn
    {
        [Key]
        public int Id { get; set; }
        
        public Guid ProjectId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public int Order { get; set; }
        
        public bool IsCompletedStage { get; set; }
        
        public DateTime CreatedAt { get; set; }

        public virtual Project Project { get; set; } = null!;
        public virtual ICollection<TodoTask> Tasks { get; set; } = new List<TodoTask>();
    }
}
