using System;
using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
    public class JobCardVM
    {
        public int JobCard_Dbkey { get; set; }

        [Display(Name = "Job Card No.")]
        public string JobCard_No { get; set; }

        [Display(Name = "Engine")]
        public string Engine { get; set; }

        [Display(Name = "Request No.")]
        public string Request_No { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime? JobCard_Date { get; set; }

        [Display(Name = "Type of Tech Development")]
        public string Tech_Development_Type { get; set; }

        [Display(Name = "Nomenclature")]
        public string Nomenclature { get; set; }

        [Display(Name = "Drawing No.")]
        public string Drawing_No { get; set; }

        [Display(Name = "Issue No.")]
        public string Issue_No { get; set; }

        [Display(Name = "Module")]
        public string Module { get; set; }

        [Display(Name = "Quantity")]
        public int? Quantity { get; set; }

        [Display(Name = "Details of Materials / Job Issued")]
        public string Material_Details { get; set; }

        [Display(Name = "Technology Description")]
        public string Technology_Description { get; set; }

        [Display(Name = "Scope Of Work")]
        public string Scope_Of_Work { get; set; }

        [Display(Name = "J/C Opened On")]
        [DataType(DataType.Date)]
        public DateTime? JC_Opened_On { get; set; }

        [Display(Name = "J/C Closed On")]
        [DataType(DataType.Date)]
        public DateTime? JC_Closed_On { get; set; }
    }
}