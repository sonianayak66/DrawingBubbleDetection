
function GetAcsnList() {
    $.ajax({
        url: '/ACSN/AcsnList',
        type: 'GET',
        success: function (data) {
            document.getElementById("tab-acsn-list").innerHTML = data;
            ApplyDatatable();
        }
    });
}

function ApplyDatatable() {
    var Windowheight = window.innerHeight;
    var x = $("#TblAcsnList").position();
    var tableHeight = (Windowheight - x.top) * 0.75;
  
    $('#TblAcsnList').DataTable({
        "paging": false,
        order: [[0, 'desc']],
        scrollY: tableHeight + 'px',
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
                extend: 'searchPanes',
                // className: 'btn btn-primary btn-sm',
                text: 'Filter',
                config: {
                    //columns: [1],
                    cascadePanes: true
                }
            },
            {
                extend: 'excel'
            }
        ]
    });
}

function GetDashborad() {
    $.ajax({
        url: '/ACSN/Dashbord',
        type: 'GET',
        success: function (data) {
            document.getElementById("tab-profile").innerHTML = data;
            GetSummery();
            GetStepSummary();
        }
    });
}

function GetStepSummary() {
    $.ajax({
        url: '/ACSN/ACSNStatusSummary',
        type: 'GET',
        success: function (data) {
            document.getElementById("StatusSummary").innerHTML = data;
        }
    });
}


function StatusUpdatePopUP(acsnKey) {
    $.ajax({
        url: '/ACSN/Status/' + acsnKey,
        type: 'GET',
        success: function (data) {
            bootbox.alert({
                message: data,
                title: "Status",
                size: 'Small',
                buttons: {
                    ok: {
                        className: 'd-none'
                    }
                }
            });
        }
    });
}

function SubmitItemStatus(btn) {
    var form = $(btn).closest("form"); 
    $.validator.unobtrusive.parse("#" + form.attr("id"));
    $(form).validate();
    if (form.valid() == false) {
        return false;
    }
    var formData = {};
    formData.isActiveStatus = $(form).find("#item_isActiveStatus").is(':checked');
    formData.acsnStatus = $(form).find("#item_acsnStatus").val();
    formData.ACSNStatusKey = $(form).find("#item_ACSNStatusKey").val();
    formData.StartDate = $(form).find("#item_StartDate").val();
    formData.EndDate = $(form).find("#item_EndDate").val();
    formData.Remarks = $(form).find("#item_Remarks").val();

    $.ajax({
        type: "POST",
        url: '/ACSN/ACSNStatus',
        data: {
            ACSNStatus: JSON.stringify(formData),
        },
        cache: false,
        success: function (data) {
            if (data.success) {
                bootbox.alert('Saved Successfully');
            }
        }
    });


}


function DeleteACSN(acsnKey) {
    var acsnDeleteDlg = bootbox.confirm({
        message: "Are you sure you want to delete this ACSN?",
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
                    url: '/ACSN/Delete?acsnKey=' + acsnKey,
                    type: 'POST',
                    success: function (data) {
                        console.log(data);
                        bootbox.alert(data.msg);

                        if (data.success) {
                            GetAcsnList();
                        }
                    },
                    error: function () {
                        bootbox.alert("Something went wrong while calling the server.");
                    }
                });
            }
        }
    });
}

function onlyUnique(value, index, array) {
    return array.indexOf(value) === index;
}

function GetSummery() {

    var AcsnSeries = new Array();
    var ACSNs = new Array();
    var AgeRange = new Array();

    $.getJSON("/ACSN/DashboardSummary", function (data) {
        console.log(data);
        var seriesData = [];
        AcsnSeries = $.unique(data.map(function (d) { return d.AcsnSeries; }));
        AcsnSeries = AcsnSeries.filter(onlyUnique);
        AgeRange = $.unique(data.map(function (d) { return d.AgeRange; }));
        AgeRange = AgeRange.filter(onlyUnique);

        $.each(AcsnSeries, function (index, series) {
            var seriesItem = {};
            seriesItem.name = series;
            seriesItem.data = [];
            $.each(AgeRange, function (index, range) {
                var dataItem = data.filter(x => x.AcsnSeries == series && x.AgeRange == range).map(x => x.ACSNs);
                dataItem = dataItem.length == 0 ? dataItem = 0 : dataItem;
                seriesItem.data.push(dataItem);
            })
            seriesData.push(seriesItem);
        })

        Highcharts.chart('container', {
            chart: {
                type: 'bar'
            },
            credits: {
                enabled: false
            },
            title: {
                text: 'ACSNs by Age'
            },
            xAxis: {
                categories: AgeRange
            },
            yAxis: {
                min: 0,
                title: {
                    text: 'Number of ACSN files'
                }
            },
            legend: {
                reversed: true
            },
            plotOptions: {
                series: {
                    stacking: 'normal',
                    dataLabels: {
                        enabled: true
                    }
                }
            },
            series: seriesData
        });

    })
}

