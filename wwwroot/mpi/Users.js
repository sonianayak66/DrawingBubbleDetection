$(document).ready(function () {
    GetUserList();
});


function GetRoles() {
    $.ajax({
        url: '/Users/RoleList',
        type: 'GET',
        success: function (data) {
            document.getElementById("cardData").innerHTML = data;
        }
    });
}
function GetUserList() {
    $.ajax({
        url: '/Users/UserList',
        type: 'GET',
        success: function (data) {
            document.getElementById("cardData").innerHTML = data;
            ApplyDataTableSearch();
        }
    });
}

function GetUserListInactive() {
    $.ajax({
        url: '/Users/InactiveUserList',
        type: 'GET',
        success: function (data) {
            document.getElementById("cardData").innerHTML = data;
            ApplyDataTableSearch();
        }
    });
}
//function ApplyDataTableSearch() {
//    $('#userlist').DataTable({
//        paging: false,
//        searching: true,
//        info: true,
//    });
//}
function ApplyDataTableSearch() {
    if (!$('#userlist').length) return;

    if ($.fn.DataTable.isDataTable('#userlist')) {
        $('#userlist').DataTable().destroy();
    }

    $('#userlist').DataTable({
        scrollY: 'calc(100vh - 330px)',
        scrollCollapse: true,
        scrollX: true,
        paging: false,
        lengthChange: false,
        searching: true,
        info: false,
        autoWidth: false,
        dom: '<"d-flex justify-content-end"f>rt',
        columnDefs: [
            { targets: -1, orderable: false, searchable: false }
        ],
        initComplete: function () {
            this.api().columns.adjust();
        }
    });
}



function GetRolePermissions(Role) {
    $.ajax({
        url: '/Users/Permissions?roleID=' + Role,
        type: 'GET',
        success: function (data) {
            document.getElementById("div_rolepermissions").innerHTML = data;
        }
    });
}

function SubmitRole() {
    $.validator.unobtrusive.parse("form");
    var validator = $("#roleform").validate();
    if ($("#roleform").valid()) {
        var data = $("#roleform").serialize();
        $.ajax({
            type: 'POST',
            url: '/Users/ManageRole',
            data: data,
            success: function (data) {
                if (data.success) {
                    bootbox.alert({
                        message: data.msg,
                        callback: function () {
                            var btn = document.getElementsByClassName("bootbox-close-button")[0];
                            btn.click();
                        }
                    });
                    GetRoles();
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

function AddRoles(ID) {
    var url = '/Users/ManageRole?ID=' + ID;
    $.ajax({
        url: url,
        type: 'GET',
        processData: false,
        contentType: false,
        success: function (data) {
            var dialog = bootbox.dialog({
                centerVertical: true,
                message: data,
                size: 'small'
            })
        },
    });
}

function SubmitRolePermisions() {
    var permissions = document.getElementsByClassName("Permission_Switch");
    var aspNetRoleClaim = [];
    for (var i = 0; i < permissions.length; i++) {
        if (permissions[i].checked) {
            var permission = {};
            permission.RoleId = document.getElementById("RoleId").value;
            permission.ClaimType = permissions[i].id;
            permission.ClaimValue = permissions[i].id;
            aspNetRoleClaim.push(permission);
        }
    }

    
    $.ajax({
        type: 'POST',
        url: '/Users/PermissionsSave',
        data: JSON.stringify(aspNetRoleClaim),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.success) {
                bootbox.alert({
                    message: data.msg,
                    callback: function () {
                       
                    }
                });
            } else {
                alert(data.msg);
            }
        },
        error: function (jqXHR, exception) {
            alert(exception);
        }
    })
    
}

// add search functionality to the table tblpermissions 

   
function SearchPermission(ctrl) {
    // Delegate to combined filter so search + assigned-only toggle work together
    ApplyPermissionFilters();
};

// ===== Bulk Role Assignment Functions =====

function GetBulkRoleAssignment() {
    $.ajax({
        url: '/Users/BulkRoleAssignment',
        type: 'GET',
        success: function (data) {
            document.getElementById("cardData").innerHTML = data;
            InitBulkRoleTable();
        }
    });
}
//Add
var _bulkDataTable = null;

// Hook custom filter into DataTable only once
$.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
    if (settings.nTable.id !== 'bulkRoleTable') return true;
    if (!_bulkDataTable) return true;

    var row = $(_bulkDataTable.row(dataIndex).node());
    if (row.length === 0) return true;

    var rowDept = (row.attr('data-department') || '').toLowerCase().trim();
    var rowType = (row.attr('data-persontype') || '').toLowerCase().trim();

    var selectedDept = ($('#filterDepartment').val() || '').toLowerCase().trim();

    var checkedTypes = [];
    $('.person-type-filter:checked').each(function () {
        checkedTypes.push(($(this).val() || '').toLowerCase().trim());
    });

    // Department filter
    if (selectedDept !== '' && rowDept !== selectedDept) {
        return false;
    }

    // Employee Type filter
    // If no checkbox checked, show all
    if (checkedTypes.length > 0 && checkedTypes.indexOf(rowType) === -1) {
        return false;
    }

    return true;
});

//function InitBulkRoleTable() {
//    if ($.fn.DataTable.isDataTable('#bulkRoleTable')) {
//        $('#bulkRoleTable').DataTable().destroy();
//    }

//    _bulkDataTable = $('#bulkRoleTable').DataTable({
//        paging: false,
//        searching: true,
//        info: true,
//        fixedHeader: true,
//        columnDefs: [
//            { orderable: false, targets: [0, 6] }
//        ]
//    });

//    $('.role-multiselect').select2({
//        placeholder: 'Select roles',
//        width: '100%',
//        allowClear: true
//    });
//}
function InitBulkRoleTable() {
    if (!$('#bulkRoleTable').length) return;

    if ($.fn.DataTable.isDataTable('#bulkRoleTable')) {
        $('#bulkRoleTable').DataTable().destroy();
    }

    _bulkDataTable = $('#bulkRoleTable').DataTable({
        scrollY: 'calc(100vh - 360px)',
        scrollCollapse: true,
        scrollX: true,
        paging: false,
        lengthChange: false,
        searching: true,
        info: false,
        autoWidth: false,
        dom: '<"d-flex justify-content-end"f>rt',
        order: [[1, 'asc']],   // sort by Name
        columnDefs: [
            { orderable: false, targets: [0, 6] }
        ],
        initComplete: function () {
            this.api().columns.adjust();
        }
    });

    $('.role-multiselect').select2({
        placeholder: 'Select roles',
        width: '100%',
        allowClear: true
    });
}

function ApplyBulkFilters() {
    if (_bulkDataTable) {
        _bulkDataTable.draw();
    }
}

function ClearBulkFilters() {
    $('#filterDepartment').val('');
    $('.person-type-filter').prop('checked', false);
    ApplyBulkFilters();
}
//end

function SelectAllBulkCheckboxes(masterCheckbox) {
    // Only select visible (filtered) rows
    var rows = $('#bulkRoleTable tbody tr:visible');
    rows.find('.bulk-user-checkbox').prop('checked', masterCheckbox.checked);
}

function ApplyRoleToSelected() {
    var roleId = $('#bulkRoleApply').val();
    var roleName = $('#bulkRoleApply option:selected').text();
    if (!roleId) {
        bootbox.alert('Please select a role to apply.');
        return;
    }

    var checkedBoxes = $('.bulk-user-checkbox:checked');
    if (checkedBoxes.length === 0) {
        bootbox.alert('Please select at least one user.');
        return;
    }

    checkedBoxes.each(function () {
        var personGuid = $(this).data('personguid');
        var selectEl = $('select.role-multiselect[data-personguid="' + personGuid + '"]');
        var currentVals = selectEl.val() || [];
        if (currentVals.indexOf(roleId) === -1) {
            currentVals.push(roleId);
            selectEl.val(currentVals).trigger('change');
        }
    });

    bootbox.alert('Role "' + roleName + '" added to ' + checkedBoxes.length + ' selected user(s).<br/><br/><strong>Note:</strong> Please click <strong>Save Changes</strong> once the role assignment is complete to apply the updates.');
}

function RemoveRoleFromSelected() {
    var roleId = $('#bulkRoleApply').val();
    var roleName = $('#bulkRoleApply option:selected').text();
    if (!roleId) {
        bootbox.alert('Please select a role to remove.');
        return;
    }

    var checkedBoxes = $('.bulk-user-checkbox:checked');
    if (checkedBoxes.length === 0) {
        bootbox.alert('Please select at least one user.');
        return;
    }

    checkedBoxes.each(function () {
        var personGuid = $(this).data('personguid');
        var selectEl = $('select.role-multiselect[data-personguid="' + personGuid + '"]');
        var currentVals = selectEl.val() || [];
        var idx = currentVals.indexOf(roleId);
        if (idx > -1) {
            currentVals.splice(idx, 1);
            selectEl.val(currentVals).trigger('change');
        }
    });

    bootbox.alert('Role "' + roleName + '" removed from ' + checkedBoxes.length + ' selected user(s).<br/><br/><strong>Note:</strong> Please click <strong>Save Changes</strong> to apply the updates.');
}

function SaveBulkRoleAssignment() {
    var userRoleData = [];
    $('select.role-multiselect').each(function () {
        var personGuid = $(this).data('personguid');
        var selectedRoles = $(this).val() || [];
        // Include all users (even those with no roles selected, to handle removals)
        userRoleData.push({
            PersonGUID: personGuid,
            RoleIds: selectedRoles
        });
    });

    if (userRoleData.length === 0) {
        bootbox.alert('No user data found.');
        return;
    }

    // Confirm before saving
    bootbox.confirm({
        message: 'Are you sure you want to save the role changes?',
        buttons: {
            confirm: { label: 'Yes, Save', className: 'btn-primary' },
            cancel: { label: 'Cancel', className: 'btn-secondary' }
        },
        callback: function (result) {
            if (result) {

                $.ajax({
                    type: 'POST',
                    url: '/Users/SaveBulkRoleAssignment',
                    data: JSON.stringify(JSON.stringify(userRoleData)),
                    dataType: "json",
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {

                        if (data.success) {
                            bootbox.alert({
                                message: data.msg,
                                callback: function () {
                                    GetBulkRoleAssignment();
                                }
                            });
                        } else {
                            bootbox.alert('Error: ' + data.msg);
                        }
                    },
                    error: function (jqXHR, exception) {

                        bootbox.alert('Error saving roles: ' + exception);
                    }
                });
            }
        }
    });
}

// ===== End Bulk Role Assignment Functions =====

// Search roles list
function SearchRoles(ctrl) {
    var value = ($(ctrl).val() || '').toLowerCase().trim();
    $("#tblRoles tr").each(function () {
        var roleName = ($(this).attr('data-rolename') || '').toLowerCase();
        $(this).toggle(value === '' || roleName.indexOf(value) > -1);
    });
}

// Filter to show only assigned (checked) permissions — works together with search
function FilterAssignedPermissions(ctrl) {
    ApplyPermissionFilters();
}

// Combined filter: search + assigned-only toggle
function ApplyPermissionFilters() {
    var input = ($('#search').val() || '').toLowerCase().trim();
    var terms = input.split(/\s+/).filter(function (t) { return t.length > 0; });
    var assignedOnly = document.getElementById('assignedOnlyToggle')?.checked || false;

    $("#tblpermissions tr").each(function () {
        var row = $(this);
        var show = true;

        // Assigned-only filter — check the actual DOM checked property
        if (assignedOnly) {
            var cb = row.find('.Permission_Switch')[0];
            if (!cb || !cb.checked) {
                show = false;
            }
        }

        // Search filter
        if (show && terms.length > 0) {
            var module = (row.attr('data-module') || '').toLowerCase();
            var description = (row.attr('data-description') || '').toLowerCase();
            var permKey = (row.attr('data-permkey') || '').toLowerCase();
            var searchText = module + ' ' + description + ' ' + permKey;

            for (var i = 0; i < terms.length; i++) {
                if (searchText.indexOf(terms[i]) === -1) {
                    show = false;
                    break;
                }
            }
        }

        row.toggle(show);
    });
}

// Select All checkboxes with classname Permission_Switch on tblpermissions where rows are visible
function SelectAll(ctrl) {
    var value = $(ctrl).prop("checked");
    $("#tblpermissions tr:visible").filter(function () {
        $(this).find("input[type=checkbox]").prop("checked", value);
    });
}


