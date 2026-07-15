using MPCRS.Models;
using Newtonsoft.Json;
using System.Data;
using System.Security.Claims;
using XAct.Users;
using static MPCRS.Utilities.Constants;

namespace MPCRS.Utilities
{
    public class UserData
    {
        public static bool IsAuthorized(ClaimsPrincipal user, UserPermissions Permission)
        {
            return validateUserClaim(user, Permission); ;
        }

        private static bool validateUserClaim(ClaimsPrincipal user, UserPermissions Permission)
        {
            try
            {
                if ((user.FindFirst(ClaimTypes.Email).Value == "lakshmiv@mail.gtre.org" || user.FindFirst(ClaimTypes.Email).Value == "manohar_tk@mail.gtre.org") && Permission != Constants.UserPermissions.Order_Module_User)
                {
                    return true;
                }

                var cachedRoleClaims = DataCaching.getCachedRoleClaims();
                if (cachedRoleClaims != null)
                {
                    List<string> roles = user.FindAll(ClaimTypes.Role)?.Select(x => x.Value).ToList();
                    if (roles != null)
                    {
                        var IsClaimAvailable = cachedRoleClaims.Where(x => x.ClaimValue == Permission.ToString() && roles.Contains(x.RoleId)).ToList();
                        if (IsClaimAvailable.Count > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // write log
            }
            return false;
        }



        public static List<AspNetRoleClaim> GetUserPermissions(ClaimsPrincipal user)
        {
            try
            {
                var cachedRoleClaims = DataCaching.getCachedRoleClaims();
                if (cachedRoleClaims != null)
                {
                    // Same exception as validateUserClaim — Lakshmi & Manohar get all permissions
                    var email = user.FindFirst(ClaimTypes.Email)?.Value;
                    if (email == "lakshmiv@mail.gtre.org" || email == "manohar_tk@mail.gtre.org")
                    {
                        // Return all distinct claims (they have access to everything except Order_Module_User)
                        return cachedRoleClaims
                            .Where(x => x.ClaimValue != UserPermissions.Order_Module_User.ToString())
                            .GroupBy(x => x.ClaimValue)
                            .Select(g => g.First())
                            .ToList();
                    }

                    List<string> roles = user.FindAll(ClaimTypes.Role)?.Select(x => x.Value).ToList();
                    if (roles != null)
                    {
                        return cachedRoleClaims.Where(x => roles.Contains(x.RoleId)).ToList();

                    }
                }
                return new List<AspNetRoleClaim>();
            }
            
            catch (Exception ex)
            {
                // write log
            }
            return new List<AspNetRoleClaim>();
        }


        public static List<NavigationMenuControl> GetUserMenuItems(ClaimsPrincipal user)
        {
            List<NavigationMenuControl> menuItems = new();
            try
            {
                var UserGUID = user.FindAll(ClaimTypes.NameIdentifier)?.Select(x => x.Value).FirstOrDefault();
                DataTable dt;

                if (user.FindFirst(ClaimTypes.Email).Value == "lakshmiv@mail.gtre.org")
                {
                    dt = Utilities.MPGlobals.GetDataForDatalist($"select * from [dbo].[NavigationMenuControl]");

                }
                else
                {
                    dt = Utilities.MPGlobals.GetDataForDatalist($"dbo.GetAllMenuItemsForUser @userID='{UserGUID}'");
                }

                if (dt.Rows.Count > 0)
                {
                    menuItems = JsonConvert.DeserializeObject<List<NavigationMenuControl>>(JsonConvert.SerializeObject(dt));
                }
            }
            catch (Exception ex)
            {

                return menuItems;
            }
           

            return menuItems;

        }
    }
}
