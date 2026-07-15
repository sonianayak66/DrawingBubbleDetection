using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using XAct;
using MPCRS.Utilities;
using static MPCRS.Utilities.Constants;
using System.Security.Claims;

namespace MPCRS
{
    public class ClaimRequirementAttribute : TypeFilterAttribute
    {
        public ClaimRequirementAttribute(UserPermissions permission) : base(typeof(ClaimRequirementFilter))
        {
            Arguments = new object[] { permission };
        }
    }
    public class ClaimRequirementFilter : IAuthorizationFilter
    {
        readonly UserPermissions _permission;
        public ClaimRequirementFilter(UserPermissions Permission)
        {
            _permission = Permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //_claim.
            // take role
            //compare claim with role and qulaify
            //var hasClaim = context.HttpContext.User.Claims.Any(c => c.Type == _claim.Type && c.Value == _claim.Value);
            //if (!hasClaim)
            //{
            //    context.Result = new ForbidResult();
            //}
            if (context.HttpContext.User.FindFirst(ClaimTypes.Email).Value != "lakshmiv@mail.gtre.org" || context.HttpContext.User.FindFirst(ClaimTypes.Email).Value != "manohar_tk@mail.gtre.org")
            {
                 if (!UserData.IsAuthorized(context.HttpContext.User, _permission))
                 {
                     context.Result = new ForbidResult();
                 }
            }
        }
    }

	public class OrClaimRequirementAttribute : TypeFilterAttribute
	{
		public OrClaimRequirementAttribute(params UserPermissions[] permissions) : base(typeof(OrClaimRequirementFilter))
		{
			Arguments = new object[] { permissions };
		}

		private class OrClaimRequirementFilter : IAuthorizationFilter
		{
			private readonly UserPermissions[] _permissions;

			public OrClaimRequirementFilter(UserPermissions[] permissions)
			{
				_permissions = permissions;
			}

			public void OnAuthorization(AuthorizationFilterContext context)
			{
				var user = context.HttpContext.User;

				if (_permissions.Any(permission => UserData.IsAuthorized(user, permission)))
				{
					return; // User has at least one of the required claims
				}

				context.Result = new ForbidResult();
			}
		}
	}
}
