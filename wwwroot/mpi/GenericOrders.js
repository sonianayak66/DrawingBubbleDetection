
//let UrlSubmittingastingOrder = "/Orders/SaveOrder";
//let UrlCastingDetail = "/Casting/CastingDetail";
//let UrlCreateEditCastingItems = "/Casting/CastingItems";
//let UrlCreateEditCastingReceipt = "/Casting/CastingReceiptItems";
/*let UrlCastingReceiptItemSplits = "/Casting/CastingReceiptItemSplit";*/
//let UrlCastingReceiptItemSplits = "/Casting/CastingReceiptItemSplitDetail";
//let UrlCastingReceiptItemSplits = "";
let UrlCastingReceiptItemSplitModel = "/Casting/CastingReceiptItemSplitModel";
let UrlSubmitCastingReceiptItemSplitModel = "/Casting/SaveReceiptSplitModel";
var previouslySelectedCastingDbkey = "";
var previouslySelectedCastingReceiptDbkey = "";
var previouslySelectedCastingReceiptDbkey = "";



function CreateEditOrder(CastingId, Ordertype) {
    var urlinput = "/Orders/OrderForm" + "?CastingId=" + CastingId +"&Ordertype=" +Ordertype;
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

function SubmitGenericOrder() {
    $.validator.unobtrusive.parse("form");
    var orderType = document.getElementById('OrderType').value;
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
            DemandDesc: document.getElementById('DemandDesc').value,
            OrderType: orderType
        };
        $.ajax({
            type: "POST",
            url: "/Orders/SaveOrder",
            data: {
                castingDetailViewModel: formData
            },
            cache: false,
            success: function (data) {
                if (data.success) {
                    bootbox.alert('Submitted Successfully', function () {
                        if (document.getElementById('CastingDbkey').value == 0) {
                            window.location.href = "/Orders/OrderDetail?OrderType=" + orderType + "&id=" + data.castingGUID;
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
function EditOrderItems(OrderId) {
    var urlinput = "/Orders/OrderItems?OrderId=" + OrderId;
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

//function GetCastingLink(CastingId) {
//    var urlinput = '/Orders/CreateCustmAccessLink?CastingGUID=' + CastingId;
//    $.get(urlinput).done(function (response) {
//        bootbox.dialog({
//            title: "Custom Access Link",
//            message: response.linkguid,
//            size: 'small',
//            closeButton: true,
//        });
//    });
//}

function GetCastingLink(CastingId) {
    var urlinput = '/Orders/OrderModuleCustomAccess?CastingGUID=' + CastingId;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Custom Access",
            message: response,
            size: 'large',
            closeButton: true,
        });
    });
}

function GetOrderSummaryTable(ordertype) {
    var urlinput = '/Orders/OrderSummary?Ordertype=' + ordertype;
    $.get(urlinput).done(function (response) {
        document.getElementById("tab-home").innerHTML = response;
        
    });
}

function GetOrderDashboardTable(ordertype) {
    var urlinput = '/Orders/Dashboard?Ordertype=' + ordertype;
    $.get(urlinput).done(function (response) {
        document.getElementById("tab-dashboard").innerHTML = response;
        
    });
}

function GetIssueDetailsTable(ordertype) {
    var urlinput = '/Orders/IssueDetails?OrderType=' + ordertype;
    $.get(urlinput).done(function (response) {
        document.getElementById("tab-Issues").innerHTML = response;
    });


}

 
function GetPartOrderData(partKey, action) {

    var targetElement = document.getElementById("Part_" + partKey);
    var IconElementOpen = document.getElementById("icon-Open-" + partKey);
    var IconElementClose = document.getElementById("icon-Close-" + partKey);
    var Ordertype = document.getElementById("generic-order-summary-Ordertype").value;

    $(IconElementOpen).toggle();
    $(IconElementClose).toggle();

    if (action == "Close") {
        $(targetElement).toggle();
        return false;
    }

   // var urlinput = '/Casting/CastingPartOrderData?partkey=' + partKey;
    var urlinput = '/Orders/PartOrderData?partkey=' + partKey + '&OrderType=' + Ordertype;
    $.get(urlinput).done(function (response) {
        targetElement.children[0].innerHTML = response;
        targetElement.style.display = "";
    });
}

function GetItemReceiptBatches(partKey, action) {
   
    var targetElement = document.getElementById("item_" + partKey);
    var IconElementOpen = document.getElementById("icon-Open-" + partKey);
    var IconElementClose = document.getElementById("icon-Close-" + partKey);
   var Ordertype = document.getElementById("Generic-Ordertype").value;

    $(IconElementOpen).toggle();
    $(IconElementClose).toggle();

    if (action == "Close") {
        $(targetElement).toggle();
        return false;
    }
    var urlinput = '/Orders/ItemLevelSummary?partKey=' + partKey + '&OrderType=' + Ordertype;
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
   // node.cells[5].children[0].classList.add('OrderchoiceClass');
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
    //selectorderElements2.forEach(function (element) {
    //    new Choices(element, {
    //        searchEnabled: true,
    //    });
    //});
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

function SaveOrderItems() {

    var castingItemViewModels = new Array();
    var items = document.getElementsByClassName("part");

   // console.log(items);
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
            item.GTREDrgNo = document.getElementsByClassName("DrawingNo")[i].value;
            var TestSpecimen_ = document.getElementsByClassName("TestSpecimen")[i].checked;
            //console.log(TestSpecimen_);
            item.TestSpecimen = TestSpecimen_ == true ? 1 : 0;
            castingItemViewModels.push(item);
        }
    } 
   // return false;
    if (castingItemViewModels.length == 0) {
        bootbox.alert('Please select at least one part item to continue .');
        return false;
    }
    var postData = JSON.stringify(castingItemViewModels);
   // console.log(postData);
    $.ajax({
        type: 'POST',
        url: "/Orders/OrderItems",
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
//CastingReceipt
function GetReceiptModel(CastingId, ReceiptGuid) {
    //var urlinput = UrlCreateEditCastingReceipt + "?CastingId=" + CastingId + "&ReceiptGuid=" + ReceiptGuid;
    var urlinput = "/Orders/OrderReceiptItems" + "?CastingId=" + CastingId + "&ReceiptGuid=" + ReceiptGuid;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Casting Receipt",
            message: response,
            size: 'large',
            closeButton: true,
        });
    });
}

// Old method name: CastingReceiptSplitDetail
function OrderReceiptSplitDetail(partkey) {
    var OrderType = document.getElementById("generic-order-summary-Ordertype").value;
    var urlinput = "/Orders/ReceiptLevelSummary?partKey=" + partkey + "&OrderType=" + OrderType;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Receipt Detail",
            message: response,
            size: 'extra-large',
            closeButton: true,
        });
    });
}

//function SubmitCastingReceipt() {
//    var castingReceiptViewModels = new Array();
//    var CastingReceiptQtySplit = new Array();
//    var items = document.getElementsByClassName("CastingItemKey");

//    if (document.getElementById("ReceiptNumber").value == "" || document.getElementById("ReceiptNumber").value == null) {
//        bootbox.alert('Receipt Number Required!');
//        return false;
//    }


//    if (document.getElementById("ReceiptDate").value == "" || document.getElementById("ReceiptDate").value == null) {
//        bootbox.alert('Receipt Date Required!');
//        return false;
//    }



//    for (var i = 0; i < items.length; i++) {
//        var item = {};
//        if (document.getElementsByClassName("CastRcpQty")[i].value != 0) {
//            item.CastingReceiptDbkey = document.getElementsByClassName("CastingReceiptDbkey")[i].value;
//            item.CastingDbkey = document.getElementById("GlobalCastingDbkey").value;
//            item.CastingItemKey = document.getElementsByClassName("CastingItemKey")[i].value
//            item.ReceiptNumber = document.getElementById("ReceiptNumber").value;
//            item.ReceiptDate = document.getElementById("ReceiptDate").value
//            item.Qty = document.getElementsByClassName("CastRcpQty")[i].value;
//            castingReceiptViewModels.push(item);
//        }
//    }




//    var postData = JSON.stringify(castingReceiptViewModels);
//    $.ajax({
//        type: 'POST',
//        url: UrlCreateEditCastingReceipt,
//        data: postData,
//        dataType: "json",
//        contentType: "application/json; charset=utf-8",
//        success: function (data) {
//            if (data.success) {
//                bootbox.alert('Submitted Successfully', function () {
//                    window.location.reload();
//                });
//            } else {
//                bootbox.alert('Failed !', function () {

//                });
//            }
//        }
//    })

//}
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

// Old method name:GetCastingReceiptSplitModel
function GetReceiptSplitModel(CastingDbkey, splitItemKey) {
    var urlinput = "/Orders/ReceiptItemSplitModel" + "?CastingDbkey=" + CastingDbkey + "&CastingSplitKey=" + splitItemKey;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Receipt",
            message: response,
            size: 'extra-large',
            closeButton: true,
            // AttachmentDoc(Receipt_dbkey);
        });
        ApplyAutoCompleteOnRemarks();
        applyTextAreaContextMenu();
        //document.getElementById("SplitRecipts").innerHTML = response;
        // AttachmentDoc(Receipt_dbkey);
    });
}


function applyTextAreaContextMenu() {
    $.contextMenu({
        selector: '.serialNoCtrl',
        callback: function (key, options) {
            if (key === "insertSerialNo") {
                var rangeInput = prompt("Enter the range (e.g., '212/A/40 to 212/A/48', '1 to 50', '41/42/A to 41/55/A', or '222/A to 226/A'):");
                if (rangeInput) {
                    var range = parseComplexRange(rangeInput);
                    if (range) {
                        var list = generateComplexList(range);
                        if (list) {
                            appendToTextArea(options.$trigger, list.join(', '));
                        } else {
                            alert("Error generating the list. Please check the range format.");
                        }
                    } else {
                        alert("Invalid range format. Please enter in the correct format.");
                    }
                }
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

function parseComplexRange(text) {
    // Match ranges with a common prefix and suffix and varying numeric part
    var match = text.match(/(.*?)(\d+)(.*)\s*to\s*\1(\d+)\3/i);
    if (match) {
        return {
            prefix: match[1],
            start: parseInt(match[2], 10),
            end: parseInt(match[4], 10),
            suffix: match[3]
        };
    }

    // Match simple range format e.g., "1 to 50"
    match = text.match(/(\d+)\s*to\s*(\d+)/i);
    if (match) {
        return {
            prefix: "",
            start: parseInt(match[1], 10),
            end: parseInt(match[2], 10),
            suffix: ""
        };
    }

    return null;
}

function generateComplexList(range) {
    var list = [];
    for (var i = range.start; i <= range.end; i++) {
        list.push(range.prefix + i + range.suffix);
    }
    return list;
}

function appendToTextArea($textarea, content) {
    var currentContent = $textarea.val();
    $textarea.val(currentContent + (currentContent ? ', ' : '') + content);
}

   

function SaveSplitItemModel(CastingDbkey) {
    var form = $('#CastingSplitModelData');
    $.validator.unobtrusive.parse("#" + form.attr("id"));
    $(form).validate();
    //console.log($(form).serialize());
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
       // url: UrlSubmitCastingReceiptItemSplitModel,
        url: "/Orders/SaveReceiptSplitModel",
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

function DeleteOrderComponents(itemdom, key, table) {
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
                    url: "/Orders/DeleteOrderComponents",
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
                url: '/Orders/DeleteReceiptDocument?documentId=' + Attachment_Db_Key + '&receiptDbkey=' + Source_table_key,
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
    //console.log(selectedValue);
    //console.log(Attachment_Db_Key);
    $.ajax({
        type: 'GET',
        url: '/Orders/UpdateReceiptDocType?documentId=' + Attachment_Db_Key + '&doctype=' + selectedValue,
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

function apply_Select2_Datatable() {
    $('#new-generic-material-issue').DataTable({
        paging: false,
      //  order:false 
    });

    $('.select-serialNos').select2({
       placeholder: "Select options",
    });

    $('.select-ForEngine').select2({
        placeholder: "Select",
    });

    $('.select-vendor').select2();
    
}

function saveGenericMaterialIssue(orderType) {
    var combinedViewModel = new FormData();
    var isValid = true;
    var form = $('#GenericMaterialIssueForm');
    $.validator.unobtrusive.parse("#" + form.attr("id"));
    $(form).validate();
    if (form.valid() == false) {
        return false;
    }

    var vendorKey = $('#issued-vendor').val();
    var reference_No = $('#referenceNo').val();
    var issueDate = $('#issuedDate').val();
    var issueDbKey = $('#issueDbKey').val();
    if (vendorKey == 0) {
        alert('Please select a vendor from the list');
        isValid = false;
        return false;
    }

    var IssueDetails = {
        VendorKey: vendorKey,
        Reference_No: reference_No,
        IssueDate: issueDate,
        Issue_type: orderType,
        IssueDbKey: issueDbKey,

    };

    var GenericMaterialIssue = [];
    var DataForValidation = [];
    $('#new-generic-material-issue tbody tr').each(function (rowNo) {

        var issueQty = $(this).find('.issue-qty').val();
        var issueSlNos = $(this).find('.select-serialNos').val();
        var issueSlNosCSV = issueSlNos.join(',');
        var qtySplitKey = $(this).find('.qtySplitKey').val();
        var qtyAvailable = $(this).find('.qtyAvailable').text();
        var qtyPerEngie = $(this).find('.qtyPerEngine').text();
        var issueItemKey = $(this).find('.issueItemKey').val();
        var forEngine = $(this).find('.select-ForEngine').val();
        var forEngineCSV = forEngine.join(',');
        var JobCardNumber = $(this).find('.JobCardNumber').val();
        var JCFileLocation = $(this).find('.JCFileLocation').val();
        var JCFileName = $(this).find('.JCFileName').val();
        var engine_Part_Dbkey = $(this).find('.engine_Part_Dbkey').val();
        issueQty = Number(issueQty);
        //console.log('issue qty',issueQty);
        //console.log(issueSlNos.length);
        //console.log(issueQty !== issueSlNos.length);
        //if (issueQty === 0) {
        //    isValid = false;
        //    alert('Issued quantity cannot be zero (Row ' + (rowNo + 1) + ').');
        //    return false;
        //}
        //if (issueSlNos.length === 0) {
        //    isValid = false;
        //    alert('No serial numbers selected (Row ' + (rowNo + 1) + ').');
        //    return false;
        //}

        if (issueQty !== 0 && issueSlNos.length !== 0) {

            if (issueQty !== issueSlNos.length) {
                isValid = false;
                alert('Issued quantity and number of serial numbers must be same (in row number ' + (rowNo + 1) + ')');
                return false;
            }
            if (issueQty > qtyAvailable) {
                isValid = false;
                alert('Issued quantity cannot be greater than available quantity (in row number ' + (rowNo + 1) + ')');
                return false;
            }
            var engineSelectedCount = forEngine ? forEngine.length : 0; // Count the number of selected items
            
            var minIssueQty = engineSelectedCount * qtyPerEngie;
            //if (issueQty < minIssueQty) {
            //    isValid = false;
            //    alert('Issue quantity cannot be less than quantity per engine (in row number ' + (rowNo + 1) + ')');
            //    return false;
            //}
            var validationData = {
                Engine_Part_Dbkey : engine_Part_Dbkey,
                EngineSelectedCount: engineSelectedCount,
                RowNo: rowNo,
                IssueQty: issueQty,
                ForEngine: forEngineCSV,
                QtyPerEngie: qtyPerEngie

            };
            DataForValidation.push(validationData);
            var rowData = {
                IssueQty: issueQty,
                IssueSlNos: issueSlNosCSV,
                QtySplitKey: qtySplitKey,
                IssueItemKey: issueItemKey,
                ForEngine: forEngineCSV,
                JobCardNumber: JobCardNumber,
                JCFileLocation: JCFileLocation,
                JCFileName: JCFileName

            };

            GenericMaterialIssue.push(rowData);
            //console.log("Context of this:", this);

            var fileInput = $(this).find('.orderItemFiles')[0]; // Get the first matched element
           // console.log("File input element:", fileInput);

            // Log the files property directly
           // console.log("Files selected:", fileInput.files);

            if (fileInput && fileInput.files.length > 0) {
                for (var j = 0; j < fileInput.files.length; j++) {
                    // Appending each file to FormData object
                    combinedViewModel.append(rowNo + 1, fileInput.files[j]);
                }
              //  console.log("Files added to FormData:", fileInput.files);
            } else {
               // console.log("No files selected.");
            }
          //  console.log(GenericMaterialIssue);

        }
    });
    

   //---------------------Validation for issue qty v/s qty per engine----------------------------
   // console.log(GenericMaterialIssue);
  //  console.log(DataForValidation);
    // Step 1: Group by Engine_Part_Dbkey
    const groupedData = DataForValidation.reduce((acc, { Engine_Part_Dbkey, IssueQty, ForEngine, QtyPerEngie, RowNo }) => {
        if (!acc[Engine_Part_Dbkey]) {
            acc[Engine_Part_Dbkey] = {
                uniqueForEngines: new Set(),
                totalIssueQty: 0,
                QtyPerEngie: QtyPerEngie, // Store QtyPerEngie for each group
                RowNo: RowNo
            };
        }

        // Add each ForEngine to the set (automatically handles uniqueness)
        if (ForEngine) {
            ForEngine.split(',').forEach(engine => acc[Engine_Part_Dbkey].uniqueForEngines.add(engine.trim()));
        }

        // Add the IssueQty
        acc[Engine_Part_Dbkey].totalIssueQty += IssueQty;

        return acc;
    }, {});

    // Step 2: Transform into an array of results
    const result = Object.keys(groupedData).map(key => {
        const { uniqueForEngines, totalIssueQty, QtyPerEngie, RowNo } = groupedData[key];
        return {
            Engine_Part_Dbkey: key,
            UniqueForEngineCount: uniqueForEngines.size,
            TotalIssueQty: totalIssueQty,
            QtyPerEngie: QtyPerEngie,
            RowNo: RowNo
        };
    });

    //console.log(result);

    result.forEach(item => {
        const totalIssueQty = item.TotalIssueQty;
        const qtyPerEngie = parseInt(item.QtyPerEngie); // Convert QtyPerEngie to number

        if (totalIssueQty >= qtyPerEngie) {
           } else {
            console.log(`For Engine Part ${item.Engine_Part_Dbkey}, the total issue quantity (${totalIssueQty}) is less than the quantity per engine (${qtyPerEngie}).`);
            isValid = false;
                isValid = false;
            alert('Issue quantity cannot be less than quantity per engine (in row number ' + (item.RowNo + 1) + ')');
                return false;
        }
    });

   // return false;
    if (GenericMaterialIssue.length < 1) {
        alert("Fill atleast one row completely before saving");
        return false;
    }

    //combinedViewModel.append('casting_MaterialIssue_VM', IssueDetails);
    combinedViewModel.append('casting_MaterialIssue_VM', JSON.stringify(IssueDetails));
    combinedViewModel.append('casting_MaterialIssue_Items_VM', JSON.stringify(GenericMaterialIssue));

    //console.log(JSON.stringify(GenericMaterialIssue));

    if (!isValid) {
        return false;
    }

    $.ajax({
        type: "POST",
        url: "/Orders/SaveGenericMaterialIssue",
        data: combinedViewModel, // The FormData object
        cache: false,
        contentType: false,
        processData: false,
        success: function (data) {
            if (data.success) {
                bootbox.alert('Saved Successfully!', function () {
                    window.location.href = '/Orders/OrdersList/?Ordertype=' + data.orderType + "&tab=Issues-tab";
                  //  window.location.reload();
                });
            } else {
                alert('Failed to save');
            }
        },
        error: function (error) {
            alert('Failed to save');
            console.error(error);
        }
    });
}
//    combinedViewModel.append('casting_MaterialIssue_VM', JSON.stringify(IssueDetails));
//    combinedViewModel.append('casting_MaterialIssue_Items_VM', JSON.stringify(GenericMaterialIssue));


//    //var combinedViewModel = {
//    //    casting_MaterialIssue_VM: IssueDetails,
//    //    casting_MaterialIssue_Items_VM: GenericMaterialIssue
//    //};
//    console.log(combinedViewModel);
//    $.ajax({
//        type: "POST",
//        url: "/Orders/SaveGenericMaterialIssue",
//        contentType: 'application/json',
//        //data: JSON.stringify(combinedViewModel),
//        data: combinedViewModel,
//        success: function (data) {
//            if (data.success) {
//                bootbox.alert('Saved Successfully !', function () {
//                    window.location.href = '/Orders/OrdersList/?Ordertype=' + data.orderType;
//                });
//            } else {
//                alert('Failed to save');
//            }
//        },
//        error: function (error) {
//            alert('Failed to save');
//            console.error(error);
//        }
//    });
//}

function deleteGenericMaterialIssue(IssueDbkey) {
    bootbox.confirm('Confirm delete this Material Issue', function (result) {
        if (result) {
            var urlinput = "/Orders/DeleteGenericMaterialIssue" + "?IssueDbkey=" + IssueDbkey;
            $.get(urlinput).done(function (response) {
                if (response.success) {
                    bootbox.alert('Deleted Successfully !', function () {
                        window.location.reload();
                    });
                }
                else {
                    alert("Failed to delete");
                }
            });
        } 
    });
}

function deleteGenericMaterialIssue_Item(issueItemKey) {
    if (issueItemKey != 0) {
        bootbox.confirm('Confirm delete this row', function (result) {
            if (result) {
                var urlinput = "/Orders/DeleteGenericMaterialIssue_Item" + "?IssueItemKey=" + issueItemKey;
                $.get(urlinput).done(function (response) {
                    if (response.success) {
                        window.location.reload();
                    }
                    else {
                        alert("Failed to delete");
                    }
                });
            }
        });
    }
   

}
function showRemainingData(orderType,issueDbKey) {
    var urlinput = "/Orders/RemainingData?OrderType=" + orderType + "&IssueDbKey= " + issueDbKey ;
    $.get(urlinput).done(function (response) {
        document.getElementById('showRemainingData').innerHTML = response + '<input type="hidden" id="issueDbKey" value=' + issueDbKey + ' />'; 
        $('#remaining-generic-material-issue').DataTable({
            paging: false,
            columnDefs: [{ width: '30%', targets: 5 }]
        });
    });
   
}

function addRow_GenericMaterialIssue(element,orderType) {
    var row = element.closest('tr');
   
    var IssueDbKey = document.getElementById('issueDbKey').value;
    //var qtySplitKey = $(row).find('.qtySplitKey').val();
    //var IssueQty = $(row).find('.qtyAvailable').val();
    //var hiddenSerialNos = $(row).find('.hiddenSerialNos').val();
    var casting_MaterialIssue_item = {
        IssueDbKey : document.getElementById('issueDbKey').value ,
        QtySplitKey: $(row).find('.qtySplitKey').val(),
        IssueQty: $(row).find('.qtyAvailable').text(),  
        IssueSlNos: $(row).find('.hiddenSerialNos').val(),
    }
   /* console.log(casting_MaterialIssue_item);*/
  
    $.ajax({
        type: "POST",
        url: "/Orders/AddRowToGenericMaterialIssue_Items",
        contentType: 'application/json',
        data: JSON.stringify(casting_MaterialIssue_item),
        success: function (data) {
            if (data.success) {
                //alert("added row");
                //showRemainingData(orderType, IssueDbKey);
                window.location.reload();

            } else {
                alert('Failed to save');
            }
        },
        error: function (error) {
            alert('Failed to save');
            console.error(error);
        }
    });
}

function GetGenericMaterialIssueSummary(orderType) {
    var urlinput = "/Orders/GenericMaterialIssueSummary" + "?OrderType=" + orderType ;
    $.get(urlinput).done(function (response) {
        document.getElementById('tab-Issues_Summary').innerHTML = response ;
        //$('#genericMaterialIssueSummaryTbl').DataTable({
        //    paging: false,
        //});
    });

}
function GetGenericMaterialIssue_Vendorwise(Engine_Part_Dbkey, action,orderType) {

    var targetElement = document.getElementById("Part-" + Engine_Part_Dbkey);
    var IconElementOpen = document.getElementById("icon_Open-" + Engine_Part_Dbkey);
    var IconElementClose = document.getElementById("icon_Close-" + Engine_Part_Dbkey);
 
    $(IconElementOpen).toggle();
    $(IconElementClose).toggle();

    if (action == "Close") {
        $(targetElement).toggle();
        return false;
    }

    // var urlinput = '/Orders/GenericMaterialIssueSummary_Vendorwise?OrderType=' + orderType + '&Engine_PartKey=' + Engine_Part_Dbkey;
    var urlinput = '/Orders/GenericMaterialIssueSummary_IssueHistory?OrderType=' + orderType + '&Engine_PartKey=' + Engine_Part_Dbkey;
    $.get(urlinput).done(function (response) {
        targetElement.children[0].innerHTML = response;
       targetElement.style.display = "";
    });
}

function GenericMaterialIssue_Split(orderType, Engine_PartKey) {
    var urlinput = '/Orders/GenericMaterialIssue_Split?OrderType=' + orderType + '&Engine_PartKey=' + Engine_PartKey;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Issue Detail",
            message: response,
            size: 'extra-large',
            closeButton: true,
        });
    });
}
function UpdateJobCardUploadCtrlColour(ctrl) {
    var td = ctrl.parentNode;
    if (td.children[0].files.length > 0) {
        td.children[3].style.color = 'green';
        // td.children[4].innerText = ctrl.files[0].name;
        // td.children[4].href = 

        //console.log(ctrl.files[0]);
    } else {
        td.children[3].style.color = 'royalblue';
    }
}
//function UpdateJobCardUploadCtrlColour(ctrl) {

//    var td = ctrl.parentNode;
//    if (td.children[0].files.length > 0) {
//        td.children[3].style.color = 'green';
//        console.log(ctrl.files[0]);
//    } else {
//        td.children[3].style.color = 'royalblue';
//    }
//}
function OpenGenricFileDialog(ctrl) {
    var tr = ctrl.closest('tr'); // Get the closest <tr> ancestor
    var jobCardInput = tr.querySelector('.JobCardNumber'); // Use querySelector for the Job Card Number input

    if (jobCardInput) {
        var JobCardNumber = jobCardInput.value; // Get the value of the Job Card Number input

        if (JobCardNumber === "") {
            alert("Please enter Job Card Number");
            return;
        }
    } else {
        alert("Job Card Number input not found.");
        return;
    }

    var td = ctrl.closest('td'); // Get the closest <td> ancestor
    var fileInput = td.querySelector('.orderItemFiles'); // Use querySelector for the file input

    if (fileInput) {
        fileInput.click(); // Trigger the file input click
    } else {
        console.error("File input not found in the current cell.");
    }
}



function UploadCastingMaterialIssueDocument(IssueDbKey, ViewType) {
   // console.log(IssueDbKey);
    if (IssueDbKey === 0) {
        return false;
    }
    var urlinput = "/Orders/GenericMaterialIssueDocument?id=" + IssueDbKey + '&Type=' + ViewType;
    $.get(urlinput).done(function (response) {
        document.getElementById('GenericMaterialIssueDocumentsDiv').innerHTML = response;
    });
}

function SaveMaterialIssueDocuments(MaterialIssueDBkey) {
    var formData = new FormData();

    var MaterailIssueUploadFiles = document.getElementsByClassName("MaterailIssueDocUploadFiles");

    var fileData = [];

    for (var i = 0; i < MaterailIssueUploadFiles.length; i++) {
        if (MaterailIssueUploadFiles[i].files.length > 0) {

            var fileItem = {};
            fileItem.Source_table_key = MaterialIssueDBkey;
            fileItem.Source_table = "Casting_MaterialIssue";
            fileItem.File_DVD_Num = document.getElementsByClassName("fileAttachmentType")[i].value;
            fileItem.File_Revision = document.getElementsByClassName("MaterialIssueUploadfileRefNum")[i].value;
            fileData.push(fileItem);
            formData.append('files', MaterailIssueUploadFiles[i].files[0]);
        }
    }
    formData.append('filesData', JSON.stringify(fileData));

    $.ajax({
        url: "/Orders/SaveGenericMaterialIssueDocument",
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            if (response.success) {
                bootbox.alert('Submitted Successfully', function () {
                    window.location.reload();
                });
            } else {
                bootbox.alert('Failed to save file');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);

        }
    });
}

function CloneMaterialIssueDocumentRow() {
    var clonedRow = $('#MaterialIssueDocumentTable tbody tr:first').clone(); // Clone the last row
    clonedRow.find('input').val(''); // Clear input values if needed
    clonedRow.show();
    $('#MaterialIssueDocumentTable').append(clonedRow);

}

function DelMaterialIssueDocData(id, issueDbkey) {
    bootbox.confirm("Are you sure you want to delete this Material Issue Document ", function (result) {
        if (result) {
            $.ajax({
                type: 'GET',
                url: '/MaterialIssue/DeleteMaterialIssueDocument?documentId=' + id,
                success: function (data) {
                    if (data.success) {
                        window.location.reload();
                    }
                    else {
                        bootbox.alert("Delete Failed");
                    }
                },
                error: function () {
                }
            });
        } else {

        }
    });
}

function SubmitOrderModuleUserMapping() {
    // Initialize an array to collect selected user data
    const selectedMappings = [];
    $(".ModuleUserselected:checked").each(function () {
        selectedMappings.push({
            Id: $(this).data("mappingid"),           // Mapping ID
            OrderId: $(this).data("orderid"),       // Order ID
            OrderType: $(this).data("ordertype"),   // Order Type
            UserGuid: $(this).data("userguid"),     // User GUID
            UserName: $(this).closest("tr").find("td:last").text().trim()
        });
    });
    //console.log(JSON.stringify(selectedMappings));
    // Verify data before sending
    if (selectedMappings.length === 0) {
        alert("No users selected!");
        return;
    }

    // Send data using AJAX
    $.ajax({
        url: '/Orders/OrderModuleCustomAccess', // Adjust your controller URL
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(selectedMappings), // Convert to JSON
        success: function (response) {
            if (response.success) {
                bootbox.alert('Submitted Successfully', function () {
                    bootbox.hideAll();
                });
            } else {
                alert("Failed to save data.");
            }
        },
        error: function (xhr, status, error) {
            console.error("AJAX Error:", status, error);
            alert("An error occurred while submitting data.");
        }
    });
}

function getCommentSection(CastingDbkey, CastingReceiptsItemSplitKey) {
    //console.log(CastingDbkey, CastingReceiptsItemSplitKey);
    var urlinput = '/Orders/ReceiptComments?CastingDbkey=' + CastingDbkey + '&CastingReceiptsItemSplitKey=' + CastingReceiptsItemSplitKey;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Remarks",
            message: response,
            size: 'large',
            closeButton: true,
        });
    });
}

function autoSaveCastingRemarks(commentElement) {
  //  console.log(commentElement);
    var data = $(commentElement).data();
 //   console.log(data);
    var comments = commentElement.value;
    const castingreceiptsitemsplitkey = data.castingreceiptsitemsplitkey;
    const castingreceiptscommentskey = data.castingreceiptscommentskey;
    const departmentid = data.departmentid;
    //console.log(castingreceiptscommentskey);
    $.ajax({
        url: '/Orders/AutoSaveCastingRemarks', // Make sure to update this URL with the correct controller action
        type: 'POST',
        data: {
            CastingReceiptsCommentsKey: castingreceiptscommentskey,
            CastingReceiptsItemSplitKey: castingreceiptsitemsplitkey,
            DepartmentId: departmentid,
            Comments: comments
        },
        success: function (response) {
            if (response.success === true) {
                $(commentElement).notify("Updated Successfully", {
                    className: "success",
                    autoHideDelay: 1000,
                    position: 'right', // Use fixed positioning to place it on top
                });
            }
        },
        error: function (error) {
            console.log("Error saving remarks:", error);
        }
    });
}

function addDepartmentRow() {
    // Get the table body by using the table's id
    var tbody = document.querySelector('#departmentTable tbody');

    // Get the hidden row template (with class "template-row")
    var hiddenRow = tbody.querySelector('tr.template-row');

    // Clone the hidden row
    var newRow = hiddenRow.cloneNode(true);

    // Make the new row visible
    newRow.style.display = '';

    // Clear the input values in the new row (optional, in case there's data)
    var displayOrderInput = newRow.querySelector('.DisplayOrder');
    var departmentSelect = newRow.querySelector('.Department');

    displayOrderInput.value = ''; // Clear the display order input
    departmentSelect.selectedIndex = 0; // Reset the department dropdown

    // Append the new row to the table body
    tbody.appendChild(newRow);
}

function DepartmentOrder() {
    var urlinput = '/Orders/RemarkDepartmentOrder';
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Department Order",
            message: response,
            size: 'medium',
            closeButton: true,
        });
    });
}

function saveDepartmentData() {
   
    var table = document.querySelector('#departmentTable');
    var rows = table.querySelectorAll('tbody tr'); // Get all rows
    rows = Array.from(rows).slice(1); // Skip the first row (index 0)
    var rowData = [];

    rows.forEach(function (row) {
      
        var displayOrder = row.querySelector('.DisplayOrder').value;
        var dptOrderId = row.querySelector('.DptOrderId').value;
        var departmentSelect = row.querySelector('.Department');
        var departmentId =departmentSelect.value; 
       
        var rowModel = {
            Id: dptOrderId ? parseInt(dptOrderId) : 0, 
            DepartmentID : departmentId,
            DisplayOrder : parseFloat(displayOrder)
        };
        if (displayOrder === null || displayOrder === 0 || displayOrder === '') {
            alert("Please add valid Display Order for all rows");
            return false;
        }
        if (departmentId === null || departmentId === '0') {
            alert("Please select a Department");
            return false;
        }
        rowData.push(rowModel);
    });
    if (rowData.length < 1) {
        return false;
    }
    var data = JSON.stringify(rowData);
   // console.log(data);
   
    $.ajax({
        url: '/Orders/SaveDepartmentOrder', // Make sure to update this URL with the correct controller action
        type: 'POST',
        data: data,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            if (response.success === true) {
                bootbox.alert({
                    message: "Saved Successfully",
                    callback: function () {
                        bootbox.hideAll();
                    }
                });

            }
            else {
                bootbox.alert("Error saving data");
            }
        },
        error: function (error) {
            console.log("Error saving remarks:", error);
        }
    });
}


