using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPCRS.ViewModels
{
    public class PrintMaterialIssueVM
    {
        public List<MaterialIssue> materialIssues { get; set; }
    }
}

public class MaterialIssue
{
    public int Issue_Dbkey { get; set; }
    public string Ref_Number { get; set; }
    public string Form_Number { get; set; }
    public string Engine_Name { get; set; }
    public string Demand_No { get; set; }
    public string Order_Ref_No { get; set; }
    public DateTime Order_Ref_Date { get; set; }
    public string PMO_Ref_No { get; set; }
    public DateTime PMO_Ref_Date { get; set; }
    public string Drawing_no { get; set; }
    public string Material_name { get; set; }
    public string Description { get; set; }
    public double Qty { get; set; }
    public string Size { get; set; }
    public string Denom { get; set; }
    public double Qty_Issue { get; set; }
    public string Heat_No { get; set; }
    public double Weight_Kg { get; set; }
    public double Amount { get; set; }
    public string DemandingOfficer { get; set; }
    public string DODesignation { get; set; }
    public string DOSection { get; set; }
    public string TechOfficer { get; set; }
    public string TODesignation { get; set; }
    public string TOSection { get; set; }
    public string PD { get; set; }
    public string PDDesignation { get; set; }
    public string PDSection { get; set; }
    public string Vendor_Name { get; set; }
    public string Vendor_Adress { get; set; }
    public string Vendor_Contact { get; set; }
    public string Vendor_State { get; set; }
    public string Vendor_City { get; set; }
    public string Vendor_Pincode { get; set; }
    public double Total_Cost { get; set; }
    public double Total_Qty { get; set; }
    public string Returnable { get; set; }
    public string Issue_Purpose { get; set; }

    public string outer_dia { get; set; }
    public string thickness { get; set; }

    public Nullable<int> Book_Serial_No { get; set; }
    public Nullable<int> Volume_No { get; set; }

}
