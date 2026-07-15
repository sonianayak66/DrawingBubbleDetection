namespace MPCRS.ViewModels
{
    public class InwardIONNoteDto
    {
        public int? InwardNoteId { get; set; }
        public string? InwardIONGUID { get; set; }

        // Physical arrival stamp date (when letter reached our office)
        public string ReceivedDate { get; set; }

        // Date printed on the letter itself (often earlier than ReceivedDate)
        public string? IONDate { get; set; }

        // Sender's own reference number as written on the letter
        public string? IONReferenceNumber { get; set; }

        // Sender info
        public string FromDepartment { get; set; }
        public string? FromPersonNameWithDesignation { get; set; }

        // Letter content
        public string Subject { get; set; }

        // Newline-separated lists (mirrors ToAddress/CopyTo pattern in ION_Note)
        public string AddressedTo { get; set; }
        public string? CopyTo { get; set; }

        // Internal notes
        public string? Remarks { get; set; }

        // Whether an acknowledgment receipt was sent back to the sender
        public bool AcknowledgmentSent { get; set; }
    }

    public class InwardIONListRequestDto
    {
        public int? PageNumber { get; set; }
        public string? SearchText { get; set; }
        public string? FromDepartment { get; set; }
        public string? AddressedTo { get; set; }
        public string? DateFrom { get; set; }
        public string? DateTo { get; set; }
    }
}
