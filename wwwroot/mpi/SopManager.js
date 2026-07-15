
//This Scripts for both MPL View and Revision Managements ;
//Based on View the logics are change;

$(document).ready(function () {
    document.getElementById("Div_ComponentDetail").innerHTML = '';
    /*  console.log(document.getElementById("isCustomAccess").value);*/
    if (document.getElementById("isCustomAccess").value == "true") {
        document.getElementById("navbarTop").innerHTML = "";
    }

    //console.log('Checked isCustomAccess')
});

var jc;
var Editdialog;
$(function () {

    //  console.log('Started LoadJstree')
    LoadJstree('All');

});

function LoadJstree(filter, IsActive = "Active") {
    $(".search-input").keyup(function () {
        var searchString = $(this).val();
        $('#jstree').jstree('search', searchString);
        // If search is cleared, hide the no results message
        if (searchString === '') {
            $('#noResultsMessage').hide();
        }
    });
    var BuildGuid = document.getElementById("BuildGuid").value;
    var customAccessData = document.getElementById("CustomAccessData").value;
    if ($('#jstree').jstree(true)) {
        // Destroy the existing jstree instance
        $('#jstree').jstree('destroy');
    }
    $.ajax({
        type: "Get",
        url: "/SOPManagement/GetBuildJsTreeData?buidguid=" + BuildGuid + "&filter=" + filter + "&activestatus=" + IsActive + "&customAccessJsonData=" + customAccessData,
        success: function (data) {
            //   console.log(data);
            var nodelog = $('#jstree').jstree(
                {
                    'core': {
                        'data': data,
                    },
                    "search": {
                        "case_insensitive": true,
                        "show_only_matches": true,
                        "show_only_matches_children": false
                    },

                    //"plugins": ["html_data", "dnd", "contextmenu", "search", "types", "adv_search", "themes"],
                    "plugins": ["html_data", "contextmenu", "dnd", "search", "types", "adv_search"],

                    "contextmenu": {
                        "items": function ($node) {
                            return {
                                "Edit": {
                                    "label": "Add New",
                                    "action": function (obj) {
                                        AddSopPart(0, $node.id);
                                    }
                                },
                                "AddWithExisting": {
                                    "label": "Add with Existing Part",
                                    "icon": "fa fa-plus-circle",
                                    "action": function (obj) {
                                        AddSopPartWithExisting(0, $node.id);
                                    }
                                },
                                "RefreshFromMPL": {
                                    "label": "Refresh from MPL",
                                    "icon": "fa fa-sync",
                                    "action": function (obj) {
                                        RefreshComponentFromMPL($node.id);
                                    }
                                },
                            };
                        }
                    },
                }).on('changed.jstree', function (e, data) {
                    var nodedata = data.instance.get_node(data.selected[0]).id;
                    var buildId = nodedata.split("_")[1];
                    GetComponentDetail(buildId);
                    applyTextAreaContextMenuForSerialNo();
                });

            // Listen for search completion
            $('#jstree').on('search.jstree', function (e, data) {
                var searchString = $('.search-input').val();
                if (searchString !== '') {
                    // Check the search results - data.res contains the matching node IDs
                    if (data.res.length === 0) {
                        $('#noResultsMessage').show();
                        $('#jstree').hide(); // Hide the tree when no results
                    } else {
                        $('#noResultsMessage').hide();
                        $('#jstree').show(); // Show the tree when there are results
                    }
                } else {
                    // When search is cleared
                    $('#noResultsMessage').hide();
                    $('#jstree').show();
                }
            });

            if (filter != 'All') {
                $('#jstree').on("ready.jstree", function (e, data) {
                    $('#jstree').jstree('search', filter);
                });
            }

            setTimeout(function () {
                countTotalNodes();
            }, 1000); // Adjust the timeout as needed


        },
        error: function (data) {
            console.error('Error while loading component tree');
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
    const treeData = $('#jstree').jstree(true).get_json('#', { flat: true });

    let totalNodes = 0;
    treeData.forEach(node => {
        // Count nodes recursively for each node
        totalNodes += countNodes(node);
    });
    try {
        document.getElementById("totalCompCount").innerHTML = "Total Count = " + (totalNodes - 1);
    } catch (e) {

    }

}


function enableReportingParent() {
    $('#ReportingParent').prop('disabled', false);
    $('#ReportingParent').select2({
        dropdownParent: $('#formSOPupdate')
    });
}

function GetComponentDetail(id) {
    var customAccessData = document.getElementById("CustomAccessData");
    var IsUnderCustomAccess = document.getElementById("isCustomAccess").value;
    if (customAccessData.value != "") {
        var jsonobj = JSON.parse(customAccessData.value);
        var csvString = jsonobj.modules;
        var itemsArray = csvString.split(',');

        let isPresent = false;
        for (var i = 0; i < itemsArray.length; i++) {
            if (itemsArray[i] != "") {
                var compdbkey = itemsArray[i].split("_")[1];
                if (compdbkey == id) {
                    isPresent = true;
                    break;
                }
            }
        }
        //jsonObj.modules.forEach(number => { 
        //    if (number == id) {
        //        isPresent = true;
        //        return true;
        //    }
        //});  
        if (isPresent == false) {
            document.getElementById("Div_ComponentDetail").innerHTML = "<h3 class='text-danger'>Access Denied</h3>";
            return false;
        }
    }

    var url = "/SOPManagement/UpdatePartDetail?Id=" + id + "&customAccess=" + IsUnderCustomAccess;
    $.get(url).done(function (response) {
        document.getElementById("Div_ComponentDetail").innerHTML = response;
        LoadMPLDocuments(id);

    });
}


function verifyCustomAccess(id) {
    var customAccessData = document.getElementById("CustomAccessData");
    if (customAccessData.value != "") {
        var jsonobj = JSON.parse(customAccessData.value);
        var csvString = jsonobj.modules;
        var itemsArray = csvString.split(',');

        let isPresent = false;
        for (var i = 0; i < itemsArray.length; i++) {
            if (itemsArray[i] != "") {
                var compdbkey = itemsArray[i].split("_")[1];
                if (compdbkey == id) {
                    isPresent = true;
                    break;
                }
            }
        }
        return isPresent;
    }


}


function LoadMPLDocuments(id) {
    var url = "/Attachment/GetMPLPartDocuments?itemKey=" + id + "&AllRevs=true";
    $.get(url).done(function (response) {
        document.getElementById("tab-MPLDoc").innerHTML = response;
    });
}

function applyTextAreaContextMenuForSerialNo() {
    $.contextMenu({
        selector: '.buildSerialNo',
        callback: function (key, options) {
            if (key === "insertSerialNo") {
                bootbox.dialog({
                    title: "Enter serial number range",
                    message: `
                </div>
                <div style="display: flex; flex-wrap: wrap;">
                <div style="margin-right: 10px;">
                    <label for="prefix">Prefix: </label>
                    <input type="text" id="prefix" class="form-control">
                </div>
                <div style="margin-right: 10px;">
                    <label for="suffix">Suffix: </label>
                    <input type="text" id="suffix" class="form-control">
                </div>
                <div style="margin-right: 10px;">
                    <label for="startRange">Start Range: </label>
                    <input type="number" id="startRange" class="form-control">
                </div>
                <div>
                    <label for="endRange">End Range: </label>
                    <input type="number" id="endRange" class="form-control">
                </div>
            </div>
            <div id="generatedslno">
         
            </div>
        `,
                    buttons: {
                        cancel: {
                            label: 'Cancel',
                            className: 'btn-secondary'
                        },
                        confirm: {
                            label: 'Generate',
                            className: 'btn-primary',
                            callback: function () {
                                // Grab the values from the inputs
                                var prefix = $('#prefix').val();
                                var suffix = $('#suffix').val();
                                var startRangeStr = String($('#startRange').val());
                                var endRangeStr = String($('#endRange').val());

                                // Validate the inputs
                                if (startRangeStr !== '' && endRangeStr !== '') {
                                    var rangeArray = [];

                                    // Check if the startRangeStr starts with '0' (indicating padding is needed)
                                    var paddingNeeded = startRangeStr.startsWith('0');

                                    // If padding is needed, calculate the number of digits for padding
                                    var numDigits = 0;
                                    if (paddingNeeded) {
                                        numDigits = Math.max(startRangeStr.length, endRangeStr.length);
                                    }

                                    // Loop over the range and pad the numbers with leading zeros (if needed)
                                    for (var i = parseInt(startRangeStr, 10); i <= parseInt(endRangeStr, 10); i++) {
                                        var paddedNumber = String(i);

                                        // Apply padding only if it's needed
                                        if (paddingNeeded) {
                                            paddedNumber = paddedNumber.padStart(numDigits, '0');
                                        }

                                        rangeArray.push(prefix + paddedNumber + suffix);
                                    }

                                    // Join the range array into a comma-separated string
                                    var resultString = rangeArray.join(', ');

                                    // Generate the editable input and save button
                                    // var editSlno = '<input id="slnos" class="form-control">';
                                    // var saveButton = '</br> <a class="btn btn-sm btn-success" onclick="saveNewSlno(' +  + ')">Save</a>';
                                    // var textarea =
                                    document.getElementsByClassName('buildSerialNo')[0].value = resultString;
                                    // console.log(textarea);
                                    // Display the generated values and input
                                    // document.getElementById('generatedslno').innerHTML = "Generated Values: " + editSlno + saveButton;
                                    // document.getElementById('slnos').value = resultString;

                                } else {
                                    alert("Please fill out all fields with valid data.");
                                }

                                //  return false;
                            }
                        }
                    }
                });
            }
        },
        items: {
            "insertSerialNo": {
                name: "Insert serial no range",
                icon: function () {
                    return 'bi bi-list-ol';
                }
            }
        }
    });
}



function SubmitComponentDetail(ctrl) {
    $('#ReportingParent').prop('disabled', false);
    var form = $(ctrl).closest("form");
    document.getElementById("btnCompSubmit").disabled = true;
    $.validator.unobtrusive.parse("form");
    console.log($(form).valid());
    if ($(form).valid()) {
        $.ajax({
            type: "POST",
            url: '/SOPManagement/UpdatePartDetail',
            data: $(form).serialize(),
            cache: false,
            success: function (data) {
                var message = data.msg;
                if (data.success) {
                    GetComponentDetail(data.id)
                } else {
                    bootbox.alert({
                        message: message,
                        size: 'medium',
                        callback: function (result) {
                            GetComponentDetail(data.id);
                        }
                    });
                }
            }
        });
    } else {
        document.getElementById("btnCompSubmit").disabled = false;
    }

}

function AddSopPart(Id, parentId) {

    if (document.getElementById("isCustomAccess").value == "true") {
        alert("You are not authorized to perform this action.")
        return false;
    }
    var url = '/SOPManagement/AdditionalSopBuildComponent?Id=' + Id + '&parentid=' + parentId.split("_")[1];
    $.get(url).done(function (response) {
        1
        bootbox.alert({
            message: response,
            title: "SOP - Additional Build Component",
            size: 'Small',
            buttons: {
                ok: {
                    className: 'd-none'
                }
            }
        });
    });
}


function SaveAdditionalSopBuildComponent(form) {
    document.getElementById("lnk_save_mpl").disabled = true;
    $.validator.unobtrusive.parse("form");
    if ($('#formAdditionalComp').valid()) {
        $.ajax({
            type: "POST",
            url: '/SOPManagement/AdditionalSopBuildComponent',
            data: $('#formAdditionalComp').serialize(),
            cache: false,
            success: function (data) {
                if (data.success) {
                    bootbox.alert({
                        message: 'Submitted Successfully',
                        callback: function () {
                            console.log('This was logged in the callback!');
                        }
                    });
                } else {
                    bootbox.alert({
                        message: 'Failed !',
                        callback: function () {
                            console.log('This was logged in the callback!');
                        }
                    });
                    document.getElementById("lnk_save_mpl").disabled = false;
                }
            }
        });
    } else {
        document.getElementById("lnk_save_mpl").disabled = false;
    }

    return false;
};

function SaveEnginepartsinfoWithExistingParts(form) {

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


    $.ajax({
        type: "POST",
        url: form.action,
        data: $(form).serialize(),
        cache: false,
        success: function (data) {
            if (data.success) {

                $.confirm({
                    icon: 'fa fa-check',
                    typeAnimated: true,
                    theme: 'modern',
                    type: 'dark',
                    title: 'Success',
                    content: data.Msg,
                    buttons: {
                        Close: {
                            text: 'Close',
                            btnClass: 'btn btn-danger btn-sm',
                            action: function () {
                                window.location.reload();
                            }
                        }
                    }
                })

            } else {

                $.confirm({
                    icon: 'fa fa-times',
                    typeAnimated: true,
                    theme: 'modern',
                    type: 'red',
                    title: 'Encountered an error!',
                    content: data.Msg,
                    buttons: {
                        Close: {
                            text: 'Close',
                            btnClass: 'btn btn-danger btn-sm',
                            action: function () {
                                document.getElementById("lnk_save_mpl").disabled = false;
                            }
                        }
                    }
                })
            }
        }
    });


    return false;
};

function VerifyMPL_RM(form) {
    $.validator.unobtrusive.parse("form");
    if ($(form).valid()) {
        $.ajax({
            type: "POST",
            url: form.action,
            data: $(form).serialize(),
            cache: false,
            success: function (data) {
                if (data.success) {
                    $.confirm({
                        icon: 'fa fa-check',
                        typeAnimated: true,
                        theme: 'modern',
                        type: 'dark',
                        title: 'Success',
                        content: data.Msg,
                        buttons: {
                            Close: {
                                text: 'Close',
                                btnClass: 'btn btn-danger btn-sm',
                                action: function () {
                                    window.location.reload();
                                }
                            }
                        }
                    })

                } else {

                    $.confirm({
                        icon: 'fa fa-times',
                        typeAnimated: true,
                        theme: 'modern',
                        type: 'red',
                        title: 'Encountered an error!',
                        content: data.Msg,
                        buttons: {
                            Close: {
                                text: 'Close',
                                btnClass: 'btn btn-danger btn-sm',
                                action: function () {

                                }
                            }
                        }
                    })
                }
            }
        });
    }
    return false;
};

function UploadFile(ID, revision) {



    if (document.getElementById("Approver_ID").value == 0) {
        $.alert({
            title: 'Alert!',
            content: 'Please select an approriate approver to proceed !',
        });
        return false;
    }


    var Drawing_File = $("#uploadedFile").get(0);



    if (Drawing_File.files.length == 0) {
        $.alert({
            title: 'Alert!',
            content: 'Please Select the file !',
        });
        return false;
    }




    var fileData = new FormData();

    if (Drawing_File != null) {
        for (var i = 0; i < Drawing_File.files.length; i++) {
            fileData.append('Files', Drawing_File.files[i]);
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
        url: "/SharedCall/SaveAttachmentsparts",
        type: 'POST',
        data: fileData,
        success: function (data) {
            if (data.success) {
                $.confirm({
                    icon: 'fa fa-check',
                    typeAnimated: true,
                    theme: 'modern',
                    type: 'dark',
                    title: 'Success',
                    content: data.Msg,
                    buttons: {
                        Close: {
                            text: 'Close',
                            btnClass: 'btn btn-danger btn-sm',
                            action: function () {
                                CallPreLoadDatasOnlyDocs();
                            }
                        }
                    }
                })

            }
        },
        cache: false,
        contentType: false,
        processData: false
    });
}

function RemoveAttachment(ID) {
    $.confirm({
        theme: 'modern',
        type: 'dark',
        title: 'Confirmation',
        content: 'Are you sure you wish to remove selected file ?',
        buttons: {
            OK: {
                text: 'OK',
                btnClass: 'btn btn-success btn-sm',
                action: function () {
                    $.ajax({
                        type: "POST",
                        url: "/Engine/DeleteAttachments",
                        data: "id=" + ID,
                        success: function (data) {
                            if (data.success) {
                                $.confirm({
                                    icon: 'fa fa-check',
                                    typeAnimated: true,
                                    theme: 'modern',
                                    type: 'dark',
                                    title: 'Success',
                                    content: 'Removed successfully',
                                    buttons: {
                                        OK: {
                                            text: 'OK',
                                            btnClass: 'btn btn-success btn-sm',
                                            action: function () {
                                                CallPreLoadDatasOnlyDocs();
                                            }
                                        }
                                    }
                                })

                            } else {
                                $.notify(data.message, {
                                    globalPosition: "top center",
                                    className: "error"
                                })

                            }
                        }
                    });
                }
            },
            Cancel: {
                text: 'Cancel',
                btnClass: 'btn btn-danger btn-sm',
                action: function () {

                }

            }
        }
    })
}

function DeletepartNode(id) {

    if (id.split('.')[1] != "P") {
        $.confirm({
            theme: 'modern',
            type: 'dark',
            title: 'Confirmation',
            content: 'Are you sure you wish to Delete !',
            buttons: {
                OK: {
                    text: 'OK',
                    btnClass: 'btn btn-success btn-sm',
                    action: function () {
                        DeleteEnginepart(id);
                    }
                },
                Cancel: {
                    text: 'Cancel',
                    btnClass: 'btn btn-danger btn-sm',
                    action: function () {
                    }
                }
            }
        })
    } else {
        $.alert({
            title: 'Alert!',
            content: 'Master Engine Part node cannot be removed !',
        });
    }

}

function DeleteEnginepart(id) {
    $.ajax({
        type: "POST",
        url: "/Engine/DeleteEngineparts?id=" + id.split('.')[0],
        success: function (data) {
            if (data.success) {
                $.confirm({
                    icon: 'fa fa-check',
                    typeAnimated: true,
                    theme: 'modern',
                    type: 'dark',
                    title: 'Success',
                    content: 'Deleted successfully',
                    buttons: {
                        OK: {
                            text: 'OK',
                            btnClass: 'btn btn-success btn-sm',
                            action: function () {
                                window.location.reload();

                            }
                        }
                    }
                })
            } else {
                $.confirm({
                    icon: 'fa fa-times',
                    typeAnimated: true,
                    theme: 'modern',
                    type: 'red',
                    title: 'Encountered an error!',
                    content: data.Msg,
                    buttons: {
                        Close: {
                            text: 'Close',
                            btnClass: 'btn btn-danger btn-sm',
                            action: function () {

                            }
                        }
                    }
                })
            }
        }
    });

}

function IsRevised(ID) {
    if (document.getElementById("Engine_Part_Dbkey").value != 0) {

        if (document.getElementById("OldRevision").value != ID.value) {
            document.getElementById("RevisionApplied").value = 1;
            document.getElementById("revnotes").style.display = "block";
        } else {
            document.getElementById("RevisionApplied").value = 0;
            document.getElementById("revnotes").style.display = "none";
        }
    }
}

function SubmitUploadFile() {

    var fileData = new FormData();

    var Filesuploaded = $("#ExcelFile").get(0);

    if (Filesuploaded.files.length != 0) {

        fileData.append('UploadExcelFile', Filesuploaded.files[0]);
        fileData.append('Source_table', document.getElementById("Source_table").value)

        jc = $.dialog({
            theme: 'supervan',
            icon: 'fa fa-spinner fa-2x fa-spin',
            title: 'Working!',
            content: 'Sit back, we are processing your request!'
        });



        $.ajax({
            url: "/Excel_Upload/UploadExcel",
            type: 'POST',
            data: fileData,
            success: function (data) {
                if (data.success) {
                    $.confirm({
                        icon: 'fa fa-check',
                        typeAnimated: true,
                        theme: 'modern',
                        type: 'dark',
                        title: 'Success',
                        content: data.Msg,
                        buttons: {
                            OK: {
                                text: 'OK',
                                btnClass: 'btn btn-success btn-sm',
                                action: function () {
                                    window.location.reload();
                                }
                            }
                        }
                    })


                } else {
                    $.confirm({
                        icon: 'fa fa-exclamation-triangle',
                        typeAnimated: true,
                        theme: 'modern',
                        type: 'red',
                        title: 'Failed',
                        content: data.Msg,
                        buttons: {
                            OK: {
                                text: 'OK',
                                btnClass: 'btn btn-success btn-sm',
                                action: function () {
                                    window.location.reload();
                                }
                            }
                        }
                    })
                }
            },
            cache: false,
            contentType: false,
            processData: false,
        });
    } else {
        alert("Please Upload Valid File !")
        return false;
    }





    return false;
}

function DeleteBulkEngineparts() {

    $.confirm({
        theme: 'modern',
        type: 'dark',
        title: 'Confirmation',
        content: 'Are you sure you wish to delete!',
        buttons: {
            OK: {
                text: 'OK',
                btnClass: 'btn btn-success btn-sm',
                action: function () {
                    DeleteEntry();
                }
            },
            Cancel: {
                text: 'Cancel',
                btnClass: 'btn btn-danger btn-sm',
                action: function () {

                }
            }
        }
    })

}

function DeleteEntry() {

    jc = $.dialog({
        theme: 'supervan',
        icon: 'fa fa-spinner fa-2x fa-spin',
        title: 'Working!',
        content: 'Sit back, we are processing your request!'
    });

    $.ajax({
        url: "/Engine/DeleteBulkEngineparts",
        type: 'POST',
        success: function (data) {
            if (data.success) {
                $.confirm({
                    icon: 'fa fa-check',
                    typeAnimated: true,
                    theme: 'modern',
                    type: 'dark',
                    title: 'Success',
                    content: data.Msg,
                    buttons: {
                        OK: {
                            text: 'OK',
                            btnClass: 'btn btn-success btn-sm',
                            action: function () {
                                window.location.reload();
                            }
                        }
                    }
                })


            } else {
                $.confirm({
                    icon: 'fa fa-exclamation-triangle',
                    typeAnimated: true,
                    theme: 'modern',
                    type: 'red',
                    title: 'Failed',
                    content: data.Msg,
                    buttons: {
                        OK: {
                            text: 'OK',
                            btnClass: 'btn btn-success btn-sm',
                            action: function () {
                                window.location.reload();
                            }
                        }
                    }
                })
            }
        },
        cache: false,
        contentType: false,
        processData: false,
    });
}

function CheckFormMode() {
    if (document.getElementById("displayMode").value == "Readonly") {
        $("#CreatePartsMain :input").attr("disabled", true);
        $("#CreatePartsMain :textarea").attr("disabled", true);
        $("#CreatePartsMain :select").attr("disabled", true);
    }
}

function CallPreLoadDatasOnlyDocs() {
    $.ajax({
        url: '/SharedCall/ViewDocs?Source_table=Engine_Parts_Master&Source_table_key=' + document.getElementById("Engine_Part_Dbkey").value,
        dataType: 'html',
        success: function (data) {
            $('#attachments').html(data);
            CheckFormMode();
        }
    });


}

function CallPreLoadDatas() {

    $.ajax({
        url: '/SharedCall/ViewDocs?Source_table=Engine_Parts_Master&Source_table_key=' + document.getElementById("Engine_Part_Dbkey").value,
        dataType: 'html',
        success: function (data) {
            $('#attachments').html(data);
            CheckFormMode();
        }
    });


    $.ajax({
        url: '/SharedCall/EngineapartUsageStats?Engine_Part_Dbkey=' + document.getElementById("Engine_Part_Dbkey").value,
        dataType: 'html',
        success: function (data) {
            $('#Statistics').html(data);

            //CheckFormMode();
        }
    });


    $.ajax({
        url: '/Configurations/Config/Audit_Logs?viewID=1&partID=' + document.getElementById("Engine_Part_Dbkey").value,
        dataType: 'html',
        success: function (data) {
            $('#RecentChanges').html(data);
            CheckFormMode();
        }
    });

    $.ajax({
        url: '/Procurement/Material/MaterialIssuesPartwise?Engine_Part_Dbkey=' + document.getElementById("Engine_Part_Dbkey").value,
        dataType: 'html',
        success: function (data) {
            $('#MaterialIssues').html(data);
            CheckFormMode();
        }
    });


    document.getElementById("tabs-Material").style.height = document.getElementById("tabs-Engine").offsetHeight + "px";
    document.getElementById("tabs-attachments").style.height = document.getElementById("tabs-Engine").offsetHeight + "px";
    document.getElementById("tabs-Statistics").style.height = document.getElementById("tabs-Engine").offsetHeight + "px";
    document.getElementById("tabs-RecentChanges").style.height = document.getElementById("tabs-Engine").offsetHeight + "px";
    document.getElementById("tabs-MaterialIssues").style.height = document.getElementById("tabs-Engine").offsetHeight + "px";
}

function ViewMfgStatus(id) {
    var res = id.split(",")[1];
    var url = '/Engines/Engine/ViewMfgStatus?PartRelationKey=' + res
    $.get(url)
        .done(function (response) {
            $.dialog({
                title: '',
                typeAnimated: true,
                closeIcon: true,
                closeIconClass: 'fa fa-times',
                theme: 'modern',
                type: 'dark',
                //modal : true,
                // height: "auto",
                //  width:"auto",
                columnClass: 'col-md-12 col-sm-12 col-lg-12 col-xs-12',
                content: response,
            });
        });
}

function EnableCheckBoxBuildTree() {

    var accessbtn = document.getElementById("CustomAccessbtn");
    $('#jstree').jstree('destroy');
    document.getElementById("CustomAccesstip").innerText = "Please select the assemblies/parts to which you wish to grant access."
    var BuildGuid = document.getElementById("BuildGuid").value;
    $.ajax({
        type: "Get",
        url: "/SOPManagement/GetBuildJsTreeData?buidguid=" + BuildGuid,
        success: function (data) {

            var nodelog = $('#jstree').jstree(
                {
                    'core': {
                        'data': data,
                    },
                    "search": {
                        "case_insensitive": true,
                        "show_only_matches": true,
                        "show_only_matches_children": true
                    },
                    "plugins": ["html_data", "dnd", "contextmenu", "search", "types", "adv_search", "themes", "checkbox"],
                    checkbox: { cascade: "down", three_state: false },
                    "contextmenu": {
                        "items": function ($node) {
                            return {
                                "Edit": {
                                    "label": "Add New",
                                    "action": function (obj) {
                                        AddSopPart(0, $node.id);
                                    }
                                }
                            };
                        }
                    }
                }).on('changed.jstree', function (e, data) {
                    var checkedNodes = $('#jstree').jstree('get_checked');
                    var numberOfCheckedNodes = checkedNodes.length;
                    console.log(numberOfCheckedNodes);
                    var accessbtn = document.getElementById("CustomAccessbtn");
                    if (numberOfCheckedNodes > 0) {
                        accessbtn.value = "Refresh"
                        SopAccessForm("");
                    } else {
                        accessbtn.value = "Custom Access"
                    }
                });
        }
    });
}

function SopAccessForm(LinkGuid) {
    var url = "/SOPManagement/CreateSOPAccess?LinkGuid=" + LinkGuid;
    $.get(url).done(function (response) {
        document.getElementById("Div_ComponentDetail").innerHTML = response;
    });
}


function SubmitSOPAccess(ctrl) {

    var checkedNodes = $('#jstree').jstree('get_checked');
    var numberOfCheckedNodes = checkedNodes.length;
    var checked_ids = [];
    if (numberOfCheckedNodes == 0) {
        bootbox.alert('Please select at least one Assembly/Part ');
        return false;
    } else {
        for (var i = 0; i <= checkedNodes.length; i++) {
            checked_ids.push(checkedNodes[i]);
        }
        document.getElementById("modules").value = checked_ids.join(',');
        document.getElementById("AccessBuildGuid").value = document.getElementById("BuildGuid").value;
        // console.log(checked_ids.join(','));
    }

    var LinkGuid = document.getElementById("LinkGuid").value;
    var form = $(ctrl).closest("form");
    document.getElementById("btnCompSubmit").disabled = true;
    $.validator.unobtrusive.parse("form");
    //console.log($(form).valid());
    var access = 'Link';
    if ($(form).valid()) {
        $.ajax({
            type: "POST",
            url: '/SOPManagement/CreateSOPAccess',
            data: $(form).serialize(),
            cache: false,
            success: function (data) {
                if (data.success) {
                    access = data.linkguid;
                    bootbox.confirm({
                        title: 'Link Created Successfully.',
                        message: access,
                        buttons: {
                            cancel: {
                                label: '<i class="fa fa-times"></i> Close'
                            },
                            confirm: {
                                label: '<i class="fa fa-check"></i> Copy Link'
                            }
                        },
                        callback: function (result) {

                            navigator.clipboard.writeText(data.linkguid);
                            // console.log('Text copied to clipboard: ' + data.linkguid);
                            $('#jstree').jstree('destroy');
                            document.getElementById("Div_ComponentDetail").innerHTML = '';
                            LoadJstree('All');
                            //if (LinkGuid != null || LinkGuid !=0)                       // I want to access this and it should be available only during edit
                            //{
                            //   // GetListOfSopAccessLinks();
                            //    Editdialog.modal('hide');
                            //}

                        }
                    });
                } else {
                    bootbox.alert('Failed');
                }
            }
        });
    } else {
        document.getElementById("btnCompSubmit").disabled = false;
    }

}


function GetListOfSopAccessLinks() {
    var url = "/SOPManagement/GetListOfSopAccessLinks?BuildGuid=" + document.getElementById("BuildGuid").value;
    $.get(url).done(function (response) {
        bootbox.alert({
            message: response,
            size: 'extra-large',
            buttons: {
                ok: {
                    className: 'd-none'
                }
            }
        });
    });
}

function SopLinkCopyToClipboard(link) {

    navigator.clipboard.writeText(link);
    //console.log('Text copied to clipboard: ' + link);
    alert('Text copied to clipboard: ' + link);

}

function EditSopAccessLink(LinkGuid) {
    //alert(LinkGuid);
    $.ajax({
        url: "/SOPManagement/CreateSOPAccess?LinkGuid=" + LinkGuid,
        type: "GET",
        success: function (data) {
            Editdialog = bootbox.alert({
                message: data,
                size: 'small',
                buttons: {
                    ok: {
                        className: 'd-none'
                    }
                }
            });

        }
    });


}


function ManageAdvanceInfo(accordionButton) {
    const actionButton = document.getElementById('btnCompSubmit');
    const isCollapsed = accordionButton.classList.contains('collapsed');

    // If expanding, disable the button; if collapsing, enable it
    if (!isCollapsed) {
        actionButton.disabled = true;
    } else {
        actionButton.disabled = false;
    }
}

function SubmitAddtionalComponentDetail() {
    var data = {
        Id: document.getElementById("Id").value,
        ReportingParent: document.getElementById("ReportingParent").value,
        Reporting_Type: document.getElementById("ReportingType").value,
        AssemblyReportingType: document.getElementById("AssemblyReportingType").value
    };

    console.log(JSON.stringify(data));

    $.ajax({
        url: '/SOPManagement/SubmitSOPAdditionalInfo',  // Update with your controller
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                alert('Saved successfully!');
            } else {
                alert('An error occurred.');
            }
        },
        error: function () {
            alert('Error during AJAX request.');
        }
    });
}


function refreshBATLData(buildguid) {
    $.ajax({
        url: '/SOPManagement/SyncBATLData?buildGuid=' + buildguid,
        type: 'POST',
        success: function (response) {
            if (response.success) {
                alert('Refreshed successfully!');
                location.reload();
            } else {
                alert('An error occurred.');
            }
        },
        error: function () {
            alert('Error during AJAX request.');
        }
    });
}

function RefreshComponentFromMPL(nodeId) {
    if (document.getElementById("isCustomAccess").value == "true") {
        alert("You are not authorized to perform this action.");
        return false;
    }

    // Extract the component ID from the node ID (format: "EnginePartDbkey_ComponentId")
    var componentId = nodeId.split("_")[1];

    if (!componentId || componentId == "0") {
        alert("Invalid component selected.");
        return false;
    }

    // Confirm before refreshing
    bootbox.confirm({
        message: "This will update the component with the latest data from MPL (Drawing Number, Description, Parent, Qty/Engine, Reporting Parent, Reporting Type, Assembly Reporting Type). Do you want to continue?",
        buttons: {
            confirm: {
                label: 'Yes, Refresh',
                className: 'btn-primary'
            },
            cancel: {
                label: 'Cancel',
                className: 'btn-secondary'
            }
        },
        callback: function (result) {
            if (result) {
                // Disable the button to prevent double-clicks
                var confirmBtn = $('.bootbox .btn-primary');
                confirmBtn.prop('disabled', true);
                confirmBtn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Refreshing...');

                $.ajax({
                    type: "POST",
                    url: '/SOPManagement/RefreshComponentFromMPL',
                    data: { componentId: componentId },
                    cache: false,
                    success: function (response) {
                        if (response.success) {
                            bootbox.alert({
                                message: "<div class='alert alert-success'>" + response.msg + "</div>" +
                                    "<strong>Updated values:</strong><br/>" +
                                    "<table class='table table-sm table-bordered mt-2'>" +
                                    "<tr><td><strong>Drawing Number:</strong></td><td>" + response.data.drawingNumber + "</td></tr>" +
                                    "<tr><td><strong>Description:</strong></td><td>" + response.data.description + "</td></tr>" +
                                    "<tr><td><strong>Parent ID:</strong></td><td>" + response.data.parentId + "</td></tr>" +
                                    "<tr><td><strong>Qty/Engine:</strong></td><td>" + response.data.qtyPerEngine + "</td></tr>" +
                                    "<tr><td><strong>Reporting Parent:</strong></td><td>" + response.data.reportingParent + "</td></tr>" +
                                    "<tr><td><strong>Reporting Type:</strong></td><td>" + response.data.reportingType + "</td></tr>" +
                                    "<tr><td><strong>Assembly Reporting Type:</strong></td><td>" + response.data.assemblyReportingType + "</td></tr>" +
                                    "</table>",
                                size: 'large',
                                callback: function () {
                                    // Refresh the component detail view if it's currently displayed
                                    GetComponentDetail(componentId);
                                    // Optionally reload the tree to reflect any parent changes
                                    // LoadJstree('All');
                                }
                            });
                        } else {
                            bootbox.alert({
                                message: "<div class='alert alert-danger'>" + response.msg + "</div>",
                                size: 'small'
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        bootbox.alert({
                            message: "<div class='alert alert-danger'>Error refreshing component: " + error + "</div>",
                            size: 'small'
                        });
                    }
                });
            }
        }
    });
}

function AddSopPartWithExisting(Id, parentId) {
    if (document.getElementById("isCustomAccess").value == "true") {
        alert("You are not authorized to perform this action.")
        return false;
    }

    var url = '/SOPManagement/AddComponentWithExistingPart?parentid=' + parentId.split("_")[1];
    $.get(url).done(function (response) {
        bootbox.dialog({
            message: response,
            title: "SOP - Add Component with Existing Part",
            size: 'xl',
            onEscape: true,
            backdrop: true,
            buttons: {
                ok: {
                    className: 'd-none'
                }
            }
        });
    });
}