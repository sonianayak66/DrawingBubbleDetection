using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using MPCRS.Models;
using System.Data;
using System.Linq;
using XAct.Library.Settings;
using static MPCRS.Utilities.Constants;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MPCRS.Utilities
{
    public class Masters
    {
        public static void RemoveCache(string cachekey)
        {
            try
            {
                DataCaching.removeCache(cachekey);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
        }
        public static DataTable GetMaster_General_Jresult(string type)
        {
            DataTable dataTable = MPGlobals.GetDataForDatalist(@"SELECT [Master_Dbkey]
                 ,[Master_Name]
             FROM [dbo].[Master_General] where[Master_Type] = '" + type + "'");
            return dataTable;

        }


        public static SelectList GetRawMaterial_ParameterList(int? itemDbKey, string v)
        {
            string cmdstr = Getcmdstrs(itemDbKey, v);
            DataTable dataTable = MPGlobals.GetDataForDatalist(cmdstr);
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                selectListItems.Add(new SelectListItem() { Text = dataTable.Rows[i]["Text"].ToString(), Value = dataTable.Rows[i]["Value"].ToString() });
            }
            return new SelectList(selectListItems, "Value", "Text");
        }


        public static SelectList GetRawMaterial_ParameterList(int? itemDbKey, string v, List<Master_Rawmaterial> master_Rawmaterials)
        {
            string MaterialName = master_Rawmaterials.Where(x => x.Raw_material_Dbkey == itemDbKey).Select(x => x.Material_name).FirstOrDefault();
            int Materialtype = master_Rawmaterials.Where(x => x.Raw_material_Dbkey == itemDbKey).Select(x => x.RM_Type).FirstOrDefault() ?? 0;
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
            if (v == "Thickness")
            {
                foreach (var item in master_Rawmaterials.Where(x => x.Material_name == MaterialName && x.RM_Type == Materialtype && x.Thick_mm != null))
                {
                    selectListItems.Add(new SelectListItem() { Text = item.Thick_mm.ToString(), Value = item.Thick_mm.ToString() });
                }
            }
            else if (v == "Outer_Dia")
            {
                foreach (var item in master_Rawmaterials.Where(x => x.Material_name == MaterialName && x.RM_Type == Materialtype && x.Dia_mm != null))
                {
                    selectListItems.Add(new SelectListItem() { Text = item.Dia_mm.ToString(), Value = item.Dia_mm.ToString() });
                }
            }
            return new SelectList(selectListItems, "Value", "Text");
        }


        public static DataTable GetRawmaterialParaJResult(int id, string type)
        {
            string cmdstr = Getcmdstrs(id, type);
            DataTable dataTable = MPGlobals.GetDataForDatalist(cmdstr);
            return dataTable;
        }

        private static string Getcmdstrs(int? id, string v)
        {
            string Cmdstr = @"  select '0' as Value ,'Select' as Text union all";
            string MaterialName = MPGlobals.GetOnedata($"SELECT [Material_name] FROM [dbo].[Master_Rawmaterials] where [Raw_material_Dbkey] ={id}");
            string Materialtype = MPGlobals.GetOnedata($"SELECT isnull([RM_Type],0) FROM [dbo].[Master_Rawmaterials] where [Raw_material_Dbkey] ={id}");
            if (v == "Thickness")
            {
                Cmdstr = Cmdstr + $"  SELECT [Thick_mm] as Value,[Thick_mm] as Text FROM [dbo].[Master_Rawmaterials] where [Material_name] = '{MaterialName}' and  RM_Type = {Materialtype} and [Thick_mm] is not null";
            }
            else if (v == "Outer_Dia")
            {
                Cmdstr = Cmdstr + $"  SELECT [Dia_mm] as Value,[Dia_mm]  as Text FROM [dbo].[Master_Rawmaterials] where [Material_name] = '{MaterialName}' and  RM_Type = {Materialtype} and  [Dia_mm] is not null";
            }
            return Cmdstr;
        }

        public static SelectList GetMasterpartTypes(int id)
        {
            int Hierarcy = 0;
            using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
            {
                if (id == 1)
                {
                    Hierarcy = 1;
                }
                else
                {
                    Hierarcy = int.Parse(MPGlobals.GetOnedata(@"SELECT [Hierarchy]  FROM [Master_Part_Types] where Type_Dbkey = (SELECT [Type_Dbkey]
                                 FROM [Engine_Parts_Master] where [Engine_Part_Dbkey] = " + id + ")"));
                }

                List<Master_Part_Type> master_Part_Types = db.Master_Part_Types.Where(x => x.Hierarchy >= Hierarcy).ToList();
                List<SelectListItem> selectListItems = new List<SelectListItem>();
                foreach (Master_Part_Type item in master_Part_Types)
                {
                    selectListItems.Add(new SelectListItem() { Text = item.Type_Part_Name, Value = item.Type_Dbkey.ToString() });
                }
                return new SelectList(selectListItems, "Value", "Text");
            }

        }

        public static SelectList GetRolesDropDownList(bool defaultSelect = false)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            DataTable dataTable = new DataTable();
            using (DESI_STFE_PRODContext db = new())
            {
                try
                {
                    List<AspNetRole> AspNetRoles = DataCaching.getCachedRole();

                    if (defaultSelect)
                    {
                        selectListItems.Add(new SelectListItem() { Value = "", Text = "Select" });
                    }
                    foreach (var item in AspNetRoles)
                    {
                        string value = item.Id;
                        string text = item.Name;
                        selectListItems.Add(new SelectListItem() { Value = value, Text = text });
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }
                return new SelectList(selectListItems, "Value", "Text");
            }
        }

        public static SelectList GetAplhabetList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            foreach (Char item in alpha)
            {
                selectListItems.Add(new SelectListItem() { Text = item.ToString(), Value = item.ToString() });
            }
            return new SelectList(selectListItems, "Value", "Text");
        }

        public static SelectList BaseLineEngineLists()
        {
            using (DESI_STFE_PRODContext db = new())
            {
                List<Base_Line_Engine> base_Line_Engines = db.Base_Line_Engines.Where(x => x.Engine_Title != "T0").ToList();
                List<SelectListItem> selectListItems = new List<SelectListItem>();

                foreach (Base_Line_Engine item in base_Line_Engines)
                {
                    selectListItems.Add(new SelectListItem() { Text = item.Engine_Title, Value = item.BL_Engine_Dbkey.ToString() });
                }

                return new SelectList(selectListItems, "Value", "Text");
            }
        }


        public static SelectList GetMasterDropDownList(MetaMasterCode metaMasterCode, bool defaultSelect = false, string parentGUID = "All")
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            DataTable dataTable = new DataTable();
            using (DESI_STFE_PRODContext db = new())
            {
                try
                {
                    List<MetaMaster> metaMasters = DataCaching.getCachedMaster();

                    if (metaMasters != null)
                    {
                        metaMasters = metaMasters.Where(x => x.MasterType == metaMasterCode.ToString()).ToList();
                    }
                    if (parentGUID != "All")
                    {
                        metaMasters = metaMasters.Where(x => x.ParentGUID == parentGUID).ToList();
                    }
                    if (defaultSelect)
                    {
                        selectListItems.Add(new SelectListItem() { Value = "", Text = "Select" });
                    }
                    foreach (var item in metaMasters)
                    {
                        string value = item.UseValue == true ? item.MasterGUID : item.DisplayText;
                        string text = item.DisplayText;
                        selectListItems.Add(new SelectListItem() { Value = value, Text = text });
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }
                return new SelectList(selectListItems, "Value", "Text");
            }
        }

        public static List<MetaMaster> GetMasters(string MasterType = "All")
        {
            List<MetaMaster> masters = new List<MetaMaster>();
            try
            {
                masters = DataCaching.getCachedMaster();
                if (MasterType != "All")
                {
                    masters = masters.Where(x => x.MasterType == MasterType).ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
            return masters;
        }

        public static string GetMasterValue(List<MetaMaster> masters, string masterGUID)
        {
            MetaMaster master = masters.Where(x => x.MasterGUID == masterGUID).FirstOrDefault();
            return master == null ? "" : master.DisplayText;
        }

        public static SelectList GetMaster_General(string Type)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            using (DESI_STFE_PRODContext db = new())
            {
                List<Master_General> master_Part_Types = db.Master_Generals.Where(x => x.is_active == 1 && x.Master_Type == Type).ToList();
                selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
                foreach (var item in master_Part_Types)
                {
                    selectListItems.Add(new SelectListItem() { Text = item.Master_Name, Value = item.Master_Dbkey.ToString() });
                }
            }
            return new SelectList(selectListItems, "Value", "Text");
        }
        public static SelectList GetUsersList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();

            using (DESI_STFE_PRODContext db = new())
            {
                List<AspNetUser> userList = db.AspNetUsers.Where(x => x.IsActiveUser == true).ToList();
                selectListItems.Add(new SelectListItem() { Value = "0", Text = "Select" });
                foreach (var item in userList)
                {
                    selectListItems.Add(new SelectListItem() { Value = item.Id, Text = item.UserName });
                }
            }

            return new SelectList(selectListItems, "Value", "Text");
        }

        public static SelectList GetOldUsersList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();

            using (DESI_STFE_PRODContext db = new())
            {
                List<AspNetUser> userList = db.AspNetUsers.Where(x => x.IsActiveUser == true).ToList();
                selectListItems.Add(new SelectListItem() { Value = "0", Text = "Select" });
                foreach (var item in userList.Where(x => x.OldUserDbkey != null))
                {
                    selectListItems.Add(new SelectListItem() { Value = item.OldUserDbkey.ToString(), Text = item.UserName });
                }
            }

            return new SelectList(selectListItems, "Value", "Text");
        }

        public static SelectList RevisionList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            selectListItems.Add(new SelectListItem() { Text = "NIL", Value = "NIL" });
            foreach (Char item in alpha)
            {
                selectListItems.Add(new SelectListItem() { Text = item.ToString(), Value = item.ToString() });
            }
            return new SelectList(selectListItems, "Value", "Text");
        }

        public static SelectList GetVendorsList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();

            using (DESI_STFE_PRODContext db = new())
            {
                List<Vendor> vendorsList = db.Vendors.ToList();
                selectListItems.Add(new SelectListItem() { Value = "0", Text = "Select" });
                foreach (var item in vendorsList)
                {
                    selectListItems.Add(new SelectListItem() { Value = item.Vendor_Dbkey.ToString(), Text = item.Vendor_Name });
                }
            }
            return new SelectList(selectListItems, "Value", "Text");
        }
        public static SelectList GetPartsList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();

            using (DESI_STFE_PRODContext db = new())
            {
                List<Engine_Parts_Master> engine_Parts_Masters = db.Engine_Parts_Masters.ToList();
                selectListItems.Add(new SelectListItem() { Value = "0", Text = "NA" });
                foreach (var item in engine_Parts_Masters)
                {
                    selectListItems.Add(new SelectListItem() { Value = item.Engine_Part_Dbkey.ToString(), Text = item.Draw_part_no + "-" + item.Description ?? "" });
                }
            }
            return new SelectList(selectListItems, "Value", "Text");
        }
        public static SelectList GetEnginePartList_NeedTobeRefactored()
        {
            DataTable dt = DataCaching.getCachedEngineParts();
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            try
            {
                selectListItems.Add(new SelectListItem() { Value = "0", Text = "Select" });
                foreach (DataRow dr in dt.Rows)
                {
                    selectListItems.Add(new SelectListItem() { Value = dr["Engine_Part_Dbkey"].ToString(), Text = dr["Draw_part_no"].ToString() + " : " + dr["Description"].ToString() });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }

            return new SelectList(selectListItems, "Value", "Text");

        }

        public static SelectList GetMaster_General(string Type, bool usevalue = true)
        {
            List<Master_General> master_Part_Types = DataCaching.getCachedMasterGeneral().Where(x => x.Master_Type == Type).ToList();
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            try
            {
                selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
                foreach (Master_General item in master_Part_Types)
                {
                    selectListItems.Add(new SelectListItem() { Text = item.Master_Name, Value = usevalue ? item.Master_Dbkey.ToString() : item.Master_Name.ToString() });
                }
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); }

            return new SelectList(selectListItems, "Value", "Text");

        }


        public static SelectList GetPartTypeList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();

            using (DESI_STFE_PRODContext db = new())
            {
                List<Master_Part_Type> master_Part_Types = db.Master_Part_Types.ToList();
                selectListItems.Add(new SelectListItem() { Value = "0", Text = "Select" });
                foreach (var item in master_Part_Types)
                {
                    selectListItems.Add(new SelectListItem() { Value = item.Type_Dbkey.ToString(), Text = item.Type_Part_Name });
                }
            }
            return new SelectList(selectListItems, "Value", "Text");
        }


        public static SelectList RawMaterialList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();

            using (DESI_STFE_PRODContext db = new())
            {
                DataTable dataTable = MPGlobals.GetDataForDatalist("dbo.RawMaterialSelectList_SSP");
                List<Master_Rawmaterial> master_Rawmaterials = MPGlobals.ConvertDataTable<Master_Rawmaterial>(dataTable);
                selectListItems.Add(new SelectListItem() { Value = "0", Text = "Select" });
                foreach (var item in master_Rawmaterials)
                {
                    selectListItems.Add(new SelectListItem() { Value = item.Raw_material_Dbkey.ToString(), Text = item.Material_name });
                }
            }
            return new SelectList(selectListItems, "Value", "Text");
        }


        public static SelectList ReportingTypeList()
        {
            using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
            {

                DataTable dataTable = MPGlobals.GetDataForDatalist(@"SELECT distinct [Reporting_Type] FROM [dbo].[Engine_Parts_Master] 
                                                                    where [Reporting_Type] IS NOT NULL ");

                List<SelectListItem> selectListItems = new List<SelectListItem>();
                selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    selectListItems.Add(new SelectListItem() { Text = dataTable.Rows[i][0].ToString(), Value = dataTable.Rows[i][0].ToString() });
                }

                return new SelectList(selectListItems, "Value", "Text");
            }
        }
        public static SelectList ManufacturingProcessList()
        {
            using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
            {
                List<MetaMaster> metaMasters = db.MetaMasters.Where(x => x.MasterType == "ManufacturingProcess").ToList();
                List<SelectListItem> selectListItems = new List<SelectListItem>();
                selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
                foreach (var item in metaMasters)
                {
                    selectListItems.Add(new SelectListItem() { Value = item.DisplayText.ToString(), Text = item.DisplayText.ToString() });
                }
                return new SelectList(selectListItems, "Value", "Text");
            }
        }

        public static SelectList GetMaterialList()
        {
            DataTable data = GetRawMaterials();
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
            for (int i = 0; i < data.Rows.Count; i++)
            {
                selectListItems.Add(new SelectListItem() { Text = data.Rows[i]["RawmaterialName"].ToString(), Value = data.Rows[i]["Raw_material_Dbkey"].ToString() });
            }
            return new SelectList(selectListItems, "Value", "Text");
        }

        public static DataTable GetRawMaterials()
        {
            string cmdstr = "dbo.RawMaterial_SelectList_SSP";
            DataTable data = MPGlobals.GetDataForDatalist(cmdstr);
            return data;
        }


        public static DataTable GetEnginePartLists()
        {
            string Cmdstr = @"[dbo].[Get_MPL]";
            return MPGlobals.GetDataForDatalist(Cmdstr);
        }

        public static SelectList GetPersons()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
            using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
            {
                List<Person> Person = db.Persons.ToList();
                foreach (Person item in Person)
                {
                    string text = item.Person_Name;
                    string value = item.Person_Dbkey.ToString();
                    selectListItems.Add(new SelectListItem() { Text = text, Value = value });
                }
                return new SelectList(selectListItems, "Value", "Text");
            }
        }

        public static SelectList ProjectsList()
        {
            using (DESI_STFE_PRODContext db = new())
            {
                List<Project> project = db.Projects.ToList();
                List<SelectListItem> selectListItems = new List<SelectListItem>();
                selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
                foreach (Project item in project)
                {
                    selectListItems.Add(new SelectListItem() { Text = item.Title, Value = item.Project_Dbkey.ToString() });
                }
                return new SelectList(selectListItems, "Value", "Text");

            }
        }

        public static SelectList GetDemandingOfficersList(string listfor = "Dropdown")
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();

            using (DESI_STFE_PRODContext db = new())
            {
                DataTable dataTable = MPGlobals.GetDataForDatalist("dbo.DemandingOfficerList_SP");
                List<User> users = MPGlobals.ConvertDataTable<User>(dataTable);
                if (listfor == "DemandDashBoard")
                {
                    selectListItems.Add(new SelectListItem() { Value = "0", Text = "All" });
                }
                else
                {
                    selectListItems.Add(new SelectListItem() { Value = "0", Text = "Select" });
                }
               
                foreach (var item in users)
                {
                    selectListItems.Add(new SelectListItem() { Value = item.UserDbkey.ToString(), Text = item.UserName });
                }
            }
            return new SelectList(selectListItems, "Value", "Text");
        }
        public static SelectList GetMaster_General_Text(string Type)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            using (DESI_STFE_PRODContext db = new())
            {
                List<Master_General> master_Part_Types = db.Master_Generals.Where(x => x.is_active == 1 && x.Master_Type == Type).ToList();
                selectListItems.Add(new SelectListItem() { Text = "Select", Value = "" });
                foreach (var item in master_Part_Types)
                {
                    selectListItems.Add(new SelectListItem() { Text = item.Master_Name, Value = item.Master_Name });
                }
            }
            return new SelectList(selectListItems, "Value", "Text");
        }
        public static SelectList GetMaster_General_Value(string Type)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            using (DESI_STFE_PRODContext db = new())
            {
                List<Master_General> master_Part_Types = db.Master_Generals.Where(x => x.is_active == 1 && x.Master_Type == Type).ToList();
                selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
                foreach (var item in master_Part_Types)
                {
                    selectListItems.Add(new SelectListItem() { Text = item.Master_Name, Value = item.Master_Dbkey.ToString() });
                }
            }
            return new SelectList(selectListItems, "Value", "Text");
        }


        public static SelectList GetMPLParts()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
            DataTable dataTable = MPGlobals.GetDataForDatalist($"[dbo].[Get_MPL_TreeData] @BL_Engine_Db_key=0");
            foreach (DataRow rows in dataTable.Rows)
            {
                selectListItems.Add(new SelectListItem() { Text = rows.ItemArray[0].ToString(), Value = rows.ItemArray[2].ToString() });
            }
            return new SelectList(selectListItems, "Value", "Text");
        }


        public static SelectList GetCastApplicableParts(string OrderType)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
            DataTable dataTable = new();
            if (OrderType == "Casting")
            {
                dataTable = MPGlobals.GetDataForDatalist($"SELECT [Draw_part_no] + ':' + isnull([Description],'') as PartNumber ,[Engine_Part_Dbkey]  FROM [dbo].[Engine_Parts_Master] where FCBP = 'Casting'");
            }
            else
            {
                dataTable = MPGlobals.GetDataForDatalist($"SELECT [Draw_part_no] + ':' + isnull([Description],'') as PartNumber ,[Engine_Part_Dbkey]  FROM [dbo].[Engine_Parts_Master]");
            }

            foreach (DataRow rows in dataTable.Rows)
            {
                selectListItems.Add(new SelectListItem() { Text = rows.ItemArray[0].ToString(), Value = rows.ItemArray[1].ToString() });
            }
            return new SelectList(selectListItems, "Value", "Text");
        }

        public static SelectList GetMaster_Demand_DocumentType()
        {
            using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
            {
                List<Master_General> Document = db.Master_Generals.Where(x => x.is_active == 1 && x.Master_Type == "Demand_Document").ToList();
                List<SelectListItem> selectListItems = new List<SelectListItem>();
                selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
                foreach (Master_General item in Document)
                {
                    selectListItems.Add(new SelectListItem() { Text = item.Master_Name, Value = item.Master_Dbkey.ToString() });
                }
                return new SelectList(selectListItems, "Value", "Text");
            }
        }

        public static SelectList GetDemand_No()
        {
            using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
            {
                List<Procurement_Demand> procurement_Demands = db.Procurement_Demands.Where(x => x.IsActive != false).ToList();
                List<SelectListItem> selectListItems = new List<SelectListItem>();
                selectListItems.Add(new SelectListItem() { Text = "#NA", Value = "0" });
                foreach (Procurement_Demand item in procurement_Demands)
                {
                    selectListItems.Add(new SelectListItem() { Text = item.Demand_No, Value = item.DemandDbKey.ToString() });
                }
                return new SelectList(selectListItems, "Value", "Text");
            }
        }

        public static SelectList GetDemandSelectList()
        {
            using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
            {
                List<Procurement_Demand> procurement_Demands = db.Procurement_Demands.Where(x => x.IsActive != false).ToList();
                List<SelectListItem> selectListItems = new List<SelectListItem>();
                selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
                foreach (Procurement_Demand item in procurement_Demands)
                {
                    selectListItems.Add(new SelectListItem() { Text = item.MMG_File_No + "-" + item.Item_Description, Value = item.DemandDbKey.ToString() }); // removed + "-" + item.Demand_No
                }
                return new SelectList(selectListItems, "Value", "Text");
            }
        }

        public static SelectList DemandReciptDocsReference(int receipt_dbkey)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            string cmdstr = @"Select '0' as Attachment_Db_Key ,'Select' as DocFileName union all SELECT      
                Convert(varchar(50), dbo.Attachments.Attachment_Db_Key) as Attachment_Db_Key,
                dbo.Master_General.Master_Name
                + ' [' + SUBSTRING(dbo.Attachments.Orginal_File_Name, 1, 5) + '...' + RIGHT(dbo.Attachments.Orginal_File_Name, 4) + ']'
                + case when isnull(dbo.Attachments.File_Revision,'') <> '' then '[' + isnull(dbo.Attachments.File_Revision, '') + ']' else '' end as DocFileName
                FROM dbo.Attachments LEFT OUTER JOIN
                dbo.Master_General ON dbo.Attachments.File_DVD_Num = dbo.Master_General.Master_Dbkey
                WHERE(dbo.Attachments.Source_table = 'Procurement_Demand_Receipts') AND(dbo.Master_General.Master_Type = 'Procurement_Doument_Type') AND
                (dbo.Attachments.Source_table_key = " + receipt_dbkey + ")";
            DataTable dataTable = MPGlobals.GetDataForDatalist(cmdstr);
            foreach (DataRow rows in dataTable.Rows)
            {
                selectListItems.Add(new SelectListItem() { Value = rows.ItemArray[0].ToString(), Text = rows.ItemArray[1].ToString() });
            }
            return new SelectList(selectListItems, "Value", "Text");

        }

        public static SelectList ForgingItemsDocsReference(int itemDbKey)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            string cmdstr = @"Select 0 as Attachment_Db_Key ,'Select' as DocFileName
                            union all SELECT      
                            dbo.Attachments.Attachment_Db_Key,
                            dbo.Master_General.Master_Name
                            + ' [' + SUBSTRING(dbo.Attachments.Orginal_File_Name, 1, 5) + '...' + RIGHT(dbo.Attachments.Orginal_File_Name, 4) + ']'
                            + case when isnull(dbo.Attachments.File_Revision,'') <> '' then '[' + isnull(dbo.Attachments.File_Revision, '') + ']' else '' end as DocFileName
                            FROM dbo.Attachments LEFT OUTER JOIN
                                                     dbo.Master_General ON dbo.Attachments.File_DVD_Num = dbo.Master_General.Master_Dbkey
                            WHERE(dbo.Attachments.Source_table = 'Forging_Receipt_Items') AND(dbo.Master_General.Master_Type = 'Procurement_Doument_Type') AND
                (dbo.Attachments.Source_table_key = " + itemDbKey + ")";
            DataTable dataTable = MPGlobals.GetDataForDatalist(cmdstr);
            foreach (DataRow rows in dataTable.Rows)
            {
                selectListItems.Add(new SelectListItem() { Value = rows.ItemArray[0].ToString(), Text = rows.ItemArray[1].ToString() });
            }
            return new SelectList(selectListItems, "Value", "Text");
        }

        public static SelectList CastingReciptDocsReference(int CastingDbkey)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            string cmdstr = @"Select '0' as Attachment_Db_Key ,'Select' as DocFileName union all SELECT      
                Convert(varchar(50), dbo.Attachments.Attachment_Db_Key) as Attachment_Db_Key,
                dbo.Master_General.Master_Name
                + ' [' + SUBSTRING(dbo.Attachments.Orginal_File_Name, 1, 5) + '...' + RIGHT(dbo.Attachments.Orginal_File_Name, 4) + ']'
                + case when isnull(dbo.Attachments.File_Revision,'') <> '' then '[' + isnull(dbo.Attachments.File_Revision, '') + ']' else '' end as DocFileName
                FROM dbo.Attachments LEFT OUTER JOIN
                dbo.Master_General ON dbo.Attachments.File_DVD_Num = dbo.Master_General.Master_Dbkey
                WHERE(dbo.Attachments.Source_table = 'Casting_Forging_File') AND(dbo.Master_General.Master_Type = 'Procurement_Doument_Type') AND
                (dbo.Attachments.Source_table_key = " + CastingDbkey + ")";
            DataTable dataTable = MPGlobals.GetDataForDatalist(cmdstr);
            foreach (DataRow rows in dataTable.Rows)
            {
                selectListItems.Add(new SelectListItem() { Value = rows.ItemArray[0].ToString(), Text = rows.ItemArray[1].ToString() });
            }
            return new SelectList(selectListItems, "Value", "Text");

        }


        public static SelectList GetEnginePartList()
        {
            DataTable dt = Masters.GetEnginePartLists();
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Value = "0", Text = "Select" });
            foreach (DataRow dr in dt.Rows)
            {
                selectListItems.Add(new SelectListItem() { Value = dr["mpldbkey"].ToString(), Text = dr["Draw_part_no"].ToString() + "-" + dr["Description"].ToString() });
            }
            return new SelectList(selectListItems, "Value", "Text");
        }


        public static SelectList GetEnginePartListForMaterialIssue()
        {
            DataTable dt = MPGlobals.GetDataForDatalist("Select Engine_Part_Dbkey as mpldbkey,Draw_part_no,Description from Engine_Parts_Master");
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Value = "0", Text = "Select" });
            foreach (DataRow dr in dt.Rows)
            {
                selectListItems.Add(new SelectListItem() { Value = dr["mpldbkey"].ToString(), Text = dr["Draw_part_no"].ToString() + "-" + dr["Description"].ToString() });
            }
            return new SelectList(selectListItems, "Value", "Text");
        }


        public static SelectList GetMaterialIssueNote()
        {
            using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
            {
                List<Material_Issue_Note> material_Issue_Notes = db.Material_Issue_Notes.Where(x => x.IsActive != false).ToList();
                List<SelectListItem> selectListItems = new List<SelectListItem>();
                selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
                foreach (Material_Issue_Note item in material_Issue_Notes)
                {
                    selectListItems.Add(new SelectListItem() { Text = item.Demand_No + "-" + item.Order_Ref_No, Value = item.Issue_Dbkey.ToString() });
                }
                return new SelectList(selectListItems, "Value", "Text");
            }
        }

        public static SelectList GetDemandOrderType(string Type)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            using (DESI_STFE_PRODContext db = new())
            {
                List<MetaMaster> masters = db.MetaMasters.Where(x => x.IsActive == true && x.MasterType == Type).ToList();
                selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
                foreach (var item in masters)
                {
                    selectListItems.Add(new SelectListItem() { Text = item.DisplayText, Value = item.DisplayText });
                }
            }
            return new SelectList(selectListItems, "Value", "Text");
        }

        public static SelectList GetPartsListForMaterialIssueDropDown()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            DataTable dataTable = MPGlobals.GetDataForDatalist("dbo.Get_PartsList_For_MaterialIssue_Dropdown");
            selectListItems.Add(new SelectListItem() { Value = "0", Text = "Select" });
            foreach (DataRow dr in dataTable.Rows)
            {
                selectListItems.Add(new SelectListItem() { Value = dr["Engine_Part_Dbkey"].ToString(), Text = dr["Draw_part_no"].ToString() + "/" + dr["Description"].ToString() });
            }
            return new SelectList(selectListItems, "Value", "Text");
        }

        public static SelectList Get_UserList_For_RevisionManagement_DropDown()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            DataTable dataTable = MPGlobals.GetDataForDatalist("dbo.Get_UserList_RevisionManagement_Dropdown");
            selectListItems.Add(new SelectListItem() { Value = "0", Text = "Select" });
            foreach (DataRow dr in dataTable.Rows)
            {
                selectListItems.Add(new SelectListItem() { Value = dr["OldUserDbkey"].ToString(), Text = dr["UserName"].ToString() });
            }
            return new SelectList(selectListItems, "Value", "Text");
        }

        public static SelectList GetMaster_General_SameValueAsText(string Type)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            using (DESI_STFE_PRODContext db = new())
            {
                List<Master_General> master_Part_Types = db.Master_Generals.Where(x => x.is_active == 1 && x.Master_Type == Type).ToList();
                selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
                foreach (var item in master_Part_Types)
                {
                    selectListItems.Add(new SelectListItem() { Text = item.Master_Name, Value = item.Master_Name });
                }
            }
            return new SelectList(selectListItems, "Value", "Text");
        }

        public static SelectList GetUsersForWorkflowAssignment(int ModuleID =0)
        {
			List<SelectListItem> selectListItems = new List<SelectListItem>();
				DataTable dataTable = MPGlobals.GetDataForDatalist($"[dbo].[Get_Users_WorkflowAssignment]  @ModuleID = {ModuleID}");
				selectListItems.Add(new SelectListItem() { Text = "Select", Value = " " });
				foreach (DataRow dr in dataTable.Rows)
				{
					selectListItems.Add(new SelectListItem() {  Text = dr["UserName"].ToString() , Value = dr["UserGuid"].ToString() });
				}
				return new SelectList(selectListItems, "Value", "Text");
		}

        public static SelectList GetMailTypeList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            DataTable dataTable = MPGlobals.GetDataForDatalist("select distinct MailType from Mailer_log ");
            selectListItems.Add(new SelectListItem() { Value = "All", Text = "All" });
            foreach (DataRow dr in dataTable.Rows)
            {
                selectListItems.Add(new SelectListItem() { Value = dr["MailType"].ToString(), Text = dr["MailType"].ToString()});
            }
            return new SelectList(selectListItems, "Value", "Text");
        }

		public static SelectList GetDepartmentList()
		{
			List<SelectListItem> selectListItems = new List<SelectListItem>();
			using (DESI_STFE_PRODContext db = new())
			{
				List<MetaMaster> departmentData = db.MetaMasters.Where(x => x.MasterType == "Departments" && x.IsActive == true).ToList();
				selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
				foreach (var item in departmentData)
				{
					selectListItems.Add(new SelectListItem() { Text = item.DisplayText, Value = item.Id.ToString() });
				}
			}
			return new SelectList(selectListItems, "Value", "Text");
		}
		

		public static DataTable GetProcurementItemTypelist()
		{
			string cmdstr = "select 'All' as Master_Name union all select Master_Name   FROM [dbo].[Master_General] where Master_Type='Procurement_Item_Type' and is_active=1 ";
			DataTable data = MPGlobals.GetDataForDatalist(cmdstr);
			return data;
		}

        public static SelectList GetEngineList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            using (DESI_STFE_PRODContext db = new())
            {
                AppSetting engineData = db.AppSettings.Where(x => x.AppSettingType == "Engines" ).FirstOrDefault();
                var Engines = engineData.DataJson.Split(",");
                selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });

                foreach (var item in Engines)
                {
                    selectListItems.Add(new SelectListItem() { Text = item, Value = item });
                }
            }
            return new SelectList(selectListItems, "Value", "Text");
        }

        public static string GetDocName(int AttachmentDbkey)
        {
            string fileName = "";
            string cmdstr = "SELECT   dbo.Master_General.Master_Name FROM [dbo].[Attachments]  LEFT OUTER JOIN       dbo.Master_General ON dbo.Attachments.File_DVD_Num = dbo.Master_General.Master_Dbkey where Attachment_Db_Key = " + AttachmentDbkey;
            DataTable dataTable = MPGlobals.GetDataForDatalist(cmdstr);
            foreach (DataRow rows in dataTable.Rows)
            {
                fileName =  rows.ItemArray[0].ToString() ?? "";
            }
            return fileName;
        }

    }

}
