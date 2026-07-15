using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Web.Mvc;

namespace MPCRS.ViewModels
{
    public class IONNoteDto
    {
        public int? IONId { get; set; }
        public string? IONGUID { get; set; }
        public string? IONNumber { get; set; }
        public string? GroupGUID { get; set; }
        // public string Office { get; set; }
        public string IONDate { get; set; }
        // public int DestinationId { get; set; }
        public string Subject { get; set; }
        public string CommunicationReference { get; set; }

        [BindNever]
        [AllowHtml] // This allows HTML content
        public string IONBody { get; set; }

        public string ToAddress { get; set; }
        public string? CopyTo { get; set; }
        public int PreparedBy { get; set; }
        public string? PreparedByDesignation { get; set; }
        public int? SentThrough { get; set; }
        public string Status { get; set; }

        // Recipients (To / CopyTo) - stored in ION_NoteRecipients table
        public List<string>? ToRecipients { get; set; }
        public List<string>? CopyToRecipients { get; set; }
    }
}
