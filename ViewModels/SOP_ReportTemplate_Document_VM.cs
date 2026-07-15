namespace MPCRS.ViewModels
{
    public class SOP_ReportTemplate_Document_VM
    {
        public int Id { get; set; }

        public string? BuildGuid { get; set; }

        public string? UserGuid { get; set; }

        public IFormFile? File { get; set; }

        public string? FileLocation { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }
}
