using Microsoft.AspNetCore.Http;
using MPCRS.Utilities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace MPCRS.ViewModels
{
    public class Upload_DocumentVM
    {
        public int DocumentID { get; set; }
        public int DemandDbKey { get; set; }
        [DisplayName("Document Type")]
        [Required(ErrorMessage = "Required")]
        public int Document_Type { get; set; }
        [DisplayName("Select Item")]
        public string Document_Name { get; set; }
        public string Remarks { get; set; }
        public string Master_Name { get; set; }
        public string Document_Location { get; set; }
        public DateTime? Updated_On { get; set; }
        public SelectList DocumentType { get; set; }
        public IFormFile DocumentFile { get; set; }
        //public HttpPostedFileBase DocumentFile { get; set; }
        public List<Upload_DocumentVM> UploadDocumentVM { get; set; }
        public Upload_DocumentVM()
        {
            DocumentType = Masters.GetMaster_Demand_DocumentType();
        }

    }
}
