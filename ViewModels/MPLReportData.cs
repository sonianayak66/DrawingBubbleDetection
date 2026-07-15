namespace MPCRS.ViewModels
{
    public class MPLReportData
    {
        public int enginedbkey { get; set; }
        public int Engine_Part_Dbkey { get; set; }
        public int Parent_id { get; set; }
        public int Level { get; set; }
        public string Draw_part_no { get; set; }
        public string Description { get; set; }
        public string Material_name { get; set; }
        public string ParentName { get; set; }
        public string Type_Part_Name { get; set; }
        public string Reporting_Type { get; set; }
        public string IsParent { get; set; }
        public int Qty_per_Engine { get; set; }
        public string Revision { get; set; }
        public string ParentRevision { get; set; }
        public double ParentQuantity { get; set; }
        public string ParentDescription { get; set; }
        public string ParentMaterial_name { get; set; }
        public string ParentReporting_type { get; set; }
        public string ParentModuleRes { get; set; }
        public string PartModuleRes { get; set; }
        public string AssemblyReportingType { get; set; }
        public double ReportDisplayOrder { get; set; }
        public int Reporting_Parent { get; set; }
        public int Part_relation_dbkey { get; set; }
        public string Part_Remarks { get; set; }
        public string Hierarchy { get; set; }
        public string PartPath { get; set; }
        public string ReportGroup { get; set; }
        public DateTime reportdate { get; set; }
        public string engineDescription { get; set; }
        public DateTime EngineRevisionDate { get; set; }
        public string ReportGroupIdentifier { get; set; }
        public decimal AssemblyDisplayOrder { get; set; }
        public string MainAssesmblyHeading { get; set; }
        public int MaxLevel { get; set; }
        public int reducer { get; set; }
        public string Collaborators { get; set; }

        public string ManufacturingComments { get; set; }
        public double Per_VendorStatus { get; set; }
        public string MfgStatus { get; set; }

        public string MfgStatus_Vendor { get; set; }
        public string MfgStatus_Qty_Engine { get; set; }
        public string MfgStatus_RVQty { get; set; }
        public string MfgStatus_Remarks { get; set; }
    }

    public class BL_Engine_Approvers
    {
        public int BL_Approvers_Dbkey { get; set; }
        public string Section { get; set; }
        public string Person_Name { get; set; }
        public string RoleName { get; set; }
        public string Designation { get; set; }
        public string ModuleName { get; set; }
        public string Engine_Title { get; set; }
        public int BL_Engine_Dbkey { get; set; }

    }
}
