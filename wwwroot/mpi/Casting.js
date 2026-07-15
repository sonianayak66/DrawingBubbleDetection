
let UrlSubmittingastingOrder = "/Casting/CastingOrder";
let UrlCastingDetail = "/Casting/CastingDetail";
let UrlCreateEditCastingItems = "/Casting/CastingItems";
let UrlCreateEditCastingReceipt = "/Casting/CastingReceiptItems";
/*let UrlCastingReceiptItemSplits = "/Casting/CastingReceiptItemSplit";*/
let UrlCastingReceiptItemSplits = "/Casting/CastingReceiptItemSplitDetail";
let UrlCastingReceiptItemSplitModel = "/Casting/CastingReceiptItemSplitModel";
let UrlSubmitCastingReceiptItemSplitModel = "/Casting/SaveReceiptSplitModel";
var previouslySelectedCastingDbkey = "";
var previouslySelectedCastingReceiptDbkey = "";
var previouslySelectedCastingReceiptDbkey = "";



function CreateEditCastingOrder(CastingId, ViewMode) {
    var urlinput = "/Casting/CastingOrder" + "?CastingId=" + CastingId + "&ViewMode=" + ViewMode;
    $.get(urlinput).done(function (response) {
        //bootbox.dialog({
        //    title: "Casting Order",
        //    message: response,
        //    size: 'large',
        //    closeButton: true,
        //});

        document.getElementById("order-basic-info-area").innerHTML = response;

        //new Choices('#VendorIds', {
        //    allowHTML: true,
        //    removeItemButton: true,
        //});

        //new Choices('#SerialNumber', {
        //    allowHTML: true,
        //    removeItemButton: true,
        //});

        new Choices('#OrderNumbers', {
            allowHTML: true,
            removeItemButton: true,
        });

        //new Choices('#RawMaterial', {
        //    searchEnabled: true,
        //});
    });
}

function SubmitCastingOrder() {
    $.validator.unobtrusive.parse("form");
    if ($('#CastingForm').valid()) {
        var formData = {
            CastingDbkey: document.getElementById('CastingDbkey').value,
            DemandNumber: document.getElementById('DemandNumber').value,
            OrderDate: document.getElementById('OrderDate').value,
            MMGOrderNumber: document.getElementById('MMGOrderNumber').value,
            OrderNumbers: document.getElementById('OrderNumbers').value,
            Remarks: document.getElementById('Remarks').value,
            OrderStatus: document.getElementById('OrderStatus').value,
            DemandingOfficer: document.getElementById('DemandingOfficer').value,
            DemandDesc: document.getElementById('DemandDesc').value
        };
        $.ajax({
            type: "POST",
            url: UrlSubmittingastingOrder,
            data: {
                castingDetailViewModel: formData
            },
            cache: false,
            success: function (data) {
                if (data.success) {
                    bootbox.alert('Submitted Successfully', function () {
                        if (document.getElementById('CastingDbkey').value == 0) {
                            window.location.href = UrlCastingDetail + "?id=" + data.castingGUID;
                        } else {
                            window.location.reload();
                        }
                    });
                } else {
                    bootbox.alert('Failed !', function () {

                    });
                }
            }
        });
    }
    return false;
}

var selectChoice;
function EditCastingItems(CastingId) {
    var urlinput = UrlCreateEditCastingItems + "?CastingId=" + CastingId;
    $.get(urlinput).done(function (response) {
        //bootbox.dialog({
        //    title: "Casting Items",
        //    message: '<div id="casting-items-container"></div>',
        //    size: 'custom-model',
        //    closeButton: true,
        //    className: 'custom-modal',
        //});

        document.getElementById("casting-items-container").innerHTML = response;
        AddRow_CastingItem();
    });
}

function GetCastingLink(CastingId) {
    var urlinput = '/Casting/CreateCustmAccessLink' + "?CastingGUID=" + CastingId;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Custom Access Link",
            message: response.linkguid,
            size: 'small',
            closeButton: true,
        });
    });
}

function GetCastingSummaryTable() {
    var urlinput = '/Casting/CastingOrderSummary';
    $.get(urlinput).done(function (response) {
        document.getElementById("tab-home").innerHTML = response;
    });
}

function GetPartOrderData(partKey, action) {

    var targetElement = document.getElementById("Part_" + partKey);
    var IconElementOpen = document.getElementById("icon-Open-" + partKey);
    var IconElementClose = document.getElementById("icon-Close-" + partKey);

    $(IconElementOpen).toggle();
    $(IconElementClose).toggle();

    if (action == "Close") {
        $(targetElement).toggle();
        return false;
    }

    var urlinput = '/Casting/CastingPartOrderData?partkey=' + partKey;
    $.get(urlinput).done(function (response) {
        targetElement.children[0].innerHTML = response;
        targetElement.style.display = "";
    });
}

function AddRow_CastingItem() {

    var selectPartElements = document.querySelectorAll('.PartchoiceClass');
    selectPartElements.forEach(function (element) {
        if (element.choices) {
            element.choices.destroy();
        }
    });

    var selectRMElements = document.querySelectorAll('.RMchoiceClass');
    selectRMElements.forEach(function (element) {
        if (element.choices) {
            element.choices.destroy();
        }
    });

    var selectVendorElements = document.querySelectorAll('.VendorchoiceClass');
    selectVendorElements.forEach(function (element) {
        if (element.choices) {
            element.choices.destroy();
        }
    });

    var selectorderElements = document.querySelectorAll('.OrderchoiceClass');
    selectorderElements.forEach(function (element) {
        if (element.choices) {
            element.choices.destroy();
        }
    });



    var x = document.getElementById("Tbl_castItems");  //get the table
    var node = x.rows[0].cloneNode(true);    //clone the previous node or row
    node.style = null;
    node.cells[0].children[0].classList.add('PartchoiceClass');
    node.cells[3].children[0].classList.add('RMchoiceClass');
    node.cells[4].children[0].classList.add('VendorchoiceClass');
    node.cells[5].children[0].classList.add('OrderchoiceClass');
    x.appendChild(node);

    var selectPartElements2 = document.querySelectorAll('.PartchoiceClass');
    selectPartElements2.forEach(function (element) {
        new Choices(element, {
            searchEnabled: true,
        });
    });


    var selectRMElements2 = document.querySelectorAll('.RMchoiceClass');
    selectRMElements2.forEach(function (element) {
        new Choices(element, {
            searchEnabled: true,
        });
    });

    var selectVendorElements2 = document.querySelectorAll('.VendorchoiceClass');
    selectVendorElements2.forEach(function (element) {
        new Choices(element, {
            searchEnabled: true,
        });
    });

    var selectorderElements2 = document.querySelectorAll('.OrderchoiceClass');
    selectorderElements2.forEach(function (element) {
        new Choices(element, {
            searchEnabled: true,
        });
    });
}


function AddRow_CastingQtySplit() {

    var x = document.getElementById("Tbl_castingQtySplitBody");  //get the table
    var node = x.rows[0].cloneNode(true);    //clone the previous node or row

    $(node).find('input, select, textarea').each(function () {
        if ($(this).attr("data-field") == "QtySplitKey" || $(this).attr("data-field") == "SplitQty") {
            $(this).val(0);
        } else if ($(this).attr("data-field") == "SerialNos") {
            $(this).val("");
        }
    });

    x.append(node);
}

function delRow_CastingQtySplit(ele) {
    var tblrow = ele.closest("tr");
    var tblBody = document.getElementById("Tbl_castingQtySplitBody");

    if (tblBody.rows.length != 1) {
        tblBody.deleteRow(tblrow.rowIndex - 1);
        var CastingQtySplitKey = 0;

        $(tblrow).find('input, select, textarea').each(function () {
            if ($(this).attr("data-field") == "CastingQtySplitKey") {
                CastingQtySplitKey = $(this).val();
            }
        });

        if (CastingQtySplitKey > 0) {
            var url = "/Casting/DeleteQtySplit?CastingQtySplitKey=" + CastingQtySplitKey;
            $.get(url).done(function (response) {

            });
        }

    }
}


function delRow(ele) {
    var tblrow = ele.closest("tr");
    document.getElementById("Tbl_castItems").deleteRow(tblrow.rowIndex - 1); //get the table
    //delete the last row
}

function SaveCastingItems() {

    var castingItemViewModels = new Array();
    var items = document.getElementsByClassName("part");

    console.log(items);
    var currentDate = moment();
    for (var i = 1; i < items.length; i++) {
        var item = {};
        if (document.getElementsByClassName("part")[i].value != 0) {
            item.CastingItemKey = document.getElementsByClassName("CastingItemKey")[i].value;
            item.CastingDbkey = document.getElementById("GlobalCastingDbkey").value;
            item.EnginePartDbkey = document.getElementsByClassName("part")[i].value;
            var qqty = document.getElementsByClassName("Qty")[i].value;
            item.OrderQty = qqty == "" ? 0 : qqty;
            item.ItemDescription = document.getElementsByClassName("ItemDescription")[i].value;
            item.Vendor = document.getElementsByClassName("Vendor")[i].value;
            item.OrderNumber = document.getElementsByClassName("OrderNumber")[i].value;
            var DeliveryDate_ = document.getElementsByClassName("DeliveryDate")[i].value;
            item.DeliveryDate = DeliveryDate_ == "" ? currentDate.format("YYYY-MM-DD") : DeliveryDate_;
            item.RawMaterial = document.getElementsByClassName("RawMaterial")[i].value;
            castingItemViewModels.push(item);
        }
    }




    if (castingItemViewModels.length == 0) {
        bootbox.alert('Please select at least one part item to continue .');
        return false;
    }
    var postData = JSON.stringify(castingItemViewModels);
    console.log(postData);
    $.ajax({
        type: 'POST',
        url: UrlCreateEditCastingItems,
        data: postData,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.success) {
                bootbox.alert('Submitted Successfully', function () {
                    window.location.reload();
                });
            } else {
                bootbox.alert('Failed !', function () {

                });
            }
        }
    })
}

function CastingReceipt(CastingId, ReceiptGuid) {
    var urlinput = UrlCreateEditCastingReceipt + "?CastingId=" + CastingId + "&ReceiptGuid=" + ReceiptGuid;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Casting Receipt",
            message: response,
            size: 'large',
            closeButton: true,
        });
    });
}


function CastingReceiptSplitDetail(partkey) {
    var urlinput = "/Casting/CastingReceiptLevelSummary?partKey=" + partkey;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Receipt Detail",
            message: response,
            size: 'extra-large',
            closeButton: true,
        });
    });
}

function SubmitCastingReceipt() {
    var castingReceiptViewModels = new Array();
    var CastingReceiptQtySplit = new Array();
    var items = document.getElementsByClassName("CastingItemKey");

    if (document.getElementById("ReceiptNumber").value == "" || document.getElementById("ReceiptNumber").value == null) {
        bootbox.alert('Receipt Number Required!');
        return false;
    }


    if (document.getElementById("ReceiptDate").value == "" || document.getElementById("ReceiptDate").value == null) {
        bootbox.alert('Receipt Date Required!');
        return false;
    }



    for (var i = 0; i < items.length; i++) {
        var item = {};
        if (document.getElementsByClassName("CastRcpQty")[i].value != 0) {
            item.CastingReceiptDbkey = document.getElementsByClassName("CastingReceiptDbkey")[i].value;
            item.CastingDbkey = document.getElementById("GlobalCastingDbkey").value;
            item.CastingItemKey = document.getElementsByClassName("CastingItemKey")[i].value
            item.ReceiptNumber = document.getElementById("ReceiptNumber").value;
            item.ReceiptDate = document.getElementById("ReceiptDate").value
            item.Qty = document.getElementsByClassName("CastRcpQty")[i].value;
            castingReceiptViewModels.push(item);
        }
    }




    var postData = JSON.stringify(castingReceiptViewModels);
    $.ajax({
        type: 'POST',
        url: UrlCreateEditCastingReceipt,
        data: postData,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.success) {
                bootbox.alert('Submitted Successfully', function () {
                    window.location.reload();
                });
            } else {
                bootbox.alert('Failed !', function () {

                });
            }
        }
    })

}
//might need to change it need to check it and do
function CastingReceiptItemSplits_Removed(CastingDbkey, CastingReceiptDbkey) {
    var urlinput = UrlCastingReceiptItemSplits + "?CastingDbkey=" + CastingDbkey + "&CastingReceiptDbkey=" + CastingReceiptDbkey;
    // these values are set here for calling this method from other model
    previouslySelectedCastingDbkey = CastingDbkey;
    previouslySelectedCastingReceiptDbkey = CastingReceiptDbkey;
    $.get(urlinput).done(function (response) {

        if (document.getElementById("Casting-Receipt-Item-Splits-container") == undefined) {
            bootbox.dialog({
                title: "Casting Receipt Item Splits",
                message: '<div id="Casting-Receipt-Item-Splits-container">' + response + ' </div>',
                size: 'extra-large',
                closeButton: true,
                className: 'custom-modal'
            });
        } else {
            document.getElementById("Casting-Receipt-Item-Splits-container").innerHTML = response;
        }
    });
}

function CastingReceiptItemSplitDetail(CastingDbkey, CastingReceiptDbkey) {
    var urlinput = UrlCastingReceiptItemSplits + "?CastingDbkey=" + CastingDbkey + "&CastingReceiptDbkey=" + CastingReceiptDbkey;
    // these values are set here for calling this method from other model
    previouslySelectedCastingDbkey = CastingDbkey;
    previouslySelectedCastingReceiptDbkey = CastingReceiptDbkey;
    $.get(urlinput).done(function (response) {

        if (document.getElementById("Casting-Receipt-Item-Splits-container") == undefined) {
            bootbox.dialog({
                title: "Casting Receipt Item Splits",
                message: '<div id="Casting-Receipt-Item-Splits-container">' + response + ' </div>',
                size: 'extra-large',
                closeButton: true,
                className: 'custom-modal'
            });
        } else {
            document.getElementById("Casting-Receipt-Item-Splits-container").innerHTML = response;
        }
    });
}

function GetCastingReceiptSplitModel_Removed(CastingDbkey, CastingReceiptDbkey, splitItemKey) {
    var urlinput = UrlCastingReceiptItemSplitModel + "?CastingDbkey=" + CastingDbkey + "&CastingReceiptDbkey=" + CastingReceiptDbkey + "&CastingSplitKey=" + splitItemKey;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Split Info",
            message: response,
            size: 'extra-large',
            closeButton: true,
            // AttachmentDoc(Receipt_dbkey);
        });
        //document.getElementById("SplitRecipts").innerHTML = response;
        // AttachmentDoc(Receipt_dbkey);
    });
}

function GetCastingReceiptSplitModel(CastingDbkey, splitItemKey) {
    var urlinput = UrlCastingReceiptItemSplitModel + "?CastingDbkey=" + CastingDbkey + "&CastingSplitKey=" + splitItemKey;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Casting Receipt",
            message: response,
            size: 'extra-large',
            closeButton: true,
            // AttachmentDoc(Receipt_dbkey);
        });
        ApplyAutoCompleteOnRemarks();
        //document.getElementById("SplitRecipts").innerHTML = response;
        // AttachmentDoc(Receipt_dbkey);
    });
}

function SaveSplitItemModel(CastingDbkey) {
    var form = $('#CastingSplitModelData');
    $.validator.unobtrusive.parse("#" + form.attr("id"));
    $(form).validate();
    console.log($(form).serialize());
    if ($('#CastingSplitModelData').valid() == false) {
        return false;
    }
    var formData = new FormData();

    var formElements = $(form).serializeArray();
    var jsonResult = {};

    $.each(formElements, function (index, element) {
        jsonResult[element.name] = element.value;
    });


    formData.append('jsonData', JSON.stringify(jsonResult));

    var splitUploadFiles = document.getElementsByClassName("splitUploadFiles");

    var fileData = [];

    for (var i = 0; i < splitUploadFiles.length; i++) {
        if (splitUploadFiles[i].files.length > 0) {
            var fileItem = {};
            fileItem.Source_table_key = CastingDbkey;
            fileItem.Source_table = "Casting_Forging_File";
            fileItem.File_DVD_Num = document.getElementsByClassName("fileAttachmentType")[i].value;
            fileItem.File_Revision = document.getElementsByClassName("splitUploadfileRefNum")[i].value;
            fileData.push(fileItem);
            formData.append('files', splitUploadFiles[i].files[0]);
        }
    }
    formData.append('filesData', JSON.stringify(fileData));
    var qtySplits = [];

    for (var i = 0; i < document.getElementById("Tbl_castingQtySplitBody").rows.length; i++) {
        var item = {};
        var currentRow = document.getElementById("Tbl_castingQtySplitBody").rows[i];
        $(currentRow).find('input, select, textarea').each(function () {
            item[$(this).attr("data-field")] = $(this).val();
        }); 
        qtySplits.push(item);
    }
    formData.append('qtySplits', JSON.stringify(qtySplits));

    $.ajax({
        url: UrlSubmitCastingReceiptItemSplitModel,
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            bootbox.alert('Submitted Successfully', function () {
                window.location.reload();
            });
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);

        }
    });
}

function CloneSplitDocumentRow() {
    var clonedRow = $('#CastingSplitDocumentTable tbody tr:first').clone(); // Clone the last row
    clonedRow.show();
    $('#CastingSplitDocumentTable').append(clonedRow);
}

function DeleteUnsavedDocs(btn) {
    $(btn).closest('tr').remove();
}

function DeleteCastingComponents(itemdom, key, table) {
    bootbox.confirm({
        title: "Confirmation",
        message: "Are you sure you wish to remove?",
        buttons: {
            cancel: {
                label: '<i class="fa fa-times"></i> Cancel'
            },
            confirm: {
                label: '<i class="fa fa-check"></i> Confirm'
            }
        },
        callback: function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: "/Casting/DeleteCastingComponents",
                    data: "key=" + key + '&table=' + table,
                    success: function (data) {
                        if (data.success) {
                            bootbox.alert(
                                "Removed successfully",
                                function () {
                                    if (table == "CastingDetails") {
                                        window.location.reload();
                                    } else if (table == "CastingItems") {
                                        var tblrow = itemdom.closest("tr");
                                        document.getElementById("Tbl_castItems").deleteRow(tblrow.rowIndex - 1); //get the table
                                    } else if (table == "CastingReceipts") {
                                        window.location.reload();
                                    } else if (table == "CastingReceiptsItemSplit") {
                                        var tblrow = itemdom.closest("tr");
                                        document.getElementById("casting_split_tbl_bdy").deleteRow(tblrow.rowIndex - 1); //get the table
                                    }
                                }
                            );
                        }
                        else {
                            bootbox.alert({
                                message: "You cannot delete the item because completed demand receipts transactions exist for the item.",
                            })
                        }
                    }
                });
            }
        }
    });
}

function DeleteCastingReceiptsDocs(Attachment_Db_Key, Source_table_key, ele) {
    bootbox.confirm("Are you sure you want to delete this document?", function (result) {
        if (result) {
            $.ajax({
                type: 'GET',
                url: '/Casting/DeleteReceiptDocument?documentId=' + Attachment_Db_Key + '&receiptDbkey=' + Source_table_key,
                success: function (data) {
                    if (data.success) {
                        bootbox.alert('Deleted Successfully', function () {
                            var tblrow = ele.closest("tr");
                            document.getElementById("CastingSplitDocTableBody").deleteRow(tblrow.rowIndex - 1); //get the table
                        });
                    }
                    else {
                        bootbox.alert("Delete Failed. Please ensure this document is not mapped to any items before attempting to delete.")
                    }
                },
                error: function () {
                }
            });
        }
    });
}

function UpdateDocumentType(Attachment_Db_Key, dropdown) {
    var selectedOption = dropdown.options[dropdown.selectedIndex];
    var selectedValue = selectedOption.value;
    console.log(selectedValue);
    console.log(Attachment_Db_Key);
    $.ajax({
        type: 'GET',
        url: '/Casting/UpdateReceiptDocType?documentId=' + Attachment_Db_Key + '&doctype=' + selectedValue,
        success: function (data) {
            if (data.success) {
                bootbox.alert('Updated Successfully', function () {

                });
                dropdown.setAttribute('disabled', '');

            }
            else {
                bootbox.alert("Failed");
            }
        },
        error: function () {
        }
    });
}


function ApplyAutoCompleteOnRemarks() {
    $.ajax({
        url: '/SharedCall/GetMetaMasterJResult?type=CastingReceiptItemRemarks',
        dataType: 'json',
        success: function (response) {
            var availableTags = response.map(function (item) {
                return item.DisplayText; // Assuming each object has a 'name' property
            });
            // Initialize autocomplete
            $("#modelItem_Remarks").autocomplete({
                source: availableTags // Specify the source of the data
            });

            $("#modelItem_Remarks_QtyRejected").autocomplete({
                source: availableTags // Specify the source of the data
            });
        },
        error: function (xhr, status, error) {
            console.error("Error fetching data:", error);
        }
    });
}

function viewMore(arrow, className) {
    var serialNo = arrow.parentElement.querySelector('.' + className);
    //console.log(serialNo);
    if (serialNo.style.maxHeight === 'none') {
        serialNo.style.maxHeight = '100px';
        serialNo.style.overflow = 'hidden';
        arrow.innerHTML = '<i class="fas fa-angle-down fa-1x" > </i>';
    }
    else {
        serialNo.style.maxHeight = 'none';
        serialNo.style.overflow = 'auto';
        arrow.innerHTML = '<i class="fas fa-angle-up fa-1x" > </i>';
    }
}