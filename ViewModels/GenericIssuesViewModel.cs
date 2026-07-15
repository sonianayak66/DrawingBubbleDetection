
using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
	public class GenericIssuesViewModel
	{
		// Common fields for both create and edit
		public int? CastingDbkey { get; set; }
		public int? QtySplitKey { get; set; }
		public string? castingGUID { get; set; }
		public string? OrderType { get; set; }
		public string? MMGOrderNumber { get; set; }
		public string? OrderNumber { get; set; }
		public string? ReceiptNumber { get; set; }
		public string? PartNumber { get; set; }
		public string? PartDescription { get; set; }
		public string? BatchNumber { get; set; }
		public string? HeatNumber { get; set; }
		public string? SplitQty { get; set; }
		public string? SerialNos { get; set; }
		public string? Vendor_Name { get; set; }
		public string? GlobalIssuedSerialNos { get; set; }
		[Required]
		public int? VendorDbKey { get; set; }
		public int? IssueQty { get; set; }
		[Required]
		public string? ReferenceNo { get; set; }
		public string? IssueSlNos { get; set; }
		public string? currentIssuedSlno { get; set; }
		[Required]
		public DateTime? IssueDate { get; set; }
		public List<string> IssuedSerialNosList { get; set; } = new List<string>();

		// Fields specific to edit
		public int? IssueDbKey { get; set; }
		public int? IssueItemKey { get; set; }

		public string? ForEngine { get; set; }
        public string? JobCardNumber { get; set; }

        public string? JCFileName { get; set; }

        public string? JCFileLocation { get; set; }
		// Additional fields needed for editing
		//public string? ReferenceNo { get; set; }
		//public string? IssueSlNos { get; set; }
		//public DateTime? IssueDate { get; set; }

		public int? QuantityPerEngine { get; set; }
		public int? Engine_Part_Dbkey { get; set; }
	}
}
