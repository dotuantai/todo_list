using System;
using System.Collections.Generic;

namespace API_v2.Models.DTOs
{
    public class TaskFeedItemDto
    {
        public string Type { get; set; } = string.Empty; // "comment" or "activity"
        
        // Common
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        
        // Comment specific
        public string? Content { get; set; }
        
        // Activity specific
        public List<FieldChangeDto>? Changes { get; set; }
    }
}
