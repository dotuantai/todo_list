using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using API_v2.Exceptions;
using API_v2.Models;
using API_v2.Models.DTOs;

namespace API_v2.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected Guid CurrentUserId
        {
            get
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null)
                {
                    throw ApiException.Unauthorized("Unable to identify user from token.");
                }

                return Guid.Parse(claim.Value);
            }
        }

        protected ActionResult Execute(Action action)
        {
            try
            {
                action();
                return Ok(new ApiResponse<object>(true, "Success", null));
            }
            catch (ApiException ex)
            {
                return StatusCode((int)ex.StatusCode, new ApiResponse<object>(false, ex.Message, null));
            }
        }

        protected ActionResult Execute<T>(Func<T> action)
        {
            try
            {
                var result = action();
                return Ok(new ApiResponse<T>(true, "Success", result));
            }
            catch (ApiException ex)
            {
                return StatusCode((int)ex.StatusCode, new ApiResponse<object>(false, ex.Message, null));
            }
        }
    }
}
