//const { detectOverflow } = require("@popperjs/core");

//const { forEach } = require("angular");


window.toggleReadMore = function (contentId, clickedEl) {
    var wrapper = clickedEl.closest('.readmore-wrapper');
    if (!wrapper) return false;

    var content = wrapper.querySelector('.readmore-text');
    var link = wrapper.querySelector('.readmore-toggle');
    if (!content) return false;

    if (content.classList.contains('collapsed')) {
        content.classList.remove('collapsed');
        content.style.setProperty('max-height', 'none', 'important');
        content.style.overflow = 'visible';
        content.setAttribute('data-expanded', 'true');
        if (link) link.innerText = 'Read less';
    } else {
        content.classList.add('collapsed');
        content.style.removeProperty('max-height');
        content.style.overflow = 'hidden';
        content.removeAttribute('data-expanded');
        if (link) link.innerText = 'Read more';
    }
    return false;
};

window.initializeReadMore = function () {
    document.querySelectorAll('.readmore-wrapper').forEach(function (wrapper) {
        var text = wrapper.querySelector('.readmore-text');
        var toggle = wrapper.querySelector('.readmore-toggle');
        if (!text || !toggle) return;

        // Reset first
        text.classList.remove('collapsed');
        text.style.removeProperty('max-height');
        text.style.overflow = 'visible';
        toggle.style.display = 'none';
        toggle.innerText = 'Read more';

        // Get full height without any clipping
        var fullHeight = text.scrollHeight;
        var lineHeight = parseFloat(window.getComputedStyle(text).lineHeight) || 20;
        var twoLinesHeight = lineHeight * 2;

        // Only show Read more if content is taller than 2 lines
        if (fullHeight > twoLinesHeight + 5) {
            text.classList.add('collapsed');
            text.style.overflow = 'hidden';
            toggle.style.display = 'inline-block';
        } else {
            // Short text - no need for Read more
            text.classList.remove('collapsed');
            toggle.style.display = 'none';
        }
    });
};



var jc;
var demandItemsTbl;

function toggleDemandTree(action) {
    if (action == "open") {
        document.getElementById("ToggleOpenControl").style.display = "none";
        document.getElementById("treeControlPanel").style.display = "";
        document.getElementById("treeControlPanel").className = "col-md-4 row";
        document.getElementById("demand-detail-area").className = "col-md-8";
    }

    if (action == "close") {
        document.getElementById("ToggleOpenControl").style.display = "";
        document.getElementById("treeControlPanel").style.display = "none";
        document.getElementById("treeControlPanel").className = "";
        document.getElementById("demand-detail-area").className = "col-md-12";
    }
}


function CloneSplitDocumentRow() {
    var clonedRow = $('#SplitDocumentsTable tbody tr:first').clone(); // Clone the last row
    clonedRow.show();
    $('#SplitDocumentsTable').append(clonedRow);
}
function DeleteUnsavedDocs(btn) {
    $(btn).closest('tr').remove();
}

function SaveSplitItemModel(Receipt_dbkey) {
    var form = $('#SplitModelData');
    $.validator.unobtrusive.parse("#" + form.attr("id"));
    $(form).validate();
    //  console.log($(form).serialize());
    if (form.valid() == false) {
        return false;
    }
    var formData = new FormData();

    var formElements = $(form).serializeArray();
    var jsonResult = {};

    $.each(formElements, function (index, element) {
        jsonResult[element.name] = element.value;
    });

    //var jsonData = JSON.stringify(jsonResult); 

    formData.append('jsonData', JSON.stringify(jsonResult));

    var splitUploadFiles = document.getElementsByClassName("splitUploadFiles");

    //for (var i = 0; i < fileInput.files.length; i++) {
    //    formData.append('files', fileInput.files[i]);
    //}

    var fileData = [];

    for (var i = 0; i < splitUploadFiles.length; i++) {
        if (splitUploadFiles[i].files.length > 0) {
            var fileItem = {};
            var fileControl = splitUploadFiles[i];
            var fileRow = $(fileControl).closest('tr');
            fileItem.Source_table_key = document.getElementById("modelItem_Receipt_dbkey").Value;
            fileItem.Source_table = "Procurement_Demand_Receipts";
            fileItem.File_DVD_Num = $(fileControl).closest('tr').find('td:first-child').children().first().val(); //fileRow.cells[1].children[0].value;
            fileItem.File_Revision = $(fileControl).closest('tr').find('td:nth-child(2)').children().first().val();
            fileData.push(fileItem);
            formData.append('files', splitUploadFiles[i].files[0]);
        }
    }
    // console.log(JSON.stringify(fileData));
    formData.append('filesData', JSON.stringify(fileData));
    // console.log(fileData);
    // console.log(formData);
    // data: { Procurement_Demand_Items_Split: JSON.stringify(Procurement_Demand_Items_Split) }, 
    $.ajax({
        url: '/DemandManagement/SaveReceiptItemSplitsModel',
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            alert('Submitted Successfully');
            GetViewDemandItemReciptSplit(Receipt_dbkey);
            var btn = document.getElementsByClassName("bootbox-close-button")[1];
            btn.click();
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);


        }
    });
}

function getStatusBadge() {
    var demanddbkey = $('#DemandDbkeyGlobalKey').val();
    $.ajax({
        type: "Get",
        url: "/DemandManagement/DemandStatusBadge?Id=" + demanddbkey,
        success: function (data) {
            document.getElementById("statusDisplayArea").innerHTML = data;
            SetTreeHeight();
        }
    });
    //getStatusTable(demandKey);
}
function getStatusTable(demandKey) {
    $.ajax({
        type: "Get",
        url: "/DemandManagement/DemandStatusTable?Id=" + demandKey,
        success: function (data) {
            document.getElementById("tab-timeline").innerHTML = data;
        }
    });
}

function EditDemandHistory(id) {
    $.ajax({
        type: "GET",
        url: "/DemandManagement/EditDemandHistory/" + id,
        cache: false,
        success: function (data) {
            bootbox.alert({
                message: data,
                title: "Edit Procurement Timeline",
                size: 'Small',
                buttons: {
                    ok: {
                        className: 'd-none'
                    }
                }
            });
        }
    });
    return false;
};

function SaveDemandHistory(form) {
    $.validator.unobtrusive.parse("form");
    if ($(form).valid()) {
        $.ajax({
            type: "POST",
            url: form.action,
            data: $(form).serialize(),
            cache: false,
            success: function (data) {
                if (data.success) {
                    bootbox.hideAll();
                    getStatusBadge(data.demandID);
                    //getStatusBadge(data.demandID);
                }
            }
        });
    }

    return false;
};

var isVisible = 0;
var dtree = $.jstree.reference('#Demandjstree');
function LoadDemandJstree() {
    $(".search-input").keyup(function () {
        var searchString = $(this).val();
        var searchResult = $('#Demandjstree').jstree('search', searchString);
        isVisible = $(searchResult).find('.jstree-search').length;
        document.getElementById("jstreeSearchcount").innerHTML = isVisible;
    });
    $.ajax({
        type: "Get",
        url: "/DemandManagement/GetDemandJsTreeData",
        success: function (data) {
            var nodelog = $('#Demandjstree').jstree(
                {
                    'core': {
                        'data': data,
                    },
                    "search": {
                        "case_insensitive": true,
                        "show_only_matches": true,
                        "show_only_matches_children": true
                    },
                    "plugins": ["html_data", "dnd", "search", "types", "adv_search"],
                }).on('changed.jstree', function (e, data) {
                    var nodedata = data.instance.get_node(data.selected[0]).id;
                    //GetComponentDetail(nodedata);
                }).bind('ready.jstree', function (e, data) {
                    // LoadProjectDemand(2);
                }).on('after_open.jstree after_close.jstree', function (e, data) {

                });

        }
    });
}

function GetComponentDetail(nodeid) {
    //var ComponentType = nodeid.split('_')[0];
    //var Key = nodeid.split('_')[1];
    //if (ComponentType == "Demand") {
    //    ShowDemandDetail(Key);
    //} else if (ComponentType == "Project") {
    //    LoadProjectDemand(Key)
    //}
}

function getDemandInfo(ctrl) {
    //console.log(ctrl);
    ShowDemandDetail(ctrl.value);
}

//Load all Demand Detail ; Basic info, Items , and documents
function ShowDemandDetail(Id) {
    if (Id == 0) {
        return false;
    }
    var urlinput = "/DemandManagement/DemandDetails?Id=" + Id;
    $.get(urlinput).done(function (response) {
        document.getElementById("DemandManagerDiv").innerHTML = response;
        document.getElementById("DemandDbkeyGlobalKey").value = Id;
        getStatusBadge();
        GetViewDemand();
        GetViewDemandItems();
        GetDemandDocuments();
        GetDemandReceiptDocuments();
        EnableShortClosureOption();
    });
}


function expandDemandArea() {
    document.getElementById("statusDisplayArea").style.display = "none";
    //document.getElementById("treeControlPanel").style.display = "none";
    document.getElementById("demand-area-toggle").innerText = "Collapse";

    // $("#demand-detail-area").attr("class", "col-12");
    $("#demand-area-toggle").attr("onclick", "collapseDemandArea()");
}

function collapseDemandArea() {
    document.getElementById("statusDisplayArea").style.display = "";
    // document.getElementById("treeControlPanel").style.display = "";
    document.getElementById("demand-area-toggle").innerText = "Expand";
    // $("#demand-detail-area").attr("class", "col-9");
    $("#demand-area-toggle").attr("onclick", "expandDemandArea()");

}

//function GetDemand() {
//   // var demanddbkey = $('#DemandDbkeyGlobalKey').val();
//    var urlinput = "/DemandManagement/CreateDemand?Id=" + demanddbkey + "&Viewtype=Readonly";
//    $.get(urlinput).done(function (response) {
//        document.getElementById("tab-DemandDetail-list").innerHTML = response;
//    });
//}

function EditDemandDetail(Id) {
    var urlinput = "/DemandManagement/CreateDemand?Id=" + Id + "&Viewtype=Edit";
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Demand Detail",
            message: response,
            size: 'extra-large',
            closeButton: true,
            className: 'custom-modal',
        });
    });
}

function EnableShortClosureOption() {
    var demanddbkey = $('#DemandDbkeyGlobalKey').val();
    var urlinput = "/DemandManagement/DemandItemBalance?DemandDbkey=" + demanddbkey;
    $.get(urlinput).done(function (data) {
        $.each(data, function (index, data) {
            if (data.Balance > 0) {
                document.getElementById("ShortCloseLink").style.display = "block";
            }
        });
    });
}


function addMilestonecol() {
    $('#tbl-milestone-viewmode thead tr').each(function () {
        var lastColumn = $(this).find('th:last');
        var clonedContent = lastColumn.clone();
        $(this).append(clonedContent);
    });
    $('#tbl-milestone-viewmode tbody tr').each(function () {
        var lastColumn = $(this).find('td:last');
        var clonedContent = lastColumn.clone();
        $(this).append(clonedContent);
    });
}

function DeleteMilestonecol(MilestoneId) {
    //console.log(MilestoneId);
    bootbox.confirm({
        message: "Are you sure you want to delete this item?",
        buttons: {
            confirm: {
                label: 'Yes',
                className: 'btn-danger'
            },
            cancel: {
                label: 'No',
                className: 'btn-secondary'
            }
        },
        callback: function (result) {
            if (result) {
                var urlinput = "/DemandManagement/DeleteMilestoneColumn/?MilestoneId=" + MilestoneId;
                $.get(urlinput).done(function (response) {
                    bootbox.hideAll();
                    //  console.log(response);
                    if (response.success == true) {
                        GetViewMilestoneDetails(false);
                    }
                    else {
                        bootbox.alert("Failed to delete!!");
                    }
                });
            }
            else {
                bootbox.hideAll();
            }
        }
    });
    //var colIndex = $(ctrl).closest('td').index();

    //var columnToDelete = colIndex; 
    //$('#tbl-milestone-viewmode tbody tr').each(function () {
    //    $(this).find('td').eq(columnToDelete).remove();
    //});

    //// Remove the corresponding header column
    //$('#tbl-milestone-viewmode thead tr th').eq(columnToDelete).remove();
}


function SaveDemandMilestoneDetails() {
    document.getElementById("validationMessage").innerHTML = "";
    // get milestone dates
    var milestoneDates = [];
    $("#tbl-milestone-editmode input").each(function () {
        var dataType = $(this).data('field');
        if (dataType == "Milestone") {
            var milestonedate = {};
            milestonedate.DeliveryDate = $(this).val();
            milestonedate.Milestone = $(this).data('milestone');
            milestoneDates.push(milestonedate);
        }
    });


    // Validate Milestone Qty

    var items = document.getElementsByClassName("tblbodyItems");
    for (var i = 0; i < items.length; i++) {
        var tr = items[i];
        var milestones = tr.querySelectorAll('input[type="number"]');
        var QtyOrdered = milestones[0].dataset.orderqty;
        var itemName = milestones[0].dataset.componentname;
        var sumofQty = 0;
        for (var j = 0; j < milestones.length; j++) {
            sumofQty = sumofQty + parseFloat(milestones[j].value);
        }
        if (sumofQty > QtyOrdered) {
            document.getElementById("validationMessage").innerHTML = "Summation of milestone qty are not valid for " + itemName;
            return false;
        }
    }


    // get milestone qtys
    var procurement_Demand_MileStones = [];
    $("#tbl-milestone-editmode input").each(function () {
        var dataType = $(this).data('field');
        if (dataType == "MilestoneQty") {
            var milestoneQty = {};
            milestoneQty.Qty = $(this).val();
            milestoneQty.MilestoneDbKey = $(this).data('milestonedbkey');
            milestoneQty.Milestone = $(this).data('milestone');
            milestoneQty.DemandDbkey = $(this).data('demandkey');
            milestoneQty.DemandItemDbKey = $(this).data('itemkey');
            milestoneQty.DeliveryDate = $(this).data('itemkey');
            var milestoneDateInfo = milestoneDates.filter(function (mile) {
                return mile.Milestone === milestoneQty.Milestone; // Filtering condition
            });
            milestoneQty.DeliveryDate = milestoneDateInfo[0].DeliveryDate;
            milestoneQty.MilestoneID = $(this).data('milestoneid');
            procurement_Demand_MileStones.push(milestoneQty);
        }
    });

    var procurementMilestones = [];

    $('.MilestoneName').each(function () {
        var milestoneName = $(this).val(); // Get the value of the input
        var milestoneId = $(this).data('milestoneid'); // Get the data-milestoneid attribute
        /*var deliveryDate = $(this).data('DeliveryDate'); */
        //var comments = $(this).data('comments'); 
        //var status = $(this).data('status');
        //var qtyPercentage = $(this).data('qtyPercentage'); 
        //var isLastMilestone = $(this).data('isLastMilestone'); 
        //var description = $(this).data('description'); 

        var $dateInput = $(this).closest('.col').find('.milestonecls');
        var deliveryDate = $dateInput.val(); // Get the value of the date input field
        var $remarkInput = $(this).closest('.col').find('.milstoneRemarks');
        var remark = $remarkInput.val(); // Get the value of the date input field

        var milestoneNameDictionary = {};
        milestoneNameDictionary.MilestoneName = milestoneName;
        milestoneNameDictionary.MilestoneID = milestoneId;
        milestoneNameDictionary.DueDate = deliveryDate;
        milestoneNameDictionary.Comments = remark;
        //milestoneNameDictionary.Comments = comments;
        //milestoneNameDictionary.Status = status;
        //milestoneNameDictionary.QtyPercentage = qtyPercentage;
        //milestoneNameDictionary.IsLastMilestone = isLastMilestone;
        //milestoneNameDictionary.Description = description;

        procurementMilestones.push(milestoneNameDictionary);
    });
    //console.log(procurementMilestones);
    //return false;
    var completeData = {
        procurement_Demand_MileStones: procurement_Demand_MileStones,
        procurementMilestones: procurementMilestones
    };
    // console.log(completeData);
    // return false;
    bootbox.hideAll();
    let dialog = bootbox.dialog({
        message: '<p class="text-center mb-0"><i class="fas fa-spin fa-cog"></i> Please wait while saving this milestone ..</p>',
        closeButton: false
    });
    $.ajax({
        url: '/DemandManagement/SaveProcurementDemandMilestone',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(completeData),
        success: function (response) {
            bootbox.hideAll();
            if (response.success) {
                bootbox.alert({
                    message: "Saved Successfully",
                    callback: function () {
                        bootbox.hideAll();
                        GetViewMilestoneDetails(false);
                    }
                });
            }
            else {
                bootbox.alert({
                    message: "Failed to submit",
                    callback: function () {
                        bootbox.hideAll();
                        GetViewMilestoneDetails(true);
                    }
                });
            }

        },
        error: function (xhr, status, error) {
            bootbox.hideAll();
            console.error('Error:', error);
        }
    });

}


function ShortClosureDemand(revertShortClose = 0) {

    var html = "";

    if (revertShortClose == 1) {
        html = "Do you wish to revert short closure ?"
    } else {
        html = '<label>Date</label><input type="date" id="ShortCloseDate" class="form-control"/> <br/> <label>Remarks</label><textarea id="ShortCloseRemarks" class="form-control"></textarea>'
    }


    bootbox.confirm({
        title: 'Short Close Demand',
        message: html,
        callback: function (result) {
            if (result) {
                var formdata = new FormData();
                var demanddbkey = $('#DemandDbkeyGlobalKey').val();

                var urlinput = "/DemandManagement/DemandShortClose";

                if (revertShortClose == 0) {
                    var ShortClosedate = $('#ShortCloseDate').val();
                    var Remarks = $('#ShortCloseRemarks').val();

                    if (ShortClosedate == null || ShortClosedate == undefined || ShortClosedate == '') {
                        alert("Please enter short closed date");
                        return false;
                    } else if (Remarks == null || Remarks == undefined || Remarks == '') {
                        alert("Please mention the reason for short closure");
                        return false;
                    }

                    formdata.append("ShortClosedate", ShortClosedate);
                    formdata.append("Remarks", Remarks);
                }

                formdata.append("demanddbkey", demanddbkey);
                formdata.append("revertshortclose", revertShortClose);


                $.ajax({
                    type: 'POST',
                    url: urlinput,
                    data: formdata,
                    processData: false,
                    contentType: false,
                    //dataType: "json",
                    //contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        if (data.success) {
                            bootbox.alert('Submitted Successfully', function () {
                                ShowDemandDetail(demanddbkey);
                            });
                        } else {
                            bootbox.alert('Failed !', function () {

                            });
                        }
                    }
                })
            }
        }
    });

}



function GetViewDemand() {
    var demanddbkey = $('#DemandDbkeyGlobalKey').val();
    var urlinput = "/DemandManagement/ViewDemands?Id=" + demanddbkey;

    $.get(urlinput).done(function (response) {
        document.getElementById("tab-DemandDetail-list").innerHTML = response;
        GetViewMilestoneDetails(false);

    });
}

function ChangeDOStatus() {
    document.getElementById("DO_Review").value = document.getElementById("DO_review_checkbox").checked;
}

function SaveDemandDetail() {
    var form = $('#CreateDemand');
    $.validator.unobtrusive.parse("#" + form.attr("id"));
    $(form).validate();
    var customData = {};

    // Gather the checked checkbox value
    customData.DO_Review = $('#DO_Review').is(':checked');

    // Optionally, log the custom data for debugging
    //  console.log(form);

    // Append the custom data to the form
    //$('<input>').attr({
    //    type: 'hidden',
    //    name: 'DO_Review',
    //    value: customData.DO_Review
    //}).appendTo(form);
    if (form.valid() == false) {
        return false;
    }
    //var data = $("#CreateDemand").serialize();
    var data = $(form).serialize();
    //   console.log(data);
    $.ajax({
        type: 'POST',
        url: '/DemandManagement/CreateDemand',
        data: data,
        success: function (data) {
            //console.log(data);
            bootbox.alert(data.msg,
                function () {
                    if (data.success) {
                        bootbox.hideAll();
                        //   window.location.href = "/DemandManagement/DemandTree/" + data.demandid;
                        // closeBootboxes();
                        // getStatusBadge(); 
                        // GetViewDemand();
                    }

                });
        },
        error: function (data) { bootbox.alert(data.msg) }
    });
    return false;
}
//----------------------------------------Demand Items--------------------------------------------------------
function GetViewDemandItems() {
    $('#loading').show();
    var demanddbkey = $('#DemandDbkeyGlobalKey').val();
    var urlinput = "/DemandManagement/ViewDemandItems?Id=" + demanddbkey + "&Viewtype=ReadWrite";
    $.get(urlinput).done(function (response) {
        document.getElementById("tab-DemandItems").innerHTML = response;
        $('#loading').hide();
        // Initialize the DataTable
        demandItemsTbl = $('#ViewDemandItems').DataTable({
            scrollX: true,
            paging: false,
            order: false,
            stateSave: true,
            ordering: false, // Disable sorting for all columns
            stateSaveCallback: function (settings, data) {
                localStorage.setItem(
                    'DataTables_' + settings.sInstance,
                    JSON.stringify(data)
                );
            },
            stateLoadCallback: function (settings) {
                return JSON.parse(localStorage.getItem('DataTables_' + settings.sInstance));
            },
            //  sorting: false,
            // pageLength: 50,
            scrollY: 400,
            dom: '<"top"lfB>rtip',
            buttons: [
                'excel'
            ],
            fixedColumns: {
                leftColumns: 6
            },
            columnDefs: [
                { "orderable": true, "targets": [0, 1, 2, 3] } // Disable sorting on columns 1 (Age) and 3 (Office)
            ]
        });



        demandItemsTbl.draw();

    });
}

function OpenReceiptCols(className) {
    var cols = document.getElementsByClassName(className);
    for (var i = 0; i < cols.length; i++) {
        cols[i].style.display = 'table-cell';
    }
    document.getElementById('open-receipt-items').style.display = 'none';
    document.getElementById('close-receipt-items').style.display = 'inline';
    demandItemsTbl.draw();
}

function CloseReceiptCols(className) {
    var cols = document.getElementsByClassName(className);
    for (var j = 0; j < cols.length; j++) {
        cols[j].style.display = 'none';
    }
    document.getElementById('open-receipt-items').style.display = 'inline';
    document.getElementById('close-receipt-items').style.display = 'none';
    demandItemsTbl.draw();
}

function EditDemandReceipts(ReceiptIndex) {

    var editBoxes = document.getElementsByClassName("receipItemEdit_" + ReceiptIndex);
    var DisplayBoxes = document.getElementsByClassName("receipItemDisplay_" + ReceiptIndex);
    var headers = document.getElementsByClassName("receipheaderEdit_" + ReceiptIndex);
    for (var i = 0; i < editBoxes.length; i++) {
        editBoxes[i].style.display = '';
    }


    for (var i = 0; i < DisplayBoxes.length; i++) {
        DisplayBoxes[i].style.display = 'none';
    }

    for (var i = 0; i < headers.length; i++) {
        headers[i].style.display = '';
    }
    document.getElementById("editControl-" + ReceiptIndex).style.display = "none";
    document.getElementById("SaveEditControl-" + ReceiptIndex).style.display = "";

    demandItemsTbl.draw();


}


function SaveDemandReceiptItems(ReceiptIndex) {
    var editBoxes = document.getElementsByClassName("receipItemEdit_" + ReceiptIndex);

    var procurement_Demand_Receipts = [];

    for (var i = 0; i < editBoxes.length; i++) {
        editBoxes[i].style.display = 'none';

        var DemandDbKey = editBoxes[i].getAttribute('data-demandDbKey');
        var DemandItemkey = editBoxes[i].getAttribute('data-demandItemKey');
        var receiptIndex = editBoxes[i].getAttribute('data-receiptIndex');
        var Receipt_dbkey = editBoxes[i].getAttribute('data-receiptDbkey');
        var Receiving_inventory = editBoxes[i].value;


        var DemandItemRecipts = {};
        DemandItemRecipts.DemandDbKey = DemandDbKey;
        DemandItemRecipts.DemandItemKey = DemandItemkey;
        DemandItemRecipts.Index_No = receiptIndex;
        DemandItemRecipts.Receipt_dbkey = Receipt_dbkey;
        DemandItemRecipts.Receipt_Date = document.getElementsByClassName("receipheaderEdit_" + ReceiptIndex)[1].value;
        DemandItemRecipts.Receipt_No = document.getElementsByClassName("receipheaderEdit_" + ReceiptIndex)[0].value;
        DemandItemRecipts.Receiving_inventory = Receiving_inventory;
        // console.log(Receiving_inventory);
        //if (Receiving_inventory != 0 && Receipt_dbkey != 0) {
        //    procurement_Demand_Receipts.push(DemandItemRecipts);
        //}
        //else if (Receiving_inventory != 0) {
        //    procurement_Demand_Receipts.push(DemandItemRecipts);
        //}
        // if (Receipt_dbkey != 0) {
        procurement_Demand_Receipts.push(DemandItemRecipts);
        //}

    }

    //for (var i = 0; i < DisplayBoxes.length; i++) {
    //    DisplayBoxes[i].style.display = '';
    //}
    //document.getElementById("editControl-" + ReceiptIndex).style.display = "";
    //document.getElementById("SaveEditControl-" + ReceiptIndex).style.display = "none";
    if (procurement_Demand_Receipts.length == 0) {
        bootbox.alert('Please update the inventory for at least one line item.');
        return false;
    }



    var postData = JSON.stringify(procurement_Demand_Receipts);

    //  console.log(postData);

    bootbox.confirm({
        message: "Are you sure you wish to save this receipt?",
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
            if (result == true) {
                $.ajax({
                    type: 'POST',
                    url: '/DemandManagement/SaveDemandItemsReceipt',
                    data: postData,
                    dataType: "json",
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        GetViewDemandItems();
                    }
                })
            }

        }
    });

    //demandItemsTbl.draw();
}

function GetDemandSplitPopUp(Receipt_dbkey) {

    var htmldata = '<div class="col-12"> <div id="documents-area" class="col-12">  </div> <div id="SplitRecipts" class="col-12">  </div>  </div>'

    bootbox.dialog({
        title: "Receipt Splits",
        message: htmldata,
        size: 'small',
        closeButton: true,
        className: 'custom-modal',
    });
    // AttachmentDoc(Receipt_dbkey);
    GetViewDemandItemReciptSplit(Receipt_dbkey);
}


function AttachmentDoc(Receipt_dbkey) {
    var urlinput = "/DemandManagement/DemandReceiptsDocs?Receipt_dbkey=" + Receipt_dbkey;
    $.get(urlinput).done(function (response) {
        document.getElementById("documents-area").innerHTML = response;
    });
}

function UploadReceiptDos(ID) {
    var filescount = 0;
    var recptDocs = document.getElementById("recptDocs_tblbody");
    var rows = recptDocs.getElementsByTagName("tr");

    var File_DVD_Num = document.getElementsByClassName("File_DVD_Num");
    var File_Revision = document.getElementsByClassName("File_Revision");
    var fileName = document.getElementsByClassName("fileName");

    //  console.log(fileName[0].files.length);

    var fileData = new FormData();
    if (fileName[0].files.length > 0) {
        var uploadeddocument = fileName[0].files[0];
        var Source_table_key = ID;
        var Source_table = 'Procurement_Demand_Receipts';
        var Attachment_type = 'Deamands_Receipt_Docs';
        var File_DVD_Num = File_DVD_Num[0].value;
        var File_Revision = File_Revision[0].value;

        fileData.append('uploadeddocument', uploadeddocument);
        fileData.append('Source_table_key', Source_table_key);
        fileData.append('Source_table', Source_table);
        fileData.append('Attachment_type', Attachment_type);
        fileData.append('File_DVD_Num', File_DVD_Num);
        fileData.append('File_Revision', File_Revision);
        filescount++;
    }

    if (filescount > 0) {
        $.ajax({
            type: "POST",
            url: '/Attachment/UploadFiles',
            data: fileData,
            cache: false,
            contentType: false,
            processData: false,
            success: function (data) {
                if (data.success) {
                    bootbox.alert('Uploaded Successfully');
                    AttachmentDoc(ID);
                }
            }
        });
    } else {
        bootbox.alert('Please upload at least one document');
    }

}


function GetDemandItems() {
    $('#loading').show();
    var demanddbkey = $('#DemandDbkeyGlobalKey').val();
    var urlinput = "/DemandManagement/DemandItems?Id=" + demanddbkey + "&Viewtype=Readonly";
    $.get(urlinput).done(function (response) {
        document.getElementById("tab-DemandItems").innerHTML = response;
        $('#loading').hide();
    });
}

function EditDemandItemDetail() {
    //let dialog = bootbox.dialog({
    //    message: '<p class="text-center mb-0"><i class="fas fa-spin fa- cog"></i> Please wait it will take some time...</p>',
    //    closeButton: false
    //});
    $('#loading').show();
    var demanddbkey = $('#DemandDbkeyGlobalKey').val();
    var urlinput = "/DemandManagement/DemandItems?Id=" + demanddbkey + "&Viewtype=Edit";
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Demand Items",
            message: response,
            size: 'extra-large',
            closeButton: true,
            className: 'custom-modal',
        });
        //new Choices('.ItemDbKey', {
        //    searchEnabled: true,
        //}); 
        select2OnPageLoad();
        $('#loading').hide();

    });
}

//--------------------------Demand Items-------------------------------------------
function GetDemandDocuments() {
    var demanddbkey = $('#DemandDbkeyGlobalKey').val();
    var urlinput = "/DemandManagement/Demand_Documents?Id=" + demanddbkey;
    $.get(urlinput).done(function (response) {
        document.getElementById("tab-DemandDocument").innerHTML = response;
    });
}

function GetDemandReceiptDocuments() {
    var demanddbkey = $('#DemandDbkeyGlobalKey').val();
    var urlinput = "/DemandManagement/DemandReceiptDocuments?Id=" + demanddbkey;
    $.get(urlinput).done(function (response) {
        document.getElementById("tab-Demand-Receipt-Document").innerHTML = response;
        $('#Receipt-Documents-Table').DataTable({
            paging: false,
            stateSave: true,
            stateSaveCallback: function (settings, data) {
                localStorage.setItem(
                    'DataTables_' + settings.sInstance,
                    JSON.stringify(data)
                );
            },
            stateLoadCallback: function (settings) {
                return JSON.parse(localStorage.getItem('DataTables_' + settings.sInstance));
            }
        });
    });
}

function UploadDemandDocuments() {
    var demanddbkey = $('#DemandDbkeyGlobalKey').val();
    var urlinput = "/DemandManagement/UploadDemandDocument?Id=" + demanddbkey;
    $.get(urlinput).done(function (response) {
        bootbox.confirm({
            title: 'Upload Demand Document',
            message: response,
            buttons: {
                cancel: {
                    label: '<i class="fa fa-times"></i> Cancel'
                },
                confirm: {
                    label: '<i class="fa fa-check"></i> Upload'
                }
            },
            callback: function (result) {
                if (result) {
                    UploadDocumentDemand();
                    return false;
                }

            }
        });
    });
}




function UploadDocumentDemand() {
    var fileUpload = $("#DocumentFile").get(0);
    var files = fileUpload.files;

    if (files.length === 0) {
        bootbox.alert({
            backdrop: true,
            title: "Required",
            message: "File is Required",
            centerVertical: true
        });
        return;
    }

    var allowedExtensions = ["pdf", "docx"];
    var fileExtension = files[0].name.split('.').pop().toLowerCase();

    if (allowedExtensions.indexOf(fileExtension) === -1) {
        bootbox.alert({
            title: "Invalid file type",
            message: "Please Upload pdf, docx format only",
            centerVertical: true
        });
        return;
    }

    var fileData = new FormData();
    fileData.append('DocumentFile', files[0]);
    // fileData.append('DocumentID', document.getElementById("DocumentID").value);
    fileData.append('DemandDbKey', document.getElementById("DemandDbKey").value);
    fileData.append('Document_Type', document.getElementById("Document_Type").value);
    fileData.append('Remarks', document.getElementById("Remarks").value);

    $.ajax({
        type: "POST",
        url: '/DemandManagement/SaveDemandDocument',
        data: fileData,
        cache: false,
        contentType: false,
        processData: false,
        success: function (data) {
            if (data.success) {
                bootbox.alert({
                    title: 'Success',
                    message: "Uploaded Successfully",
                    centerVertical: true,
                    callback: function () {

                        $('.bootbox.modal').modal('hide');
                        //var btn = document.getElementsByClassName("bootbox-close-button")[1];
                        //btn.click();

                        GetDemandDocuments();
                    }

                });

            } else {
                bootbox.alert({
                    title: 'Encountered an error!',
                    message: "Failed",
                    centerVertical: true
                });
            }
        },
        error: function () {
            bootbox.alert({
                title: 'Error!',
                message: 'An error occurred while processing your request.',
                centerVertical: true
            });
        }
    });

}

function DelDocData(id) {
    bootbox.confirm("Are you sure you want to delete this Demand Document ", function (result) {
        if (result) {
            $.ajax({
                type: 'GET',
                url: '/DemandManagement/DeleteDemandDoc?id=' + id,
                success: function (data) {
                    if (data.success) {
                        bootbox.alert("Deleted Successfully");
                        closeBootboxes();
                    }
                    else {
                        bootbox.alert("Delete Failed")
                    }
                },
                error: function () {
                }
            });
        } else {

        }
    });
}

function DeleteDemand(id) {
    bootbox.confirm("Are you sure you want to delete this Demand", function (result) {
        if (result) {
            $.ajax({
                type: 'GET',
                url: '/DemandManagement/DeleteDemand?id=' + id,
                success: function (data) {
                    if (data.success) {
                        bootbox.alert("Deleted Successfully");
                    }
                    else {
                        bootbox.alert(data.msg)
                    }
                },
                error: function () {
                }
            });
        } else {
            bootbox.alert("Deletion canceled.");
        }
    });
}

function ApplySelect2() {
    $('.select2class').select2(
        {
            tags: true,
            createTag: function (params) {
                return {
                    id: -1,
                    text: params.term,
                    newOption: true
                }
            }
        });
    $('.select2class_numbers').select2(
        {
            tags: true,
            createTag: function (params) {
                if (Number.isInteger(parseInt(params.term))) {
                    return {
                        id: params.term,
                        text: params.term,
                        newOption: true
                    }
                } else {
                    return {
                        id: 0,
                        text: 0,
                        newOption: true
                    }
                }
            }
        });
}




function ToggleItemTypeLists() {
    var itemtype = document.getElementById("Item_Type").value;
    if (itemtype == "RM" || itemtype == "NA") {
        document.getElementById("Billtable").style.display = "block";
        document.getElementById("Bearing_tbl").style.display = "none";
    } else {
        document.getElementById("Bearing_tbl").style.display = "block";
        document.getElementById("Billtable").style.display = "none";
    }
}


function FillRawMaterialParameters(RawItem) {
    var item = RawItem.value;
    if (Number.isInteger(parseInt(item)) && parseInt(item) > 0) {
        $(RawItem).closest('tr').find("select").each(function () {
            if (this.id == 'Outer_Dia_mm') {
                var selectList = $(this);
                selectList.empty();
                $.getJSON("/SharedCall/GetRawmaterialParaJResult", "id=" + RawItem.value + "&type=Outer_Dia", function (data) {
                    $.each(data, function (index, data) {
                        var option = $('<option>').text(data.Text).val(data.Value);
                        selectList.append(option);
                    });
                });
            }
            if (this.id == 'Thickness') {
                var selectList = $(this);
                selectList.empty();
                $.getJSON("/SharedCall/GetRawmaterialParaJResult", "id=" + RawItem.value + "&type=Thickness", function (data) {
                    $.each(data, function (index, data) {
                        var option = $('<option>').text(data.Text).val(data.Value);
                        selectList.append(option);
                    });
                });
            }
        });
    }
}


function addRow() {
    //try {
    //    $('.select2class').select2('destroy');
    //} catch (e) {

    //}

    //$('.select2class_numbers').select2('destroy');
    //var x = document.getElementById("demandEditItemstbl");  //get the table
    //var node = x.rows[0].cloneNode(true);    //clone the previous node or row
    //node.style = null;
    //x.appendChild(node);   //add the node or row to the table
    /*   ApplySelect2();*/

    //$('.select2class').select2({
    //    width: "100%",
    //    dropdownParent: $('#my-id')
    //});

    var selectElements = document.querySelectorAll('.ItemDbKey');
    // Loop through each select element
    selectElements.forEach(function (element) {
        // Check if Choice.js instance is attached to the element
        if (element.choices) {
            // If Choice.js instance is attached, destroy it
            element.choices.destroy();
        }
    });
    var x = document.getElementById("demandEditItemstbl");  //get the table
    var node = x.rows[0].cloneNode(true);    //clone the previous node or row
    node.style = null;
    node.cells[1].children[0].classList.add('ItemDbKey')
    x.appendChild(node);

    var selectElements1 = document.querySelectorAll('.ItemDbKey');
    selectElements1.forEach(function (element) {
        new Choices(element, {
            searchEnabled: true,
            searchResultLimit: 15
        });
    });



}

function select2OnPageLoad() {
    var selectElements = document.querySelectorAll('.ItemDbKey');
    // Loop through each select element
    selectElements.forEach(function (element) {
        // Check if Choice.js instance is attached to the element
        if (element.choices) {
            // If Choice.js instance is attached, destroy it
            element.choices.destroy();
        }
    });

    var selectElements1 = document.querySelectorAll('.ItemDbKey');
    selectElements1.forEach(function (element) {
        new Choices(element, {
            searchEnabled: true,
            searchResultLimit: 15
        });
    });

}

function delRow(ele) {
    var tblrow = ele.closest("tr");
    document.getElementById("demandEditItemstbl").deleteRow(tblrow.rowIndex - 1); //get the table
    //delete the last row
}


function RemoveReceiptDocs(ele) {
    var tblrow = ele.closest("tr");
    document.getElementById("recptDocs_tblbody").deleteRow(tblrow.rowIndex - 1); //get the table
}

function AddReceiptDocRow() {
    //$('.select2class').select2('destroy');
    //$('.select2class_numbers').select2('destroy');
    var x = document.getElementById("recptDocs_tblbody");  //get the table
    var node = x.rows[0].cloneNode(true);    //clone the previous node or row
    node.style = null;
    x.appendChild(node);   //add the node or row to the table
    /*   ApplySelect2();*/
}


function InactivateDemandItem(Dbkey, ele) {

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
                    url: "/DemandManagement/DeleteDemandItem",
                    data: "id=" + Dbkey,
                    success: function (data) {
                        if (data.success) {
                            bootbox.alert(
                                "Removed successfully",
                                function () {
                                    var tblrow = ele.closest("tr");
                                    document.getElementById("demandEditItemstbl").deleteRow((tblrow.rowIndex - 1));
                                }
                            );
                        }
                        else {
                            bootbox.alert({
                                message: "You cannot delete the item because completed demand receipts transactions exist for the item.",
                                //callback: function () {
                                //    var tblrow = ele.closest("tr");
                                //    document.getElementById("demandEditItemstbl").deleteRow(tblrow.rowIndex - 1);
                                //}
                            })
                        }
                    }
                });
            }
        }
    });

};

// Save or Edit Demand Items
function SaveDemandItemDetail(DemandItemType) {
    var Procurement_Demand_Items = [];
    var items = document.getElementsByClassName("DemandItemKey");
    for (var i = 1; i < items.length; i++) {

        var Demand_Items = {};
        Demand_Items.DemandItemKey = document.getElementsByClassName("DemandItemKey")[i].value;

        Demand_Items.DemandDbKey = $('#DemandDbkeyGlobalKey').val();
        Demand_Items.UOM = document.getElementsByClassName("UOM")[i].value;
        Demand_Items.Qty = document.getElementsByClassName("Qty")[i].value;
        Demand_Items.MMGOrderNumber = document.getElementsByClassName("MMGOrderNumber")[i].value;

        if (DemandItemType == "RM") {
            Demand_Items.ItemDbKey = document.getElementsByClassName("Items")[i].value;

            //  Demand_Items.Outer_Dia_mm = document.getElementsByClassName("Outer_Dia_mm")[i].value;
            //  Demand_Items.Thickness = document.getElementsByClassName("Thickness")[i].value;

        } else if (DemandItemType == "Parts" || DemandItemType == "LRU" || DemandItemType == "BOI") {

            Demand_Items.Engine_Part_Dbkey = document.getElementsByClassName("Engine_Part_Dbkey")[i].value;
            Demand_Items.Item_Code = document.getElementsByClassName("Item_Code")[i].value;
            Demand_Items.Item_Sub_Type = document.getElementsByClassName("Item_Sub_Type")[i].value;
            Demand_Items.Remarks = document.getElementsByClassName("Remarks")[i].value;

        } else if (DemandItemType == "SER" || DemandItemType == "Service") {

            Demand_Items.Item_Code = document.getElementsByClassName("Item_Code")[i].value;
            Demand_Items.Item_Sub_Type = document.getElementsByClassName("Item_Sub_Type")[i].value;
            Demand_Items.Remarks = document.getElementsByClassName("Remarks")[i].value;
        }

        Procurement_Demand_Items.push(Demand_Items);
    }
    /*    console.log(JSON.stringify(Procurement_Demand_Items));*/
    $.ajax({
        type: "POST",
        url: "/DemandManagement/SaveDemandItems",
        data: {
            demanditems: JSON.stringify(Procurement_Demand_Items),
        },
        dataType: "json",
        success: function (data) {
            if (data.success) {
                bootbox.alert({
                    message: "Submitted Sucessfully",
                    callback: function () {
                        closeBootboxes();
                        GetViewDemandItems();
                    }
                });
            } else {
                alert("Failed");
            }
        },
        failure: function (response) {
            alert("Failed");
        }
    });

}

function closeBootboxes() {
    var btn = document.getElementsByClassName("bootbox-close-button")[0];
    btn.click();
}

// Adding new column to add new receipts
function AddNewReciptColumn() {

    var checkNewItemExist = document.getElementsByClassName("NewReceiptItem");

    if (checkNewItemExist.length == 0) {
        document.getElementById("DemandItems-tab").click();
        demandItemsTbl.destroy();

        var table = document.getElementById("ViewDemandItems");

        var headerCell = document.createElement("th");
        headerCell.innerHTML = "<input type='text' placeholder='Receipt Number' class='receipheaderEdit_0 NewReceiptItem' /><br/><input type='date' class='receipheaderEdit_0' /><i onclick='SaveDemandReceiptItems(0)' style='cursor:pointer;color:green' class='uil uil-save fs-2 float-end'></i>";
        table.querySelector("thead tr").appendChild(headerCell);

        var rows = table.querySelectorAll("tbody tr");
        for (var i = 0; i < rows.length; i++) {
            var itemkey = rows[i].querySelector("td:first-child input[type='number']")
            var newCell = rows[i].insertCell(-1);
            newCell.innerHTML = "<input type='number' data-demandDbKey='" + itemkey.getAttribute("data-demandDbKey") + "' data-demandItemKey='" + itemkey.getAttribute("data-demandItemKey") + "' data-receiptIndex='0' data-receiptDbkey='0' class='receipItemEdit_0' value='0'  />";
        }

        demandItemsTbl = $('#ViewDemandItems').DataTable({
            scrollX: true,
            scrollCollapse: true,
            paging: false,
            order: false,
            stateSave: true,
            stateSaveCallback: function (settings, data) {
                localStorage.setItem(
                    'DataTables_' + settings.sInstance,
                    JSON.stringify(data)
                );
            },
            stateLoadCallback: function (settings) {
                return JSON.parse(localStorage.getItem('DataTables_' + settings.sInstance));
            },
            scrollY: 400,
            dom: '<"top"lfB>rtip',
            buttons: [
                'excel'
            ],
            fixedColumns: {
                leftColumns: 5
            }
        });

        demandItemsTbl.columns.adjust().draw();
        $("#ViewDemandItems_wrapper > div.dataTables_scroll > div.dataTables_scrollBody").scrollLeft(10000);
    }




}

function LoadProjectDemand() {
    document.getElementById("statusDisplayArea").innerHTML = '';
    var key = $('select#projectlist option:selected').val();
    var DemandOfficerKey = $('select#Demand-officer-select option:selected').val();
    var urlinput = "/DemandManagement/Dashboard?ProjectDbkey=" + key + "&DemandingofficerDbKey=" + DemandOfficerKey;
    var urlDemandList = "/DemandManagement/GetDemandList?ProjectId=" + key + "&demandingofficerId=" + DemandOfficerKey;
    $.get(urlinput).done(function (response) {
        document.getElementById("DemandManagerDiv").innerHTML = response;
        GetDataForBarChart();
        GetDataForDemandStatus();
        GetProject_Do_Report(key, DemandOfficerKey);
        GetExecutiveSummary('All');
        GetDemandDashboard('LRU');
        //var sanctionedCost = document.getElementById("Sanctioned").Value;
        //var formatedSanctionedCost = new Intl.NumberFormat("en-IN", { style: "currency", currency: "INR" }).format(sanctionedCost);
        //console.log(formatedSanctionedCost);

    });


    $.get(urlDemandList).done(function (data) {
        var demandlist = data;
        var selectList = $('#Demand-select');
        selectList.empty();
        selectList.append($('<option>').text('Select').val(0));
        $.each(demandlist, function (index, demandlist) {
            var demandDisplayText = demandlist.mmG_File_No + '-' + demandlist.item_Description;
            if (demandlist.isShortClosure == 1) {
                demandDisplayText = demandDisplayText + " (Short Closed)";
            }
            var option = $('<option>').text(demandDisplayText).val(demandlist.demandDbKey);
            selectList.append(option);
        });
    });

}

function GetDataForBarChart() {
    var colors = [];
    colors[0] = '#ec823b';
    colors[1] = '#78bd5d';

    var Sanctioned = (document.getElementById("Sanctioned").innerText);
    var Actual = (document.getElementById("Actual").innerText);
    var Estimated = (document.getElementById("Estimated").innerText);
    var Remaining = (document.getElementById("Remaining").innerText);

    var Actual_res = parseFloat(Actual.replace(/,/g, ''));
    var Remaining_res = parseFloat(Remaining.replace(/,/g, ''));
    var Estimated_res = parseFloat(Estimated.replace(/,/g, ''));
    var est_Sanctioned_res_remain = parseFloat(Sanctioned.replace(/,/g, '')) - parseFloat(Estimated.replace(/,/g, ''));
    var sac_est_res_remain = parseFloat(Sanctioned.replace(/,/g, '')) - parseFloat(Actual.replace(/,/g, ''));

    document.getElementById("Sanctioned").innerText = new Intl.NumberFormat("en-IN", { style: "currency", currency: "INR" }).format(Sanctioned);
    (document.getElementById("Actual").innerText) = new Intl.NumberFormat("en-IN", { style: "currency", currency: "INR" }).format(Actual);
    (document.getElementById("Estimated").innerText) = new Intl.NumberFormat("en-IN", { style: "currency", currency: "INR" }).format(Estimated);
    (document.getElementById("Remaining").innerText) = new Intl.NumberFormat("en-IN", { style: "currency", currency: "INR" }).format(Remaining);

    var data_Estimated_Sanctioned = [Estimated_res, Actual_res];
    var data_Actual_Sanctioned = [est_Sanctioned_res_remain, sac_est_res_remain];


    Highcharts.chart('container3', {
        chart: {
            type: 'bar',
            plotBackgroundColor: null,
            plotBorderWidth: null,
            plotShadow: false,
            height: 200,
        },
        credits: {
            enabled: false
        },
        title: {
            text: 'Demand Cost'
        },
        xAxis: {
            /*   categories: ['Estimated/Sanctioned', 'Actual/Sanctioned']*/
            //categories: ['Sanctioned/Estimated', 'Sanctioned/Actual']
            categories: ['Estimated', 'Actual']
        },
        yAxis: {
            min: 0,
            title: {
                text: 'Amount'
            }
        },
        legend: {
            reversed: true
        },
        plotOptions: {
            series: {
                stacking: 'normal'
            }
        },
        colors: colors,
        series: [
            {
                name: 'Remaining',
                data: data_Actual_Sanctioned,
                pointWidth: 18
            },
            {
                name: 'Amount',
                data: data_Estimated_Sanctioned,
                pointWidth: 18
            }
        ]
    });
}

function ProcurementMilestoneDetails() {
    var demanddbkey = $('#DemandDbkeyGlobalKey').val();
    var urlinput = "/DemandManagement/procurmentMileStoneDetails?Id=" + demanddbkey;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Procurement Milestone",
            message: response,
            size: 'extra-large',
            closeButton: true,
            className: 'custom-modal',
        });
    });
}

function GetViewMilestoneDetails(IsEditMode) {
    //   console.log(IsEditMode);
    var demanddbkey = $('#DemandDbkeyGlobalKey').val();

    if (IsEditMode == true) {
        var urlinput_EditMilestone = "/DemandManagement/procurementDemandMilestone?Id=" + demanddbkey + "&IsEditMode=" + IsEditMode;
        $.get(urlinput_EditMilestone).done(function (response) {
            document.getElementById("MilestoneDetail").innerHTML = response;
        });
        return;
    }
    var urlinput = `/DemandManagement/MilestoneStatusReport?demandingOfficerkey=0&status=All&duedate=All&project=0&demandDbkey=${demanddbkey}&viewMode=DemandScreen`;
    $.get(urlinput).done(function (response) {
        document.getElementById("MilestoneDetail").innerHTML = response;
        //$('#tbl-milestone-viewmode').DataTable({
        //    paging: false
        //});
    });
}




//---------------------mile stone---------------------------------
function addRowMS() {
    //$('.select2class').select2('destroy');
    //$('.select2class_numbers').select2('destroy');
    var x = document.getElementById("Milestonetbl");  //get the table
    var node = x.rows[0].cloneNode(true);    //clone the previous node or row
    node.style = null;
    x.appendChild(node);   //add the node or row to the table
    /*   ApplySelect2();*/
}

function delRowMS(ele) {
    var tblrow = ele.closest("tr");
    document.getElementById("Milestonetbl").deleteRow(tblrow.rowIndex - 1); //get the table
    //delete the last row
}
function InactivateMilestoneItem(Dbkey, ele) {

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
                    url: "/DemandManagement/DeleteMilestoneItem",
                    data: "id=" + Dbkey,
                    success: function (data) {
                        if (data.success) {
                            bootbox.alert({
                                message: "Removed successfully",
                                callback: function () {
                                    var tblrow = ele.closest("tr");
                                    document.getElementById("Milestonetbl").deleteRow(tblrow.rowIndex - 1);
                                }
                            })
                        } else {

                        }
                    }
                });
            }
        }
    });

};

function SaveProcurementMilestone() {

    var ProcurementMilestone_Items = [];
    var items = document.getElementsByClassName("MilestoneID");
    for (var i = 1; i < items.length; i++) {

        var MS_Items = {};
        MS_Items.MilestoneID = document.getElementsByClassName("MilestoneID")[i].value;
        MS_Items.DemandDbKey = $('#DemandDbkeyGlobalKey').val();
        MS_Items.MilestoneName = document.getElementsByClassName("MilestoneName")[i].value;
        MS_Items.Components = document.getElementsByClassName("Components")[i].value;
        MS_Items.Description = document.getElementsByClassName("Description")[i].value;
        MS_Items.DueDate = document.getElementsByClassName("DueDate")[i].value;
        MS_Items.CompletionDate = document.getElementsByClassName("CompletionDate")[i].value;
        MS_Items.Comments = document.getElementsByClassName("Comments")[i].value;
        ProcurementMilestone_Items.push(MS_Items);
    }

    $.ajax({
        type: "POST",
        url: "/DemandManagement/SaveProcurementMileStone",
        data: JSON.stringify(ProcurementMilestone_Items),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.success) {
                bootbox.alert({
                    message: "Submitted Sucessfully",
                    callback: function () {
                        closeBootboxes();

                    }
                });
            } else {
                alert("Failed");
            }
        },
        failure: function (response) {
            alert("Failed");
        }
    });

}




function GetDataForDemandStatus() {
    Highcharts.chart('container', {
        data: {
            table: 'datatable2'
        },
        chart: {
            type: 'column'
        },
        credits: {
            enabled: false
        },
        title: {
            text: 'Demand status v/s No of demands as on date'
        },
        yAxis: {
            allowDecimals: false,
            title: {
                text: 'Units'
            }
        },
        plotOptions: {
            series: {
                dataLabels: {
                    enabled: true,
                    borderRadius: 2,
                    y: -10,
                    shape: 'callout',
                },
                //events: {
                //    click: function () {
                //        alert(
                //            'Category: ' + this.status + ', value: ' + this.y
                //        );
                //    }
                //}
            },

        },
        tooltip: {
            formatter: function () {
                return '<b>' + this.series.name + '</b><br/>' +
                    this.point.y + ' ' + this.point.name.toLowerCase();
            }
        },
        legend: {
            enabled: false
        }
    });
}

function GetDemandNumbersByStatus(status, prjiD, DemandingOfficerDbkey) {
    $.ajax({
        type: "GET",
        url: "/DemandManagement/GetDemandNumbersByStatus?id=" + status + "&prjtID=" + prjiD + "&DemandingOfficerDbkey=" + DemandingOfficerDbkey,
        cache: false,
        success: function (data) {
            bootbox.dialog({
                title: "Demands",
                message: data,
                size: 'large',
                closeButton: true,
            });
        }
    });
    return false;
}

function ViewDemandByStatus(Id) {
    closeBootboxes();
    ShowDemandDetail(Id)
}

function PrintDiv(divId) {
    $('#' + divId).printThis({
        //beforePrintEvent: RemoveNonPrintElements(0),
        //afterPrint: RemoveNonPrintElements(1),
    });
}

function RemoveNonPrintElements(Display) {
    var noprintsElements = document.getElementsByClassName("exclude-print");
    // console.log(noprintsElements.length);
    for (var i = 0; i < noprintsElements.length; i++) {
        if (Display == 0) {
            noprintsElements[i].style.display = 'none';
        } else {
            noprintsElements[i].style.display = 'block';
        }
    }
}

//--------------------- Procurment Split---------------------------

function AddSplitRows() {
    //destroySelect2OnReceiptSplit();
    var table = document.getElementById("SpliTblBody");
    var node = table.rows[0].cloneNode(true);
    node.cells[0].children[0].value = 0;
    //node.cells[5].children[0].value = 0;
    //node.cells[6].children[0].value = 0;
    //node.cells[7].children[0].value = 0;
    //node.cells[8].children[0].value = "";
    //node.cells[9].children[0].value = "";
    //node.cells[10].children[0].value = "";
    //  node.cells[11].children[0].value = "";
    node.style.display = "";
    table.appendChild(node);
    //applySelect2OnReceiptSplit();
}


function RemoveReceiptSplit(ele) {
    var tblrow = ele.closest("tr");
    document.getElementById("SpliTblBody").deleteRow(tblrow.rowIndex - 1); //get the table

}


function GetViewDemandItemReciptSplit(Receipt_dbkey) {
    var urlinput = "/DemandManagement/ReceiptItemSplits?Id=" + Receipt_dbkey;
    $.get(urlinput).done(function (response) {
        document.getElementById("SplitRecipts").innerHTML = response;
        //applySelect2OnReceiptSplit();
    });
}

function GetViewDemandItemReciptSplitModel(Receipt_dbkey, splitItemKey) {
    var urlinput = "/DemandManagement/ReceiptItemSplitsModel?Receipt_dbkey=" + Receipt_dbkey + "&splitKey=" + splitItemKey;
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


//function destroySelect2OnReceiptSplit() {

//    //var dropdowns = document.getElementsByClassName("Attachment_Db_Key");
//    //for (i = 0; i < dropdowns.length; i++) {
//    //    try {
//    //        $(dropdowns[i]).select2('destroy');
//    //    }
//    //    catch (e) {

//    //    }
//    //}

//    try {
//        $('.Attachment_Db_Key').select2('destroy');
//    } catch (e) {

//    }
//}

//function applySelect2OnReceiptSplit() {
//    var dropdowns = document.getElementsByClassName("Attachment_Db_Key");
//    //for (i = 0; i < dropdowns.length; i++) {
//    //    try {
//    //        $(dropdowns[i]).select2('destroy');
//    //    }
//    //    catch (e)
//    //    {

//    //    }
//    //    $(dropdowns[i]).select2({
//    //        dropdownParent: $('#SplitRecipts')
//    //    });
//    //}

//    $('.Attachment_Db_Key').select2({
//        dropdownParent: $('#SplitRecipts')
//    });
//}

function SaveRawMaterialSplit(ReceiptDbkey) {

    var splitTable = document.getElementById("SpliTblBody");
    var rows = splitTable.getElementsByTagName("tr");

    var Procurement_Demand_Items_Split = [];
    var items = document.getElementsByClassName("Receipt_dbkey");
    for (var i = 1; i < items.length; i++) {

        var SplitID = document.getElementsByClassName("SplitId")[i].value;
        var checkbox = document.getElementsByClassName("Attachment_Db_Key_" + SplitID);

        var checkboxcell = rows[i].cells[10];
        var docCheckbox = checkboxcell.querySelectorAll('input[type="checkbox"]');

        //for (var r = 0; r < docCheckbox.length; r++) {
        //    console.log(docCheckbox[r].checked);   
        //}

        var Attachment_Db_Key_Data = [];
        var Demand_Items_Split = {};
        Demand_Items_Split.SplitId = SplitID;
        Demand_Items_Split.Receipt_dbkey = document.getElementsByClassName("Receipt_dbkey")[i].value;
        Demand_Items_Split.Measurement = document.getElementsByClassName("Measurement")[i].value;
        Demand_Items_Split.Measurement_breadth = document.getElementsByClassName("Measurement_breadth")[i].value;
        Demand_Items_Split.UOM = document.getElementsByClassName("UOM")[i].value;
        Demand_Items_Split.Weight = document.getElementsByClassName("Weight")[i].value;
        Demand_Items_Split.Material_Reference_No = document.getElementsByClassName("Material_Reference_No")[i].value;
        Demand_Items_Split.Heat_No = document.getElementsByClassName("Heat_No")[i].value;
        Demand_Items_Split.Batch_No = document.getElementsByClassName("Batch_No")[i].value;

        for (var r = 0; r < docCheckbox.length; r++) {
            if (docCheckbox[r].checked) {
                var AttachmentDbKey = docCheckbox[r].getAttribute('data-attachmentKey');
                Attachment_Db_Key_Data.push(AttachmentDbKey);
            }
        }

        //for (var j = 0; j < checkbox.length; j++) {
        //    if (checkbox[j].checked) {
        //       var AttachmentDbKey = checkbox[j].getAttribute('data-attachmentKey');
        //        Attachment_Db_Key_Data.push(AttachmentDbKey);
        //    }
        //}

        Demand_Items_Split.Attachment_Db_Key_Data = Attachment_Db_Key_Data;
        Procurement_Demand_Items_Split.push(Demand_Items_Split);
    }
    //console.log(Procurement_Demand_Items_Split);

    $.ajax({
        type: "POST",
        url: "/DemandManagement/SaveReceiptItemSplits",
        data: { Procurement_Demand_Items_Split: JSON.stringify(Procurement_Demand_Items_Split) },
        //dataType: "json",
        //contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.success) {
                bootbox.alert({
                    message: "Submitted Successfully",
                    backdrop: true,
                    callback: function () {
                        GetViewDemandItemReciptSplit(ReceiptDbkey);
                    }
                });
            } else {
                alert("Failed");
            }
        },
        failure: function (response) {
            alert("Failed");
        }
    });
}
function applySelect2() {
    $('.multiselect').select2();
}


function GetAdditionalInfoReciptSplit(ctrl, Receipt_dbkey, SplitID) {
    //$(ctrl).prop('disabled', true);
    var urlinput = "/DemandManagement/AdditionalInfo?Receipt_dbkey=" + Receipt_dbkey + "&SplitID=" + SplitID;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Additional Info",
            message: response,
            size: 'extra-large',
            //closeButton: true,
            //className: 'custom-modal',
        });
        // $(ctrl).prop('disabled', false);
    });
}


function AddRowAdnlInfo() {
    var table = document.getElementById("TblAdnlInfoEntryBody");
    //var lastRowIndex = table.rows.length - 1; // Index of the last row
    var lastRow = table.rows[0];
    var node = lastRow.cloneNode(true);
    var serialNumberCell = node.cells[2];
    serialNumberCell.children[0].value = table.rows.length;
    node.cells[1].children[0].value = "new";    //need to get verified
    node.cells[6].children[0].value = 0;
    node.style.display = "";
    table.appendChild(node);
}

function delRowAdnlInfo(ele) {
    var tableBody = document.getElementById("TblAdnlInfoEntryBody");
    var tblrow = ele.closest("tr");
    var rowIndex = tblrow.rowIndex;
    tableBody.deleteRow(rowIndex - 1);
}

function SaveAdditionalInfo() {
    var splitTable = document.getElementById("TblAdnlInfoEntryBody");
    var rows = splitTable.getElementsByTagName("tr");
    var Guid = document.getElementsByClassName("recordGUID");
    var AdditionalInfoRawData = [];


    for (var i = 1; i < Guid.length; i++) {
        var DocumentData = [];
        var checkboxcell = rows[i].cells[5];
        var docCheckbox = checkboxcell.querySelectorAll('input[type="checkbox"]');
        //for (var j = 0; j < docCheckbox.length; j++) {
        //    console.log(docCheckbox.checked);
        //}

        var items = {};
        items.parentKey = document.getElementsByClassName("parentKey")[i].value;
        items.recordGUID = document.getElementsByClassName("recordGUID")[i].value;
        items.item_SerialNumber = document.getElementsByClassName("SerialNumber")[i].value;
        items.Item_Part = document.getElementsByClassName("adnlInfoItem")[i].value;
        items.refNos = document.getElementsByClassName("refNos")[i].value;
        items.remarks = document.getElementsByClassName("remarks")[i].value;
        for (var r = 0; r < docCheckbox.length; r++) {
            if (docCheckbox[r].checked) {
                var adnlInfoDocs = docCheckbox[r].getAttribute('data-attachmentKey');
                DocumentData.push(adnlInfoDocs);
            }
        }
        if (DocumentData.length > 0) {
            items.documents = DocumentData.join(',');
        }


        if (items.Item_Part != "0") {
            AdditionalInfoRawData.push(items);
        }
    }

    // console.log(AdditionalInfoRawData);
    //console.log(AdditionalInfoRawData);
    $.ajax({
        type: "POST",
        url: '/DemandManagement/SaveAdditionalInfo',
        data: {
            AdnlInfoData: JSON.stringify(AdditionalInfoRawData),
        },
        cache: false,
        success: function (data) {
            if (data.success) {
                bootbox.alert({
                    message: "Submitted Successfully",
                    callback: function () {
                        GetViewDemandItemReciptSplit();
                        var btn = document.getElementsByClassName("bootbox-close-button")[1];
                        btn.click();
                    }
                });
            } else {
                alert("Failed");
            }
        },
        failure: function (response) {
            alert("Failed");
        }
    });
}

function DelDemandReciptSplit(id, ele) {

    if (id == 0) {
        RemoveReceiptSplit(ele);
    } else {
        bootbox.confirm("Are you sure you want to delete this Split ", function (result) {
            if (result) {
                $.ajax({
                    type: 'GET',
                    url: '/DemandManagement/DeleteReceiptItemSplits?id=' + id,
                    success: function (data) {
                        if (data.success) {
                            bootbox.alert("Deleted Successfully");
                            RemoveReceiptSplit(ele);
                        }
                        else {
                            bootbox.alert("Delete Failed")
                        }
                    },
                    error: function () {
                    }
                });
            }
        });
    }

}

function DeleteDemandReceiptDocs(dockey, SplitId, ele) {
    bootbox.confirm("Are you sure you want to delete this document?", function (result) {
        if (result) {
            $.ajax({
                type: 'GET',
                url: '/DemandManagement/DeleteReceiptDocument?documentId=' + dockey + '&SplitId=' + SplitId,
                success: function (data) {
                    if (data.success) {
                        bootbox.alert('Deleted Successfully', function () {
                            var tblrow = ele.closest("tr");
                            document.getElementById("SplitModelDocTablbdy").deleteRow(tblrow.rowIndex - 1); //get the table
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


function SetTreeHeight() {
    var screenheight = window.innerHeight;
    var navheight = document.getElementById("navbarTop").clientHeight;
    var statusDisplayAreaheight = document.getElementById("statusDisplayArea").clientHeight;
    var treeheight = screenheight - ((navheight + statusDisplayAreaheight) + 120);
    document.getElementById("areaDemandjstree").style.height = treeheight + "px";

    //console.log(document.getElementById("areaDemandjstree").style.height);
    //console.log(treeheight);
}


function UpdateDocumentType(Attachment_Db_Key, dropdown) {
    var selectedOption = dropdown.options[dropdown.selectedIndex];
    var selectedValue = selectedOption.value;
    //console.log(selectedValue);
    //console.log(Attachment_Db_Key);
    $.ajax({
        type: 'GET',
        url: '/DemandManagement/UpdateReceiptDocType?documentId=' + Attachment_Db_Key + '&doctype=' + selectedValue,
        success: function (data) {
            if (data.success) {
                bootbox.alert('Updated Successfully', function () {

                });
            }
            else {
                bootbox.alert("Failed")
            }
        },
        error: function () {
        }
    });

    function getTable() {
        var demandDbkey = $("#Demands").val();
        //  console.log(demandDbkey);
        $.ajax({
            url: '/DemandManagement/GetDemandReceiptsHistoryJResult/' + demandDbkey,
            type: 'GET',
            success: function (data) {
                //  console.log(data);
                tableData = data;
                applyDatatable(tableData);
            }
        });
    }

    function applyDatatable(tableData) {

        try {
            dataTable.destroy();
        } catch (e) {
        }
        var dataTable;
        dataTable = $("#DemandHistory").DataTable({
            ajax: function (dataSent, callback, settings) {
                callback({ data: tableData });
            },
            responsive: true,
            paging: false,
            dom: 'Bfrtip',
            order: [
                [0, 'asc'],
                [1, 'asc'],
                [2, 'asc'],

            ],
            stateSave: true,
            stateSaveCallback: function (settings, data) {
                localStorage.setItem(
                    'DataTables_' + settings.sInstance,
                    JSON.stringify(data)
                );
            },
            stateLoadCallback: function (settings) {
                return JSON.parse(localStorage.getItem('DataTables_' + settings.sInstance));
            },
            rowGroup: {
                dataSrc: ['demand_No', 'mmG_File_No', 'receipt_No']
            },
            columnDefs: [
                {
                    targets: [0, 1, 2],
                    visible: false
                }
            ],
            "columns": [
                { "data": "demand_No" },
                { "data": "mmG_File_No" },
                { "data": "receipt_No" },
                { "data": "raw_material_Name" },
                { "data": "orderQty" },
                { "data": "receiptQty" },
                {
                    "data": null,
                    render: function (data) {
                        var Balance = data.orderQty - data.receiptQty;
                        return '<label>' + Balance + '</label>';
                    }
                },
                {
                    "data": null,
                    render: function (data) {
                        return '<input type="button" class="btn btn-primary btn-sm" value="Documents" onclick="getSplitInfoDisplayDialog(' + data.receipt_dbkey + ')" /> '
                    }
                }
            ],
            buttons: [
                'excel'
            ]
        });

    }

    function getSplitInfoDisplayDialog(receipt_dbkey) {

        var urlinput = "/DemandManagement/ReceiptItemSplits?Id=" + receipt_dbkey;
        $.get(urlinput).done(function (response) {
            bootbox.dialog({
                title: "Split Info",
                message: response,
                size: 'extra-large',
                closeButton: true,
                className: 'custom-modal',
            });
            //new Choices('.ItemDbKey', {
            //    searchEnabled: true,
            //});

        });

    }

    function getPartsData() {
        var urlinput = "/DemandManagement/PartsDataView/";
        $.get(urlinput).done(function (response) {
            //   console.log(response);
            document.getElementById("Parts-tab").innerHTML = response;
        });
    }



}
var demandListDataTbl;

function GetDemandListInTableFormat() {
    var projectDbkey = $('select#projectlist option:selected').val();
    var DemandingOfficerKey = $('select#Demand-officer-select option:selected').val();
    var urlinput = "/DemandManagement/DemandDetailsInTableFormat/?ProjectDbkey=" + projectDbkey + "&DemandingOfficerKey=" + DemandingOfficerKey;

    $.get(urlinput).done(function (response) {
        $('#tab-list').html(response);

        var $table = $('#demandDetailsTable');
        if ($table.length === 0) {
            console.error('demandDetailsTable not found in loaded response');
            return;
        }

        if ($.fn.DataTable.isDataTable('#demandDetailsTable')) {
            $('#demandDetailsTable').DataTable().clear().destroy();
        }

        demandListDataTbl = $table.DataTable({
            dom: '<"top d-flex justify-content-between align-items-center"Bf>rt<"bottom d-flex justify-content-between align-items-center"lip><"clear">',
            buttons: [
                {
                    extend: 'excel',
                    text: 'Excel',
                    className: 'btn btn-default',
                    exportOptions: {
                        columns: [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]
                    }
                }
            ],
            paging: true,
            pageLength: 25,
            lengthMenu: [10, 25, 50, 100],
            searching: true,
            info: true,
            ordering: false,
            autoWidth: false,
            responsive: false,
            scrollX: true,
            scrollY: 'calc(100vh - 360px)',
            scrollCollapse: true,
            deferRender: true,
            destroy: true,
            fixedHeader: false,
            rowGroup: {
                dataSrc: function (row) {
                    return row[0] + ' - ' + row[1];
                }
            },
            columnDefs: [
                { targets: [0, 1], visible: false },
                { targets: 2, width: "130px" },   // MMG Number
                { targets: 3, width: "220px" },   // Description
                { targets: 4, width: "70px" },    // Item type
                { targets: 5, width: "95px" },    // Estimated Cost
                { targets: 6, width: "95px" },    // Actual Cost
                { targets: 7, width: "110px" },   // Running Balance
                { targets: 8, width: "120px" },   // Payment Made Till Date
                { targets: 9, width: "120px" },   // Balance Order Value
                { targets: 10, width: "90px" },   // Tender Mode
                { targets: 11, width: "110px" },  // Demanding Officer
                { targets: 12, width: "160px" },  // Vendor
                { targets: 13, width: "95px" },   // Current Status
                { targets: 14, width: "170px" },  // Remarks
                { targets: 15, width: "90px" },   // Updated On
                { targets: 16, width: "60px", orderable: false, searchable: false } // View
            ],
            initComplete: function () {
                var api = this.api();
                setTimeout(function () {
                    api.columns.adjust();
                }, 300);
            },
            drawCallback: function () {
                var api = this.api();
                setTimeout(function () {
                    api.columns.adjust();
                }, 50);

                if (window.initializeReadMore) {
                    window.initializeReadMore();
                }
            }
        });

        setTimeout(function () {
            if (demandListDataTbl) {
                demandListDataTbl.columns.adjust();
            }
        }, 500);

        $(window)
            .off('resize.demandDetailsTable')
            .on('resize.demandDetailsTable', function () {
                if (demandListDataTbl) {
                    demandListDataTbl.columns.adjust();
                }
            });

    }).fail(function (xhr, status, error) {
        console.error('Failed to load DemandDetailsInTableFormat:', error);
        $('#tab-list').html('<div class="alert alert-danger">Failed to load list data.</div>');
    });
}


function goTODetailsTab(demandDbkey) {
    $('#Demand-select').val(demandDbkey).trigger('change.select2');
    ShowDemandDetail(demandDbkey)
    document.getElementById('details-tab').click();
}

function goToListTab() {
    document.getElementById('list-tab').click();
}

function GetProject_Do_Report(projectDbkey, DemandOfficerKey) {
    var urlinput = "/DemandManagement/Project_DemandingOfficer_Report/?ProjectDbkey=" + projectDbkey + "&DemandingOfficerDbKey=" + DemandOfficerKey;
    $.get(urlinput).done(function (response) {
        document.getElementById("project_DO_report").innerHTML = response;
        //$('#demandDetailsTable').DataTable({
        //    paging: false
        //});
    });
}

function toogleProject(project, action) {

    var targetElements = document.getElementsByClassName("demandingOfficers-" + project);
    var IconElementOpen = document.getElementById("icon-Open-" + project);
    var IconElementClose = document.getElementById("icon-Close-" + project);
    $(IconElementOpen).toggle();
    $(IconElementClose).toggle();

    Array.from(targetElements).forEach(function (tr) {
        if (action == 'Open') {
            tr.style.display = '';
        } else {
            tr.style.display = 'none';
        }
    });
}

function toggleDemandStatus(action) {
    var targetElements = document.getElementsByClassName("open-status-cols");
    var openIconDemandsStatus = document.getElementById("openIconDemandsStatus");
    var closeIconDemandsStatus = document.getElementById("closeIconDemandsStatus");
    $(openIconDemandsStatus).toggle();
    $(closeIconDemandsStatus).toggle();
    Array.from(targetElements).forEach(function (tr) {
        if (action == 'Open') {
            tr.style.display = '';
        } else {
            tr.style.display = 'none';
        }
    });

}

function getDemandListPopup(DemandingOfficerKey, ProjectDbkey, CurrrentStatus) {
    var urlinput = "/DemandManagement/DemandsList/?DemadingOfficerKey=" + DemandingOfficerKey + "&ProjectDbkey=" + ProjectDbkey + "&CurrrentStatus=" + CurrrentStatus;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "List of Demands",
            message: response,
            size: 'extra-large',
            closeButton: true,
            // className: 'custom-modal',
        });
        $('#demandListTbl').DataTable({
            paging: false
        });
    });
}

function createMilestone(DemandDbkey, EstimatedOrderDate) {
    if (EstimatedOrderDate == "") {
        alert("Please Add the Order Date");
        return;
    }
    var urlinput = "/DemandManagement/CreateMilestone/?DemandDbkey=" + DemandDbkey + "&EstimatedOrderDate=" + EstimatedOrderDate;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Create Milestone",
            message: response,
            size: 'medium',
            closeButton: true,
            // className: 'custom-modal',
        });
    });
}

function SaveMilestoneForm() {
    var form = $('#createMilestoneForm');
    $.validator.unobtrusive.parse("#" + form.attr("id"));
    if (form.valid() == false) {
        return false;
    }
    var milestoneID = document.getElementById("MilestoneID").value;
    var demandDbKey = document.getElementById("DemandDbKey").value;
    var milestoneName = document.getElementById("MilestoneName").value;
    var dueDate = document.getElementById("DueDate").value;
    var remarks = document.getElementById("Remarks").value;
    var qtyPercentage = document.getElementById("QtyPercentage").value;
    var isLastMilestone = document.getElementById("IsLastMilestone").checked;
    var milestoneNo = document.getElementById("MilestoneNo").value;

    var procurementMilestone = {
        MilestoneID: milestoneID,
        DemandDbKey: demandDbKey,
        MilestoneName: milestoneName,
        DueDate: dueDate,
        Comments: remarks,
        QtyPercentage: qtyPercentage,
        IsLastMilestone: isLastMilestone,
        MilestoneNo: milestoneNo
    }
    bootbox.hideAll();
    let dialog = bootbox.dialog({
        message: '<p class="text-center mb-0"><i class="fas fa-spin fa-cog"></i> Please wait while creating this milestone ..</p>',
        closeButton: false
    });
    $.ajax({
        url: '/DemandManagement/CreateMilestone',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(procurementMilestone),

        success: function (response) {
            // console.log(response);
            bootbox.hideAll();
            if (response.success == true) {
                bootbox.alert({
                    message: "Created Successfully",
                    callback: function () {
                        bootbox.hideAll();
                        GetViewMilestoneDetails(false);
                    }
                });
            }
            else if (response.success == "allZeros") {
                bootbox.alert("All items are already used in other milestones");
            }
            else {
                bootbox.alert("Failed to submit");
            }

        },
        error: function (xhr, status, error) {
            bootbox.hideAll();
            console.error('Error:', error);
        }
    });
}

function displayWarningMessage() {
    var isLastMilestone = document.getElementById("IsLastMilestone").checked;
    var warningElement = document.getElementById("warningMsg");
    //var plannedDateOfCompletion = document.getElementById("plannedDateOfReceipt").value;
    //var formattedDate = moment(plannedDateOfCompletion).format("YYYY-MM-DD");
    //var OrderDate = document.getElementById("EstimatedOrderDate").value;
    // const diffInDays = (moment(plannedDateOfCompletion)).diff(moment(OrderDate), 'days');
    if (isLastMilestone) {
        warningElement.innerText = "Remaining quantity of all items will be added in this milestone";
        warningElement.style.display = '';
        document.getElementById("QtyPercentage").value = 100;
        //console.log(formattedDate);
        //document.getElementById("DueDate").value = formattedDate;
        //document.getElementById("NoOfDays").value = diffInDays;

    }
    else {
        warningElement.style.display = 'none';
        document.getElementById("QtyPercentage").value = null;
    }

}


function extendMileStone(DemandDBKey, MilestoneID) {
    var urlinput = "/DemandManagement/ExtendMilestone?demandDBkey=" + DemandDBKey + "&MilestoneID=" + MilestoneID;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Extend Milestone",
            message: response,
            size: 'large',
            closeButton: true,
            // className: 'custom-modal',
        });
    });
}
function validateExtendedDate() {
    var extendedDateofMilestone = new Date(document.getElementById('ExtendedDate').value);
    var dueDateElements = document.querySelectorAll('#MileStoneListTbl tbody .due-date');
    var maxDueDate = new Date(0); // Initialize to a very early date

    dueDateElements.forEach(function (element) {
        var dueDate = new Date(element.textContent);
        if (dueDate > maxDueDate) {
            maxDueDate = dueDate; // Update max due date if current due date is later
        }
    });

    if (extendedDateofMilestone <= maxDueDate) {
        alert('Please select a date after the highest due date.');
        document.getElementById('ExtendedDate').value = ''; // Clear invalid date
    }
}


function selectAllCheckboxes(masterCheckbox) {
    var checkboxes = document.querySelectorAll('#MileStoneListTbl tbody .milestone-checkbox');
    checkboxes.forEach(function (checkbox) {
        checkbox.checked = masterCheckbox.checked;
    });
}


function saveExtendedMileStone() {
    var extendedDate = document.getElementById('ExtendedDate').value;
    var updatedBy = document.getElementById('UserID').value;

    if (!extendedDate) {
        $.notify("There is no Order Date Please fill the Order Date ");
        return;
    }
    var milestoneID = '';
    var formData = [];

    var checkboxes = document.querySelectorAll('.milestone-checkbox:checked');


    if (checkboxes.length === 0) {
        alert('Please select at least one milestone to extend.');
        return;
    }

    checkboxes.forEach(function (checkbox) {
        var milestoneDbKey = checkbox.getAttribute('data-milestonedbkey');

        milestoneID += milestoneDbKey + ',';
    });


    formData.push({
        MilestoneID: milestoneID,
        ExtendedDate: extendedDate,
        UpdatedBy: updatedBy
    });
    //console.log('Form Data:', formData);


    $.ajax({
        url: '/DemandManagement/SaveExtendedMileStone',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(formData),

        success: function (response) {
            //console.log(response);
            if (response.success == true) {
                bootbox.alert({
                    message: "MileStone Extended Successfully",
                    callback: function () {
                        const currentUrl = window.location.href;
                        // console.log(currentUrl);
                        if (currentUrl.includes("DemandManagement/MilestoneReport")) {
                            window.location.reload();
                            return false;
                        }
                        GetViewMilestoneDetails(false);
                        bootbox.hideAll();
                    }
                });
            }
            else if (response.success == false) {
                bootbox.alert(response.message);
            }
            else {
                bootbox.alert("Failed to submit");
            }

        },
        error: function (xhr, status, error) {
            dialog.modal('hide');
            console.error('Error:', error);
        }
    });

}



// Demand Milestone Status summary page

function showMilestoneItems(demandDbkey, milstoneID, action) {
    var targetElement = document.getElementById("Demannd_" + milstoneID);
    var IconElementOpen = document.getElementById("icon-Open-" + milstoneID);
    var IconElementClose = document.getElementById("icon-Close-" + milstoneID);
    //console.log(targetElement);
    $(IconElementOpen).toggle();
    $(IconElementClose).toggle();

    if (action == "Close") {
        $(targetElement).toggle();
        return false;
    }

    var urlinput = '/DemandManagement/MilestoneItemsForSummaryTable?DemandDbkey=' + demandDbkey + '&MilestoneId=' + milstoneID;
    $.get(urlinput).done(function (response) {
        //console.log(response);
        targetElement.children[0].innerHTML = response;
        //targetElement.innerHTML = response;
        targetElement.style.display = "";
    });
}


function calculateDueDate() {

    var numberOfDays = parseInt(document.getElementById('NoOfDays').value, 10);
    var estimatedOrderDateString = document.getElementById('EstimatedOrderDate').value;
    //console.log(estimatedOrderDateString);
    var dateFormat = 'DD-MMM-YY';
    var estimatedOrderDate = moment(estimatedOrderDateString, dateFormat, true);

    if (!estimatedOrderDate.isValid()) {
        console.log('Invalid date format. Please use "DD-MMM-YY".');
        return;
    }
    var dueDate = estimatedOrderDate.add(numberOfDays, 'days');
    var formattedDueDate = dueDate.format('YYYY-MM-DD');
    document.getElementById('DueDate').value = formattedDueDate;

}

function filterMilstoneSummary() {
    var Project = document.getElementById("Project-select").value;
    var Demand_officer = document.getElementById("Demand-officer-select").value;
    var Milestone_status = document.getElementById("Milestone-status-select").value;
    var Milestone_due = document.getElementById("Milestone-due-select").value;
    var DemandDbkey = document.getElementById("Milestone-Demand-select").value;
    var urlDemandList = "/DemandManagement/GetDemandList?ProjectId=" + Project + "&demandingofficerId=" + Demand_officer;

    $.get(urlDemandList).done(function (data) {
        var demandlist = data;
        var selectList = $('#Milestone-Demand-select');
        selectList.empty();
        selectList.append($('<option>').text('Select').val(0));
        $.each(demandlist, function (index, demandlist) {
            var demandDisplayText = demandlist.mmG_File_No + '-' + demandlist.item_Description;
            var option = $('<option>').text(demandDisplayText).val(demandlist.demandDbKey);
            selectList.append(option);
        });
    });


    $.ajax({
        url: `/DemandManagement/MilestoneStatusReport?demandingOfficerkey=${Demand_officer}&status=${Milestone_status}&duedate=${Milestone_due}&project=${Project}&demandDbkey=${DemandDbkey}&viewMode=MilestoneReportScreen`,
        type: "GET",
        success: function (data) {
            document.getElementById("milestone-report-area").innerHTML = data;
        },
        error: function (error) {
            console.error("Error fetching data:", error);
        }
    });
}


var executileSummaryDataTbl;
function GetExecutiveSummary(filter) {


    var projectDbkey = $('select#projectlist option:selected').val();
    var DemandingOfficerKey = $('select#Demand-officer-select option:selected').val();
    var urlinput = "/DemandManagement/ExecutiveSummary/?ProjectDbkey=" + projectDbkey + "&DemandingOfficerKey=" + DemandingOfficerKey + "&filter=" + filter;

    $.get(urlinput).done(function (response) {
        // Update the table content
        document.getElementById("tab-executive-summary").innerHTML = response;

        executileSummaryDataTbl = $('#executive-summary-tbl').DataTable({

            dom: 'Bfrtip', // Add the button container
            //  dom: '<"top"lfB>rtip',
            buttons: [
                {
                    extend: 'excel',
                    text: 'Excel',
                    className: 'btn btn-default',
                    // Uncomment and adjust exportOptions if specific columns need exporting
                    // exportOptions: {
                    //     columns: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]
                    // }
                }
            ],

            paging: false, // Disable paging
            autoWidth: false, // Disable auto width calculation
            lengthChange: true, // Enable length change dropdown
            rowGroup: {
                dataSrc: function (row) {
                    return row[0] + '-' + row[1]; // Adjust based on data
                }
            },
            columnDefs: [
                {
                    targets: [0, 1], // Columns to hide (adjust indices as needed)
                    visible: false
                }
            ],
            rowCallback: function (row, data) {
                // Exclude rows with the 'demand-items-row' class from DataTables processing
                if ($(row).hasClass('demand-items-row')) {
                    $(row).remove();
                }
            },
            createdRow: function (row, data, dataIndex) {
                // Add specific behavior for dynamically added rows if needed
                if ($(row).hasClass('demand-items-row')) {
                    $(row).css('display', 'none'); // Ensure hidden rows remain hidden
                }
            }
        });


        var clickedBtn = document.getElementById('btn-' + filter);
        clickedBtn.classList.add('btn-outline-primary');
        clickedBtn.classList.add('btn-primary');
        // Reinitialize DataTable for newly added rows (if dynamically added rows are present)
        $('#executive-summary-tbl').on('click', '.details-control', function () {
            var tr = $(this).closest('tr');
            var row = executileSummaryDataTbl.row(tr);

            if (row.child.isShown()) {
                // Close the row
                var demandDbkey = tr.data('demand-db-key');
                var IconElementOpen = document.getElementById("di-icon-Open-" + demandDbkey);
                var IconElementClose = document.getElementById("di-icon-Close-" + demandDbkey);
                //console.log(targetElement);
                $(IconElementOpen).toggle();
                $(IconElementClose).toggle();
                row.child.hide();
                tr.removeClass('shown');
            } else {
                // Open the row and load dynamic content
                var demandDbkey = tr.data('demand-db-key'); // Ensure correct key is used
                var IconElementOpen = document.getElementById("di-icon-Open-" + demandDbkey);
                var IconElementClose = document.getElementById("di-icon-Close-" + demandDbkey);
                //console.log(targetElement);
                $(IconElementOpen).toggle();
                $(IconElementClose).toggle();
                $.get('/DemandManagement/ViewDemandItems?Id=' + demandDbkey + '&Viewtype=Readonly', function (response) {
                    var wrappedResponse = '<div style=" white-space:normal; padding-left:20px; padding-right:20px; margin-left:20px; margin-right:20px; ">' + response + '</div>';

                    row.child(wrappedResponse).show();
                    tr.addClass('shown');
                    tr.next('tr').css('background-color', '#d7e3ff'); // Add color to the expanded row
                });
            }
        });


    });

}


function filterItemtype(filter) {
    var actionCells = document.getElementsByClassName("Item_Typeclass");

    for (i = 0; i < actionCells.length; i++) {

        actionCells[i].parentElement.style.display = ""
        if (actionCells[i].innerText.trim() != filter && filter != "All") {

            actionCells[i].parentElement.style.display = "none";
        }
    }

}


function showDemandItems(demandDbkey, action) {
    var targetElement = document.getElementById("Demannditems_" + demandDbkey);
    var IconElementOpen = document.getElementById("di-icon-Open-" + demandDbkey);
    var IconElementClose = document.getElementById("di-icon-Close-" + demandDbkey);
    //console.log(targetElement);
    $(IconElementOpen).toggle();
    $(IconElementClose).toggle();

    if (action == "Close") {
        $(targetElement).toggle();
        return false;
    }

    var urlinput = '/DemandManagement/ViewDemandItems?Id=' + demandDbkey + '&Viewtype=Readonly';
    $.get(urlinput).done(function (response) {
        if (targetElement) {
            targetElement.children[0].innerHTML = response; // Insert content
            targetElement.style.display = ""; // Show the hidden row
        } else {
            console.error("Target element not found.");
        }
    });
}

function filterExeSumDemand(filter) {

    GetExecutiveSummary(filter);
}


function GetDemandDashboard(itemType) {

    var urlinput = "/DemandManagement/DashboardSummary/?itemType=" + itemType;
    $.get(urlinput).done(function (response) {
        document.getElementById("tab-dashboard").innerHTML = response;
    });

}


function openAdjustmentPopup(demandItemKey) {
    $.ajax({
        type: "GET",
        url: "/DemandManagement/DemandItemQtyAdjustment",
        data: { demandItemKey: demandItemKey },
        success: function (data) {
            // Create modal if it doesn't exist
            if ($('#adjustmentModal').length === 0) {
                $('body').append('<div class="modal fade" id="adjustmentModal" tabindex="-1"></div>');
            }

            // Load partial view content into modal
            $('#adjustmentModal').html(data);

            // Show modal
            var modal = new bootstrap.Modal(document.getElementById('adjustmentModal'));
            modal.show();
        },
        error: function (error) {
            console.error("Error loading adjustment popup:", error);
            alert("Error loading adjustment popup. Please try again.");
        }
    });
}


// Add this JavaScript function to your Demand.js file or in a script section

function SaveDemandItemAdjustment() {
    // Get values from the popup
    var demandItemKey = parseInt($('#hdnDemandItemKey').val());
    var adjustmentDbkey = parseInt($('#hdnAdjustmentDbkey').val());
    var adjustmentQty = parseFloat($('#txtAdjustmentQty').val());
    var adjustmentRemarks = $('#txtAdjustmentRemarks').val().trim();

    // Validation
    if (isNaN(adjustmentQty)) {
        alert('Please enter a valid adjustment quantity');
        $('#txtAdjustmentQty').focus();
        return;
    }

    if (adjustmentRemarks === '') {
        alert('Please enter remarks for this adjustment');
        $('#txtAdjustmentRemarks').focus();
        return;
    }

    // Prepare data
    var data = {
        demandItemKey: demandItemKey,
        adjustmentDbkey: adjustmentDbkey,
        adjustmentQty: adjustmentQty,
        adjustmentRemarks: adjustmentRemarks
    };

    // Show loading indicator (optional)
    // You can add a spinner or disable the button here

    // AJAX call to save
    $.ajax({
        type: "POST",
        url: "/DemandManagement/SaveDemandItemAdjustment",
        data: data,
        dataType: "json",
        success: function (response) {
            if (response.success) {
                // Close modal
                var modal = bootstrap.Modal.getInstance(document.getElementById('adjustmentModal'));
                modal.hide();

                // Show success message
                alert(response.message);

                // Refresh the view to show updated adjustment
                GetViewDemandItems();
            } else {
                alert('Error: ' + response.message);
            }
        },
        error: function (error) {
            console.error("Error saving adjustment:", error);
            alert('Error saving adjustment. Please try again.');
        }
    });
}

function DeleteDemandReceipts(indexNo) {
    var confirmDelete = confirm("Are you sure you want to delete?");
    if (!confirmDelete) {
        return;
    }

    // Get the edit box for this index
    var editBox = document.querySelector(".receipItemEdit_" + indexNo);
    if (!editBox) {
        alert("Receipt not found on screen.");
        return;
    }

    // Read DemandDbKey from data attribute
    var demandDbKey = editBox.getAttribute("data-demandDbKey");

    // AJAX call to your ASP.NET Core action
    $.ajax({
        url: '/DemandManagement/DeleteDemandReceipts',
        type: 'POST',
        data: {
            demandDbKey: demandDbKey,
            indexNo: indexNo
        },
        success: function (result) {
            // result is the JSON from your action: { success: true/false, message: "..." }

            alert(result.message);

            if (result.success) {

                GetViewDemandItems();
            }
        },
        error: function (xhr, status, error) {
            alert("Error while deleting receipt: " + error);
        }
    });
}


function GoToBulkEditFinancialDetails() {
    var projectDbkey = $('select#projectlist option:selected').val();
    var demandingOfficerKey = $('select#Demand-officer-select option:selected').val();

    window.location.href = '/DemandManagement/BulkEditFinancialDetails?ProjectDbkey='
        + projectDbkey + '&DemandingOfficerKey=' + demandingOfficerKey;
}

