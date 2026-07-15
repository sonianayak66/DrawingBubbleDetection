/*!
 * Fancytree Taxonomy Browser
 *
 * Copyright (c) 2015, Martin Wendt (https://wwWendt.de)
 *
 * Released under the MIT license
 * https://github.com/mar10/fancytree/wiki/LicenseInfo
 *
 * @version @VERSION
 * @date @DATE
 */

/* global Handlebars */
/* eslint-disable no-console */

(function ($, window, document) {
    "use strict";

    /*******************************************************************************
     * Private functions and variables
     */

    var taxonTree,
        searchResultTree,
        tmplDetails,
        tmplInfoPane,
        tmplMedia,
        timerMap = {},
        // USER_AGENT = "Fancytree Taxonomy Browser/1.0",
        GBIF_URL = "//api.gbif.org/v1/",
        TAXONOMY_KEY = "d7dddbf4-2cf0-4f39-9b2a-bb099caae36c", // GBIF backbone taxonomy
        SEARCH_PAGE_SIZE = 5,
        CHILD_NODE_PAGE_SIZE = 200,
        glyphOpts = {
            preset: "bootstrap3",
            map: {
                expanderClosed: "bi bi-caret-right fs-1", // glyphicon-plus-sign
                expanderLazy: "bi bi-caret-right fs-1", // glyphicon-plus-sign
               // expanderOpen: "bi bi-folder-fill fs-1", // glyphicon-collapse-down
            },
        };

    // Load and compile handlebar templates

    //$.get("details.tmpl.html", function (data) {
    //	tmplDetails = Handlebars.compile(data);
    //	Handlebars.registerPartial("tmplDetails", tmplDetails);
    //});
    //$.get("media.tmpl.html", function (data) {
    //	tmplMedia = Handlebars.compile(data);
    //	Handlebars.registerPartial("tmplMedia", tmplMedia);
    //});
    //$.get("info-pane.tmpl.html", function (data) {
    //	tmplInfoPane = Handlebars.compile(data);
    //});

    /** Update UI elements according to current status
     */


 
    function updateControls() {
        var query = $.trim($("input[name=query]").val());

        $("#btnPin").attr("disabled", !taxonTree.getActiveNode());
        $("#btnUnpin")
            .attr("disabled", !taxonTree.isFilterActive())
            .toggleClass("btn-success", taxonTree.isFilterActive());
        $("#btnResetSearch").attr("disabled", query.length === 0);
        $("#btnSearch").attr("disabled", query.length < 2);
    }

    /**
     * Invoke callback after `ms` milliseconds.
     * Any pending action of this type is cancelled before.
     */
    function _delay(tag, ms, callback) {
        /*jshint -W040:true */
        var self = this;

        tag = "" + (tag || "default");
        if (timerMap[tag] != null) {
            clearTimeout(timerMap[tag]);
            delete timerMap[tag];
            // console.log("Cancel timer '" + tag + "'");
        }
        if (ms == null || callback == null) {
            return;
        }
        // console.log("Start timer '" + tag + "'");
        timerMap[tag] = setTimeout(function () {
            // console.log("Execute timer '" + tag + "'");
            callback.call(self);
        }, +ms);
    }

    /**
     */
    function _callWebservice(cmd, data) {
        console.log()
        return $.ajax({
            //url: GBIF_URL + cmd,
            //url: '/tree/json.json', 
            url: '/DocManager/GetDocuments?id=' + data + '&type=' + cmd,
            data: $.extend({}, data),
            cache: true,
            // 2022-11-10: Datatype 'JSONP' no longer works:
            // '[Error] Refused to execute http://api.gbif.org/v1/species/... as script because "X-Content-Type-Options: nosniff" was given and its Content-Type is not a script MIME type.
            // We rely on CORS, but this only works if no additoinal header is set
            // headers: { "Api-User-Agent": USER_AGENT },
            dataType: "json",
        });
    }

    /**
     */
    function updateItemDetails(key) {
        $("#tmplDetails").addClass("busy");
        $.bbq.pushState({ key: key });
        $.when(
            _callWebservice('Document', key),
        ).done(function (data) {
            $.ajax({
                url: '/DocManager/Detail?docdbkey=' + key,
                type: 'GET',
                success: function (data) {
                    document.getElementById("tmplInfoPane").innerHTML = data;
                    document.getElementById("btnResetSearch").click();
                },
                error: function (request, error) {                
                    alert("Request: " + JSON.stringify(request));
                }
            });
            console.log(data.results[0].Refrence_Title);
            console.log(data.results[0].Item_type);
            updateControls();

            if (data.results[0].Item_type == "File") {
                $("#updateFolderbtn").attr("disabled", true);
                $("#uploadfilebtn").attr("disabled", true);
                $("#createFolderbtn").attr("disabled", true);
                try {
                    $("#accessconfigbtn").attr("disabled", true);
                } catch (e) {

                }
            } else {
                $("#updateFolderbtn").attr("disabled", false);
                $("#createFolderbtn").attr("disabled", false);
                $("#uploadfilebtn").attr("disabled", false);
                try {
                    $("#accessconfigbtn").attr("disabled", false);
                } catch (e) {

                }
            }
        });

    }
 
    function updateBreadcrumb(key, loadTreeNodes) {
        var $ol = $("ol.breadcrumb").addClass("busy"),
            activeNode = taxonTree.getActiveNode();

        if (activeNode && activeNode.key !== key) {
            activeNode.setActive(false); // deactivate, in case the new key is not found
        }
        $.when(
            /*	_callWebservice("species/" + key + "/parents"),*/
            _callWebservice("Parent", key),
            //_callWebservice("species/" + key)
            _callWebservice("Document", key)
        ).done(function (parents, node) {
            // Both requests resolved (result format: [ data, statusText, jqXHR ])
            var nodeList = parents[0].results,
                keyList = [];
            console.log(nodeList);
            nodeList.push(node[0].results[0]);

            // Display as <OL> list (for Bootstrap breadcrumbs)
            $ol.empty().removeClass("busy");
            $.each(nodeList, function (i, o) {
                var name = o.Refrence_Title;
                keyList.push(o.Document_Dbkey);
                if ("" + o.Document_Dbkey === "" + key) {
                    $ol.append(
                        $("<li class='active breadcrumb-item'>").append(
                            $("<span>", {
                                text: name,
                                title: o.rank,
                            })
                        )
                    );
                } else {
                    $ol.append(
                        $("<li class='breadcrumb-item'>").append(
                            $("<a>", {
                                href: "#key=" + o.Document_Dbkey,
                                text: name,
                                title: o.rank,
                            })
                        )
                    );
                }
            });
            if (loadTreeNodes) {
                // console.log("updateBreadcrumb - loadKeyPath", keyList);
                taxonTree.loadKeyPath(
                    "/" + keyList.join("/"),
                    function (n, status) {
                        // console.log("... updateBreadcrumb - loadKeyPath " + n.title + ": " + status);
                        switch (status) {
                            case "loaded":
                                n.makeVisible();
                                break;
                            case "ok":
                                n.setActive();
                                // n.makeVisible();
                                break;
                        }
                    }
                );
            }
        });
    }

    /**
     */
    function search(query) {
        query = $.trim(query);
        console.log("searching for '" + query + "'...");
        // Store the source options for optional paging
        searchResultTree.lastSourceOpts = {
            // url: GBIF_URL + "species/match",  // Fuzzy matches scientific names against the GBIF Backbone Taxonomy
            //url: GBIF_URL + "species/search", // Full text search of name usages covering the scientific and vernacular name, the species description, distribution and the entire classification across all name usages of all or some checklists
            //url: '/tree/serach.json',
            url: '/DocManager/GetDocuments/?type=Search&searchtags=' + query,
            data: {
                q: query,
                datasetKey: TAXONOMY_KEY,
                // name: query,
                // strict: "true",
                // hl: true,
                limit: SEARCH_PAGE_SIZE,
                offset: 0,
            },
            cache: true,
            // headers: { "Api-User-Agent": USER_AGENT }
            // dataType: "jsonp"
        };
        $("#searchResultTree").addClass("busy");
        searchResultTree
            .reload(searchResultTree.lastSourceOpts)
            .done(function (result) {
                // console.log("search returned", result);
                if (result.length < 1) {
                    searchResultTree.getRootNode().setStatus("nodata");
                }
                $("#searchResultTree").removeClass("busy");

                // https://github.com/tbasse/jquery-truncate
                // SLOW!
                // $("div.truncate").truncate({
                // 	multiline: true
                // });

                updateControls();
            });
    }

  


    /*******************************************************************************
     * Pageload Handler
     */



    $(function () {
        $("#taxonTree").fancytree({
            extensions: ["filter", "glyph", "wide"],
            filter: {
                mode: "hide",
            },
            glyph: glyphOpts,
            autoCollapse: true,
            activeVisible: true,
            autoScroll: true,
            source: {
                //url: GBIF_URL + "species/root/" + TAXONOMY_KEY,
                url: '/DocManager/GetDocuments?id=0&type=Children',
                //url: '/tree/Json.Json',
                data: {},
                cache: true,
                // dataType: "jsonp"
            },

            init: function (event, data) {
                updateControls();
                $(window).trigger("hashchange"); // trigger on initial page load
            },
            lazyLoad: function (event, data) {
                data.result = {
                    //url: GBIF_URL + "species/" + data.node.key + "/children",
                    //url: '/tree/children.json',
                    url: '/DocManager/GetDocuments?id=' + data.node.key + '&type=Children',
                    data: {
                        limit: CHILD_NODE_PAGE_SIZE,
                    },
                    cache: true,
                    // dataType: "jsonp"
                };
                // store this request options for later paging
                data.node.lastSourceOpts = data.result;
            },
            postProcess: function (event, data) {
                var response = data.response;

                data.node.info("taxonTree postProcess", response);
                data.result = $.map(response.results, function (o) {
                    return (
                        o && {
                            title: o.Refrence_Title,
                            key: o.Document_Dbkey,
                            nubKey: o.Document_Dbkey,
                            folder: true,
                            lazy: true,
                        }
                    );
                });
                if (response.endOfRecords === false) {
                    // Allow paging
                    data.result.push({
                        title: "(more)",
                        statusNodeType: "paging",
                    });
                } else {
                    // No need to store the extra data
                    delete data.node.lastSourceOpts;
                }
            },
            activate: function (event, data) {
                $("#tmplDetails").addClass("busy");
                $("ol.breadcrumb").addClass("busy");
                updateControls();
                _delay("showDetails", 500, function () {
                    updateItemDetails(data.node.key);
                    updateBreadcrumb(data.node.key);
                });
            },
            clickPaging: function (event, data) {
                // Load the next page of results
                var source = $.extend(
                    true,
                    {},
                    data.node.parent.lastSourceOpts
                );
                source.data.offset = data.node.parent.countChildren() - 1;
                data.node.replaceWith(source);
            },
        });
 
        $("#searchResultTree").fancytree({
            extensions: ["table", "wide"],
            source: [{ title: "No Results." }],
            minExpandLevel: 10,
            icon: false,
            table: {
                nodeColumnIdx: 2,
            },
            postProcess: function (event, data) {
                var response = data.response;

                data.node.info("search postProcess", response);
                data.result = $.map(response.results, function (o) {
                    var res = $.extend(
                        {
                            title: o.Refrence_Title,
                            key: o.Document_Dbkey,
                        },
                        o
                    );
                    return res;
                });
                // Append paging link
                //if (
                //	response.count != null &&
                //	response.offset + response.limit < response.count
                //) {
                //	data.result.push({
                //		title:
                //			"(" +
                //			(response.count -
                //				response.offset -
                //				response.limit) +
                //			" more)",
                //		statusNodeType: "paging",
                //	});
                //}
                data.node.info("search postProcess 2", data.result);
            },
            // loadChildren: function(event, data) {
            // 	$("#searchResultTree td div.cell").truncate({
            // 		multiline: true
            // 	});
            // },
            renderColumns: function (event, data) {
                var i,
                    node = data.node,
                    $tdList = $(node.tr).find(">td"),


                    i = 0;
                function _setCell($cell, text) {
                    $("<div class='truncate'>")
                        .attr("title", text)
                        .text(text)
                        .appendTo($cell);
                }
                $tdList.eq(i++).text(node.key);
                $tdList.eq(i++).text(node.data.Parent_id);
                i++; // #1: node.title = Refrence_Title
                $tdList.eq(i++).text(node.data.Description);
                $tdList.eq(i++).text(node.data.File_type);
                $tdList.eq(i++).text(node.data.File_Size);
                $tdList.eq(i++).text(node.data.Approved_Status_text);
                $tdList.eq(i++).text(node.data.Updated_By_UserName);
                $tdList.eq(i++).text(node.data.Updated_On);
                $tdList.eq(i++).text(node.data.SearchTags);


            },
            activate: function (event, data) {
                if (data.node.isStatusNode()) {
                    return;
                }
                _delay("activateNode", 500, function () {
                    updateItemDetails(data.node.key);
                    updateBreadcrumb(data.node.key);
                });
            },
            clickPaging: function (event, data) {
                // Load the next page of results
                var source = $.extend(
                    true,
                    {},
                    searchResultTree.lastSourceOpts
                );
                source.data.offset = data.node.parent.countChildren() - 1;
                data.node.replaceWith(source);
            },
        });

        taxonTree = $.ui.fancytree.getTree("#taxonTree");
        searchResultTree = $.ui.fancytree.getTree("#searchResultTree");

        // Bind a callback that executes when document.location.hash changes.
        // (This code uses bbq: https://github.com/cowboy/jquery-bbq)
        $(window).on("hashchange", function (e) {
            var key = $.bbq.getState("key");
            console.log("bbq key", key);
            if (key) {
                updateBreadcrumb(key, true);
                updateItemDetails(key);
            }
        }); // don't trigger now, since we need the the taxonTree root nodes to be loaded first

        $("input[name=query]")
            .on("keyup", function (e) {
                var query = $.trim($(this).val()),
                    lastQuery = $(this).data("lastQuery");

                if ((e && e.which === $.ui.keyCode.ESCAPE) || query === "") {
                    $("#btnResetSearch").trigger("click");
                    return;
                }
                if (e && e.which === $.ui.keyCode.ENTER && query.length >= 2) {
                    $("#btnSearch").trigger("click");
                    return;
                }
                if (query === lastQuery || query.length < 2) {
                    console.log("Ignored query '" + query + "'");
                    return;
                }
                $(this).data("lastQuery", query);
                _delay("search", 1, function () {
                    $("#btnSearch").trigger("click");
                });
                $("#btnResetSearch").attr("disabled", query.length === 0);
                $("#btnSearch").attr("disabled", query.length < 2);
            })
            .focus();

        $("#btnResetSearch").click(function (e) {
            $("#searchResultPane").collapse("hide");
            $("input[name=query]").val("");
            searchResultTree.clear();
            updateControls();
        });

        $("#btnSearch")
            .click(function (event) {
                $("#searchResultPane").collapse("show");
                search($("input[name=query]").val());
            })
            .attr("disabled", true);

        $("#btnPin").click(function (event) {
            taxonTree.filterBranches(function (n) {
                return n.isActive();
            });
            updateControls();
        });

        $("#btnUnpin").click(function (event) {
            taxonTree.clearFilter();
            updateControls();
        });
 
        // -----------------------------------------------------------------------------
    }); // end of pageload handler

  

 
})(jQuery, window, document);

function ReloadPage() {
    window.location.href = '/DocManager/Index';
}

function CreateFolder(actiontitle) {
    var key = $.bbq.getState("key");
    console.log("bbq key", key);
    if (!key) {
        key = 0;
    }
    $.ajax({
        url: '/DocManager/Folder?docdbkey=' + key + '&actionon=' + actiontitle,
        type: 'GET',
        success: function (data) {
            var titletxt = actiontitle == "CreateFolder" ? "Create Folder" : "Update Folder";
            titletxt = actiontitle == "UploadFile" ? "Upload File" : titletxt;
            bootbox.dialog({
                message: data,
                title: titletxt,
                size: 'meduim',
            });
        },
        error: function (request, error) {
            alert("Request: " + JSON.stringify(request));
        }
    });
}

function UpdateDocumentFolderinfo() {
    $.validator.unobtrusive.parse("form");
    if ($('#DocMainForm').valid()) {

        var fileData = new FormData();

        var filesinput = document.getElementById("DocSFile").files;

        for (var i = 0; i < filesinput.length; i++) {
            fileData.append('Files', filesinput[i]);
        }


        fileData.append('Refrence_Title', document.getElementById("Refrence_Title").value);
        fileData.append('Description', document.getElementById("Description").value);
        fileData.append('Parent_id', document.getElementById("Parent_id").value);
        fileData.append('Document_Dbkey', document.getElementById("Document_Dbkey").value);
        fileData.append('Item_type', document.getElementById("Item_type").value);
        fileData.append('is_required_approve', document.getElementById("is_required_approve").checked);
        fileData.append('Inherit_Parent_Access', document.getElementById("Inherit_Parent_Access").checked);
        fileData.append('SearchTags', document.getElementById("SearchTags").value);
        //fileData.append('Note', note);



        if (document.getElementById("Item_type").value == 'File') {
            if (filesinput.length == 0) {
                alert("Input File is Required");
                return false;
            }
        }
        console.log(fileData);
        document.getElementById("lnksave").disabled = true;

        $.ajax({
            type: "POST",
            url: '/DocManager/SaveDocumentDetail',
            data: fileData,
            dataType: "json",
            cache: false,
            contentType: false,
            processData: false,
            success: function (data) {
                //console.log(data);
                bootbox.alert({
                    message: data.msg,
                    callback: function () {
                        var btn = document.getElementsByClassName("bootbox-close-button");
                        btn[0].click();
                        window.location.href = '/DocManager/Index#key=' + data.docdbkey;
                        window.location.reload(true);
                    }
                });
            }
        });
    }
    return false;

}

function GetFolderAccessInfo() {
    var key = $.bbq.getState("key");
    console.log("bbq key", key);
    if (!key) {
        key = 0;
    }
    $.ajax({
        url: '/DocManager/AccessDetail?docdbkey=' + key,
        type: 'GET',
        success: function (data) {
            bootbox.dialog({
                message: data,
                title: 'Access Info',
                size: 'large',
            });
        },
        error: function (request, error) {
            alert("Request: " + JSON.stringify(request));
        }
    });
}



                    ``