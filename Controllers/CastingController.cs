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


namespace MPCRS.Controllers
{
    public class CastingController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;

        public CastingController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }


        public IActionResult CastingList()
        {
            List<CastingDetailViewModel> castingViewModel = new List<CastingDetailViewModel>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.CastingLists_SSP");
                castingViewModel = db.Read<CastingDetailViewModel>().ToList();
            }
            return View(castingViewModel);
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

        public IActionResult CastingDetail(string id)
        {

            CastingViewModel castingViewModel = new CastingViewModel();
            ViewBag.customAccess = "false";
            ViewBag.Id = id;

            if (id == "NEW")
            {
                CastingDetailViewModel castingDetailViewModel_ = new CastingDetailViewModel();
                castingDetailViewModel_.CastingDbkey = 0;
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


        [AllowAnonymous]
        public IActionResult CustomAccess(string accessToken = "")
        {
            using (_dbContext)
            {
                ViewBag.customAccess = "true";
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
                        return View("CastingDetail", castingViewModel);
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
                var db = connection.QueryMultiple($"dbo.CastingDetail_SSP @CastingId={CastingId}");
                castingViewModel.castingDetailViewModel = db.Read<CastingDetailViewModel>().FirstOrDefault();
                castingViewModel.CastingItemViewModel = db.Read<CastingItemViewModel>().ToList();
                //castingViewModel.vendors = db.Read<Vendor>().ToList();
                castingViewModel.castingReceiptViewModel = db.Read<CastingReceiptViewModel>().ToList();
                castingViewModel.receiptSplitVMs = db.Read<CastingReceiptSplitVM>().ToList();
                castingViewModel.receiptBatchSummary = db.Read<CastingReceiptBatchSummary>().ToList();
                return castingViewModel;
            }
        }

        [HttpGet]
        public IActionResult CastingOrder(int CastingId)
        {
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
        public IActionResult CastingOrder(CastingDetailViewModel castingDetailViewModel)
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


        [HttpGet]
        public IActionResult CastingItems(int CastingId)
        {
            CastingViewModel castingViewModel = GetCastingDetail(CastingId);
            return PartialView(castingViewModel);
        }


        [HttpPost]
        public IActionResult CastingItems([FromBody] IEnumerable<CastingItemViewModel> castingItemViewModels)
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
                    castingItem.OrderQty = item.OrderQty;
                    castingItem.UpdatedBy = UserId;
                    castingItem.UpdatedOn = DateTime.Now;

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


        [HttpGet]
        public IActionResult CastingReceiptItems(int CastingId, string ReceiptGuid = "")
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

        [HttpGet]
        public IActionResult DetailedCastingData(int partKey = 0, bool tableOnly = false)
        {
            List<DetailedCastingDataVM> castingData = new List<DetailedCastingDataVM>();
            ViewBag.tableOnly = tableOnly;
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.GetCastingDetailsSplitLevel @partKey={partKey}");
                castingData = db.Read<DetailedCastingDataVM>().ToList();
            }

            return View(castingData);
        }

        [HttpGet]
        public IActionResult CastingReceiptLevelSummary(int partKey = 0)
        {
            List<CastingReceiptLevelSummaryVM> castingData = new List<CastingReceiptLevelSummaryVM>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.CastingReceiptLevelSummary @partKey={partKey}");
                castingData = db.Read<CastingReceiptLevelSummaryVM>().ToList();
            }

            return View(castingData);
        }



        [HttpGet]
        public IActionResult CastingOrderSummary()
        {
            List<CastingOrderSummaryVM> CastingSummary = new List<CastingOrderSummaryVM>();

            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.CastingSummaryData");
                CastingSummary = db.Read<CastingOrderSummaryVM>().ToList();
            }

            return View(CastingSummary);
        }


        [HttpGet]
        public IActionResult CastingPartOrderData(int partkey)
        {
            List<CastingOrderSummaryVM> CastingSummary = new List<CastingOrderSummaryVM>();

            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.GetCastingPartOrderData @partKey = {partkey}");
                CastingSummary = db.Read<CastingOrderSummaryVM>().ToList();
            }

            return View(CastingSummary);
        }


        [HttpPost]
        //public IActionResult CastingReceiptItems([FromBody] IEnumerable<CastingReceiptViewModel> castingReceiptViewModels)
        //{
            //    using (_dbContext)
            //    {
            //        int UserId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
            //        Guid newGuid = Guid.NewGuid();

            //        string ReceiptGuid = castingReceiptViewModels.FirstOrDefault().ReceiptGuid;
            //        if (string.IsNullOrEmpty(ReceiptGuid))
            //        {
            //            ReceiptGuid = newGuid.ToString();
            //        }

            //        foreach (var item in castingReceiptViewModels)
            //        {
            //            CastingReceipt castingReceipt = new CastingReceipt();
            //            if (item.CastingReceiptDbkey != 0)
            //            {
            //                castingReceipt = _dbContext.CastingReceipts.Where(x => x.CastingReceiptDbkey == item.CastingReceiptDbkey).FirstOrDefault();
            //            }

            //            castingReceipt.CastingReceiptDbkey = item.CastingReceiptDbkey;
            //            castingReceipt.ReceiptGuid = ReceiptGuid;
            //            castingReceipt.CastingDbkey = item.CastingDbkey;
            //            castingReceipt.CastingItemKey = item.CastingItemKey;
            //            castingReceipt.ReceiptNumber = item.ReceiptNumber;
            //            castingReceipt.ReceiptDate = item.ReceiptDate;
            //            castingReceipt.Qty = item.Qty;

            //            if (item.CastingReceiptDbkey == 0)
            //            {
            //                castingReceipt.Isdeleted = false;
            //                castingReceipt.CreatedBy = UserId;
            //                castingReceipt.CreatedOn = DateTime.Now;
            //                _dbContext.Add(castingReceipt);
            //            }
            //            else
            //            {
            //                castingReceipt.UpdatedBy = UserId;
            //                castingReceipt.UpdatedOn = DateTime.Now;
            //                _dbContext.Entry(castingReceipt).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            //            }
            //        }
            //        _dbContext.SaveChanges();
            //    }
            //   return Json(new { success = true, msg = "Submitted successfully" });
            //}


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
            //List<SplitDocumentChecklistVM> documentChecklist = new List<SplitDocumentChecklistVM>();
            //using (var connection = mPDapperContext.CreateConnection())
            //{
            //	var db = connection.QueryMultiple($"dbo.[GetCastingSplitDocInfo] @partDbKey={partDbKey}, @receiptSplitDbKey={CastingSplitKey}");
            //	documentChecklist = db.Read<SplitDocumentChecklistVM>().ToList();
            //} 
            return PartialView();
        }


        //[HttpGet]
        //      public IActionResult CastingReceiptItemSplitModel(int CastingDbkey, int CastingSplitKey = -1)
        //      {

        //          List<CastingReceiptSplitVM> castingReceiptSplitVMs = new();
        //          DataTable dt = MPGlobals.GetDataForDatalist($"dbo.[GetCasting_Recipt_Split] @CastingId={CastingDbkey},@CastingReceiptDbkey= {CastingReceiptDbkey},@CastingSplitKey ={CastingSplitKey}");
        //          castingReceiptSplitVMs = MPGlobals.ConvertDataTable<CastingReceiptSplitVM>(dt);

        //          ViewBag.CastingReceiptDBkey = 0;
        //	List<Models.Attachment> attachments = new List<Models.Attachment>();

        //          if(castingReceiptSplitVMs.Count > 0 && castingReceiptSplitVMs.FirstOrDefault().Attachments != null && CastingSplitKey != -1)
        //          {
        //		attachments = _dbContext.Attachments.Where(x => x.Source_table == "Casting_Forging_File" && x.Source_table_key == CastingReceiptDbkey).ToList();
        //		var attachmentKeys = castingReceiptSplitVMs.FirstOrDefault().Attachments.Split(",").ToList();
        //		List<int> intList = attachmentKeys.Select(s => int.Parse(s)).ToList();
        //		attachments = attachments.Where(x => intList.Contains(x.Attachment_Db_Key)).ToList();
        //	}
        //          if(CastingSplitKey == -1) 
        //          {
        //              CastingReceiptSplitVM castingReceiptSplitViewModel = castingReceiptSplitVMs.Count > 0? castingReceiptSplitVMs.FirstOrDefault() : new CastingReceiptSplitVM();
        //              castingReceiptSplitViewModel.CastingSplitId = 0;
        //              castingReceiptSplitViewModel.SerialNumber = "";
        //              castingReceiptSplitViewModel.BatchNumber = "";
        //              castingReceiptSplitViewModel.HeatNumber = "";
        //              castingReceiptSplitViewModel.Attachments = "";
        //              castingReceiptSplitVMs = new List<CastingReceiptSplitVM>();
        //              castingReceiptSplitVMs.Add(castingReceiptSplitViewModel);
        //          }
        //          var myTuple = new Tuple<List<CastingReceiptSplitVM>, List<Models.Attachment>>(castingReceiptSplitVMs, attachments);
        //          return View(myTuple);

        //      }


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
        public IActionResult CastingQtySplit(int CastingSplitKey =0)
        {
            List<CastingReceiptQtySplit> castingQtySplit = _dbContext.CastingReceiptQtySplits.Where(x=>x.ReceiptsItemSplitKey == CastingSplitKey).ToList();
            return PartialView(castingQtySplit);
        }


        [HttpGet]
        public IActionResult CastingReceiptItemSplitModel(int CastingDbkey, int CastingSplitKey = -1)
        {
            CastingReceiptDetailViewModel castingReceiptSplitVMs = new();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.[Casting_Receipts_SSP] @CastingId={CastingDbkey},@ReceiptDbkey={CastingSplitKey}");
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
                    // string SaveDirectory = string.Empty;
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
                CastingreceiptsItemSplit.OrderItemKey = castingReceiptSplit.OrderItemKey;
                CastingreceiptsItemSplit.ReceiptNumber = castingReceiptSplit.ReceiptNumber;
                CastingreceiptsItemSplit.ReceiptDate = castingReceiptSplit.ReceiptDate;
                CastingreceiptsItemSplit.Remarks = castingReceiptSplit.Remarks;   
                CastingreceiptsItemSplit.Attachments = AttachmentDbKey;
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

                foreach (var item in castingQtySplit)
                {
                    item.ReceiptsItemSplitKey = CastingreceiptsItemSplit.Id;
                    item.UpdatedOn = DateTime.Now;
                    item.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                  //  item.Remarks = castingQtySplit.FirstOrDefault().Remarks;


                    if (item.QtySplitKey != 0)
                    {
                        _dbContext.Entry(item).State = EntityState.Modified;
                    }
                    else if (item.QtySplitKey == 0)
                    {   
                        _dbContext.Add(item);
                    } 
                } 

                _dbContext.SaveChanges();
                return Json(new { success = true, msg = "Successfully Saved" });
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }

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


        public ActionResult DeleteCastingComponents(string key, string table)
        {
            int UserId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
            string cmdstr = $"exec [dbo].DeleteCastingDetail_USP @key='{key}',@table = '{table}',@userid={UserId}";
            MPGlobals.ExceSQLNonQuery(cmdstr);
            return Json(new { success = true, msg = "Removed successfully" });
        }


        [Authorize]
        [ClaimRequirement(UserPermissions.Casting_Delete)]
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

    }
}
