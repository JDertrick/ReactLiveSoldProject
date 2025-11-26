using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace ReactLiveSoldProject.Server.Controllers.Base
{
    public abstract class BaseController : ControllerBase
    {
        protected Guid? GetOrganizationId()
        {
            var organizationIdClaim = User.FindFirstValue("OrganizationId");
            if (Guid.TryParse(organizationIdClaim, out var organizationId))
            {
                return organizationId;
            }
            return null;
        }

        protected Guid? GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}
