import React, { useState, useEffect, useCallback, useRef } from "react";
import {
  Box,
  Typography,
  Paper,
  TextField,
  Button,
  IconButton,
  Chip,
  CircularProgress,
  Autocomplete,
  FormControlLabel,
  Checkbox,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Divider,
  Dialog,
  DialogTitle,
  DialogContent,
} from "@mui/material";
import {
  Save,
  Cancel,
  Close,
  Upload,
  Delete,
  FileDownload,
  AttachFile,
  Visibility,
  Cancel as CancelIcon,
} from "@mui/icons-material";
import { inwardIonApi } from "../../../services/inwardIonApi";
import { ionApi } from "../../../services/ionApi";
import { usePermissions } from "../../../context/PermissionsContext";

// ── ChipField (type-or-select, inline edit) ──
const ChipField = ({ label, chips, onDelete, onEdit, inputValue, onInputChange, onAdd, options, required, error, helperText, disabled }) => {
  const inputRef = useRef(null);
  const [editingIndex, setEditingIndex] = useState(null);
  const [editingValue, setEditingValue] = useState("");
  const editInputRef = useRef(null);

  const startEdit = (index) => {
    setEditingIndex(index);
    setEditingValue(chips[index]);
    setTimeout(() => editInputRef.current?.focus(), 0);
  };

  const commitEdit = () => {
    if (editingIndex === null) return;
    const trimmed = editingValue.trim();
    if (trimmed && trimmed !== chips[editingIndex]) {
      onEdit(editingIndex, trimmed);
    }
    setEditingIndex(null);
    setEditingValue("");
  };

  const cancelEdit = () => {
    setEditingIndex(null);
    setEditingValue("");
  };

  return (
    <Box>
      <Typography variant="caption" color={error ? "error" : "text.secondary"} sx={{ mb: 0.5, display: "block", fontWeight: 500 }}>
        {label} {required && "*"}
      </Typography>
      <Paper
        variant="outlined"
        sx={{
          p: 0.75,
          minHeight: 40,
          display: "flex",
          flexWrap: "wrap",
          gap: 0.5,
          alignItems: "center",
          borderColor: error ? "error.main" : "divider",
          "&:focus-within": { borderColor: "primary.main", borderWidth: 2 },
        }}
      >
        {chips.map((chip, i) =>
          editingIndex === i ? (
            <TextField
              key={i}
              inputRef={editInputRef}
              value={editingValue}
              onChange={(e) => setEditingValue(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") { e.preventDefault(); commitEdit(); }
                if (e.key === "Escape") { cancelEdit(); }
              }}
              onBlur={commitEdit}
              size="small"
              variant="standard"
              sx={{ width: Math.max(80, editingValue.length * 8 + 20), "& .MuiInput-input": { fontSize: 13, py: 0.25 } }}
              autoFocus
            />
          ) : (
            <Chip
              key={i}
              label={chip}
              size="small"
              onClick={disabled ? undefined : () => startEdit(i)}
              onDelete={disabled ? undefined : () => onDelete(i)}
              sx={{ maxWidth: 250, cursor: disabled ? "default" : "pointer" }}
            />
          )
        )}
        {!disabled && (
          <Autocomplete
            freeSolo
            options={options}
            getOptionLabel={(o) => (typeof o === "string" ? o : o.GroupName || "")}
            inputValue={inputValue}
            onInputChange={(_, v, reason) => {
              if (reason !== "reset") onInputChange(v);
            }}
            onChange={(_, selected) => {
              if (selected) {
                const val = typeof selected === "string" ? selected : selected.GroupName;
                onAdd(val);
              }
            }}
            size="small"
            disableClearable
            clearOnBlur={false}
            sx={{ flex: 1, minWidth: 120, "& .MuiOutlinedInput-notchedOutline": { border: "none" } }}
            renderInput={(params) => (
              <TextField
                {...params}
                inputRef={inputRef}
                variant="outlined"
                placeholder={chips.length === 0 ? "Type or select..." : "Add more..."}
                size="small"
                onKeyDown={(e) => {
                  if (e.key === "Enter" && inputValue.trim()) {
                    e.preventDefault();
                    onAdd(inputValue);
                  }
                }}
                sx={{ "& .MuiOutlinedInput-root": { p: "0 !important" } }}
              />
            )}
          />
        )}
      </Paper>
      {helperText && (
        <Typography variant="caption" color="error" sx={{ mt: 0.25 }}>
          {helperText}
        </Typography>
      )}
    </Box>
  );
};

// ── helpers ──
const textToChips = (text) => {
  if (!text) return [];
  return text.split("\n").map((s) => s.trim()).filter((s) => s);
};
const chipsToText = (chips) => chips.join("\n");

const today = () => new Date().toISOString().slice(0, 10);

const MONTHS = ["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"];
const formatDateDisplay = (isoDate) => {
  if (!isoDate) return "";
  const [y, m, d] = isoDate.split("-");
  const monthIdx = parseInt(m, 10) - 1;
  if (monthIdx < 0 || monthIdx > 11) return isoDate;
  return `${d}-${MONTHS[monthIdx]}-${y.slice(-2)}`;
};

// ──────────────────────────────────────────────
const InwardIONFormView = ({ noteData, mode = "create", onSave, onCancel }) => {
  const { hasPermission } = usePermissions();

  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState({});

  // File-group options for department suggestions + users for addressee
  const [fileGroups, setFileGroups] = useState([]);
  const [users, setUsers] = useState([]);

  // Form data
  const [formData, setFormData] = useState({
    InwardIONGUID: "",
    ReceivedDate: today(),
    IONDate: "",
    IONReferenceNumber: "",
    FromDepartment: "",
    FromPersonNameWithDesignation: "",
    Subject: "",
    AddressedTo: "",
    CopyTo: "",
    Remarks: "",
    AcknowledgmentSent: false,
  });

  // Chip state for AddressedTo and CopyTo
  const [toChips, setToChips] = useState([]);
  const [toInput, setToInput] = useState("");
  const [copyChips, setCopyChips] = useState([]);
  const [copyInput, setCopyInput] = useState("");

  // Attachments
  const [attachments, setAttachments] = useState([]);
  const [pendingFiles, setPendingFiles] = useState([]);

  const scrollRef = useRef(null);
  const receivedDateRef = useRef(null);
  const ionDateRef = useRef(null);

  // Attachment preview state
  const [previewUrl, setPreviewUrl] = useState(null);
  const [previewOpen, setPreviewOpen] = useState(false);
  const [previewType, setPreviewType] = useState("pdf");
  const [previewName, setPreviewName] = useState("");

  // Load file groups + internal users for suggestions
  useEffect(() => {
    const loadOptions = async () => {
      try {
        const [fgRes, usersRes] = await Promise.all([
          ionApi.getFileGroups(),
          ionApi.getInternalUsers(),
        ]);
        setFileGroups(fgRes.data || []);
        setUsers(usersRes.data || []);
      } catch (e) {
        console.error("Error loading options:", e);
      }
    };
    loadOptions();
  }, []);

  // Load existing note in edit mode
  useEffect(() => {
    if (mode === "edit" && noteData?.InwardIONGUID) {
      loadNoteDetail();
    }
  }, [mode, noteData]);

  const loadNoteDetail = async () => {
    try {
      setLoading(true);
      const res = await inwardIonApi.getInwardNoteDetail(noteData.InwardIONGUID);
      const d = res.data;
      if (d) {
        setFormData({
          InwardIONGUID: d.InwardIONGUID || "",
          ReceivedDate: d.ReceivedDate ? d.ReceivedDate.slice(0, 10) : today(),
          IONDate: d.IONDate ? d.IONDate.slice(0, 10) : "",
          IONReferenceNumber: d.IONReferenceNumber || "",
          FromDepartment: d.FromDepartment || "",
          FromPersonNameWithDesignation: d.FromPersonNameWithDesignation || "",
          Subject: d.Subject || "",
          AddressedTo: d.AddressedTo || "",
          CopyTo: d.CopyTo || "",
          Remarks: d.Remarks || "",
          AcknowledgmentSent: !!d.AcknowledgmentSent,
        });
        setToChips(textToChips(d.AddressedTo));
        setCopyChips(textToChips(d.CopyTo));

        // load existing attachments
        if (d.InwardNoteId) {
          try {
            const attRes = await inwardIonApi.getAttachments(d.InwardNoteId);
            setAttachments(attRes.data || []);
          } catch (e) {
            console.error("Error loading attachments:", e);
          }
        }
      }
    } catch (err) {
      console.error("Error loading inward note detail:", err);
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (field, value) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: "" }));
  };

  // ── Chip handlers for AddressedTo ──
  const handleToAdd = useCallback((val) => {
    const trimmed = val.trim();
    if (!trimmed) return;
    setToChips((prev) => {
      if (prev.includes(trimmed)) return prev;
      const updated = [...prev, trimmed];
      setFormData((f) => ({ ...f, AddressedTo: chipsToText(updated) }));
      return updated;
    });
    setToInput("");
    if (errors.AddressedTo) setErrors((prev) => ({ ...prev, AddressedTo: "" }));
  }, [errors.AddressedTo]);

  const handleToDelete = useCallback((index) => {
    setToChips((prev) => {
      const updated = prev.filter((_, i) => i !== index);
      setFormData((f) => ({ ...f, AddressedTo: chipsToText(updated) }));
      return updated;
    });
  }, []);

  const handleToEdit = useCallback((index, value) => {
    setToChips((prev) => {
      const updated = [...prev];
      updated[index] = value;
      setFormData((f) => ({ ...f, AddressedTo: chipsToText(updated) }));
      return updated;
    });
  }, []);

  // ── Chip handlers for CopyTo ──
  const handleCopyAdd = useCallback((val) => {
    const trimmed = val.trim();
    if (!trimmed) return;
    setCopyChips((prev) => {
      if (prev.includes(trimmed)) return prev;
      const updated = [...prev, trimmed];
      setFormData((f) => ({ ...f, CopyTo: chipsToText(updated) }));
      return updated;
    });
    setCopyInput("");
  }, []);

  const handleCopyDelete = useCallback((index) => {
    setCopyChips((prev) => {
      const updated = prev.filter((_, i) => i !== index);
      setFormData((f) => ({ ...f, CopyTo: chipsToText(updated) }));
      return updated;
    });
  }, []);

  const handleCopyEdit = useCallback((index, value) => {
    setCopyChips((prev) => {
      const updated = [...prev];
      updated[index] = value;
      setFormData((f) => ({ ...f, CopyTo: chipsToText(updated) }));
      return updated;
    });
  }, []);

  // ── Pending file management ──
  const handleAddPendingFile = (file) => {
    setPendingFiles((prev) => [...prev, file]);
  };

  const handleRemovePendingFile = (index) => {
    setPendingFiles((prev) => prev.filter((_, i) => i !== index));
  };

  // ── Existing attachment actions (use correct field names) ──
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

  // ── Validation ──
  const validate = () => {
    const errs = {};
    if (!formData.ReceivedDate) errs.ReceivedDate = "Received date is required";
    if (!formData.IONReferenceNumber.trim()) errs.IONReferenceNumber = "ION reference number is required";
    if (!formData.FromDepartment.trim()) errs.FromDepartment = "From department is required";
    if (!formData.Subject.trim()) errs.Subject = "Subject is required";
    if (toChips.length === 0) errs.AddressedTo = "At least one recipient is required";
    return errs;
  };

  // ── Save ──
  const handleSave = async () => {
    const errs = validate();
    if (Object.keys(errs).length > 0) {
      setErrors(errs);
      // Scroll to first error
      return;
    }

    try {
      setSaving(true);
      const payload = {
        InwardIONGUID: formData.InwardIONGUID || null,
        ReceivedDate: formData.ReceivedDate,
        IONDate: formData.IONDate || null,
        IONReferenceNumber: formData.IONReferenceNumber || null,
        FromDepartment: formData.FromDepartment,
        FromPersonNameWithDesignation: formData.FromPersonNameWithDesignation || null,
        Subject: formData.Subject,
        AddressedTo: formData.AddressedTo,
        CopyTo: formData.CopyTo || null,
        Remarks: formData.Remarks || null,
        AcknowledgmentSent: formData.AcknowledgmentSent,
      };

      const response = await inwardIonApi.saveInwardNote(payload);
      const saved = response.data;

      // Upload pending files
      if (pendingFiles.length > 0 && saved?.InwardNoteId) {
        for (const file of pendingFiles) {
          try {
            await inwardIonApi.uploadAttachment(saved.InwardNoteId, file);
          } catch (err) {
            console.error("Error uploading attachment:", err);
          }
        }
      }

      onSave(saved);
    } catch (err) {
      console.error("Error saving inward note:", err);
      alert("Failed to save. Please try again.");
    } finally {
      setSaving(false);
    }
  };

  const activeFileGroups = fileGroups.filter((g) => g.IsActive);
  const deptSuggestions = activeFileGroups.map((g) => g.GroupName);

  // Combined options for AddressedTo / CopyTo: groups + users as flat strings
  const addresseeOptions = [
    ...activeFileGroups.map((g) => g.GroupName),
    ...users.map((u) => u.UserName + (u.Designation ? ` — ${u.Designation}` : "")),
  ].filter((v, i, arr) => arr.indexOf(v) === i); // deduplicate

  const isPreviewableFile = (fileName) => {
    const ext = (fileName || "").split(".").pop()?.toLowerCase() || "";
    return ["pdf", "jpg", "jpeg", "png", "gif", "bmp", "webp"].includes(ext);
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

      const blob = new Blob([response.data], { type: mimeType });
      const url = window.URL.createObjectURL(blob);

      if (ext === "pdf") {
        setPreviewType("pdf");
      } else {
        setPreviewType("image");
      }
      setPreviewUrl(url);
      setPreviewName(fileName);
      setPreviewOpen(true);
    } catch (err) {
      console.error("Error viewing attachment:", err);
      alert("Failed to load attachment");
    }
  };

  const handlePreviewClose = () => {
    if (previewUrl) window.URL.revokeObjectURL(previewUrl);
    setPreviewOpen(false);
    setPreviewUrl(null);
  };

  return (
    <>
      {/* Title bar */}
      <Box
        sx={{
          px: 2, py: 1.5,
          borderBottom: 1,
          borderColor: "divider",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          bgcolor: "background.paper",
        }}
      >
        <Typography variant="h6" sx={{ fontWeight: 600, fontSize: "1.1rem" }}>
          {mode === "create" ? "Log Inward ION" : "Edit Inward ION"}
        </Typography>
        <IconButton onClick={onCancel} size="small">
          <Close />
        </IconButton>
      </Box>

      {/* Scrollable Form Content */}
      {loading ? (
        <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", flex: 1, flexDirection: "column", gap: 2, p: 2 }}>
          <CircularProgress />
          <Typography>Loading...</Typography>
        </Box>
      ) : (
        <Box ref={scrollRef} sx={{ overflowY: "auto", p: 2, flex: 1 }}>

          {/* Received Date */}
          <Box sx={{ position: "relative", mb: 2 }}>
            <TextField
              label="Received Date"
              value={formatDateDisplay(formData.ReceivedDate)}
              required
              size="small"
              error={!!errors.ReceivedDate}
              helperText={errors.ReceivedDate}
              fullWidth
              InputLabelProps={{ shrink: true }}
              onClick={() => receivedDateRef.current?.showPicker?.()}
              InputProps={{ readOnly: true, sx: { cursor: "pointer" } }}
            />
            <input
              ref={receivedDateRef}
              type="date"
              value={formData.ReceivedDate}
              onChange={(e) => handleInputChange("ReceivedDate", e.target.value)}
              tabIndex={-1}
              style={{
                position: "absolute",
                top: 0, left: 0, width: "1px", height: "1px",
                opacity: 0, pointerEvents: "none",
              }}
            />
          </Box>

          {/* ION Date */}
          <Box sx={{ position: "relative", mb: 2 }}>
            <TextField
              label="ION Date (on letter)"
              value={formatDateDisplay(formData.IONDate)}
              size="small"
              fullWidth
              InputLabelProps={{ shrink: true }}
              onClick={() => ionDateRef.current?.showPicker?.()}
              InputProps={{ readOnly: true, sx: { cursor: "pointer" } }}
            />
            <input
              ref={ionDateRef}
              type="date"
              value={formData.IONDate}
              onChange={(e) => handleInputChange("IONDate", e.target.value)}
              tabIndex={-1}
              style={{
                position: "absolute",
                top: 0, left: 0, width: "1px", height: "1px",
                opacity: 0, pointerEvents: "none",
              }}
            />
          </Box>

          {/* ION Reference Number */}
          <TextField
            label="ION Reference Number"
            value={formData.IONReferenceNumber}
            onChange={(e) => handleInputChange("IONReferenceNumber", e.target.value)}
            required
            size="small"
            fullWidth
            placeholder="Sender's ION number"
            error={!!errors.IONReferenceNumber}
            helperText={errors.IONReferenceNumber}
            sx={{ mb: 2 }}
          />

          {/* From Department */}
          <Autocomplete
            freeSolo
            options={deptSuggestions}
            inputValue={formData.FromDepartment}
            onInputChange={(_, v, reason) => {
              if (reason !== "reset") handleInputChange("FromDepartment", v);
            }}
            onChange={(_, selected) => {
              if (selected) handleInputChange("FromDepartment", selected);
            }}
            size="small"
            sx={{ mb: 2 }}
            renderInput={(params) => (
              <TextField
                {...params}
                label="From Department"
                required
                size="small"
                error={!!errors.FromDepartment}
                helperText={errors.FromDepartment}
              />
            )}
          />

          {/* From Person */}
          <TextField
            label="From (Person & Designation)"
            value={formData.FromPersonNameWithDesignation}
            onChange={(e) => handleInputChange("FromPersonNameWithDesignation", e.target.value)}
            size="small"
            fullWidth
            placeholder="e.g. Shri XYZ, Scientist-F"
            sx={{ mb: 2 }}
          />

          {/* Subject */}
          <TextField
            label="Subject"
            value={formData.Subject}
            onChange={(e) => handleInputChange("Subject", e.target.value)}
            required
            size="small"
            error={!!errors.Subject}
            helperText={errors.Subject}
            fullWidth
            multiline
            minRows={2}
            maxRows={4}
            sx={{ mb: 2 }}
          />

          {/* Addressed To */}
          <Box sx={{ mb: 2 }}>
            <ChipField
              label="Addressed To"
              chips={toChips}
              onDelete={handleToDelete}
              onEdit={handleToEdit}
              inputValue={toInput}
              onInputChange={setToInput}
              onAdd={handleToAdd}
              options={addresseeOptions}
              required
              error={!!errors.AddressedTo}
              helperText={errors.AddressedTo}
              disabled={false}
            />
          </Box>

          {/* Copy To */}
          <Box sx={{ mb: 2 }}>
            <ChipField
              label="Copy To"
              chips={copyChips}
              onDelete={handleCopyDelete}
              onEdit={handleCopyEdit}
              inputValue={copyInput}
              onInputChange={setCopyInput}
              onAdd={handleCopyAdd}
              options={addresseeOptions}
              required={false}
              error={false}
              helperText=""
              disabled={false}
            />
          </Box>

          {/* Remarks */}
          <TextField
            label="Remarks"
            value={formData.Remarks}
            onChange={(e) => handleInputChange("Remarks", e.target.value)}
            size="small"
            fullWidth
            multiline
            minRows={2}
            maxRows={4}
            sx={{ mb: 2 }}
          />

          {/* Acknowledgment */}
          <FormControlLabel
            control={
              <Checkbox
                checked={formData.AcknowledgmentSent}
                onChange={(e) => handleInputChange("AcknowledgmentSent", e.target.checked)}
              />
            }
            label="Acknowledgment Sent"
            sx={{ mb: 2 }}
          />

          <Divider sx={{ mb: 2 }} />

          {/* ===== Attachments Section ===== */}
          <Box>
            <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 1 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                <AttachFile fontSize="small" sx={{ verticalAlign: "middle", mr: 0.5 }} />
                Attachments
              </Typography>
              <Button
                variant="outlined"
                size="small"
                startIcon={<Upload />}
                component="label"
              >
                Add File
                <input
                  type="file"
                  hidden
                  multiple
                  onChange={(e) => {
                    Array.from(e.target.files).forEach((f) => handleAddPendingFile(f));
                    e.target.value = "";
                  }}
                />
              </Button>
            </Box>

            {/* Existing attachments (already saved) */}
            {attachments.length > 0 && (
              <List dense disablePadding>
                {attachments.map((att) => {
                  const canPreview = isPreviewableFile(att.Orginal_File_Name);
                  return (
                    <ListItem key={att.Attachment_Db_Key} divider sx={{ pr: 14 }}>
                      <ListItemText
                        primary={att.Orginal_File_Name}
                        primaryTypographyProps={{ variant: "body2", noWrap: true }}
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
                        <IconButton size="small" onClick={() => handleDeleteAttachment(att)} title="Delete" color="error">
                          <Delete fontSize="small" />
                        </IconButton>
                      </Box>
                    </ListItem>
                  );
                })}
              </List>
            )}

            {/* Pending files (not yet uploaded) */}
            {pendingFiles.length > 0 && (
              <>
                {attachments.length > 0 && (
                  <Typography variant="caption" color="text.secondary" sx={{ display: "block", mt: 1, mb: 0.5 }}>
                    Pending upload (will be saved with the form):
                  </Typography>
                )}
                <List dense disablePadding>
                  {pendingFiles.map((file, idx) => (
                    <ListItem key={idx} divider sx={{ bgcolor: "action.hover" }}>
                      <ListItemText
                        primary={file.name}
                        secondary={`${(file.size / 1024).toFixed(1)} KB — pending`}
                        primaryTypographyProps={{ variant: "body2" }}
                      />
                      <ListItemSecondaryAction>
                        <IconButton size="small" onClick={() => handleRemovePendingFile(idx)} color="error" title="Remove">
                          <Close fontSize="small" />
                        </IconButton>
                      </ListItemSecondaryAction>
                    </ListItem>
                  ))}
                </List>
              </>
            )}

            {attachments.length === 0 && pendingFiles.length === 0 && (
              <Typography variant="body2" color="text.secondary" sx={{ py: 1, textAlign: "center" }}>
                No attachments. Click "Add File" to attach scanned copies.
              </Typography>
            )}
          </Box>
        </Box>
      )}

      {/* Sticky Footer with Save */}
      {!loading && (
        <Box
          sx={{
            px: 2, py: 1.5,
            borderTop: 1,
            borderColor: "divider",
            bgcolor: "background.paper",
            display: "flex",
            justifyContent: "flex-end",
            gap: 1,
          }}
        >
          <Button onClick={onCancel} disabled={saving} size="small">
            Cancel
          </Button>
          <Button
            startIcon={<Save />}
            onClick={handleSave}
            disabled={saving}
            variant="contained"
            size="small"
          >
            {saving ? "Saving..." : "Save"}
          </Button>
        </Box>
      )}

      {/* Attachment Preview Dialog */}
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
    </>
  );
};

export default InwardIONFormView;
