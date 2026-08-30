using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using API_v2.Exceptions;
using API_v2.Models;
using API_v2.Models.DTOs;
using API_v2.Repositories.IRepositories;
using API_v2.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace API_v2.Services
{
    public class TaskCommentService : ITaskCommentService
    {
        private static readonly Regex MentionRegex = new(
            @"(?<![\w@])@(?<email>[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly ITaskRepository _taskRepository;
        private readonly ITaskCommentRepository _commentRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<TaskCommentService> _logger;

        public TaskCommentService(
            ITaskRepository taskRepository,
            ITaskCommentRepository commentRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            INotificationService notificationService,
            ILogger<TaskCommentService> logger)
        {
            _taskRepository = taskRepository;
            _commentRepository = commentRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        private async Task SendMentionNotificationsAsync(
            int taskId,
            string content,
            Guid currentUserId,
            string authorEmail)
        {
            var mentionedEmails = MentionRegex.Matches(content)
                .Select(match => match.Groups["email"].Value.ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (mentionedEmails.Count == 0)
            {
                return;
            }

            var task = await _taskRepository.GetByIdAsync(taskId);

            if (task?.ProjectId == null)
            {
                return;
            }

            var recipients = (await _projectRepository.GetProjectMembersAsync(task.ProjectId.Value))
                .Where(member => member.UserId != currentUserId && member.User.IsActive &&
                    mentionedEmails.Contains(member.User.Email.ToLowerInvariant()))
                .Select(member => member.UserId).Distinct().ToList();

            foreach (var userId in recipients)
            {
                try
                {
                    await _notificationService.CreateAndSendNotificationAsync(
                        userId,
                        "You were mentioned in a comment",
                        $"{authorEmail} mentioned you in task '{task.Title}'.",
                        "Mention",
                        taskId.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to send mention notification for task {TaskId} to user {UserId}.",
                        taskId,
                        userId);
                }
            }
        }

        private async Task ValidateUserAccessToTask(int taskId, Guid currentUserId)
        {
            var task = await _taskRepository.GetByIdWithDetailsAsync(taskId);

            if (task == null)
            {
                throw ApiException.NotFound("Task not found.");
            }

            if (task.ProjectId.HasValue)
            {
                var isMember = await _projectRepository.GetMemberAsync(task.ProjectId.Value, currentUserId) is not null ||
                    await _projectRepository.IsSystemAdminAsync(currentUserId);

                if (!isMember)
                {
                    throw ApiException.Forbidden("You do not have access to this task.");
                }
            }
            else
            {
                // If the task has no project, maybe only the creator can comment (or we allow it).
                if (task.CreatorId != currentUserId)
                {
                    throw ApiException.Forbidden("You do not have access to this task.");
                }
            }
        }

        public async Task<PagedResponse<TaskCommentResponse>> GetCommentsAsync(int taskId, Guid currentUserId, int page = 1, int limit = 5)
        {
            await ValidateUserAccessToTask(taskId, currentUserId);

            var (comments, totalCount) = await _commentRepository.GetByTaskIdAsync(taskId, page, limit);
            var responses = comments.Select(tc => new TaskCommentResponse
                {
                    Id = tc.Id,
                    TaskId = tc.TaskId,
                    UserId = tc.UserId,
                    UserName = tc.User.Email,
                    Content = tc.Content,
                    CreatedAt = tc.CreatedAt
                }).ToList();

            responses.Reverse();

            return new PagedResponse<TaskCommentResponse>
            {
                Items = responses,
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

            _commentRepository.Add(comment);
            await _commentRepository.SaveAsync();

            var user = await _userRepository.GetByIdAsync(currentUserId);
            var authorEmail = user?.Email ?? "Unknown";

            await SendMentionNotificationsAsync(
                taskId,
                comment.Content,
                currentUserId,
                authorEmail);

            return new TaskCommentResponse
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                UserName = authorEmail,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<TaskCommentResponse> UpdateCommentAsync(int commentId, UpdateTaskCommentRequest req, Guid currentUserId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);

            if (comment == null)
            {
                throw ApiException.NotFound("Comment not found.");
            }

            if (comment.UserId != currentUserId)
            {
                throw ApiException.Forbidden("You can only edit your own comments.");
            }

            comment.Content = req.Content;
            await _commentRepository.SaveAsync();

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
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                throw ApiException.NotFound("Comment not found.");
            }

            if (comment.UserId != currentUserId)
            {
                throw ApiException.Forbidden("You can only delete your own comments.");
            }

            _commentRepository.Remove(comment);
            await _commentRepository.SaveAsync();
        }
    }
}
