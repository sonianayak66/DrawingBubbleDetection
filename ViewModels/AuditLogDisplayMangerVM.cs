using MPCRS.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
namespace MPCRS.ViewModels
{
    public class AuditLogDisplayMangerVM
    {
        public int DisplayManagerKey { get; set; }

        public string? SourceTable { get; set; }

        public string? ColumnName { get; set; }

        public string? Display_ColumnName { get; set; }

        public bool? DisplayData { get; set; }

        public string? DataType { get; set; }

        public string? Action { get; set; }

        public int? DisplayOrder { get; set; }

        public bool? Force_Display_Data { get; set; }
    }
}
