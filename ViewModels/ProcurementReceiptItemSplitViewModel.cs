using Microsoft.AspNetCore.Mvc.Rendering;

namespace MPCRS.ViewModels
{
    public class ProcurementReceiptItemSplitViewModel
    {
        public int SplitId { get; set; }
        public int Receipt_dbkey { get; set; }
        public double? Measurement { get; set; }
        public string? Material_name { get; set; }
        public string? UOM { get; set; }
        public string? Material_Reference_No { get; set; }
        public string? Heat_No { get; set; }
        public string? Batch_No { get; set; }
        public string? Attachment_Db_Key { get; set; }
        public double? Measurement_breadth { get; set; }
        public double? Weight { get; set; }
        public string? Inner_Dia_mm { get; set; }
        public string? Outer_Dia_mm { get; set; }
        public string? Thickness { get; set; }
        public string? height { get; set; }
        public string? inputuom { get; set; }
        public int[]? Attachment_Db_Key_Data { get; set; }
        public string? Attachment_Db_Key_Data_String { get; set; }
        public string? Master_Name { get; set; }
        public string? Revision { get; set; }
        public string? Raw_material_Name { get; set; }
        public SelectList DocumentReference { get; set; }

    }

    public class ProcurementAdditionalInfo
    {
        public int parentKey { get; set; }
        public string recordGUID { get; set; }
        public string item_SerialNumber { get; set; }
        public string Item_Part { get; set; }
        public string documents { get; set; }
        public string refNos { get; set; }
        public string remarks { get; set; }
        public int updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }

    public class ProcurementAdditionalInfoTest
    {
        public int parentKey { get; set; }
        public string recordGUID { get; set; }
        public string item_SerialNumber { get; set; }
        public string Item_Part { get; set; }
        public int[] documents { get; set; }
        public string refNos { get; set; }
        public string remarks { get; set; }
        public int updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }

    public class MaterialIssueProcurementReceiptItemSplitVM
    {
        public int Issue_Item_Dbkey { get; set; }
        public int Issue_Dbkey { get; set; }
        public int Raw_material_Dbkey { get; set; }
        public string Material_name { get; set; }
        public string UOM { get; set; }
        public string Outer_Dia_mm { get; set; }
        public string Inner_Dia_mm { get; set; }
        public string Thickness { get; set; }
        public int Qty { get; set; }
        public string MMG_File_No { get; set; }
        public string Receipt_No { get; set; }
        public string Material_Reference_No { get; set; }
        public string Heat_No { get; set; }
        public string Batch_No { get; set; }
        public int split_issue_id { get; set; }
        public int SplitId { get; set; }

        public string Master_Name { get; set; }

        public Nullable<double> Measurement { get; set; }
        public Nullable<double> Measurement_breadth { get; set; }
        public Nullable<double> Weight { get; set; }
    }

    public class ProcurementReceiptSplitAttachmentVM
    {
        public List<ProcurementReceiptItemSplitViewModel> procurementReceiptItemSplitVM { get; set; }

        public List<AttachmentVM> AttachmentVM { get; set; }
    }

}
