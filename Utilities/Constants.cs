using System.ComponentModel;

namespace MPCRS.Utilities
{

    public class PermissionDescription : Attribute
    {
        public string Module { get; private set; }
        public string Description { get; private set; }

        public PermissionDescription(string module, string descrption)
        {
            this.Module = module;
            this.Description = descrption;
        }
    }

    public class RoleDescription : Attribute
    {
        public string Module { get; private set; }
        public string Description { get; private set; }

        public RoleDescription(string module, string descrption)
        {
            this.Module = module;
            this.Description = descrption;
        }
    }

    public static class EnumHelper
    {
        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }
    }




    public class Constants
    {
        public enum UserPermissions
        {
            [PermissionDescription("User Management", "Read User Information")]
            User_Read,
            [PermissionDescription("User Management", "Create and Edit User")]
            User_Write,
            [PermissionDescription("User Management", "Delete User")]
            User_Delete,
            [PermissionDescription("User Management", "Create and Edit Roles")]
            User_RolesWrite,
            [PermissionDescription("Masters", "Read Masters")]
            Masters_Read,
            [PermissionDescription("Masters", "Create and Edit Masters")]
            Masters_Write,
            [PermissionDescription("ACSN", "View ACSN List & Reports")]
            ACSN_Read,
            [PermissionDescription("ACSN", "Create and Edit ACSN Items")]
            ACSN_Write,
            [PermissionDescription("ACSN", "Delete ACSN Items")]
            ACSN_Delete,

            [PermissionDescription("Document Management", "Access to Document Browser")]
            DOC_Read,
            
            [PermissionDescription("Document Management", "Document Access Configuration")]
            DOC_Write,
            [PermissionDescription("Document Management", "Delete Document Folder")]
            DOC_Delete_Folder,
            [PermissionDescription("Document Management", "Drag and Drop / Move Folder and Files")]
            DOC_Dnd,

            [PermissionDescription("Performance Prediction", "Read Predictions")]
            Performance_Prediction_Read,
            [PermissionDescription("Performance Prediction", "Run Predictions")]
            Performance_Prediction_Write,

            [PermissionDescription("Standard of Preparation", "Read Engine Builds, SOP Template & SOP Information")]
            SOP_Read,
            [PermissionDescription("Standard of Preparation", "Edit Engine Builds, SOP Template & SOP Information,SOP Custom Access")]
            SOP_Write,
            [PermissionDescription("Standard of Preparation", "Delete SOP Build")]
            SOP_Delete,
            [PermissionDescription("Standard of Preparation", "Read documents")]
            Sop_Documents_Read,

            [PermissionDescription("Standard of Preparation", "Write documents")]
            Sop_Documents_Write,

            [PermissionDescription("Standard of Preparation", "Delete documents")]
            Sop_Documents_Delete,

            [PermissionDescription("SOP Report Template", "Create/Edit Section Template")]
            Sop_ReportTemplate_Write,

			[PermissionDescription("SOP Documents Status", "Read SOP Documents Status")]
			Sop_Documents_Status_Read,

			[PermissionDescription("NCR", "Read NCR")]
            NCR_Read,

            [PermissionDescription("NCR", "Create / Edit NCR")]
            NCR_Write,

            [PermissionDescription("NCR", "Delete NCR")]
            NCR_Delete,

            [PermissionDescription("NCR", "NCR Assignments Admin")]
            NCR_Assignments_Admin,

            [PermissionDescription("NCR", "NCR Assignments Admin Delete")]
            NCR_Assignments_Admin_Delete,
            
            [PermissionDescription("NCR", "NCR Assignments Module Delete")]
            NCR_Assignments_Module_Delete,

            [PermissionDescription("NCR", "NCR Module User")]
            NCR_Module_User,

            [PermissionDescription("NCR", "View all assigned NCR - Read only")]
            NCR_Assignments_Readonly,

            [PermissionDescription("Raw Materials", "Read Raw Materials")]
            Raw_Materials_Read,

            [PermissionDescription("Raw Materials", "Create/Edit Raw Materials")]
            Raw_Materials_Write,

            [PermissionDescription("Raw Materials", "Read Inventory Report")]
            Inventory_Read,

            [PermissionDescription("Vendors", "Read Vendors")]
            Vendors_Read,

            [PermissionDescription("Vendors", "Create/Edit Vendors")]
            Vendors_Write,
            [PermissionDescription("Projects", "Read Projects")]
            Projects_Read,

            [PermissionDescription("Projects", "Create/Edit Projects")]
            Projects_Write,

            [PermissionDescription("Configuration", "Record Deletion Requests / Approval")]
            Record_Deletion,

            [PermissionDescription("Configuration", "Read Configuration")]
            Configuration_Read,

            [PermissionDescription("Configuration", "Create/Edit Configuration")]
            Configuration_Write,

            [PermissionDescription("Master Part List", "Read MPL Tree/Table View")]
            MPL_Read,

            [PermissionDescription("Master Part List", "Verify Raw Material / Add Addition Info")]
            MPL_Write,

            [PermissionDescription("MPL Report", "Read MPL Report / MPL Revisions")]
            MPL_Report_Read,

            [PermissionDescription("MPL Report", "Edit Assy display order/ Add addition report info - MPL Report")]
            MPL_Report_Write,


            [PermissionDescription("MPL Revision Management", "Create Part / Alter Master Parts")]
            MPL_Revision_Write,

            [PermissionDescription("MPL Revision Management", "Read MPL Revision")]
            MPL_Revision_Read,

			[PermissionDescription("MPL", "Read MPL Drawing Status")]
			MPL_DocumentStatus_Read,

			[PermissionDescription("Base Line Engines", "Read BL Engines")]
            BaseLineEngine_Read,

            [PermissionDescription("Base Line Engines", "Create/Alter/Clone/Add MPL Approvers")]
            BaseLineEngine_Write,

            [PermissionDescription("Demand Management", "Read Demands")]
            Demand_Read,

            [PermissionDescription("Demand Management", "Create/Edit Demand")]
            Demand_Write,
            
            [PermissionDescription("Demand Management", "Read Financial Details")]
            Demand_Financial_Read,//Added

            [PermissionDescription("Demand Management", "Edit Financial Details")]
            Demand_Financial_Edit,//Added


            [PermissionDescription("Demand Management", "Create/Edit Demand Receipts")]
            Demand_Write_Receipts,

            [PermissionDescription("Demand Management", "Upload Demand Documents")]
            Demand_Write_Documents,

            [PermissionDescription("Demand Management", "Delete Demand, Demand Items, Receipts, Milestones and Documents")]
            Demand_Delete,

            [PermissionDescription("Demand Management", "Delete Demand Receipts")]
            Demand_Receipt_Delete,

            [PermissionDescription("Demand Management", "Short Closure")]
            Demand_Short_Closure,

            [PermissionDescription("Demand Management", "Demanding Officer")]
            Demand_Demanding_Officer,

            [PermissionDescription("Procurement", "Procurement Reports")]
            Procurement_Reports,

            [PermissionDescription("Casting", "Read Casting Orders")]
            Casting_Read,
            [PermissionDescription("Casting", "Create/Edit Casting Order")]
            Casting_Write,
            [PermissionDescription("Casting", "Create/Edit Casting Receipts")]
            Casting_Write_Receipts,
            [PermissionDescription("Casting", "Delete Casting Order, Items, Receipts and Documents")]
            Casting_Delete,

            [PermissionDescription("Casting/Pyro/Forging", "Order Module User")]
            Order_Module_User,
            [PermissionDescription("Casting/Pyro/Forging", "Order Module Comment")]
            Order_Receipt_Comment,

            [PermissionDescription("Pyro", "Read Pyro Orders")]
			Pyro_Read,
			[PermissionDescription("Pyro", "Create/Edit Pyro Order")]
			Pyro_Write,
			[PermissionDescription("Pyro", "Create/Edit Pyro Receipts")]
			Pyro_Write_Receipts,
			[PermissionDescription("Pyro", "Delete Pyro Order, Items, Receipts and Documents")]
			Pyro_Delete,

			[PermissionDescription("Forging", "Read Forging Orders")]
			Forging_Read,
			[PermissionDescription("Forging", "Create/Edit Forging Order")]
			Forging_Write,
			[PermissionDescription("Forging", "Create/Edit Forging Receipts")]
			Forging_Write_Receipts,
			[PermissionDescription("Forging", "Delete Forging Order, Items, Receipts and Documents")]
			Forging_Delete,



			[PermissionDescription("Material Issue", "Read Material Issue")]
            MaterialIssue_Read,

			[PermissionDescription("Material Issue", "Create/Edit Material Issue")]
			MaterialIssue_Write,

			[PermissionDescription("Material Issue", "Create/Edit Forging Receipts")]
			MaterialIssue_ForgingReceipts,

			[PermissionDescription("Material Issue", "Delete Issue Order, Items, Receipts and Documents")]
			MaterialIssue_Delete,

            [PermissionDescription("Approval Management", "Read Approval Management Requests")]
            Approval_Management_Read_Requests,

            [PermissionDescription("Approval Management", "Approve or Reject Requests")]
            Approval_Management_Approve_Requests,

            [PermissionDescription("MPL Drawings", "View MPL Drawings")]
            MPLDocuments_View,

			[PermissionDescription("MPL Drawings", "Download MPL Drawings")]
			MPLDocuments_Download,
 
            [PermissionDescription("MPL Drawings", "Download Without Water Mark")]
            Download_WO_waterMark,

            [PermissionDescription("Test Data Repository", "Read Test Data Repository")]
			Test_Data_Repository_Read,
			
            [PermissionDescription("Configuration", "Read Exception Logs")]
			Exception_Logs_Read,

			[PermissionDescription("Master General", "Read Master General")]
			Master_General_Read,
            
            [PermissionDescription("Audit Logs", "Read Audit Logs")]
			Read_Audit_Logs,

            [PermissionDescription("Master Search", "Access Master Search in home page")]
            Access_Master_Search,

			[PermissionDescription("Generative AI", "Chat with Database")]
            GEN_AI,

			[PermissionDescription("Generative AI", "Chat with Documents")]
			Chat_With_Documents,

           [PermissionDescription("Generative AI", "Doc Embedding status")]
            Doc_Embedding_status,

            [PermissionDescription("Import Inspection Report", "Upload File")]
            MsAccess_File_Upload,
            [PermissionDescription("Import Inspection Report", "Read/View Inspection Reports")]
            Import_Inspection_Report_Read,

            [PermissionDescription("Task Planner", "Access and view the Task Planner interface")]
            TaskPlanner_View,

            // Projects
            [PermissionDescription("Task Planner", "View Projects")]
            TaskPlanner_Projects_Read,
            [PermissionDescription("Task Planner", "Create and Edit Projects")]
            TaskPlanner_Projects_Write,
            [PermissionDescription("Task Planner", "Delete Projects")]
            TaskPlanner_Projects_Delete,

            // Tasks
            [PermissionDescription("Task Planner", "View Tasks")]
            TaskPlanner_Tasks_Read,
   
            [PermissionDescription("Task Planner", "View All Tasks (Public + My Private)")]
            TaskPlanner_TaskList_AllTasks,

            [PermissionDescription("Task Planner", "View User Tasks (Assigned to Me + My Private)")]
            TaskPlanner_TaskList_UserTasks,

            [PermissionDescription("Task Planner", "Create and Edit Tasks")]
            TaskPlanner_Tasks_Write,
            [PermissionDescription("Task Planner", "Delete Tasks")]
            TaskPlanner_Tasks_Delete,
            [PermissionDescription("Task Planner", "Assign Users to Tasks")]
            TaskPlanner_Tasks_Assign,

            // Comments
            [PermissionDescription("Task Planner", "View Task Comments")]
            TaskPlanner_Comments_Read,
            [PermissionDescription("Task Planner", "Add and Edit Comments")]
            TaskPlanner_Comments_Write,
            [PermissionDescription("Task Planner", "Delete Comments")]
            TaskPlanner_Comments_Delete,

            // Attachments
            [PermissionDescription("Task Planner", "Upload Attachments")]
            TaskPlanner_Attachments_Upload,
            [PermissionDescription("Task Planner", "Delete Attachments")]
            TaskPlanner_Attachments_Delete,

            // Reports
            [PermissionDescription("Task Planner", "View Reports and Dashboard")]
            TaskPlanner_Reports_View,

            // Email Integration
            [PermissionDescription("Task Planner", "View Emails")]
            TaskPlanner_Emails_Read,
            [PermissionDescription("Task Planner", "Convert Emails to Tasks")]
            TaskPlanner_Emails_Convert,

            [PermissionDescription("Task Planner", "TaskPlanner Admin")]
            TaskPlanner_Admin,

            [PermissionDescription("ION", "ION Admin")]
            ION_Admin, 
            [PermissionDescription("ION", "Read IONs")]
            ION_View,
            [PermissionDescription("ION", "Approve IONs")]
            ION_Approve, 
            [PermissionDescription("ION", "Edit IONs")]
            ION_Edit, 
            [PermissionDescription("ION", "Create IONs")]
            ION_Create, 
            [PermissionDescription("ION", "Delete IONs")]
            ION_Delete,
            [PermissionDescription("ION", "ION Support Operator - View all IONs, upload scanned copies, mark approved")]
            ION_SupportOperator,

            [PermissionDescription("Inward ION", "Inward ION Admin")]
            ION_Inward_Admin,
            [PermissionDescription("Inward ION", "View Inward IONs")]
            ION_Inward_View,
            [PermissionDescription("Inward ION", "Create Inward IONs")]
            ION_Inward_Create,
            [PermissionDescription("Inward ION", "Edit Inward IONs")]
            ION_Inward_Edit,
            [PermissionDescription("Inward ION", "Delete Inward IONs")]
            ION_Inward_Delete,

            [PermissionDescription("Data Entry Tracking", "Configure Data Entry Tracking modules")]
            DataEntry_Tracking_Config,
            [PermissionDescription("Data Entry Tracking", "View Data Entry Tracking Dashboard")]
            DataEntry_Tracking_Read,

            [PermissionDescription("Drawing Bubble Inspection", "View Drawing Bubble Inspection")]
            DrawingBubble_Read,

            [PermissionDescription("Drawing Bubble Inspection", "Process Drawing Bubble Inspection")]
            DrawingBubble_Process,

            [PermissionDescription("Drawing Bubble Inspection", "Delete Drawing Bubble Inspection")]
            DrawingBubble_Delete,

        }
 
        public enum VTSDataRecordType
        {
            [Description("VTSDataRecordType")]
            New,
        }
        public enum CacheKeys
        {
            [Description("CacheKeys")]
            MetaMaster,
            OrganizantionList,
            Persons,
            TicketCategories,
            Categories,
            EscalateList,
            AssetsList,
            RoleClaims,
            AspNetRoles,
            ACSNList,
            AllEngineParts,
            MasterGeneral,
            dashborad
        }
        public enum LoginBy
        {
            OTP, Email, Impersonate
        }

        public enum MetaMasterCode
        {
            BloodGroup
           , BodyType
           , Brand
           , Country
           , DriveType
           , Education
           , FuelType
           , Gender
           , MaritalStatus
           , Nationality
           , Prefix
           , Profession
           , Religion
           , State
           , VehicleColor
           , VehicleType
           , MfgYear
           , AssetType
           , Binary
           , Designations
           , LeaveType
           , HolidayType
           , DocumentType
           , Document
           , AttendanceRegularization
           , TicketStatus
           , TicketImpacts
           , TicketUrgency
           , OrganizantionType
           , EscalateTicketCategory
           , EscalateTicketStatus
           , PersonType
           , Departments
           , AcsnSeries
           , ACSNRecordStatus
                , CastingReceiptItemRemarks


        }

        public enum dbOperation
        {
            Create, Read, Update, Delete
        }

        public enum HttpAction
        {
            GET,
            POST,
            PUT,
            PATCH,
            DELETE
        }



    }



}
