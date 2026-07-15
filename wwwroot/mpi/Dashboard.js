// =============================================
// Manufacturing & Quality Section - Chart Functions
// =============================================
 
function renderModulePartsChart(data, canvasId = 'modulePartsChart') {
    var canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.error('Canvas element not found: ' + canvasId);
        return;
    }

    var ctx = canvas.getContext('2d');

    // Destroy existing chart if any
    if (window.modulePartsChartInstance) {
        window.modulePartsChartInstance.destroy();
    }

    window.modulePartsChartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: data.map(x => x.moduleName || 'Not Assigned'),
            datasets: [{
                label: 'Parts Count',
                data: data.map(x => x.partsCount),
                backgroundColor: 'rgba(54, 162, 235, 0.6)',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 1,
                borderRadius: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return 'Parts: ' + context.parsed.y;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        precision: 0
                    }
                }
            }
        }
    });
}

 
function renderNCRStageChart(data, canvasId = 'ncrStageChart') {
    var canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.error('Canvas element not found: ' + canvasId);
        return;
    }

    var ctx = canvas.getContext('2d');

    // Destroy existing chart if any
    if (window.ncrStageChartInstance) {
        window.ncrStageChartInstance.destroy();
    }

    window.ncrStageChartInstance = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: data.map(x => x.stage),
            datasets: [{
                data: data.map(x => x.ncrCount),
                backgroundColor: [
                    'rgba(255, 99, 132, 0.7)',
                    'rgba(54, 162, 235, 0.7)',
                    'rgba(255, 206, 86, 0.7)',
                    'rgba(75, 192, 192, 0.7)',
                    'rgba(153, 102, 255, 0.7)',
                    'rgba(255, 159, 64, 0.7)'
                ],
                borderColor: '#fff',
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    position: 'right',
                    labels: {
                        padding: 15,
                        font: {
                            size: 11
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            var label = context.label || '';
                            var value = context.parsed || 0;
                            var total = context.dataset.data.reduce((a, b) => a + b, 0);
                            var percentage = ((value / total) * 100).toFixed(1);
                            return label + ': ' + value + ' (' + percentage + '%)';
                        }
                    }
                }
            }
        }
    });
}

 
function initializeCollapsibleTables() {
    document.querySelectorAll('[data-bs-toggle="collapse"]').forEach(function (element) {
        // Remove existing listeners to avoid duplicates
        var newElement = element.cloneNode(true);
        element.parentNode.replaceChild(newElement, element);

        newElement.addEventListener('click', function () {
            var icon = this.querySelector('.collapse-icon');
            if (icon) {
                icon.classList.toggle('uil-angle-down');
                icon.classList.toggle('uil-angle-up');
            }
        });
    });
}

 
function initializeManufacturingQualityCharts(chartData) {
    console.log('chart');
    if (chartData.modulePartsData && chartData.modulePartsData.length > 0) {
        renderModulePartsChart(chartData.modulePartsData);
    }

    if (chartData.ncrStageData && chartData.ncrStageData.length > 0) {
        renderNCRStageChart(chartData.ncrStageData);
    }

    initializeCollapsibleTables();
}
 
function initProcurementStatusChart(canvasId, statusData) {
    try {
        var canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error('Canvas element not found:', canvasId);
            return;
        }

        var ctx = canvas.getContext('2d');

        // Generate colors based on status keywords
        var statusColors = statusData.map(function (item) {
            var status = item.currentStatus ? item.currentStatus.toLowerCase() : '';

            // Color mapping based on status type
            if (status.includes('approval') || status.includes('pending')) {
                return 'rgba(255, 193, 7, 0.8)'; // Yellow - Pending/Approval
            }
            if (status.includes('po') || status.includes('ordered')) {
                return 'rgba(13, 110, 253, 0.8)'; // Blue - PO Issued
            }
            if (status.includes('receipt') || status.includes('receiving')) {
                return 'rgba(13, 202, 240, 0.8)'; // Cyan - Receipt
            }
            if (status.includes('inspection') || status.includes('quality')) {
                return 'rgba(111, 66, 193, 0.8)'; // Purple - Inspection
            }
            if (status.includes('payment') || status.includes('paid')) {
                return 'rgba(40, 167, 69, 0.8)'; // Green - Payment
            }
            if (status.includes('cancel') || status.includes('reject')) {
                return 'rgba(220, 53, 69, 0.8)'; // Red - Cancelled
            }

            return 'rgba(108, 117, 125, 0.8)'; // Gray - Default
        });

        // Create chart
        var chart = new Chart(ctx, {
            type: 'pie',
            data: {
                labels: statusData.map(function (x) { return x.currentStatus; }),
                datasets: [{
                    data: statusData.map(function (x) { return x.demandCount; }),
                    backgroundColor: statusColors,
                    borderColor: '#ffffff',
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'right',
                        labels: {
                            padding: 15,
                            font: {
                                size: 12
                            },
                            generateLabels: function (chart) {
                                var data = chart.data;
                                if (data.labels.length && data.datasets.length) {
                                    return data.labels.map(function (label, i) {
                                        var value = data.datasets[0].data[i];
                                        return {
                                            text: label + ' (' + value + ')',
                                            fillStyle: data.datasets[0].backgroundColor[i],
                                            hidden: false,
                                            index: i
                                        };
                                    });
                                }
                                return [];
                            }
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                var label = context.label || '';
                                var value = context.parsed || 0;
                                var total = context.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                                var percentage = ((value / total) * 100).toFixed(1);
                                return label + ': ' + value + ' demands (' + percentage + '%)';
                            }
                        }
                    }
                },
                onClick: function (event, activeElements) {
                    if (activeElements && activeElements.length > 0) {
                        var index = activeElements[0].index;
                        var status = statusData[index].currentStatus;
                        var count = statusData[index].demandCount;

                        // Show info dialog
                        if (typeof bootbox !== 'undefined') {
                            bootbox.alert({
                                title: '<i class="uil uil-info-circle"></i> ' + status,
                                message: '<strong>Total Demands:</strong> ' + count + '<br><br>' +
                                    '<em class="text-muted">Future enhancement: Click to view detailed list of demands in this status.</em>',
                                size: 'small',
                                centerVertical: true
                            });
                        } else {
                            alert(status + ': ' + count + ' demands');
                        }
                    }
                }
            }
        });

        console.log('Procurement Status Chart initialized successfully');
        return chart;

    } catch (error) {
        console.error('Error initializing Procurement Status Chart:', error);
    }
}

 
function initTopVendorsChart(canvasId, vendorData) {
    try {
        var canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error('Canvas element not found:', canvasId);
            return;
        }

        var ctx = canvas.getContext('2d');

        // Take only top 5 vendors for space optimization
        var topVendors = vendorData.slice(0, 5);

        // Store full names for tooltips
        var fullNames = topVendors.map(function (x) { return x.vendor_Name; });

        // Create truncated labels for display
        var truncatedLabels = topVendors.map(function (x) {
            return truncateVendorName(x.vendor_Name, 25);
        });

        // Convert values to Crores
        var values = topVendors.map(function (x) {
            return (x.totalOrderValue / 10000000).toFixed(2);
        });

        // Create gradient colors (darker for higher values)
        var colors = values.map(function (value, index) {
            var intensity = 1 - (index * 0.15); // Gradually lighter
            return 'rgba(13, 110, 253, ' + intensity + ')';
        });

        var chart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: truncatedLabels,
                datasets: [{
                    label: 'Order Value (₹ Cr)',
                    data: values,
                    backgroundColor: colors,
                    borderColor: 'rgba(13, 110, 253, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                indexAxis: 'y', // Horizontal bars
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false // Hide legend to save space
                    },
                    tooltip: {
                        callbacks: {
                            title: function (context) {
                                // Show full vendor name in tooltip
                                var index = context[0].dataIndex;
                                return fullNames[index];
                            },
                            label: function (context) {
                                return 'Order Value: ₹' + context.parsed.x + ' Cr';
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Order Value (₹ Crores)',
                            font: {
                                size: 11
                            }
                        },
                        ticks: {
                            font: {
                                size: 10
                            },
                            callback: function (value) {
                                return '₹' + value;
                            }
                        }
                    },
                    y: {
                        ticks: {
                            font: {
                                size: 11
                            },
                            autoSkip: false // Don't skip any labels
                        }
                    }
                },
                layout: {
                    padding: {
                        left: 10,
                        right: 20,
                        top: 5,
                        bottom: 5
                    }
                }
            }
        });

        console.log('Top Vendors Chart initialized successfully');
        return chart;

    } catch (error) {
        console.error('Error initializing Top Vendors Chart:', error);
    }
}

function truncateVendorName(fullName, maxLength) {
    if (!fullName) return '';

    // Remove common business suffixes to save space
    var cleaned = fullName
        .replace(/\s+Private Limited$/i, '')
        .replace(/\s+Pvt\.?\s*Ltd\.?$/i, '')
        .replace(/\s+Limited$/i, '')
        .replace(/\s+Ltd\.?$/i, '')
        .replace(/\s+Corporation$/i, '')
        .replace(/\s+Corp\.?$/i, '')
        .replace(/\s+Incorporated$/i, '')
        .replace(/\s+Inc\.?$/i, '')
        .replace(/\s+Industries$/i, '')
        .replace(/\s+Ind\.?$/i, '')
        .replace(/\s+Company$/i, '')
        .replace(/\s+Co\.?$/i, '')
        .replace(/\s+\(India\)$/i, '')
        .trim();

    // If still too long, truncate with ellipsis
    if (cleaned.length > maxLength) {
        return cleaned.substring(0, maxLength - 3) + '...';
    }

    return cleaned;
}



