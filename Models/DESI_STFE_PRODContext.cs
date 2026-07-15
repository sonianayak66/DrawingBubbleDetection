using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MPCRS.Models;

public partial class DESI_STFE_PRODContext : DbContext
{
    public DESI_STFE_PRODContext()
    {
    }

    public DESI_STFE_PRODContext(DbContextOptions<DESI_STFE_PRODContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ACSN> ACSNs { get; set; }

    public virtual DbSet<ACSNItem> ACSNItems { get; set; }

    public virtual DbSet<ACSN_Config> ACSN_Configs { get; set; }

    public virtual DbSet<ActionLog> ActionLogs { get; set; }

    public virtual DbSet<AppSetting> AppSettings { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserRole> AspNetUserRoles { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<AuditLogDisplayManager> AuditLogDisplayManagers { get; set; }

    public virtual DbSet<Audit_log> Audit_logs { get; set; }

    public virtual DbSet<BATL_Build_assignment_Json> BATL_Build_assignment_Jsons { get; set; }

    public virtual DbSet<BATL_CastingSerialMapping_Json> BATL_CastingSerialMapping_Jsons { get; set; }

    public virtual DbSet<BATL_NCR_Json> BATL_NCR_Jsons { get; set; }

    public virtual DbSet<BATL_RMC_Json> BATL_RMC_Jsons { get; set; }

    public virtual DbSet<Base_Line_Engine> Base_Line_Engines { get; set; }

    public virtual DbSet<Base_Line_Engines_Approver> Base_Line_Engines_Approvers { get; set; }

    public virtual DbSet<CastingDetail> CastingDetails { get; set; }

    public virtual DbSet<CastingItem> CastingItems { get; set; }

    public virtual DbSet<CastingReceiptQtySplit> CastingReceiptQtySplits { get; set; }

    public virtual DbSet<CastingReceiptsComment> CastingReceiptsComments { get; set; }

    public virtual DbSet<CastingReceiptsItemSplit> CastingReceiptsItemSplits { get; set; }

    public virtual DbSet<Casting_DepartmentOrder> Casting_DepartmentOrders { get; set; }

    public virtual DbSet<Casting_MaterialIssue> Casting_MaterialIssues { get; set; }

    public virtual DbSet<Casting_MaterialIssue_Item> Casting_MaterialIssue_Items { get; set; }

    public virtual DbSet<Custom_Css_Style> Custom_Css_Styles { get; set; }

    public virtual DbSet<DataEntry_Tracking_Config> DataEntry_Tracking_Configs { get; set; }

    public virtual DbSet<DataEntry_Tracking_History> DataEntry_Tracking_Histories { get; set; }

    public virtual DbSet<DataEntry_Tracking_Remark> DataEntry_Tracking_Remarks { get; set; }

    public virtual DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    public virtual DbSet<Demand_Document> Demand_Documents { get; set; }

    public virtual DbSet<Demand_Verification> Demand_Verifications { get; set; }

    public virtual DbSet<Documentation> Documentations { get; set; }

    public virtual DbSet<Documents_Access_Config> Documents_Access_Configs { get; set; }

    public virtual DbSet<Drawing_Bubble_Inspection> Drawing_Bubble_Inspections { get; set; }

    public virtual DbSet<Drawing_Bubble_Inspection_Item> Drawing_Bubble_Inspection_Items { get; set; }

    public virtual DbSet<EBC_SerialNoLog> EBC_SerialNoLogs { get; set; }

    public virtual DbSet<Engine> Engines { get; set; }

    public virtual DbSet<EngineBuild> EngineBuilds { get; set; }

    public virtual DbSet<EngineBuildComponent> EngineBuildComponents { get; set; }

    public virtual DbSet<Engine_Parts_Master> Engine_Parts_Masters { get; set; }

    public virtual DbSet<Engine_Parts_Master_Log> Engine_Parts_Master_Logs { get; set; }

    public virtual DbSet<Engine_Parts_Revisoin_History> Engine_Parts_Revisoin_Histories { get; set; }

    public virtual DbSet<Engine_Parts_Usage> Engine_Parts_Usages { get; set; }

    public virtual DbSet<ExceptionLog> ExceptionLogs { get; set; }

    public virtual DbSet<ExternalMfgStatus> ExternalMfgStatuses { get; set; }

    public virtual DbSet<Forging_Receipt> Forging_Receipts { get; set; }

    public virtual DbSet<Forging_Receipt_Item> Forging_Receipt_Items { get; set; }

    public virtual DbSet<Forging_Split> Forging_Splits { get; set; }

    public virtual DbSet<GanttTask> GanttTasks { get; set; }

    public virtual DbSet<ION_Destination> ION_Destinations { get; set; }

    public virtual DbSet<ION_Enclosure> ION_Enclosures { get; set; }

    public virtual DbSet<ION_FileGroup> ION_FileGroups { get; set; }

    public virtual DbSet<ION_InwardNote> ION_InwardNotes { get; set; }

    public virtual DbSet<ION_Note> ION_Notes { get; set; }

    public virtual DbSet<ION_NoteRecipient> ION_NoteRecipients { get; set; }

    public virtual DbSet<ION_OfficeConfig> ION_OfficeConfigs { get; set; }

    public virtual DbSet<ION_SerialTracking> ION_SerialTrackings { get; set; }

    public virtual DbSet<ION_Template> ION_Templates { get; set; }

    public virtual DbSet<InspectionReport> InspectionReports { get; set; }

    public virtual DbSet<InspectionReportRecord> InspectionReportRecords { get; set; }

    public virtual DbSet<Issue> Issues { get; set; }

    public virtual DbSet<IssueComment> IssueComments { get; set; }

    public virtual DbSet<IssueHistory> IssueHistories { get; set; }

    public virtual DbSet<Job_Card> Job_Cards { get; set; }

    public virtual DbSet<MPLReportView> MPLReportViews { get; set; }

    public virtual DbSet<Mail_Credential> Mail_Credentials { get; set; }

    public virtual DbSet<Mail_Template> Mail_Templates { get; set; }

    public virtual DbSet<Mailer_Log> Mailer_Logs { get; set; }

    public virtual DbSet<Manufacturing_Process_Documents_Required> Manufacturing_Process_Documents_Requireds { get; set; }

    public virtual DbSet<Master_Activity> Master_Activities { get; set; }

    public virtual DbSet<Master_General> Master_Generals { get; set; }

    public virtual DbSet<Master_Part_Type> Master_Part_Types { get; set; }

    public virtual DbSet<Master_Rawmaterial> Master_Rawmaterials { get; set; }

    public virtual DbSet<MaterialIssueDocument> MaterialIssueDocuments { get; set; }

    public virtual DbSet<MaterialIssue_DR_SplitMapping> MaterialIssue_DR_SplitMappings { get; set; }

    public virtual DbSet<Material_IssueItems_Consolidation> Material_IssueItems_Consolidations { get; set; }

    public virtual DbSet<Material_Issue_Item> Material_Issue_Items { get; set; }

    public virtual DbSet<Material_Issue_Items_Part> Material_Issue_Items_Parts { get; set; }

    public virtual DbSet<Material_Issue_Note> Material_Issue_Notes { get; set; }

    public virtual DbSet<MetaMaster> MetaMasters { get; set; }

    public virtual DbSet<NCR_Item_Rework> NCR_Item_Reworks { get; set; }

    public virtual DbSet<NCR_Item_Workflow_Tracking> NCR_Item_Workflow_Trackings { get; set; }

    public virtual DbSet<NCR_ModuleToUserMapping> NCR_ModuleToUserMappings { get; set; }

    public virtual DbSet<NCR_SerialNumber_Engine_Mapping> NCR_SerialNumber_Engine_Mappings { get; set; }

    public virtual DbSet<NCR_Workflow_Assignment> NCR_Workflow_Assignments { get; set; }

    public virtual DbSet<NCR_Workflow_Assignment_Module> NCR_Workflow_Assignment_Modules { get; set; }

    public virtual DbSet<NCR_Workflow_Assignment_User> NCR_Workflow_Assignment_Users { get; set; }

    public virtual DbSet<NCR_Workflow_Assignments_Log> NCR_Workflow_Assignments_Logs { get; set; }

    public virtual DbSet<NCR_Workflow_Assignments_New> NCR_Workflow_Assignments_News { get; set; }

    public virtual DbSet<NCR_Workflow_Stage> NCR_Workflow_Stages { get; set; }

    public virtual DbSet<NLPDatabaseDef> NLPDatabaseDefs { get; set; }

    public virtual DbSet<NLPDomainModelDef> NLPDomainModelDefs { get; set; }

    public virtual DbSet<NLtoSql_Log> NLtoSql_Logs { get; set; }

    public virtual DbSet<NavigationMenuControl> NavigationMenuControls { get; set; }

    public virtual DbSet<NonConformanceReport> NonConformanceReports { get; set; }

    public virtual DbSet<NonConformanceReport_Item> NonConformanceReport_Items { get; set; }

    public virtual DbSet<NonConformanceReport_Item_Rework> NonConformanceReport_Item_Reworks { get; set; }

    public virtual DbSet<NonConformanceReport_log> NonConformanceReport_logs { get; set; }

    public virtual DbSet<OTPVerification> OTPVerifications { get; set; }

    public virtual DbSet<Order_ModuleUserMapping> Order_ModuleUserMappings { get; set; }

    public virtual DbSet<PartManufactureStatus> PartManufactureStatuses { get; set; }

    public virtual DbSet<PerformancePrediction> PerformancePredictions { get; set; }

    public virtual DbSet<Person> Persons { get; set; }

    public virtual DbSet<ProcurementMilestone> ProcurementMilestones { get; set; }

    public virtual DbSet<Procurement_Demand> Procurement_Demands { get; set; }

    public virtual DbSet<Procurement_Demand_Item> Procurement_Demand_Items { get; set; }

    public virtual DbSet<Procurement_Demand_Item_Adjustment> Procurement_Demand_Item_Adjustments { get; set; }

    public virtual DbSet<Procurement_Demand_MileStone> Procurement_Demand_MileStones { get; set; }

    public virtual DbSet<Procurement_Demand_Receipt> Procurement_Demand_Receipts { get; set; }

    public virtual DbSet<Procurement_Demands_History> Procurement_Demands_Histories { get; set; }

    public virtual DbSet<Procurement_Milestone> Procurement_Milestones { get; set; }

    public virtual DbSet<Procurement_Milestone_Extension> Procurement_Milestone_Extensions { get; set; }

    public virtual DbSet<Procurement_Milestone_Item> Procurement_Milestone_Items { get; set; }

    public virtual DbSet<Procurement_Milestone_Merge_Source> Procurement_Milestone_Merge_Sources { get; set; }

    public virtual DbSet<Procurement_ReceiptItemSplit> Procurement_ReceiptItemSplits { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<RM_Qty_Vendor_Split> RM_Qty_Vendor_Splits { get; set; }

    public virtual DbSet<Rawmaterial_Consolidation> Rawmaterial_Consolidations { get; set; }

    public virtual DbSet<Rawmaterial_Inventory> Rawmaterial_Inventories { get; set; }

    public virtual DbSet<RecordDeletionLog> RecordDeletionLogs { get; set; }

    public virtual DbSet<RecordsForDeletion> RecordsForDeletions { get; set; }

    public virtual DbSet<Roles_URL_Module_map> Roles_URL_Module_maps { get; set; }

    public virtual DbSet<SOP_BuildReportSection> SOP_BuildReportSections { get; set; }

    public virtual DbSet<SOP_BuildReportSection_Backup> SOP_BuildReportSection_Backups { get; set; }

    public virtual DbSet<SOP_CustomAccessLink> SOP_CustomAccessLinks { get; set; }

    public virtual DbSet<SOP_ReportTemplate> SOP_ReportTemplates { get; set; }

    public virtual DbSet<SOP_ReportTemplate_Backup> SOP_ReportTemplate_Backups { get; set; }

    public virtual DbSet<SOP_ReportTemplate_Document> SOP_ReportTemplate_Documents { get; set; }

    public virtual DbSet<Static_MPL_Report_Datum> Static_MPL_Report_Data { get; set; }

    public virtual DbSet<TaskManager_ApplicationSetting> TaskManager_ApplicationSettings { get; set; }

    public virtual DbSet<TaskManager_Email> TaskManager_Emails { get; set; }

    public virtual DbSet<TaskManager_EmailConfiguration> TaskManager_EmailConfigurations { get; set; }

    public virtual DbSet<TaskManager_EmailConfiguration1> TaskManager_EmailConfigurations1 { get; set; }

    public virtual DbSet<TaskManager_EmailNotification> TaskManager_EmailNotifications { get; set; }

    public virtual DbSet<TaskManager_EmailTaskRelationship> TaskManager_EmailTaskRelationships { get; set; }

    public virtual DbSet<TaskManager_GlobalBucket> TaskManager_GlobalBuckets { get; set; }

    public virtual DbSet<TaskManager_Project> TaskManager_Projects { get; set; }

    public virtual DbSet<TaskManager_ProjectBucket> TaskManager_ProjectBuckets { get; set; }

    public virtual DbSet<TaskManager_Task> TaskManager_Tasks { get; set; }

    public virtual DbSet<TaskManager_TaskActivityLog> TaskManager_TaskActivityLogs { get; set; }

    public virtual DbSet<TaskManager_TaskAssignment> TaskManager_TaskAssignments { get; set; }

    public virtual DbSet<TaskManager_TaskChecklist> TaskManager_TaskChecklists { get; set; }

    public virtual DbSet<TaskManager_TaskComment> TaskManager_TaskComments { get; set; }

    public virtual DbSet<TaskTracker> TaskTrackers { get; set; }

    public virtual DbSet<Task_Tree_Master> Task_Tree_Masters { get; set; }

    public virtual DbSet<TestDataRepoJson> TestDataRepoJsons { get; set; }

    public virtual DbSet<TestDataRepository> TestDataRepositories { get; set; }

    public virtual DbSet<TestDataRepository_backup> TestDataRepository_backups { get; set; }

    public virtual DbSet<TestDataValue> TestDataValues { get; set; }

    public virtual DbSet<TestDataValue_backup> TestDataValue_backups { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserSession> UserSessions { get; set; }

    public virtual DbSet<User_Addtional_Role_Map> User_Addtional_Role_Maps { get; set; }

    public virtual DbSet<User_Module> User_Modules { get; set; }

    public virtual DbSet<User_Role> User_Roles { get; set; }

    public virtual DbSet<User_session_manager> User_session_managers { get; set; }

    public virtual DbSet<Utilization_Session> Utilization_Sessions { get; set; }

    public virtual DbSet<Utilization_Task_Log> Utilization_Task_Logs { get; set; }

    public virtual DbSet<Utilization_todo_StatusLog> Utilization_todo_StatusLogs { get; set; }

    public virtual DbSet<Utilization_todo_Task> Utilization_todo_Tasks { get; set; }

    public virtual DbSet<Vendor> Vendors { get; set; }

    public virtual DbSet<WatermarkConfiguration> WatermarkConfigurations { get; set; }

    public virtual DbSet<persons_datum> persons_data { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["MPCRS"]);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_CI_AI");

        modelBuilder.Entity<ACSN>(entity =>
        {
            entity.HasKey(e => e.acsnKey).HasName("PK_ACSNConfiguration");

            entity.ToTable("ACSN");

            entity.Property(e => e.ACSN_Status)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ACSNnum)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.DrawingNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModuleRefNum).IsUnicode(false);
            entity.Property(e => e.NewRevision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReceivedDate).HasColumnType("date");
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.Series)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.existingRevision)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ACSNItem>(entity =>
        {
            entity.HasKey(e => e.ACSNStatusKey);

            entity.Property(e => e.Documents)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.EndDate).HasColumnType("date");
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.StartDate).HasColumnType("date");
            entity.Property(e => e.acsnStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.updatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<ACSN_Config>(entity =>
        {
            entity.HasKey(e => e.AcsnConfigKey);

            entity.Property(e => e.Prefix)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Series)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ActionLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ActionLogs");

            entity.ToTable("ActionLog");

            entity.Property(e => e.ActionName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.MetaCode)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Method)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.HasKey(e => e.AppSettingKey).HasName("PK_AppSettings_1");

            entity.Property(e => e.AppSettingKey).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AppSettingType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DataJson).IsUnicode(false);
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.RoleId).HasMaxLength(450);

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DefaultLandingPage)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.PersonGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserRole>(entity =>
        {
            entity.HasKey(e => e.RoleMappingID).HasName("PK_AspNetUserRoles_1");

            entity.Property(e => e.RoleMappingID)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.RoleId).HasMaxLength(450);
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetUserRoles).HasForeignKey(d => d.RoleId);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserRoles).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Attachment_Db_Key);

            entity.Property(e => e.AttachmentGUID)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Attachment_FileName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Attachment_location)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Attachment_type)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.File_DVD_Num)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.File_Revision)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Orginal_File_Name)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Source_table)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");
        });

        modelBuilder.Entity<AuditLogDisplayManager>(entity =>
        {
            entity.HasKey(e => e.DisplayManagerKey);

            entity.ToTable("AuditLogDisplayManager");

            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ColumnName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DataType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Display_ColumnName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.SourceTable)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Audit_log>(entity =>
        {
            entity.HasKey(e => e.Log_Db_Key);

            entity.Property(e => e.Changes_On_type)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Event_Description).IsUnicode(false);
            entity.Property(e => e.Json_Data).IsUnicode(false);
            entity.Property(e => e.Previous_JsonData).IsUnicode(false);
            entity.Property(e => e.Remarks)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
            entity.Property(e => e.table_name)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.Activity_Db_keyNavigation).WithMany(p => p.Audit_logs)
                .HasForeignKey(d => d.Activity_Db_key)
                .HasConstraintName("FK_Audit_logs_Master_Activity");
        });

        modelBuilder.Entity<BATL_Build_assignment_Json>(entity =>
        {
            entity.ToTable("BATL_Build_assignment_Json");

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<BATL_CastingSerialMapping_Json>(entity =>
        {
            entity.ToTable("BATL_CastingSerialMapping_Json");

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<BATL_NCR_Json>(entity =>
        {
            entity.ToTable("BATL_NCR_Json");

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<BATL_RMC_Json>(entity =>
        {
            entity.ToTable("BATL_RMC_Json");

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<Base_Line_Engine>(entity =>
        {
            entity.HasKey(e => e.BL_Engine_Dbkey).HasName("PK_Engines");

            entity.ToTable(tb => tb.HasTrigger("A_IUD_AuditLog"));

            entity.Property(e => e.Engine_Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Engine_Title)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Revision_date).HasColumnType("datetime");
            entity.Property(e => e.Revision_title)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.Updated_By_UserGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");
        });

        modelBuilder.Entity<Base_Line_Engines_Approver>(entity =>
        {
            entity.HasKey(e => e.BL_Approvers_Dbkey);

            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<CastingDetail>(entity =>
        {
            entity.HasKey(e => e.CastingDbkey);

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.DemandDesc).IsUnicode(false);
            entity.Property(e => e.DemandNumber)
                .HasMaxLength(750)
                .IsUnicode(false);
            entity.Property(e => e.MMGOrderNumber).IsUnicode(false);
            entity.Property(e => e.OrderDate).HasColumnType("date");
            entity.Property(e => e.OrderNumbers).IsUnicode(false);
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OrderType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.castingGUID)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CastingItem>(entity =>
        {
            entity.HasKey(e => e.CastingItemKey);

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.DeliveryDate).HasColumnType("date");
            entity.Property(e => e.GTREDrgNo)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.ItemDescription).IsUnicode(false);
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PartName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<CastingReceiptQtySplit>(entity =>
        {
            entity.HasKey(e => e.QtySplitKey);

            entity.ToTable("CastingReceiptQtySplit");

            entity.Property(e => e.Remarks)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.SerialNos).IsUnicode(false);
            entity.Property(e => e.StatusRemarks)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<CastingReceiptsComment>(entity =>
        {
            entity.HasKey(e => e.CastingReceiptsCommentsKey);

            entity.Property(e => e.Comments).IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.UserGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CastingReceiptsItemSplit>(entity =>
        {
            entity.ToTable("CastingReceiptsItemSplit");

            entity.Property(e => e.Attachments).IsUnicode(false);
            entity.Property(e => e.BatchNumber).IsUnicode(false);
            entity.Property(e => e.HeatNumber).IsUnicode(false);
            entity.Property(e => e.ReceiptDate).HasColumnType("date");
            entity.Property(e => e.ReceiptGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReceiptNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SerialNumber).IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.VendorDrawingNo)
                .HasMaxLength(80)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Casting_DepartmentOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_CastingReceiptRemark_DepartmentOrder");

            entity.ToTable("Casting_DepartmentOrder");
        });

        modelBuilder.Entity<Casting_MaterialIssue>(entity =>
        {
            entity.HasKey(e => e.IssueDbKey);

            entity.ToTable("Casting_MaterialIssue");

            entity.Property(e => e.IssueDate).HasColumnType("date");
            entity.Property(e => e.Issue_type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Reference_No)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Reference No");
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<Casting_MaterialIssue_Item>(entity =>
        {
            entity.HasKey(e => e.IssueItemKey);

            entity.Property(e => e.ForEngine)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.IssueSlNos).IsUnicode(false);
            entity.Property(e => e.JCFileLocation).IsUnicode(false);
            entity.Property(e => e.JCFileName).IsUnicode(false);
            entity.Property(e => e.JobCardNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<Custom_Css_Style>(entity =>
        {
            entity.Property(e => e.Page)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Style_Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<DataEntry_Tracking_Config>(entity =>
        {
            entity.HasKey(e => e.ConfigId);

            entity.ToTable("DataEntry_Tracking_Config");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DisplayColumns).IsUnicode(false);
            entity.Property(e => e.ExclusionCondition).IsUnicode(false);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.ModuleName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PrimaryKeyColumn)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SourceTable)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.UpdateFrequencyDays).HasDefaultValueSql("((7))");
            entity.Property(e => e.UpdatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedOnColumn)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DataEntry_Tracking_History>(entity =>
        {
            entity.HasKey(e => e.HistoryId);

            entity.ToTable("DataEntry_Tracking_History");

            entity.HasIndex(e => e.RemarkId, "IX_DataEntry_History_RemarkId");

            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ActionOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NewValue).IsUnicode(false);
            entity.Property(e => e.OldValue).IsUnicode(false);

            entity.HasOne(d => d.Remark).WithMany(p => p.DataEntry_Tracking_Histories)
                .HasForeignKey(d => d.RemarkId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DataEntry_History_Remarks");
        });

        modelBuilder.Entity<DataEntry_Tracking_Remark>(entity =>
        {
            entity.HasKey(e => e.RemarkId);

            entity.HasIndex(e => new { e.ConfigId, e.SourceRecordKey }, "IX_DataEntry_Remarks_Config_Record");

            entity.Property(e => e.RemarkOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RemarkText).IsUnicode(false);

            entity.HasOne(d => d.Config).WithMany(p => p.DataEntry_Tracking_Remarks)
                .HasForeignKey(d => d.ConfigId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DataEntry_Remarks_Config");
        });

        modelBuilder.Entity<Demand_Document>(entity =>
        {
            entity.HasKey(e => e.DocumentID);

            entity.ToTable("Demand_Document");

            entity.Property(e => e.Document_Location).IsUnicode(false);
            entity.Property(e => e.Document_Name)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Remarks)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<Demand_Verification>(entity =>
        {
            entity.HasKey(e => e.Verification_Id);

            entity.ToTable("Demand_Verification");

            entity.Property(e => e.Verification_Id).ValueGeneratedNever();
            entity.Property(e => e.Demand_Desc).IsUnicode(false);
            entity.Property(e => e.Demand_No)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Demanding_Officer)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Project).IsUnicode(false);
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.Verified_By)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Documentation>(entity =>
        {
            entity.HasKey(e => e.Document_Dbkey);

            entity.ToTable("Documentation", tb => tb.HasTrigger("IUD_Documents"));

            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.File_Location).IsUnicode(false);
            entity.Property(e => e.File_Name)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.File_Size)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.File_type)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Item_type)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.Note).IsUnicode(false);
            entity.Property(e => e.Refrence_Title)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.SearchTags).IsUnicode(false);
            entity.Property(e => e.System_File_Name)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
            entity.Property(e => e.VectorExecutionEndTime).HasColumnType("datetime");
            entity.Property(e => e.VectorExecutionStartTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<Documents_Access_Config>(entity =>
        {
            entity.HasKey(e => e.doc_config_dbkey);

            entity.ToTable("Documents_Access_Config");

            entity.HasIndex(e => new { e.UserDbkey, e.Document_Dbkey }, "UQ_UserId_Document_Dbkey").IsUnique();

            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<Drawing_Bubble_Inspection>(entity =>
        {
            entity.HasKey(e => e.Inspection_Dbkey);

            entity.ToTable("Drawing_Bubble_Inspection");

            entity.Property(e => e.Annotated_File_Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Annotated_File_Path)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Detection_Method).HasMaxLength(50);
            entity.Property(e => e.Draw_part_no)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Is_Active)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Original_File_Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Original_File_Path)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Processed_On)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("('Completed')");
        });

        modelBuilder.Entity<Drawing_Bubble_Inspection_Item>(entity =>
        {
            entity.HasKey(e => e.Item_Dbkey);

            entity.Property(e => e.Bubble_Number)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Confidence).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Dimension)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Is_Active)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Tolerance)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<EBC_SerialNoLog>(entity =>
        {
            entity.HasKey(e => e.LogID);

            entity.Property(e => e.Previous_SerialNo).IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.Updated_serialNo).IsUnicode(false);
        });

        modelBuilder.Entity<Engine>(entity =>
        {
            entity.HasKey(e => e.Engine_Dbkey).HasName("PK_Engines_sub");

            entity.ToTable(tb => tb.HasTrigger("A_IUD_Engines_Audit_Logs"));

            entity.Property(e => e.Attachment_Name)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Attachment_location)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Engine_Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Engine_Name_varient)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Engine_Number)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Priority)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Unique_Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");

            entity.HasOne(d => d.BL_Engine_DbkeyNavigation).WithMany(p => p.Engines)
                .HasForeignKey(d => d.BL_Engine_Dbkey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Engines_Base_Line_Engines");
        });

        modelBuilder.Entity<EngineBuild>(entity =>
        {
            entity.HasIndex(e => e.BuildGuid, "IX_EngineBuilds_BuildGuid");

            entity.Property(e => e.BuildDate).HasColumnType("date");
            entity.Property(e => e.BuildGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BuildName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ClonedFrom)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.ReferenceNumber)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<EngineBuildComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SOPEnginePartComponents");

            entity.ToTable(tb => tb.HasTrigger("A_IUD_SOPComp_Audit_Logs"));

            entity.HasIndex(e => e.BuildDbkey, "IX_EngineBuildComponents_BuildDbkey");

            entity.HasIndex(e => e.DrawingNumber, "IX_EngineBuildComponents_DrawingNumber");

            entity.HasIndex(e => new { e.DrawingNumber, e.BuildDbkey }, "IX_EngineBuildComponents_DrawingNumber_BuildDbkey");

            entity.Property(e => e.AssemblyReportingType)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ContractNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.DrawingNumber)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.JobCard)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.ReportingType)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SchemeNumber)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.SerialNumber).IsUnicode(false);
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<Engine_Parts_Master>(entity =>
        {
            entity.HasKey(e => e.Engine_Part_Dbkey).HasName("PK_Engine_Parts_Master_1");

            entity.ToTable("Engine_Parts_Master", tb => tb.HasTrigger("A_IU_MPL_Audit_Log"));

            entity.Property(e => e.AssemblyReportingType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Comments).IsUnicode(false);
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Draw_part_no)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Drawing_File)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Drawing_File_location)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Drawing_no)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Execution_Resp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Execution_Resp_additionalLevel)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FCBP)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Record_Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Reporting_Type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Revision)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Solid_Model)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Solid_Model_location)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Solid_model_no)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
            entity.Property(e => e.rm_updated_on).HasColumnType("datetime");
        });

        modelBuilder.Entity<Engine_Parts_Master_Log>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_Engine_Parts_Master_Logs1");

            entity.Property(e => e.AssemblyReportingType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Comments).IsUnicode(false);
            entity.Property(e => e.Created_on).HasColumnType("datetime");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Draw_part_no)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Drawing_File)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Drawing_File_location)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Drawing_no)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Execution_Resp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Execution_Resp_additionalLevel)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FCBP)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Record_Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Reporting_Type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Revision)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Solid_Model)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Solid_Model_location)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Solid_model_no)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
            entity.Property(e => e.rm_updated_on).HasColumnType("datetime");
        });

        modelBuilder.Entity<Engine_Parts_Revisoin_History>(entity =>
        {
            entity.HasKey(e => e.Rev_History_Dbkey);

            entity.ToTable("Engine_Parts_Revisoin_History");

            entity.Property(e => e.Drawing_File)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Drawing_File_location)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Revision_Date).HasColumnType("date");
            entity.Property(e => e.Revision_Notes)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Solid_Model_File)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Solid_Model_File_location)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");
            entity.Property(e => e.is_latest)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Engine_Parts_Usage>(entity =>
        {
            entity.HasKey(e => e.Part_relation_dbkey);

            entity.ToTable("Engine_Parts_Usage", tb => tb.HasTrigger("After_update_EngineParts"));

            entity.Property(e => e.Collaborators).IsUnicode(false);
            entity.Property(e => e.CollaboratorsId).IsUnicode(false);
            entity.Property(e => e.Comments).IsUnicode(false);
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Execution_Resp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Execution_Resp_additionalLevel)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ManufacturingComments)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Module_PBS)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.PartType_PBS)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Part_Remarks).IsUnicode(false);
            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");

            entity.HasOne(d => d.BL_Engine_DbkeyNavigation).WithMany(p => p.Engine_Parts_Usages)
                .HasForeignKey(d => d.BL_Engine_Dbkey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Engine_Parts_Usage_Base_Line_Engines");

            entity.HasOne(d => d.Engine_Part_DbkeyNavigation).WithMany(p => p.Engine_Parts_Usages)
                .HasForeignKey(d => d.Engine_Part_Dbkey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Engine_Parts_Usage_Engine_Parts_Master");
        });

        modelBuilder.Entity<ExceptionLog>(entity =>
        {
            entity.ToTable("ExceptionLog");

            entity.Property(e => e.DateErrorRaised).HasColumnType("datetime");
        });

        modelBuilder.Entity<ExternalMfgStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_VendorComponentDetail");

            entity.ToTable("ExternalMfgStatus");

            entity.Property(e => e.Json).IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<Forging_Receipt>(entity =>
        {
            entity.HasKey(e => e.forging_recp_dbkey);

            entity.Property(e => e.MMG_File_No)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Receipt_Date).HasColumnType("date");
            entity.Property(e => e.Receipt_Number)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<Forging_Receipt_Item>(entity =>
        {
            entity.HasKey(e => e.forging_item_dbkey);

            entity.Property(e => e.GTRE_Drawing_No)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.HAL_Drawing_No)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");

            entity.HasOne(d => d.forging_recp_dbkeyNavigation).WithMany(p => p.Forging_Receipt_Items)
                .HasForeignKey(d => d.forging_recp_dbkey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Forging_Receipt_Items_Forging_Receipts");
        });

        modelBuilder.Entity<Forging_Split>(entity =>
        {
            entity.HasKey(e => e.forging_item_split_dbkey);

            entity.Property(e => e.Attachment_Db_Key).IsUnicode(false);
            entity.Property(e => e.Batch_Number)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.GTRE_Drawing_No)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Heat_Number)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Sl_No_Forging).IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
            entity.Property(e => e.part_name)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.HasOne(d => d.forging_item_dbkeyNavigation).WithMany(p => p.Forging_Splits)
                .HasForeignKey(d => d.forging_item_dbkey)
                .HasConstraintName("FK_Forging_Splits_Forging_Receipt_Items");
        });

        modelBuilder.Entity<GanttTask>(entity =>
        {
            entity.HasKey(e => e.pID);

            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.category)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.pCaption)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.pClass)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.pDepend)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.pEnd).HasColumnType("date");
            entity.Property(e => e.pLink)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.pName)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.pNotes).IsUnicode(false);
            entity.Property(e => e.pPlanEnd).HasColumnType("date");
            entity.Property(e => e.pPlanStart).HasColumnType("date");
            entity.Property(e => e.pRes)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.pStart).HasColumnType("date");
            entity.Property(e => e.sector)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ION_Destination>(entity =>
        {
            entity.HasKey(e => e.DestinationId).HasName("PK__ION_Dest__DB5FE4CCA4173718");

            entity.HasIndex(e => e.DestinationGUID, "UQ__ION_Dest__1660B40FAB65E8CB").IsUnique();

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DestinationCode).HasMaxLength(10);
            entity.Property(e => e.DestinationGUID)
                .HasMaxLength(36)
                .IsUnicode(false);
            entity.Property(e => e.DestinationName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");
        });

        modelBuilder.Entity<ION_Enclosure>(entity =>
        {
            entity.HasKey(e => e.EnclosureId).HasName("PK__ION_Encl__4A63C54C62A85E60");

            entity.HasIndex(e => e.EnclosureGUID, "UQ__ION_Encl__9AC851472FC1D088").IsUnique();

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.EnclosureDescription).HasMaxLength(500);
            entity.Property(e => e.EnclosureGUID)
                .HasMaxLength(36)
                .IsUnicode(false);
            entity.Property(e => e.IONGUID)
                .HasMaxLength(36)
                .IsUnicode(false);
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");
            entity.Property(e => e.SortOrder).HasDefaultValueSql("((1))");
        });

        modelBuilder.Entity<ION_FileGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId);

            entity.HasIndex(e => e.GroupGUID, "UQ_ION_FileGroups_GUID").IsUnique();

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.GroupGUID)
                .HasMaxLength(36)
                .IsUnicode(false);
            entity.Property(e => e.GroupName).HasMaxLength(200);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.ReferenceNo).HasMaxLength(100);
        });

        modelBuilder.Entity<ION_InwardNote>(entity =>
        {
            entity.HasKey(e => e.InwardNoteId);

            entity.ToTable("ION_InwardNote");

            entity.HasIndex(e => e.CreatedBy, "IX_ION_InwardNote_CreatedBy");

            entity.HasIndex(e => e.ReceivedDate, "IX_ION_InwardNote_ReceivedDate").IsDescending();

            entity.HasIndex(e => e.InwardIONGUID, "UQ_ION_InwardNote_GUID").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FromDepartment).HasMaxLength(500);
            entity.Property(e => e.FromPersonNameWithDesignation).HasMaxLength(500);
            entity.Property(e => e.IONDate).HasColumnType("date");
            entity.Property(e => e.IONReferenceNumber).HasMaxLength(200);
            entity.Property(e => e.InwardIONGUID).HasMaxLength(50);
            entity.Property(e => e.ReceivedDate).HasColumnType("date");
            entity.Property(e => e.Subject).HasMaxLength(1000);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<ION_Note>(entity =>
        {
            entity.HasKey(e => e.IONId).HasName("PK__ION_Note__215CD3A41F147AA9");

            entity.HasIndex(e => e.IONGUID, "UQ__ION_Note__0E2B9CB746AAEEBB").IsUnique();

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.GroupGUID)
                .HasMaxLength(36)
                .IsUnicode(false);
            entity.Property(e => e.IONDate).HasColumnType("date");
            entity.Property(e => e.IONGUID)
                .HasMaxLength(36)
                .IsUnicode(false);
            entity.Property(e => e.IONNumber).HasMaxLength(50);
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");
            entity.Property(e => e.Office).HasMaxLength(100);
            entity.Property(e => e.PreparedByDesignation).HasMaxLength(200);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.ScannedCopyPath).HasMaxLength(500);
            entity.Property(e => e.ScannedCopyUploaded).HasDefaultValueSql("((0))");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("('Draft')");
            entity.Property(e => e.Subject).HasMaxLength(500);
        });

        modelBuilder.Entity<ION_NoteRecipient>(entity =>
        {
            entity.HasIndex(e => e.IONGUID, "IX_ION_NoteRecipients_IONGUID");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.GroupGUID)
                .HasMaxLength(36)
                .IsUnicode(false);
            entity.Property(e => e.IONGUID)
                .HasMaxLength(36)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ION_OfficeConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigId).HasName("PK__ION_Offi__C3BC335C98684D54");

            entity.ToTable("ION_OfficeConfig");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.Office).HasMaxLength(100);
            entity.Property(e => e.RefNoPrefix).HasMaxLength(20);
        });

        modelBuilder.Entity<ION_SerialTracking>(entity =>
        {
            entity.HasKey(e => e.TrackingId).HasName("PK__ION_Seri__3C19EDF1A9EF1FCC");

            entity.ToTable("ION_SerialTracking");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.GroupGUID)
                .HasMaxLength(36)
                .IsUnicode(false);
            entity.Property(e => e.LastSerialNumber).HasDefaultValueSql("((0))");
        });

        modelBuilder.Entity<ION_Template>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__ION_Temp__F87ADD27F613F4F8");

            entity.ToTable("ION_Template");

            entity.HasIndex(e => e.TemplateGUID, "UQ_ION_Template_GUID").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.GroupGUID)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.SubjectTemplate).HasMaxLength(500);
            entity.Property(e => e.TemplateGUID)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TemplateName).HasMaxLength(200);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<InspectionReport>(entity =>
        {
            entity.HasKey(e => e.Inspect_Rpt_key);

            entity.ToTable("InspectionReport");

            entity.Property(e => e.BuildNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Drawing_No)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.File_Location).IsUnicode(false);
            entity.Property(e => e.File_Name).IsUnicode(false);
            entity.Property(e => e.Job_No).IsUnicode(false);
            entity.Property(e => e.Part_relation_dbkey)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Quantity)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.RMC_Number).IsUnicode(false);
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Serial_No).IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<InspectionReportRecord>(entity =>
        {
            entity.HasKey(e => e.Inspect_File_DBkey).HasName("PK_Inspection_Report_Record");

            entity.ToTable("InspectionReportRecord");

            entity.Property(e => e.File_Location).IsUnicode(false);
            entity.Property(e => e.File_Name)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.File_SystemName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.File_UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.File_Updatedby)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VendorCode)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Issue>(entity =>
        {
            entity.HasKey(e => e.SlNo).HasName("PK__Issues__6C861604C9B75905");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Developer).HasMaxLength(100);
            entity.Property(e => e.IssueType).HasMaxLength(50);
            entity.Property(e => e.Priority).HasMaxLength(50);
            entity.Property(e => e.Reporter).HasMaxLength(100);
            entity.Property(e => e.Section).HasMaxLength(50);
            entity.Property(e => e.Solution_Description).HasMaxLength(200);
            entity.Property(e => e.Solution_Placeholder).HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<IssueComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__IssueCom__C3B4DFCA3B130041");

            entity.Property(e => e.CommentedBy)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<IssueHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__IssueHis__4D7B4ABD24E80351");

            entity.ToTable("IssueHistory");

            entity.Property(e => e.ChangedBy).HasMaxLength(100);
            entity.Property(e => e.ChangedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NewStatus).HasMaxLength(50);
            entity.Property(e => e.OldStatus).HasMaxLength(50);
        });

        modelBuilder.Entity<Job_Card>(entity =>
        {
            entity.HasKey(e => e.JobCard_Dbkey).HasName("PK__Job_Card__9DBC2487E8FAC001");

            entity.ToTable("Job_Card");

            entity.Property(e => e.Drawing_No).HasMaxLength(100);
            entity.Property(e => e.Engine).HasMaxLength(100);
            entity.Property(e => e.Issue_No).HasMaxLength(50);
            entity.Property(e => e.JC_Closed_On).HasColumnType("date");
            entity.Property(e => e.JC_Opened_On).HasColumnType("date");
            entity.Property(e => e.JobCard_Date).HasColumnType("date");
            entity.Property(e => e.JobCard_No).HasMaxLength(50);
            entity.Property(e => e.Module).HasMaxLength(100);
            entity.Property(e => e.Nomenclature).HasMaxLength(200);
            entity.Property(e => e.Request_No).HasMaxLength(100);
            entity.Property(e => e.Tech_Development_Type).HasMaxLength(200);
            entity.Property(e => e.Updated_On)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<MPLReportView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("MPLReportView");

            entity.Property(e => e.AssemblyReportingType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Collaborators).IsUnicode(false);
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Draw_part_no)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Execution_Resp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsParent)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.ManufacturingComments)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Material_name)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ParentDescription).IsUnicode(false);
            entity.Property(e => e.ParentMaterial_name)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ParentModuleRes)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ParentName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ParentReporting_type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ParentRevision)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PartModuleRes)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Part_Remarks).IsUnicode(false);
            entity.Property(e => e.Reporting_Type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Type_Part_Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Mail_Credential>(entity =>
        {
            entity.HasKey(e => e.Sl_No).HasName("PK_MailCredentials");

            entity.ToTable(tb => tb.HasTrigger("A_IUD_EmailConfig_AuditLog"));

            entity.Property(e => e.MailID)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SMTP_HostName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SSL)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<Mail_Template>(entity =>
        {
            entity.HasKey(e => e.Mail_Temp_ID).HasName("PK_Email_Templates");

            entity.Property(e => e.BlindCopy)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.CopyTo)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.EmailTriggerDays)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Mail_Subject)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Mail_Temp_Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Parameters)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Recipients)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Source_table_name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<Mailer_Log>(entity =>
        {
            entity.ToTable("Mailer_Log");

            entity.Property(e => e.Body).IsUnicode(false);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.MailFrom)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MailTo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MailType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Subject)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Manufacturing_Process_Documents_Required>(entity =>
        {
            entity.ToTable("Manufacturing_Process_Documents_Required");

            entity.Property(e => e.Attachment_Type)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Master_Activity>(entity =>
        {
            entity.HasKey(e => e.Activity_Db_key);

            entity.ToTable("Master_Activity");

            entity.Property(e => e.Activity_Name)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");
        });

        modelBuilder.Entity<Master_General>(entity =>
        {
            entity.HasKey(e => e.Master_Dbkey);

            entity.ToTable("Master_General", tb => tb.HasTrigger("A_IUD_Master_Gen_Audit_Logs"));

            entity.Property(e => e.Master_Name)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Master_Type)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Misc)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");
        });

        modelBuilder.Entity<Master_Part_Type>(entity =>
        {
            entity.HasKey(e => e.Type_Dbkey);

            entity.Property(e => e.Type_Part_Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<Master_Rawmaterial>(entity =>
        {
            entity.HasKey(e => e.Raw_material_Dbkey);

            entity.Property(e => e.Dia_mm)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Material_name)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Raw_material_Name).IsUnicode(false);
            entity.Property(e => e.RawmaterialGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Remarks)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Thick_mm)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");
            entity.Property(e => e.height)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.inner_Dia_mm)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MaterialIssueDocument>(entity =>
        {
            entity.HasKey(e => e.MaterialIssueDocumentKey);

            entity.Property(e => e.FileLocation)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.FileName)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.uploadedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<MaterialIssue_DR_SplitMapping>(entity =>
        {
            entity.HasKey(e => e.split_issue_id);

            entity.ToTable("MaterialIssue_DR_SplitMapping");

            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<Material_IssueItems_Consolidation>(entity =>
        {
            entity.HasKey(e => e.split_issue_id);

            entity.ToTable("Material_IssueItems_Consolidation");

            entity.Property(e => e.Updated_on).HasColumnType("datetime");

            entity.HasOne(d => d.Consolidated_dbkeyNavigation).WithMany(p => p.Material_IssueItems_Consolidations)
                .HasForeignKey(d => d.Consolidated_dbkey)
                .HasConstraintName("FK_Material_IssueItems_Consolidation_Rawmaterial_Consolidation");
        });

        modelBuilder.Entity<Material_Issue_Item>(entity =>
        {
            entity.HasKey(e => e.Issue_Item_Dbkey);

            entity.Property(e => e.Denom)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(450)
                .IsUnicode(false);
            entity.Property(e => e.Drawing_no).IsUnicode(false);
            entity.Property(e => e.EngineLevel)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Heat_No)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.JCFileLocation).IsUnicode(false);
            entity.Property(e => e.JCFileName).IsUnicode(false);
            entity.Property(e => e.JobCardNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SerialNo)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Size)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
            entity.Property(e => e.height)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.outer_dia)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.thickness)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Issue_DbkeyNavigation).WithMany(p => p.Material_Issue_Items)
                .HasForeignKey(d => d.Issue_Dbkey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Material_Issue_Items_Material_Issue_Note");

            entity.HasOne(d => d.Raw_material_DbkeyNavigation).WithMany(p => p.Material_Issue_Items)
                .HasForeignKey(d => d.Raw_material_Dbkey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Material_Issue_Items_Master_Rawmaterials");
        });

        modelBuilder.Entity<Material_Issue_Items_Part>(entity =>
        {
            entity.HasKey(e => e.Material_Issue_Items_Parts_Dbkey);

            entity.Property(e => e.Part_Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Material_Issue_Note>(entity =>
        {
            entity.HasKey(e => e.Issue_Dbkey);

            entity.ToTable("Material_Issue_Note");

            entity.Property(e => e.Attachment_Db_Key)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Demand_No)
                .HasMaxLength(350)
                .IsUnicode(false);
            entity.Property(e => e.Engine_Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Form_Number)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.JobCardFileLocation)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.JobCardFileName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Job_Card)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Order_Ref_Date).HasColumnType("date");
            entity.Property(e => e.Order_Ref_No)
                .HasMaxLength(350)
                .IsUnicode(false);
            entity.Property(e => e.PMO_Ref_Date).HasColumnType("date");
            entity.Property(e => e.PMO_Ref_No)
                .HasMaxLength(350)
                .IsUnicode(false);
            entity.Property(e => e.Ref_Number)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Returnable)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<MetaMaster>(entity =>
        {
            entity.ToTable("MetaMaster");

            entity.Property(e => e.DisplayText)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.MasterGUID)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MasterType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ParentGUID)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<NCR_Item_Rework>(entity =>
        {
            entity.HasKey(e => e.ReworkID).HasName("PK__NCR_Item__CF7C815A17226C5F");

            entity.ToTable("NCR_Item_Rework");

            entity.Property(e => e.ClearedOn).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.IsCleared).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsRework).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsTrialAssembly).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsUnmarked).HasDefaultValueSql("((0))");
            entity.Property(e => e.MarkedOn).HasColumnType("datetime");
            entity.Property(e => e.NCRWorkFlowGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UnmarkedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<NCR_Item_Workflow_Tracking>(entity =>
        {
            entity.HasKey(e => e.TrackingID).HasName("PK__NCR_Item__3C19EDD16FB4CC9C");

            entity.ToTable("NCR_Item_Workflow_Tracking");

            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.NCRWorkFlowGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<NCR_ModuleToUserMapping>(entity =>
        {
            entity.ToTable("NCR_ModuleToUserMapping");

            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.UserGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<NCR_SerialNumber_Engine_Mapping>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NCR_Seri__3214EC077AF0E4EF");

            entity.ToTable("NCR_SerialNumber_Engine_Mapping");

            entity.Property(e => e.Engine).HasMaxLength(200);
            entity.Property(e => e.NCR_GUId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SerialNumber).HasMaxLength(200);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<NCR_Workflow_Assignment>(entity =>
        {
            entity.HasKey(e => e.NCRWorkflowID).HasName("PK_NCR_Workflow");

            entity.Property(e => e.AssignedOn).HasColumnType("datetime");
            entity.Property(e => e.AssigneeUserGUIDs).IsUnicode(false);
            entity.Property(e => e.NCRGUID)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NCRWorkflowGUID)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.WorkUpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<NCR_Workflow_Assignment_Module>(entity =>
        {
            entity.HasKey(e => e.AssignmentModuleID).HasName("PK__NCR_Work__ED0D6487EA52FF13");

            entity.Property(e => e.NCRWorkflowGUID)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<NCR_Workflow_Assignment_User>(entity =>
        {
            entity.HasKey(e => e.AssignmentUserID).HasName("PK__NCR_Work__B7532ACB9C00C2B4");

            entity.Property(e => e.NCRWorkflowGUID)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserGUID)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<NCR_Workflow_Assignments_Log>(entity =>
        {
            entity.HasKey(e => e.NCRWorkflowLogsID).HasName("PK_NCR_WorkflowLogs");

            entity.Property(e => e.NCRGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NCRWorkflowGUID)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Status_Verbose).IsUnicode(false);
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<NCR_Workflow_Assignments_New>(entity =>
        {
            entity.HasKey(e => e.NCRWorkflowGUID).HasName("PK__NCR_Work__94743643E8415ABD");

            entity.ToTable("NCR_Workflow_Assignments_New");

            entity.Property(e => e.NCRWorkflowGUID)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.AssignedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.NCRGUID)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.WorkUpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<NCR_Workflow_Stage>(entity =>
        {
            entity.HasKey(e => e.StageID).HasName("PK__NCR_Work__03EB7AF827BE9A80");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.SkipForTrialAssembly).HasDefaultValueSql("((0))");
            entity.Property(e => e.StageDescription).IsUnicode(false);
            entity.Property(e => e.StageName).HasMaxLength(50);
        });

        modelBuilder.Entity<NLPDatabaseDef>(entity =>
        {
            entity.HasKey(e => e.tableKey);

            entity.ToTable("NLPDatabaseDef");

            entity.Property(e => e.DomainModel)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.tableName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<NLPDomainModelDef>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_NLPDatabaseModelDef");

            entity.ToTable("NLPDomainModelDef");

            entity.Property(e => e.DomainModel)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModelDescription).IsUnicode(false);
            entity.Property(e => e.SampleQuries).IsUnicode(false);
        });

        modelBuilder.Entity<NLtoSql_Log>(entity =>
        {
            entity.ToTable("NLtoSql_Log");

            entity.Property(e => e.ExecutionTime)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Prompt).IsUnicode(false);
            entity.Property(e => e.Question).IsUnicode(false);
            entity.Property(e => e.RequestType).IsUnicode(false);
            entity.Property(e => e.SqlQuery).IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<NavigationMenuControl>(entity =>
        {
            entity.HasKey(e => e.MenuItemKey);

            entity.ToTable("NavigationMenuControl");

            entity.Property(e => e.ClaimRequirement)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.DisplayName)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.LandingPage)
                .HasMaxLength(250)
                .IsUnicode(false);
        });

        modelBuilder.Entity<NonConformanceReport>(entity =>
        {
            entity.ToTable("NonConformanceReport", tb => tb.HasTrigger("NonConformanceReportLog"));

            entity.Property(e => e.AssignedUserGuid).IsUnicode(false);
            entity.Property(e => e.Chair)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ComitteeReferred)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DARno)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ECM_No)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ECM_TR_NO)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FileLocation)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Inspection_Report_No)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.JobCard)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Module)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModuleAssignedOn).HasColumnType("datetime");
            entity.Property(e => e.NCRGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OrignalFileName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ReceivedDate).HasColumnType("date");
            entity.Property(e => e.ReferenceNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.ReportStatus)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SerialNumber).IsUnicode(false);
            entity.Property(e => e.Stage_Final)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Stress)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SystemFileName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Tas)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.UseNewWorkflow).HasDefaultValueSql("((0))");
        });

        modelBuilder.Entity<NonConformanceReport_Item>(entity =>
        {
            entity.HasKey(e => e.NCRItemKey);

            entity.Property(e => e.CHAIR_Rework_Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CHAIR_Rework_UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.Chair_Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Chair_UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Deviation_Reason_Analysis).IsUnicode(false);
            entity.Property(e => e.DrgZone)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Engine).IsUnicode(false);
            entity.Property(e => e.Module_Rework_Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Module_Rework_UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.Module_Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Module_UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.NCRGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NCRWorkFlowGuid)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.STFE_PO_Remark_UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.STRESS_Rework_Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.STRESS_Rework_UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.SerialNumber).IsUnicode(false);
            entity.Property(e => e.Serial_No_in_Inspection_Rep)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.SlNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Stress_Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Stress_UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.TAS_Rework_Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TAS_Rework_UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.TAS_Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TAS_UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<NonConformanceReport_Item_Rework>(entity =>
        {
            entity.HasKey(e => e.RecordID).HasName("PK__NonConfo__FBDF78C93CB78F11");

            entity.ToTable("NonConformanceReport_Item_Rework");

            entity.HasIndex(e => new { e.NCRItemKey, e.StageName, e.IsActive }, "IX_NCRItemStageRework_ItemStage");

            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.MarkedOn).HasColumnType("datetime");
            entity.Property(e => e.StageName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<NonConformanceReport_log>(entity =>
        {
            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Chair)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ComitteeReferred)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FileLocation)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Module)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NCR_Guid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OrignalFileName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ReceivedDate).HasColumnType("date");
            entity.Property(e => e.ReferenceNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.ReportStatus)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SerialNumber).IsUnicode(false);
            entity.Property(e => e.Stress)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SystemFileName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Tas)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<OTPVerification>(entity =>
        {
            entity.ToTable("OTPVerification");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.OTP)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Validity).HasComputedColumnSql("(case when dateadd(minute,(10),[CreatedOn])>getdate() then (1) else (0) end)", false);
        });

        modelBuilder.Entity<Order_ModuleUserMapping>(entity =>
        {
            entity.ToTable("Order_ModuleUserMapping");

            entity.Property(e => e.OrderType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdateOn).HasColumnType("datetime");
            entity.Property(e => e.UserGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PartManufactureStatus>(entity =>
        {
            entity.ToTable("PartManufactureStatus");

            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<PerformancePrediction>(entity =>
        {
            entity.HasKey(e => e.predictionKey);

            entity.ToTable("PerformancePrediction");

            entity.Property(e => e.Title)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.createdBy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.createdon).HasColumnType("datetime");
            entity.Property(e => e.inputFilename)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.predectionGUID)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Person_Dbkey);

            entity.Property(e => e.Person_Name)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");
        });

        modelBuilder.Entity<ProcurementMilestone>(entity =>
        {
            entity.HasKey(e => e.MilestoneID);

            entity.Property(e => e.Comments).IsUnicode(false);
            entity.Property(e => e.CompletionDate).HasColumnType("date");
            entity.Property(e => e.Components).IsUnicode(false);
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.DueDate).HasColumnType("date");
            entity.Property(e => e.IsLastMilestone).HasDefaultValueSql("((0))");
            entity.Property(e => e.MilestoneName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<Procurement_Demand>(entity =>
        {
            entity.HasKey(e => e.DemandDbKey);

            entity.Property(e => e.ActualCost).HasColumnType("numeric(28, 2)");
            entity.Property(e => e.AdvancePaid).HasColumnType("numeric(28, 2)");
            entity.Property(e => e.CurrentStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Demand_No)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EstimatedCost).HasColumnType("numeric(28, 2)");
            entity.Property(e => e.EstimatedOrderDate).HasColumnType("datetime");
            entity.Property(e => e.Item_Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Item_Type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MMG_File_No)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OrderNumbers)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.OrderType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PaymentMadeTillDate).HasColumnType("numeric(28, 2)");
            entity.Property(e => e.Planned_Date_of_receipt).HasColumnType("datetime");
            entity.Property(e => e.Project_Head)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Quantity).HasColumnType("numeric(18, 2)");
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.ShortCloseReason)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ShortClosedOn).HasColumnType("datetime");
            entity.Property(e => e.StatusDate).HasColumnType("date");
            entity.Property(e => e.TenderMode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UOM)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<Procurement_Demand_Item>(entity =>
        {
            entity.HasKey(e => e.DemandItemKey);

            entity.Property(e => e.Inner_Dia_mm)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.Item_Code)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Item_Sub_Type)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.LineItem)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.MMGOrderNumber)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Outer_Dia_mm)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.Thickness)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.UOM)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
            entity.Property(e => e.height)
                .HasMaxLength(70)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Procurement_Demand_Item_Adjustment>(entity =>
        {
            entity.HasKey(e => e.Adjustment_Dbkey).HasName("PK__Procurem__1586391CE3C32B9D");

            entity.Property(e => e.Adjusted_On)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Adjustment_Remarks).HasMaxLength(500);
        });

        modelBuilder.Entity<Procurement_Demand_MileStone>(entity =>
        {
            entity.HasKey(e => e.MilestoneDbKey).HasName("PK_Procurment_Demand_MileStone");

            entity.ToTable("Procurement_Demand_MileStone");

            entity.Property(e => e.DeliveryDate).HasColumnType("date");
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<Procurement_Demand_Receipt>(entity =>
        {
            entity.HasKey(e => e.Receipt_dbkey);

            entity.Property(e => e.Created_On).HasColumnType("datetime");
            entity.Property(e => e.Receipt_Date).HasColumnType("date");
            entity.Property(e => e.Receipt_No)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");
            entity.Property(e => e.breadth).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.length).HasColumnType("numeric(18, 0)");

            entity.HasOne(d => d.DemandDbKeyNavigation).WithMany(p => p.Procurement_Demand_Receipts)
                .HasForeignKey(d => d.DemandDbKey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Procurement_Demand_Receipts_Procurement_Demands");

            entity.HasOne(d => d.DemandItemKeyNavigation).WithMany(p => p.Procurement_Demand_Receipts)
                .HasForeignKey(d => d.DemandItemKey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Procurement_Demand_Receipts_Procurement_Demand_Items");
        });

        modelBuilder.Entity<Procurement_Demands_History>(entity =>
        {
            entity.HasKey(e => e.Demand_Procurement_History_Key);

            entity.ToTable("Procurement_Demands_History");

            entity.Property(e => e.ActionDate).HasColumnType("date");
            entity.Property(e => e.ActionStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<Procurement_Milestone>(entity =>
        {
            entity.HasKey(e => e.MilestoneID);

            entity.HasIndex(e => new { e.DemandDbKey, e.Status }, "IX_Procurement_Milestones_DemandStatus");

            entity.Property(e => e.Comments).IsUnicode(false);
            entity.Property(e => e.CompletionDate).HasColumnType("date");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.CurrentDueDate).HasColumnType("date");
            entity.Property(e => e.MilestoneName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.OriginalDueDate).HasColumnType("date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValueSql("('Active')");
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<Procurement_Milestone_Extension>(entity =>
        {
            entity.HasKey(e => e.ExtensionID);

            entity.HasIndex(e => e.MilestoneID, "IX_Milestone_Extensions_MilestoneID");

            entity.Property(e => e.ExtendedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExtensionType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValueSql("('Extension')");
            entity.Property(e => e.NewDueDate).HasColumnType("date");
            entity.Property(e => e.PreviousDueDate).HasColumnType("date");
            entity.Property(e => e.Reason).IsUnicode(false);
        });

        modelBuilder.Entity<Procurement_Milestone_Item>(entity =>
        {
            entity.HasKey(e => e.MilestoneItemID);

            entity.HasIndex(e => e.DemandItemDbKey, "IX_Milestone_Items_DemandItemDbKey");

            entity.HasIndex(e => e.MilestoneID, "IX_Milestone_Items_MilestoneID");

            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<Procurement_Milestone_Merge_Source>(entity =>
        {
            entity.HasKey(e => e.MergeSourceID);

            entity.HasIndex(e => e.ExtensionID, "IX_Merge_Sources_ExtensionID");

            entity.HasIndex(e => e.SourceMilestoneID, "IX_Merge_Sources_SourceMilestoneID");

            entity.Property(e => e.SourceDueDate).HasColumnType("date");
        });

        modelBuilder.Entity<Procurement_ReceiptItemSplit>(entity =>
        {
            entity.HasKey(e => e.SplitId);

            entity.ToTable("Procurement_ReceiptItemSplit");

            entity.Property(e => e.AdditionalinfoJson).IsUnicode(false);
            entity.Property(e => e.Attachment_Db_Key).IsUnicode(false);
            entity.Property(e => e.Batch_No)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Heat_No)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Material_Reference_No).IsUnicode(false);
            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UOM)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Project_Dbkey);

            entity.ToTable("Project");

            entity.Property(e => e.Attachment_Name)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Attachment_location)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.DOS).HasColumnType("date");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Display_title)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.EDO).HasColumnType("date");
            entity.Property(e => e.Project_Number)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Unique_Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");
        });

        modelBuilder.Entity<RM_Qty_Vendor_Split>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("RM_Qty_Vendor_Split");

            entity.Property(e => e.ID).ValueGeneratedOnAdd();
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<Rawmaterial_Consolidation>(entity =>
        {
            entity.HasKey(e => e.Consolidated_dbkey);

            entity.ToTable("Rawmaterial_Consolidation");

            entity.Property(e => e.Batch_No)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Heat_No)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Material_Reference_No)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.UOM)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");
        });

        modelBuilder.Entity<Rawmaterial_Inventory>(entity =>
        {
            entity.HasKey(e => e.RM_Inventory_Dbkey);

            entity.ToTable("Rawmaterial_Inventory");

            entity.Property(e => e.Description)
                .HasMaxLength(350)
                .IsUnicode(false);
            entity.Property(e => e.Trans_Datetime).HasColumnType("datetime");
            entity.Property(e => e.Trans_Type)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");

            entity.HasOne(d => d.Raw_material_DbkeyNavigation).WithMany(p => p.Rawmaterial_Inventories)
                .HasForeignKey(d => d.Raw_material_Dbkey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rawmaterial_Inventory_Master_Rawmaterials");
        });

        modelBuilder.Entity<RecordDeletionLog>(entity =>
        {
            entity.ToTable("RecordDeletionLog");

            entity.Property(e => e.JsonData).IsUnicode(false);
            entity.Property(e => e.SourceTable)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<RecordsForDeletion>(entity =>
        {
            entity.HasKey(e => e.DeletionKey);

            entity.ToTable("RecordsForDeletion");

            entity.Property(e => e.ApprovalStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ApprovedBy)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ApprovedOn).HasColumnType("datetime");
            entity.Property(e => e.InitiatedBy)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.InitiatedOn).HasColumnType("datetime");
            entity.Property(e => e.ReasonForDeletion)
                .HasMaxLength(350)
                .IsUnicode(false);
            entity.Property(e => e.SourceDisplayName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.SourceTableName)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Roles_URL_Module_map>(entity =>
        {
            entity.HasKey(e => e.Module_Map_ID).HasName("PK_Users_URL_Module_map");

            entity.ToTable("Roles_URL_Module_map", tb => tb.HasTrigger("A_IUD_UserRoleMap_Audit_Log"));

            entity.Property(e => e.Updated_On).HasColumnType("datetime");

            entity.HasOne(d => d.User_Role_DbkeyNavigation).WithMany(p => p.Roles_URL_Module_maps)
                .HasForeignKey(d => d.User_Role_Dbkey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Roles_URL_Module_map_User_Roles");
        });

        modelBuilder.Entity<SOP_BuildReportSection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SOP_EngineBuildReport_2");

            entity.ToTable("SOP_BuildReportSection");

            entity.Property(e => e.BuildGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReportTemplateSectionGUID)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SopReportSectionGUID)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_By)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<SOP_BuildReportSection_Backup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SOP_EngineBuildReport");

            entity.ToTable("SOP_BuildReportSection_Backup");

            entity.Property(e => e.BuildGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReportTemplateSectionGUID)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SopReportSectionGUID)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_By)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<SOP_CustomAccessLink>(entity =>
        {
            entity.HasKey(e => e.LinkdbKey);

            entity.Property(e => e.BuildGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LinkGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.Updated_By)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SOP_ReportTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SOP_ReportConfiguration_1");

            entity.ToTable("SOP_ReportTemplate");

            entity.Property(e => e.AccessibleUsers).IsUnicode(false);
            entity.Property(e => e.SectionHeader)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.TemplateSectionGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_By)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<SOP_ReportTemplate_Backup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SOP_ReportConfiguration");

            entity.ToTable("SOP_ReportTemplate_Backup");

            entity.Property(e => e.AccessibleUsers).IsUnicode(false);
            entity.Property(e => e.SectionHeader)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.TemplateSectionGuid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_By)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<SOP_ReportTemplate_Document>(entity =>
        {
            entity.ToTable("SOP_ReportTemplate_Document");

            entity.Property(e => e.BuildGuid)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FileLocation)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.FileName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.UserGuid)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Static_MPL_Report_Datum>(entity =>
        {
            entity.HasKey(e => e.Sl_No);

            entity.Property(e => e.AssemblyReportingType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Collaborators).IsUnicode(false);
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Draw_part_no)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EngineRevisionDate).HasColumnType("datetime");
            entity.Property(e => e.Hierarchy).IsUnicode(false);
            entity.Property(e => e.IsParent)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ManufacturingComments).IsUnicode(false);
            entity.Property(e => e.Material_name)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.MfgStatus).IsUnicode(false);
            entity.Property(e => e.ParentDescription)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.ParentMaterial_name)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.ParentModuleRes)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.ParentName)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.ParentReporting_type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ParentRevision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PartModuleRes)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.PartPath).IsUnicode(false);
            entity.Property(e => e.Part_Remarks).IsUnicode(false);
            entity.Property(e => e.ReportGroup)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.ReportGroupIdentifier).IsUnicode(false);
            entity.Property(e => e.Reporting_Type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Type_Part_Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
            entity.Property(e => e.engineDescription).IsUnicode(false);
        });

        modelBuilder.Entity<TaskManager_ApplicationSetting>(entity =>
        {
            entity.HasKey(e => e.SettingId).HasName("PK__TaskMana__54372B1DD4AC6195");

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.DataType)
                .HasMaxLength(20)
                .HasDefaultValueSql("('string')");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.SettingKey).HasMaxLength(100);
            entity.Property(e => e.SettingValue).HasMaxLength(500);
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<TaskManager_Email>(entity =>
        {
            entity.HasKey(e => e.EmailId).HasName("PK__TaskMana__7ED91ACF561E1913");

            entity.Property(e => e.CleanSubject).HasMaxLength(1000);
            entity.Property(e => e.EmailGUID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.EmailProcessedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FromEmail).HasMaxLength(255);
            entity.Property(e => e.FromName).HasMaxLength(255);
            entity.Property(e => e.InReplyTo).HasMaxLength(500);
            entity.Property(e => e.MessageId).HasMaxLength(500);
            entity.Property(e => e.Subject).HasMaxLength(1000);
            entity.Property(e => e.ThreadId).HasMaxLength(500);
        });

        modelBuilder.Entity<TaskManager_EmailConfiguration>(entity =>
        {
            entity.HasKey(e => e.ConfigId).HasName("PK__TaskMana__C3BC335C477F6806");

            entity.ToTable("TaskManager_EmailConfiguration");

            entity.Property(e => e.ConfigKey).HasMaxLength(100);
            entity.Property(e => e.ConfigValue).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<TaskManager_EmailConfiguration1>(entity =>
        {
            entity.HasKey(e => e.ConfigId).HasName("PK__TaskMana__C3BC335CD485C33E");

            entity.ToTable("TaskManager_EmailConfigurations");

            entity.Property(e => e.ConfigGUID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ConfigName).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ImapPort).HasDefaultValueSql("((993))");
            entity.Property(e => e.ImapServer).HasMaxLength(255);
            entity.Property(e => e.InboxFolder)
                .HasMaxLength(100)
                .HasDefaultValueSql("('INBOX')");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Password).HasMaxLength(500);
            entity.Property(e => e.UseSSL)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Username).HasMaxLength(255);
        });

        modelBuilder.Entity<TaskManager_EmailNotification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__TaskMana__20CF2E12FB0DFFBE");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.NotificationGUID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.NotificationType).HasMaxLength(50);
        });

        modelBuilder.Entity<TaskManager_EmailTaskRelationship>(entity =>
        {
            entity.HasKey(e => e.RelationshipId).HasName("PK__TaskMana__31FEB88181E637EE");

            entity.Property(e => e.ConfidenceScore).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.RelationshipGUID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RelationshipType).HasMaxLength(50);
        });

        modelBuilder.Entity<TaskManager_GlobalBucket>(entity =>
        {
            entity.HasKey(e => e.BucketId).HasName("PK__TaskMana__945A0254B36477E1");

            entity.Property(e => e.BucketColor).HasMaxLength(7);
            entity.Property(e => e.BucketDescription).HasMaxLength(500);
            entity.Property(e => e.BucketGUID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.BucketName).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<TaskManager_Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__TaskMana__761ABEF005101778");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.EndDate).HasColumnType("date");
            entity.Property(e => e.Priority).HasMaxLength(20);
            entity.Property(e => e.ProjectGUID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ProjectName).HasMaxLength(255);
            entity.Property(e => e.ProjectStatus)
                .HasMaxLength(50)
                .HasDefaultValueSql("('Active')");
            entity.Property(e => e.StartDate).HasColumnType("date");
        });

        modelBuilder.Entity<TaskManager_ProjectBucket>(entity =>
        {
            entity.HasKey(e => e.BucketId).HasName("PK__TaskMana__945A02540D80ED7D");

            entity.Property(e => e.BucketColor).HasMaxLength(7);
            entity.Property(e => e.BucketDescription).HasMaxLength(500);
            entity.Property(e => e.BucketGUID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.BucketName).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<TaskManager_Task>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__TaskMana__7C6949B14D7680A3");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DueDate).HasColumnType("date");
            entity.Property(e => e.EstimatedHours).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Priority).HasMaxLength(20);
            entity.Property(e => e.ProgressPercentage).HasDefaultValueSql("((0))");
            entity.Property(e => e.StartDate).HasColumnType("date");
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.TaskGUID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.TaskTitle).HasMaxLength(255);
        });

        modelBuilder.Entity<TaskManager_TaskActivityLog>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("PK__TaskMana__45F4A79138D6E006");

            entity.ToTable("TaskManager_TaskActivityLog");

            entity.Property(e => e.ActivityGUID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ActivityType).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FieldName).HasMaxLength(100);
            entity.Property(e => e.TargetName).HasMaxLength(200);
        });

        modelBuilder.Entity<TaskManager_TaskAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__TaskMana__32499E77131ABF14");

            entity.Property(e => e.AssignedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<TaskManager_TaskChecklist>(entity =>
        {
            entity.HasKey(e => e.ChecklistId).HasName("PK__TaskMana__4C1D499A31832020");

            entity.Property(e => e.ChecklistGUID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ItemText).HasMaxLength(500);
        });

        modelBuilder.Entity<TaskManager_TaskComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__TaskMana__C3B4DFCADFA5FC31");

            entity.Property(e => e.CommentGUID).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CommentType)
                .HasMaxLength(50)
                .HasDefaultValueSql("('Comment')");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<TaskTracker>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK_ProjectTasks");

            entity.ToTable("TaskTracker");

            entity.Property(e => e.EndDate).HasColumnType("date");
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.StartDate).HasColumnType("date");
            entity.Property(e => e.TaskName).HasMaxLength(50);
            entity.Property(e => e.Updated_By).HasMaxLength(50);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<Task_Tree_Master>(entity =>
        {
            entity.HasKey(e => e.task_master_dbkey);

            entity.ToTable("Task_Tree_Master");

            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Estimated_Effort).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.Weightage_percentage).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.item_type)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.task_status)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.title)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.updated_on).HasColumnType("datetime");
        });

        modelBuilder.Entity<TestDataRepoJson>(entity =>
        {
            entity.HasKey(e => e.TestDataJSonDbKey);

            entity.ToTable("TestDataRepoJson");
        });

        modelBuilder.Entity<TestDataRepository>(entity =>
        {
            entity.HasKey(e => e.TestdataDbKey);

            entity.ToTable("TestDataRepository");

            entity.Property(e => e.AtmosphericPressure)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.BuildNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CellNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.DecuSWBuildNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EngineName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NH)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NL)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RoomTemperature)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.RunNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<TestDataRepository_backup>(entity =>
        {
            entity.HasKey(e => e.TestdataDbKey);

            entity.ToTable("TestDataRepository_backup");

            entity.Property(e => e.AtmosphericPressure)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.BuildNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CellNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.DecuSWBuildNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EngineName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NH)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NL)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RoomTemperature)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.RunNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<TestDataValue>(entity =>
        {
            entity.HasKey(e => e.TestDataValDbKey).HasName("PK_TestDataValue_2");

            entity.ToTable("TestDataValue");

            entity.Property(e => e.A2OK)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.A2OKsts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ACKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ACSFR)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ACSOP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ACSOP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ACSOT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AIT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AIT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AITavg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ASP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ASPSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ASPSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ASPTP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ASP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AST)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AST_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AST_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AltRdy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BAF)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BAF_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BAF_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BAP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BAT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BATSaA)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BPIP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BPIPT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BPIP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BPIP_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BRG5CP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BRG5CT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BatSASts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CEGT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CEGT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CEGT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CEGT_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CEGT_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CMDSTST)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CNSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CNSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CNSKT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2DC)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2DC_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2DC_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2DC_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2DC_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2DCavg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2avg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3C)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3C_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3C_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3C_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3C_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3C_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3C_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3Cavg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_7)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_8)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DEC)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DEC_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DONOFF)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DRVSTST)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DS)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DSTAkT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DS_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DS_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DcuOnSts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EGT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EGT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EgSDCmT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EgSRly)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EgStCT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EncdrT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EngShd)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EngStp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EngStr)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EngStr_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Engshtdsts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Engstcdsts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ExtExc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ExtExcSts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FBSP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FBSP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FBSP_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FBSP_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FCSSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FIP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FIP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FIP_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FMV)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FMVDMT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FPSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FTIF)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FTIF_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FTP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FTP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FTP_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GBSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GBSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GG)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GGCmFBT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBAT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBAT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBSKT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBSKT_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBSKT_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBSKT_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LINRISKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LINROSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LINROSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LINROSKT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LINROSKT_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LP_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LS)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MBP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MBP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MPOP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MPOP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MPOP_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NH)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NHSTST)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NHT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NH_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NH_67_)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NH>67%");
            entity.Property(e => e.NL)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NLT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NL_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NL_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OK)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OLPR)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OLPR_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OLPR_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OLPR_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OOSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OTP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OTSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OTSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OTSKT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OTSKT_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OTT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OTT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_10)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_11)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_12)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_13)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_14)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_15)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_16)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_17)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_18)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_19)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_20)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_21)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_22)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_23)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_24)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_25)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_26)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_27)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_28)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_29)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_30)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_31)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_32)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_33)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_34)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_35)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_36)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_37)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_38)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_39)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_7)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_8)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_9)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_11)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_12)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_13)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_14)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1avg_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P2B)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P2B_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P2B_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P2B_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P2C)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P2C_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P3T)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P6B)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P6B_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P6Bavg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P6_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P6avg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCCT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCCT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCCT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCCmFBT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCFP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCFP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCFT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCFT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCFT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCHP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCLP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCPP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCRFT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PGG)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PGGSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PGGSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PGGSts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PI2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PI2Sts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PISKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PISKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PISKT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PLA)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PLA_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PSp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PSp_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PV124)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PV124Sts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PV31)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PV3PI1Sts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PWMCURRT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PYROSWITCH)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PrepStrt)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Prp2srtSts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PrpStr)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RBHSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RBSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RBST)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SASP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SASP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SASP_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SASP_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SASP_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SASP_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SASP_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SAST)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SAST_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SAST_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SAST_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SMSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.STS)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.STST)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW_7)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SpGT67)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_10)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_7)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_8)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_9)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1avg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T2Bavg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T2T)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T2avg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T3_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T3avg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T5T)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T5_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6B)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6Bavg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6C)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6_7)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6avg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.THROTLT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TIME)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.V)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VALT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VFCSV)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VFMA)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VFMV)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VFSMA)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VGBV)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VLPT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VNDVENT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VOPV)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VRMA)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VRMT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.V_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.V_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.V_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.V_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.V_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.V_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VltgRdy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VolRdy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WF)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WFDEM)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WFDEMCT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WFDEMT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WFM)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WF_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WF_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WFx)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WFx_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.XG)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.XG_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e._2DDp)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("2DDp");
            entity.Property(e => e._2DDp_1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("2DDp_1");
            entity.Property(e => e._2H)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("2H");
            entity.Property(e => e._2HT)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("2HT");
            entity.Property(e => e._2H_1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("2H_1");
            entity.Property(e => e._2H_2)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("2H_2");
            entity.Property(e => e.p1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_40)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_41)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_42)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_43)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_44)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_45)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_46)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_47)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_10)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_15)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_16)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_17)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_18)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_7)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_8)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_9)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1avg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p2B_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p2B_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p2iC)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p2iC_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p2iC_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p3ku)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.pHPC2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.pHPC2_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.prp2srtcnf)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TestDataValue_backup>(entity =>
        {
            entity.HasKey(e => e.TestDataValDbKey).HasName("PK_TestDataValue");

            entity.ToTable("TestDataValue_backup");

            entity.Property(e => e.A2OK)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.A2OKsts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ACKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ACSFR)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ACSOP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ACSOP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ACSOT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AIT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AIT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AITavg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ASP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ASPSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ASPSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ASPTP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ASP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AST)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AST_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AST_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AltRdy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BAF)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BAF_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BAF_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BAP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BAT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BATSaA)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BPIP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BPIPT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BPIP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BPIP_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BRG5CP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BRG5CT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BatSASts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CEGT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CEGT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CEGT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CEGT_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CEGT_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CMDSTST)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CNSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CNSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CNSKT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2DC)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2DC_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2DC_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2DC_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2DC_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2DCavg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP2avg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3C)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3C_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3C_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3C_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3C_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3C_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3C_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CP3Cavg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_7)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CSSKT_8)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DEC)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DEC_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DONOFF)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DRVSTST)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DS)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DSTAkT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DS_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DS_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DcuOnSts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EGT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EGT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EgSDCmT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EgSRly)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EgStCT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EncdrT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EngShd)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EngStp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EngStr)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EngStr_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Engshtdsts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Engstcdsts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ExtExc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ExtExcSts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FBSP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FBSP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FBSP_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FBSP_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FCSSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FIP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FIP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FIP_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FMV)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FMVDMT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FPSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FTIF)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FTIF_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FTP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FTP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FTP_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GBSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GBSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GG)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GGCmFBT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBAT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBAT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBSKT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBSKT_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBSKT_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JPBSKT_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LINRISKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LINROSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LINROSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LINROSKT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LINROSKT_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LP_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LS)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MBP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MBP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MPOP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MPOP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MPOP_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NH)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NHSTST)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NHT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NH_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NH_67_)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NH>67%");
            entity.Property(e => e.NL)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NLT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NL_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NL_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OK)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OLPR)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OLPR_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OLPR_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OLPR_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OOSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OTP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OTSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OTSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OTSKT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OTSKT_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OTT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OTT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_10)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_11)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_12)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_13)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_14)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_15)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_16)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_17)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_18)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_19)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_20)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_21)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_22)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_23)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_24)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_25)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_26)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_27)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_28)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_29)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_30)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_31)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_32)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_33)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_34)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_35)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_36)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_37)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_38)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_39)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_7)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_8)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1D_9)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_11)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_12)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_13)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_14)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1_7)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P1avg_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P2B)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P2B_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P2B_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P2B_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P2C)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P2C_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P3T)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P6B)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P6B_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P6Bavg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P6_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.P6avg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCCT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCCT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCCT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCCmFBT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCFP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCFP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCFT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCFT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCFT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCHP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCLP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCPP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PCRFT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PGG)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PGGSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PGGSKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PGGSts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PI2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PI2Sts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PISKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PISKT_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PISKT_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PLA)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PLA_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PSp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PSp_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PV124)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PV124Sts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PV31)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PV3PI1Sts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PWMCURRT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PYROSWITCH)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PrepStrt)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Prp2srtSts)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PrpStr)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RBHSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RBSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RBST)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SASP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SASP_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SASP_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SASP_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SASP_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SASP_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SASP_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SAST)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SAST_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SAST_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SAST_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SMSKT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.STS)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.STST)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SW_7)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SpGT67)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_10)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_7)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_8)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1_9)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T1avg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T2Bavg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T2T)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T2avg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T3_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T3avg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T5T)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T5_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6B)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6Bavg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6C)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6_7)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.T6avg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.THROTLT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.V)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VALT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VFCSV)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VFMA)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VFMV)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VFSMA)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VGBV)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VLPT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VNDVENT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VOPV)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VRMA)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VRMT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.V_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.V_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.V_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.V_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.V_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.V_6)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VltgRdy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VolRdy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WF)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WFDEM)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WFDEMCT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WFDEMT)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WFM)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WF_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WF_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WFx)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WFx_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.XG)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.XG_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e._2DDp)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("2DDp");
            entity.Property(e => e._2DDp_1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("2DDp_1");
            entity.Property(e => e._2H)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("2H");
            entity.Property(e => e._2HT)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("2HT");
            entity.Property(e => e._2H_1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("2H_1");
            entity.Property(e => e._2H_2)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("2H_2");
            entity.Property(e => e.p1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_40)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_41)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_42)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_43)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_44)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_45)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_46)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1D_47)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_10)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_15)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_16)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_17)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_18)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_8)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1_9)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p1avg)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p2B_4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p2B_5)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p2iC)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p2iC_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p2iC_2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.p3ku)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.pHPC2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.pHPC2_1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.prp2srtcnf)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserDbkey);

            entity.ToTable(tb => tb.HasTrigger("A_IUD_Users_Audit_log"));

            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
            entity.Property(e => e.UserName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.User_type)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Vendor)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.User_Role_DbkeyNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.User_Role_Dbkey)
                .HasConstraintName("FK_Users_User_Roles");
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.SessionGuid).HasName("PK_UserSession_1");

            entity.ToTable("UserSession");

            entity.Property(e => e.SessionGuid).HasDefaultValueSql("(newid())");
            entity.Property(e => e.BrowserInfo)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ClientIP)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SessionEnd).HasColumnType("datetime");
            entity.Property(e => e.SessionStart).HasColumnType("datetime");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User_Addtional_Role_Map>(entity =>
        {
            entity.HasKey(e => e.User_Adt_role_Dbkey);

            entity.ToTable("User_Addtional_Role_Map");

            entity.Property(e => e.Updated_On).HasColumnType("datetime");

            entity.HasOne(d => d.Master_DbkeyNavigation).WithMany(p => p.User_Addtional_Role_Maps)
                .HasForeignKey(d => d.Master_Dbkey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Addtional_Role_Map_Master_General");

            entity.HasOne(d => d.UserDbkeyNavigation).WithMany(p => p.User_Addtional_Role_Maps)
                .HasForeignKey(d => d.UserDbkey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Addtional_Role_Map_Users");
        });

        modelBuilder.Entity<User_Module>(entity =>
        {
            entity.HasKey(e => e.Module_Dbkey).HasName("PK_Project_Modules");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("AD_Delete_Mod_map");
                    tb.HasTrigger("AI_create_Mod_map");
                });

            entity.HasIndex(e => e.Url, "UQ_Url").IsUnique();

            entity.Property(e => e.MenuGroup)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MenuItem)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Module_Name)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Page_title)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");
            entity.Property(e => e.Url)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User_Role>(entity =>
        {
            entity.HasKey(e => e.User_Role_Dbkey);

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("AI_create_Mod_map_on_roles");
                    tb.HasTrigger("A_IUD_UserRoles_Audit_log");
                });

            entity.Property(e => e.LandingPage)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Role_name)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<User_session_manager>(entity =>
        {
            entity.HasKey(e => e.Session_Dbkey).HasName("PK_User");

            entity.ToTable("User_session_manager", tb => tb.HasTrigger("AIU_InsertAudit"));

            entity.Property(e => e.Browser_Type)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Device_Type)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IP_address)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Session_End).HasColumnType("datetime");
            entity.Property(e => e.Session_ID)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Session_Start).HasColumnType("datetime");
        });

        modelBuilder.Entity<Utilization_Session>(entity =>
        {
            entity.HasKey(e => e.Session_ID);

            entity.Property(e => e.LogOut_datetime).HasColumnType("datetime");
            entity.Property(e => e.Login_datetime).HasColumnType("datetime");
        });

        modelBuilder.Entity<Utilization_Task_Log>(entity =>
        {
            entity.HasKey(e => e.Utli_Log_Dbkey);

            entity.Property(e => e.Activity)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Login_datetime).HasColumnType("datetime");
            entity.Property(e => e.Remarks).IsUnicode(false);
            entity.Property(e => e.SuperVicer)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.Task)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Task_End).HasColumnType("datetime");
            entity.Property(e => e.Task_Start).HasColumnType("datetime");
            entity.Property(e => e.Task_Status)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Task_note)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.Updated_On).HasColumnType("datetime");
        });

        modelBuilder.Entity<Utilization_todo_StatusLog>(entity =>
        {
            entity.HasKey(e => e.logdbkey).HasName("PK_Utilization_todo_tasks_StatusLogs");

            entity.Property(e => e.File_location)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.attachment_file_name).IsUnicode(false);
            entity.Property(e => e.status_date).HasColumnType("date");
            entity.Property(e => e.system_file_name).IsUnicode(false);
            entity.Property(e => e.task_status)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.updated_on).HasColumnType("datetime");
        });

        modelBuilder.Entity<Utilization_todo_Task>(entity =>
        {
            entity.HasKey(e => e.todo_task_dbkey).HasName("PK_Utilization_to_do_TaskLists");

            entity.ToTable("Utilization_todo_Task");

            entity.Property(e => e.File_location)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Updated_on).HasColumnType("datetime");
            entity.Property(e => e.attachment_file_name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.system_file_name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.task_description).IsUnicode(false);
            entity.Property(e => e.task_due_date).HasColumnType("date");
            entity.Property(e => e.task_name)
                .HasMaxLength(70)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(e => e.Vendor_Dbkey);

            entity.Property(e => e.Updated_On).HasColumnType("datetime");
            entity.Property(e => e.Vendor_Adress)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Vendor_City)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Vendor_Contact)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Vendor_Email)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Vendor_ID_System)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Vendor_ID_User)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Vendor_Name)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Vendor_Pincode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Vendor_State)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.vendor_GUID)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<WatermarkConfiguration>(entity =>
        {
            entity.ToTable("WatermarkConfiguration");

            entity.Property(e => e.ConfigurationFor)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<persons_datum>(entity =>
        {
            entity.HasKey(e => e.personDbKey);

            entity.Property(e => e.PersonGUID)
                .HasMaxLength(70)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
