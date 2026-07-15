using Microsoft.AspNetCore.Mvc.Rendering;
using MPCRS.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
    public class CastingViewModel
    {
        public CastingDetailViewModel castingDetailViewModel { get; set; }
        public List<CastingItemViewModel> CastingItemViewModel { get; set; }
        public List<CastingReceiptViewModel> castingReceiptViewModel { get; set; }
        public List<Vendor> vendors { get; set; }
        public CastingReceiptSplitVM castingReceiptSplitVM { get; set; }
        public List<CastingReceiptSplitVM> receiptSplitVMs { get; set; }
        public List<CastingReceiptBatchSummary> receiptBatchSummary{ get; set; }
    }


    public class CastingReceiptDetailViewModel
    {
        public List<CastingItemViewModel> CastingItemViewModel { get; set; }
        public CastingReceiptSplitVM receiptSplitVM { get; set; }
        public List<Attachment> attachment { get; set; } 
        public List<CastingReceiptQtySplit> QtySplit { get; set; }
}

    public class CastingDetailViewModel
    {
        public int CastingDbkey { get; set; }
        [Required]
        [DisplayName("Order Number")]
        public string? DemandNumber { get; set; }
        public string? OrderType { get; set; }
        public string? castingGUID { get; set; }
        [Required]
        [DisplayName("Order Date")]
        public DateTime? OrderDate { get; set; }
        [DisplayName("Raw Material")]
        public int? RawMaterial { get; set; }
        [DisplayName("Part Number")]
        public string? PartNumber { get; set; }

        public int? EnginePartDbkey { get; set; }
        [DisplayName("Total Qty")]
        public double? TotalQty { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public string? Vendors { get; set; }

        public string? SerialNumber { get; set; }
        [Required]
        [DisplayName("MMG Order Numbers")]
        public string? MMGOrderNumber { get; set; }

        public string? Remarks { get; set; }
        public string? DemandDesc { get; set; }
        public string? OrderNumbers { get; set; }

        public SelectList VendorsApplicable { get; set; }


        public string? LinkGuid { get; set; }
        public string? VendorsNames { get; set; }
        [DisplayName("Vendor")]
        public List<int> VendorIds { get; set; }

        public string? OrderStatus { get; set; }
        public int? DemandingOfficer { get; set; }

        public string? DOName {  get; set; }
    }

    public class CastingItemViewModel
    {
        public int CastingItemKey { get; set; }

        public int? CastingDbkey { get; set; }

        public int? EnginePartDbkey { get; set; }

        public double? OrderQty { get; set; }
        public string? PartName { get; set; } 
        public string? GTREDrgNo { get; set; }

        public string? ItemDescription { get; set; }

        public int? Vendor { get; set; }
        public string? Vendor_Name { get; set; }
        public string? OrderNumber { get; set; }

        public double? SumReceivedQty { get; set; }
        public double? AcceptedQty { get; set; }
        public double? RejectedQty { get; set; }

        public int? RawMaterial { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? Raw_material_Name { get; set; }
        public int? TestSpecimen { get; set; }
    }

    public class CastingReceiptViewModel
    {
        public int CastingReceiptDbkey { get; set; }

        public string? ReceiptGuid { get; set; }

        public int? CastingDbkey { get; set; }

        public int? CastingItemKey { get; set; }
        [Required]
        [DisplayName("Receipt Number")]
        public string? ReceiptNumber { get; set; }
        [Required]
        [DisplayName("Receipt Date")]
        public DateTime? ReceiptDate { get; set; }

        public double? Qty { get; set; }

        public string? PartName { get; set; }
        public string? Vendor_Name { get; set; }
        
    }


    public class CastingReceiptBatchSummary
    {
        public int CastingDbkey { get; set; }
        public int OrderItemKey { get; set; } 
        public double? SplitQty { get; set; } 
        public string? ReceiptNumber { get; set; }
        public string? BatchNumber { get; set; }
        public string? StatusRemarks { get; set; }
        public string? Vendor_Name { get; set; }
        public string? SerialNos { get; set; }
        public string? PartName { get; set; } 
        public DateTime? ReceiptDate { get; set; } 

    }

    public class CastingReceiptSplitVM
    {
        public int? CastingReceiptDbkey { get; set; }
        public string? ReceiptGuid { get; set; }
        public int? CastingDbkey { get; set; }
        public int? OrderItemKey { get; set; }
        [Required]
        public string ReceiptNumber { get; set; }
        [Required]
        public DateTime ReceiptDate { get; set; }    
        public double? CastReceiptQty { get; set; }
        public string? PartName { get; set; }
        public string? Vendor_Name { get; set; }
        public int Id { get; set; }
        public int? EnginePartDbkey { get; set; }
        public string? SerialNumber { get; set; }
        public string? BatchNumber { get; set; }
        public string? VendorDrawingNo { get; set; }
        public string? HeatNumber { get; set; }
        [Required]
        public double? Qty { get; set; }
        public string? Attachments { get; set; }
        public string? Revision { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
        public double? QtyRejected { get; set; }
        public string? Remarks_QtyRejected { get; set; }
        public string? SerialNumber_Rejected { get; set; }

    }

     
   

    public class DetailedCastingDataVM
    {
        public string OrderNumber { get; set; }
        public int CastingDbkey { get; set; }
        public string VendorsNames { get; set; }
        public string RawMaterialName { get; set; } 
        public string HeatNumber { get; set; } 
        public string BatchNumber { get; set; } 
        public string remarks { get; set; }
        public DateTime? OrderDate { get; set; }
        public int? OrderQuantity { get; set; } 
        public int? QtyAccepted { get; set; } 
        public int? QtyRejected { get; set; } 
        public int? EnginePartDbkey { get; set; } 
        public int? SplitId { get; set; } 
        public string Draw_part_no { get; set; }
        public string Description { get; set; } 
        public DateTime? ReceiptDate { get; set; }
        public string OrderStatus { get; set; }
        public string ReceiptNumber { get; set; }
        public string ItemVendor { get; set; }
        public string receiptSplitStatus { get; set; }
        public string SerialNumber { get; set; }
        public string SerialNumber_Rejected { get; set; }
    }

	public class SplitDocumentChecklistVM
	{
        public int Attachment_Db_Key { get; set; }
        public string Attachment_Type { get; set; } 
		public string Attachment_location { get; set; }
		public string Attachment_FileName { get; set; } 
		public string FileReference { get; set; } 
	}
    
    public class CastingOrderSummaryVM
	{
		public int? Engine_Part_Dbkey { get; set; } 
		public string Draw_part_no { get; set; }
		public string Description { get; set; } 
		public string Vendor_Name { get; set; } 
		public int? AcceptedQty { get; set; } 
		public int? RejectedQty { get; set; } 
		public int? OrderQty { get; set; } 
		public int? TotalRecievedQty { get; set; }
        public string? Remarks { get; set; }
        public int? QuantityPerEngine { get; set; }
        public int? IssuedQty { get; set; }
        public int? IsuueQtyPerEngine { get; set; }
        public string? ForEngine { get; set; }

    }

    public class CastingReceiptLevelSummaryVM
    {
        public int? EnginePartDbkey { get; set; }
        public string ReceiptNumber { get; set; }
        public string Draw_part_no { get; set; }
        public string Description { get; set; }
        public string Vendor_Name { get; set; }
        public string SplitItemKeys { get; set; }
        public string OrderNumber { get; set; } 
        public DateTime? ReceiptDate { get; set; }
        public int? OrderItemKey { get; set; }
        public int? OrderQty { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? Remarks { get; set; }

    }


}
