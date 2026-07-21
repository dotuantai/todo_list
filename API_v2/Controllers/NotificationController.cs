using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API_v2.Models.DTOs;
using API_v2.Services.Interfaces;
using System;
using System.Collections.Generic;

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
        public ActionResult Get()
        {
            var result = _notificationService.GetNotifications(CurrentUserId);
            return Ok(new ApiResponse<List<NotificationResponse>>(true, "Success", result));
        }

        [HttpPut("{id:guid}/read")]
        public ActionResult MarkAsRead(Guid id)
        {
            _notificationService.MarkAsRead(id, CurrentUserId);
            return Ok(new ApiResponse<object>(true, "Notification marked as read.", null));
        }

        [HttpPut("read-all")]
        public ActionResult MarkAllAsRead()
        {
            _notificationService.MarkAllAsRead(CurrentUserId);
            return Ok(new ApiResponse<object>(true, "All notifications marked as read.", null));
        }
    }
}
