using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API_v2.Models.DTOs;
using API_v2.Services.Interfaces;

namespace API_v2.Controllers
{
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : BaseApiController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var result = await _notificationService.GetNotificationsAsync(CurrentUserId, page, pageSize);
            return Ok(new ApiResponse<PagedResponse<NotificationResponse>>(true, "Success", result));
        }

        [HttpPut("{id:guid}/read")]
        public async Task<ActionResult> MarkAsRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Notification marked as read.", null));
        }

        [HttpPut("read-all")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync(CurrentUserId);
            return Ok(new ApiResponse<object>(true, "All notifications marked as read.", null));
        }
    }
}
