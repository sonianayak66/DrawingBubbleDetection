//const { P } = require("../vendors/draggable/draggable.bundle.legacy");

//const { error } = require("jquery");

var jc;

function LoadMplJstree() {

    $(".search-input").keyup(function () {
        var searchString = $(this).val();
        $('#mpljstree').jstree('search', searchString);
    });

    $.ajax({
        type: "Get",
        url: "/MPL/GetMasterPartList?baselineenginedbkey=" + document.getElementById("BaseLineEngineDbkey").value,
        success: function (data) {

            var nodelog = $('#mpljstree').jstree(
                {
                    'core': {
                        'data': data,
                    },

                    "search": {
                        "case_insensitive": true,
                        "show_only_matches": true,
                        "show_only_matches_children": true
                    },

                    "plugins": ["html_data", "contextmenu", "dnd", "search", "types", "adv_search"],

                    "contextmenu": {
                        'show_at_node': true,
                        "items": function ($node) {

                            if (document.getElementById("MPLView").value == 'RevisionManagement') {
                                return {

                                    "Create": {
                                        "label": "Create",
                                        "action": function (obj) {
                                            CreateEngineMasterPart($node.id, "Create", "Read_Write");
                                        }
                                    },
                                    "CreateWithExistingParts": {
                                        "label": "Create With Existing Parts",
                                        "action": function (obj) {
                                            CreateEngineMasterPart($node.id, "Create_With_Existing_Part", "Read_Write");
                                        }
                                    },
                                    "Edit": {
                                        "label": "Update",
                                        "action": function (obj) {
                                            CreateEngineMasterPart($node.id, "Edit", "Read_Write");
                                        }
                                    }
                                };
                            } else {
                                return {
                                    "View": {
                                        "label": "View",
                                        "action": function (obj) {
                                            CreateEngineMasterPart($node.id, "Edit", "Readonly");
                                        }
                                    },
                                    "Materialissue": {
                                        "label": "Material issue",
                                        "action": function (obj) {
                                            GetCreatepopUpMaterialIssue($node.id, 1, "Readonly");
                                        }
                                    },
                                    "MfgStatus": {
                                        "label": "Mfg Status",
                                        "action": function (obj) {
                                            ViewMfgStatus($node.id, 1, "Readonly");
                                        }
                                    },
                                    "InspectionRports": {
                                        "label": "Inspection Reports",
                                        "action": function (obj) {
                                            ViewInspectinReports($node.id, 1, "Readonly");
                                        }
                                    },
                                };
                            }


                        }
                    }


                });


            setTimeout(function () {
                countTotalNodes();
            }, 1000); // Adjust the timeout as needed
        }
    });
}


function LoadMPLJstreeWithCheckBox() {

    localStorage.removeItem("MPL");

    $(".search-input").keyup(function () {
        var searchString = $(this).val();
        $('#mpljstree').jstree('search', searchString);
    });

    $.ajax({
        type: "Get",
        url: "/MPL/GetMasterPartList?BaselineEngineKey=" + document.getElementById("BaseLineEngineDbkey").value + "&isactive=-1",
        success: function (data) {

            var nodelog = $('#mpljstree').jstree(
                {
                    'core': {
                        'data': data,
                        "check_callback": true
                    },

                    "search": {
                        "case_insensitive": true,
                        "show_only_matches": true,
                        "show_only_matches_children": true
                    },

                    "checkbox": {
                        "keep_selected_style": false,
                        "visible": true,
                        "three_state": false,
                        "whole_node": true,
                    },

                    "plugins": ["checkbox", "search", "adv_search"],

                });
            setTimeout(function () {
                countTotalNodes();
            }, 1000); // Adjust the timeout as needed
        }
    });

}

function countNodes(node) {
    let count = 1; // Start with the current node
    if (node.children) {
        node.children.forEach(child => {
            count += countNodes(child);
        });
    }
    return count;
}

// Function to get total node count
function countTotalNodes() {
    // Get the entire tree in JSON format
    const treeData = $('#mpljstree').jstree(true).get_json('#', { flat: true });

    let totalNodes = 0;
    treeData.forEach(node => {
        // Count nodes recursively for each node
        totalNodes += countNodes(node);
    });

    document.getElementById("totalCompCount").innerHTML = "Total Count = " + (totalNodes - 1);

}



var a = [];

$("#mpljstree").bind("changed.jstree",
    function (e, data) {
        try {
            var Node = data.node.id;
            a = JSON.parse(localStorage.getItem('MPL')) || [];
            var replaced = false;
            var partrelationKey = data.node.id.split('_')[1];

            for (i = 0; i < a.length; i++) {
                if (a[i].split(';')[0] == data.node.id.split('_')[0]) {
                    var item = data.node.id.split('_')[0] + ";" + data.node.id.split('_')[1] + ";" + data.node.state.selected + ";" + partrelationKey;
                    a[i] = item;
                    replaced = true;
                }
            }
            if (replaced == false) {
                a.push(data.node.id.split('_')[0] + ";" + data.node.id.split('_')[1] + ";" + data.node.state.selected + ";" + partrelationKey);
            }

            localStorage.setItem('MPL', JSON.stringify(a));

        } catch (e) {

        }
    });


function SubmitBaseLinePartsChanges() {
    var blenginedbkey = document.getElementById("BaseLineEngineDbkey").value;
    a = JSON.parse(localStorage.getItem('MPL')) || [];

    if (a.length == 0) {
        bootbox.alert('Please make changes and submit', function () { });
    }
    else {
        $.ajax({
            type: "POST",
            url: '/MPL/ManageBaselineEngineParts',
            data: {
                MPL: JSON.stringify(a),
                Engine_Dbkey: 0,
                BL_Engine_Dbkey: blenginedbkey
            },
            cache: false,
            success: function (data) {

                if (data.success) {
                    bootbox.alert('Updated Successfully', function () { });
                } else {
                    console.log(data);
                    bootbox.alert('Error', function () { });
                }
            }
        });
    }
}





var dataTable;
function GetMplDataTable(BLE_dbkey, EngineDbkey) {

    console.log(BLE_dbkey, EngineDbkey)

    try {
        dataTable.destroy();
    } catch (e) {
    }

    dataTable = $("#MPLdatatables").DataTable({
        "ajax": {
            "url": "/MPL/GetMplDetailList?BL_dbkey=" + BLE_dbkey + "&EngineDbkey=" + EngineDbkey,
            "type": "GET",
            "dataSrc": "",
            "datatype": "json"
        },
        scrollY: 300,
        paging: false,
        deferRender: true,
        dom: 'Bfrtip',
        buttons: [{
            extend: 'excel',
            title: 'Master Part List',
            //exportOptions: {
            //    columns: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11] //Your Colume value those you want
            //}
        }],

        "columns": [

            { "data": "Engine_Part_Dbkey" },
            { "data": "Type_Part_Name" },
            //{ "data": "Draw_part_no" },
            { "data": "Draw_part_no_original" },
            { "data": "Description" },
            { "data": "Revision" },
            { "data": "Quantity" }, 
            { "data": "RMName" },
            { "data": "Module_Responsibility_" },
            { "data": "FCBP" },
            { "data": "parent_partno" },
            { "data": "Reporting_Type" },
            { "data": "Execution_Resp" },
            //{
            //    "data": "mpldbkey",
            //    "render": function (data, type, row) {
            //        if (type === 'display') {
            //            if (row.is_rm_verified == 1) {
            //                return "<a style='cursor:pointer' class='btn btn-sm btn-success' onclick='VerifyRM(" + row.mpldbkey + ")'>Verified</a>"
            //            } else {
            //                return "<a style='cursor:pointer' class='btn btn-sm btn-warning' onclick='VerifyRM(" + row.mpldbkey + ")'>Verify</a>"
            //            }

            //        }
            //        return data;
            //    }
            //},
            {
                "data": "mpldbkey",
                "render": function (data, type, row) {
                    // 'data' is the data for the cell
                    // 'type' is the display type ('display', 'filter', 'type', or 'sort')
                    // 'row' is the data for the whole row
                    if (type === 'display') {
                        // Format the date for display
                        return "<a style='cursor:pointer' class='btn btn-sm btn-success' onclick='AdditionalPartsInfo(" + row.mpldbkey + "," + row.Part_relation_dbkey + ")'>++Info</a>"
                    }
                    return data;
                }
            },

            {
                "data": "mpldbkey",
                "render": function (data, type, row) {
                    // 'data' is the data for the cell
                    // 'type' is the display type ('display', 'filter', 'type', or 'sort')
                    // 'row' is the data for the whole row
                    if (type === 'display') {
                        // Format the date for display
                        return "<a style='cursor:pointer' class='btn btn-sm btn-success' onclick='PBSInfo(" + row.mpldbkey + "," + row.Part_relation_dbkey + ")'>PBS Info</a>"
                    }
                    return data;
                }
            }
        ],
        "initComplete": function (settings, json) {

        }


    });

    //$('.DT-search .form-control').attr('placeholder', 'Search...');
    //$('.DT-search .form-control').attr('class', 'DT-search form-control');
    //$('.DT-lf-right').attr('class', 'pull-left');
    //$('.dataTables_length').attr('class', 'dataTables_length DT-lf-right');

}

function AdjustDatatable() {
    dataTable.columns.adjust();
}


function ViewChangesOnMPL(url) {
    $.get(url)
        .done(function (response) {
            bootbox.dialog({
                title: "Action Logs",
                message: response,
                size: 'extra-large',
                closeButton: true,
                className: 'custom-modal',
            });
        });
}


function UploadManufacturingStatus() {
    var url = "";
    $.get(url)
        .done(function (response) {
            bootbox.dialog({
                title: "Action Logs",
                message: response,
                closeButton: true,
                className: 'custom-modal',
            });
        });
}


function GetUploadMfgStatusExcel() {
    var url = '/ExcelUpload/Index?basetable=JsonVendorComponentDetail';
    $.get(url).done(function (response) {
        bootbox.alert({
            message: response,
            title: "Upload Manufacturing Status",
            size: 'Small',
            buttons: {
                ok: {
                    className: 'd-none'
                }
            }
        });
    });
}


function PostUploadMfgStatusExcel() {
    var uploadFileCtrl = document.getElementById("MfgExcelFile");


    if (uploadFileCtrl.files.length == 0) {
        $(uploadFileCtrl.parentNode).notify("Please select file to upload", {
            // globalPosition: "top center",
            className: "danger"
        });
        return false;
    }

    var attachmentVM = new FormData();
    var Source_table_key = 0;
    var Source_table = document.getElementById("Source_table");


    attachmentVM.append('uploadeddocument', uploadFileCtrl.files[0]);
    attachmentVM.append('Source_table_key', Source_table_key.value);
    attachmentVM.append('Source_table', Source_table.value);



    $.ajax({
        type: "POST",
        url: '/Attachment/UploadExcel',
        /*    async: true,*/
        data: attachmentVM,
        cache: false,
        contentType: false,
        processData: false,
        cache: false,
        success: function (data) {
            if (data.success) {
                $(uploadFileCtrl.parentNode).notify(data.msg, {
                    className: "success"
                });
            } else {
                $(uploadFileCtrl.parentNode).notify(data.msg, {
                    className: "danger"
                });
            }
        }
    });
}

let MPLdialog;
function CreateEngineMasterPart(EnginePartDbkey, formAction, displaytype) {
    // formAction -- [Create under selected part, Create with existing part , Edit selected Part]
    var url;

    if (displaytype == "Readonly") {
        url = '/MPL/ViewMasterEnginePart?PartDbkey_PartRelationKey=' + EnginePartDbkey + "&formAction=" + formAction + "&displayType=" + displaytype;
    }
    else {
        url = '/MPL/MasterEnginePart?PartDbkey_PartRelationKey=' + EnginePartDbkey + "&formAction=" + formAction + "&displayType=" + displaytype;
    }

    $.get(url)
        .done(function (response) {
            MPLdialog = bootbox.dialog({
                title: "Engine Master Part",
                message: response,
                size: 'extra-large',
                closeButton: true,
                className: 'custom-modal'
            });

            MPLdialog.on('shown.bs.modal', function () {
                afterDialogRendered();
            });

        });
}


var dttable;
function afterDialogRendered() {
    try {
        dttable = $('#MPL_Existing').DataTable(
            {
                scrollY: '400px',
                scrollCollapse: true,
                paging: false,
                ordering: false,
                deferRender: true,
            }
        );
    } catch (e) {

    }

}



function ValidatePartsSelect(PartCheckStatus, partDbkey, Part_relation_dbkey) {
    dttable.search('').columns().search('').draw();
    document.getElementById("CustomValidationMessageforPartselected").innerText = "";
    document.getElementById("Engine_Part_Dbkey").value = 0;
    document.getElementById("Part_relation_dbkey").value = 0;
    var table = document.getElementById("MPL_Existing");
    var rows = table.getElementsByTagName("tr");

    for (var i = 1; i < rows.length; i++) {
        if (PartCheckStatus.checked == true) {
            table.rows[i].cells[0].children[0].disabled = true;
            document.getElementById("Engine_Part_Dbkey").value = partDbkey;
            document.getElementById("Part_relation_dbkey").value = Part_relation_dbkey;
            if (table.rows[i].cells[0].children[0].checked == true) {
                document.getElementById("CustomValidationMessageforPartselected").innerText = table.rows[i].cells[1].innerText;
            }

        } else {
            table.rows[i].cells[0].children[0].disabled = false;
        }
    }
    PartCheckStatus.disabled = false;
}


function SaveEnginePart() {

    document.getElementById("lnk_save_mpl").disabled = true;
    $.validator.unobtrusive.parse("form");
    if ($('#EnginePartMasterForm').valid()) {
        $.ajax({
            type: "POST",
            url: "/MPL/MasterEnginePart",
            data: $('#EnginePartMasterForm').serialize(),
            cache: false,
            success: function (data) {
                if (data.success) {
                    bootbox.alert('Updated Successfully', function () { });
                } else {
                    bootbox.alert('Encountered an error!', function () { document.getElementById("lnk_save_mpl").disabled = false; });
                }
            }
        });
    } else {
        document.getElementById("lnk_save_mpl").disabled = false;
    }
    return false;
}



function SaveEnginepartsinfoWithExistingParts() {
    document.getElementById("lnk_save_mpl").disabled = true;

    if (document.getElementById("Approver_ID").value == 0) {
        document.getElementById("validateMsg_Approver_ID").innerText = "Required";
        document.getElementById("lnk_save_mpl").disabled = false;
        return false;
    }

    if (document.getElementById("Engine_Part_Dbkey").value == 0) {
        document.getElementById("CustomValidationMessageforPartselected").innerText = "Please select atleast one part.";
        document.getElementById("lnk_save_mpl").disabled = false;
        return false;
    }

    /* alert(document.getElementById("Approver_ID").value);*/
    $.ajax({
        type: "POST",
        url: "/MPL/CreateEnginepartsWithExisting",
        data: $('#MPLExistingPartCreateForm').serialize(),
        cache: false,
        success: function (data) {
            if (data.success) {
                bootbox.alert('Created Successfully', function () { window.location.reload() });
            } else {
                bootbox.alert('Encountered an error!', function () { document.getElementById("lnk_save_mpl").disabled = false; });
            }
        }
    });
    return false;
}

function CheckFormMode() {
    console.log(document.getElementById("DisplayType").value);
    if (document.getElementById("DisplayType").value == "Readonly") {
        $("#EnginePartMasterForm :input").attr("disabled", true);
        $("#EnginePartMasterForm :textarea").attr("disabled", true);
        $("#EnginePartMasterForm :select").attr("disabled", true);

    }
}

function CallPreLoadDatas() {

    var recentChangesUrl = '/SharedCall/Audit_Logs?viewID=1&partID=' + document.getElementById("Engine_Part_Dbkey").value;
    var materialIssueListUrl = '/MPL/MaterialIssues?Engine_Part_Dbkey=' + document.getElementById("Engine_Part_Dbkey").value;
    var AttachMentsUrl = '/Attachment/ViewAttachments?Source_table=Engine_Parts_Master&Source_table_key=' + document.getElementById("Engine_Part_Dbkey").value;

    $.get(recentChangesUrl).done(function (response) {
        $('#RecentChanges').html(response)
    });

    $.get(materialIssueListUrl).done(function (response) {
        $('#MaterialIssues').html(response)
    });

    $.get(AttachMentsUrl).done(function (response) {
        $('#attachments').html(response)
    });

    CheckFormMode();
}

function AdjustauditlogDatatable() {
    auditlogsdataTable.columns.adjust();
}


function UploadFile(ID, revision) {

    if (document.getElementById("Approver_ID").value == 0) {
        bootbox.alert('Please select an appropriate approver to proceed !', function () { });
        return false;
    }

    var Drawing_File = $("#uploadedFile").get(0);

    if (Drawing_File.files.length == 0) {
        bootbox.alert('Please Select the file !', function () { });
        return false;
    }


    var fileData = new FormData();

    if (Drawing_File != null) {
        for (var i = 0; i < Drawing_File.files.length; i++) {
            fileData.append('uploadeddocument', Drawing_File.files[i]);
        }
    }

    // Adding one more key to FormData object
    fileData.append('Source_table_key', ID);
    fileData.append('Revision', revision);
    fileData.append('File_DVD_Num', document.getElementById("dvdnumber").value);
    fileData.append('Source_table', 'Engine_Parts_Master');
    fileData.append('Attachment_type', document.getElementById("filetype").value);
    fileData.append('File_Revision', document.getElementById("filerevision").value);
    fileData.append('Approver', document.getElementById("Approver_ID").value);
    fileData.append('Part_number', document.getElementById("Draw_part_no").value);

    $.ajax({
        url: "/Attachment/UploadFiles",
        type: 'POST',
        data: fileData,
        success: function (data) {
            if (data.success) {
                document.getElementById('uploadattcahment').disabled = true;
                CallPreLoadDatasOnlyDocs();
                bootbox.alert('Uploaded Successfully', function () { });
            }
        },
        cache: false,
        contentType: false,
        processData: false
    });
}

function CallPreLoadDatasOnlyDocs() {
    var AttachMentsUrl = '/Attachment/ViewAttachments?Source_table=Engine_Parts_Master&Source_table_key=' + document.getElementById("Engine_Part_Dbkey").value;
    $.get(AttachMentsUrl).done(function (response) {
        $('#attachments').html(response)
    });

}


function AdditionalPartsInfo(EnginePartDbkey, PartRelationDbkey) {

    var Url = '/MPL/UpdatePartAdditionalInfo?PartDbkey=' + EnginePartDbkey + "&PartRelationkey=" + PartRelationDbkey;
    $.get(Url).done(function (response) {
        bootbox.dialog({
            title: "Update Additional Information",
            message: response,
            size: 'large',
            closeButton: true
        });
    });
}



function saveAdnlInfo() {

    var Engine_PartsVMdata = {};
    Engine_PartsVMdata.Reporting_Type = document.getElementById("Reporting_Type").value;
    Engine_PartsVMdata.Execution_Resp = document.getElementById("Execution_Resp").value;
    Engine_PartsVMdata.Engine_Part_Dbkey = document.getElementById("Engine_Part_Dbkey").value;
    Engine_PartsVMdata.Parent_id = document.getElementById("Parent_id").value;
    Engine_PartsVMdata.AssemblyReportingType = document.getElementById("AssemblyReportingType").value;
    Engine_PartsVMdata.ReportDisplayOrder = document.getElementById("ReportDisplayOrder").value;
    Engine_PartsVMdata.Part_relation_dbkey = document.getElementById("Part_relation_dbkey").value;
    Engine_PartsVMdata.Reporting_Parent = document.getElementById("Reporting_Parent").value;
    Engine_PartsVMdata.Part_Remarks = document.getElementById("Part_Remarks").value;
    Engine_PartsVMdata.ManufacturingComments = document.getElementById("ManufacturingComments").value;
    Engine_PartsVMdata.Execution_Resp_additionalLevel = document.getElementById("Execution_Resp_additionalLevel").value;
    Engine_PartsVMdata.CollaboratorsId = $('#CollaboratorArr option:selected').toArray().map(item => item.value).join();
    Engine_PartsVMdata.Collaborators = $('#CollaboratorArr option:selected').toArray().map(item => item.text).join();
    //  var url = '/Engines/Engine/SaveAdditionalPartInfo?Engine_Part_Dbkey=' + Engine_Part_Dbkey + '&Execution_Resp=' + Execution_Resp + '&Reporting_Type=' + Reporting_Type;

    //  alert($('#CollaboratorsId option:selected').toArray().map(item => item.id).join());


    $.ajax({
        type: "POST",
        url: '/MPL/SaveAdditionalPartInfo',
        data: {
            engine_PartsVM: Engine_PartsVMdata
        },
        cache: false,
        success: (function (response) {
            if (response.success) {
                bootbox.alert('Updated Successfully', function () {
                    var btn = document.getElementsByClassName("bootbox-close-button")[0];
                    btn.click();
                });

            } else {
                bootbox.alert('Save failed !', function () { });
            }

        }),
    });

}



function ViewMfgStatus(id) {

    var res = id.split("_")[1];

    var url = '/MPL/ViewMfgStatus?PartRelationKey=' + res
    $.get(url)
        .done(function (response) {
            bootbox.dialog({
                title: 'Manufacturing Status',
                message: response,
                size: 'large',
            });
        });
}

function RemoveAttachment(AttachmentKey) {
    var bootboxDeleteDlg = bootbox.confirm({
        message: "Are you sure you wish to delete this file?",
        centerVertical: true,
        buttons: {
            confirm: {
                label: 'Yes',
                className: 'btn-success'
            },
            cancel: {
                label: 'No',
                className: 'btn-danger'
            }
        },
        callback: function (result) {
            if (result) {
                $.ajax({
                    async: false,
                    url: '/Attachment/DeleteAttachment/' + AttachmentKey,
                    type: 'GET',
                    success: function (data) {
                        bootbox.alert('Removed');
                        CallPreLoadDatas();
                    }
                });
            }

        }
    });

}
var dialog2;
function ManufacturingProcessDocumentList() {
    var partDbkey = document.getElementById("Engine_Part_Dbkey").value;
    var url = "/MPL/ManufacturingProcessRequiredDocuments?enginePartDbkey=" + partDbkey;
    $.get(url)
        .done(function (response) {
            //bootbox.dialog({
            //    title: 'Manufacturing Status',
            //    message: response,
            //    size: 'medium',
            //}); 
            dialog2 = xdialog.open({
                title: 'Casting documents checklist',
                body: response,
                buttons: ['ok', 'cancel'],
                modal: true,
                aftercreate: function () {
                    document.getElementById("button-saveManufacturingProcessDocumentList").style.display = "none";
                },
                onok: function () {
                    saveManufacturingProcessDocumentList();
                    //return false;
                }
            });
            // dialog2.show();
            //dialog2.hide();
            //dialog2.destroy();  
        });
}
function saveManufacturingProcessDocumentList() {

    var requiredDocuments = [];
    var checkBoxes = document.getElementsByClassName("required-docs");
    for (i = 0; i < checkBoxes.length; i++) {
        var requiredDocument = {};
        requiredDocument.id = $(checkBoxes[i]).attr("data-dbkey");
        requiredDocument.AttachmentTypeKey = $(checkBoxes[i]).attr("data-typekey");
        requiredDocument.Part_DbKey = $(checkBoxes[i]).attr("data-partdbkey");
        requiredDocument.Attachment_Type = $(checkBoxes[i]).attr("data-filename");
        requiredDocument.Required = checkBoxes[i].checked;
        requiredDocuments.push(requiredDocument);
    }

    $.ajax({
        type: "POST",
        url: '/MPL/ManufacturingProcessRequiredDocuments',
        data: JSON.stringify(requiredDocuments),
        cache: false,
        // processData: false,
        contentType: 'application/json',
        success: (function (response) {
            if (response.success) {
                dialog2.destroy();
                alert('Updated Successfully');
            } else {
                alert('Something went wrong. Please try again later');
            }

            return false;
        }),
        error: (function (error) {
            console.log(error);
            alert('Something went wrong. Please try again later');
        })
    });

}

function ShowExtractedFiles(attachmentDbkey) {
    var url = '/DownloadFiles/ShowExtractedFiles?fileKey=' + attachmentDbkey + '&SourceTable=Attachment';
    $.get(url).done(function (response) {
        //  console.log(response);
        bootbox.dialog({
            title: 'Extracted Files',
            message: response,
            size: 'large',
            closeButton: true,
        });
    }).fail(function (xhr, status, error) {
        console.error("Error: ", error);
        console.error("Status: ", status);
        console.error("Response: ", xhr.responseText);
    });
}
function viewExtractedFiles(path, filename, docID) {
    $.ajax({
        url: '/DownloadFiles/ViewExtractedFiles?path=' + path + '&fileName=' + filename + '&docID=' + docID,
        dataType: 'html',
        success: function (data) {
            bootbox.dialog({
                message: data,
                size: 'large',
                closeButton: true,
                className: 'custom-modal',
            });
        }
    });
}


function LoadMPLJSTreeTbl() {
    var BLE_dbkey = document.getElementById("BaseLineEngineDbkey").value;

    $("#searchMPLTreeTbl").on("click keydown", function (event) {        
        if (event.type === "click" || (event.type === "keydown" && event.key === "Enter")) {

            var searchString = $(".search-input-treeTbl").val();
            //console.log("Search input:", searchString);
            if (searchString.length > 3) {
                let LoadingDialog = bootbox.dialog({
                    message: `
            <p class="text-center mb-0">
                <img src="/assets/img/searching_doc.gif" alt="Loading..." width="200" height="200">
            </p>
            <p class="text-center mb-0">Searching relevant MPL...</p>`,
                    closeButton: false
                });
                setTimeout(() => {
                    $('#mplJSTreeTbl').jstree('search', searchString);
                    bootbox.hideAll();
                    // console.log("Search performed and loading dialog closed.");
                }, 2000);
            } else {
                //console.log("Search input is too short:", searchString, "(length:", searchString.length, ")");

                alert("Please enter more than 4 characters for the search.");

            }
        }
    });
    var nodelog = $('div#mplJSTreeTbl').jstree({
        plugins: ["table", "search", "contextmenu","state"],
        //plugins: [ "search", "contextmenu"],
        themes: {
            name: "default",
            icons: true
        },
        "core": {
            "data": function (node, cb) {
                if (node.id === "#") {
                    // Load root nodes
                    $.get("/MPL/MPLJsonData?baselineenginedbkey=" + BLE_dbkey, function (data) {
                        cb(data);
                    });
                }
                //else {
                //    // Load child nodes dynamically
                //    $.get("/MPL/MPLJsonData?parentNodeId=" + node.id, function (data) {
                //        cb(data);
                //    });
                //}
            },
            "check_callback": true,
            "themes": {
                "responsive": false
            }
        },
        "state": {
            key: "jstree-state" // Optional: Persist open/close state
        },
        "search": {
            case_insensitive: true,
            show_only_matches: true,
            show_only_matches_children: true,
            search_callback: function (searchString, node) {
                // Search only on the tree column (node.text)
                return node.text && node.text.toLowerCase().includes(searchString.toLowerCase());
            }
        },

        // Table Plugin with Lazy Loading
        "table": {
            columns: [
                { width: "auto", header: "MPL", title: "_DATA_" },
                { width: "auto", value: "qty_per_Engine", header: "Qty", valueClass: "spanclass" },
                { width: "auto", value: "rmName", header: "RM", valueClass: "spanclass" },
                { width: "auto", value: "moduleResponsibility", header: "Mod Resp", valueClass: "spanclass" },
                { width: "auto", value: "fcbp", header: "F/C/B/P", valueClass: "spanclass" },
                { width: "auto", value: "reporting_Type", header: "Rep type", valueClass: "spanclass" },
                { width: "auto", value: "execution_Resp", header: "Exc Res", valueClass: "spanclass" },
            ],
            resizable: true,
            fixed: true,
            minWidth: 100
        },
        "contextmenu": {
            'show_at_node': true,
            "items": function ($node) {
                return {
                    "View": {
                        "label": "View",
                        "action": function (obj) {
                            CreateEngineMasterPart($node.id, "Edit", "Readonly");
                        }
                    },
                    "Materialissue": {
                        "label": "Material issue",
                        "action": function (obj) {
                            GetCreatepopUpMaterialIssue($node.id, 1, "Readonly");
                        }
                    },
                    "MfgStatus": {
                        "label": "Mfg Status",
                        "action": function (obj) {
                            ViewMfgStatus($node.id, 1, "Readonly");
                        }
                    },
                    "addInfo": {
                        "label": "Add Info",
                        "action": function (obj) {
                            AdditionalPartsInfoTreeView($node.id, 1, "Readonly");
                        }
                    },
                };
            }
        },

    });
    setTimeout(function () {
        countTotalNodestbletree();
    }, 1000); // Adjust the timeout as needed

    $('#mplJSTreeTbl').on('before_close.jstree', function (e, data) {
        return false; // Prevent collapse
    });
   
    
}
function AdditionalPartsInfoTreeView(id) {
    var enginePartDbkey = id.split("_")[0];
    var partRelationDbkey = id.split("_")[1];
    AdditionalPartsInfo(enginePartDbkey, partRelationDbkey);

}
function countTotalNodestbletree() {
    // Get the entire tree in JSON format
    const treeData = $('#mplJSTreeTbl').jstree(true).get_json('#', { flat: true });

    let totalNodes = 0;
    treeData.forEach(node => {
        // Count nodes recursively for each node
        totalNodes += countNodes(node);
    });
    console.log(totalNodes);

    document.getElementById("totalCompCountTreetbl").innerHTML = "Total Count = " + (totalNodes - 1);

}

function ViewInspectinReports(id) {

    var res = id.split("_")[1];

    var url = '/MSAccess/GetInspectionReport?id=' + res
    $.get(url)
        .done(function (response) {
            bootbox.dialog({
                title: 'Inspection Reports',
                message: response,
                size: 'large',
            });
        });

}

var uniqueMPLPartsDatatbl;
function GetMplUniquePartsTbl(BLE_dbkey, EngineDbkey) { 

    try {
        uniqueMPLPartsDatatbl.destroy();
    } catch (e) {
    }

    uniqueMPLPartsDatatbl = $("#mplUniquePartsTbl").DataTable({
        "ajax": {
            "url": "/MPL/GetUniqueMplPartList?BL_dbkey=" + BLE_dbkey + "&EngineDbkey=" + EngineDbkey,
            "type": "GET",
            "dataSrc": "",
            "datatype": "json"
        },
        scrollY: 400,
        paging: false,
        deferRender: true,
        dom: 'Bfrtip',
        buttons: [{
            extend: 'excel',
            title: 'Unique Master Part List', 
        }],

        "columns": [  
            { "data": "Type_Part_Name" }, 
            { "data": "Draw_part_no_original" },
            { "data": "Description" },
            { "data": "Revision" },
            { "data": "Quantity" },
            { "data": "RMName" },
            { "data": "Module_Responsibility_" },
            { "data": "FCBP" },
            { "data": "parent_partno" },
            { "data": "Reporting_Type" },
            { "data": "Execution_Resp" }, 
        ],
        "initComplete": function (settings, json) {

        } 
    }); 

}


function PBSInfo(EnginePartDbkey, PartRelationDbkey) {

    var Url = '/MPL/PBSInfo?PartDbkey=' + EnginePartDbkey + "&PartRelationkey=" + PartRelationDbkey;
    $.get(Url).done(function (response) {
        bootbox.dialog({
            title: "Update Part Breakdown Structure Info",
            message: response,
            size: 'large',
            closeButton: true
        });
    });
}

function SavePBSInfo() {
    // Get form values
    var partDbkey = $('#partDbkey').val();
    var partRelationDbkey = $('#partRelationDbkey').val();
    var modulePBS = $('#Module_PBS').val();
    var partTypePBS = $('#PartType_PBS').val();

    // Basic validation
    if (modulePBS == "0" || modulePBS == "") {
        alert("Please select a Module");
        return;
    }

    if (partTypePBS == "0" || partTypePBS == "") {
        alert("Please select a Part Type");
        return;
    }

    // Prepare data object
    var data = {
        PartDbkey: partDbkey,
        PartRelationDbkey: partRelationDbkey,
        ModulePBS: modulePBS,
        PartTypePBS: partTypePBS
    };

    // Make AJAX call
    $.ajax({
        url: '/MPL/SavePBSInfo',
        type: 'POST',
        data: data,
        success: function (response) {
            if (response.success) {
                alert("PBS Info saved successfully!");
                $('.bootbox').modal('hide');
                // Refresh the datatable
                dataTable.ajax.reload();
            } else {
                alert("Error: " + response.message);
            }
        },
        error: function () {
            alert("An error occurred while saving PBS Info");
        }
    });
}

 