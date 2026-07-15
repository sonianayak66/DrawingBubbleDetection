import React, { useState, useEffect } from "react";
import {
  Box,
  Typography,
  Paper,
  Button,
  Chip,
  List,
  ListItem,
  ListItemText,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  CircularProgress,
  Divider,
  Switch,
  FormControlLabel,
} from "@mui/material";
import {
  Edit,
  Delete,
  FileDownload,
  Upload,
  AttachFile,
  Close,
  Visibility,
  Cancel as CancelIcon,
  CheckCircle,
} from "@mui/icons-material";
import { inwardIonApi } from "../../../services/inwardIonApi";
import { usePermissions } from "../../../context/PermissionsContext";

const MONTHS = ["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"];
const formatDate = (dateStr) => {
  if (!dateStr) return "-";
  const d = new Date(dateStr);
  return `${String(d.getDate()).padStart(2, "0")}-${MONTHS[d.getMonth()]}-${d.getFullYear()}`;
};

const InwardIONDetailView = ({ inwardIONGUID, onEdit, onBack, onDelete }) => {
  const { hasPermission } = usePermissions();
  const isAdmin = hasPermission("ION_Inward_Admin");
  const canEdit = hasPermission("ION_Inward_Edit");
  const canDelete = hasPermission("ION_Inward_Delete");
  const isSupportOp = hasPermission("ION_SupportOperator");

  const [noteData, setNoteData] = useState(null);
  const [attachments, setAttachments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [uploadingFile, setUploadingFile] = useState(false);
  const [previewUrl, setPreviewUrl] = useState(null);
  const [previewOpen, setPreviewOpen] = useState(false);
  const [previewType, setPreviewType] = useState("pdf");
  const [previewName, setPreviewName] = useState("");

  useEffect(() => {
    if (inwardIONGUID) {
      loadDetails();
    }
  }, [inwardIONGUID]);

  const loadDetails = async () => {
    try {
      setLoading(true);
      const res = await inwardIonApi.getInwardNoteDetail(inwardIONGUID);
      const d = res.data;
      setNoteData(d);

      if (d?.InwardNoteId) {
        const attRes = await inwardIonApi.getAttachments(d.InwardNoteId);
        setAttachments(attRes.data || []);
      }
    } catch (err) {
      console.error("Error loading inward note detail:", err);
    } finally {
      setLoading(false);
    }
  };

  const handleDownloadAttachment = async (att) => {
    try {
      const response = await inwardIonApi.downloadAttachment(att.Attachment_Db_Key);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", att.Orginal_File_Name);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (err) {
      console.error("Error downloading attachment:", err);
      alert("Failed to download attachment");
    }
  };

  const handleViewAttachment = async (att) => {
    try {
      const response = await inwardIonApi.downloadAttachment(att.Attachment_Db_Key);
      const fileName = att.Orginal_File_Name || "";
      const ext = fileName.split(".").pop()?.toLowerCase() || "";
      let mimeType = "application/octet-stream";
      if (ext === "pdf") mimeType = "application/pdf";
      else if (["jpg", "jpeg"].includes(ext)) mimeType = "image/jpeg";
      else if (ext === "png") mimeType = "image/png";
      else if (ext === "gif") mimeType = "image/gif";
      else if (ext === "webp") mimeType = "image/webp";
      else if (ext === "bmp") mimeType = "image/bmp";

      const blob = new Blob([response.data], { type: mimeType });
      const url = window.URL.createObjectURL(blob);

      if (ext === "pdf") {
        setPreviewType("pdf");
        setPreviewUrl(url);
        setPreviewName(fileName);
        setPreviewOpen(true);
      } else if (["jpg", "jpeg", "png", "gif", "bmp", "webp"].includes(ext)) {
        setPreviewType("image");
        setPreviewUrl(url);
        setPreviewName(fileName);
        setPreviewOpen(true);
      } else {
        // Non-previewable — just download
        const link = document.createElement("a");
        link.href = url;
        link.setAttribute("download", fileName);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
      }
    } catch (err) {
      console.error("Error viewing attachment:", err);
      alert("Failed to load attachment");
    }
  };

  const handleUploadFile = async (e) => {
    const files = Array.from(e.target.files);
    if (files.length === 0 || !noteData?.InwardNoteId) return;
    try {
      setUploadingFile(true);
      for (const file of files) {
        await inwardIonApi.uploadAttachment(noteData.InwardNoteId, file);
      }
      const attRes = await inwardIonApi.getAttachments(noteData.InwardNoteId);
      setAttachments(attRes.data || []);
    } catch (err) {
      console.error("Error uploading file:", err);
      alert("Failed to upload file");
    } finally {
      setUploadingFile(false);
      e.target.value = "";
    }
  };

  const handleDeleteAttachment = async (att) => {
    if (!window.confirm(`Delete "${att.Orginal_File_Name}"?`)) return;
    try {
      await inwardIonApi.deleteAttachment(att.Attachment_Db_Key);
      setAttachments((prev) => prev.filter((a) => a.Attachment_Db_Key !== att.Attachment_Db_Key));
    } catch (err) {
      console.error("Error deleting attachment:", err);
      alert("Failed to delete attachment");
    }
  };

  const handlePreviewClose = () => {
    if (previewUrl) window.URL.revokeObjectURL(previewUrl);
    setPreviewOpen(false);
    setPreviewUrl(null);
  };

  // Toggle acknowledgment and save immediately
  const handleAckToggle = async () => {
    const newVal = !noteData.AcknowledgmentSent;
    // Optimistic update
    setNoteData((prev) => ({ ...prev, AcknowledgmentSent: newVal }));
    try {
      await inwardIonApi.saveInwardNote({
        InwardIONGUID: noteData.InwardIONGUID,
        ReceivedDate: noteData.ReceivedDate?.slice(0, 10),
        IONDate: noteData.IONDate?.slice(0, 10) || null,
        IONReferenceNumber: noteData.IONReferenceNumber || null,
        FromDepartment: noteData.FromDepartment,
        FromPersonNameWithDesignation: noteData.FromPersonNameWithDesignation || null,
        Subject: noteData.Subject,
        AddressedTo: noteData.AddressedTo,
        CopyTo: noteData.CopyTo || null,
        Remarks: noteData.Remarks || null,
        AcknowledgmentSent: newVal,
      });
    } catch (err) {
      console.error("Error toggling acknowledgment:", err);
      // Revert on failure
      setNoteData((prev) => ({ ...prev, AcknowledgmentSent: !newVal }));
      alert("Failed to update acknowledgment status");
    }
  };

  const renderChips = (text) => {
    if (!text) return <Typography variant="body2" color="text.secondary">-</Typography>;
    const items = text.split("\n").map((s) => s.trim()).filter((s) => s);
    return (
      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
        {items.map((item, i) => (
          <Chip key={i} label={item} size="small" variant="outlined" />
        ))}
      </Box>
    );
  };

  const isPreviewableFile = (fileName) => {
    const ext = (fileName || "").split(".").pop()?.toLowerCase() || "";
    return ["pdf", "jpg", "jpeg", "png", "gif", "bmp", "webp"].includes(ext);
  };

  if (loading) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", height: "100%", flexDirection: "column", gap: 2 }}>
        <CircularProgress />
        <Typography>Loading...</Typography>
      </Box>
    );
  }

  if (!noteData) {
    return (
      <Box sx={{ p: 3, textAlign: "center" }}>
        <Typography>Inward ION not found.</Typography>
        <Button onClick={onBack} sx={{ mt: 1 }}>Close</Button>
      </Box>
    );
  }

  return (
    <Box sx={{ display: "flex", flexDirection: "column", height: "100%" }}>
      {/* Fixed Header */}
      <Box
        sx={{
          px: 2, py: 1.5,
          borderBottom: 1,
          borderColor: "divider",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          bgcolor: "background.paper",
          flexShrink: 0,
          zIndex: 1,
        }}
      >
        <Typography variant="h6" sx={{ fontWeight: 600, fontSize: "1.1rem" }} noWrap>
          {noteData.IONReferenceNumber || "Inward ION"}
        </Typography>
        <Box sx={{ display: "flex", alignItems: "center", gap: 0.5 }}>
          {(isAdmin || canEdit || isSupportOp) && (
            <IconButton size="small" onClick={() => onEdit(noteData)} title="Edit">
              <Edit fontSize="small" />
            </IconButton>
          )}
          {(isAdmin || canDelete) && (
            <IconButton size="small" color="error" onClick={() => onDelete(noteData)} title="Delete">
              <Delete fontSize="small" />
            </IconButton>
          )}
          <IconButton onClick={onBack} size="small">
            <Close />
          </IconButton>
        </Box>
      </Box>

      {/* Scrollable Detail Content */}
      <Box sx={{ flex: 1, overflowY: "auto", p: 2 }}>

        {/* Field: Received Date */}
        <Box sx={{ mb: 2 }}>
          <Typography variant="caption" color="text.secondary">Received Date</Typography>
          <Typography variant="body1" sx={{ fontWeight: 600 }}>
            {formatDate(noteData.ReceivedDate)}
          </Typography>
        </Box>

        {/* Field: ION Date */}
        <Box sx={{ mb: 2 }}>
          <Typography variant="caption" color="text.secondary">ION Date (on letter)</Typography>
          <Typography variant="body1">
            {formatDate(noteData.IONDate)}
          </Typography>
        </Box>

        {/* Field: Reference Number */}
        <Box sx={{ mb: 2 }}>
          <Typography variant="caption" color="text.secondary">ION Reference Number</Typography>
          <Typography variant="body1" sx={{ fontFamily: "monospace", fontWeight: 600 }}>
            {noteData.IONReferenceNumber || "-"}
          </Typography>
        </Box>

        <Divider sx={{ mb: 2 }} />

        {/* Field: From Department */}
        <Box sx={{ mb: 2 }}>
          <Typography variant="caption" color="text.secondary">From Department</Typography>
          <Typography variant="body1" sx={{ fontWeight: 600 }}>
            {noteData.FromDepartment}
          </Typography>
        </Box>

        {/* Field: From Person */}
        <Box sx={{ mb: 2 }}>
          <Typography variant="caption" color="text.secondary">From (Person & Designation)</Typography>
          <Typography variant="body1">
            {noteData.FromPersonNameWithDesignation || "-"}
          </Typography>
        </Box>

        {/* Field: Subject */}
        <Box sx={{ mb: 2 }}>
          <Typography variant="caption" color="text.secondary">Subject</Typography>
          <Typography variant="body1" sx={{ fontWeight: 500 }}>
            {noteData.Subject}
          </Typography>
        </Box>

        <Divider sx={{ mb: 2 }} />

        {/* Field: Addressed To */}
        <Box sx={{ mb: 2 }}>
          <Typography variant="caption" color="text.secondary" sx={{ display: "block", mb: 0.5 }}>
            Addressed To
          </Typography>
          {renderChips(noteData.AddressedTo)}
        </Box>

        {/* Field: Copy To */}
        <Box sx={{ mb: 2 }}>
          <Typography variant="caption" color="text.secondary" sx={{ display: "block", mb: 0.5 }}>
            Copy To
          </Typography>
          {renderChips(noteData.CopyTo)}
        </Box>

        {/* Field: Remarks */}
        {noteData.Remarks && (
          <Box sx={{ mb: 2 }}>
            <Typography variant="caption" color="text.secondary">Remarks</Typography>
            <Typography variant="body1" sx={{ whiteSpace: "pre-wrap" }}>
              {noteData.Remarks}
            </Typography>
          </Box>
        )}

        {/* Field: Acknowledgment — live toggle */}
        <Box sx={{ mb: 2 }}>
          <FormControlLabel
            control={
              <Switch
                checked={!!noteData.AcknowledgmentSent}
                onChange={handleAckToggle}
                color="success"
                size="small"
              />
            }
            label={
              <Typography variant="body2">
                Acknowledgment Sent
              </Typography>
            }
          />
        </Box>

        <Divider sx={{ mb: 2 }} />

        {/* ===== Attachments ===== */}
        <Box>
          <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 1 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
              <AttachFile fontSize="small" sx={{ verticalAlign: "middle", mr: 0.5 }} />
              Attachments ({attachments.length})
            </Typography>

            {(isAdmin || canEdit || isSupportOp) && (
              <Button
                variant="outlined"
                size="small"
                startIcon={uploadingFile ? <CircularProgress size={14} /> : <Upload />}
                component="label"
                disabled={uploadingFile}
              >
                Upload
                <input type="file" hidden multiple onChange={handleUploadFile} />
              </Button>
            )}
          </Box>

          {attachments.length === 0 ? (
            <Typography variant="body2" color="text.secondary" sx={{ py: 2, textAlign: "center" }}>
              No attachments uploaded yet.
            </Typography>
          ) : (
            <List dense disablePadding>
              {attachments.map((att) => {
                const canPreview = isPreviewableFile(att.Orginal_File_Name);
                return (
                  <ListItem key={att.Attachment_Db_Key} divider sx={{ pr: 12 }}>
                    <ListItemText
                      primary={att.Orginal_File_Name}
                      primaryTypographyProps={{ variant: "body2", noWrap: true }}
                      secondary={att.UploadedByName || null}
                      secondaryTypographyProps={{ variant: "caption" }}
                    />
                    <Box sx={{ position: "absolute", right: 8, display: "flex", gap: 0.25 }}>
                      {canPreview && (
                        <IconButton size="small" onClick={() => handleViewAttachment(att)} title="View" color="primary">
                          <Visibility fontSize="small" />
                        </IconButton>
                      )}
                      <IconButton size="small" onClick={() => handleDownloadAttachment(att)} title="Download">
                        <FileDownload fontSize="small" />
                      </IconButton>
                      {(isAdmin || canEdit || isSupportOp) && (
                        <IconButton size="small" onClick={() => handleDeleteAttachment(att)} color="error" title="Delete">
                          <Delete fontSize="small" />
                        </IconButton>
                      )}
                    </Box>
                  </ListItem>
                );
              })}
            </List>
          )}
        </Box>
      </Box>

      {/* Preview Dialog */}
      <Dialog
        open={previewOpen}
        onClose={handlePreviewClose}
        maxWidth="lg"
        fullWidth
        PaperProps={{ sx: { height: "90vh" } }}
      >
        <DialogTitle>
          <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
            <Typography variant="h6" noWrap sx={{ flex: 1, mr: 2 }}>{previewName || "Attachment Preview"}</Typography>
            <IconButton onClick={handlePreviewClose} size="small">
              <CancelIcon />
            </IconButton>
          </Box>
        </DialogTitle>
        <DialogContent sx={{ p: 0 }}>
          {previewUrl && (
            previewType === "pdf" ? (
              <iframe
                src={previewUrl}
                style={{ width: "100%", height: "100%", border: "none" }}
                title="PDF Preview"
              />
            ) : (
              <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", height: "100%", p: 2 }}>
                <img
                  src={previewUrl}
                  alt="Attachment Preview"
                  style={{ maxWidth: "100%", maxHeight: "100%", objectFit: "contain" }}
                />
              </Box>
            )
          )}
        </DialogContent>
      </Dialog>
    </Box>
  );
};

export default InwardIONDetailView;
