using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using static MPCRS.Utilities.Constants;

namespace MPCRS.Controllers
{
    public class AuthController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;

        [Obsolete]
        public AuthController(IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }

        public IActionResult Index(string id = "", string ReturnUrl = "/Home/Index")
        {
            AuthenticateByEmailViewModel authenticateByEmailViewModel = new();
            authenticateByEmailViewModel.ReturnUrl = ReturnUrl;
            _ = HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            //var linkTimeLocal = GetLinkerTime(Assembly.GetExecutingAssembly());
             ViewBag.LinkTimeLocal = version.LegalCopyright;
            return View(authenticateByEmailViewModel);
        }

         

        [HttpPost]
        public IActionResult AuthenticateByEmail(AuthenticateByEmailViewModel authenticateByEmailViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (authenticateByEmailViewModel.Email != null && authenticateByEmailViewModel.Password != null)
                    {
                        // Convenience: if user typed only the prefix (no '@'), auto-append the default domain
                        if (!authenticateByEmailViewModel.Email.Contains('@'))
                        {
                            authenticateByEmailViewModel.Email = authenticateByEmailViewModel.Email.Trim() + "@mail.gtre.org";
                        }
                        string password = CheckIsOldUser(authenticateByEmailViewModel.Email.ToLower(), authenticateByEmailViewModel.Password);
                        authenticateByEmailViewModel.Password = password;
                        authenticateByEmailViewModel.AuthenticateBy = LoginBy.Email.ToString();
                        authenticateByEmailViewModel = IsAuthenticated(authenticateByEmailViewModel);
                        if (authenticateByEmailViewModel.IsAuthenticated == true)
                        {
                            return Redirect(authenticateByEmailViewModel.ReturnUrl);
                        }
                        authenticateByEmailViewModel.ResponseMessage = "Invalid Email or Password";
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }
            }
            return View("Index", authenticateByEmailViewModel);
        }

		private string CheckIsOldUser(string email, string password)
		{
            // For the users who synched from old tables used UserDbkey  for password encryption
            try {
                string encryptpassword = "";
                using (var db = new DESI_STFE_PRODContext())
                {
                    AspNetUser aspNetUser = db.AspNetUsers.AsNoTracking().Where(x => x.Email == email && x.IsActiveUser == true).FirstOrDefault();
                    if (aspNetUser is not null)
                    {
                        if (aspNetUser.TwoFactorEnabled || aspNetUser.OldUserDbkey == null)
                        {
                           // For the users who changed the password after synched from old tables used email for password encryption   
                            encryptpassword = MPGlobals.Encrypt_SHA256(password + email);  
                        }
                        else
                        {
                            // For the users who synched from old tables used UserDbkey  for password encryption    
                            encryptpassword = MPGlobals.Encrypt_SHA256(password + aspNetUser.OldUserDbkey.ToString());
                        }
                    }

                }
                return encryptpassword;
            }
            catch(Exception e)
            {
                return "";
            }
			
		}

		public AuthenticateByEmailViewModel IsAuthenticated(AuthenticateByEmailViewModel authenticateByEmailViewModel)
        {
            authenticateByEmailViewModel.ClientIP = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            authenticateByEmailViewModel.Browser = Request.Headers["User-Agent"];
            string JsonPara = JsonConvert.SerializeObject(authenticateByEmailViewModel);
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.Authenticate_User @Params='{JsonPara}'");
                AuthenticateUser authenticateUser = db.Read<AuthenticateUser>().FirstOrDefault();
                try
                {
					if (authenticateUser is not null)
					{
						if (authenticateUser.IsAuthenticated == true)
						{
                            if( authenticateByEmailViewModel.ReturnUrl.IsNullOrEmpty())
                            {
                                authenticateByEmailViewModel.ReturnUrl = "/Home/Index";
                            }
							
							authenticateByEmailViewModel.IsAuthenticated = authenticateUser.IsAuthenticated;
							List<UserRoles> userRoles = db.Read<UserRoles>().ToList();
							var claims = new List<Claim>();
							claims.Add(new Claim(ClaimTypes.Name, authenticateUser.UserSessionGuid));
							claims.Add(new Claim(ClaimTypes.NameIdentifier, authenticateUser.UserGuid));
							claims.Add(new Claim(ClaimTypes.GivenName, authenticateUser.DisplayName));
							claims.Add(new Claim(ClaimTypes.Email, authenticateUser.Email));
							claims.Add(new Claim(ClaimTypes.Sid, authenticateUser.UserDbkey.ToString()));
							foreach (var item in userRoles)
							{
								claims.Add(new Claim(ClaimTypes.Role, item.RoleGuid));
							}
							var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

							//claims.Add(new Claim(ClaimTypes.Upn, GenerateJwtToken(identity)));
							//identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
							var principal = new ClaimsPrincipal(identity);
							var props = new AuthenticationProperties();
							//props.IsPersistent = model.RememberMe;
							HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props).Wait();
							return authenticateByEmailViewModel;
						}
					}
				}
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }
                return authenticateByEmailViewModel;
            }
        }

        private string GenerateJwtToken(ClaimsIdentity claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddDays(30),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<IActionResult> UnAuthorized(string retUrl = "/Home/Index")
        {
            ViewBag.returnUrl = retUrl;
            return View();
        }



        [AllowAnonymous]
        public IActionResult Impersonate(string id, string key)
        {
            AuthenticateByEmailViewModel authenticateByEmailViewModel = new AuthenticateByEmailViewModel();
            authenticateByEmailViewModel.IsAuthenticated = false;
            try
            {
				if (!id.IsNullOrEmpty())
				{
					authenticateByEmailViewModel.Email = id;
					authenticateByEmailViewModel.Password = key;
					authenticateByEmailViewModel.ReturnUrl = "/Home/Index";
					authenticateByEmailViewModel.AuthenticateBy = LoginBy.Impersonate.ToString();
					authenticateByEmailViewModel = IsAuthenticated(authenticateByEmailViewModel);
					if (authenticateByEmailViewModel.IsAuthenticated == true)
					{
						return Redirect(authenticateByEmailViewModel.ReturnUrl);
					}
				}
			}
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
            return View(authenticateByEmailViewModel);
        }



        [AllowAnonymous]
        public IActionResult TriggerMilestoneMail()
        {
            try
            {
                MPCRS.Utilities.Notification.TriggerMilestoneDueMail();
                return Json(new { MailSent = true });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { MailSent = false });
            }
        }


        [AllowAnonymous]
        public IActionResult ProcessDocIntoVectorDB()
        {
            try
            {
				string result =  Utilities.SaveToVectoDB.ProccessDocumentsIntoVectorDb(null);
				return Json(new { response = result });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { Processed = false });
            }
        }


    }
}
