using System;
using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
    public class Procurement_Demand_FinancialBulkEdit_VM
    {
        public int DemandDbKey { get; set; }
        public int? Project_Dbkey { get; set; }
        public int? DemandingOfficerKey { get; set; }

        [Display(Name = "MMG Number")]
        public string MMG_File_No { get; set; }

        [Display(Name = "Description")]
        public string Item_Description { get; set; }

        [Display(Name = "Actual Cost")]
        public decimal? ActualCost { get; set; }

        [Display(Name = "Running Balance")]
        public decimal? ProjectRunningBalance { get; set; }

        [Display(Name = "Payment Made Till Date")]
        public decimal? PaymentMadeTillDate { get; set; }

        [Display(Name = "Balance Order Value")]
        public decimal? BalanceOrderValue { get; set; }

        [Display(Name = "Current Status")]
        public string CurrentStatus { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Status Date")]
        public DateTime? StatusDate { get; set; }

        [Display(Name = "Updated On")]
        public DateTime? Updated_On { get; set; }

        [Display(Name = "Advance Paid")]
        public decimal? AdvancePaid { get; set; }
    }

    


    public class Procurement_Demand_FinancialBulkEditPage_VM
    {
        public int ProjectDbkey { get; set; }
        public int DemandingOfficerKey { get; set; }

        public List<Procurement_Demand_FinancialBulkEdit_VM> Items { get; set; } = new();
    }



}