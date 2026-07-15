var jc;
var forgeingRcptTble;
var BATLIssueTbl;

function toggleDemandTree(action) {
    if (action == "open") {
        document.getElementById("ToggleOpenControl").style.display = "none";
        document.getElementById("treeControlPanel").style.display = "";
        document.getElementById("treeControlPanel").className = "col-md-4 row";
        document.getElementById("MI-detail-area").className = "col-md-8";
    }

    if (action == "close") {
        document.getElementById("ToggleOpenControl").style.display = "";
        document.getElementById("treeControlPanel").style.display = "none";
        document.getElementById("treeControlPanel").className = "";
        document.getElementById("MI-detail-area").className = "col-md-12";
    }
}



function SetTreeHeight() {
    var screenheight = window.innerHeight;
    var navheight = document.getElementById("navbarTop").clientHeight;
    var treeheight = screenheight - ((navheight) + 120);
    document.getElementById("areaDemandjstree").style.height = treeheight + "px";
}

function LoadMaterialJstree() {
    $(".search-input").keyup(function () {
        var searchString = $(this).val();
        $('#MaterialIssuejstree').jstree('search', searchString);
    });
    $.ajax({
        type: "Get",
        url: "/MaterialIssue/GetMaterialIssueJsTreeData",
        success: function (data) {
            //  console.log(data);
            var nodelog = $('#MaterialIssuejstree').jstree(
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
                   // console.log(data);
                    var nodedata = data.instance.get_node(data.selected[0]).id;
                  //  console.log(nodedata);
                    GetComponentDetail(nodedata)
                }).bind('ready.jstree', function (e, data) {

                });

        }
    });
}


function GetComponentDetail(nodeid) {
    console.log(nodeid);
    //var ComponentType = nodeid.split('_')[0];
    //var Key = nodeid.split('_')[1];
    //if (ComponentType == "Demand") {
   //console.log(nodeid);
    //if (nodeid.value != 0) {
    //    ShowMaterialIssueDetail(nodeid.value);
    //}
    //else {
    //    return false;
    //}

    if (nodeid != 0) {

        ShowMaterialIssueDetail(nodeid);
       // console.log(nodeid);
    }
    else {
        return false;
    }

    //}
}


function ShowMaterialIssueDetail(Id) {
    console.log(Id);
    window.location.href = "/MaterialIssue/ViewMaterialIssueDetail?Id=" + Id; 
    //var urlinput = "/MaterialIssue/ViewMaterialIssueDetail?Id=" + Id;
    //$.get(urlinput).done(function (response) {
    //     console.log(response);
        //document.getElementById("MIDiv").innerHTML = response;
        //console.log(response);
        //document.getElementById("MaterialIssueGlobalKey").value = Id;
        //GetMaterialIssueDetail();
        //GetViewForgeingReceipts();
        //GetViewForgeingReceiptsDocuments();
   // });
}


function GetMaterialIssueDetail() {
    var dbkey = $('#MaterialIssueGlobalKey').val();
    var urlinput = "/MaterialIssue/ViewMaterialIssueDetail?Id=" + dbkey;
    $.get(urlinput).done(function (response) {
        document.getElementById("tab-DemandDetail-list").innerHTML = response;
    });
}

function GetViewForgeingReceipts() {
    var dbkey = $('#MaterialIssueGlobalKey').val();
    var urlinput = "/MaterialIssue/ViewForgingReceipts?Id=" + dbkey;
    $.get(urlinput).done(function (response) {
        document.getElementById("tab-Receipts").innerHTML = response;
        forgeingRcptTble = $('#MaterialIssueItemstbl').DataTable({
            scrollX: true,
            paging: false,
            order: false,
            sorting: false,
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

            // pageLength: 50,
            scrollY: 400,
            dom: '<"top"lfB>rtip',
            buttons: [
                'excel'
            ],
            fixedColumns: {
                leftColumns: 4
            }
        });
        forgeingRcptTble.draw();
    });
}
var forgeingRcptDocTble;
function GetViewForgeingReceiptsDocuments() {
    var dbkey = $('#MaterialIssueGlobalKey').val();
    var urlinput = "/MaterialIssue/ViewForgingReceiptsDocuments?IssueDbkey=" + dbkey;
    $.get(urlinput).done(function (response) {
        document.getElementById("tab-ForgingRcpDocument").innerHTML = response;
        /*  forgeingRcptDocTble =*/
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
            },
        });
        /*    forgeingRcptDocTble.draw();*/
    });
}


function OpenReceiptCols(className) {
    var cols = document.getElementsByClassName(className);
    for (var i = 0; i < cols.length; i++) {
        cols[i].style.display = 'table-cell';
    }
    document.getElementById('open-receipt-items').style.display = 'none';
    document.getElementById('close-receipt-items').style.display = 'inline';
    forgeingRcptTble.draw();
}

function CloseReceiptCols(className) {
    var cols = document.getElementsByClassName(className);
    for (var j = 0; j < cols.length; j++) {
        cols[j].style.display = 'none';
    }
    document.getElementById('open-receipt-items').style.display = 'inline';
    document.getElementById('close-receipt-items').style.display = 'none';
    forgeingRcptTble.draw();
}

function PrintDiv(divId) {
    $('#' + divId).printThis({
        //beforePrintEvent: RemoveNonPrintElements(0),
        //afterPrint: RemoveNonPrintElements(1),
    });
}

var MI_Model = null;

function EditMaterialIssue(Id) {
    var urlinput = "/MaterialIssue/NewMaterialIssue?id=" + Id + "&Viewtype=Edit";
    $.get(urlinput).done(function (response) {
        MI_Model = bootbox.dialog({
            title: "Material Issue",
            message: response,
            size: 'extra-large',
            closeButton: true,
            className: 'custom-modal',
        });

        // Initialize Select2 for Demand dropdown after modal is fully shown
        MI_Model.on('shown.bs.modal', function () {
            initDemandSelect2();
        });

        var drawingNoElements = document.querySelectorAll('.Drawing_no');
        var rawMaterialElements = document.querySelectorAll('.Raw_material_Dbkey');


        // Create the options for Raw_material_Dbkey once
        //var partsListOptionsFragment = document.createDocumentFragment();
        //PartsListForDropDownJson.forEach(function (option) {
        //    var optionElement = document.createElement('option');
        //    optionElement.value = option.value;
        //    optionElement.text = option.text;
        //    partsListOptionsFragment.appendChild(optionElement);
        //});

        //// Populate and initialize Drawing_no select elements
        //drawingNoElements.forEach(function (select) {
        //    var drawingNoHiddenField = select.parentNode.querySelector('.hdnPartNumberKey').value;
        //    select.appendChild(partsListOptionsFragment.cloneNode(true));
        //    JSON.parse(drawingNoHiddenField).forEach(function (value) {
        //        var option = select.querySelector('option[value="' + value + '"]');
        //        if (option) {
        //            option.selected = true;
        //        }
        //    });

        //    new TomSelect(select, {
        //        plugins: ['remove_button'],
        //        sortField: {
        //            field: "text",
        //            direction: "asc"
        //        },
        //        search: true
        //    });
        //});

        // Phase 2 Optimized: Batch-load all pre-selected parts in ONE call, then init TomSelect from cache
        var allPartKeys = [];
        var rowPartKeysMap = []; // stores { selectElement, keys[] } per row

        var drawingNoElements = document.querySelectorAll('.Drawing_no');
        drawingNoElements = Array.prototype.slice.call(drawingNoElements);

        var rawMaterialElements = document.querySelectorAll('.Raw_material_Dbkey');

        // Create the options for Raw_material_Dbkey once
        var rawMaterialOptionsFragment = document.createDocumentFragment();
        rawMaterialListJson.forEach(function (option) {
            var optionElement = document.createElement('option');
            optionElement.value = option.value;
            optionElement.text = option.text;
            rawMaterialOptionsFragment.appendChild(optionElement);
        });

        // Populate Raw_material_Dbkey select elements
        rawMaterialElements.forEach(function (select) {
            var rawMaterialIDHiddenField = select.parentNode.querySelector('.hdnRawMaterialKey').value;
            select.appendChild(rawMaterialOptionsFragment.cloneNode(true));
            select.value = rawMaterialIDHiddenField;
        });

        // Batch-load all pre-selected parts in ONE call
        var allPartKeys = [];
        var rowPartKeysMap = [];

        drawingNoElements.forEach(function (select) {
            var hdnField = select.parentNode.querySelector('.hdnPartNumberKey');
            if (!hdnField) return;
            var selectedKeys = JSON.parse(hdnField.value);
            rowPartKeysMap.push({ select: select, keys: selectedKeys });
            selectedKeys.forEach(function (key) {
                if (allPartKeys.indexOf(key) === -1) {
                    allPartKeys.push(key);
                }
            });
        });

        if (allPartKeys.length > 0) {
            fetch('/MaterialIssue/GetPartsByKeys', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(allPartKeys)
            })
                .then(function (response) { return response.json(); })
                .then(function (allPartsData) {
                    var partsLookup = {};
                    allPartsData.forEach(function (p) {
                        partsLookup[p.value] = p;
                    });

                    rowPartKeysMap.forEach(function (row) {
                        var preSelected = row.keys
                            .filter(function (k) { return partsLookup[k]; })
                            .map(function (k) { return partsLookup[k]; });
                        initPartsTomSelect(row.select, preSelected);
                    });

                    window._partsLookupCache = partsLookup;
                })
                .catch(function () {
                    rowPartKeysMap.forEach(function (row) {
                        initPartsTomSelect(row.select, []);
                    });
                });
        } else {
            drawingNoElements.forEach(function (select) {
                initPartsTomSelect(select, []);
            });
        }

        // Create the options for Raw_material_Dbkey once
        var rawMaterialOptionsFragment = document.createDocumentFragment();
        rawMaterialListJson.forEach(function (option) {
            var optionElement = document.createElement('option');
            optionElement.value = option.value;
            optionElement.text = option.text;
            rawMaterialOptionsFragment.appendChild(optionElement);
        });

        // Populate Raw_material_Dbkey select elements
        rawMaterialElements.forEach(function (select) {
            var rawMaterialIDHiddenField = select.parentNode.querySelector('.hdnRawMaterialKey').value;
            select.appendChild(rawMaterialOptionsFragment.cloneNode(true));
            select.value = rawMaterialIDHiddenField;
        });

      
    });
}



//function addRow() {
//    var dpdwns = document.getElementsByClassName("new-row-item");

//        //for (i = 0; i < dpdwns.length; i++) {
//        //    try {
//        //        $(dpdwns[i]).select2('destroy');
//        //    } catch (e) {

//        //    }
//        //}


//    var PartDbKey = document.getElementById("PartDbKey").value;
//    var x = document.getElementById("tblbody");  //get the table
//    var node = x.rows[0].cloneNode(true);    //clone the previous node or row

//    node.style = null;
//    x.appendChild(node);   //add; the node or row to the table

//    var lastRow = $(x).find('tr:last');
//    //console.log($(lastRow).find('.Drawing_no_New'));
//    // Initialize TomSelect on the new dropdowns only

//  //  var selectControl = $(node).find('.Drawing_no').data('tomselect');
//   // console.log(selectControl);
//    //  selectControl.destroy();

//    var newSelect = $(lastRow).find('.Drawing_no_New');
//    new TomSelect(newSelect, {
//        //create: true,
//        plugins: ['remove_button'],
//        sortField: {
//            field: "text",
//            direction: "asc"
//        },
//        search: true,
//        //dropdownParent: $('.modal-content')
//    });

//    //for (i = 0; i < dpdwns.length; i++) {
//    //    $(dpdwns[i]).TomSelect({
//    //        dropdownParent: $('#mi-issue-table .modal-content')
//    //    });
//    //}
//    //var lastRow = $(x).find('tr:last');
//    //var newCell = $(lastRow).find('.Drawing_no');
//    //var selectElement = $(newCell).find('select');
//    //console.log(selectElement.prevObject[0]);
//    //new TomSelect(selectElement.prevObject[0], {
//    //    //create: true,
//    //    searchable: true,
//    //    plugins: ['remove_button'],
//    //    sortField: {
//    //        field: "text",
//    //        direction: "asc"
//    //    }
//    //});


//}

function addRow() {
    var x = document.getElementById("tblbody");
    var node = x.rows[0].cloneNode(true);
    node.style = null;
    x.appendChild(node);

    var lastRow = x.querySelector('tr:last-child');

    // Clear all inputs
    lastRow.querySelectorAll('input[type="text"], textarea').forEach(function (el) { el.value = ''; });
    lastRow.querySelector('.Issue_Item_Dbkey').value = '0';
    lastRow.querySelector('.hdnPartNumberKey').value = '';
    lastRow.querySelector('.hdnPartNames').value = '';
    lastRow.querySelector('.hdnRawMaterialKey').value = '';
    lastRow.querySelector('.hdnVendorKey').value = '';

    // Clear display badges
    lastRow.querySelector('.parts-badges').innerHTML = '<span class="text-muted" style="font-size:0.75rem;">Click ✏️ to add</span>';
    lastRow.querySelector('.rm-badges').innerHTML = '<span class="text-muted" style="font-size:0.75rem;">-</span>';
    lastRow.querySelector('.vendor-badges').innerHTML = '<span class="text-muted" style="font-size:0.75rem;">-</span>';
}


function delRow(ele) {
    var tblrow = ele.closest("tr");
    document.getElementById("tblbody").deleteRow(tblrow.rowIndex - 1); //get the table
    //delete the last row
}

function GetPartListtoDropDown() {

    var Service_ref = new Array();
    $.ajax({
        cache: false,
        url: "/MaterialIssue/GetPartNoJson",
        dataType: "json",
        success: function (data) {
            $.each(data, function (index, data) {
                Service_ref.push(data);
            })
            Service_ref.sort(); 

            //$(".Drawing_no").autocomplete({
            //    source: data,
            //    delay: 0,
            //    minLength: 2,
            //    autoFocus: true,
            //    select: function (event, ui) {
            //        // prevent autocomplete from updating the textbox
            //        event.preventDefault();
            //        // manually update the textbox and hidden field
            //        $(this).val(ui.item.label);
            //        var parent = $(this).parent();
            //        parent.children[0].innerText = ui.item.label;
            //        // $(this).closest("tr").find("td:eq(1) input[type='hidden']").val(ui.item.value);
            //    }
            //});
        }
    });
}

function TotalCalaculation() {
    var table = document.getElementById("tblbody");
    var rows = table.getElementsByTagName("tr");
    var totIssueQty = 0;
    var totCost = 0;
    for (var i = 0; i < rows.length; i++) {
        if ($(table.rows[i].children[1].children[0]).val() != 0) {

            var qty = document.getElementsByClassName("Qty")[i];
            var orderQty = 0;

            if (isNaN(qty.value) || qty.value == "") {
                qty.value = 0;
                orderQty = 0;
            } else {
                orderQty = qty.value;
            }


            var Issueqty = document.getElementsByClassName("Qty_Issue")[i];
            var IssQty = 0;
            if (isNaN(Issueqty.value) || Issueqty.value == "") {
                Issueqty.value = 0;
                IssQty = 0;
            } else {
                IssQty = Issueqty.value;
            }

            totIssueQty = parseFloat(totIssueQty) + parseFloat(IssQty);


            var amount = document.getElementsByClassName("Amount")[i];
            var tAmount = 0;
            if (isNaN(amount.value) || amount.value == "") {
                amount.value = 0;
                tAmount = 0;
            } else {
                tAmount = amount.value;
            }

            totCost = parseFloat(totCost) + parseFloat(tAmount);
        }
    }

    document.getElementById("Total_Qty").value = totIssueQty;
    document.getElementById("Total_Cost").value = totCost;

}

function FillRawMaterialParameters(RawItem) {
    var item = RawItem.value;
    if (Number.isInteger(parseInt(item)) && parseInt(item) > 0) {
        $(RawItem).closest('tr').find("select").each(function () {
            if (this.id == 'item_outer_dia') {
                var selectList = $(this);
                selectList.empty();
                $.getJSON("/SharedCall/GetRawmaterialParaJResult", "id=" + RawItem.value + "&type=Outer_Dia", function (data) {
                    $.each(data, function (index, data) {
                        var option = $('<option>').text(data.Text).val(data.Value);
                        selectList.append(option);
                    });
                });
            }
            // if (this.id == 'item_height') {
            //     var selectList = $(this);
            //     selectList.empty();
            //     $.getJSON("/SharedCall/GetRawmaterialParaJResult", "id=" + RawItem.value + "&type=height", function (data) {
            //         $.each(data, function (index, data) {
            //             var option = $('<option>').text(data.Text).val(data.Value);
            //             selectList.append(option);
            //         });
            //     });
            // }

            if (this.id == 'item_thickness') {
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

function DoInactive(Dbkey, ele) {

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
                    url: "/MaterialIssue/DeleteIssueItem",
                    data: "id=" + Dbkey,
                    success: function (data) {
                        if (data.success) {
                            bootbox.alert({
                                message: "Removed successfully",
                                callback: function () {
                                    try {
                                        var tblrow = ele.closest("tr");
                                        if (tblrow) {
                                            document.getElementById("tblbody").deleteRow(tblrow.rowIndex - 1);
                                        }
                           
                                    } catch (e) {

                                    }
                     
                                }
                            })
                        } else {
                            bootbox.alert({
                                message: "You cannot delete this item because forging receipts transactions exist for the same.",
                                callback: function () {
                                    var tblrow = ele.closest("tr");
                                    document.getElementById("tblbody").deleteRow(tblrow.rowIndex - 1);
                                }
                            })
                        }
                    }
                });
            }
        }
    });

};

function SaveMaterialIssue() {
    if (!$('#CreateMaterialIssue').valid()) { return false }
    loadAllRemainingRows();
    var data = $("#CreateMaterialIssue").serialize();
    TotalCalaculation();
    var fileData = new FormData();

    var Material_Issue_main = new Array();
    var mainItems = {}; 

    mainItems.Issue_Dbkey = document.getElementById("Issue_Dbkey").value;
    mainItems.Engine_Name = document.getElementById("Engine_Name").value;
    //mainItems.Demand_No = document.getElementById("Demand_No").value;
    //mainItems.DemandDbKey = document.getElementById('Demand_Number').value;
    // Get demand number text from dropdown selected option
    var demandDropdown = document.getElementById("Demand_Number");
    mainItems.Demand_No = demandDropdown.options[demandDropdown.selectedIndex]?.text || '';
    mainItems.DemandDbKey = demandDropdown.value;
    mainItems.Order_Ref_No = document.getElementById("Order_Ref_No").value;
    mainItems.Order_Ref_Date = document.getElementById("Order_Ref_Date").value;
    mainItems.Vendor = document.getElementById("Vendor").value;
    mainItems.Total_Qty = document.getElementById("Total_Qty").value;
    mainItems.Total_Cost = document.getElementById("Total_Cost").value;
    mainItems.Returnable = document.getElementById("Returnable").value;
    mainItems.Issue_Purpose = document.getElementById("Issue_Purpose").value;
   // mainItems.PMO_Ref_No = document.getElementById("PMO_Ref_No").value;
   // mainItems.PMO_Ref_Date = document.getElementById("PMO_Ref_Date").value;
    Material_Issue_main.push(mainItems);

    // ---------------------------------------------------------------------
    var Material_Issue_Items = new Array();

    var table = document.getElementById("tblbody");
    var rows = table.getElementsByTagName("tr");

    var vaildateLineItem = true;
    var Amount = 0.0;

     
    for (var i = 1; i < rows.length; i++) {
        var mItems = {}; 
      
        // if (document.getElementsByClassName("Raw_material_Dbkey")[i].value != 0) {
        //console.log(table.rows[i].children[3].children[0]);
        if ($(table.rows[i].children[3].children[0]).val() != 0) {
            // mItems.Issue_Item_Dbkey = table.rows[i].cells[0].children[0].value;
            mItems.Issue_Item_Dbkey = document.getElementsByClassName("Issue_Item_Dbkey")[i].value; 
           // if (document.getElementsByClassName("Drawing_no")[i].value == "") {
            if ($(table.rows[i].children[1].children[0]).val() == "") {
                bootbox.alert({
                    title: 'Alert!',
                    message: 'Please enter the Part No for line item ' + i,
                });
                vaildateLineItem = false;
                break;
            }
            var amountValue = document.getElementsByClassName("Amount")[i].value;
            Amount = (amountValue && !isNaN(parseFloat(amountValue))) ? parseFloat(amountValue) : 0.0;

            
            // mItems.PartNumberKey = $(table.rows[i].children[1].children[0]).val();
            // Get part keys from hidden input (comma-separated)
            var partKeysStr = table.rows[i].querySelector('.hdnPartNumberKey').value;
            mItems.PartNumberKey = partKeysStr ? partKeysStr.split(',').map(Number).filter(function (n) { return !isNaN(n) && n > 0; }) : [];

            // Get raw material from hidden input
            mItems.Raw_material_Dbkey = table.rows[i].querySelector('.hdnRawMaterialKey').value || 0;

            // Get vendor from hidden input
            mItems.Vendor_Dbkey = table.rows[i].querySelector('.hdnVendorKey').value || 0;

            //mItems.Drawing_no = document.getElementsByClassName("Drawing_no")[i].value;                 //table.rows[i].cells[1].children[0].value;
            mItems.Qty = document.getElementsByClassName("Qty")[i].value;                                  //table.rows[i].cells[2].children[0].value;
            //mItems.Raw_material_Dbkey = document.getElementsByClassName("Raw_material_Dbkey")[i].value;    //table.rows[i].cells[3].children[0].value;
          //  mItems.Raw_material_Dbkey = $(table.rows[i].children[3].children[0]).val();   //table.rows[i].cells[3].children[0].value;
           // mItems.Vendor_Dbkey = document.getElementsByClassName("Vendor_Dbkey")[i].value;


            //   mItems.outer_dia = document.getElementsByClassName("outer_dia")[i].value;                      //table.rows[i].cells[4].children[0].value;
            //   mItems.thickness = document.getElementsByClassName("thickness")[i].value;                      //table.rows[i].cells[5].children[0].value;
            // mItems.height = document.getElementsByClassName("height").value;                          //table.rows[i].cells[6].children[0].value;
            mItems.Size = document.getElementsByClassName("Size")[i].value;                               //table.rows[i].cells[7].children[0].value;
            mItems.Denom = "Each";
            mItems.Qty_Issue = document.getElementsByClassName("Qty_Issue")[i].value;                      //table.rows[i].cells[8].children[0].value;
            mItems.Heat_No = document.getElementsByClassName("Heat_No")[i].value;                          //table.rows[i].cells[9].children[0].value;

            mItems.EngineLevel = document.getElementsByClassName("EngineLevel")[i].value;                   //Add

            mItems.Weight_Kg = document.getElementsByClassName("Weight_Kg")[i].value;                      //table.rows[i].cells[10].children[0].value;
            mItems.Amount = Amount;                         //table.rows[i].cells[11].children[0].value;
            mItems.SerialNo = document.getElementsByClassName("SerialNo")[i].value;
            mItems.jcFileName = document.getElementsByClassName("JCFileName")[i].value;
            mItems.JCFileLocation = document.getElementsByClassName("JCFileLocation")[i].value;
            mItems.JobCardNumber = document.getElementsByClassName("JobCardNumber")[i].value;              //table.rows[i].cells[12].children[0].value;
            Material_Issue_Items.push(mItems); 
            var fileinput = document.getElementsByClassName("itemFiles")[i];

            for (j = 0; j < fileinput.files.length; j++) {
                //Appending each file to FormData object
                fileData.append(i, fileinput.files[0]);
            }
        }
    }

    //-----------------------------------------------------------------------------
 
    //console.log(Material_Issue_main, Material_Issue_Items);

    fileData.append('MaterialIssuemain', JSON.stringify(Material_Issue_main));
    fileData.append('MaterialIssueItems', JSON.stringify(Material_Issue_Items));
   

    if (!vaildateLineItem) {
        return false;
    }
    if (Material_Issue_Items.length == 0) {
        $.alert({
            title: 'Alert!',
            content: 'Please enter atleast one line item !',
        });
        return false;
    }
    document.getElementById("SaveLnk").disabled = true;

    let dialog = bootbox.dialog({
        message: '<p class="text-center mb-0"><i class="fas fa-spin fa-cog"></i> Please wait while saving this Material Issue...</p>',
        closeButton: false
    });
    $.ajax({
        type: "POST",
        url: "/MaterialIssue/SaveMaterialIssue",
        data: fileData,
        //data: {
        //    MaterialIssuemain: JSON.stringify(Material_Issue_main),
        //    MaterialIssueItems: JSON.stringify(Material_Issue_Items)
        //},
        cache: false,
        contentType: false,
        processData: false,
        // dataType: "json",
        // contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.success) {
              //  document.getElementById("SavePrint").disabled = false;
                document.getElementById("Issue_Dbkey").value = data.Dbkey;
                dialog.modal('hide');
                bootbox.alert({
                    message: "Submitted Successfully",
                    callback: function () {
                        window.location.reload();
                       // bootbox.hideAll();
                    }
                });
              
            } else {
                dialog.modal('hide');
                alert("Failed");
                document.getElementById("SaveLnk").disabled = false;
            }
        },
        failure: function (response) {
            alert("Failed");
            document.getElementById("SaveLnk").disabled = false;
        }
    });
}

function PrintIssue() {
    var a = document.createElement('a');
    a.href = '/MaterialIssue/PrintMaterialIssue/' + document.getElementById("Issue_Dbkey").value;
    a.setAttribute('target', '_blank');
    a.click();
}

function OpenFileDialog(ctrl) {
    var row = ctrl.closest('tr');
    var jobCardInput = row.querySelector('.JobCardNumber');
    if (jobCardInput) {
        var jobCardVal = jobCardInput.value || row.querySelector('.cell-jobcard .display-text')?.textContent || '';
        if (!jobCardVal || jobCardVal.trim() === '' || jobCardVal.trim() === '-') {
            bootbox.alert("Please enter Job Card Number");
            return;
        }
    }

    var td = ctrl.closest('td');
    var fileInput = td.querySelector('.itemFiles');
    if (fileInput) {
        fileInput.click();
    }
}

function UpdateJCUploadCtrlColour(ctrl) {
    var td = ctrl.closest('td');
    var uploadIcon = td.querySelector('.fa-upload');
    if (ctrl.files && ctrl.files.length > 0) {
        if (uploadIcon) uploadIcon.style.color = 'green';
    } else {
        if (uploadIcon) uploadIcon.style.color = 'royalblue';
    }
}

function closeBootboxes() {
    var btn = document.getElementsByClassName("bootbox-close-button")[0];
    btn.click();
}


function EditForgeingReceipts(ReceiptIndex) {

    var editBoxes = document.getElementsByClassName("receipItemEdit_" + ReceiptIndex);
    var editBoxesDrno = document.getElementsByClassName("receipItemEditDrawNo_" + ReceiptIndex);
    var DisplayBoxes = document.getElementsByClassName("receipItemDisplay_" + ReceiptIndex);
    var headers = document.getElementsByClassName("receipheaderEdit_" + ReceiptIndex);

    for (var i = 0; i < editBoxes.length; i++) {
        editBoxes[i].style.display = '';
    }

    for (var i = 0; i < editBoxesDrno.length; i++) {
        editBoxesDrno[i].style.display = '';
    }


    for (var i = 0; i < DisplayBoxes.length; i++) {
        DisplayBoxes[i].style.display = 'none';
    }

    for (var i = 0; i < headers.length; i++) {
        headers[i].style.display = '';
    }
    document.getElementById("editControl-" + ReceiptIndex).style.display = "none";
    document.getElementById("SaveEditControl-" + ReceiptIndex).style.display = "";

    forgeingRcptTble.draw();


}


// Adding new column to add new receipts
function AddNewReciptColumn() {
    var checkNewItemExist = document.getElementsByClassName("NewReceiptItem");

    if (checkNewItemExist.length == 0) {
        document.getElementById("Receipts-tab").click();
        forgeingRcptTble.destroy();

        var table = document.getElementById("MaterialIssueItemstbl");

        var headerCell = document.createElement("th");
        headerCell.innerHTML = "<input type='text' placeholder='Receipt Number' class='receipheaderEdit_0 NewReceiptItem' /><br/><input type='date' class='receipheaderEdit_0' /><i onclick='SaveForgeingReceiptItems(0)' style='cursor:pointer;color:green' class='uil uil-save fs-2 float-end'></i>";
        table.querySelector("thead tr").appendChild(headerCell);

        var rows = table.querySelectorAll("tbody tr");
        for (var i = 0; i < rows.length; i++) {
            var itemkey = rows[i].querySelector("td:first-child input[type='number']")
            var newCell = rows[i].insertCell(-1);
            newCell.innerHTML = "<input type='number' placeholder='Receiving Inventory' data-IssueItemDbkey='" + itemkey.getAttribute("data-IssueItemDbkey") + "' style='max-width:60px' data-forgingitemdbkey='0'  data-forgingreceiptDbkey='0' class='receipItemEdit_0' value='0'  /><input placeholder='Vendor Drawing No' style='max-width:100px'  type='text' data-IssueItemDbkey='" + itemkey.getAttribute("data-IssueItemDbkey") + "' data-forgingitemdbkey='0' data-forgingreceiptDbkey='0' class='receipItemEditDrawNo_0'   />";
        }

        demandItemsTbl = $('#MaterialIssueItemstbl').DataTable({
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
                leftColumns: 4
            }
        });

        demandItemsTbl.columns.adjust().draw();
        $("#MaterialIssueItemstbl_wrapper > div.dataTables_scroll > div.dataTables_scrollBody").scrollLeft(10000);
    }
}


function SaveForgeingReceiptItems(ReceiptIndex) {
    var editBoxes = document.getElementsByClassName("receipItemEdit_" + ReceiptIndex);
    var editBoxesDrawingNumber = document.getElementsByClassName("receipItemEditDrawNo_" + ReceiptIndex);

    var forgeing_Receipts = new Array();

    for (var i = 0; i < editBoxes.length; i++) {
        editBoxes[i].style.display = 'none';
        var forging_recp_dbkey = editBoxes[i].getAttribute('data-forgingreceiptDbkey');
        var Issue_Item_Dbkey = editBoxes[i].getAttribute('data-IssueItemDbkey');
        var Receipt_Date = document.getElementsByClassName("receipheaderEdit_" + ReceiptIndex)[1].value;
        var Receipt_Number = document.getElementsByClassName("receipheaderEdit_" + ReceiptIndex)[0].value;

        var forging_item_dbkey = editBoxes[i].getAttribute('data-forgingitemdbkey');
        var HAL_Drawing_No = editBoxesDrawingNumber[i].value;
        var Receiving_inventory = editBoxes[i].value;

        var ForgeingReciptsItems = {};
        ForgeingReciptsItems.forging_recp_dbkey = forging_recp_dbkey;
        ForgeingReciptsItems.Receipt_Number = Receipt_Number;
        ForgeingReciptsItems.Receipt_Date = Receipt_Date;
        ForgeingReciptsItems.Issue_Item_Dbkey = Issue_Item_Dbkey;

        ForgeingReciptsItems.forging_item_dbkey = forging_item_dbkey;
        ForgeingReciptsItems.HAL_Drawing_No = HAL_Drawing_No;
        ForgeingReciptsItems.Receiving_Inventory = Receiving_inventory;

        if (Receiving_inventory > 0) {
            forgeing_Receipts.push(ForgeingReciptsItems);
        }

    }

    //for (var i = 0; i < DisplayBoxes.length; i++) {
    //    DisplayBoxes[i].style.display = '';
    //}
    //document.getElementById("editControl-" + ReceiptIndex).style.display = "";
    //document.getElementById("SaveEditControl-" + ReceiptIndex).style.display = "none";
    if (forgeing_Receipts.length == 0) {
        bootbox.alert('Please update the inventory for at least one line item.');
        return false;
    }


    var postData = JSON.stringify(forgeing_Receipts);

   // console.log(postData);

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
                    url: '/MaterialIssue/SaveForgingReceipt',
                    data: postData,
                    dataType: "json",
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        GetViewForgeingReceipts();
                    }
                })
            }

        }
    });

    //demandItemsTbl.draw();
}


function GetForgingSplitPopUp(forging_item_dbkey) {
    var htmldata = '<div class="col-12"> <div id="documents-area" class="col-12">  </div> <div id="SplitRecipts" class="col-12">  </div>  </div>'
    bootbox.dialog({
        title: "Forging Receipt Documents",
        message: htmldata,
        size: 'small',
        closeButton: true,
        className: 'custom-modal',
    });
    // AttachmentDoc(forging_item_dbkey);
    GetViewForgingItemReciptSplit(forging_item_dbkey);
}

function GetViewForgingItemReciptSplit(forging_item_dbkey) {
    var urlinput = "/MaterialIssue/ReceiptItemSplits?forging_item_dbkey=" + forging_item_dbkey;
    $.get(urlinput).done(function (response) {
        document.getElementById("SplitRecipts").innerHTML = response;
        //applySelect2OnReceiptSplit();
    });
}




function AttachmentDoc(forging_item_dbkey) {
    var urlinput = "/MaterialIssue/ReceiptsDocs?forging_item_dbkey=" + forging_item_dbkey;
    $.get(urlinput).done(function (response) {
        document.getElementById("documents-area").innerHTML = response;
    });
}

function UploadForginghReceiptDos(ID) {

    var filescount = 0;
    var recptDocs = document.getElementById("recptDocs_tblbody");
    var rows = recptDocs.getElementsByTagName("tr");

    var File_DVD_Num = document.getElementsByClassName("File_DVD_Num");
    var File_Revision = document.getElementsByClassName("File_Revision");
    var fileName = document.getElementsByClassName("fileName");

    var fileData = new FormData();


    if (fileName[0].files.length > 0) {
        fileData.append('uploadeddocument', fileName[0].files[0]);
        fileData.append('Source_table_key', ID);
        fileData.append('Source_table', 'Forging_Receipt_Items');
        fileData.append('Attachment_type', 'Forging_Receipt_Docs');
        fileData.append('File_DVD_Num', File_DVD_Num[0].value); //Used As a Document Master Type
        fileData.append('File_Revision', File_Revision[0].value); //Used As a Document Refrence number
    }

    if (fileName[0].files.length > 0) {

        $.ajax({
            url: "/Attachment/UploadFiles",
            type: 'POST',
            data: fileData,
            success: function (data) {
                if (data.success) {
                    if (data.success) {
                        bootbox.alert('Uploaded Successfully');
                        AttachmentDoc(ID);
                    }
                }
            },
            cache: false,
            contentType: false,
            processData: false,
        });
    } else {
        bootbox.alert('Please upload at least one document');
    }
    return false;
}

// pending ; will continue after splits 
function DeleteForgingRcpDocs(dockey, ele, receiptkey) {
    bootbox.confirm("Are you sure you want to delete this document ", function (result) {
        if (result) {
            $.ajax({
                type: 'GET',
                url: '/MaterialIssue/DeleteForgingRcpDocument?documentId=' + dockey + '&receiptDbkey=' + receiptkey,
                success: function (data) {
                    if (data.success) {
                        var tblrow = ele.closest("tr");
                        document.getElementById("recptDocs_tblbody").deleteRow(tblrow.rowIndex - 1); //get the table
                        bootbox.alert("Deleted Successfully");
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

function AddSplitRows() {
    var table = document.getElementById("forgingSplits");
    var node = table.rows[0].cloneNode(true);
    node.cells[0].children[0].value = 0;
    node.style.display = "";
    table.appendChild(node);
}


function delforgingSplitrow(ele) {

    var tblrow = ele.closest("tr");
    var Id = tblrow.cells[0].children[0].value;
    if (Id != 0) {
        Deleteconsolconfirm(Id, ele);
    } else {
        document.getElementById("forgingSplits").deleteRow(tblrow.rowIndex - 1); //get the table
    }
}

function DeleteForgeReceiptDocs(Attachment_Db_Key, forging_item_split_dbkey, ele, forging_item_dbkey) {
    bootbox.confirm("Are you sure you want to delete this document?", function (result) {
        if (result) {
            $.ajax({
                type: "POST",
                url: "/MaterialIssue/DeleteForgingSplitDocument?Attachment_Db_Key=" + Attachment_Db_Key + "&forging_item_split_dbkey=" + forging_item_split_dbkey,
                success: function (data) {
                    if (data.success) {
                        bootbox.alert({
                            message: "Deleted Successfully",
                            callback: function () {
                                var tblrow = ele.closest("tr");
                                document.getElementById("ForgingSplitModelDocTablbdy").deleteRow(tblrow.rowIndex - 1);
                            }
                        });
                        GetViewForgingItemReciptSplit(forging_item_dbkey);

                    } else {
                        bootbox.alert({
                            message: "Failed",
                            callback: function () {

                            }
                        });
                    }
                }
            });
        }
    });

}








function Deleteconsolconfirm(Id, ele) {
    bootbox.confirm("Are you sure you want to delete this Split?", function (result) {
        if (result) {
            $.ajax({
                type: "POST",
                url: "/MaterialIssue/DeleteForgingSplitItem?Id=" + Id,
                success: function (data) {
                    if (data.success) {
                        bootbox.alert({
                            message: "Deleted Successfully",
                            callback: function () {
                                var tblrow = ele.closest("tr");
                                document.getElementById("forgingSplits").deleteRow(tblrow.rowIndex - 1);
                            }
                        });
                    } else {
                        bootbox.alert({
                            message: "Failed",
                            callback: function () {

                            }
                        });
                    }
                }
            });
        }
    });
}


function SaveForgingItemSplits(forging_item_dbkey) {

    var table = document.getElementById("forgingSplits");
    var rows = table.getElementsByTagName("tr");
    var rawCons = new Array();

    for (var i = 1; i < rows.length; i++) {
        var items = {};
        var Attachment_Db_Key_Data = [];
        items.forging_item_split_dbkey = table.rows[i].cells[0].children[0].value;
        items.forging_item_dbkey = document.getElementById("forging_item_dbkey").value;
        items.part_name = table.rows[i].cells[1].innerHTML;
        items.GTRE_Drawing_No = table.rows[i].cells[2].children[0].value;
        items.Batch_Number = table.rows[i].cells[3].children[0].value;
        items.Heat_Number = table.rows[i].cells[4].children[0].value;
        items.Sl_No_Forging = table.rows[i].cells[5].children[0].value;

        var checkboxcell = rows[i].cells[6];
        var docCheckbox = checkboxcell.querySelectorAll('input[type="checkbox"]');

        for (var r = 0; r < docCheckbox.length; r++) {
            if (docCheckbox[r].checked) {
                var AttachmentDbKey = docCheckbox[r].getAttribute('data-attachmentKey');
                Attachment_Db_Key_Data.push(AttachmentDbKey);
            }
        }
        items.Attachment_Db_Key_Data = Attachment_Db_Key_Data;
        rawCons.push(items);
    }

    if (rawCons.length > 0) {
        $.ajax({
            type: "POST",
            url: '/MaterialIssue/SaveForgingItemSplits',
            data: {
                ForgingSplits: JSON.stringify(rawCons),
            },
            cache: false,
            success: function (data) {
                if (data.success) {
                    bootbox.alert({
                        message: "Submitted Successfully",
                        callback: function () {
                            // GetViewDemandItemReciptSplit(forging_item_dbkey);
                        }
                    });
                } else {
                    bootbox.alert('Failed!');
                }
            }
        });
    } else {
        bootbox.alert('Please enter the split data');
    }




}



function GetSplitItemMapping(Id, OnlyMappedItem) {
    var urlinput = "/MaterialIssue/GetSplitMapping?issueitemkey=" + Id + "&OnlyMappedItem=" + OnlyMappedItem;
    $.get(urlinput).done(function (response) {  
        bootbox.dialog({
            title: "Split Item Mapping",
            message: response,
            size: 'large',
            closeButton: true,
            className: 'custom-modal',
        });
    });
}

function SaveMaterialIssueSplitMapping() {

    var table = document.getElementById("tblMatIssueConsBody");
    var rows = table.getElementsByTagName("tr");
    var mainarray = new Array();
    for (var i = 0; i < rows.length; i++) {

        var items = {};
        if (table.rows[i].cells[13].children[0].checked == true) {
            items.split_issue_id = table.rows[i].cells[1].children[0].value;
            items.Issue_Item_Dbkey = table.rows[i].cells[1].children[1].value;
            items.Issue_Dbkey = table.rows[i].cells[1].children[2].value;
            items.DR_Item_SplitId = table.rows[i].cells[1].children[3].value;
            mainarray.push(items);
        }

    }
    if (mainarray.length != 0) {
        $.ajax({
            type: "POST",
            url: '/MaterialIssue/SaveMaterialIssueItemSplitMapping',
            data: {
                MaterialIssueConsolidation: JSON.stringify(mainarray),
            },
            cache: false,
            success: function (data) {
                if (data.success) {
                    bootbox.alert('Submitted Successfully', function () {
                      //  console.log('This was logged in the callback!');
                    });

                }
                else {
                    bootbox.alert('Failed !', function () {
                      //  console.log('This was logged in the callback!');
                    });
                }

            }
        });
    } else {
        alert("Please check at least one item");

    }
}


function expandMIArea() {
    document.getElementById("treeControlPanel").style.display = "none";
    document.getElementById("demand-area-toggle").innerText = "Collapse";

    $("#MI-detail-area").attr("class", "col-12");
    $("#demand-area-toggle").attr("onclick", "collapseDemandArea()");
}

function collapseDemandArea() {
    document.getElementById("treeControlPanel").style.display = "";
    document.getElementById("demand-area-toggle").innerText = "Expand";
    $("#MI-detail-area").attr("class", "col-9");
    $("#demand-area-toggle").attr("onclick", "expandMIArea()");

}

function editForgingSplitRow(forging_recp_dbkey, forging_item_split_dbkey) {
    var url = "/MaterialIssue/EditForgingSplitRow?forging_split_dbkey=" + forging_item_split_dbkey + "&forging_recp_dbkey=" + forging_recp_dbkey;
    $.get(url).done(function (response) {
      //  console.log(response);
        bootbox.dialog({
            title: "Receipt Splits",
            message: response,
            size: 'large',
            closeButton: true,
        });
    });
}


function SaveSplitItemModel(Receipt_dbkey, forging_item_dbkey) {
    var form = $('#ForgingSplitModelData');
    $.validator.unobtrusive.parse("#" + form.attr("id"));
    $(form).validate();
    //console.log($(form).serialize());
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
            fileItem.Source_table_key = forging_item_dbkey;
            fileItem.Source_table = "Forging_Receipt_Items";
            fileItem.File_DVD_Num = $(fileControl).closest('tr').find('td:first-child').children().first().val(); //fileRow.cells[1].children[0].value;
            fileItem.File_Revision = $(fileControl).closest('tr').find('td:nth-child(2)').children().first().val();
            fileData.push(fileItem);
            formData.append('files', splitUploadFiles[i].files[0]);
        }
    }
    console.log(JSON.stringify(fileData));
    formData.append('filesData', JSON.stringify(fileData));
  //  console.log(fileData);
   // console.log(formData);
    // data: { Procurement_Demand_Items_Split: JSON.stringify(Procurement_Demand_Items_Split) }, 
    $.ajax({
        url: '/MaterialIssue/SaveForgingReceiptSplitModel',
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            alert('Submitted Successfully');
            GetViewForgingItemReciptSplit(forging_item_dbkey);
            var btn = document.getElementsByClassName("bootbox-close-button")[1];
            btn.click();
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);


        }
    });
}



function CloneSplitDocumentRow() {
    var clonedRow = $('#ForgingSplitDocumentsTable tbody tr:first').clone(); // Clone the last row
    clonedRow.show();
    $('#ForgingSplitDocumentsTable').append(clonedRow);
}
function DeleteUnsavedDocs(btn) {
    $(btn).closest('tr').remove();
}


function UpdateDocumentType(Attachment_Db_Key, dropdown) {
    var selectedOption = dropdown.options[dropdown.selectedIndex];
    var selectedValue = selectedOption.value;
    //console.log(selectedValue);
    //console.log(Attachment_Db_Key);
    $.ajax({
        type: 'GET',
        url: '/MaterialIssue/UpdateForgeReceiptDocType?ForgeID=' + Attachment_Db_Key + '&doctype=' + selectedValue,
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
}

function getIssueDetails() {
  //  $('#loading').show();
    $.ajax({
        type: 'GET',
        url: '/MaterialIssue/MaterialIssueDetails',
        success: function (data) {
           console.log(data);
            forgingData = data;
            setDatatables(forgingData);
        },
        error: function (error) {
          //  console.log(error);
        }
    });
  //  $('#loading').hide();
}

function setDatatables(forgingReceipts) {
    try {
        if ($.fn.DataTable.isDataTable('#MItable')) {
            $('#MItable').DataTable().destroy();
        }
        $('#MItable tbody').empty();
    } catch (e) { }

    var table = $('#MItable').DataTable({
        data: forgingReceipts,
        paging: false,
        searching: true,
        ordering: false,
        info: true,
        responsive: false,
        autoWidth: false,
        scrollY: 550,
        scrollCollapse: true,
        dom: 'Bfrtip',
        buttons: [{
            extend: 'excel',
            title: 'Material Issue Items',
            exportOptions: {
                columns: [1, 2, 3, 4, 5, 6, 7]
            }
        }],
        columns: [
            {
                data: "order_Ref_No",
                visible: false,
                searchable: false
            },
            { data: "drawing_no" },
            { data: "qty" },
            { data: "material_name" },
            { data: "size" },
            { data: "weight" },
            { data: "job_Card" },
            {
                data: null,
                searchable: true,
                orderable: false,
                render: function (data, type, row) {
                    var fileName = row.jcFileName || row.JCFileName || "";
                    var fileLocation = row.jobCardFileLocation || row.JCFileLocation || "";

                    if (type === 'filter' || type === 'sort') {
                        return fileName;
                    }

                    if (fileLocation && fileName && fileLocation !== '-' && fileName !== '-') {
                        return '<a class="badge badge-phoenix badge-phoenix-success fs--2" target="_blank" style="cursor:pointer" href="' + fileLocation + '">' + fileName + '</a>';
                    }

                    return "-";
                }
            }
        ],
        columnDefs: [
            { targets: 0, visible: false, searchable: false }
        ],
        initComplete: function () {
            $('#filterPartName, #filterQty, #filterRMName, #filterSize, #filterWeight, #filterJobCard, #filterJobCardDoc')
                .off('keyup change');

            $('#filterPartName').on('keyup change', function () {
                table.column(1).search(this.value).draw();
            });

            $('#filterQty').on('keyup change', function () {
                table.column(2).search(this.value).draw();
            });

            $('#filterRMName').on('keyup change', function () {
                table.column(3).search(this.value).draw();
            });

            $('#filterSize').on('keyup change', function () {
                table.column(4).search(this.value).draw();
            });

            $('#filterWeight').on('keyup change', function () {
                table.column(5).search(this.value).draw();
            });

            $('#filterJobCard').on('keyup change', function () {
                table.column(6).search(this.value).draw();
            });

            $('#filterJobCardDoc').on('keyup change', function () {
                table.column(7).search(this.value).draw();
            });
        }
    });
}

//function setDatatables(forgingReceipts) {
//    try {
//        dataTable.destroy();
//    } catch (e) {
//    }
//    var dataTable;
//    var groupColumn = 0;
//    datatable = $('#MItable').DataTable({
//        "Sort": false,
//        data: forgingReceipts,
//        responsive: true,
//        paging: false,
//        //stateSave: true,
//        //stateSaveCallback: function (settings, data) {
//        //    localStorage.setItem(
//        //        'DataTables_' + settings.sInstance,
//        //        JSON.stringify(data)
//        //    );
//        //},
//        //stateLoadCallback: function (settings) {
//        //    return JSON.parse(localStorage.getItem('DataTables_' + settings.sInstance));
//        //},
//        /*  stateSave: true,*/
//        dom: 'Bfrtip',
//        scrollY:550,
//        buttons: [{
//            extend: 'excel',
//            title: 'Material Issue Items',
//            exportOptions: {
//                columns: [0, 1, 2, 3, 4, 5,6,7] //Your Colume value those you want
//            }
//        }],
       
//        "columns": [
//            {
//                "data": "order_Ref_No",
//                //"render": function (a, b, data, d) {
//                //    return '<label>' + data.Order_Ref_No + '</label>' + '<input type="hidden" value="' + data.Issue_Dbkey + '"/>' + ' <input type="hidden" value="JOBCARD"/>';
//                //}
//            },
//            { "data": "drawing_no" },
//            { "data": "qty" },
//            { "data": "material_name" },
//            {"data":"size"},
//            { "data": "weight" },
//           // { "data": "vendor_Name" },
//      /*      { "data": "runningBalnce" },*/
//            { "data": "job_Card" },
//            {
//                "data": null,
//                render: function (a, b, data, d) {
//                    if (data.jobCardFileLocation != null && data.jcFileName != null && data.JCFileLocation != '-' && data.jcFileName != '-')
//                        return '<a class="badge badge-phoenix badge-phoenix-success fs--2" target="_blank" style="cursor:pointer" href="' + data.jobCardFileLocation + '" >' + data.jcFileName + '</a>'
//                    else
//                        return "-"
//                }
//            },
//           /* { "data":null}*/
//        ],

//        //"columnDefs": [
//        //    { "visible": false, "targets": groupColumn },
//        //    {
//        //        "targets": [-1], render: function (a, b, data, d) {
//        //            if(data.jobCardFileLocation != null && data.jcFileName != null)
//        //                return '<a class="badge badge-phoenix badge-phoenix-success fs--2" target="_blank" style="cursor:pointer" href="' + data.jobCardFileLocation + '" >' + data.jcFileName + '</a>'
//        //            else
//        //            return "-"
//        //        }
//        //    },
//        //    //{
//        //    //    "targets": [-1], render: function (a, b, data, d) {
//        //    //        return '<a style="cursor:pointer;color:#61affe" onclick = "GetSplitItemMapping('+data.issue_Item_Dbkey+',1)">Split</a>'
//        //    //    }
//        //    //}
//        //],
//        //"order": [
//        //    ['order_Ref_No','asc']
//        //],
//        rowGroup: {
//            dataSrc: 'order_Ref_No'
//        },
//        columnDefs: [
//            {
//                targets: [0],
//                visible: false
//            }
//        ]
//        //"drawCallback": function (settings) {
//        //    var api = this.api();
//        //    var rows = api.rows({ page: 'current' }).nodes();
//        //    var last = null;
//        //    //console.log(rows);
//        //    api.column(groupColumn, { page: 'current' }).data().each(function (group, i) {
//        //        /*  console.log($(rows).eq(i).find("td:eq(0)").children[1].val());*/
//        //        //var dbkey = rows[i].children[0].children[1].value;
//        //        //console.log(dbkey);
//        //        //var jobcard = rows[i].children[0].children[2].value;
//        //        //var jobcardloc = rows[i].children[0].children[3].value;

//        //        if (last !== group) {
//        //            // $(rows).eq(i).before('<tr class="group"><td style="color:#61affe">' + group + '</td> <td><a href="/Procurement/Material/Issuenew/' + dbkey + '"  class="btn btn-primary">Edit</a></td> <td ><a target="_blank" href="/Procurement/Material/PrintMaterialIssue/' + dbkey + '"  class="btn btn-primary">Print</a></td> <td><button class="btn btn-danger" onclick="DeleteMaterialIssue(' + dbkey + ')">Delete</button></td> <td colspan="3"><a class="btn btn-warning" target="_blank" href="' + jobcardloc+'">JC:'+jobcard+'</a></td></tr>');
//        //            $(rows).eq(i).before('<tr class="group"><td colspan="8" style="color:#61affe" ><b>'+ group+'</b></tr>');
//        //            last = group;
//        //        }

//        //    });
//       // }
//    });
//}

//function SaveSplitItemModel(Receipt_dbkey) {
//    var form = $('#ForgingSplitModelData');
//    $.validator.unobtrusive.parse("#" + form.attr("id"));
//    $(form).validate();
//    console.log($(form).serialize());
//    if (form.valid() == false) {
//        return false;
//    }
//    var formData = new FormData();

//    var formElements = $(form).serializeArray();
//    var jsonResult = {};

//    $.each(formElements, function (index, element) {
//        jsonResult[element.name] = element.value;
//    });

//    //var jsonData = JSON.stringify(jsonResult);

//    formData.append('jsonData', JSON.stringify(jsonResult));

//    var splitUploadFiles = document.getElementsByClassName("splitUploadFiles");

//    //for (var i = 0; i < fileInput.files.length; i++) {
//    //    formData.append('files', fileInput.files[i]);
//    //}

//    var fileData = [];

//    for (var i = 0; i < splitUploadFiles.length; i++) {
//        if (splitUploadFiles[i].files.length > 0) {
//            var fileItem = {};
//            var fileControl = splitUploadFiles[i];
//            var fileRow = $(fileControl).closest('tr');
//            fileItem.Source_table_key = document.getElementById("modelItem_Receipt_dbkey").Value;
//            fileItem.Source_table = "Forging_Receipt_Items";
//            fileItem.File_DVD_Num = $(fileControl).closest('tr').find('td:first-child').children().first().val(); //fileRow.cells[1].children[0].value;
//            fileItem.File_Revision = $(fileControl).closest('tr').find('td:nth-child(2)').children().first().val();
//            fileData.push(fileItem);
//            formData.append('files', splitUploadFiles[i].files[0]);
//        }
//    }
//    console.log(JSON.stringify(fileData));
//    formData.append('filesData', JSON.stringify(fileData));
//    console.log(fileData);
//    console.log(formData);
//    // data: { Procurement_Demand_Items_Split: JSON.stringify(Procurement_Demand_Items_Split) },
//    $.ajax({
//        url: '/MaterialIssue/SaveForgingReceiptSplitModel',
//        type: 'POST',
//        data: formData,
//        contentType: false,
//        processData: false,
//        success: function (response) {
//            alert('Submitted Successfully');
//            GetViewDemandItemReciptSplit(Receipt_dbkey);
//            var btn = document.getElementsByClassName("bootbox-close-button")[1];
//            btn.click();
//        },
//        error: function (xhr, status, error) {
//            console.error('Error:', error);


//        }
//    });
//}


var groupColumn = 0;
function GetForgingReceipts() {
    try {
        dataTable.destroy();
    } catch (e) {
    }


    dataTable = $("#MatIssueHis").DataTable({
        "ajax": {
            "url": "/MaterialIssue/GetReceiptsHistory",
            "type": "GET",
            "dataSrc": "",
            "datatype": "json"
        },
        responsive: true,
        paging: false,
        "order": [],
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
        dom: 'Bfrtip',
        buttons: [{
            extend: 'excel',
            title: 'Forging Receipts',
            exportOptions: {
                columns: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9] //Your Colume value those you want
            }
        }],
        "columns": [

            {
                "data": "Receipt_Number",
                render: function (a, b, data, d) {
                    return '<label>' + data.Receipt_Number + '</label>' + '<input type="hidden" value="' + data.MMG_File_No + '"/>' + '<input type="hidden" value="' + data.forging_recp_dbkey + '"/>';
                }
            },

            //{
            //    "data": "Receipt_Date", render: function (data, type, row) {
            //        if (type === 'display' || type === 'filter') {
            //            var rowvalue = row["Receipt_Date"];
            //            return (moment(data).format("DD/MM/YYYY"));
            //        }

            //        return data;
            //    }
            //},
            {
                "data": "Draw_part_no",
                render: function (a, b, data, d) {
                    return '<label>' + data.Draw_part_no + '</label>' +
                        '<input type="hidden" value="' + data.MMG_File_No + '"/>' +
                        '<input type="hidden" value="' + data.Receipt_Number + '"/>' +
                        '<input type="hidden" value="' + data.Receipt_Date + '"/>' +
                        '<input type="hidden" value="' + data.forging_recp_dbkey + '"/>';
                }
            },
            { "data": "Material_name" },
            { "data": "GTRE_Drawing_No" },
            { "data": "HAL_Drawing_No" },
            { "data": "Total_Qty" },
            { "data": "Receiving_Inventory" },
            { "data": "Balance" },
            {
                "data": "forging_recp_dbkey",
                render: function (a, b, data, d) {
                    return '<button onclick="GetDetailSplit(' + data.forging_item_dbkey + ')" class="btn btn-sm btn-success">Splits</button>'
                    /* '<button type="button" class="btn btn-success" onclick="GetDetailSpli(' + data.Issue_Dbkey + ')"><span class="btn-label right">1,307</span>Detailed Split</button>'*/
                }
            },
            {
                "data": "forging_recp_dbkey",
                render: function (a, b, data, d) {
                    return '<button onclick="GetUploadDocsModel(' + data.forging_item_dbkey + ')" class="btn btn-sm btn-info">Docs</button>'
                    /* '<button type="button" class="btn btn-success" onclick="GetDetailSpli(' + data.Issue_Dbkey + ')"><span class="btn-label right">1,307</span>Detailed Split</button>'*/
                }
            }
            ,
            //{
            //    "data": "forging_recp_dbkey",
            //    render: function (a, b, data, d) {
            //        return '<a href="/Procurement/Forging/Receipts?recp_dbkey=' + data.forging_recp_dbkey + '"  class="btn btn-sm btn-warning writeaccess">Edit</a>'
            //    }
            //}
            //,
            //{
            //    "data": "forging_recp_dbkey",
            //    render: function (a, b, data, d) {
            //        return '<button class="btn btn-sm btn-danger writeaccess" onclick="DeleteForgingReceipts(' + data.forging_recp_dbkey + ')">Delete</button>'
            //    }
            //},



        ],

        drawCallback: function (settings) {

            var api = this.api();

            var tableRows = api.rows({ page: 'current' }).nodes();


            var groupName = null;
            var lastGroup = null;
            var lastSub = null;
            var SubGroup = null;
            var dbkey = null;


            $(tableRows).each(function () {

                groupName = this.cells[1].children[1].value;
                SubGroup = this.cells[1].children[2].value;
                minSubGroup = (moment(this.cells[1].children[3].value).format("DD/MM/YYYY"));
                dbkey = this.cells[1].children[4].value;


                if (lastGroup != groupName) {
                    $(this).before('<tr class="group"><td style="color:#61affe" colspan="12">MMG File No : ' + groupName + '</td> </tr>');
                    lastGroup = groupName;
                }

                if (lastSub != SubGroup) {
                    /* $(this).before('<tr class="subgroup" style="color:red"><td colspan="2"> Receipt No : ' + SubGroup + '</td><td colspan="2"> Receipt Date : ' + minSubGroup + '</td><td colspan="1" ><a href="/Procurement/Forging/Receipts?recp_dbkey=' + dbkey + '"  class="btn btn-warning">Edit</a></td><td><button class="btn btn-danger" onclick="DeleteForgingReceipts(' + dbkey +')">Delete</button></td></tr>');*/
                    $(this).before('<tr class="subgroup" style="color:red"><td colspan="2"> Receipt No : ' + SubGroup + '</td><td colspan="8"> Receipt Date : ' + minSubGroup + '</td> </tr>');
                    lastSub = SubGroup;
                }

            });
            
        },
    });

    $('.DT-search .form-control').attr('placeholder', 'Search...');
    $('.DT-search .form-control').attr('class', 'DT-search form-control');
    $('.DT-lf-right').attr('class', 'pull-left');
    $('.dataTables_length').attr('class', 'dataTables_length DT-lf-right');

}


function UploadMaterialIssueDocument(MaterialIssueDBKey,ViewType) {
    var urlinput = "/MaterialIssue/MaterialIssueDocument?id=" + MaterialIssueDBKey + '&Type=' + ViewType;
    $.get(urlinput).done(function (response) {
        //bootbox.dialog({
        //    title: "Material Issue document",
        //    message: response,
        //    size: 'large',
        //    closeButton: true,
        //    // AttachmentDoc(Receipt_dbkey);
        //});
        document.getElementById('MaterialIssueDocumentsDiv').innerHTML = response;
       
    });
}

function CloneMaterialIssueDocumentRow() {
    var clonedRow = $('#MaterialIssueDocumentTable tbody tr:first').clone(); // Clone the last row
    clonedRow.find('input').val(''); // Clear input values if needed
    clonedRow.show();
    $('#MaterialIssueDocumentTable').append(clonedRow);

}



function SaveMaterialIssueDocuments(MaterialIssueDBkey) {
    var formData = new FormData();

    var MaterailIssueUploadFiles = document.getElementsByClassName("MaterailIssueDocUploadFiles");
   
    var fileData = [];

    for (var i = 0; i < MaterailIssueUploadFiles.length; i++) {
        if (MaterailIssueUploadFiles[i].files.length > 0) {
           
            var fileItem = {};
            fileItem.Source_table_key = MaterialIssueDBkey;
            fileItem.Source_table = "Material_Issue_Note";
            fileItem.File_DVD_Num = document.getElementsByClassName("fileAttachmentType")[i].value;
            fileItem.File_Revision = document.getElementsByClassName("MaterialIssueUploadfileRefNum")[i].value;
            fileData.push(fileItem);
            formData.append('files', MaterailIssueUploadFiles[i].files[0]);
        }
    }
    formData.append('filesData', JSON.stringify(fileData));

    $.ajax({
        url: "/MaterialIssue/SaveMaterialIssueDocument",
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            if (response.success) {
                bootbox.alert('Submitted Successfully', function () {
                    UploadMaterialIssueDocument(MaterialIssueDBkey, 'Edit');
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

function DelMaterialIssueDocData(id,issueDbkey) {
    bootbox.confirm("Are you sure you want to delete this Material Issue Document ", function (result) {
        if (result) {
            $.ajax({
                type: 'GET',
                url: '/MaterialIssue/DeleteMaterialIssueDocument?documentId=' + id ,
                success: function (data) {
                    if (data.success) {
                        UploadMaterialIssueDocument(issueDbkey, 'Edit');
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

function DeleteMaterialIssue(IssueDbkey) {
    bootbox.confirm("Are you sure you want to delete this Material Issue ", function (result) {
        if (result) {
            $.ajax({
                type: 'GET',
                url: '/MaterialIssue/DeleteMaterialIssue?IssueDbkey=' + IssueDbkey,
                success: function (data) {
                    if (data.success) {
                        bootbox.alert('Deleted Successfully');
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

function viewMaterialIssueDocuments(MaterialIssueDBKey, ViewType) {
    var urlinput = "/MaterialIssue/MaterialIssueDocument?id=" + MaterialIssueDBKey + '&Type=' + ViewType;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Material Issue document",
            message: response,
            size: 'large',
            closeButton: true,
            // AttachmentDoc(Receipt_dbkey);
        });
       

    });
}

    
function getBATLIssueSummary() {
    var urlinput = "/MaterialIssue/BATL_IssuesSummary" ;
    $.get(urlinput).done(function (response) {
        document.getElementById("tab-BATLSummary").innerHTML = response;
        BATLIssueTbl = $('#BATLIssueSummaryTbl').DataTable({ 
            paging: false, 
           
            scrollX: true,
            scrollCollapse: true,
            autoWidth: false,
            responsive: false,
            scrollY: 500,
            columnDefs: [
                {
                    targets: 0,       // First column
                    width: '20%',
                },
                {
                    targets: 2,       // First column
                    width: '20%',
                },
                {
                    targets: 6,       // First column
                    width: '15%',
                }
            ]
        });
      //  BATLIssueTbl.columns.adjust().draw(); 
            $('#BATLIssueSummaryTbl').DataTable().columns.adjust();
         
    });
}
function initPartsTomSelect(selectElement, preSelectedItems) {
    // preSelectedItems is an array of { value: ..., text: ... } for edit mode
    if (selectElement.tomselect) {
        return selectElement.tomselect;
    }
    return new TomSelect(selectElement, {
        plugins: ['remove_button'],
        valueField: 'value',
        labelField: 'text',
        searchField: 'text',
        sortField: {
            field: "text",
            direction: "asc"
        },
        maxOptions: 20,
        // Pre-populate with existing selections (for edit mode)
        options: preSelectedItems || [],
        items: (preSelectedItems || []).map(function (item) { return item.value; }),
        load: function (query, callback) {
            if (query.length < 2) return callback(); // min 2 chars to search
            fetch('/MaterialIssue/SearchParts?term=' + encodeURIComponent(query))
                .then(function (response) { return response.json(); })
                .then(function (data) { callback(data); })
                .catch(function () { callback(); });
        },
        // Don't load on focus with empty query - saves unnecessary calls
        shouldLoad: function (query) {
            return query.length >= 2;
        },
        placeholder: 'Type 2+ chars to search parts...',
        render: {
            no_results: function (data, escape) {
                return '<div class="no-results">No parts found for "' + escape(data.input) + '"</div>';
            },
            loading: function () {
                return '<div class="spinner-border spinner-border-sm" role="status"><span class="visually-hidden">Loading...</span></div>';
            }
        }
    });
}


function initPartsDropdownsForRows(rows) {
    var partsLookup = window._partsLookupCache || {};

    rows.forEach(function (row) {
        var select = row.querySelector('.Drawing_no');
        if (select && !select.tomselect) {
            var hdnField = row.querySelector('.hdnPartNumberKey');
            if (hdnField) {
                var selectedKeys = JSON.parse(hdnField.value);
                var preSelected = selectedKeys
                    .filter(function (k) { return partsLookup[k]; })
                    .map(function (k) { return partsLookup[k]; });
                initPartsTomSelect(select, preSelected);
            }

            // Also populate raw material dropdown if not already done
            var rmSelect = row.querySelector('.Raw_material_Dbkey');
            if (rmSelect && rmSelect.options.length <= 1) {
                var rmHdn = row.querySelector('.hdnRawMaterialKey');
                rawMaterialListJson.forEach(function (option) {
                    var opt = document.createElement('option');
                    opt.value = option.value;
                    opt.text = option.text;
                    rmSelect.appendChild(opt);
                });
                if (rmHdn) rmSelect.value = rmHdn.value;
            }
        }
    });
}

// ============================================
// Smart Inline Search System (No TomSelect)
// ============================================

var searchDebounceTimer = null;

// Close search results and clear input for a single cell
function closeSearchResults(td) {
    var inlineSearch = td.querySelector('.inline-search');
    if (!inlineSearch) return;
    var results = inlineSearch.querySelector('.search-results');
    if (results) { results.innerHTML = ''; results.style.display = 'none'; }
    var input = inlineSearch.querySelector('.search-input');
    if (input) input.value = '';
}

// Global: click anywhere outside an inline-search closes all open search results
document.addEventListener('click', function (e) {
    // If click is inside an inline-search or on a search icon button, let it be
    if (e.target.closest('.inline-search') || e.target.closest('.btn-edit-cell')) return;

    // Close search results in every cell
    document.querySelectorAll('.inline-search').forEach(function (el) {
        var td = el.closest('td');
        if (td) closeSearchResults(td);
    });
});

// Open inline search for a cell
function openInlineSearch(editBtn, type) {
    var td = editBtn.closest('td');
    var row = td.closest('tr');

    // Close search results in all other cells in the same row
    if (row) {
        row.querySelectorAll('td').forEach(function (otherTd) {
            if (otherTd === td) return;
            closeSearchResults(otherTd);
        });
    }

    var displayDiv = td.querySelector('.smart-display');
    var searchDiv = td.querySelector('.inline-search');
    var searchInput = searchDiv.querySelector('.search-input');

    displayDiv.style.display = 'none';
    searchDiv.style.display = 'block';
    searchInput.value = '';
    searchInput.focus();

    // Clear previous results
    var resultsDiv = searchDiv.querySelector('.search-results');
    resultsDiv.innerHTML = '';
    resultsDiv.style.display = 'none';

    // For parts: show currently selected items as removable badges
    if (type === 'parts') {
        var selectedDiv = searchDiv.querySelector('.selected-items');
        if (selectedDiv) {
            selectedDiv.innerHTML = '';
            var hdnKeys = td.querySelector('.hdnPartNumberKey').value;
            var hdnNames = td.querySelector('.hdnPartNames').value;
            if (hdnKeys && hdnKeys.trim() !== '') {
                var keys = hdnKeys.split(',');
                var names = hdnNames.split(',');
                for (var i = 0; i < keys.length; i++) {
                    var name = (names[i] || '').trim();
                    var key = keys[i].trim();
                    if (key) {
                        addSelectedBadge(selectedDiv, key, name, 'parts');
                    }
                }
            }
        }
    }

    // Attach search event
    searchInput.onkeyup = function () {
        clearTimeout(searchDebounceTimer);
        var query = this.value.trim();
        var minChars = (type === 'vendor') ? 1 : 2;
        if (query.length < minChars) {
            resultsDiv.style.display = 'none';
            return;
        }
        searchDebounceTimer = setTimeout(function () {
            performSearch(td, query, type, resultsDiv);
        }, 300);
    };
}

// Perform AJAX search based on type
function performSearch(td, query, type, resultsDiv) {
    var url = '';
    if (type === 'parts') {
        url = '/MaterialIssue/SearchParts?term=' + encodeURIComponent(query);
    } else if (type === 'rawmaterial') {
        url = '/MaterialIssue/SearchRawMaterials?term=' + encodeURIComponent(query);
    } else if (type === 'vendor') {
        // Smart: pass raw material key to filter vendors
        var row = td.closest('tr');
        var rmKey = row.querySelector('.hdnRawMaterialKey').value || '0';
        url = '/MaterialIssue/SearchVendors?term=' + encodeURIComponent(query) + '&rawMaterialDbKey=' + rmKey;
    }

    resultsDiv.innerHTML = '<div style="padding:5px;color:#666;">Searching...</div>';
    resultsDiv.style.display = 'block';

    fetch(url)
        .then(function (r) { return r.json(); })
        .then(function (data) {
            resultsDiv.innerHTML = '';
            if (data.length === 0) {
                resultsDiv.innerHTML = '<div style="padding:5px;color:#999;">No results found</div>';
                return;
            }
            data.forEach(function (item) {
                var div = document.createElement('div');
                div.style.cssText = 'padding:4px 8px;cursor:pointer;border-bottom:1px solid #eee;font-size:0.8rem;';
                div.textContent = item.text;
                div.setAttribute('data-value', item.value);
                div.setAttribute('data-text', item.text);
                div.onmouseover = function () { this.style.backgroundColor = '#e9ecef'; };
                div.onmouseout = function () { this.style.backgroundColor = ''; };
                div.onclick = function () {
                    selectSearchResult(td, item.value, item.text, type);
                };
                resultsDiv.appendChild(div);
            });
        })
        .catch(function () {
            resultsDiv.innerHTML = '<div style="padding:5px;color:red;">Search failed</div>';
        });
}

// Handle selection from search results
function selectSearchResult(td, value, text, type) {
    var searchDiv = td.querySelector('.inline-search');
    var resultsDiv = searchDiv.querySelector('.search-results');
    var searchInput = searchDiv.querySelector('.search-input');

    if (type === 'parts') {
        // Multi-select: add badge, keep search open
        var selectedDiv = searchDiv.querySelector('.selected-items');
        // Check if already selected
        if (selectedDiv.querySelector('[data-key="' + value + '"]')) return;
        addSelectedBadge(selectedDiv, value, text, 'parts');
        searchInput.value = '';
        resultsDiv.style.display = 'none';
        searchInput.focus();

        // Auto-fill raw material, qty and vendor (only if fields are empty)
        var row = td.closest('tr');
        if (row && (row.classList.contains('new-row') || row.getAttribute('data-mode') === 'editing')) {
            triggerSmartRMFill(row, value);
        }
    } else {
        // Single-select: for RM and Vendor, update badge inline
        if (type === 'rawmaterial') {
            td.querySelector('.hdnRawMaterialKey').value = value;
            var currentDiv = td.querySelector('.inline-search .current-selection');
            if (!currentDiv) {
                currentDiv = document.createElement('div');
                currentDiv.className = 'current-selection mb-1';
                searchDiv.insertBefore(currentDiv, searchDiv.firstChild);
            }
            currentDiv.innerHTML = '<span class="badge mb-1" style="font-size:0.75rem;background-color:#e9ecef;color:#495057;border:1px solid #ced4da;">' + escapeHtml(text) + ' <a style="color:#dc3545;cursor:pointer;margin-left:3px;font-weight:bold;" onclick="clearRawMaterial(this)">&times;</a></span>';
            // Clear search
            searchDiv.querySelector('.search-input').value = '';
            resultsDiv.style.display = 'none';
            // Smart chain: trigger vendor fill
            triggerSmartVendorFill(td.closest('tr'), value);
            // Load Heat No / Batch No options for selected raw material
            loadHeatBatchOptions(td.closest('tr'), value);
        } else if (type === 'vendor') {
            td.querySelector('.hdnVendorKey').value = value;
            var currentDiv = td.querySelector('.inline-search .current-selection');
            if (!currentDiv) {
                currentDiv = document.createElement('div');
                currentDiv.className = 'current-selection mb-1';
                searchDiv.insertBefore(currentDiv, searchDiv.firstChild);
            }
            currentDiv.innerHTML = '<span class="badge mb-1" style="font-size:0.75rem;background-color:#e9ecef;color:#495057;border:1px solid #ced4da;">' + escapeHtml(text) + ' <a style="color:#dc3545;cursor:pointer;margin-left:3px;font-weight:bold;" onclick="clearVendor(this)">&times;</a></span>';
            searchDiv.querySelector('.search-input').value = '';
            resultsDiv.style.display = 'none';
        }
    }
}

// Sync part badges back to hidden inputs (hdnPartNumberKey & hdnPartNames)
function syncPartKeysFromBadges(td) {
    var badges = td.querySelectorAll('.selected-items .badge[data-key]');
    var keys = [];
    var names = [];
    badges.forEach(function (b) {
        keys.push(b.getAttribute('data-key'));
        // badge text minus the × button
        var text = b.childNodes[0] ? b.childNodes[0].textContent.trim() : '';
        if (text) names.push(text);
    });
    td.querySelector('.hdnPartNumberKey').value = keys.join(',');
    td.querySelector('.hdnPartNames').value = names.join(',');
}

// Add a removable badge for multi-select (parts)
function addSelectedBadge(container, key, text, type) {
    var badge = document.createElement('span');
    badge.className = 'badge mb-1 me-1';
    badge.setAttribute('data-key', key);
    badge.style.cssText = 'font-size:0.75rem;background-color:#e9ecef;color:#495057;border:1px solid #ced4da;cursor:default;';
    badge.innerHTML = escapeHtml(text) + ' <a style="color:#dc3545;cursor:pointer;margin-left:3px;font-weight:bold;" onclick="removeSelectedBadge(this)">&times;</a>';
    container.appendChild(badge);

    // Sync keys to hidden input
    var td = container.closest('td');
    if (td) syncPartKeysFromBadges(td);
}

// Remove a selected badge
function removeSelectedBadge(closeBtn) {
    var badge = closeBtn.closest('.badge');
    var td = badge.closest('td');
    badge.remove();

    // Sync keys to hidden input after removal
    if (td) syncPartKeysFromBadges(td);
}
 

// ============================================
// Smart Auto-Fill Chain
// ============================================

// When parts are selected, auto-fill Raw Material, Qty and Vendor (only for new rows, only if empty)
function triggerSmartRMFill(row, partKey) {
    fetch('/MaterialIssue/GetPartSmartData?partDbKey=' + partKey)
        .then(function (r) { return r.json(); })
        .then(function (data) {
            if (!data.part) return;

            var isNewRow = row.classList.contains('new-row');

            // --- Raw Material auto-fill (only if currently empty) ---
            var rmHdn = row.querySelector('.hdnRawMaterialKey');
            if (data.part.Raw_material_Dbkey && (!rmHdn.value || rmHdn.value === '' || rmHdn.value === '0')) {
                var rmKey = data.part.Raw_material_Dbkey;
                var rmName = data.part.Raw_material_Name || data.part.Material_name || '';

                rmHdn.value = rmKey;

                // Update edit-mode: create or update current-selection badge
                var rmInlineSearch = row.querySelector('.cell-rawmaterial .inline-search');
                if (rmInlineSearch) {
                    var rmCurrentDiv = rmInlineSearch.querySelector('.current-selection');
                    if (!rmCurrentDiv) {
                        rmCurrentDiv = document.createElement('div');
                        rmCurrentDiv.className = 'current-selection mb-1';
                        rmInlineSearch.insertBefore(rmCurrentDiv, rmInlineSearch.firstChild);
                    }
                    rmCurrentDiv.innerHTML = '<span class="badge mb-1" style="font-size:0.75rem;background-color:#e9ecef;color:#495057;border:1px solid #ced4da;">' + escapeHtml(rmName) + ' <a style="color:#dc3545;cursor:pointer;margin-left:3px;font-weight:bold;" onclick="clearRawMaterial(this)">&times;</a></span>';
                }

                // Smart vendor fill (only if vendor is empty)
                var vHdn = row.querySelector('.hdnVendorKey');
                if (!vHdn.value || vHdn.value === '' || vHdn.value === '0') {
                    if (data.vendors && data.vendors.length === 1) {
                        var vKey = data.vendors[0].Vendor_Dbkey;
                        var vName = data.vendors[0].Vendor_Name;
                        vHdn.value = vKey;

                        var vInlineSearch = row.querySelector('.cell-vendor .inline-search');
                        if (vInlineSearch) {
                            var vCurrentDiv = vInlineSearch.querySelector('.current-selection');
                            if (!vCurrentDiv) {
                                vCurrentDiv = document.createElement('div');
                                vCurrentDiv.className = 'current-selection mb-1';
                                vInlineSearch.insertBefore(vCurrentDiv, vInlineSearch.firstChild);
                            }
                            vCurrentDiv.innerHTML = '<span class="badge mb-1" style="font-size:0.75rem;background-color:#e9ecef;color:#495057;border:1px solid #ced4da;">' + escapeHtml(vName) + ' <a style="color:#dc3545;cursor:pointer;margin-left:3px;font-weight:bold;" onclick="clearVendor(this)">&times;</a></span>';
                        }
                    }
                }
            }

            // --- Raw Material Qty & Weight auto-fill (only if empty) ---
            var isEditingRow = row.getAttribute('data-mode') === 'editing';
            if (isNewRow || isEditingRow) {
                var partsQty = parseFloat(row.querySelector('.Qty')?.value) || 0;

                // Store per-part values on the row for recalculation when Parts Qty changes
                if (data.rmQtyPerPart) row.setAttribute('data-rm-qty-per-part', data.rmQtyPerPart);
                if (data.rmWeightPerPart) row.setAttribute('data-rm-weight-per-part', data.rmWeightPerPart);

                // Store qty per engine and show engine-wise toggle if available
                if (data.qtyPerEngine) {
                    row.setAttribute('data-qty-per-engine', data.qtyPerEngine);
                    var engineToggle = row.querySelector('.engine-toggle');
                    if (engineToggle) engineToggle.style.display = '';
                    var hintSpan = row.querySelector('.qty-per-engine-hint');
                    if (hintSpan) hintSpan.textContent = '× ' + data.qtyPerEngine + ' qty/eng';
                }

                // Auto-fill Raw Material Qty (pieces: e.g. 10 bars, 2 sheets)
                if (data.rmQtyPerPart) {
                    var qtyIssueInput = row.querySelector('.Qty_Issue');
                    if (qtyIssueInput && (!qtyIssueInput.value || qtyIssueInput.value.trim() === '' || qtyIssueInput.value === '0')) {
                        qtyIssueInput.value = partsQty > 0 ? partsQty * data.rmQtyPerPart : data.rmQtyPerPart;
                    }
                }

                // Auto-fill Raw Material Weight (kg)
                if (data.rmWeightPerPart) {
                    var weightInput = row.querySelector('.Weight_Kg');
                    if (weightInput && (!weightInput.value || weightInput.value.trim() === '' || weightInput.value === '0')) {
                        weightInput.value = partsQty > 0 ? partsQty * data.rmWeightPerPart : data.rmWeightPerPart;
                    }
                }
            }

            // Load Heat No / Batch No dropdown options for the current raw material
            var currentRmKey = row.querySelector('.hdnRawMaterialKey')?.value || '';
            if (currentRmKey && currentRmKey !== '' && currentRmKey !== '0') {
                loadHeatBatchOptions(row, currentRmKey);
            }
        })
        .catch(function () { });
}
// When RM is manually changed, refresh vendor suggestions
function triggerSmartVendorFill(row, rmKey) {
    fetch('/MaterialIssue/SearchVendors?term=&rawMaterialDbKey=' + rmKey)
        .then(function (r) { return r.json(); })
        .then(function (vendors) {
            var vTd = row.querySelector('.cell-vendor');
            if (vendors.length === 1) {
                vTd.querySelector('.hdnVendorKey').value = vendors[0].value;
                var vBadges = vTd.querySelector('.vendor-badges');
                vBadges.innerHTML = '<span class="badge mb-1" style="font-size:0.75rem;background-color:#e9ecef;color:#495057;border:1px solid #ced4da;">' + escapeHtml(vendors[0].text) + '</span>';
            } else if (vendors.length > 1) {
                var vBadges = vTd.querySelector('.vendor-badges');
                vBadges.innerHTML = '<span class="text-muted" style="font-size:0.75rem;">' + vendors.length + ' vendors available — click ✏️</span>';
            }
        })
        .catch(function () { });
}

// ============================================
// Engine-Wise Qty
// ============================================
 
// Toggle the engine-wise qty section
function toggleEngineQty(link) {
    var td = link.closest('.cell-qty');
    var section = td.querySelector('.engine-qty-section');

    if (section.style.display === 'none') {
        section.style.display = '';
        link.textContent = 'Close';   

        var engineInput = section.querySelector('.EngineCount');
        engineInput.focus();
    } else {
        section.style.display = 'none';
        link.textContent = '× By Engine';    

        // Clear engine count and hidden field when switching back
        section.querySelector('.EngineCount').value = '';
        td.querySelector('.PartQty_EngineWise').value = '';
    }
}

// Recalculate Parts Qty (and downstream RM Qty/Weight) from engine count
function recalcFromEngines(row) {
    var engineCount = parseFloat(row.querySelector('.EngineCount')?.value) || 0; 
    var engineEl = row.querySelector('.EngineCount');
    if (!isNaN(engineCount) && engineCount < 0) {
        engineEl.value = ''; // remove the -ve number user typed  
        return;
    }
    var qtyPerEngine = parseFloat(row.getAttribute('data-qty-per-engine')) || 0;
    if (!qtyPerEngine) return;

    var partsQty = engineCount * qtyPerEngine;
    row.querySelector('.Qty').value = partsQty;
    row.querySelector('.PartQty_EngineWise').value = engineCount || '';

    // Recalculate RM Qty
    var rmQtyPerPart = parseFloat(row.getAttribute('data-rm-qty-per-part'));
    if (rmQtyPerPart) {
        row.querySelector('.Qty_Issue').value = partsQty * rmQtyPerPart;
    }

    // Recalculate RM Weight
    var rmWeightPerPart = parseFloat(row.getAttribute('data-rm-weight-per-part'));
    if (rmWeightPerPart) {
        row.querySelector('.Weight_Kg').value = partsQty * rmWeightPerPart;
    }
}

// Utility: escape HTML
function escapeHtml(text) {
    var div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}


// ============================================
// Heat No / Batch No Dropdown
// ============================================
var heatBatchListCounter = 0;

function loadHeatBatchOptions(row, rawMaterialDbKey) {
    if (!rawMaterialDbKey || rawMaterialDbKey === '0') return;

    var heatInput = row.querySelector('.Heat_No');
    if (!heatInput) return;

    fetch('/MaterialIssue/GetHeatBatchNumbers?rawMaterialDbKey=' + rawMaterialDbKey)
        .then(function (r) { return r.json(); })
        .then(function (values) {
            // Create or reuse datalist
            var listId = heatInput.getAttribute('list');
            var datalist;

            if (listId) {
                datalist = document.getElementById(listId);
            }

            if (!datalist) {
                heatBatchListCounter++;
                listId = 'heatBatchList_' + heatBatchListCounter;
                datalist = document.createElement('datalist');
                datalist.id = listId;
                heatInput.setAttribute('list', listId);
                heatInput.parentElement.appendChild(datalist);
            }

            datalist.innerHTML = '';
            if (values && values.length > 0) {
                values.forEach(function (v) {
                    if (v) {
                        var opt = document.createElement('option');
                        opt.value = v;
                        datalist.appendChild(opt);
                    }
                });
            }
        })
        .catch(function () { });
}

// ============================================
// Row State Management
// ============================================

function editRow(btn) {
    var row = btn.closest('tr');
    row.setAttribute('data-mode', 'editing');

    // Show edit-mode, hide view-mode
    row.querySelectorAll('.view-mode').forEach(function (el) { el.style.display = 'none'; });
    row.querySelectorAll('.edit-mode').forEach(function (el) { el.style.display = ''; });

    bindEngineMultiSelect(row);

    // Populate edit-mode badges from current hidden values
    populateEditBadges(row);

    // Light highlight
    row.style.backgroundColor = '#fffde7';

    // Initialize engine-wise inputs: fetch part data to get qtyPerEngine
    var partKeys = row.querySelector('.hdnPartNumberKey')?.value || '';
    var savedEngineQty = row.querySelector('.PartQty_EngineWise')?.value || '';

    if (partKeys) {
        var firstPartKey = partKeys.split(',')[0].trim();
        if (firstPartKey) {
            fetch('/MaterialIssue/GetPartSmartData?partDbKey=' + firstPartKey)
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (!data.part) return;

                    if (data.rmQtyPerPart) row.setAttribute('data-rm-qty-per-part', data.rmQtyPerPart);
                    if (data.rmWeightPerPart) row.setAttribute('data-rm-weight-per-part', data.rmWeightPerPart);

                    if (data.qtyPerEngine) {
                        row.setAttribute('data-qty-per-engine', data.qtyPerEngine);
                        var engineToggle = row.querySelector('.engine-toggle');
                        if (engineToggle) engineToggle.style.display = '';
                        var hintSpan = row.querySelector('.qty-per-engine-hint');
                        if (hintSpan) hintSpan.textContent = '\u00d7 ' + data.qtyPerEngine + ' qty/eng';

                        // If engine qty was previously saved, show engine section
                        if (savedEngineQty && parseFloat(savedEngineQty) > 0) {
                            var section = row.querySelector('.engine-qty-section');
                            if (section) section.style.display = '';
                            if (engineToggle) engineToggle.textContent = 'Close';
                        }
                    }
                })
                .catch(function () { });
        }
    }

    // Wire up EngineCount input handler (only once)
    var engineInput = row.querySelector('.EngineCount');
    if (engineInput && !engineInput.hasAttribute('data-handler-bound')) {
        engineInput.setAttribute('data-handler-bound', 'true');
        engineInput.addEventListener('input', function () {
            recalcFromEngines(row);
        });
    }

    // Load Heat No / Batch No options for the current raw material
    var editRmKey = row.querySelector('.hdnRawMaterialKey')?.value || '';
    if (editRmKey && editRmKey !== '0') {
        loadHeatBatchOptions(row, editRmKey);
    }
}

function cancelEditRow(btn) {
    var row = btn.closest('tr');
    row.setAttribute('data-mode', 'readonly');

    // Show view-mode, hide edit-mode
    row.querySelectorAll('.view-mode').forEach(function (el) { el.style.display = ''; });
    row.querySelectorAll('.edit-mode').forEach(function (el) { el.style.display = 'none'; });

    // Close any open inline searches
    row.querySelectorAll('.inline-search').forEach(function (el) { el.style.display = 'none'; });
    row.querySelectorAll('.smart-display').forEach(function (el) { el.style.display = ''; });

    // Remove highlight
    row.style.backgroundColor = '';
}

function populateEditBadges(row) {
    // Get all three cells upfront for cross-cell focus handlers
    var partsCell = row.querySelector('.cell-parts');
    var rmCell = row.querySelector('.cell-rawmaterial');
    var vCell = row.querySelector('.cell-vendor');

    // Parts — show removable badges + search input directly
    var partKeys = row.querySelector('.hdnPartNumberKey').value;
    var partNames = row.querySelector('.hdnPartNames').value;
    var partsEditMode = partsCell.querySelector('.edit-mode');

    // Hide the smart-display wrapper, show inline-search directly
    partsEditMode.querySelector('.smart-display').style.display = 'none';
    var partsSearch = partsEditMode.querySelector('.inline-search');
    partsSearch.style.display = 'block';

    // Populate selected-items with removable badges
    var selectedDiv = partsSearch.querySelector('.selected-items');
    selectedDiv.innerHTML = '';
    if (partKeys && partKeys.trim() !== '') {
        var keys = partKeys.split(',');
        var names = partNames.split(',');
        for (var i = 0; i < keys.length; i++) {
            var name = (names[i] || '').trim();
            var key = (keys[i] || '').trim();
            if (key) {
                addSelectedBadge(selectedDiv, key, name, 'parts');
            }
        }
    }

    // Attach search handler
    var searchInput = partsSearch.querySelector('.search-input');
    searchInput.value = '';
    var resultsDiv = partsSearch.querySelector('.search-results');
    resultsDiv.style.display = 'none';
    searchInput.onfocus = function () {
        // Close search results in other cells when this input gets focus
        [rmCell, vCell].forEach(function (otherCell) { if (otherCell) closeSearchResults(otherCell); });
    };
    searchInput.onkeyup = function () {
        clearTimeout(searchDebounceTimer);
        var query = this.value.trim();
        if (query.length < 2) { resultsDiv.style.display = 'none'; return; }
        searchDebounceTimer = setTimeout(function () {
            performSearch(partsCell, query, 'parts', resultsDiv);
        }, 300);
    };

    // Raw Material — show current value as badge + search input
    var rmKey = row.querySelector('.hdnRawMaterialKey').value;
    var rmEditMode = rmCell.querySelector('.edit-mode');

    rmEditMode.querySelector('.smart-display').style.display = 'none';
    var rmSearch = rmEditMode.querySelector('.inline-search');
    rmSearch.style.display = 'block';

    // Show current RM as a badge above search
    var rmCurrentDiv = rmSearch.querySelector('.current-selection') || document.createElement('div');
    rmCurrentDiv.className = 'current-selection mb-1';
    var viewRmText = row.querySelector('.cell-rawmaterial .view-mode .rm-badges').textContent.trim();
    if (rmKey && rmKey !== '0' && viewRmText && viewRmText !== '-') {
        rmCurrentDiv.innerHTML = '<span class="badge mb-1" style="font-size:0.75rem;background-color:#e9ecef;color:#495057;border:1px solid #ced4da;">' + escapeHtml(viewRmText) + ' <a style="color:#dc3545;cursor:pointer;margin-left:3px;font-weight:bold;" onclick="clearRawMaterial(this)">&times;</a></span>';
    } else {
        rmCurrentDiv.innerHTML = '';
    }
    if (!rmSearch.querySelector('.current-selection')) {
        rmSearch.insertBefore(rmCurrentDiv, rmSearch.firstChild);
    }

    var rmSearchInput = rmSearch.querySelector('.search-input');
    rmSearchInput.value = '';
    var rmResultsDiv = rmSearch.querySelector('.search-results');
    rmResultsDiv.style.display = 'none';
    rmSearchInput.onfocus = function () {
        [partsCell, vCell].forEach(function (otherCell) { if (otherCell) closeSearchResults(otherCell); });
    };
    rmSearchInput.onkeyup = function () {
        clearTimeout(searchDebounceTimer);
        var query = this.value.trim();
        if (query.length < 2) { rmResultsDiv.style.display = 'none'; return; }
        searchDebounceTimer = setTimeout(function () {
            performSearch(rmCell, query, 'rawmaterial', rmResultsDiv);
        }, 300);
    };

    // Vendor — show current value as badge + search input
    var vKey = row.querySelector('.hdnVendorKey').value;
    var vEditMode = vCell.querySelector('.edit-mode');

    vEditMode.querySelector('.smart-display').style.display = 'none';
    var vSearch = vEditMode.querySelector('.inline-search');
    vSearch.style.display = 'block';

    var vCurrentDiv = vSearch.querySelector('.current-selection') || document.createElement('div');
    vCurrentDiv.className = 'current-selection mb-1';
    var viewVText = row.querySelector('.cell-vendor .view-mode .vendor-badges').textContent.trim();
    if (vKey && vKey !== '0' && viewVText && viewVText !== '-') {
        vCurrentDiv.innerHTML = '<span class="badge mb-1" style="font-size:0.75rem;background-color:#e9ecef;color:#495057;border:1px solid #ced4da;">' + escapeHtml(viewVText) + ' <a style="color:#dc3545;cursor:pointer;margin-left:3px;font-weight:bold;" onclick="clearVendor(this)">&times;</a></span>';
    } else {
        vCurrentDiv.innerHTML = '';
    }
    if (!vSearch.querySelector('.current-selection')) {
        vSearch.insertBefore(vCurrentDiv, vSearch.firstChild);
    }

    var vSearchInput = vSearch.querySelector('.search-input');
    vSearchInput.value = '';
    var vResultsDiv = vSearch.querySelector('.search-results');
    vResultsDiv.style.display = 'none';
    vSearchInput.onfocus = function () {
        [partsCell, rmCell].forEach(function (otherCell) { if (otherCell) closeSearchResults(otherCell); });
    };
    vSearchInput.onkeyup = function () {
        clearTimeout(searchDebounceTimer);
        var query = this.value.trim();
        if (query.length < 1) { vResultsDiv.style.display = 'none'; return; }
        searchDebounceTimer = setTimeout(function () {
            performSearch(vCell, query, 'vendor', vResultsDiv);
        }, 300);
    };
}

function saveRow(btn) {
    var row = btn.closest('tr');
    var issueDbkey = row.getAttribute('data-issue-dbkey');
    var itemDbkey = row.getAttribute('data-item-dbkey');

    var rawMaterialDbkey = parseInt(row.querySelector('.hdnRawMaterialKey').value) || 0;

    // Validate required fields
    if (!rawMaterialDbkey || rawMaterialDbkey === 0) {
        bootbox.alert('Please select Raw Material');
        return;
    }

    // Build FormData
    var formData = new FormData();
    formData.append('Issue_Dbkey', parseInt(issueDbkey) || 0);
    formData.append('Issue_Item_Dbkey', parseInt(itemDbkey) || 0);
    formData.append('PartKeys', row.querySelector('.hdnPartNumberKey').value || '');
    formData.append('Qty', parseFloat(row.querySelector('.edit-mode .Qty')?.value) || 0);
    formData.append('Raw_material_Dbkey', rawMaterialDbkey);
    formData.append('Vendor_Dbkey', parseInt(row.querySelector('.hdnVendorKey').value) || '');
    formData.append('SerialNo', row.querySelector('.edit-mode .SerialNo')?.value || '');
    formData.append('Size', row.querySelector('.edit-mode .Size')?.value || '');
    formData.append('Qty_Issue', parseFloat(row.querySelector('.edit-mode .Qty_Issue')?.value) || 0);
    formData.append('Heat_No', row.querySelector('.edit-mode .Heat_No')?.value || '');
    formData.append('EngineLevel', row.querySelector('.EngineLevel')?.value || '');
    formData.append('Weight_Kg', row.querySelector('.edit-mode .Weight_Kg')?.value || '');
    formData.append('Amount', parseFloat(row.querySelector('.edit-mode .Amount')?.value) || 0);
    formData.append('JobCardNumber', row.querySelector('.edit-mode .JobCardNumber')?.value || '');
    formData.append('JCFileName', row.querySelector('.JCFileName')?.value || '-');
    formData.append('JCFileLocation', row.querySelector('.JCFileLocation')?.value || '-');
    formData.append('PartQty_EngineWise', row.querySelector('.PartQty_EngineWise')?.value || '');

    // Attach file if user selected one
    var fileInput = row.querySelector('.cell-jcupload .itemFiles');
    if (fileInput && fileInput.files && fileInput.files.length > 0) {
        formData.append('jobCardFile', fileInput.files[0]);
    }

    // Show saving indicator
    btn.classList.remove('fa-check');
    btn.classList.add('fa-spinner', 'fa-spin');
    btn.style.pointerEvents = 'none';

    fetch('/MaterialIssue/SaveSingleItem', {
        method: 'POST',
        body: formData
    })
        .then(function (r) { return r.json(); })
        .then(function (result) {
            if (result.success) {
                if (!result.data) {
                    console.warn('SaveSingleItem returned success but no data');
                    bootbox.alert('Saved, but could not refresh row. Please reload.');
                    return;
                }
                // Update row with returned data
                refreshRowFromData(row, result.data);

                // Clear file input so it doesn't re-upload next save
                if (fileInput) fileInput.value = '';

                // Rebuild actions cell (fixes spinner stuck issue)
                var actionsCell = row.querySelector('.cell-actions');
                actionsCell.innerHTML =
                    '<div class="view-mode">' +
                    '<a class="fa fa-pencil" style="color:royalblue;cursor:pointer;margin-right:5px;" onclick="editRow(this)" title="Edit"></a>' +
                    '<a class="fa fa-trash" style="color:red;cursor:pointer;" onclick="deleteRow(this)" title="Delete"></a>' +
                    '</div>' +
                    '<div class="edit-mode" style="display:none;">' +
                    '<a class="fa fa-check" style="color:green;cursor:pointer;margin-right:5px;font-size:1.1rem;" onclick="saveRow(this)" title="Save"></a>' +
                    '<a class="fa fa-times" style="color:#888;cursor:pointer;font-size:1.1rem;" onclick="cancelEditRow(this)" title="Cancel"></a>' +
                    '</div>';

                // Switch back to readonly
                row.setAttribute('data-mode', 'readonly');
                row.querySelectorAll('.view-mode').forEach(function (el) { el.style.display = ''; });
                row.querySelectorAll('.edit-mode').forEach(function (el) { el.style.display = 'none'; });
                row.querySelectorAll('.inline-search').forEach(function (el) { el.style.display = 'none'; });
                row.querySelectorAll('.smart-display').forEach(function (el) { el.style.display = ''; });

                // Brief green flash
                row.style.backgroundColor = '#d4edda';
                setTimeout(function () { row.style.backgroundColor = ''; }, 1000);
            } else {
                bootbox.alert('Save failed: ' + (result.msg || 'Unknown error'));
            }
        })
        .catch(function (err) {
            console.error('SaveRow error:', err);
            bootbox.alert('Save failed: ' + err.message);
        })
        .finally(function () {
            try {
                btn.classList.remove('fa-spinner', 'fa-spin');
                btn.classList.add('fa-check');
                btn.style.pointerEvents = '';
            } catch (e) { }
        });
}

function refreshRowFromData(row, data) {
    // Update hidden inputs
    row.setAttribute('data-item-dbkey', data.Issue_Item_Dbkey);
    row.querySelector('.Issue_Item_Dbkey').value = data.Issue_Item_Dbkey;
    row.querySelector('.hdnPartNumberKey').value = data.PartKeys || '';
    row.querySelector('.hdnPartNames').value = data.Drawing_no || '';
    row.querySelector('.hdnRawMaterialKey').value = data.Raw_material_Dbkey || '';
    row.querySelector('.hdnVendorKey').value = data.Vendor_Dbkey || '';
    row.querySelector('.JCFileName').value = data.JCFileName || '-';
    row.querySelector('.JCFileLocation').value = data.JCFileLocation || '-';
    row.querySelector('.PartQty_EngineWise').value = data.PartQty_EngineWise || '';
    row.querySelector('.EngineLevel').value = data.EngineLevel || ''; //Add

    // Show engine-wise info in view mode if present
    var engineDisplay = row.querySelector('.cell-qty .engine-wise-display');
    if (engineDisplay) {
        if (data.PartQty_EngineWise && data.PartQty_EngineWise > 0) {
            engineDisplay.textContent = '(' + data.PartQty_EngineWise + ' eng)';
            engineDisplay.style.display = '';
        } else {
            engineDisplay.style.display = 'none';
        }
    }

    // Update view-mode display badges
    // Parts
    var partsBadges = row.querySelector('.cell-parts .view-mode .parts-badges');
    partsBadges.innerHTML = '';
    if (data.Drawing_no) {
        data.Drawing_no.split(',').forEach(function (p) {
            p = p.trim();
            if (p) partsBadges.innerHTML += '<span class="badge mb-1" style="font-size:0.75rem;background-color:#e9ecef;color:#495057;border:1px solid #ced4da;">' + escapeHtml(p) + '</span> ';
        });
    } else {
        partsBadges.innerHTML = '<span class="text-muted" style="font-size:0.75rem;">-</span>';
    }

    // Raw Material
    var rmBadges = row.querySelector('.cell-rawmaterial .view-mode .rm-badges');
    rmBadges.innerHTML = data.Raw_material_Name
        ? '<span class="badge mb-1" style="font-size:0.75rem;background-color:#e9ecef;color:#495057;border:1px solid #ced4da;">' + escapeHtml(data.Raw_material_Name) + '</span>'
        : '<span class="text-muted" style="font-size:0.75rem;">-</span>';

    // Vendor
    var vBadges = row.querySelector('.cell-vendor .view-mode .vendor-badges');
    vBadges.innerHTML = data.Vendor_Name
        ? '<span class="badge mb-1" style="font-size:0.75rem;background-color:#e9ecef;color:#495057;border:1px solid #ced4da;">' + escapeHtml(data.Vendor_Name) + '</span>'
        : '<span class="text-muted" style="font-size:0.75rem;">-</span>';

    // Simple text fields
    row.querySelector('.cell-qty .view-mode .display-text').textContent = data.Qty || '';
    row.querySelector('.cell-serialno .view-mode .display-text').textContent = data.SerialNo || '-';
    row.querySelector('.cell-size .view-mode .display-text').textContent = data.Size || '-';
    row.querySelector('.cell-qtyissue .view-mode .display-text').textContent = data.Qty_Issue || '';
    row.querySelector('.cell-heatno .view-mode .display-text').textContent = data.Heat_No || '-';
    row.querySelector('.cell-enginelevel .view-mode .display-text').textContent = data.EngineLevel || '-';
    row.querySelector('.cell-weight .view-mode .display-text').textContent = data.Weight_Kg || '';
    row.querySelector('.cell-jobcard .view-mode .display-text').textContent = data.JobCardNumber || '-';

    // Update Heat No input value for next edit
    var heatInput = row.querySelector('.cell-heatno .Heat_No');
    if (heatInput) heatInput.value = data.Heat_No || '';

    // Job card file icon (view mode)
    var jcView = row.querySelector('.cell-jcupload .view-mode');
    if (data.JCFileName && data.JCFileName !== '-' && data.JCFileName !== '') {
        jcView.innerHTML = '<a target="_blank" href="' + data.JCFileLocation + '" title="' + escapeHtml(data.JCFileName) + '"><i class="fa fa-file" style="color:green;"></i></a>';
    } else {
        jcView.innerHTML = '<span class="text-muted">-</span>';
    }

    // Job card file name (edit mode) - update or create existing-file display
    var jcEditMode = row.querySelector('.cell-jcupload .edit-mode');
    var existingFileDiv = jcEditMode.querySelector('.existing-file');
    if (data.JCFileName && data.JCFileName !== '-' && data.JCFileName !== '') {
        if (!existingFileDiv) {
            existingFileDiv = document.createElement('div');
            existingFileDiv.className = 'existing-file mb-1';
            existingFileDiv.style.fontSize = '0.7rem';
            jcEditMode.insertBefore(existingFileDiv, jcEditMode.firstChild);
        }
        existingFileDiv.innerHTML = '<a target="_blank" href="' + data.JCFileLocation + '" title="View ' + escapeHtml(data.JCFileName) + '" style="color:green;text-decoration:none;"><i class="fa fa-file"></i> <span class="existing-file-name">' + escapeHtml(data.JCFileName) + '</span></a>';
    } else if (existingFileDiv) {
        existingFileDiv.remove();
    }

    // Clear the file input so it doesn't re-upload the same file on next save
    var fileInput = row.querySelector('.cell-jcupload .itemFiles');
    if (fileInput) fileInput.value = '';
}

function deleteRow(btn) {
    var row = btn.closest('tr');
    var itemDbkey = row.getAttribute('data-item-dbkey');

    // New unsaved row — just remove from DOM
    if (!itemDbkey || itemDbkey === '0') {
        row.remove();
        toggleNewRowsSaveButton();
        return;
    }

    bootbox.confirm({
        message: 'Are you sure you want to delete this item?',
        buttons: {
            confirm: { label: 'Delete', className: 'btn-danger' },
            cancel: { label: 'Cancel', className: 'btn-secondary' }
        },
        callback: function (confirmed) {
            if (!confirmed) return;

            fetch('/MaterialIssue/DeleteSingleItem', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Issue_Item_Dbkey: parseInt(itemDbkey) })
            })
                .then(function (r) { return r.json(); })
                .then(function (result) {
                    if (result.success) {
                        row.style.backgroundColor = '#f8d7da';
                        setTimeout(function () { row.remove(); }, 300);
                    } else {
                        bootbox.alert('Delete failed: ' + (result.msg || 'Unknown error'));
                    }
                })
                .catch(function (err) {
                    bootbox.alert('Delete failed: ' + err.message);
                });
        }
    });
}

function addNewRow() {
    var tbody = document.getElementById("tblbody");
    var issueDbkey = document.getElementById('Issue_Dbkey').value;
    var template = document.getElementById('newRowTemplate');

    // Clone the template row
    var newRow = template.cloneNode(true);
    newRow.removeAttribute('id');
    newRow.style.display = '';
    newRow.style.backgroundColor = '#fff8e1';
    newRow.className = 'new-row';
    newRow.setAttribute('data-item-dbkey', '0');
    newRow.setAttribute('data-issue-dbkey', issueDbkey);
    newRow.setAttribute('data-mode', 'editing');

    // Remove cloned select2 markup from engine cell
    newRow.querySelectorAll('.select2-container').forEach(function (el) {
        el.remove();
    });

    var engineSelect = newRow.querySelector('.engine-select');
    if (engineSelect) {
        engineSelect.classList.remove('select2-hidden-accessible');
        engineSelect.removeAttribute('data-select2-id');
        engineSelect.removeAttribute('tabindex');
        engineSelect.removeAttribute('aria-hidden');
        engineSelect.removeAttribute('style');

        engineSelect.querySelectorAll('option').forEach(function (opt) {
            opt.selected = false;
        });
    }

    var engineHidden = newRow.querySelector('.EngineLevel');
    if (engineHidden) {
        engineHidden.value = '';
    }

    tbody.appendChild(newRow);
    bindEngineMultiSelect(newRow);

    // Attach search handlers for the three search cells
    var partsCell = newRow.querySelector('.cell-parts');
    var rmCell = newRow.querySelector('.cell-rawmaterial');
    var vCell = newRow.querySelector('.cell-vendor');

    function attachSearchHandler(cell, type, minChars) {
        var input = cell.querySelector('.search-input');
        if (!input) return;
        input.onfocus = function () {
            [partsCell, rmCell, vCell].forEach(function (c) {
                if (c !== cell) closeSearchResults(c);
            });
        };
        input.onkeyup = function () {
            clearTimeout(searchDebounceTimer);
            var query = this.value.trim();
            var resultsDiv = cell.querySelector('.search-results');
            if (query.length < minChars) {
                resultsDiv.style.display = 'none';
                return;
            }
            searchDebounceTimer = setTimeout(function () {
                performSearch(cell, query, type, resultsDiv);
            }, 300);
        };
    }

    attachSearchHandler(partsCell, 'parts', 2);
    attachSearchHandler(rmCell, 'rawmaterial', 2);
    attachSearchHandler(vCell, 'vendor', 1);

    var qtyInput = newRow.querySelector('.Qty');
    if (qtyInput) {
        qtyInput.addEventListener('input', function () {
            var partsQty = parseFloat(this.value) || 0;

            var rmQtyPerPart = parseFloat(newRow.getAttribute('data-rm-qty-per-part'));
            if (rmQtyPerPart) {
                var qtyIssueInput = newRow.querySelector('.Qty_Issue');
                if (qtyIssueInput) {
                    qtyIssueInput.value = partsQty * rmQtyPerPart;
                }
            }

            var rmWeightPerPart = parseFloat(newRow.getAttribute('data-rm-weight-per-part'));
            if (rmWeightPerPart) {
                var weightInput = newRow.querySelector('.Weight_Kg');
                if (weightInput) {
                    weightInput.value = partsQty * rmWeightPerPart;
                }
            }
        });
    }

    var engineInput = newRow.querySelector('.EngineCount');
    if (engineInput) {
        engineInput.addEventListener('input', function () {
            recalcFromEngines(newRow);
        });
    }

    toggleNewRowsSaveButton();
}
 

function toggleNewRowsSaveButton() {
    var newRows = document.querySelectorAll('#tblbody .new-row');
    var btn = document.getElementById('btnSaveNewRows');
    if (btn) {
        btn.style.display = newRows.length > 0 ? '' : 'none';
    }
}

function saveAllNewRows() {
    var newRows = document.querySelectorAll('#tblbody .new-row');
    if (newRows.length === 0) {
        bootbox.alert('No new rows to save');
        return;
    }

    var issueDbkey = parseInt(document.getElementById('Issue_Dbkey').value);
    if (!issueDbkey || issueDbkey === 0) {
        bootbox.alert('Please save the header first');
        return;
    }

    var btn = document.getElementById('btnSaveNewRows');
    btn.disabled = true;
    btn.textContent = 'Saving...';

    // Build a single FormData with all rows
    var formData = new FormData();
    newRows.forEach(function (row, index) {
        var prefix = 'items[' + index + '].';
        formData.append(prefix + 'Issue_Dbkey', issueDbkey);
        formData.append(prefix + 'Issue_Item_Dbkey', 0);
        formData.append(prefix + 'PartKeys', row.querySelector('.hdnPartNumberKey').value || '');
        formData.append(prefix + 'Qty', parseFloat(row.querySelector('.Qty')?.value) || 0);
        formData.append(prefix + 'Raw_material_Dbkey', parseInt(row.querySelector('.hdnRawMaterialKey').value) || 0);
        formData.append(prefix + 'Vendor_Dbkey', parseInt(row.querySelector('.hdnVendorKey').value) || 0);
        formData.append(prefix + 'SerialNo', row.querySelector('.SerialNo')?.value || '');
        formData.append(prefix + 'Size', row.querySelector('.Size')?.value || '');
        formData.append(prefix + 'Qty_Issue', parseFloat(row.querySelector('.Qty_Issue')?.value) || 0);
        formData.append(prefix + 'Heat_No', row.querySelector('.Heat_No')?.value || '');
        formData.append(prefix + 'EngineLevel', row.querySelector('.EngineLevel')?.value || '');
        formData.append(prefix + 'Weight_Kg', row.querySelector('.Weight_Kg')?.value || '');
        formData.append(prefix + 'Amount', parseFloat(row.querySelector('.Amount')?.value) || 0);
        formData.append(prefix + 'JobCardNumber', row.querySelector('.JobCardNumber')?.value || '');
        formData.append(prefix + 'JCFileName', row.querySelector('.JCFileName')?.value || '-');
        formData.append(prefix + 'JCFileLocation', row.querySelector('.JCFileLocation')?.value || '-');
        formData.append(prefix + 'PartQty_EngineWise', row.querySelector('.PartQty_EngineWise')?.value || '');

        // Attach file if user selected one (keyed by index for server-side mapping)
        var fileInput = row.querySelector('.cell-jcupload .itemFiles');
        if (fileInput && fileInput.files && fileInput.files.length > 0) {
            formData.append('jobCardFile_' + index, fileInput.files[0]);
        }
    });

    // Single network call for all rows
    fetch('/MaterialIssue/SaveAllNewItems', {
        method: 'POST',
        body: formData
    })
        .then(function (r) { return r.json(); })
        .then(function (response) {
            if (!response.success) {
                bootbox.alert('Save failed: ' + (response.msg || 'Unknown error'));
                return;
            }

            var failed = 0;
            response.results.forEach(function (result, index) {
                var row = newRows[index];
                if (result.success && result.data) {
                    // Convert new row to saved row
                    row.classList.remove('new-row');
                    refreshRowFromData(row, result.data);

                    // Restore proper action buttons
                    var actionsCell = row.querySelector('.cell-actions');
                    actionsCell.innerHTML =
                        '<div class="view-mode">' +
                        '<a class="fa fa-pencil" style="color:royalblue;cursor:pointer;margin-right:5px;" onclick="editRow(this)" title="Edit"></a>' +
                        '<a class="fa fa-trash" style="color:red;cursor:pointer;" onclick="deleteRow(this)" title="Delete"></a>' +
                        '</div>' +
                        '<div class="edit-mode" style="display:none;">' +
                        '<a class="fa fa-check" style="color:green;cursor:pointer;margin-right:5px;font-size:1.1rem;" onclick="saveRow(this)" title="Save"></a>' +
                        '<a class="fa fa-times" style="color:#888;cursor:pointer;font-size:1.1rem;" onclick="cancelEditRow(this)" title="Cancel"></a>' +
                        '</div>';

                    // Switch to readonly
                    row.setAttribute('data-mode', 'readonly');
                    row.querySelectorAll('.view-mode').forEach(function (el) { el.style.display = ''; });
                    row.querySelectorAll('.edit-mode').forEach(function (el) { el.style.display = 'none'; });
                    row.style.backgroundColor = '#d4edda';
                    setTimeout(function () { row.style.backgroundColor = ''; }, 1500);
                } else {
                    failed++;
                }
            });

            if (failed > 0) {
                bootbox.alert(failed + ' row(s) failed to save');
            }
            toggleNewRowsSaveButton();
        })
        .catch(function (err) {
            bootbox.alert('Save failed: ' + err.message);
        })
        .finally(function () {
            btn.disabled = false;
            btn.textContent = 'Save All New Rows';
        });
}

// ============================================
// Demand Select2 & Auto-Fill
// ============================================

function initDemandSelect2() {
    var $demandSelect = $('#Demand_Number');
    if (!$demandSelect.length) return;

    // Find the modal-body parent for dropdownParent (so dropdown renders inside the modal and isn't clipped)
    var $modalBody = $demandSelect.closest('.modal-body');
    if (!$modalBody.length) {
        $modalBody = $demandSelect.closest('.bootbox, .modal');
    }
    var select2Options = {
        placeholder: 'Search by MMG No / Order No / Description...',
        allowClear: true,
        width: '100%'
    };
    if ($modalBody.length) {
        select2Options.dropdownParent = $modalBody;
    }

    // Pre-select saved demand value before initializing Select2
    var savedDemandDbKey = $('#DemandDbKey').val();
    if (savedDemandDbKey && savedDemandDbKey !== '0') {
        $demandSelect.val(savedDemandDbKey);
    }

    $demandSelect.select2(select2Options);

    // On demand selection change, fetch details and auto-fill header fields
    $demandSelect.on('change', function () {
        var demandDbKey = $(this).val();
        if (!demandDbKey || demandDbKey === '0') return;

        $.get('/MaterialIssue/GetDemandDetails', { id: demandDbKey }, function (data) {
            if (!data.success) return;

            // Store demandNo on the selected option so SaveHeader can read it
            var selectedOption = $demandSelect[0].options[$demandSelect[0].selectedIndex];
            if (selectedOption) {
                selectedOption.setAttribute('data-demandno', data.demandNo);
            }

            // Pre-fill Order Ref No (only if currently empty)
            var orderRefInput = document.getElementById('Order_Ref_No');
            if (orderRefInput && (!orderRefInput.value || orderRefInput.value.trim() === '')) {
                orderRefInput.value = data.orderNumbers;
            }

            // Pre-fill Order Ref Date (only if currently empty)
            var orderDateInput = document.getElementById('Order_Ref_Date');
            if (orderDateInput && (!orderDateInput.value || orderDateInput.value.trim() === '')) {
                orderDateInput.value = data.orderDate;
            }

            // Pre-fill Vendor (only if currently not selected)
            var vendorSelect = document.getElementById('Vendor');
            if (vendorSelect && data.vendorDbkey && data.vendorDbkey !== 0 &&
                (!vendorSelect.value || vendorSelect.value === '0' || vendorSelect.value === '')) {
                vendorSelect.value = data.vendorDbkey;
            }
        });
    });
}

// ============================================

function SaveHeader() {
    var issueDbkey = parseInt(document.getElementById('Issue_Dbkey').value) || 0;
    var demandSelect = document.getElementById("Demand_Number");
    var selectedOption = demandSelect ? demandSelect.options[demandSelect.selectedIndex] : null;

    var headerData = {
        Issue_Dbkey: issueDbkey,
        Engine_Name: document.getElementById("Engine_Name").value,
        DemandDbKey: parseInt(demandSelect.value) || null,
        Demand_No: selectedOption ? (selectedOption.getAttribute('data-demandno') || selectedOption.text) : '',
        Order_Ref_No: document.getElementById("Order_Ref_No").value,
        Order_Ref_Date: document.getElementById("Order_Ref_Date").value || null,
        Vendor: parseInt(document.getElementById("Vendor").value) || null,
        Returnable: document.getElementById("Returnable").value,
        Issue_Purpose: parseInt(document.getElementById("Issue_Purpose").value) || null
    };

    var btn = document.getElementById('SaveHeaderLnk');
    btn.disabled = true;
    btn.textContent = 'Saving...';

    fetch('/MaterialIssue/SaveHeader', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(headerData)
    })
        .then(function (r) { return r.json(); })
        .then(function (result) {
            if (result.success) {
                // Update Issue_Dbkey if it was a new issue
                if (result.data.Issue_Dbkey) {
                    document.getElementById('Issue_Dbkey').value = result.data.Issue_Dbkey;
                }
                bootbox.alert({ message: 'Header saved successfully', size: 'small' });
            } else {
                bootbox.alert('Save failed: ' + (result.msg || 'Unknown error'));
            }
        })
        .catch(function (err) {
            bootbox.alert('Save failed: ' + err.message);
        })
        .finally(function () {
            btn.disabled = false;
            btn.textContent = 'Save Header';
        });
}
function clearRawMaterial(closeBtn) {
    var row = closeBtn.closest('tr');
    row.querySelector('.hdnRawMaterialKey').value = '0';
    closeBtn.closest('.current-selection').innerHTML = '';
}

function clearVendor(closeBtn) {
    var row = closeBtn.closest('tr');
    row.querySelector('.hdnVendorKey').value = '0';
    closeBtn.closest('.current-selection').innerHTML = '';
}
