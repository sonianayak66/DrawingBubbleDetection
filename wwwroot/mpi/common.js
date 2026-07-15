var DeletionRequestDialog = null;

function Submitform(FormName, target, successFunc, redirectURL = '') {
    $.validator.unobtrusive.parse("form");
    var validator = $("#" + FormName).validate();
    if ($("#" + FormName).valid()) {
        var data = $("#" + FormName).serialize();
        console.log(data);
        $.ajax({
            type: 'POST',
            url: target,
            data: data,
            success: function (data) {
                if (data.success) {
                    DisplayMsg(data.msg, successFunc, redirectURL)
                    location.reload();
                } else {
                    alert(data.msg);
                }

            },
            error: function (jqXHR, exception) {
                alert(exception);
            }
        })
    }
    return false;
}

function DisplayMsg(msg, action, redirectURL) {
    bootbox.alert(msg, function () {
        if (redirectURL != '') {
            Redirect(redirectURL);
        } else {
            action();
        }
    });
}

function Redirect(url) {
    window.location = url;
}

// ----- LOADER MANAGEMENT START
$(document).on("ajaxStart", function () {
    $('#loading').show();
});
$(document).on("ajaxError", function () {
    $('#loading').hide();
});
$(document).on("ajaxComplete", function () {
    $('#loading').hide();
});
$(document).on("ajaxComplete", function () {
    $('#ajaxSuccess').hide();
});
$(window).on('load', function () {
    $('#loading').hide();
})
// ------ LOADER MANAGEMENT END


function createRecordDeletionRequest(sourceKey, SourceTableName, DisplayName) {

    var DeletionManagementVM = {};
    DeletionManagementVM.DeletionKey = 0;
    DeletionManagementVM.SourceTableName = SourceTableName;
    DeletionManagementVM.SourceTableKey = sourceKey;
    DeletionManagementVM.SourceDisplayName = DisplayName;

    $.ajax({
        type: 'POST',
        url: '/RecordsDeletion/CreateApprovalRequest',
        contentType: 'application/json',
        data: JSON.stringify(DeletionManagementVM),
        success: function (response) {
            DeletionRequestDialog = bootbox.dialog({
                title: "Create " + DisplayName + " deletion request",
                message: response,
                //size: 'medium',
                closeButton: true,
            });
        },
        error: function (jqXHR, exception) {
            alert(exception);
        }
    })
}


function SubmitRecordDeletionRequest(btn) {

    var $form = $(btn).closest('form');
    $.validator.unobtrusive.parse($form);
    $($form).validate();
    if ($($form).valid() == false) {
        return false;
    }

    var formData = $($form).serializeArray();
    var jsonData = {};
    $(formData).each(function (index, obj) {
        jsonData[obj.name] = obj.value;
    });

    $.ajax({
        type: 'POST',
        url: '/RecordsDeletion/SubmitApprovalRequest',
        contentType: 'application/json',
        data: JSON.stringify(jsonData),
        success: function (response) {
            bootbox.alert(response.msg);
            DeletionRequestDialog.modal("hide");
        },
        error: function (jqXHR, exception) {
            alert(exception);
        }
    })

}



function ConfirmDeletionRequest(btn, deletionKey) {

    var RecordType = "";
    var RecordReference = "";
  

    $(btn).closest('tr').find('td').each(function () {
        var cellContent = $(this).text();
        var datafield = $(this).data('field');
        if (datafield == "SourceDisplayName") {
            RecordType = cellContent;
        } else if (datafield == "RefNo") {
            RecordReference = cellContent;
        }
    }); 

    let dialog = bootbox.dialog({
        title: 'Confirm Deletion',
        message: "<p>Delete <b>" + RecordType + "</b> with reference <b>" + RecordReference +"</b>? </p> <p> Please note this action is <b>irreversible</b>. </p> ",
        size: 'large',
        buttons: {
            cancel: {
                label: "Reject",
                className: 'btn-danger',
                callback: function () {
                    ActionDeletionRequest('Rejected', deletionKey);
                }
            },
            noclose: {
                label: "Approve",
                className: 'btn-success',
                callback: function () {
                    ActionDeletionRequest('Approved', deletionKey);
                }
            },
            ok: {
                label: "Close",
                className: 'btn-info',
                callback: function () {
                    console.log('Custom OK clicked');
                }
            }
        }
    });
}


function ActionDeletionRequest(action,deletionKey) {
    var urlinput = "/RecordsDeletion/RequestAction?DeletionKey=" + deletionKey + "&UserAction=" + action;
    $.get(urlinput).done(function (response) {
        bootbox.alert(response.msg);
        window.location.reload();
    });
}



function DemandReceiptDocumentReMapping(ReceiptDbkey) {
    var html = '<form action="#" method="post">'+
                     '<b for="passwordField">Password:</b>' +
                     '<input type="password" id="passwordField" name="passwordField" class="form-control">' +
                     '<hr/>' +
                     '<label for= "agreeCheckbox" >' +
                     '<input type="checkbox" id="agreeCheckbox" name="agreeCheckbox"> <b>Force Update</b>' +
                     '</label>' +
                '</form>';

    bootbox.confirm({
        title: 'Confirmation on Document Mapping to splits',
        message: html,
        callback:function(result) {
            if (result) {

                var password = document.getElementById("passwordField").value;
                var forceupdate = document.getElementById("agreeCheckbox").checked;

                if (password == '') {
                    alert("Password is required!")
                    return false;
                }

            

                bootbox.confirm('Are you sure on update?',
                    function (result) {
                        $.ajax({
                            type: 'Get',
                            url: '/DemandManagement/ReceiptDocumentMappingToSplit?ReceiptDbkey=' + ReceiptDbkey + "&forceupdate=" + forceupdate + "&password=" + password,
                            success: function (response) {
                                bootbox.alert(response.responseMessage);

                            },
                            error: function (jqXHR, exception) {
                                alert(exception);
                            }
                        })
                    });
            }

        }
    });
}

function getJcDetails(jcNo) {
    $.ajax({
        type: 'GET',
        url: '/Home/JobCardDetails',
        data: { jcNo: jcNo },
        success: function (html) {
            bootbox.dialog({
                title: 'Job Card Details',
                message: html,
                size: 'extra-large',
                onEscape: true, 
                backdrop: true
            });
        },
        error: function () {
            bootbox.alert("Error loading Job Card details.");
        }
    });
}

 
function getVendorDetails(vendorDbkey, rawMaterialDbkey) {
    $.ajax({
        type: 'GET',
        url: '/Home/VendorRawMaterialDetails',
        data: {
            vendorDbkey: vendorDbkey,
            rawMaterialDbkey: rawMaterialDbkey
        },
        success: function (html) {
            bootbox.dialog({
                title: 'Vendor Material Details',
                message: html,
                size: 'extra-large',
                onEscape: true,
                backdrop: true
            });
        },
        error: function () {
            bootbox.alert("Error loading vendor material details.");
        }
    });
}


 
function getRMPopUP(materialId, materialName) {

    var urlinput = "/Report/MaterialDetailsPopup?rawMaterialDbkey=" + materialId;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: materialName,
            message: response,
            size: 'extra-large',
            closeButton: true,
            onEscape: true,
            backdrop: true
        });
    });

}
