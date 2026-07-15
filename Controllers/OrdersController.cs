using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using System.Data;
using System.Security.Claims;
using WebGrease.Css;
using Microsoft.AspNetCore.Authorization;
using static MPCRS.Utilities.Constants;
using System.Globalization;
using XAct.Library.Settings;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.Text.RegularExpressions;
using System.Diagnostics.Metrics;



namespace MPCRS.Controllers
{
	[Authorize]
	public class OrdersController : Controller
	{
		private readonly DESI_STFE_PRODContext _dbContext;
		private readonly IConfiguration _configuration;
		private readonly MPDapperContext mPDapperContext;

		public OrdersController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
		{
			_dbContext = context;
			_configuration = configuration;
			this.mPDapperContext = mPDapperContext;
		}

		[Authorize]
		[OrClaimRequirementAttribute(UserPermissions.Casting_Read, UserPermissions.Forging_Read, UserPermissions.Pyro_Read, UserPermissions.Order_Module_User)]
		public IActionResult OrdersList(string Ordertype, string tab = "dashboard-tab")
		{
			var hasCastingReadPermission = UserData.IsAuthorized(User, Constants.UserPermissions.Casting_Read);
			var hasPyroReadPermission = UserData.IsAuthorized(User, Constants.UserPermissions.Pyro_Read);
			var hasForgingReadPermission = UserData.IsAuthorized(User, Constants.UserPermissions.Forging_Read);
			var IsModuleUser = UserData.IsAuthorized(User, Constants.UserPermissions.Order_Module_User);

			if (IsModuleUser)
			{
                return RedirectToAction("Orders", "Orders", new { Ordertype = Ordertype });
            }


			if ((Ordertype == "Casting" && hasCastingReadPermission) || (Ordertype == "Pyro" && hasPyroReadPermission) || (Ordertype == "Forging" && hasForgingReadPermission) || IsModuleUser)
			{
				if (string.IsNullOrEmpty(Ordertype))
				{
					return RedirectToAction("invalidData");
				}
				List<CastingDetailViewModel> castingViewModel = GetCastingDetailViewModel(Ordertype);
                ViewBag.Ordertype = Ordertype;
				ViewBag.Tab = tab;
				return View(castingViewModel);
			}
			else
			{
				return RedirectToAction("UnAuthorized", "Auth");
			}

		}

        public IActionResult Orders(string Ordertype)
		{
			ViewBag.Ordertype = Ordertype;
            List<CastingDetailViewModel> castingViewModel = GetCastingDetailViewModel(Ordertype);
            return View(castingViewModel);
		}


		public IActionResult OrderModuleUserMapping(string Ordertype = "Casting")
		{
			using (var connection = mPDapperContext.CreateConnection())
			{
                ViewBag.Ordertype = Ordertype;
                List<OrderModuleUserMapping> orderModuleUserMapping = new List<OrderModuleUserMapping>();
				var db = connection.QueryMultiple($"OrderModuleUsersList_SSP");
				orderModuleUserMapping = db.Read<OrderModuleUserMapping>().ToList();
				return View(orderModuleUserMapping);
			}
		}

        public IActionResult RemoveOrderModuleUserMapping(int id)
        {
			MPGlobals.ExceSQLNonQuery($"Delete from [dbo].[Order_ModuleUserMapping] where Id = {id}");
            return Json(new { success = true, msg = "Submitted successfully" });
        }


        public List<CastingDetailViewModel> GetCastingDetailViewModel(string Ordertype)
		{
            string UserGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<CastingDetailViewModel> castingDetailViewModels = new List<CastingDetailViewModel>();
            var IsModuleUser = UserData.IsAuthorized(User, Constants.UserPermissions.Order_Module_User);
            using (var connection = mPDapperContext.CreateConnection())
            {
                if (!IsModuleUser)
                {
                    UserGuid = "All";
                }
                var db = connection.QueryMultiple($"dbo.[GenericOrdersLists_SSP] @OrderType='{Ordertype}', @UserGuid= '{UserGuid}'");
                castingDetailViewModels = db.Read<CastingDetailViewModel>().ToList();
            }

            return castingDetailViewModels;
        }

        public IActionResult ViewCastingDetail(string id)
		{
			ViewBag.customAccess = "false";
			var data = _dbContext.CastingDetails.Where(x => x.castingGUID == id).FirstOrDefault();
			if (data != null)
			{
				CastingViewModel castingViewModel = GetCastingDetail(data.CastingDbkey);
				//CastingViewModel castingViewModel = GetCastingDetail(CastingId);
				return View(castingViewModel);
			}
			else
			{
				return RedirectToAction("invalidData");
			}
		}

		public IActionResult OrderDetail(string id, string Ordertype)
		{
			if (string.IsNullOrEmpty(Ordertype))
			{
				return RedirectToAction("invalidData");
			}

			CastingViewModel castingViewModel = new CastingViewModel();
			ViewBag.customAccess = "false";
			ViewBag.Id = id;
			ViewBag.Ordertype = Ordertype;

			if (id == "NEW")
			{
				CastingDetailViewModel castingDetailViewModel_ = new CastingDetailViewModel();
				castingDetailViewModel_.CastingDbkey = 0;
				castingDetailViewModel_.OrderType = Ordertype;
				castingViewModel.castingDetailViewModel = castingDetailViewModel_;
				castingViewModel.CastingItemViewModel = new List<CastingItemViewModel>();
				castingViewModel.receiptSplitVMs = new List<CastingReceiptSplitVM>();
				return View(castingViewModel);
			}

			var data = _dbContext.CastingDetails.Where(x => x.castingGUID == id).FirstOrDefault();
			if (data != null)
			{
				castingViewModel = GetCastingDetail(data.CastingDbkey);
				//CastingViewModel castingViewModel = GetCastingDetail(CastingId);
				return View(castingViewModel);
			}
			else
			{
				return RedirectToAction("invalidData");
			}
		}


		public IActionResult CreateCustmAccessLink(string CastingGUID)
		{
			using (_dbContext)
			{
				AppSetting appSettings = _dbContext.AppSettings.Where(x => x.AppSettingType == "CastinCustomAccess").FirstOrDefault();

				var availabeLink = _dbContext.SOP_CustomAccessLinks.Where(x => x.BuildGuid == CastingGUID && x.UpdatedOn > DateTime.Now.AddHours(-10)).OrderByDescending(x => x.UpdatedOn).FirstOrDefault();
				if (availabeLink != null)
				{
					var jsoninfo = JsonConvert.DeserializeObject<SOPCustomAccessLink>(availabeLink.LinkDataJson);
					return Json(new { success = true, linkguid = jsoninfo.Link });
				}


				SOP_CustomAccessLink sOP_CustomAccessLink_db = new SOP_CustomAccessLink();
				sOP_CustomAccessLink_db.BuildGuid = CastingGUID;
				sOP_CustomAccessLink_db.UpdatedOn = DateTime.Now;
				sOP_CustomAccessLink_db.Updated_By = User.FindFirst(ClaimTypes.NameIdentifier).Value;

				Guid newGuid = Guid.NewGuid();
				sOP_CustomAccessLink_db.LinkGuid = newGuid.ToString();
				SOPCustomAccessLink sOPCustomAccessLink = new();
				// sOPCustomAccessLink.LinkGuid = Guid.NewGuid().ToString();
				sOPCustomAccessLink.AccessBuildGuid = CastingGUID;
				sOPCustomAccessLink.AccessGrantedBy = User.FindFirst(ClaimTypes.NameIdentifier).Value;
				sOPCustomAccessLink.access_validity = DateTime.Now.AddDays(1);
				sOPCustomAccessLink.Link = appSettings.DataJson + sOP_CustomAccessLink_db.LinkGuid;
				sOPCustomAccessLink.LinkGuid = sOP_CustomAccessLink_db.LinkGuid;
				sOP_CustomAccessLink_db.LinkDataJson = JsonConvert.SerializeObject(sOPCustomAccessLink);
				_dbContext.Add(sOP_CustomAccessLink_db);

				_dbContext.SaveChanges();
				return Json(new { success = true, linkguid = sOPCustomAccessLink.Link });
			}
		}

		[HttpGet]
		public IActionResult OrderModuleCustomAccess(string CastingGUID)
		{
            using (var connection = mPDapperContext.CreateConnection())
			{
                OrdersModuleUserDetails ordersModuleUserDetails = new OrdersModuleUserDetails();
                var db = connection.QueryMultiple($"OrderModuleUsers_SSP  @Guid = '{CastingGUID}'");
                ordersModuleUserDetails.castingDetail = db.Read<CastingDetail>().FirstOrDefault();
                ordersModuleUserDetails.orderModuleUserMappings = db.Read<OrderModuleUserMapping>().ToList();
                return PartialView(ordersModuleUserDetails);
            }            
		}

        [HttpPost]
        public IActionResult OrderModuleCustomAccess([FromBody] List<OrderModuleUserMapping> orderModuleUserMappings)
        {
            if (orderModuleUserMappings == null || !orderModuleUserMappings.Any())
            {
                return Json(new { success = false, msg = "No data received" });
            }
            using (_dbContext)
			{
				MPGlobals.ExceSQLNonQuery($"Delete from [dbo].[Order_ModuleUserMapping] where [OrderId] = '{orderModuleUserMappings[0].OrderId}'");
                int UserId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                foreach (var item in orderModuleUserMappings)
                {
                    Order_ModuleUserMapping orderModuleUserMapping = new Order_ModuleUserMapping();
                    orderModuleUserMapping.OrderId = item.OrderId;
                    orderModuleUserMapping.OrderType = item.OrderType;
                    orderModuleUserMapping.UserGuid = item.UserGuid;
                    orderModuleUserMapping.UpdateBy = UserId;
                    orderModuleUserMapping.UpdateOn = DateTime.Now;
					_dbContext.Add(orderModuleUserMapping);

                }
				_dbContext.SaveChanges();
            }
            return Json(new { success = true, msg = "Submitted successfully" });
        }


        [AllowAnonymous]
		public IActionResult CustomAccess(string accessToken = "")
		{
			using (_dbContext)
			{
				ViewBag.customAccess = "true";
                ViewBag.customAccess_casting = "true";
                ViewBag.accessExpired = "false";
				SOPCustomAccessLink linkData = new();
				SOP_CustomAccessLink sOP_CustomAccessLink = _dbContext.SOP_CustomAccessLinks.Where(x => x.LinkGuid == accessToken).FirstOrDefault();
				if (sOP_CustomAccessLink != null)
				{
					linkData = JsonConvert.DeserializeObject<SOPCustomAccessLink>(sOP_CustomAccessLink.LinkDataJson);

					ViewBag.customLinkData = sOP_CustomAccessLink.LinkDataJson;
					if (linkData.access_validity < DateTime.Now)
					{
						ViewBag.accessExpired = "true";
						return View();
					}

					if (User.Identity.IsAuthenticated == false)
					{
						var authresult = TemporaryImpersonate(linkData.AccessGrantedBy);
						return View();
					}

					var data = _dbContext.CastingDetails.Where(x => x.castingGUID == sOP_CustomAccessLink.BuildGuid).FirstOrDefault();
					if (data != null)
					{
						CastingViewModel castingViewModel = GetCastingDetail(data.CastingDbkey);
						//CastingViewModel castingViewModel = GetCastingDetail(CastingId); 
						return View("OrderDetail", castingViewModel);
					}

					ViewBag.accessExpired = "true";
					return View();

				}
				else
				{
					ViewBag.accessExpired = "true";
					return View();
				}
			}
		}


		public bool TemporaryImpersonate(string userGUID)
		{
			AuthenticateByEmailViewModel authenticateByEmailViewModel = new AuthenticateByEmailViewModel();
			authenticateByEmailViewModel.ClientIP = Request.HttpContext.Connection.RemoteIpAddress.ToString();
			authenticateByEmailViewModel.Browser = Request.Headers["User-Agent"];
			authenticateByEmailViewModel.AuthenticateBy = userGUID;

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
							authenticateByEmailViewModel.ReturnUrl = "/Home/Index";
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
							return User.Identity.IsAuthenticated;
						}
					}
					return false;
				}
				catch (Exception ex)
				{
					ErrorHandler.LogException(ex);
					return false;
				}
			}
		}

		public IActionResult invalidData()
		{

			return View();
		}


		public CastingViewModel GetCastingDetail(int CastingId)
		{
			CastingViewModel castingViewModel = new CastingViewModel();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"dbo.[GenericOrderDetail_SSP]  @GenericOrderId={CastingId}");
				castingViewModel.castingDetailViewModel = db.Read<CastingDetailViewModel>().FirstOrDefault();
				castingViewModel.CastingItemViewModel = db.Read<CastingItemViewModel>().ToList();
				//castingViewModel.vendors = db.Read<Vendor>().ToList();
				castingViewModel.castingReceiptViewModel = db.Read<CastingReceiptViewModel>().ToList();
				castingViewModel.receiptSplitVMs = db.Read<CastingReceiptSplitVM>().ToList();
				castingViewModel.receiptBatchSummary = db.Read<CastingReceiptBatchSummary>().ToList();
				return castingViewModel;
			}
		}

		//old method name in casting : CastingOrder
		[HttpGet]
		public IActionResult OrderForm(int CastingId, string Ordertype)
		{

			if (string.IsNullOrEmpty(Ordertype))
			{
				return RedirectToAction("invalidData");
			}

			CastingViewModel castingViewModel = new CastingViewModel();
			CastingDetailViewModel castingorderModel = new CastingDetailViewModel();
			if (CastingId == 0)
			{
				return PartialView(castingorderModel);
			}
			else
			{
				castingViewModel = GetCastingDetail(CastingId);
				castingorderModel = castingViewModel.castingDetailViewModel;
				return PartialView(castingorderModel);
			}
		}

		[HttpPost]
		public IActionResult SaveOrder(CastingDetailViewModel castingDetailViewModel)
		{
			using (_dbContext)
			{
				int dbkey = 0;
				Guid newGuid = Guid.NewGuid();
				int UserId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
				CastingDetail castingDetail = new CastingDetail();
				if (castingDetailViewModel.CastingDbkey != 0)
				{
					castingDetail = _dbContext.CastingDetails.Where(x => x.CastingDbkey == castingDetailViewModel.CastingDbkey).FirstOrDefault();
				}
				castingDetail.DemandNumber = castingDetailViewModel.DemandNumber;
				castingDetail.OrderDate = castingDetailViewModel.OrderDate;
				castingDetail.OrderStatus = castingDetailViewModel.OrderStatus;
				castingDetail.OrderType = castingDetailViewModel.OrderType;
				castingDetail.MMGOrderNumber = castingDetailViewModel.MMGOrderNumber;
				castingDetail.Remarks = castingDetailViewModel.Remarks;
				castingDetail.OrderNumbers = castingDetailViewModel.OrderNumbers;
				castingDetail.DemandingOfficer = castingDetailViewModel.DemandingOfficer;
				castingDetail.DemandDesc = castingDetailViewModel.DemandDesc;
				castingDetail.UpdatedBy = UserId;
				castingDetail.UpdatedOn = DateTime.Now;

				if (castingDetailViewModel.CastingDbkey == 0)
				{
					castingDetail.Isdeleted = false;
					castingDetail.castingGUID = newGuid.ToString();
					castingDetail.CreatedBy = UserId;
					castingDetail.CreatedOn = DateTime.Now;
					_dbContext.Add(castingDetail);
					_dbContext.SaveChanges();
				}
				else
				{
					_dbContext.Entry(castingDetail).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
					_dbContext.SaveChanges();
				}
				dbkey = castingDetail.CastingDbkey;
				return Json(new { success = true, castingGUID = castingDetail.castingGUID, msg = "Submitted successfully" });

			}

		}


		//old method name in casting : CastingItems
		[HttpGet]
		public IActionResult OrderItems(int OrderId)
		{
			CastingViewModel castingViewModel = GetCastingDetail(OrderId);
			return PartialView(castingViewModel);
		}

		//old method name in casting : CastingItems
		[HttpPost]
		public IActionResult OrderItems([FromBody] IEnumerable<CastingItemViewModel> castingItemViewModels)
		{
			using (_dbContext)
			{
				int UserId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
				foreach (var item in castingItemViewModels)
				{
					CastingItem castingItem = new CastingItem();
					if (item.CastingItemKey != 0)
					{
						castingItem = _dbContext.CastingItems.Where(x => x.CastingItemKey == item.CastingItemKey).FirstOrDefault();
					}
					castingItem.CastingItemKey = item.CastingItemKey;
					castingItem.CastingDbkey = item.CastingDbkey;
					castingItem.EnginePartDbkey = item.EnginePartDbkey;
					castingItem.ItemDescription = item.ItemDescription;
					castingItem.Vendor = item.Vendor;
					castingItem.OrderNumber = item.OrderNumber;
					castingItem.DeliveryDate = item.DeliveryDate;
					castingItem.RawMaterial = item.RawMaterial;
					castingItem.GTREDrgNo = item.GTREDrgNo;
					castingItem.OrderQty = item.OrderQty;
					castingItem.UpdatedBy = UserId;
					castingItem.UpdatedOn = DateTime.Now;
					castingItem.TestSpecimen = item.TestSpecimen;

                    if (item.CastingItemKey == 0)
					{
						castingItem.CreatedBy = UserId;
						castingItem.CreatedOn = DateTime.Now;
						_dbContext.Add(castingItem);
					}
					else
					{
						_dbContext.Entry(castingItem).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
					}
				}
				_dbContext.SaveChanges();
			}
			return Json(new { success = true, msg = "Successfully Saved" });
		}

		// old method name: CastingReceiptItems
		[HttpGet]
		public IActionResult OrderReceiptItems(int CastingId, string ReceiptGuid = "")
		{
			CastingViewModel castingViewModel = GetCastingDetail(CastingId);
			if (!string.IsNullOrEmpty(ReceiptGuid))
			{
				castingViewModel.castingReceiptViewModel = castingViewModel.castingReceiptViewModel.Where(x => x.ReceiptGuid == ReceiptGuid).ToList();
				ViewBag.ReceiptNumber = castingViewModel.castingReceiptViewModel.FirstOrDefault().ReceiptNumber;
				ViewBag.ReceiptDate = castingViewModel.castingReceiptViewModel.FirstOrDefault().ReceiptDate;
				ViewBag.ReceiptGuid = ReceiptGuid;
			}
			return PartialView(castingViewModel);
		}

		// old method : DetailedCastingData
		[HttpGet]
		public IActionResult DetailedReceiptData(string OrderType, int partKey = 0, bool tableOnly = false)
		{
			if (string.IsNullOrEmpty(OrderType))
			{
				return RedirectToAction("invalidData");
			}
			List<DetailedCastingDataVM> castingData = new List<DetailedCastingDataVM>();
			ViewBag.tableOnly = tableOnly;
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"dbo.GetGenericOrderDetailsSplitLevel @partKey={partKey}, @orderType='{OrderType}'");
				castingData = db.Read<DetailedCastingDataVM>().ToList();
			}
			ViewBag.orderType = OrderType;

			return View(castingData);
		}

		// Old Method name: CastingReceiptLevelSummary
		[HttpGet]
		public IActionResult ReceiptLevelSummary(string OrderType, int partKey = 0)
		{
			List<CastingReceiptLevelSummaryVM> castingData = new List<CastingReceiptLevelSummaryVM>();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"dbo.GenericOrderReceiptLevelSummary @partKey={partKey}, @OrderType='{OrderType}'");
				castingData = db.Read<CastingReceiptLevelSummaryVM>().ToList();
			}

			return View(castingData);
		}



		[HttpGet]
		public IActionResult OrderSummary(string Ordertype)
		{
			if (string.IsNullOrEmpty(Ordertype))
			{
				return RedirectToAction("invalidData");
			}

			List<CastingOrderSummaryVM> CastingSummary = new List<CastingOrderSummaryVM>();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"dbo.[GenericOrderSummaryData] @OrderType='{Ordertype}'");
				CastingSummary = db.Read<CastingOrderSummaryVM>().ToList();
			}
			return View(CastingSummary);
		}



        [HttpGet]
        public IActionResult Dashboard(string Ordertype)
        {
            ViewBag.Engines = MPGlobals.GetDataForDatalist("Select [DataJson] from [dbo].[AppSettings] where [AppSettingType] = 'Engines'").Rows[0][0].ToString();
            try
			{
                if (string.IsNullOrEmpty(Ordertype))
                {
                    return RedirectToAction("invalidData");
                }

                List<CastingOrderSummaryVM> CastingSummary = new List<CastingOrderSummaryVM>();
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var db = connection.QueryMultiple($"dbo.[GenericOrderDashboard] @OrderType='{Ordertype}'");
                    CastingSummary = db.Read<CastingOrderSummaryVM>().ToList();
                }            
                return View(CastingSummary);
            }
			catch (Exception ex)
			{
				ErrorHandler.LogException(ex);
                return View(new List<CastingOrderSummaryVM>());

            }

        }



        // old method name: CastingPartOrderData
        [HttpGet]
		public IActionResult PartOrderData(int partkey, string OrderType)
		{
			if (string.IsNullOrEmpty(OrderType))
			{
				return RedirectToAction("invalidData");
			}
			List<CastingOrderSummaryVM> CastingSummary = new List<CastingOrderSummaryVM>();
			string cmd = $"dbo.[GetGenericOrderPartOrderData] @partKey = {partkey}, @OrderType='{OrderType}'";
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"dbo.[GetGenericOrderPartOrderData] @partKey = {partkey}, @OrderType='{OrderType}'");
				CastingSummary = db.Read<CastingOrderSummaryVM>().ToList();
			}

			return View(CastingSummary);
		}

		[HttpGet]
		public IActionResult ItemLevelSummary(int partKey, string OrderType)
		{
			if (string.IsNullOrEmpty(OrderType))
			{
				return RedirectToAction("invalidData");
			}
			List<CastingReceiptBatchSummary> CastingSummary = new List<CastingReceiptBatchSummary>();
			string cmd = $"dbo.[GetGenericOrderPartOrderData] @partKey = {partKey}, @OrderType='{OrderType}'";
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"dbo.[ItemLevel_BatchSummary] @partKey = {partKey}");
				CastingSummary = db.Read<CastingReceiptBatchSummary>().ToList();
			}


			return View(CastingSummary);

		}


		[HttpGet]
		public IActionResult CastingReceiptItemSplit(int CastingDbkey, int CastingReceiptDbkey = 0, int CastingSplitKey = -1)
		{
			CastingViewModel castingViewModel = new CastingViewModel();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"dbo.[GetCasting_Recipt_Split] @CastingId={CastingDbkey},@CastingReceiptDbkey ={CastingReceiptDbkey},@CastingSplitKey ={CastingSplitKey}");
				castingViewModel.receiptSplitVMs = db.Read<CastingReceiptSplitVM>().ToList();
			}
			ViewBag.CastingReceiptDbkey = CastingReceiptDbkey;
			ViewBag.CastingDbkey = CastingDbkey;
			return View(castingViewModel);
		}


		[HttpGet]
		public IActionResult CastingReceiptItemSplitDetail(int CastingDbkey, int CastingReceiptDbkey = 0, int CastingSplitKey = -1)
		{
			CastingViewModel castingViewModel = new CastingViewModel();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"dbo.[GetCasting_Recipt_Split] @CastingId={CastingDbkey},@CastingReceiptDbkey ={CastingReceiptDbkey},@CastingSplitKey ={CastingSplitKey}");
				castingViewModel.receiptSplitVMs = db.Read<CastingReceiptSplitVM>().ToList();
			}
			ViewBag.CastingReceiptDbkey = CastingReceiptDbkey;
			ViewBag.CastingDbkey = CastingDbkey;
			return View(castingViewModel);
		}

		[HttpGet]
		public IActionResult CastingSplitDocChecklist()
		{
			return PartialView();
		}

		[HttpGet]
		public IActionResult DeleteQtySplit(int CastingQtySplitKey)
		{
			if (CastingQtySplitKey > 0)
			{
				var item = _dbContext.CastingReceiptQtySplits.Where(x => x.QtySplitKey == CastingQtySplitKey).FirstOrDefault();
				if (item != null)
				{
					_dbContext.CastingReceiptQtySplits.Remove(item);
					_dbContext.SaveChanges();
				}
			}
			return Json(new { success = true, msg = "Deleted successfully" });
		}


		[HttpGet]
		public IActionResult CastingQtySplit(int CastingSplitKey = 0)
		{
			List<CastingReceiptQtySplit> castingQtySplit = _dbContext.CastingReceiptQtySplits.Where(x => x.ReceiptsItemSplitKey == CastingSplitKey).ToList();
			return PartialView(castingQtySplit);
		}

		//Old method name: CastingReceiptItemSplitModel
		[HttpGet]
		public IActionResult ReceiptItemSplitModel(int CastingDbkey, int CastingSplitKey = -1)
		{
			CastingReceiptDetailViewModel castingReceiptSplitVMs = new();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"dbo.[GenericOrders_Receipts_SSP] @CastingId={CastingDbkey},@ReceiptDbkey={CastingSplitKey}");
				castingReceiptSplitVMs.CastingItemViewModel = db.Read<CastingItemViewModel>().ToList();
				castingReceiptSplitVMs.receiptSplitVM = db.Read<CastingReceiptSplitVM>().FirstOrDefault();
				castingReceiptSplitVMs.attachment = db.Read<Attachment>().ToList();

				castingReceiptSplitVMs.QtySplit = _dbContext.CastingReceiptQtySplits.Where(x => x.ReceiptsItemSplitKey == CastingSplitKey).ToList();

				if (castingReceiptSplitVMs.QtySplit.Count == 0)
				{
					var qtySplit = new CastingReceiptQtySplit();
					qtySplit.ReceiptsItemSplitKey = CastingSplitKey;
					qtySplit.SplitQty = 0;
					castingReceiptSplitVMs.QtySplit.Add(qtySplit);
				}


				if (castingReceiptSplitVMs.receiptSplitVM == null)
				{
					CastingReceiptSplitVM castingReceiptSplitVM_ = new CastingReceiptSplitVM();
					castingReceiptSplitVM_.Id = 0;
					castingReceiptSplitVM_.CastingDbkey = CastingDbkey;
					castingReceiptSplitVM_.ReceiptDate = DateTime.Now;
					castingReceiptSplitVMs.receiptSplitVM = castingReceiptSplitVM_;
				}

			}
			return View(castingReceiptSplitVMs);
		}


        [HttpPost]
        public async Task<IActionResult> SaveReceiptSplitModelAsync([FromForm] UploadViewModel model)
        {
            try
            {
                List<AttachmentVM> attachments = JsonConvert.DeserializeObject<List<AttachmentVM>>(model.filesData);
                CastingReceiptSplitVM castingReceiptSplit = JsonConvert.DeserializeObject<CastingReceiptSplitVM>(model.JsonData);
                List<CastingReceiptQtySplit> castingQtySplit = JsonConvert.DeserializeObject<List<CastingReceiptQtySplit>>(model.qtySplits);
                CastingReceiptsItemSplit CastingreceiptsItemSplit = new CastingReceiptsItemSplit();
                string AttachmentDbKey = "";

                if (castingReceiptSplit.Attachments != null)
                {
                    AttachmentDbKey = castingReceiptSplit.Attachments;
                }

                int counter = 0;
                foreach (var item in attachments)
                {
                    var userguid = User.Identity.Name;
                    string systemfilename = string.Empty;
                    string filename = string.Empty;
                    string SavePath = string.Empty;
                    item.uploadeddocument = model.files[counter];
                    item.AttachmentGUID = Guid.NewGuid().ToString();

                    if (item.uploadeddocument != null)
                    {
                        Models.Attachment att = new();
                        filename = item.uploadeddocument.FileName;
                        systemfilename = item.AttachmentGUID + Path.GetExtension(item.uploadeddocument.FileName);
                        SavePath = GetDestinationFolder() + systemfilename;

                        using (var stream = new FileStream(SavePath, FileMode.Create))
                        {
                            item.uploadeddocument.CopyTo(stream);
                        }

                        att.Attachment_FileName = systemfilename;
                        att.Orginal_File_Name = filename;
                        att.Attachment_location = @"/Attachments/Casting_Forging_File/";
                        att.Attachment_type = item.Attachment_type;
                        att.Source_table_key = item.Source_table_key;
                        att.Source_table = item.Source_table;
                        att.Revision = item.Revision;
                        att.File_Revision = item.File_Revision;
                        att.AttachmentGUID = item.AttachmentGUID;
                        att.Attachment_type = item.Attachment_type;
                        att.File_DVD_Num = item.File_DVD_Num;
                        att.Updated_on = DateTime.Now;
                        att.Updated_by = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                        _dbContext.Attachments.Add(att);
                        _dbContext.SaveChanges();

                        if (AttachmentDbKey == "")
                        {
                            AttachmentDbKey = att.Attachment_Db_Key.ToString();
                        }
                        else
                        {
                            AttachmentDbKey = AttachmentDbKey + "," + att.Attachment_Db_Key;
                        }
                    }
                    counter++;
                }

                CastingreceiptsItemSplit.Id = castingReceiptSplit.Id;
                CastingreceiptsItemSplit.BatchNumber = castingReceiptSplit.BatchNumber;
                CastingreceiptsItemSplit.HeatNumber = castingReceiptSplit.HeatNumber;
                CastingreceiptsItemSplit.SerialNumber = castingReceiptSplit.SerialNumber;
                CastingreceiptsItemSplit.Revision = castingReceiptSplit.Revision;
                CastingreceiptsItemSplit.Status = castingReceiptSplit.Status;
                CastingreceiptsItemSplit.Attachments = AttachmentDbKey;
                CastingreceiptsItemSplit.VendorDrawingNo = castingReceiptSplit.VendorDrawingNo;
                CastingreceiptsItemSplit.OrderItemKey = castingReceiptSplit.OrderItemKey;
                CastingreceiptsItemSplit.ReceiptNumber = castingReceiptSplit.ReceiptNumber;
                CastingreceiptsItemSplit.ReceiptDate = castingReceiptSplit.ReceiptDate;
                CastingreceiptsItemSplit.Remarks = castingReceiptSplit.Remarks;
                CastingreceiptsItemSplit.UpdatedOn = DateTime.Now;
                CastingreceiptsItemSplit.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                if (CastingreceiptsItemSplit.Id != 0)
                {
                    _dbContext.CastingReceiptsItemSplits.Entry(CastingreceiptsItemSplit).State = EntityState.Modified;
                }
                else
                {
                    _dbContext.CastingReceiptsItemSplits.Add(CastingreceiptsItemSplit);
                }
                _dbContext.SaveChanges();

                // FIXED: Use AsNoTracking() to avoid tracking conflicts
                var existingQtySplits = _dbContext.CastingReceiptQtySplits
                    .AsNoTracking() // ← ADDED: Don't track these entities
                    .Where(x => x.ReceiptsItemSplitKey == CastingreceiptsItemSplit.Id)
                    .ToList();

                // Get the IDs of splits that are in the incoming data
                var incomingQtySplitKeys = castingQtySplit
                    .Where(x => x.QtySplitKey != 0)
                    .Select(x => x.QtySplitKey)
                    .ToList();

                // Find splits that exist in DB but NOT in incoming data = these were deleted
                var splitsToDelete = existingQtySplits
                    .Where(x => !incomingQtySplitKeys.Contains(x.QtySplitKey))
                    .ToList();

                // Delete the removed splits
                if (splitsToDelete.Any())
                {
                    // FIXED: Since these are not tracked, we need to attach them before deleting
                    _dbContext.CastingReceiptQtySplits.RemoveRange(splitsToDelete);
                    _dbContext.SaveChanges(); // Save deletions first
                }

                // Now process the incoming splits (add/update)
                foreach (var item in castingQtySplit)
                {
                    item.ReceiptsItemSplitKey = CastingreceiptsItemSplit.Id;
                    item.UpdatedOn = DateTime.Now;
                    item.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    if (item.QtySplitKey != 0)
                    {
                        // UPDATE existing - Use Update() instead of setting State
                        _dbContext.CastingReceiptQtySplits.Update(item); // ← FIXED
                    }
                    else
                    {
                        // ADD new
                        _dbContext.CastingReceiptQtySplits.Add(item);
                    }
                }

                _dbContext.SaveChanges();
                return Json(new { success = true, msg = "Successfully Saved" });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        private string GetDestinationFolder()
		{
			string directoryname = @"/Attachments/Casting_Forging_File/";
			string SaveDirectory = string.Empty;
			SaveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/" + directoryname);
			if (!Directory.Exists(SaveDirectory))
			{
				Directory.CreateDirectory(SaveDirectory);
			}
			return SaveDirectory + "/";
		}

		private string GetGenericMateriaIssueDocDestinationFolder()
		{
			string directoryname = @"/Attachments/Casting_MaterialIssue_Docs/";
			string SaveDirectory = string.Empty;
			SaveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/" + directoryname);
			if (!Directory.Exists(SaveDirectory))
			{
				Directory.CreateDirectory(SaveDirectory);
			}
			return SaveDirectory + "/";
		}

		//old method name in casting : DeleteCastingComponents
		public ActionResult DeleteOrderComponents(string key, string table)
		{
			int UserId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
			string cmdstr = $"exec [dbo].[DeleteGenericOrderDetail_USP] @key='{key}',@table = '{table}',@userid={UserId}";
			MPGlobals.ExceSQLNonQuery(cmdstr);
			return Json(new { success = true, msg = "Removed successfully" });
		}


		[Authorize]
		//	[ClaimRequirement(UserPermissions.Casting_Delete)]
		public ActionResult DeleteReceiptDocument(int documentId = 0, int receiptDbkey = 0)
		{
			try
			{
				using (_dbContext)
				{
					bool validtodelete = true;
					if (validtodelete)
					{
						MPGlobals.ExceSQLNonQuery($"Delete from [dbo].[Attachments] where Attachment_Db_Key ={documentId}");
						return Json(new { success = true });
					}
					else
					{
						return Json(new { success = false });
					}
				}
			}
			catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }

		}


		[Authorize]
		public ActionResult UpdateReceiptDocType(int documentId = 0, int doctype = 0)
		{
			try
			{
				using (_dbContext)
				{
					MPGlobals.ExceSQLNonQuery($"Update Attachments set File_DVD_Num = {doctype} where  Attachment_Db_Key = {documentId}");
					return Json(new { success = true });
				}
			}
			catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }

		}

		public IActionResult IssueDetails(string OrderType)
		{

			List<GenericMaterialIssuedDetialsVM> MaterialIssueData = new List<GenericMaterialIssuedDetialsVM>();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"[dbo].[Get_GenericMaterialIssuedDetails_SSP] @OrderType ={OrderType}");
				MaterialIssueData = db.Read<GenericMaterialIssuedDetialsVM>().ToList();
			}
			ViewBag.OrderType = OrderType;
			return View(MaterialIssueData);
		}

		public IActionResult NewGenericMaterialIssue(string OrderType, int IssueDbKey = 0)
		{
			ViewBag.OrderType = OrderType;
			List<GenericIssuesViewModel> IssueData = new List<GenericIssuesViewModel>();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"dbo.GenericIssueDetails_SSP @OrderType ={OrderType}, @IssueDbkey={IssueDbKey}");
				IssueData = db.Read<GenericIssuesViewModel>().OrderBy(x => x.IssueItemKey).ToList();
			}
			return View(IssueData);
		}

		[HttpPost] 
		//public IActionResult SaveGenericMaterialIssue([FromBody] GenericMaterialIssueVM genericMaterialIssueVM)
		public IActionResult SaveGenericMaterialIssue()
		{
			GenericMaterialIssueVM genericMaterialIssueVM = new();
			var casting_MaterialIssue_VM = Request.Form["casting_MaterialIssue_VM"];
			var casting_MaterialIssue_Items_VM = Request.Form["casting_MaterialIssue_Items_VM"];			
			var FormFiles = Request.Form.Files;
			genericMaterialIssueVM.casting_MaterialIssue_VM = JsonConvert.DeserializeObject<Casting_MaterialIssue_VM>(casting_MaterialIssue_VM);
			genericMaterialIssueVM.casting_MaterialIssue_Items_VM = JsonConvert.DeserializeObject<List<Casting_MaterialIssue_Item_VM>>(casting_MaterialIssue_Items_VM);

			if (genericMaterialIssueVM != null)
				{
					//var FormFiles = genericMaterialIssueVM.FormFiles;
					var user = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
					Casting_MaterialIssue casting_MaterialIssue = new Casting_MaterialIssue();
					casting_MaterialIssue.IssueDbKey = genericMaterialIssueVM.casting_MaterialIssue_VM.IssueDbKey;
					casting_MaterialIssue.IssueDate = genericMaterialIssueVM.casting_MaterialIssue_VM.IssueDate;
					casting_MaterialIssue.VendorKey = genericMaterialIssueVM.casting_MaterialIssue_VM.VendorKey;
					casting_MaterialIssue.Issue_type = genericMaterialIssueVM.casting_MaterialIssue_VM.Issue_type;
					casting_MaterialIssue.Reference_No = genericMaterialIssueVM.casting_MaterialIssue_VM.Reference_No;
					casting_MaterialIssue.UpdatedBy = user;
					casting_MaterialIssue.UpdatedOn = DateTime.Now;
					if ( genericMaterialIssueVM.casting_MaterialIssue_VM.IssueDbKey == 0)
					{
						_dbContext.Add(casting_MaterialIssue);
					}
					else
					{
						_dbContext.Casting_MaterialIssues.Entry(casting_MaterialIssue).State = EntityState.Modified;
					}
					_dbContext.SaveChanges();

					var Issue_DbKey = casting_MaterialIssue.IssueDbKey;
					int counter = 1;
				foreach (var item in genericMaterialIssueVM.casting_MaterialIssue_Items_VM)
				{
					Casting_MaterialIssue_Item fileSavedInfo = UploadJobCardDocument(FormFiles, counter, (int)item.IssueItemKey, Issue_DbKey);

					Casting_MaterialIssue_Item casting_MaterialIssue_Item = new Casting_MaterialIssue_Item();
						casting_MaterialIssue_Item.IssueItemKey = item.IssueItemKey;
						casting_MaterialIssue_Item.IssueDbKey = Issue_DbKey;
						casting_MaterialIssue_Item.QtySplitKey = item.QtySplitKey;
						casting_MaterialIssue_Item.IssueQty = item.IssueQty;
						casting_MaterialIssue_Item.IssueSlNos = item.IssueSlNos;
						casting_MaterialIssue_Item.UpdatedBy = user;
						casting_MaterialIssue_Item.UpdatedOn = DateTime.Now;
						casting_MaterialIssue_Item.ForEngine = item.ForEngine;
						casting_MaterialIssue_Item.JobCardNumber = item.JobCardNumber;
						casting_MaterialIssue_Item.JCFileLocation = item.JCFileLocation;
						casting_MaterialIssue_Item.JCFileName = item.JCFileName;


						if (fileSavedInfo != null)
						{
							casting_MaterialIssue_Item.JCFileLocation = fileSavedInfo.JCFileLocation;
							casting_MaterialIssue_Item.JCFileName = fileSavedInfo.JCFileName;
						}
						if ( item.IssueItemKey == 0)
						{
							_dbContext.Casting_MaterialIssue_Items.Add(casting_MaterialIssue_Item);
						}
						else
						{
							_dbContext.Casting_MaterialIssue_Items.Entry(casting_MaterialIssue_Item).State = EntityState.Modified;
						}
					counter++;
				}
					_dbContext.SaveChanges();

					return Json(new { success = true, orderType = genericMaterialIssueVM.casting_MaterialIssue_VM.Issue_type });
				}
				else
				{
					return Json(new { success = false });
				}

		}
		private Casting_MaterialIssue_Item UploadJobCardDocument(IFormFileCollection UploadedFiles, int counter, int Issue_Item_Dbkey, int Issue_Dbkey)
		{
			Casting_MaterialIssue_Item casting_material_Issue_Item = new Casting_MaterialIssue_Item();

			try
			{
				if (UploadedFiles != null && UploadedFiles.Count > 0)
				{

					var UploadedFile = UploadedFiles.FirstOrDefault(x => x.Name == counter.ToString());
					if (UploadedFile != null)
					{
						string systemfilename = string.Empty;
						string filename = string.Empty;
						string SavePath = string.Empty;
						filename = UploadedFile.FileName;
						systemfilename = Guid.NewGuid().ToString() + Path.GetExtension(UploadedFile.FileName);
						SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Attachments/Casting_MaterialIssue_Docs");

						Regex reg = new Regex("[*'\",_&#^@ ()!`~{};:<>?/+-]");
						filename = reg.Replace(filename, "_");
						string originalFileName = filename.Trim();
						if (!Directory.Exists(SavePath))
						{
							Directory.CreateDirectory(SavePath);
						}

						SavePath = Path.Combine(SavePath, systemfilename);

						using (var stream = new FileStream(SavePath, FileMode.Create))
						{
							UploadedFile.CopyTo(stream);
						}

						casting_material_Issue_Item.JCFileName = originalFileName;
						casting_material_Issue_Item.JCFileLocation = "/Attachments/Casting_MaterialIssue_Docs/" + systemfilename;
						return casting_material_Issue_Item;						
					}

				}

			}
			catch (Exception ex)
			{
				ErrorHandler.LogException(ex);
			}
			return null;
		}

		public IActionResult DeleteGenericMaterialIssue(int IssueDbkey)
		{
			if (IssueDbkey > 0)
			{
				MPGlobals.ExceSQLNonQuery($"Delete  FROM [dbo].[Casting_MaterialIssue] where IssueDbKey  ={IssueDbkey} ");
				MPGlobals.ExceSQLNonQuery($"Delete  FROM [dbo].[Casting_MaterialIssue_Items] where IssueDbKey  ={IssueDbkey} ");
				return Json(new { success = true });
			}
			else
			{
				return Json(new { success = false });
			}
		}

		public IActionResult DeleteGenericMaterialIssue_Item(int IssueItemKey)
		{
			if (IssueItemKey > 0)
			{
				MPGlobals.ExceSQLNonQuery($"Delete  FROM [dbo].[Casting_MaterialIssue_Items] where IssueItemKey  ={IssueItemKey} ");
				return Json(new { success = true });
			}
			else
			{
				return Json(new { success = false });
			}
		}

		public IActionResult RemainingData(string OrderType, int IssueDbKey)
		{
			ViewBag.OrderType = OrderType;
			List<GenericIssuesViewModel> IssueData = new List<GenericIssuesViewModel>();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"[dbo].[GenericMaterialIssue_RemainingData] @IssueDbKey = {IssueDbKey} , @OrderType = {OrderType}");
				IssueData = db.Read<GenericIssuesViewModel>().ToList();
			}
			return PartialView(IssueData);
		}

		[HttpPost]
		public IActionResult AddRowToGenericMaterialIssue_Items([FromBody] Casting_MaterialIssue_Item casting_MaterialIssue_item )
		{
			if (casting_MaterialIssue_item != null)
			{
				
				casting_MaterialIssue_item.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
				casting_MaterialIssue_item.UpdatedOn = DateTime.Now;
				_dbContext.Add(casting_MaterialIssue_item);
				_dbContext.SaveChanges();
				return Json(new { success = true });
			}
			else
			{
				return Json(new { success = false });
			}
		}

		[HttpGet]
		public IActionResult GenericMaterialIssueSummary(string OrderType)
		{
			ViewBag.OrderType = OrderType;
			List<GenericMaterialIssueSummary_VM> SummaryData = new List<GenericMaterialIssueSummary_VM>();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"[dbo].[GenericMaterialIssueSummary_SSP] @OrderType={OrderType}");
				SummaryData = db.Read<GenericMaterialIssueSummary_VM>().ToList();
			}
			return PartialView(SummaryData);
			
		}
		[HttpGet]
		public IActionResult GenericMaterialIssueSummary_Vendorwise(string OrderType,int Engine_PartKey)
		{
			ViewBag.OrderType = OrderType;
			List<GenericMaterialIssueSummary_Vendorwise_VM> Summary_vendorwiseData = new List<GenericMaterialIssueSummary_Vendorwise_VM>();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"[dbo].[GenericMaterialIssueSummary_Vendorwise_SSP] @OrderType = {OrderType}, @Engine_Part_Dbkey = {Engine_PartKey}");
				Summary_vendorwiseData = db.Read<GenericMaterialIssueSummary_Vendorwise_VM>().ToList();
			}
			return PartialView(Summary_vendorwiseData);

		}


        [HttpGet]
        public IActionResult GenericMaterialIssueSummary_IssueHistory(string OrderType, int Engine_PartKey)
        {
            ViewBag.OrderType = OrderType;
            List<GenericMaterialIssueSummary_IssueHistory_VM> issueHistoryData = new List<GenericMaterialIssueSummary_IssueHistory_VM>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"[dbo].[GenericMaterialIssueSummary_IssueHistory_SSP] @OrderType = {OrderType}, @Engine_Part_Dbkey = {Engine_PartKey}");
                issueHistoryData = db.Read<GenericMaterialIssueSummary_IssueHistory_VM>().ToList();
            }
            return PartialView(issueHistoryData);
        }

        [HttpGet]
		public IActionResult GenericMaterialIssue_Split(string OrderType, int Engine_PartKey)
		{
			ViewBag.OrderType = OrderType;
			List<GenericMaterialIssueSummary_Vendorwise_VM> Summary_vendorwiseData = new List<GenericMaterialIssueSummary_Vendorwise_VM>();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"[dbo].[GenericMaterialIssueSummary_Vendorwise_SSP] @OrderType = {OrderType}, @Engine_Part_Dbkey = {Engine_PartKey}");
				Summary_vendorwiseData = db.Read<GenericMaterialIssueSummary_Vendorwise_VM>().ToList();
			}
			return PartialView(Summary_vendorwiseData);

		}

		public IActionResult GenericMaterialIssueDocument(int id, string Type)
		{
			using (_dbContext)
			{
				List<AttachmentVM> attachmentsVM = new();
				ViewBag.IssueDbKey = id;
				ViewBag.Type = Type;
				List<Models.Attachment> attachments = _dbContext.Attachments.Where(x => x.Source_table_key == id && x.Source_table == "Casting_MaterialIssue").ToList();
				if (attachments.Count != 0)
				{
					attachmentsVM = JsonConvert.DeserializeObject<List<AttachmentVM>>(JsonConvert.SerializeObject(attachments));
				}
				return View(attachmentsVM);
			}

		}

		[HttpPost]
		public  IActionResult SaveGenericMaterialIssueDocument([FromForm] UploadViewModel model)
		{
			try
			{
				List<AttachmentVM> attachments = JsonConvert.DeserializeObject<List<AttachmentVM>>(model.filesData);
				int counter = 0;
				foreach (var item in attachments)
				{
					var userguid = User.Identity.Name;
					string systemfilename = string.Empty;
					string filename = string.Empty;
					string SavePath = string.Empty;
					item.uploadeddocument = model.files[counter];
					item.AttachmentGUID = Guid.NewGuid().ToString();
					if (item.uploadeddocument != null)
					{
						Models.Attachment att = new();
						filename = item.uploadeddocument.FileName;
						systemfilename = item.AttachmentGUID + Path.GetExtension(item.uploadeddocument.FileName);
						SavePath = GetGenericMateriaIssueDocDestinationFolder() + systemfilename;
						using (var stream = new FileStream(SavePath, FileMode.Create))
						{
							item.uploadeddocument.CopyTo(stream);
						}
						att.Attachment_FileName = systemfilename;
						att.Orginal_File_Name = filename;
						att.Attachment_location = @"/Attachments/Casting_MaterialIssue_Docs/";
						att.Attachment_type = "Casting_Material_Issue_Doc";
						att.Source_table_key = item.Source_table_key;
						att.Source_table = item.Source_table;
						att.Revision = item.Revision;
						att.File_Revision = item.File_Revision;
						att.AttachmentGUID = item.AttachmentGUID;
						att.File_DVD_Num = item.File_DVD_Num;
						att.Updated_on = DateTime.Now;
						att.Updated_by = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
						_dbContext.Attachments.Add(att);
						_dbContext.SaveChanges();
					}
					counter++;
				}
				return Json(new { success = true, msg = "Saved Successfully" });
			}
			catch (Exception ex) 
			{
				ErrorHandler.LogException(ex); 
				return Json(new { success = false, msg = ex.Message });
			}

		}

		public IActionResult ReceiptComments(int CastingDbkey,int CastingReceiptsItemSplitKey)
		{
            string UserGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
			ViewBag.userGuid = UserGuid;
            ReceiptCommentVM receiptCommentVM = new ReceiptCommentVM();
            
            using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"[dbo].[Receipt_Comment_UserDetails] @OrderId = {CastingDbkey} ,@CastingReceiptsItemSplitKey= {CastingReceiptsItemSplitKey} ");
                receiptCommentVM.castingReceiptSplitVM = db.Read<CastingReceiptSplitVM>().FirstOrDefault();
                receiptCommentVM.receiptCommentUserDetailsVMs = db.Read<ReceiptCommentUserDetailsVM>().ToList();
               
            }

			return PartialView(receiptCommentVM);
		}

		[HttpPost]
		public IActionResult AutoSaveCastingRemarks(CastingReceiptsComment commentDetails)
		{
			try
			{
                string UserGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
               
				if (commentDetails != null)
				{
					int MappingKey = _dbContext.Casting_DepartmentOrders.Where(x => x.DepartmentID == commentDetails.DepartmentID)
																		.Select(x => x.Id).FirstOrDefault();
					commentDetails.UserGuid = UserGuid;
					commentDetails.UpdatedOn = DateTime.Now;
					commentDetails.MappingKey = MappingKey;
					if(commentDetails.CastingReceiptsCommentsKey == 0)
					{
						_dbContext.Add(commentDetails);
					}
					else
					{
						_dbContext.Entry(commentDetails).State = EntityState.Modified;
					}
					_dbContext.SaveChanges();

                    return Json(new { success = true });
				}
				else{
                    return Json(new { success = false });
                }
               
            }
			catch (Exception ex)
			{
                ErrorHandler.LogException(ex);
                return Json(new { success = false });
               
			}
			
		}

		public IActionResult RemarkDepartmentOrder()
		{
			List<DepartmentOrderVM> departmentOrderVMs = new List<DepartmentOrderVM>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"[dbo].[CastingDeaprtmentOrder_SSP]");
                departmentOrderVMs = db.Read<DepartmentOrderVM>().ToList();
            }
            return PartialView(departmentOrderVMs);
		}

		[HttpPost]
		public IActionResult SaveDepartmentOrder([FromBody] List<Casting_DepartmentOrder> deptOrder)
		{
			try
			{
				foreach (var item in deptOrder)
				{
					if (item.Id == 0)
					{
						_dbContext.Add(item);
					}
					else
					{
						_dbContext.Entry(item).State = EntityState.Modified;
					}
				}
				_dbContext.SaveChanges();
				return Json(new { success = true });
			}
			catch (Exception ex)
			{
				ErrorHandler.LogException(ex);
				return Json(new { success = false });
			}
			
		}

	}

}

