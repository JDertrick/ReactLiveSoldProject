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
    }
}
