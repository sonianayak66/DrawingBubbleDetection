namespace MPCRS.ViewModels
{
    public class RMCDiagnosticVM
    {
        public string RMC_Number { get; set; }
        public string Status { get; set; }  // OK | RMC_NOT_IN_JSON | GUID_NOT_IN_MASTER | NO_PROCUREMENT_MATCH
        public string Raw_material_Name { get; set; }
        public string Heat_No { get; set; }
        public string Batch_No { get; set; }
    }

    public class RMCAttachmentVM
    {
        public int Attachment_Db_Key { get; set; }
        public string Orginal_File_Name { get; set; }
    }

    public class RMCDataPopupVM
    {
        public RMCDiagnosticVM Diagnostic { get; set; }
        public List<ProcurementReceiptItemSplitViewModel> Splits { get; set; } = new();
        public List<RMCAttachmentVM> Attachments { get; set; } = new();
    }
}