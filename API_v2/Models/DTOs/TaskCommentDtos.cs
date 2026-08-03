using System;
using System.ComponentModel.DataAnnotations;

namespace API_v2.Models.DTOs
{
    public class TaskCommentResponse
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTaskCommentRequest
    {
        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateTaskCommentRequest
    {
        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;
    }
}
