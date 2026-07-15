function GlobalDeleteAttachment(ctrl, AttachmentKey, sourceKey, soureTable, deleteAccess) {  
    var parentContainer = ctrl.closest('.Attachment-display');  
  var bootboxDeleteDlg =   bootbox.confirm({
        message: "Are you sure you wish to delete this file?",
        centerVertical: true,
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
                    async: false,
                    url: '/Attachment/DeleteAttachment/' + AttachmentKey,
                    type: 'GET',
                    success: function (data) {
                        Globalgetattachments(parentContainer, sourceKey, soureTable, deleteAccess)
                    }
                });
            }
             
        }
  });   

    $(bootboxDeleteDlg).delay(3000).css("background-color", "background: rgba(0,0,0,0.5)"); 
}
 

function GlobalUploadFile(btn) {

    if (btn.files.length == 0) {
        $(btn.parentNode.parentNode).notify("Please select file to upload", {
            // globalPosition: "top center",
            className: "danger"
        });
        return false;
    }

    var attachmentVM = new FormData();
    var fileContainer = btn.parentNode;
    var uploadeddocument = fileContainer.children[3];
    var deleteAccess = fileContainer.children[4];
    var Source_table_key = fileContainer.children[0];
    var Source_table = fileContainer.children[1];
    var Attachment_type = fileContainer.children[2];
  
    attachmentVM.append('uploadeddocument', uploadeddocument.files[0]);
    attachmentVM.append('Source_table_key', Source_table_key.value);
    attachmentVM.append('Source_table', Source_table.value);
    attachmentVM.append('deleteAccess', deleteAccess.value);
    attachmentVM.append('Attachment_type', $(Attachment_type).find('option:selected').text());
     
    $.ajax({
        type: "POST",
        url: '/Attachment/UploadFiles',
        /*    async: true,*/
        data: attachmentVM,
        cache: false,
        contentType: false,
        processData: false,
        cache: false,
        success: function (data) {
            if (data.success) {
                $(btn.parentNode.parentNode).notify("Uploaded", {
                    className: "success"
                });
                Globalgetattachments(btn.parentNode.parentNode.querySelector("#Attachment-display"), Source_table_key.value, Source_table.value, deleteAccess.value)
            }
        }
    });
}

function Globalgetattachments(parent, itemKey, soureTable, deleteAccess) {
    $.ajax({
        url: '/Attachment/GetAttachments?itemKey=' + itemKey + '&sourceTableName=' + soureTable + '&deleteAccess=' + deleteAccess,
        type: 'GET',
        success: function (data) {
            parent.innerHTML = data;
        }
    });
}