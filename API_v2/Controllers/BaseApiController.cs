using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using API_v2.Exceptions;

namespace API_v2.Controllers
{
    /// <summary>
    /// Common authenticated-controller helpers. Service exceptions flow to
    /// GlobalExceptionHandlerMiddleware; controllers must not catch and expose them.
    /// </summary>
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

    }
}
