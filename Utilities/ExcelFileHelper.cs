using ExcelDataReader;
using System.Data;

namespace MPCRS.Utilities
{
    public class ExcelFileHelper
    {
        public static DataTable SaveAsDatatable(string excelFilePath)
        {
            DataTable dt = new DataTable();

            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IExcelDataReader reader = null;
                if (excelFilePath.EndsWith(".xls"))
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else if (excelFilePath.EndsWith(".xlsx"))
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }

                if (reader == null)
                    return dt;

                var ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true,
                        FilterColumn = (rowReader, columnIndex) => {
                            return rowReader[columnIndex] != null;
                        }
                    }
                });

                dt = ds.Tables[0];

                return dt;
            }
        }
    }
}
