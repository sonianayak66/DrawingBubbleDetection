using MPCRS.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using MPCRS.Models;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;

namespace MPCRS.ViewModels
{
    public class MaterialIssueVM
    {
        public int Issue_Dbkey { get; set; }
        [DisplayName("Reference No.")]
       
        public string? Ref_Number { get; set; }
        [DisplayName("Form Number")]

        public string? Form_Number { get; set; }
        [Required]
        [DisplayName("Engine")]
        public string? Engine_Name { get; set; }

        [Required(ErrorMessage = "Required")]
        [DisplayName("Demand No")]        
        public string? Demand_No { get; set; }

       
        [DisplayName("Created on")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? created_on { get; set; }

        public int? DemandDbKey { get; set; }
        [Required]
        [DisplayName("Order Ref No")]
        public string? Order_Ref_No { get; set; }

        [Required]
        [DisplayName("Order Ref Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Order_Ref_Date { get; set; }
        [DisplayName("PMO Ref No")]
        public string? PMO_Ref_No { get; set; }
        [DisplayName("PMO Ref Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? PMO_Ref_Date { get; set; }
        [DisplayName("Demanding Officer")]
        public int? Demanding_Officer { get; set; }
        [DisplayName("Tech Officer")]
        public int? Tech_Officer { get; set; }
        [DisplayName("Project Director")]
        public int? Project_Director { get; set; }
        [Required]
        [DisplayName("Vendor")]
        public int? Vendor { get; set; }
        [DisplayName("MR No")]
        public int? MR_No { get; set; }
        [DisplayName("Book Serial No")]
        public int? Book_Serial_No { get; set; }
        [DisplayName("Volume No")]
        public int? Volume_No { get; set; }
        [DisplayName("Total Qty")]
        public double? Total_Qty { get; set; }
        [DisplayName("Total Cost")]
        public double? Total_Cost { get; set; }
        public string? Returnable { get; set; }

        [Required(ErrorMessage = "Required")]
        [DisplayName("Issue Purpose")]
        public int? Issue_Purpose { get; set; }
      
        [DisplayName("Job Card")]
        public string? Job_Card { get; set; }
        public string? JobCardFileLocation { get; set; }
        public string? JobCardFileName { get; set; }
        public string? Raw_material_Name { get; set; }
       
        public bool? IsActive { get; set; }

        public string? Attachment_Db_Key { get; set; }

        public List<Material_Issue_Items_VM> material_Issue_Items { get; set; }

        public List<ForgingReceipts> forgingReceipts { get; set; }

        public List<Attachment> attachments { get; set; }

        public string? Vendor_Name { get; set; }
        public string? Issue_Purpose_text { get; set; }
        [DisplayName("Engine")]
        public string? EngineLevel { get; set; }
    }
    public class Material_Issue_Items_VM
    {
        public int? Issue_Item_Dbkey { get; set; }
        public int? Issue_Dbkey { get; set; }
        public int? Raw_material_Dbkey { get; set; }
        public string? Drawing_no { get; set; }
        public string? Material_name { get; set; }
        public string? Description { get; set; }
        public int? Engine_Part_Dbkey { get; set; }
        public double? Qty { get; set; }
        public string? Size { get; set; }
        public string? Denom { get; set; }
        public double? Qty_Issue { get; set; }
        public string? Heat_No { get; set; }
        public string? EngineLevel { get; set; } // Added
        public double? Weight_Kg { get; set; }
        public double? Amount { get; set; }
        public DateTime? Updated_On { get; set; }
        public int? Updated_By { get; set; }
        public string? outer_dia { get; set; }
        public string? thickness { get; set; }
        public string? height { get; set; }
        public string? JobCardNumber { get; set; }
        public string? JCFileName { get; set; }
        public string? JCFileLocation { get; set; }
        public bool? IsActive { get; set; }
        public string? Raw_material_Name { get; set; }
        public SelectList Thickness_list { get; set; }
        public SelectList Outer_Dia_mm_list { get; set; }
        public SelectList height_list { get; set; }
        public int?[] PartNumberKey { get; set; }
        public string? SerialNo { get; set; }
        public SelectList PartsList { get; set; }
       public SelectList RawMaetrialList { get; set; }

        // NEW PROPERTIES FOR VENDOR
        public int? Vendor_Dbkey { get; set; }
        public string? Vendor_Name { get; set; }
        public SelectList VendorsList { get; set; }
        public string? PartKeys { get; set; }
        public double? PartQty_EngineWise { get; set; }

    }

    public class ForgingReceipts
    {
        public int? forging_recp_dbkey { get; set; }
        public int? forging_item_dbkey { get; set; }      
        public string? Receipt_Number { get; set; }
        public DateTime Receipt_Date { get; set; }
        public double? Total_Qty { get; set; }
        public string? GTRE_Drawing_No { get; set; }
        public string? HAL_Drawing_No { get; set; }
        public double? Receiving_Inventory { get; set; }
        public int? Issue_Item_Dbkey { get; set; }      
    }


    public class MaterialIssueSummaryVM
    {
        public int? Issue_Dbkey	{ get; set; }
        public int? Issue_Item_Dbkey	{ get; set; }
        public DateTime? Order_Ref_Date  { get; set; }
        public string? Job_Card    { get; set; }
        public string? JobCardFileLocation { get; set; }
        public string? JCFileName  { get; set; }
        public string? Order_Ref_No    { get; set; }
        public int? Raw_material_Dbkey  { get; set; }
        public string? Material_name   { get; set; }
        public double? outer_dia   { get; set; }
        public double? thickness   { get; set; }
        public double? Qty { get; set; }
        public string? Drawing_no  { get; set; }
        public double? Mlength { get; set; }
        public double? Mreadth { get; set; }
        public double? Weight  { get; set; }
        public string? Vendor_Name { get; set; }
        public string? PartNumber  { get; set; }
        public double? runningBalnce   { get; set; } 

        public string? Size { get; set; }
    }

    public class ForgingSplitsVM
    {
        public int forging_item_split_dbkey { get; set; }
        public int forging_item_dbkey { get; set; }
        public int forging_recp_dbkey { get; set; }
        public string part_name { get; set; }
        public string GTRE_Drawing_No { get; set; }
        public string Batch_Number { get; set; }
        public string Heat_Number { get; set; }
        public string Sl_No_Forging { get; set; }
        public string Receipt_Number { get; set; }
        public System.DateTime Receipt_Date { get; set; }
        public string MMG_File_No { get; set; }
        public string Attachment_Db_Key { get; set; }
        public int[]? Attachment_Db_Key_Data { get; set; }
        public SelectList DocumentReference { get; set; }
        public double Receiving_Inventory { get; set; }
    }

    public class ForgingReceiptDocumentsVM
    {
        public int? Attachment_Db_Key { get; set; }
        public string? Source_table { get; set; }
        public int? Source_table_key { get; set; }
        public string? Attachment_location { get; set; }
        public string? Attachment_FileName { get; set; }
        public string? Attachment_type { get; set; }
        public string? Orginal_File_Name { get; set; }
        public string? File_Revision { get; set; }
        public int? Updated_by { get; set; }
        public DateTime? Updated_on { get; set; }
        public int? Approved_status { get; set; }
        public Guid? AttachmentGUID { get; set; }
        public string? DocumentType { get; set; }
        public string? Demand_No { get; set; }
        public string? MMG_File_No { get; set; }
        public string? Receipt_Number { get; set; }
        public DateTime? Receipt_Date { get; set; }
        public string? Drawing_no { get; set; }
        public string? GTRE_Drawing_No { get; set; }
        public string? HAL_Drawing_No { get; set; }
        public string? Receiving_Inventory { get; set; }
        public int? Issue_Dbkey { get; set; }
		public string? Attachments { get; set; }

		
	}



}

    

