using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPCRS.Models;
using MPCRS.ViewModels;

namespace MPCRS.Controllers
{

    public class DemandVerificationController : Controller
    {
        [HttpGet]
        public ActionResult Verify()
        {
            using (DESI_STFE_PRODContext DbContext = new DESI_STFE_PRODContext())
            {
                List<Demand_Verification> demand_Verifications = DbContext.Demand_Verifications.ToList();
                return View(demand_Verifications);
            }    
        }

        [HttpGet]
        public ActionResult AddDemandDetails(int VerifyId)
        {
            using(DESI_STFE_PRODContext Db = new DESI_STFE_PRODContext())
            {
                Demand_Verification verification = Db.Demand_Verifications.Where(x => x.Verification_Id == VerifyId).FirstOrDefault();
				
				DemandVerificationVM vm = new DemandVerificationVM();
                
				if (verification != null) 
                {
					vm.Verification_Id = verification.Verification_Id;
					vm.Demand_No = verification.Demand_No;
					vm.Demand_Desc = verification.Demand_Desc;
					vm.Demanding_Officer = verification.Demanding_Officer;
					vm.Project =verification.Project;
					vm.Items = verification.Items;
					vm.Receipt = verification.Receipt;
					vm.Receipt_Docs = verification.Receipt_Docs;
					vm.Remarks = verification.Remarks;
					vm.Verified = verification.Verified;
					vm.Verified_By = verification.Verified_By;
					return PartialView(vm);
                }
                else
                {
                    vm = new DemandVerificationVM();
					return PartialView(vm);
				}
			
			}
           
        }

        [HttpPost]
        public ActionResult AddDemandDetails([FromBody]DemandVerificationVM demand_Verification)
        {
           using(DESI_STFE_PRODContext Db = new DESI_STFE_PRODContext())
            {

                Demand_Verification verification = new Demand_Verification();
               
                if(demand_Verification.Verification_Id == 0)
				{
					
					verification.Verification_Id = demand_Verification.Verification_Id;
					verification.Demand_No = demand_Verification.Demand_No;
					verification.Demand_Desc = demand_Verification.Demand_Desc;
					verification.Demanding_Officer = demand_Verification.Demanding_Officer;
					verification.Project = demand_Verification.Project;
					verification.Items = demand_Verification.Items;
					verification.Receipt = demand_Verification.Receipt;
					verification.Receipt_Docs = demand_Verification.Receipt_Docs;
					verification.Remarks = demand_Verification.Remarks;
					verification.Verified = demand_Verification.Verified;
					verification.Verified_By = demand_Verification.Verified_By;
					Db.Add(verification);
					
				}
				else
                {
					verification.Verification_Id = demand_Verification.Verification_Id;
					verification.Demand_No = demand_Verification.Demand_No;
					verification.Demand_Desc = demand_Verification.Demand_Desc;
					verification.Demanding_Officer = demand_Verification.Demanding_Officer;
					verification.Project = demand_Verification.Project;
					verification.Items = demand_Verification.Items;
					verification.Receipt = demand_Verification.Receipt;
					verification.Receipt_Docs = demand_Verification.Receipt_Docs;
					verification.Remarks = demand_Verification.Remarks;
					verification.Verified = demand_Verification.Verified;
					verification.Verified_By = demand_Verification.Verified_By;
					Db.Entry(verification).State = EntityState.Modified;
				}
				Db.SaveChanges();
			}
           return Json(new {success=true,id=demand_Verification.Verification_Id});
        }

		public ActionResult DeleteVerificationRow(int Verifyid)
		{
			if(Verifyid != 0)
			{
				using(DESI_STFE_PRODContext db =  new DESI_STFE_PRODContext())
				{
					Demand_Verification demand_Verification = db.Demand_Verifications.Where(x => x.Verification_Id == Verifyid).FirstOrDefault();
					db.Remove(demand_Verification);
					db.SaveChanges();
				}
				return Json(new { success = true, });
			}
			return Json(new { success = false });
		}

	}
}
