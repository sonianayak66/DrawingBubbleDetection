

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ NCR INDEX ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
var NCRData;
var NCRDataForList = [];
var NCRAttachments;
var tomselect;
var tomSelectForMultipleModules;
var dataTable;
//Get NCR Data
var newrows = 0;
var identityMceEditor = 1000;
function getNCRData() {
    $.ajax({
        type: "Get",
        url: "/NCR/GetNCRdatalist",
        success: function (data) {
            NCRDataForList = JSON.parse(data);
            GetNCRdatalist();
        }
    })
}
function loadPartialView(ncrGuid, ncrWorkFlowGUID, containerId) {
    dataJson = { ncrGuid: ncrGuid, ncrWorkFlowGUID: ncrWorkFlowGUID };
    //console.log(dataJson);
    $.ajax({
        type: "GET",
        url: "/NCR/NCRItemstatus",
        data: dataJson,
        success: function (response) {
            $("#" + containerId).html(response);
        },
        error: function () {
            $("#" + containerId).html("<p>Error loading partial view</p>");
        }
    });
}
function GetNCRdatalist() {
    try {
        dataTable.destroy();
    } catch (e) { }
    

    dataTable = $("#NCRtable").DataTable({
        scrollY: 510,
        data: NCRDataForList,
        paging: false,
        autoWidth: true,
        // responsive: true,
        //"order": [[0, "asc"]],
        dom: 'Bfrtip',
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
        buttons: [
            {
                extend: 'excel',
                title: 'Non Conformance Reports',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10] //Your Colume value those you want
                }
            }],
        columns: [
            {
                data: null,
                // render: function (data) {
                //     return '<p data-sort="YYYYMMDD">' + moment(data.ReceivedDate).format('DD/MM / YYYY') + ' </p>' /* + '</p><p>[' + data.ReferenceNumber + ']</p>' */;
                // }
                render: function (data, type, row) {
                    if (type === 'sort' || type === 'type') {
                        // Return the raw date for sorting and type detection
                        return data.ReceivedDate;
                    }
                    // Format the date for display
                    return '<p>' + moment(data.ReceivedDate).format('DD/MMM/YYYY') + '</p>' + '<p>[' + data.ReferenceNumber + ']</p>';
                },
                type: 'date-eu' // Define the type of data for correct sorting
            },
            //  { "data": "ReferenceNumber" },
            { data: "ReceivedFrom" },
            { data: "ComitteeReferred" },
            {
                data: null,
                render: function (data) {
                    return '<p>' + data.Draw_part_no + '/' + data.PartDes + '</p>';
                }
            },
            { data: "Vendor_Name" },

            {
                data: null,
                className: "serialNoCol",
                render: function (data) {
                    if (data.SerialNumber == null) {
                        return '';
                    }
                    else {
                        /*if (data.statusUpdatedCount == null) {*/
                        var serialNo = data.SerialNumber.split(',');
                            var counter = 0;
                            var html = serialNo.map(function (serial) {
                                if (serial.trim() != "") {
                                    counter++;
                                    if (counter <= 5) {
                                        return '<p style="margin:0px" class="badge badge-phoenix badge-phoenix-primary  fs--2 text-dark">' + serial + '</p>';
                                    } else {
                                        return '<p style="display:none; margin:0px" class="badge badge-phoenix badge-phoenix-primary fs--2 text-dark">' + serial + '</p>';
                                    }
                                }
                               
                            }).join('');

                            if (serialNo.length > 5) {
                                html += '<span data-action="show" style="margin-left:10px; cursor:pointer" onclick="toggleSerialNumberDisplay(this)"><i class="fas fa-angle-down fa-1x"></i></span>';
                                 }

                            return html;
                        //}
                        //else {
                        //    return '';
                        //    //var containerId = 'partial-container-' + data.NCRGuid;
                        //    //setTimeout(function () {
                        //    //    loadPartialView(data.NCRGuid, 'null', containerId);
                        //    //}, 0);

                        //    //return '<div id="' + containerId + '"></div>';
                        //}
                    }
                }
            },

            { data: "Revision" },
            { data: "Qty" },
            { data: "ReportStatus" },
            { data: "Remarks" },
            { data: "Module_Responsibilty" },
            {
                //data: "UpdatedOn"
                data: null,
                render: function (data, type, row) {
                    if (type === 'sort' || type === 'type') {
                        // Return the raw date for sorting and type detection
                        return data.UpdatedOn;
                    }
                    // Format the date for display
                    return '<p>' + moment(data.UpdatedOn).format('DD/MMM/YYYY') + '</p>';
                },
                type: 'date-eu' // Define the type of data for correct sorting
            },
            { data: "EditLink" },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data, type, row) {
                    // If no permission → no button
                    if (!window.canDeleteNcr) {
                        return '';
                    }

                    // You need some unique id to delete – adjust according to your model:
                    // Example: data.NCRGuid or data.NCRId
                    var ncrGuid = data.NCRGuid; // change to your real key
                  
                    return '<button class="" onclick="deleteNcr(\'' + ncrGuid + '\')">' +
                        '<i class="fa fa-trash" style="color:red;"></i>' +
                        '</button>';
                }
            }
        ],
        initComplete: function (settings, json) {
            setTimeout(ReDrawDatable, 1000);
        }

    });

}

function toggleSerialNumberDisplay(toggleEle) {
    var action = $(toggleEle).data("action");

    toggleEle.children[0].classList.remove("fa-angle-down");
    toggleEle.children[0].classList.remove("fa-angle-up");

    if (action == "show") {
        $(toggleEle).data('action', 'Hide');
        toggleEle.children[0].classList.add("fa-angle-up");

    } else {
        $(toggleEle).data('action', 'show');
        toggleEle.children[0].classList.add("fa-angle-down");
    }



    var parentCell = toggleEle.parentElement;
    for (i = 0; i < parentCell.children.length; i++) {
        if (toggleEle == parentCell.children[i]) {
            continue;
        }
        if (i > 5 && action == "Hide") {
            parentCell.children[i].style.display = "none";
        } else {
            parentCell.children[i].style.display = "";
        }
    }
}

function ReDrawDatable() {
    dataTable.draw()
}
function displayAttachments(ncrKey, partnumber) {
    var itemKey = ncrKey;
    var soureTable = "NonConformanceReport";
    var parent = document.getElementById("attachmentDisplay");
    parent.innerHTML = "";
    var deleteAccess = document.getElementById("Permission_delete").value;
    Globalgetattachments(parent, itemKey, soureTable, deleteAccess);
    //pause thread for 2 seconds
    setTimeout(function () {
        bootbox.alert({
            title: "Attachments for " + partnumber,
            message: parent.innerHTML,
            centerVertical: true
        });
    }, 2000);
}
function viewMoreNCRItems(arrow, className) {
    console.log(arrow, className);
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

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ NCR Create and Edit ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
function applySelect2() {
    $('.searchable').select2({
        // theme: "classic"
    });
}

function SubmitReport() {
    if (!$('#NCR').valid()) { return false }
    // var data = $("#NCR").serializeArray();

    var formData = {};
    $('#NCR').find('input, select,textarea').each(function (index, element) {
        var name = $(element).data('field');
        var value = $(element).val();
        formData[name] = value;
    });

    



    var tableData = [];

    $('#tblbody tr').each(function (index, row) {
        if (index === 0) return; // Skip the first row
        var rowData = {};
        $(row).find('input, select , textarea').each(function (index, input) {
            var elementType = input.nodeName.toLowerCase();
            var field = $(input).data('field');
            var value;
            if (elementType === 'input') {
                value = $(input).val();
            } else if (elementType === 'select') {
                value = $(input).val();
            } else if (elementType === 'textarea') {
                var editorId = $(input).attr('id');
                if (tinymce.get(editorId)) {
                    // Fetch the content from the TinyMCE editor
                    value = tinymce.get(editorId).getContent();
                } else {
                    // If not TinyMCE, get the textarea value normally
                    value = $(input).val();
                }
            } else {
                value = $(input).val();
            }

 


            if (value === "") {
                validData = false;
            }
            rowData[field] = value;
        });
        tableData.push(rowData);

    });
   // console.log(tableData);
    var dataToSend = {
        nonConformanceReportVM: formData,
        nonConformanceReport_Items: tableData
    };
   // console.log(dataToSend);
 
    $.ajax({
        type: 'POST',
        url: '/NCR/NCR',
        contentType: 'application/json',
        data: JSON.stringify(dataToSend),
        success: function (data) {
            bootbox.alert(data.msg,
                function () {
                    window.location.href = "/NCR/NCR?NCRGuid=" + data.ncrguid;
                });
        },
        error: function (data) { bootbox.alert(data.msg) }
    });
}

//function addRow(slno) {
//    newrows = newrows + 1;
//    identityMceEditor = identityMceEditor + 1;
//    var newrowcls = "DimensionNewRow_" + newrows;
//    var tblBody = document.getElementById("tblbody"); // Get the table body
//    var node = tblBody.rows[0].cloneNode(true); // Clone the hidden template row
//    var className = 'SerialNumber';
//    if (slno !== undefined) {
//        var serialNumberInput = node.querySelector('.' + className);
//        if (serialNumberInput) {
//            serialNumberInput.value = slno; // Set the serial number
//        }
//    }
//    node.style.display = ''; // Show the row

//    var thirdTd = node.getElementsByTagName('td')[1]; // Index 2 is the 3rd <td>
//    if (thirdTd) {
//        // Get the textarea inside the third td and add a class to it
//        var textarea = thirdTd.getElementsByTagName('textarea')[0]; // Find the textarea in the third td
//        if (textarea) {
//            textarea.classList.add(newrowcls); // Add your custom class to the textarea
//            textarea.id = "DrawingDimension_" +  identityMceEditor;
//        }
//    }

//    var fourthTd = node.getElementsByTagName('td')[2]; // Index 2 is the 3rd <td>
//    if (fourthTd) {
//        // Get the textarea inside the third td and add a class to it
//        var textarea = fourthTd.getElementsByTagName('textarea')[0]; // Find the textarea in the third td
//        if (textarea) {
//            textarea.classList.add(newrowcls); // Add your custom class to the textarea
//            textarea.id = "ActualDimension_" + identityMceEditor;
//        }
//    }




//    tblBody.appendChild(node); // Add the new row to the table
//    initializeTinyMCE("."+newrowcls);
//}


function addRow(slno) {
    newrows = newrows + 1;
    identityMceEditor = identityMceEditor + 1;
    var newrowcls = "DimensionNewRow_" + newrows;
    var tblBody = document.getElementById("tblbody");
    var node = tblBody.rows[0].cloneNode(true);

    if (slno !== undefined) {
        var serialNumberSelect = node.querySelector('select[data-field="SerialNumber"]');
        if (serialNumberSelect) {
            serialNumberSelect.value = slno;
            $(serialNumberSelect).trigger('change');
        }
    }
    node.style.display = '';

    // Find textareas by data-field attribute instead of position
    var drawingTextarea = node.querySelector('textarea[data-field="DrawingDimension"]');
    var actualTextarea = node.querySelector('textarea[data-field="ActualDimension"]');
    var reworkTextarea = node.querySelector('textarea[data-field="Rework_Dimension"]');

    // Setup Drawing Dimension textarea
    if (drawingTextarea) {
        drawingTextarea.classList.add(newrowcls);
        drawingTextarea.classList.add('Dimension');
        drawingTextarea.id = "DrawingDimension_" + identityMceEditor;
        drawingTextarea.value = ''; // Clear any existing content
    }

    // Setup Actual Dimension textarea
    if (actualTextarea) {
        actualTextarea.classList.add(newrowcls);
        actualTextarea.classList.add('Dimension');
        actualTextarea.id = "ActualDimension_" + identityMceEditor;
        actualTextarea.value = ''; // Clear any existing content
    }

    // Setup Rework Dimension textarea (if it exists)
    if (reworkTextarea) {
        reworkTextarea.classList.add(newrowcls);
        reworkTextarea.classList.add('Dimension');
        reworkTextarea.id = "ReworkDimension_" + identityMceEditor;
        reworkTextarea.value = ''; // Clear any existing content
    }

    tblBody.appendChild(node);

    // Initialize TinyMCE for all textareas with a small delay to ensure DOM is ready
    // Initialize TinyMCE with minimal delay
    requestAnimationFrame(function () {
        if (drawingTextarea) {
            initializeTinyMCEForElement(drawingTextarea.id);
        }
        if (actualTextarea) {
            initializeTinyMCEForElement(actualTextarea.id);
        }
        if (reworkTextarea) {
            initializeTinyMCEForElement(reworkTextarea.id);
        }
    });
}

function initializeTinyMCEForElement(elementId) {
    tinymce.init({
        selector: '#' + elementId,
        height: 80,
        branding: false,
        toolbar: false,
        menubar: false,
        plugins: 'table | contextmenu',
        contextmenu: 'inserttable | Straightness | Dia | Flatness | Circularity | Cylindricity | Perpendicularity | Parallelism | Symmetry | TRunout | Concentricity | Angularity | Position | CRunout | PSurface | PLine | Datum | MMC | LMC | RFS | Degree',
        setup: function (editor) {
            editor.on('init', function () {
                const body = editor.getBody();
                body.style.lineHeight = '1.3';
                body.style.fontSize = '14px';
                body.style.padding = '2px 8px';
                body.style.margin = '0';
                body.style.paddingTop = '0';
                body.style.fontWeight = 'normal'; // Add this
                body.style.fontFamily = 'inherit'; // Add this
            });

            function insertWithClass(character) {
                editor.insertContent(`<span style="font-size: 14px!important; font-weight: normal!important;">${character}</span>`);
            }

            // Add all your existing menu items here (copy from your existing initializeTinyMCE function)
            editor.ui.registry.addMenuItem('Straightness', {
                text: 'Straightness : ⏤',
                onAction: function () {
                    const specialCharacter = '⏤';
                    editor.insertContent(specialCharacter);
                }
            });

            editor.ui.registry.addMenuItem('Dia', {
                text: 'Dia : ⌀',
                onAction: function () {
                    // const specialCharacter = '⌀';
                    // editor.insertContent(specialCharacter);
                    insertWithClass('⌀');
                }
            });

            editor.ui.registry.addMenuItem('Flatness', {
                text: 'Flatness : ▱',
                onAction: function () {
                    //const specialCharacter = '▱';
                    //editor.insertContent(specialCharacter);
                    insertWithClass('▱');
                }
            });

            editor.ui.registry.addMenuItem('Circularity', {
                text: 'Circularity : ◯',
                onAction: function () {
                    const specialCharacter = '◯';
                    editor.insertContent(specialCharacter);
                }
            });

            editor.ui.registry.addMenuItem('Cylindricity', {
                text: 'Cylindricity : ⌭',
                onAction: function () {
                    //const specialCharacter = '⌭';
                    //editor.insertContent(specialCharacter);
                    insertWithClass('⌭');
                }
            });

            editor.ui.registry.addMenuItem('Perpendicularity', {
                text: 'Perpendicularity : ⟂',
                onAction: function () {
                    const specialCharacter = '⟂';
                    editor.insertContent(specialCharacter);

                }
            });

            editor.ui.registry.addMenuItem('Parallelism', {
                text: 'Parallelism : //',
                onAction: function () {
                    const specialCharacter = '//';
                    editor.insertContent(specialCharacter);
                }
            });

            editor.ui.registry.addMenuItem('Symmetry', {
                text: 'Symmetry : ⌯',
                onAction: function () {
                    //const specialCharacter = '⌯';
                    //editor.insertContent(specialCharacter);
                    insertWithClass('⌯');
                }
            });


            editor.ui.registry.addMenuItem('TRunout', {
                text: 'Total Runout : ⌰',
                onAction: function () {
                    //const specialCharacter = '⌰';
                    //editor.insertContent(specialCharacter);
                    insertWithClass('⌰');
                }
            });

            editor.ui.registry.addMenuItem('Concentricity', {
                text: 'Concentricity : ⊙',
                onAction: function () {
                    //const specialCharacter = '⊙';
                    //editor.insertContent(specialCharacter);
                    insertWithClass('⊙');
                }
            });


            editor.ui.registry.addMenuItem('Angularity', {
                text: 'Angularity : ∠',
                onAction: function () {
                    const specialCharacter = '∠';
                    editor.insertContent(specialCharacter);
                }
            });

            editor.ui.registry.addMenuItem('Position', {
                text: 'Position : ⌖',
                onAction: function () {
                    //const specialCharacter = '⌖';
                    //editor.insertContent(specialCharacter);
                    insertWithClass('⌖');
                }
            });

            editor.ui.registry.addMenuItem('CRunout', {
                text: 'Circular Runout : ↗',
                onAction: function () {
                    const specialCharacter = '↗';
                    editor.insertContent(specialCharacter);
                }
            });

            editor.ui.registry.addMenuItem('PSurface', {
                text: 'Profile of a Surface : ⌓',
                onAction: function () {
                    const specialCharacter = '⌓';
                    editor.insertContent(specialCharacter);
                }
            });

            editor.ui.registry.addMenuItem('PLine', {
                text: 'Profile of a Line : ⌒',
                onAction: function () {
                    const specialCharacter = '⌒';
                    editor.insertContent(specialCharacter);
                }
            });

            editor.ui.registry.addMenuItem('Datum', {
                text: 'Datum : ⌷',
                onAction: function () {
                    const specialCharacter = '⌷';
                    editor.insertContent(specialCharacter);
                }
            });

            editor.ui.registry.addMenuItem('MMC', {
                text: 'Maximum Material Condition (MMC) : Ⓜ',
                onAction: function () {
                    const specialCharacter = 'Ⓜ';
                    editor.insertContent(specialCharacter);
                }
            });

            editor.ui.registry.addMenuItem('LMC', {
                text: 'Least Material Condition (LMC) : Ⓛ',
                onAction: function () {
                    const specialCharacter = 'Ⓛ';
                    editor.insertContent(specialCharacter);
                }
            });

            editor.ui.registry.addMenuItem('RFS', {
                text: 'Regardless of Feature Size (RFS) : Ⓡ',
                onAction: function () {
                    const specialCharacter = 'Ⓡ';
                    editor.insertContent(specialCharacter);
                }
            });

            editor.ui.registry.addMenuItem('Degree', {
                text: 'Degree : °',
                onAction: function () {
                    const specialCharacter = '°';
                    editor.insertContent(specialCharacter);
                }
            });


            editor.on('contextmenu', function (e) {
                const target = e.target;
                if (target.nodeName !== 'TEXTAREA') {
                    e.preventDefault();
                }
            });
        }
    });

}
 
function removeRow(button) {

    var key = $(button).data('value');
    if (key != "0") {
        bootbox.confirm({
            message: "Are you sure you want to delete this item?",
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
                bootbox.hideAll();
                if (result) {
                    deleteSavedItem(key);
                    deleteUIitem(button);
                } else {
                    return false;
                }
            }
        });
    } else {
        deleteUIitem(button);
    }
}

function deleteUIitem(button) {
    var row = button.parentNode.parentNode;
    var tblBody = document.getElementById("tblbody");
    if (tblBody.rows.length > 1) {
        tblBody.removeChild(row);
    } else {
        alert("At least one row must be present in the table.");
    }
}
function deleteSavedItem(key) {
    $.ajax({
        url: '/NCR/deleteNCRItem?key=' + key,
        type: "GET",
        success: function (data) {
        },
        error: function (error) {
            console.error("Error fetching data:", error);
        }
    });
}

function applyTextAreaContextMenu() {
    $.contextMenu({
        selector: '#nonConformanceReportVM_SerialNumber',
        callback: function (key, options) {
            if (key === "insertSerialNo") {
                var slnos = document.getElementById("nonConformanceReportVM_SerialNumber").value;
                // Split the value by commas into an array
                var serialNumbers = slnos.split(',');

                // Loop through each serial number in the order entered
                for (var i = 0; i < serialNumbers.length; i++) {
                    var serialNumber = serialNumbers[i].trim(); // Trim any whitespace
                    var tableData = [];

                    $('#tblbody tr').each(function (index, row) {
                        if (index === 0) return; // Skip the first row (template)
                        var rowData = {};
                        $(row).find('select, input').each(function (index, select) {
                            var field = $(select).data('field');
                            var value = $(select).val();
                            rowData[field] = value;
                        });
                        tableData.push(rowData);
                    });

                    var isSerialNumberPresent = tableData.find(x => x.SerialNumber === serialNumber);
                    if (!isSerialNumberPresent) {
                        addRow(serialNumber);
                    }
                }
            }

            if (key === "generateSerialNos") {
                var rangeInput = prompt("Enter the range (e.g., 'BTP-1 to BTP-15', '1 to 50', '41/42/A to 41/55/A', or '222/A to 226/A'):");
                if (rangeInput) {
                    var range = parseComplexRange_NCR(rangeInput);
                    if (range) {
                        var list = generateComplexList_NCR(range);
                        if (list) {
                            appendToTextArea_NCR(options.$trigger, list.join(', '));
                        } else {
                            alert("Error generating the list. Please check the range format.");
                        }
                    } else {
                        alert("Invalid range format. Please enter in the correct format.");
                    }
                }
            }

            if (key === "Sl_engine_mapping") {
                openSerialEngineMapping();
            }
        },
        items: {
            "generateSerialNos": {
                name: "Generate serial nos",
                icon: function () {
                    return 'bi bi-list-ol';
                }
            },
            "insertSerialNo": {
                name: "Insert serial nos to deviations table",
                icon: function () {
                    return 'bi bi-list-ol';
                }
            },
            "Sl_engine_mapping": {
                name: "Map serial numbers to engine",
                icon: function () {
                    return 'bi bi-list-ol';
                }
            }
            
        }
    });
}

function parseComplexRange_NCR(text) {
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

function generateComplexList_NCR(range) {
    var list = [];
    for (var i = range.start; i <= range.end; i++) {
        list.push(range.prefix + i + range.suffix);
    }
    return list;
}

function appendToTextArea_NCR(inputField, content) {
    var currentContent = inputField.val();
    inputField.val(currentContent + (currentContent ? ', ' : '') + content);
}

function autofillallenginename(clickedInput) {
    const valueToFill = clickedInput.value;
    const className = 'EngineName'; // Class name to target

    // Find all inputs with the same class name and fill them
    const inputs = document.querySelectorAll('.' + className);
    inputs.forEach(input => {
        if (input !== clickedInput && input.value === '') {
            input.value = valueToFill;
        }
    });
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ NCR Module Assignment ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

function GetPopupToAssign(NCRGUID) {

    $.get("/NCR/checkExistingAssignment?NCRGUID=" + NCRGUID).done(function (checkExistingData) {
        // if (response.worklowguid != "") {
        //     //window.location.href = "/NCR/AssignmentDetail?id=" + response.worklowguid;
        // }
        // else
        // {
        if (checkExistingData.isSerialNoPresent === false) {
            bootbox.alert("There are no serial numbers to assign");
        }
        else {
            var urlinput = "/NCR/NCRWorkflowAssignment?NCRGUID=" + NCRGUID + "&workflowguid=" + checkExistingData.workflowguid;
            $.get(urlinput).done(function (response) {

                bootbox.dialog({
                    title: "Assign NCR",
                    message: response,
                    size: 'extra-large',
                    closeButton: true,
                });
                tomselect = new TomSelect("#UsersSelectList", {
                    create: false,
                    plugins: ['remove_button'],
                });
                tomSelectForMultipleModules = new TomSelect("#ModuleID", {
                    create: false,
                   // plugins: ['remove_button'],
                });
            });
        }
        //}
    });
}

function ShowAssignedNCR(ncrGUID) {
    var urlinput = "/NCR/checkifModuleAssignedforNCR?ncrguid=" + ncrGUID;
    $.get(urlinput).done(function (response) {
        if (response.success === true) {
            GetPopupToAssignModuleToNCR(ncrGUID,0);
        }
        else {
            window.location.href = "/NCR/AssignedNCRList?NCRID=" + ncrGUID;
        }

    });
}


// need to use this while test new ncr workflow 
//function ShowAssignedNCR(ncrGUID) {
//    var urlinput = "/NCR/checkifModuleAssignedforNCR?ncrguid=" + ncrGUID;
//    $.get(urlinput).done(function (response) {
//        if (response.success === true) {
//            // NCR not yet assigned - show new workflow assignment popup
//            GetPopupToAssignNewWorkflow(ncrGUID);  //  this is in ncr workflow js 
//        }
//        else {
//            // NCR already assigned - redirect to view
//            window.location.href = "/NCR/AssignedNCRList?NCRID=" + ncrGUID;
//        }
//    });
//}


function filterUsers() {
    var userSelectList = document.getElementById("UsersSelectList");
    var ModuleID = document.getElementById("ModuleID").value;
    var urlinput = "/NCR/GetUSersForAssignment?ModuleID=" + ModuleID;
    $.get(urlinput).done(function (response) {
        tomselect.setValue(response);
    });
}

//function saveToLogs(ncrItemKey, element) {
//    //if (!$('#ncrWorkflowLog').valid()) { return false }

//    //ncr items - serial number status
//    var tableData = [];
//    $('#tblbody tr').each(function (index, row) {
//        var rowData = {};
//        $(row).find('input, select,textarea').each(function (index, input) {
//            var field = $(input).data('field');
//            var value = $(input).val();
//            if (value === "") {
//                validData = false;
//            }
//            rowData[field] = value;
//         //   console.log(field);
//           // console.log(value);
//        });
//        tableData.push(rowData);
//    });
//    // console.log(tableData);

//    //ncr workflow logs data
//    var formData = {};
//    formData.NCRWorkflowGUID = $("#NCRWorkflowGUID").val();
//    formData.Status = $("#ncrLogStatus").val();
//    formData.Remarks = $("#ncRLogRemarks").val();
//    // console.log(formData);

//    var dataToSend = {
//        AssignmentData: formData,
//        NcrItems: tableData
//    };
//  //  console.log(dataToSend);
//   // return false;
//    $.ajax({
//        url: '/NCR/SaveAssignmentLogs',
//        type: 'POST',
//        data: JSON.stringify(dataToSend),
//        contentType: 'application/json',
//        success: function (response) {
//            if (ncrItemKey === 0) {
//                if (response.success == true) {
//                    bootbox.alert({
//                        message: "Saved Successfully",
//                        callback: function () {
//                            window.location.reload();
//                        }
//                    });
//                }
//                else {
//                    bootbox.alert("Failed to save");
//                }
//            }
//            else {
//                if (response.success == true) {
//                    $(element).notify("Remarks Updated", {
//                        className: "success",
//                        autoHideDelay: 2000,
//                    });
//                   // window.location.reload();
//                }
//                else {

//                }
//            }
//        },
//        error: function (xhr, status, error) {
//            console.error('Error:', error);
//            bootbox.alert("Failed to save");

//        }
//    });
//}

function TransferNCRModuleItem(NCRGUID) {
    GetPopupToAssign(NCRGUID);
}
function saveWorkflow() {
    if (!$('#AssignWorkflow').valid()) { return false }
    var form = $('#AssignWorkflow');
    var users = $("#UsersSelectList").val();
    var checkboxes = document.querySelectorAll('input.NCRItemKeys:checked');
    let NCRItemKeys = [];
    var ModuleID = $("#ModuleID").val();
    if (ModuleID == 0) {
        bootbox.alert("Select a Module to transfer");
        return false
    }
    if (users == "") {
        bootbox.alert("Select a User to transfer");
        return false
    }


    $.validator.unobtrusive.parse("#" + form.attr("id"));
    $(form).validate();
    if (form.valid() == false) {
        return false;
    }
    checkboxes.forEach((checkbox) => {
        NCRItemKeys.push(checkbox.value);
    });
    var data = {};

    data.NCRGUID = $("#ncrguid").val();
    data.ModuleID = ModuleID;
    data.AssigneeUserGUIDs = users.join(',');
    data.Remarks = $("#assignmentData_Remarks").val();
    data.NCRItemKeys = NCRItemKeys.join(',');
    if (data.NCRItemKeys == "") {
        bootbox.alert("Select a Serial Number to transfer");
        return false;
    }
    bootbox.hideAll();
    let LoadingDialog = bootbox.dialog({
        message: '<p class="text-center mb-0"><i class="fas fa-spin fa-cog"></i> Please wait</p>',
        closeButton: false
    });
    //console.log(data);
    $.ajax({
        url: '/NCR/SaveNCRWorkflowAssignment',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            bootbox.hideAll();
            bootbox.alert({
                message: "Referred Successfully",
                callback: function () {
                    bootbox.hideAll();
                    window.location.reload();
                }
            });
        },
        error: function (xhr, status, error) {
            bootbox.hideAll();
            console.error('Error:', error);
            bootbox.alert({
                message: "Failed to assign",
                callback: function () {
                    bootbox.hideAll();
                }
            });
        }
    });
}

function selectAllNCRSerialNumber(masterCheckbox) {
    var checkboxes = document.querySelectorAll('#NCRSerialNoDataTbl tbody .NCRItemKeys');
    checkboxes.forEach(function (checkbox) {
        checkbox.checked = masterCheckbox.checked;
    });
}

function checkIndividual(checkbox) {
    const selectAllCheckbox = document.getElementById('NCRItemKeys-selectall');
    const checkboxes = document.querySelectorAll('input.NCRItemKeys');

    // Check if any checkbox is unchecked
    const allChecked = Array.from(checkboxes).every(checkbox => checkbox.checked);

    // If any checkbox is unchecked, uncheck the "Select All" checkbox
    selectAllCheckbox.checked = allChecked;
}

function GetPopupToAssignModuleToNCR(NCRGUID,moduleId) {
    var urlinput = "/NCR/AssignModuleToNCR?NCRGUID=" + NCRGUID;
    $.get(urlinput).done(function (response) {
        bootbox.dialog({
            title: "Assign NCR",
            message: response,
            size: 'lg',
            closeButton: true,
        });
        tomselect = new TomSelect("#UsersSelectList", {
            create: false,
            plugins: ['remove_button'],
        });
        const moduleSelect = new TomSelect("#ModuleID", {
            create: false,
        });
        moduleSelect.setValue(moduleId);
        if (moduleId != 0 || moduleId != '') {
            moduleSelect.disable();
        }
    });
}
function saveAssignModuleToNCR() {
    var form = $('#AssignModuleToNCR');
    var users = $("#UsersSelectList").val();
    let Moduleid = $("#ModuleID").val();
    $.validator.unobtrusive.parse("#" + form.attr("id"));
    $(form).validate();
    if (form.valid() == false) {
        return false;
    }
    if (Moduleid == 0) {
        bootbox.alert("Select a module to assign");
        return false;
    }
    var data = {};

    data.NCRGuid = $("#ncrguid").val();
    data.Module_Responsibilty = $("#ModuleID").val();
    data.AssignedUserGuid = users.join(', ');
    // console.log(data);
    if (users == "") {
        bootbox.alert("Select a user to assign");
        return false;
    }
    //custom loading screen
    bootbox.hideAll();
    let loaderdialog = bootbox.dialog({
        message: '<p class="text-center mb-0"><i class="fas fa-spin fa-cog"></i>Please wait while assigning NCR to Module and a mail is being sent</p>',
        closeButton: false
    });
    $.ajax({
        url: '/NCR/SaveModuleAssignedToNCR',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            // console.log(response);
            if (response.failed === false) {
                bootbox.alert({
                    message: response.msg,
                    callback: function () {
                        window.location.reload();
                    }
                });
            }
            else {
                bootbox.alert({
                    message: response.msg,
                    callback: function () {
                        window.location.reload();
                    }
                });
            }

        },
        error: function (xhr, status, error) {
            bootbox.hideAll();
            console.error('Error:', error);
            bootbox.alert({
                message: "Failed to assign",
                callback: function () {
                    window.location.reload();
                }
            });
        }
    });
}

function ncrModuleDelete(ncrGuid, WorkFlowAssignmentGuid) {
    console.log(ncrGuid);
    bootbox.confirm({
        message: "Confirm delete this NCR Assignment ? all the remarks entered will the lost",
        buttons: {
            confirm: {
                label: 'Delete',
                className: 'btn-danger'
            },
            cancel: {
                label: 'Cancel',
                className: 'btn-secondary'
            }
        },
        callback: function (result) {
            console.log('This was logged in the callback: ' + result);
            if (result) {
                $.ajax({
                    url: '/NCR/DeleteNCRModuleAssignedData?ncrGuid=' + ncrGuid + '&WorkFlowAssignmentGuid=' + WorkFlowAssignmentGuid,
                    success: function (response) {
                        if (response.success === true) {
                            bootbox.alert({
                                message: "Deleted Successfully",
                                callback: function () {
                                    window.location.href = '/NCR/AssignedNCRList';
                                }
                            });
                        }
                        else {
                            bootbox.alert({
                                message: "Delete Failed",
                                callback: function () {
                                    bootbox.hideAll();
                                }
                            });
                        }

                    },
                });
            }
            else {
                bootbox.hideAll();
            }
        }
    });
}

function ncrItemDelete(ncrGuid, WorkFlowAssignmentGuid, ncrItemKey) {
    console.log(ncrGuid);
    bootbox.confirm({
        message: "Confirm revert this referred deviation ? the remarks entered will the removed",
        buttons: {
            confirm: {
                label: 'Revert',
                className: 'btn-danger'
            },
            cancel: {
                label: 'Cancel',
                className: 'btn-secondary'
            }
        },
        callback: function (result) {
            // console.log('This was logged in the callback: ' + result);
            if (result) {
                $.ajax({
                    url: '/NCR/DeleteModuleAssignment?ncrGuid=' + ncrGuid + '&WorkFlowAssignmentGuid=' + WorkFlowAssignmentGuid + '&ncrItemKey=' + ncrItemKey,
                    success: function (response) {
                        if (response.success === true) {
                            bootbox.alert({
                                message: "Reverted Successfully",
                                callback: function () {
                                    // bootbox.hideAll();
                                    window.location.reload();
                                }
                            });
                        }
                        else {
                            bootbox.alert({
                                message: "Failed",
                                callback: function () {
                                    bootbox.hideAll();
                                }
                            });
                        }

                    },
                });
            }
            else {
                bootbox.hideAll();
            }
        }
    });
}

function GetNCRWorkFlowData(ncrguid, action) {

    var targetElement = document.getElementById("NCRItem_" + ncrguid);
    var IconElementOpen = document.getElementById("icon-Open-" + ncrguid);
    var IconElementClose = document.getElementById("icon-Close-" + ncrguid);
    // var Ordertype = document.getElementById("generic-order-summary-Ordertype").value;

    $(IconElementOpen).toggle();
    $(IconElementClose).toggle();

    if (action == "Close") {
        $(targetElement).toggle();
        return false;
    }

    var urlinput = '/NCR/AssignedListData?NcrGuid=' + ncrguid;
    $.get(urlinput).done(function (response) {
        targetElement.children[0].innerHTML = response;
        targetElement.style.display = "";
    })
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~GD&T Symbols~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

function getGDTSymbols() {
    return [
        { name: "Straightness", symbol: "⏤" },
        { name: "Dia", symbol: "⌀" },
        { name: "Flatness", symbol: "▱" },
        { name: "Circularity", symbol: "◯" },
        { name: "Cylindricity", symbol: "⌭" },
        { name: "Perpendicularity", symbol: "⟂" },
        { name: "Parallelism", symbol: "//" },
        { name: "Symmetry", symbol: "⌯" },
        { name: "Total Runout", symbol: "⌰" },
        { name: "Concentricity", symbol: "⊙" },
        { name: "Angularity", symbol: "∠" },
        { name: "Position", symbol: "⌖" },
        { name: "Circular Runout", symbol: "↗" },
        { name: "Profile of a Surface", symbol: "⌓" },
        { name: "Profile of a Line", symbol: "⌒" },
        { name: "Datum", symbol: "⌷" },
        { name: "Maximum Material Condition (MMC)", symbol: "Ⓜ" },
        { name: "Least Material Condition (LMC)", symbol: "Ⓛ" },
        { name: "Regardless of Feature Size (RFS)", symbol: "Ⓡ" }
    ];
}

function getGDTSymbolMenuItems() {
    const symbols = getGDTSymbols();
    const menuItems = {};
    symbols.forEach((item) => {
        menuItems["insertSymbol" + item.symbol] = {
            name: item.symbol + ' ' + item.name,
            // Removed the icon property to avoid displaying an icon
        };
    });
    return menuItems;
}

function insertSymbol(symbol, inputBox) {
    const cursorPosition = inputBox.selectionStart;
    const textBeforeCursor = inputBox.value.substring(0, cursorPosition);
    const textAfterCursor = inputBox.value.substring(cursorPosition);
    inputBox.value = textBeforeCursor + symbol + textAfterCursor;
    inputBox.focus();
    inputBox.setSelectionRange(cursorPosition + symbol.length, cursorPosition + symbol.length);
  
    autoSaveRemarks(inputBox)

}

function applySymbolsContextMenu() {
    $.contextMenu({
        selector: '.DrawingDimension, .ActualDimension', // Add the Class Name to use GD&T symbols
        callback: function (key, options) {
            if (key.startsWith("insertSymbol")) {
                const symbol = key.replace("insertSymbol", ""); // Extract the symbol key
                const inputBox = options.$trigger[0];
                insertSymbol(symbol, inputBox); // Call the insertSymbol function with the symbol
            } 
        },
        items: {
            ...getGDTSymbolMenuItems()
       
        }
    });
}

function applyContextMenuForRemarks() {
    $.contextMenu({
        selector: '.serialNoRemarks', // Add the Class Name to use GD&T symbols
        callback: function (key, options) {
            if (key.startsWith("insertSymbol")) {
                const symbol = key.replace("insertSymbol", ""); // Extract the symbol key
                const inputBox = options.$trigger[0];
                insertSymbol(symbol, inputBox); // Call the insertSymbol function with the symbol
            } else if (key === "markForRework") {
                markForRework(options.$trigger[0],1);
            }
            else if (key === "markForTrialAssembly") {
                markForRework(options.$trigger[0],2);
            }
        },
        items: {
            "markForRework": {
                name: "Mark for Rework",
                icon: "action",
                className: "mark-for-rework"
            }, 
            "markForTrialAssembly": {
                name: "Mark for Trial Assembly",
                icon: "action",
                className: "mark-for-trial-assembly"
            }, 
            ...getGDTSymbolMenuItems(), 
           

        }
    });
}

// Define the custom action function
function markForRework(triggerEl, reworkType) {
   
    // Find the closest container and the textarea inside it
    const container = triggerEl.closest('.gd-t-container');
    const textarea = container.querySelector('textarea[data-field]');
    if (!textarea) {
        console.error('Textarea not found in the same container.');
        triggerEl.checked = false;
        return;
    }
    let markFor = "";
    if (reworkType == 1) {
        markFor = "Rework";
    }
    else if (reworkType == 2) {
        markFor = "Trial Assembly";
    } 
    const NcrItemKey = textarea.getAttribute('data-ncritemkey');
    const remarksType = textarea.getAttribute('data-field');
   
    bootbox.confirm({
        message: "Are you sure you want to mark this serial number for " + markFor + "?",
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
                    type: "POST",
                    url: '/NCR/MarkAsRework?remarksType=' + remarksType + '&NcrItemKey=' + NcrItemKey + '&reworkType=' + reworkType,
                    success: function (response) {
                        if (response.success == true) {
                            bootbox.alert({
                                message: "Successfully marked for " + markFor,
                                callback: function () {
                                    // bootbox.hideAll();
                                    window.location.reload();
                                }
                            });
                        }
                        else {
                           
                        }
                    },
                    error: function (error) {
                        console.error(error);
                    }
                });
            } else {
                // user cancelled → uncheck the checkbox that triggered the action
                if (triggerEl && triggerEl.matches('input[type="checkbox"]')) {
                    triggerEl.checked = false;
                }
                bootbox.hideAll();
               
                return false;
            }
        }
    });
}




function autoSaveRemarks(editor) {
    const remarksType = editor.getElement().getAttribute('data-field');
    const cleanedRemarks = editor.getContent().trim(); 
    const remarks = cleanedRemarks;
    const ncrItemKey = editor.getElement().getAttribute('data-ncrItemKey');
    if (remarks === null || $.trim(remarks) === "") {
        return
    }
    $.ajax({
        url: '/NCR/AutoSaveRemarks',
        method: 'POST', // Use POST for saving data
        data: {
            remarksType: remarksType,
            ncrItemkey: ncrItemKey,
            remarks: remarks
        },
        success: function (response) {
            if (response.success === "invalid") {
                return;
            }
            if (response.success === true) {
              //  window.location.reload();
                $(editor.getElement()).notify("Remarks Updated", {
                    className: "success",
                    autoHideDelay: 1000,
                    position: 'auto'
                });
            } else {
                $(editor.getElement()).notify("Failed", {
                    className: "danger",
                    autoHideDelay: 1500,
                });
            }
        },
        error: function (error) {
            console.error(error);
        }
    });
}

function forwardNCRtoModule(NCRGuid, moduleId, moduleName) {
    //console.log(moduleId);
   // return false;
    
    bootbox.confirm({
        message: "Are you sure you want to forward this NCR to " + moduleName + " ? Entering remarks for previous module users will be disabled",
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
                GetPopupToAssignModuleToNCR(NCRGuid, moduleId);
            } else {
                bootbox.hideAll();
                return false;
            }
        }
    });
}

function MarkUpdatingRemarksAsCompleted() {
    const classNamesToCheckValue = 'serialNoRemarks'; // Replace with your specific class names
    const classNamestoSend = 'serialNoRemarks, AfterMarksAsCompleteRemarks'; // Replace with your specific class names
    const table = document.getElementById('itemsTbl');
    const textareas = table.querySelectorAll(`textarea.${classNamesToCheckValue.replace(/, /g, ', textarea.')}`);
    if (typeof tinymce !== 'undefined') {
        tinymce.triggerSave(); // This syncs ALL TinyMCE editors to their textareas
    }

    for (const textarea of textareas) {
        const textareaId = textarea.id;
        let content = '';

        // Try to get content from TinyMCE editor first
        if (typeof tinymce !== 'undefined' && tinymce.get(textareaId)) {
            content = tinymce.get(textareaId).getContent();
        } else {
            // Fallback to textarea value
            content = textarea.value;
        }

        content = content.trim();

        // Remove HTML tags to check actual text content
        const textOnly = content.replace(/<[^>]*>/g, '').trim();

        if (textOnly === '' || content === '') {
            bootbox.alert('Please fill in remarks for all serial number available');
            return false;
        }
    }
    
    bootbox.confirm({
        message: "Are you sure you want to mark as completed? you will no longer be able to update this NCR",
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
               
                const textareas = document.querySelectorAll(`textarea.${classNamestoSend.replace(/, /g, ', textarea.')}`);

                const dataAttributes = Array.from(textareas).map(textarea => ({
                    remarksType: textarea.getAttribute('data-field'),
                    NCRItemKey: textarea.getAttribute('data-ncrItemKey'),
                    NCRWorkflowGUID: textarea.getAttribute('data-workFlowGuid'),
                    NCRGuid: textarea.getAttribute('data-NCRGuid')
                }));
              //  console.log(dataAttributes);
              //  return false;
                $.ajax({
                    type: "POST",
                    url: '/NCR/MarkUpdatingRemarksAsCompleted',
                    data: JSON.stringify(dataAttributes),
                    contentType: 'application/json',
                    success: function (response) {
                        if (response.success == true) {
                            bootbox.alert({
                                message: "Successfully marked as completed",
                                callback: function () {
                                    window.location.reload();
                                }
                            });

                        }
                        else {
                            bootbox.alert("Failed to submit!");
                        }
                    },
                    error: function (error) {
                        console.error(error);
                    }
                });
            } else {
                bootbox.hideAll();
                return false;
            }
        }
    });



  
}

function CloseNCR(NcrGuid) {
    var urlinput = "/NCR/CloseNCRData?NcrGuid=" + NcrGuid;
    $.get(urlinput).done(function (response) {

        bootbox.confirm({
            title: "Close NCR",
            message: response,
            size: 'lg',
            closeButton: true,
            buttons: {
                confirm: {
                    label: 'Save',
                    className: 'btn-success'
                }
            },
            callback: function (result) {
                if (result) {

                    // Validate ECM fields
                    var hasRework = $('#closeNcr_HasRework').val() === '1';
                    var ecmTrNo = $('#closeNcr_ECM_TR_NO').val();
                    var ecmNo = $('#closeNcr_ECM_No').val();

                    if (hasRework && (!ecmTrNo || $.trim(ecmTrNo) === '')) {
                        bootbox.alert('ECM TR No. is required when Rework/Trial Assembly exists');
                        return false;
                    }

                    if (!ecmNo || $.trim(ecmNo) === '') {
                        bootbox.alert('ECM No. is required to close the NCR');
                        return false;
                    }
                    var reportStatus = $('#closeNcr_ReportStatus').val();
                    if (!reportStatus || reportStatus === '') {
                        bootbox.alert('Report Status is required');
                        return false;
                    }


                    // Validate all statuses
                    var allStatusesValid = true;
                    var items = [];

                    $('.ncrItemKey').each(function (index) {
                        var ncrItemKey = $(this).val();
                        var ncrStatus = $('.ncrStatus').eq(index).val();
                        var serialNumber = $('.SerialNumber').eq(index).val();

                        if (ncrStatus === '') {
                            bootbox.alert('Select a valid status for all serial numbers');
                            allStatusesValid = false;
                            return false;
                        }

                        items.push({
                            NCRItemKey: parseInt(ncrItemKey),
                            Status: ncrStatus,
                            SerialNumber: serialNumber
                        });
                    });

                    if (!allStatusesValid) {
                        return false;
                    }

                    // Build the post model
                    var postData = {
                        NCRGuid: NcrGuid,
                        ECM_TR_NO: hasRework ? $.trim(ecmTrNo) : null,
                        ECM_No: $.trim(ecmNo),
                        ReportStatus: reportStatus,
                        Items: items
                    };

                    $.ajax({
                        type: "POST",
                        url: '/NCR/CloseNCR',
                        contentType: 'application/json',
                        data: JSON.stringify(postData),
                        success: function (response) {
                            if (response.success == true) {
                                bootbox.alert({
                                    message: "Successfully closed the NCR",
                                    callback: function () {
                                        bootbox.hideAll();
                                        window.location.reload();
                                    }
                                });
                            }
                            else {
                                bootbox.alert(response.msg || "Failed to submit!");
                            }
                        },
                        error: function (error) {
                            console.error(error);
                            bootbox.alert("An error occurred while closing the NCR");
                        }
                    });
                } else {
                    bootbox.hideAll();
                    return false;
                }
            }
        });
    });
}



function FillRawMaterial() {
    var dbkey = document.getElementById("nonConformanceReportVM_Engine_Part_Dbkey").value;
    $.ajax({
        url: '/NCR/GetRawMaterialOfPart',  // Replace 'ControllerName' with your actual controller name
        type: 'GET',
        data: { EnginePartDbkey: dbkey },
        success: function (response) {

            // Parse the JSON response
            const parsedData = JSON.parse(response);
            const rawMaterialValue = parsedData[0].Raw_Material;
            const revisionValue = parsedData[0].Revision;

            console.log(rawMaterialValue);

            // Update the Raw Material field
            $('#nonConformanceReportVM_RawMaterial').val(rawMaterialValue).trigger('change');

            // Update the select2 dropdown
            var rawMaterialDropdown = $('select[data-field="RawMaterial"]');
            rawMaterialDropdown.val(rawMaterialValue).trigger('change');

            // Uncomment if you want to set the revision value
             document.getElementById("nonConformanceReportVM_Revision").value = revisionValue;
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
        }
    });
}

function printNCRReport(NcrGuid) {
   
   var url = "/NCR/PrintNCRModule?NCRGuid=" + NcrGuid;
    window.open(url, '_blank');

}

function updateRemarksAfterMarkAsComplete(element, ncrItemKey) {
    const reamrksType = element.getAttribute('data-field');
    const remarks = element.value;
    if (remarks === null || $.trim(remarks) == "") {
        return;
    }
    $.ajax({
        url: '/NCR/UpdateRemarksAfterMarkAsComplete?remarksType=' + reamrksType + '&ncrItemkey=' + ncrItemKey + '&remarks=' + remarks,
        success: function (response) {
            if (response.success === "invalid") {
                return;
            }
            if (response.success == true) {
                window.location.reload();
                $(element).notify("Remarks Updated", {
                    className: "success",
                    autoHideDelay: 1000,
                    position: 'right'
                });

            }
            else {
                $(element).notify("Failed", {
                    className: "danger",
                    autoHideDelay: 1500,
                });
            }
        },
        error: function (error) {
            console.error(error);
        }
    });

}

function MarkSerialNoStatus(NcrGuid) {
    var ncrStatusData = [];
    var ncrRowData = {};
    var urlinput = "/NCR/CloseNCRData?NcrGuid=" + NcrGuid;
    $.get(urlinput).done(function (response) {
       
        var keys = document.getElementsByClassName('ncrItemKey');
        // Get all the select elements
        var statuses = document.getElementsByClassName('ncrStatus');

        // Iterate through the rows and log the values

        bootbox.confirm({
            title: "Mark NCR Serial Number Status",
            message: response,
            size: 'lg',
            closeButton: true,
            buttons: {
                confirm: {
                    label: 'Save',
                    className: 'btn-success'
                }
            },
            callback: function (result) {
                if (result) {
                   
                    var allStatusesValid = true; // Flag to track status validity

                    $('.ncrItemKey').each(function (index) {
                        var ncrItemKey = $(this).val(); // Get the hidden input value
                        var ncrStatus = $('.ncrStatus').eq(index).val(); // Get the corresponding select value
                        if (ncrStatus === '') {
                            bootbox.alert('Select a valid status for all serial numbers');
                            allStatusesValid = false;
                            return false; // Exit the loop if status is not valid
                        }
                        ncrRowData = {
                            NCRGuid: NcrGuid,
                            NCRItemKey: ncrItemKey,
                            Status: ncrStatus
                        }
                        ncrStatusData.push(ncrRowData);
                    });
                    if (!allStatusesValid) {
                        return false;
                    }

                    $.ajax({
                        type: "POST",
                        url: '/NCR/CloseNCR',
                        contentType: 'application/json',
                        data: JSON.stringify(ncrStatusData),
                        success: function (response) {
                            if (response.success == true) {
                                bootbox.alert({
                                    message: "Successfully closed the NCR",
                                    callback: function () {
                                        bootbox.hideAll();
                                        window.location.reload();
                                    }
                                });

                            }
                            else {
                                bootbox.alert("Failed to submit!");
                            }
                        },
                        error: function (error) {
                            console.error(error);
                        }
                    });
                } else {
                    bootbox.hideAll();
                    return false;
                }
            }
        });
    });
}


 

// Function to open the mapping modal
function openSerialEngineMapping() {
    

    var ncrGUID = $("#NCRGuid").val();
    console.log(ncrGUID);
    if (!ncrGUID || ncrGUID === '') {
        alert('Please save the NCR first before mapping serial numbers to engines.');
        return;
    }

    // Load the partial view with mapping data
    $.ajax({
        url: '/NCR/SerialNumberEngineMapping',
        type: 'GET',
        data: { NCRGUID: ncrGUID },
        success: function (response) {
            // Remove existing modal if any
            $('#serialEngineMapModal').remove();

            // Append new modal to body
            $('body').append(response);

            // Show the modal
            var myModal = new bootstrap.Modal(document.getElementById('serialEngineMapModal'));
            myModal.show();
        },
        error: function (xhr, status, error) {
            console.error('Error loading mapping data:', error);
            alert('Error loading serial number mapping. Please try again.');
        }
    });
}

// Function to save the mappings  
function saveSerialEngineMapping() {
    var mappings = [];

    // Collect all mappings from the table
    $('#mappingTableBody tr').each(function () {
        var id = $(this).find('.mapping-id').val();
        var serialNumber = $(this).find('.mapping-serial-input').val();
        var engine = $(this).find('.mapping-engine-select').val();

        if (serialNumber && serialNumber.trim() !== '') {
            mappings.push({
                Id: parseInt(id) || 0,
                SerialNumber: serialNumber.trim(),
                Engine: engine || ''
            });
        }
    });

    if (mappings.length === 0) {
        alert('No mappings to save.');
        return;
    }

    // Disable save button to prevent double-click
    var saveBtn = $('#saveMappingBtn');
    saveBtn.prop('disabled', true).html('<i class="bi bi-hourglass-split"></i> Saving...');
    var ncrGUID = $("#NCRGuid").val();
   
    // Send to server
    $.ajax({
        url: '/NCR/SaveSerialNumberEngineMapping',
        type: 'POST',
        data: {
            NCRGUID: ncrGUID,
            mappingData: JSON.stringify(mappings)
        },
        success: function (response) {
            if (response.success) {
                // Show success message
                alert('Mapping saved successfully!');

                // Close modal
                var modal = bootstrap.Modal.getInstance(document.getElementById('serialEngineMapModal'));
                modal.hide();

                // Refresh the pills display
                loadMappingPills();
            } else {
                alert('Error: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('Save error:', error);
            alert('Error saving mappings. Please try again.');
        },
        complete: function () {
            // Re-enable save button
            saveBtn.prop('disabled', false).html('<i class="bi bi-save"></i> Save Mapping');
        }
    });
}

function loadMappingPills() {
    var ncrGUID = $("#NCRGuid").val();
    if (!ncrGUID || ncrGUID === '' || ncrGUID === '00000000-0000-0000-0000-000000000000') {
       // $('#mappingPillsContainer').html('<span class="text-muted"><em>Save NCR first to view mappings.</em></span>');
        return;
    }

    $.ajax({
        url: '/NCR/SerialNumberEngineMapping',
        type: 'GET',
        data: { NCRGUID: ncrGUID },
        success: function (response) {
            // Parse the response to extract data
            var $response = $(response);
            var hasMappings = false;
            var pillsHtml = '';

            // Extract data from table rows in the response
            $response.find('#mappingTableBody tr').each(function () {
                var serialNumber = $(this).find('.mapping-serial-input').val();
                var engine = $(this).find('.mapping-engine-select').val();

                if (serialNumber && engine && engine !== '' && engine !== 'Select Engine') {
                    hasMappings = true;
                    pillsHtml += `
                        <div class="mapping-pill">
                            <span class="serial-part">${serialNumber}</span>
                            <span class="separator">→</span>
                            <span class="engine-part">${engine}</span>
                        </div>
                    `;
                }
            });

            if (hasMappings) {
                $('#mappingPillsContainer').html(pillsHtml);
            } else {
                $('#mappingPillsContainer').html('<span class="text-muted"><em>Right click on serial number input element to map serial number with engine</em></span>');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error loading pills:', error);
            $('#mappingPillsContainer').html('<span class="text-danger"><em>Error loading mappings.</em></span>');
        }
    });
}


function markAsAccepted(checkbox, ncrItemKey, remarksType) {
    var $container = $(checkbox).closest('.gd-t-container');
    var $textarea = $container.find('textarea[data-field="' + remarksType + '"]');
    var textareaId = $textarea.attr('id');

    $.ajax({
        url: '/NCR/MarkAsAcceptedUnderConcession',
        method: 'POST',
        data: {
            remarksType: remarksType,
            ncrItemKey: ncrItemKey
        },
        success: function (response) {
            if (response.success === true) {
                var displayText = response.finalRemarks || 'Accepted under concession';

                var editor = tinymce.get(textareaId);
                if (editor) {
                    editor.setContent(displayText);
                    editor.save();
                } else {
                    $textarea.val(displayText);
                }

                var notifyTarget = editor ? $(editor.getElement()) : $textarea;
                notifyTarget.notify("Remarks Updated", {
                    className: "success",
                    autoHideDelay: 1000,
                    position: "auto"
                });
                $(checkbox).prop('checked', false);
            } else {
                $(checkbox).prop('checked', false);
                $textarea.notify(response.msg || "Failed to update remarks", {
                    className: "danger",
                    autoHideDelay: 1500
                });
            }
        },
        error: function (error) {
            console.error(error);
            $(checkbox).prop('checked', false);
            $textarea.notify("Error while saving", {
                className: "danger",
                autoHideDelay: 1500
            });
        }
    });
}

function deleteNcr(ncrGuid) {
    console.log(ncrGuid);
    if (!ncrGuid) {
        alert('Invalid NCR Guid.');
        return;
    }

    if (!confirm('Are you sure you want to delete this NCR?')) {
        return;
    }

    $.ajax({
        url: '/NCR/DeleteNcr',  
        type: 'POST',
        data: { ncrGUid: ncrGuid },        
        success: function (result) {
            
            alert(result.message);

            if (result.success) { 
                window.location.reload();             
            }
        },
        error: function (xhr, status, error) {
            alert('Error while deleting NCR: ' + error);
        }
    });
}

// New function to handle checkbox state changes (mark/unmark)
function handleReworkCheckbox(triggerEl, reworkType) {

    // Find the closest container and the textarea inside it
    const container = triggerEl.closest('.gd-t-container');
    const textarea = container ? container.querySelector('textarea[data-field]') : null;

    if (!textarea) {
        console.error('Textarea not found in the same container.');
        triggerEl.checked = !triggerEl.checked; // Revert checkbox state
        return;
    }

    const NcrItemKey = textarea.getAttribute('data-ncritemkey');
    const remarksType = textarea.getAttribute('data-field');
    const isChecked = triggerEl.checked;

    // Determine action
    let action = "";
    let markFor = "";
    if (reworkType == 1) {
        markFor = "Rework";
        action = isChecked ? "mark" : "unmark";
    } else if (reworkType == 2) {
        markFor = "Trial Assembly";
        action = isChecked ? "mark" : "unmark";
    }

    // Show confirmation dialog
    let confirmMessage = action === "mark"
        ? `Are you sure you want to mark this serial number for ${markFor}?`
        : `Are you sure you want to unmark this serial number from ${markFor}?`;

    bootbox.confirm({
        message: confirmMessage,
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
                // User confirmed
                if (action === "mark") {
                    // Call the existing MarkAsRework endpoint
                    markReworkItem(NcrItemKey, remarksType, reworkType, triggerEl);
                } else {
                    // Call the new UndoReworkMarking endpoint
                    undoReworkItem(NcrItemKey, triggerEl);
                }
            } else {
                // User cancelled → revert checkbox state
                triggerEl.checked = !triggerEl.checked;
                bootbox.hideAll();
            }
        }
    });
}

// Function to mark item for rework/trial assembly
function markReworkItem(NcrItemKey, remarksType, reworkType, triggerEl) {
    let markFor = reworkType == 1 ? "Rework" : "Trial Assembly";

    // Get the stage from the checkbox data attribute
    const stage = triggerEl.getAttribute('data-stage');

    // Validate stage exists
    if (!stage) {
        console.error('Stage not found on checkbox element');
        triggerEl.checked = false;
        bootbox.alert("Error: Stage information missing");
        return;
    }

    $.ajax({
        type: "POST",
        url: '/NCR/MarkAsRework',
        data: {
            remarksType: remarksType,
            NcrItemKey: NcrItemKey,
            reworkType: reworkType,
            stage: stage
        },
        success: function (response) {
            if (response.success == true) {

                // Find other checkbox in same row and uncheck it (mutually exclusive)
                const otherCheckboxType = reworkType == 1 ? 'trial-checkbox' : 'rework-checkbox';
                const otherCheckbox = document.querySelector(`#${otherCheckboxType.replace('-checkbox', '')}-checkbox-${NcrItemKey}`);
                if (otherCheckbox) {
                    otherCheckbox.checked = false;
                }

                bootbox.alert({
                    message: "Successfully marked for " + markFor,
                    callback: function () {
                        window.location.reload();
                    }
                });
            } else {
                // Revert checkbox on failure
                triggerEl.checked = false;
                bootbox.alert("Failed to mark for " + markFor);
            }
        },
        error: function (error) {
            console.error(error);
            // Revert checkbox on error
            triggerEl.checked = false;
            bootbox.alert("Error while marking for " + markFor);
        }
    });
}

// Function to undo rework/trial assembly marking
function undoReworkItem(NcrItemKey, triggerEl) {
    const stage = triggerEl.getAttribute('data-stage');

    // Validate stage exists
    if (!stage) {
        console.error('Stage not found on checkbox element');
        triggerEl.checked = !triggerEl.checked; // Revert checkbox state
        bootbox.alert("Error: Stage information missing");
        return;
    }

    $.ajax({
        type: "POST",
        url: '/NCR/UndoReworkMarking',
        data: {
            NcrItemKey: NcrItemKey,
            stage: stage 
        },
        success: function (response) {
            if (response.success == true) {
                bootbox.alert({
                    message: "Successfully unmarked",
                    callback: function () {
                        window.location.reload();
                    }
                });
            } else {
                // Revert checkbox on failure
                triggerEl.checked = true;
                bootbox.alert("Failed to unmark: " + (response.msg || "Unknown error"));
            }
        },
        error: function (error) {
            console.error(error);
            // Revert checkbox on error
            triggerEl.checked = true;
            bootbox.alert("Error while unmarking");
        }
    });
}

function updateReportStatusPreview() {
    var allSelected = true;
    var allCleared = true;
    var allRejected = true;
    var anyRejected = false;

    $('.ncrStatus').each(function () {
        var val = $(this).val();
        if (!val || val === '') {
            allSelected = false;
            return;
        }
        if (!val.toLowerCase().startsWith('cleared')) {
            allCleared = false;
        }
        if (val.toLowerCase() !== 'rejected') {
            allRejected = false;
        }
        if (val.toLowerCase() === 'rejected') {
            anyRejected = true;
        }
    });

    if (allSelected) {
        if (allCleared) {
            $('#closeNcr_ReportStatus').val('Cleared');
        } else if (allRejected) {
            $('#closeNcr_ReportStatus').val('Rejected');
        } else if (anyRejected) {
            $('#closeNcr_ReportStatus').val('Processed - Partially Cleared');
        } else {
            $('#closeNcr_ReportStatus').val('Cleared');
        }
    }
}