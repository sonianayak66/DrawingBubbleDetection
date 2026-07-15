using MPCRS.Models;

namespace MPCRS.ViewModels
{
    public class SOPReportVM
    {
        // List of templates section header 
        public EngineBuildsVM engineBuildsVM { get; set; }
        public List<SOP_ReportTemplate> sOPReportTemplate { get; set; }
        public List<SOP_BuildReportSection_Repo> SOP_BuildReportSections { get; set; }
        public List<EngineBuildComponents> engineBuildComponents { get; set; }
      

    }



    public partial class SOP_BuildReportSection_Repo
    {
        public int Id { get; set; }

        public string SopReportSectionGUID { get; set; } = null!;

        public string BuildGuid { get; set; } = null!;

        public string? ReportTemplateSectionGUID { get; set; }

        public string? Body { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsReviewed { get; set; }

        public bool IsActive { get; set; }

        public string? Updated_By { get; set; }

        public DateTime? Updated_On { get; set; }

        public string? ExtractedBody { get; set; }

        public int? AttachmentKey { get; set; }

        public string? Attachment_FileName { get; set; }
        public string? Attachment_location { get; set; }
    }

    public class EngineBuildComponents
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
        public int SOP_ComponentId { get; set; }
        public int SOP_BuildDbkey { get; set; }
        public string SOP_BuildPartRevision { get; set; }
        public string SOP_BuildPartQtyPerEngine { get; set; }
        public string SOP_BuildPartDescription { get; set; }
        public string SOP_BuildPartJobCard { get; set; }
        public string SOP_BuildPartSerialNumber { get; set; }
        public string SOP_BuildPartRemarks { get; set; }
        public bool? IsReplaced { get; set; } = false;
        public bool? IsUpdated { get; set; } = false;
        public bool? IsRemoved { get; set; } = false;
        public bool? IsNewlyAdded { get; set; } = false;
        public string SOPPartStatus { get; set; }

        // BATL Sync Status - indicates if part has data synced from BATL
        public int HasBATLData { get; set; }

        // Execution Responsibility from Master_General
        public string ExecutionResponsibility { get; set; }
    }

}
