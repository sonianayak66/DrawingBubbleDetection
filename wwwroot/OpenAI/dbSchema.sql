--THIS IS MICROSFT SQL SERVER DATABASE SCHEMA; developed for managing engine manufacturing process control
CREATE TABLE Master_General(
	Master_Dbkey int,
	Master_Name varchar(150)) 
CREATE TABLE MetaMaster(
	Id int,
	MasterGUID varchar(50),
	MasterType varchar(50),
	DisplayText varchar(80))
CREATE TABLE Vendors(
	Vendor_Dbkey int,-- Primary key
	Vendor_Name varchar(300),
	Vendor_Email varchar(300),
	Vendor_Contact varchar(300),-- person name and phone numbers
	Vendor_Adress varchar(max) -- address details
) 
CREATE TABLE Master_Rawmaterials(
	Raw_material_Dbkey int,
	is_active bit,-- 1 is active and 0 is in active
	Raw_material_Name varchar(max)  -- this is contacted with name , diameter, thickness and raw material type (Bar,sheet,Pipe,Vendor Material)
) 
CREATE TABLE AspNetUsers(
	Id nvarchar(450),
	UserName nvarchar(256),--User name
	Email nvarchar(256),
	OldUserDbkey int-- used as FOREIGN KEY in lot of table
)
CREATE TABLE Project(
	Project_Dbkey int,
	Title varchar(150),
	Display_title varchar(150),
	Description varchar(250),
	DOS date,-- date of sanction
	EDO date,-- expected date of sanction
	Project_Number varchar(150),
	No_of_Engines float,-- Number of engine 
	Unique_Name varchar(50),-- Engine names
	BL_Engine_Dbkey int,-- FOREIGN KEY from  Base_Line_Engines table; Project.BL_Engine_Dbkey can be left joined with  Base_Line_Engines.BL_Engine_Dbkey
	EstimatedCost float 
	) 
CREATE TABLE ACSN(
	acsnKey int, -- Primary key
	SerialNumber int,--serial numbers of ACSN 
	Series varchar(50), --series which having data like T-Series,T-Series and New Drawing
	ReceivedDate date,
	ModuleRefNum varchar(50),-- Module reference number
	PartDbKey int, -- FOREIGN KEY from  Engine_Parts_Master table pk key is Engine_Part_Dbkey; ACSN.PartDbKey can be left joined with  Engine_Parts_Master.Engine_Part_Dbkey
	DrawingNumber varchar(50),
	description varchar(250),
	Module int, -- FOREIGN KEY of Master_General  table pk key is Master_Dbkey ; ACSN.Module can be left joined with Master_General.Master_Dbkey and we can get module name by selecting Master_General.master_Name
	existingRevision varchar(50),
	NewRevision varchar(50),
	Remarks varchar(800),	
	ACSNnum varchar(100),
	ACSN_Status varchar(100) -- Closed and Open are the available ACSN status
) 
CREATE TABLE ACSNItems(
	ACSNStatusKey int,  -- Primary key 
	acsnKey int, --FOREIGN KEY from ACSN table pk key is acsnKey ;ACSNItems.acsnKey can be left joined with  ACSN.acsnKey
	acsnStatus varchar(50),
	acsnStepId int, --Step taken to work on and their status; ACSN  FOREIGN KEY of Master_General  table pk key is Master_Dbkey ; ACSN.acsnStepId can be left joined with Master_General.Master_Dbkey and we can get  ascn status name by selecting Master_General.master_Name
	StartDate date,
	EndDate date,
	Remarks varchar(max),
	isActiveStatus bit -- Record activation status like, 1 means active 0 means inactive	
 ) 
CREATE TABLE Base_Line_Engines(
	BL_Engine_Dbkey int, -- Primary key 
	Engine_Title varchar(150),-- name/title of base line engine
	Engine_Description varchar(250),
	is_active int,	-- 0 means Inactive state; 1 means active state
	Revision_date datetime
)
CREATE TABLE CastingDetails(
	CastingDbkey int,-- Primary key 
	castingGUID varchar(50),
	OrderType varchar(50), -- Casting,Forging and Pyro are the available order types 
	DemandNumber varchar(750),
	OrderDate date,
	Remarks varchar(max),
	MMGOrderNumber varchar(max),	
	Isdeleted bit,-- 0 means not deleted record
	OrderStatus varchar(50),-- Open,InProgress and Completed are the available OrderStatus
	OrderNumbers varchar(max),
	DemandingOfficer int,--FOREIGN KEY from AspNetUsers table;AspNetUsers.OldUserDbkey can be left joined with  CastingDetails.DemandingOfficer
	DemandDesc varchar(max) 
) 
CREATE TABLE CastingItems(
	CastingItemKey int,-- Primary key 
	CastingDbkey int,--FOREIGN KEY from CastingDetails table;CastingItems.CastingDbkey can be left joined with  CastingDetails.CastingDbkey
	EnginePartDbkey int,--FOREIGN KEY from Engine_Parts_Master table;CastingItems.EnginePartDbkey can be left joined with  Engine_Parts_Master.Engine_Part_Dbkey
	PartName varchar(50),
	GTREDrgNo varchar(80),
	ItemDescription varchar(max),
	OrderQty float,
	Vendor int,--FOREIGN KEY from Vendors table;CastingItems.Vendor can be left joined with  Vendors.Vendor_Dbkey
	OrderNumber varchar(50),	
	Isdeleted bit,-- 1 means deleted rest is active
	DeliveryDate date,
	RawMaterial int  -- FOREIGN KEY from Vendors table
)
-- Casting Material Issue
CREATE TABLE Casting_MaterialIssue(
	IssueDbKey int,-- Primary key
	VendorKey int, -- Foreign key from Vendors table, Casting_MaterialIssue.VendorKey can be left joined with  Vendors.Vendor_Dbkey
	IssueDate date,
	Reference varchar(200),-- Reference numbers 
	Issue_type varchar(50) -- Issue type column having data like 'Casting'
)
---- Casting Material Issue Items
CREATE TABLE Casting_MaterialIssue_Items(
     IssueItemKey int,-- Primary key
     IssueDbKey int, -- Foreign key from Casting_MaterialIssue table,Casting_MaterialIssue_Items.IssueDbKey can be left joined with  Casting_MaterialIssue.IssueDbKey
     QtySplitKey int,
     IssueQty int,
     IssueSlNos varchar(max),	
     ForEngine varchar(250) 
	) 
CREATE TABLE CastingReceiptQtySplit(
	QtySplitKey int,
	ReceiptsItemSplitKey int,
	SplitQty int,
	StatusRemarks varchar(150),
	Remarks varchar(250),
	SerialNos varchar(max)	
 )
CREATE TABLE CastingReceiptsItemSplit(
	Id int,
	SerialNumber varchar(max),
	BatchNumber varchar(max),
	HeatNumber varchar(max),
	Attachments varchar(200),
	UpdatedBy int,
	UpdatedOn datetime,
	Revision varchar(50),
	Status varchar(50),
	Isdeleted bit,
	OrderItemKey int,
	ReceiptNumber varchar(50),
	ReceiptDate date,
	ReceiptGuid varchar(50),
	Remarks varchar(max),
	VendorDrawingNo varchar(80) 
)
-- Engine_Parts_Master is also called as MPL or Master part list
CREATE TABLE Engine_Parts_Master
(Engine_Part_Dbkey int,
	Engine_Dbkey int,-- Pk key and it act as child key
	Type_Dbkey int, --Foreign key from Master_Part_Types table, Engine_Parts_Master.Type_Dbkey can be left joined with  Master_Part_Types.Type_Dbkey
	Draw_part_no varchar(150),
	Revision varchar(100),	
	Quantity float,
	Manufacturing_Duration float,
	Description varchar(max),
	Comments varchar(max),
	Raw_Material int,
	Module_Responsibility int,--Foreign key from Master_General table, Engine_Parts_Master.Module_Responsibility can be left joined with  Master_General.Master_Dbkey
	Parent_id int,-- act as Parent key to maintain relationship ;
)
CREATE TABLE Master_Part_Types(
	Type_Dbkey int,
	Type_Part_Name varchar(50) ) 
--Engine_Parts_Usage is Master part list for different baseline engines
CREATE TABLE Engine_Parts_Usage(
	Part_relation_dbkey int,--primary key
	BL_Engine_Dbkey int,--Foreign key from Base_Line_Engines table, Engine_Parts_Usage.BL_Engine_Dbkey can be left joined with  Base_Line_Engines.BL_Engine_Dbkey
	Engine_Part_Dbkey int,--Foreign key from Engine_Parts_Master table, Engine_Parts_Usage.Engine_Part_Dbkey can be left joined with  Engine_Parts_Master.Engine_Part_Dbkey -- child key
	Parent_id int,-- act as Parent key
	is_active int,-- 1 is active and 0 is inactive
	Revision varchar(50),
	Qty_per_Engine int,	
	Description varchar(max),
	Comments varchar(max),
	Reporting_Parent int,
	Part_Remarks varchar(max),
	ReportDisplayOrder float,
	Module_Responsibility int,
	Raw_Material int,
	Execution_Resp varchar(50),
	Execution_Resp_additionalLevel varchar(50),
	Collaborators varchar(max),
	ManufacturingComments varchar(max),
	ForSopOnly bit -- if 1 means return as Yes else No
	) 
-- EngineBuilds is also called as SOP / Builds
CREATE TABLE EngineBuilds(
	Id int,
	BuildGuid varchar(50),
	BaseLineEngineDbkey int,--Foreign key from Base_Line_Engines table, EngineBuilds.BaseLineEngineDbkey can be left joined with  Base_Line_Engines.BL_Engine_Dbkey
	BuildName varchar(150),-- BuildName is also called as Build or SOP name , sample values will be like T1B1,T1B2 ,J1B1 
	BuildDate date,
	ReferenceNumber varchar(150),
	Description varchar(max)	
)
-- EngineBuildComponents is also called as SOP Components
CREATE TABLE EngineBuildComponents(
	Id int,-- PK key
	BuildDbkey int,--Foreign key from EngineBuilds table, EngineBuildComponents.BuildDbkey can be left joined with  EngineBuilds.Id
	BaseLineEngineDbkey int,--Foreign key from Base_Line_Engines table, EngineBuilds.BaseLineEngineDbkey can be left joined with  Base_Line_Engines.BL_Engine_Dbkey
	PartRelationKey int,--Foreign key from Engine_Parts_Usage table, EngineBuildComponents.PartRelationKey can be left joined with  Engine_Parts_Usage.Part_relation_dbkey
	EnginePartDbkey int,--Foreign key from Engine_Parts_Master table, Engine_Parts_Usage.Engine_Part_Dbkey can be left joined with  Engine_Parts_Master.Engine_Part_Dbkey -- child key
	ParentId int,-- act as Parent key to maintain relationship
	IsActive int,-- 1 means active and 0 means inactive
	Revision varchar(50),
	QtyPerEngine int,
	Description varchar(max),
	JobCard varchar(50),
	ContractNumber varchar(50),
	SerialNumber varchar(max),
	Remarks varchar(max),
	DrawingNumber varchar(150),-- Drawing number for this SOP Components
	SchemeNumber varchar(150),
	UpdatedBy varchar(150),--Foreign key from AspNetUsers table, EngineBuildComponents.UpdatedBy can be left joined with  AspNetUsers.Id 
	UpdatedOn datetime,
	IsReplaced bit 
)
--Material_Issue_Note is also called as Material Issues
CREATE TABLE Material_Issue_Note(
	Issue_Dbkey int,-- PK key
	Demand_No varchar(350),
	Order_Ref_No varchar(350),
	Order_Ref_Date date,
	Vendor int,--FOREIGN KEY from Vendors table;Material_Issue_Note.Vendor can be left joined with  Vendors.Vendor_Dbkey
	Total_Qty float,
	Total_Cost float,
	Returnable varchar(100),
	Issue_Purpose int--FOREIGN KEY of Master_General;Material_Issue_Note.Issue_Purpose can be left joined with Master_General.Master_Dbkey 
  ) 
-- Child table of Material_Issue_Note having material issues items
CREATE TABLE Material_Issue_Items(
	Issue_Item_Dbkey int,
	Issue_Dbkey int,--FOREIGN KEY from Material_Issue_Note table;Material_Issue_Items.Issue_Dbkey can be left joined with  Material_Issue_Note.Issue_Dbkey
	Raw_material_Dbkey int,--FOREIGN KEY of Master_Rawmaterials;Material_Issue_Items.Raw_material_Dbkey can be left joined with Master_Rawmaterials.Raw_material_Dbkey 
	Drawing_no varchar(max),
	Description varchar(450),
	Engine_Part_Dbkey int,--Foreign key from Engine_Parts_Master table, Engine_Parts_Usage.Engine_Part_Dbkey can be left joined with  Engine_Parts_Master.Engine_Part_Dbkey
	Qty float,
	Size varchar(250),
	Denom varchar(250),
	Qty_Issue float,
	Weight_Kg float,
	JobCardNumber varchar(50),
	JCFileName varchar(max),
	SerialNo varchar(max) ) 
CREATE TABLE Material_Issue_Items_Parts(
	Material_Issue_Items_Parts_Dbkey int,
	Issue_Dbkey int,
	Issue_Item_Dbkey int,
	Engine_Part_Dbkey int,
	Part_Name varchar(100) ) 
CREATE TABLE Material_IssueItems_Split(
	split_issue_id int,
	Issue_Item_Dbkey int,
	Issue_Dbkey int,
	SplitId int )
-- NonConformanceReport also called as NCR
CREATE TABLE NonConformanceReport(
	Id int,
	NCRGuid varchar(50),
	ReferenceNumber varchar(100),
	ReceivedDate date,
	ReceivedFrom int, --FOREIGN KEY of Master_General;NonConformanceReport.ReceivedFrom can be left joined with Master_General.Master_Dbkey  to get ReceivedFrom from selecting Master_General.Master_Name
	ComitteeReferred varchar(50),-- L1 and L2 are the available data for ComitteeReferred 
	ReportStatus varchar(100),-- In Process,Cleared and Rejected are the available data for ReportStatus 
	Remarks varchar(max),
	Engine_Part_Dbkey int,--Foreign key from Engine_Parts_Master table, Engine_Parts_Usage.Engine_Part_Dbkey can be left joined with  Engine_Parts_Master.Engine_Part_Dbkey
	Vendor int,--FOREIGN KEY from Vendors table;Material_Issue_Note.Vendor can be left joined with  Vendors.Vendor_Dbkey
	SerialNumber varchar(max), 
	Revision varchar(50),
	Qty int,
	Module varchar(100),-- In Process,Cleared and Comments are the available data for Module 
	Stress varchar(100),-- In Process,Cleared and Comments are the available data for Stress 
	Chair varchar(100),-- In Process,Cleared and Comments are the available data for Chair 
	Tas varchar(100),-- In Process,Cleared and Comments are the available data for Tas 
	Module_Responsibilty int --Foreign key from Master_General table, NonConformanceReport.Module_Responsibilty can be left joined with  Master_General.Master_Dbkey 
) 
-- Procurement_Demands is also called as Demands (if Raw material Item types use where Item_Type = 'RM' for filter)
	CREATE TABLE Procurement_Demands(
	DemandDbKey int,
	Project_Head varchar(50),-- Project 
	Demand_No varchar(50),
	Item_Description varchar(500),
	UOM varchar(50),--Unit of measurement;  Each,Kg,Service and NA are the available data for UOM
	Item_Type varchar(50),--RM,SER,LRU,BOI,Service,Parts are available data
	EstimatedCost numeric(18, 5),
	TenderMode varchar(50),
	DemandingOfficer int,--Foreign key from AspNetUsers table; Procurement_Demands.DemandingOfficer can be left join with  AspNetUsers.OldUserDbkey;
	StatusDate date,
	CurrentStatus varchar(50),-- demand status ; TD approval,AD approval and DO initiation are sample data for CurrentStatus
	DO_Review bit,-- 1 means DO reviewed ,0 means not reviewed
	EstimatedOrderDate datetime,
	Delivery_Schedule int,
	Planned_Date_of_receipt datetime,
	Remarks varchar(max),
	MMG_File_No varchar(50),
	ActualCost numeric(18, 5),
	Vendor_Dbkey int,--FOREIGN KEY from Vendors table;Procurement_Demands.Vendor_Dbkey can be left joined with  Vendors.Vendor_Dbkey
	IsShortClosure bit, -- if 1 means its short closed
	ShortClosedOn datetime,
	ShortClosedBy int,--Foreign key from AspNetUsers table; Procurement_Demands.ShortClosedBy can be left join with  AspNetUsers.OldUserDbkey;
	ShortCloseReason varchar(180),
	OrderNumbers varchar(250),
	OrderType varchar(50))
CREATE TABLE Procurement_Demand_Items(
	DemandItemKey int,
	DemandDbKey int,--Foreign key from Procurement_Demands table; Procurement_Demand_Items.DemandDbKey can be left join with  Procurement_Demands.DemandDbKey;
	ItemDbKey int,--Foreign key from Master_Rawmaterials table; Procurement_Demand_Items.ItemDbKey can be left join with  Master_Rawmaterials.Master_Rawmaterials;
	UOM varchar(50),
	Qty float,-- Quantity of item demanded
	Remarks varchar(max),
	Item_Code varchar(150),
	Engine_Part_Dbkey int,--Foreign key from Engine_Parts_Master table, Procurement_Demand_Items.Engine_Part_Dbkey can be left joined with  Engine_Parts_Master.Engine_Part_Dbkey
	Item_Sub_Type varchar(70),
	height varchar(70),
	ShortCloseQty float,
	MMGOrderNumber varchar(150)) 
CREATE TABLE Procurement_Demand_MileStone(
	MilestoneDbKey int,
	DemandDbkey int,
	DemandItemDbKey int,
	Milestone int,
	DeliveryDate date,
	Qty float )
CREATE TABLE Procurement_Demand_Receipts(
	Receipt_dbkey int,
	Receipt_Date date,
	Receipt_No varchar(100),
	DemandDbKey int,
	DemandItemKey int,
	Physical_inventory float,
	Receiving_inventory float,
	Updated_By int,
	Updated_on datetime,
	Index_No int,		
	breadth numeric(18, 0)) 
CREATE TABLE Procurement_ReceiptItemSplit(
	SplitId int,
	Receipt_dbkey int,
	Measurement float,
	UOM varchar(50),
	Material_Reference_No varchar(max),
	Heat_No varchar(50),
	Batch_No varchar(50),
	Updated_by int,
	Updated_on datetime,
	Attachment_Db_Key varchar(max),
	Measurement_breadth float,
	Weight float,
	AdditionalinfoJson varchar(max),
	Revision varchar(50)) 
 
