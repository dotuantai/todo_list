using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_v2.Datas;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_v2.Services
{
    public class TaskCommentService : ITaskCommentService
    {
        private readonly AppDbContext _context;

        public TaskCommentService(AppDbContext context)
        {
            _context = context;
        }

        private async Task ValidateUserAccessToTask(int taskId, Guid currentUserId)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                throw new Exception("Task not found.");
            }

            if (task.ProjectId.HasValue)
            {
                var isMember = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == task.ProjectId.Value && pm.UserId == currentUserId);

                if (!isMember)
                {
                    throw new Exception("You do not have access to this task.");
                }
            }
            else
            {
                // If the task has no project, maybe only the creator can comment (or we allow it).
                if (task.CreatorId != currentUserId)
                {
                    throw new Exception("You do not have access to this task.");
                }
            }
        }

        public async Task<PagedResponse<TaskCommentResponse>> GetCommentsAsync(int taskId, Guid currentUserId, int page = 1, int limit = 5)
        {
            await ValidateUserAccessToTask(taskId, currentUserId);

            var query = _context.TaskComments
                .Include(tc => tc.User)
                .Where(tc => tc.TaskId == taskId);

            var totalCount = await query.CountAsync();

            var comments = await query
                .OrderByDescending(tc => tc.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(tc => new TaskCommentResponse
                {
                    Id = tc.Id,
                    TaskId = tc.TaskId,
                    UserId = tc.UserId,
                    UserName = tc.User.Email,
                    Content = tc.Content,
                    CreatedAt = tc.CreatedAt
                })
                .ToListAsync();

            comments.Reverse();

            return new PagedResponse<TaskCommentResponse>
            {
                Items = comments,
                TotalCount = totalCount,
                Page = page,
                PageSize = limit
            };
        }

        public async Task<TaskCommentResponse> CreateCommentAsync(int taskId, CreateTaskCommentRequest req, Guid currentUserId)
        {
            await ValidateUserAccessToTask(taskId, currentUserId);

            var comment = new TaskComment
            {
                TaskId = taskId,
                UserId = currentUserId,
                Content = req.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskComments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(currentUserId);

            return new TaskCommentResponse
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                UserName = user?.Email ?? "Unknown",
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<TaskCommentResponse> UpdateCommentAsync(int commentId, UpdateTaskCommentRequest req, Guid currentUserId)
        {
            var comment = await _context.TaskComments
                .Include(tc => tc.User)
                .FirstOrDefaultAsync(tc => tc.Id == commentId);

            if (comment == null)
            {
                throw new Exception("Comment not found.");
            }

            if (comment.UserId != currentUserId)
            {
                throw new Exception("You can only edit your own comments.");
            }

            comment.Content = req.Content;
            await _context.SaveChangesAsync();

            return new TaskCommentResponse
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                UserName = comment.User.Email,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task DeleteCommentAsync(int commentId, Guid currentUserId)
        {
            var comment = await _context.TaskComments.FindAsync(commentId);
            if (comment == null)
            {
                throw new Exception("Comment not found.");
            }

            if (comment.UserId != currentUserId)
            {
                throw new Exception("You can only delete your own comments.");
            }

            _context.TaskComments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }
}
