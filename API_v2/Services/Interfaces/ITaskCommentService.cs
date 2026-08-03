using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface ITaskCommentService
    {
        Task<List<TaskCommentResponse>> GetCommentsAsync(int taskId, Guid currentUserId);
        Task<TaskCommentResponse> CreateCommentAsync(int taskId, CreateTaskCommentRequest req, Guid currentUserId);
        Task<TaskCommentResponse> UpdateCommentAsync(int commentId, UpdateTaskCommentRequest req, Guid currentUserId);
        Task DeleteCommentAsync(int commentId, Guid currentUserId);
    }
}
