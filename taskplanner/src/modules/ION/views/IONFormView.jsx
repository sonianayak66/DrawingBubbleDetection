import React, { useState, useEffect, useCallback, useRef, useImperativeHandle, forwardRef } from "react";
import { MentionsInput, Mention } from "react-mentions";
import {
  Box,
  Typography,
  Paper,
  Grid,
  TextField,
  Button,
  IconButton,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Chip,
  Divider,
  CircularProgress,
  Autocomplete,
  Collapse,
  Alert,
} from "@mui/material";
import {
  Save,
  Cancel,
  Add,
  Delete,
  Edit as EditIcon,
  DragIndicator,
  AttachFile,
  Close,
  Upload,
  FileDownload,
  ExpandMore,
  ExpandLess,
  Print,
  BookmarkAdd,
} from "@mui/icons-material";
import { ionApi } from "../../../services/ionApi";
import { usePermissions } from "../../../context/PermissionsContext";
import JoditRichTextEditor from "../../shared/components/Common/JoditRichTextEditor";
import { generateIONPdf } from "../utils/generateIONPdf";

// Helper: parse newline-separated text into chip array
const textToChips = (text) => {
  if (!text) return [];
  return text.split("\n").map(s => s.trim()).filter(s => s);
};

// Helper: join chip array back to newline-separated text
const chipsToText = (chips) => chips.join("\n");

// Default chip that should always appear LAST in Copy To list (if present)
const OFFICE_COPY = "Office Copy";

// Ensures Office Copy always appears at the end of the Copy To list if it exists
const reorderWithOfficeCopyLast = (chips) => {
  const withoutOfficeCopy = chips.filter(c => c !== OFFICE_COPY);
  const hasOfficeCopy = chips.includes(OFFICE_COPY);
  return hasOfficeCopy ? [...withoutOfficeCopy, OFFICE_COPY] : withoutOfficeCopy;
};

// Format YYYY-MM-DD to DD-MMM-YY for display
const MONTHS = ["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"];
const formatDateDisplay = (isoDate) => {
  if (!isoDate) return "";
  const [y, m, d] = isoDate.split("-");
  const monthIdx = parseInt(m, 10) - 1;
  if (monthIdx < 0 || monthIdx > 11) return isoDate;
  return `${d}-${MONTHS[monthIdx]}-${y.slice(-2)}`;
};

// Chip input component — defined OUTSIDE to avoid re-mount on parent re-render
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
        {chips.map((chip, i) => (
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
              sx={{ maxWidth: 200, cursor: disabled ? 'default' : 'pointer' }}
            />
          )
        ))}
        {!disabled && (
          <Autocomplete
            freeSolo
            options={options}
            getOptionLabel={(o) => typeof o === "string" ? o : o.GroupName || ""}
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

// Enclosure panel with inline file upload/download/delete
// forwardRef so parent can call flushPendingInput() before save
const EnclosurePanel = forwardRef(({ enclosures, onAdd, onDelete, onUpload, onDownload, onDeleteAttachment, onPendingFileAdd, onPendingFileRemove, attachments, pendingFiles, disabled }, ref) => {
  const [inputValue, setInputValue] = useState("");
  const [expandedId, setExpandedId] = useState(null);
  const [uploadingId, setUploadingId] = useState(null);

  // Expose flushPendingInput to parent
  useImperativeHandle(ref, () => ({
    flushPendingInput: () => {
      if (inputValue.trim()) {
        onAdd(inputValue.trim());
        setInputValue("");
        return true;
      }
      return false;
    }
  }));

  const handleAdd = () => {
    if (inputValue.trim()) {
      onAdd(inputValue.trim());
      setInputValue("");
    }
  };

  const handleFileUpload = async (event, enc, index) => {
    const file = event.target.files[0];
    if (!file) return;

    const isSaved = !!enc.EnclosureId;

    if (isSaved) {
      // Already persisted — upload immediately via API
      try {
        setUploadingId(enc.EnclosureId);
        await onUpload(enc.EnclosureId, file);
      } finally {
        setUploadingId(null);
        event.target.value = "";
      }
    } else {
      // Not yet saved — store file locally, will be uploaded on ION save
      onPendingFileAdd(index, file);
      event.target.value = "";
    }
  };

  const handleRemovePendingFile = (encIndex, fileIndex) => {
    onPendingFileRemove(encIndex, fileIndex);
  };

  return (
    <Box>
      <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 0.5 }}>
        Enclosures
      </Typography>

      {/* Inline add */}
      {!disabled && (
        <Box sx={{ display: 'flex', gap: 0.5, mb: 1 }}>
          <TextField
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            placeholder="Enclosure description..."
            size="small"
            fullWidth
            onKeyDown={(e) => {
              if (e.key === "Enter") {
                e.preventDefault();
                handleAdd();
              }
            }}
          />
          <IconButton onClick={handleAdd} color="primary" size="small" disabled={!inputValue.trim()}>
            <Add />
          </IconButton>
        </Box>
      )}

      {/* Enclosure list */}
      {enclosures.length > 0 ? (
        <List dense disablePadding>
          {enclosures.map((enc, index) => {
            const isSaved = !!enc.EnclosureId;
            const encKey = enc.EnclosureId || `new-${index}`;
            const isExpanded = expandedId === encKey;
            const encAttachments = isSaved ? (attachments[enc.EnclosureId] || []) : [];
            const encPendingFiles = (!isSaved && pendingFiles[index]) ? pendingFiles[index] : [];
            const hasFiles = enc.HasAttachment || encPendingFiles.length > 0;

            return (
              <React.Fragment key={encKey}>
                <ListItem
                  disableGutters
                  divider={!isExpanded}
                  sx={{ py: 0.25, cursor: 'pointer' }}
                  onClick={() => setExpandedId(isExpanded ? null : encKey)}
                >
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                        <Typography variant="body2" sx={{ fontSize: 13, flex: 1 }}>
                          {index + 1}. {enc.EnclosureDescription}
                        </Typography>
                        {hasFiles && (
                          <AttachFile sx={{ fontSize: 14, color: 'success.main' }} />
                        )}
                      </Box>
                    }
                  />
                  <ListItemSecondaryAction>
                    <IconButton size="small" onClick={(e) => { e.stopPropagation(); setExpandedId(isExpanded ? null : encKey); }}>
                      {isExpanded ? <ExpandLess fontSize="small" /> : <ExpandMore fontSize="small" />}
                    </IconButton>
                    {!disabled && (
                      <IconButton size="small" color="error" onClick={(e) => { e.stopPropagation(); onDelete(index); }}>
                        <Delete fontSize="small" />
                      </IconButton>
                    )}
                  </ListItemSecondaryAction>
                </ListItem>

                {/* Expand panel — works for both saved and unsaved enclosures */}
                <Collapse in={isExpanded} timeout="auto" unmountOnExit>
                  <Box sx={{ pl: 2, pr: 1, py: 1, bgcolor: 'grey.50', borderBottom: '1px solid', borderColor: 'divider' }}>
                    {/* Upload button */}
                    {!disabled && (
                      <Box sx={{ mb: 1 }}>
                        <input
                          accept=".pdf,.doc,.docx,.xls,.xlsx,.jpg,.jpeg,.png,.gif"
                          style={{ display: "none" }}
                          id={`enc-upload-${encKey}`}
                          type="file"
                          onChange={(e) => handleFileUpload(e, enc, index)}
                        />
                        <label htmlFor={`enc-upload-${encKey}`}>
                          <Button
                            component="span"
                            startIcon={<Upload />}
                            size="small"
                            variant="outlined"
                            disabled={isSaved && uploadingId === enc.EnclosureId}
                            sx={{ fontSize: 12 }}
                          >
                            {(isSaved && uploadingId === enc.EnclosureId) ? "Uploading..." : "Upload File"}
                          </Button>
                        </label>
                      </Box>
                    )}

                    {/* Saved attachment list (from server) */}
                    {encAttachments.length > 0 && (
                      <List dense disablePadding>
                        {encAttachments.map((att) => (
                          <ListItem key={att.Attachment_Db_Key} disablePadding sx={{ py: 0.25 }}>
                            <ListItemText
                              primary={
                                <Typography variant="body2" sx={{ fontSize: 12 }}>
                                  <AttachFile sx={{ fontSize: 12, mr: 0.5, verticalAlign: "middle" }} />
                                  {att.Orginal_File_Name}
                                </Typography>
                              }
                              secondary={
                                att.UploadedByName && (
                                  <Typography variant="caption" color="text.secondary" sx={{ fontSize: 10 }}>
                                    {att.UploadedByName} {att.Updated_on ? `• ${new Date(att.Updated_on).toLocaleDateString()}` : ''}
                                  </Typography>
                                )
                              }
                            />
                            <Box sx={{ display: 'flex', gap: 0.25 }}>
                              <IconButton size="small" color="primary" onClick={() => onDownload(att.Attachment_Db_Key, att.Orginal_File_Name)} title="Download">
                                <FileDownload sx={{ fontSize: 16 }} />
                              </IconButton>
                              {!disabled && (
                                <IconButton size="small" color="error" onClick={() => onDeleteAttachment(att.Attachment_Db_Key, enc.EnclosureId)} title="Delete">
                                  <Delete sx={{ fontSize: 16 }} />
                                </IconButton>
                              )}
                            </Box>
                          </ListItem>
                        ))}
                      </List>
                    )}

                    {/* Pending files (not yet uploaded — will be sent on Save) */}
                    {encPendingFiles.length > 0 && (
                      <List dense disablePadding>
                        {encPendingFiles.map((file, fileIdx) => (
                          <ListItem key={`pending-${fileIdx}`} disablePadding sx={{ py: 0.25 }}>
                            <ListItemText
                              primary={
                                <Typography variant="body2" sx={{ fontSize: 12 }}>
                                  <AttachFile sx={{ fontSize: 12, mr: 0.5, verticalAlign: "middle" }} />
                                  {file.name}
                                </Typography>
                              }
                              secondary={
                                <Typography variant="caption" color="warning.main" sx={{ fontSize: 10 }}>
                                  Pending upload (will be saved with ION)
                                </Typography>
                              }
                            />
                            {!disabled && (
                              <IconButton size="small" color="error" onClick={() => handleRemovePendingFile(index, fileIdx)} title="Remove">
                                <Delete sx={{ fontSize: 16 }} />
                              </IconButton>
                            )}
                          </ListItem>
                        ))}
                      </List>
                    )}

                    {encAttachments.length === 0 && encPendingFiles.length === 0 && (
                      <Typography variant="caption" color="text.secondary" sx={{ fontStyle: "italic" }}>
                        No files attached yet
                      </Typography>
                    )}
                  </Box>
                </Collapse>
              </React.Fragment>
            );
          })}
        </List>
      ) : (
        <Typography variant="body2" color="text.secondary" sx={{ fontStyle: "italic", py: 1, textAlign: "center" }}>
          No enclosures added
        </Typography>
      )}
    </Box>
  );
});

// Communication Reference list with # mention search in input
const CommRefListField = forwardRef(({ label, items, onAdd, onDelete, onUpdate, onReorder, placeholder, disabled, fetchDemands }, ref) => {
  const [inputValue, setInputValue] = useState("");
  const [editIndex, setEditIndex] = useState(null); // Index of item being edited
  const [dragIndex, setDragIndex] = useState(null); // Index of item currently being dragged
  const [dragOverIndex, setDragOverIndex] = useState(null); // Index currently hovered during drag

  // Strip react-mentions markup: #[display](id) → display
  const stripMentionMarkup = (text) => {
    if (!text) return text;
    return text.replace(/#\[([^\]]+)\]\([^)]+\)/g, "$1");
  };

  // Expose flushPendingInput to parent
  useImperativeHandle(ref, () => ({
    flushPendingInput: () => {
      const clean = stripMentionMarkup(inputValue).trim();
      if (clean) {
        if (editIndex !== null) {
          onUpdate(editIndex, clean);
          setEditIndex(null);
        } else {
          onAdd(clean);
        }
        setInputValue("");
        return true;
      }
      return false;
    }
  }));

  const handleAdd = () => {
    const clean = stripMentionMarkup(inputValue).trim();
    if (clean) {
      if (editIndex !== null) {
        onUpdate(editIndex, clean);
        setEditIndex(null);
      } else {
        onAdd(clean);
      }
      setInputValue("");
    }
  };

  const handleEditClick = (index) => {
    setInputValue(items[index]);
    setEditIndex(index);
  };

  const handleCancelEdit = () => {
    setInputValue("");
    setEditIndex(null);
  };

  // Drag and drop handlers
  const handleDragStart = (index) => {
    setDragIndex(index);
  };

  const handleDragOver = (e, index) => {
    e.preventDefault();
    if (index !== dragOverIndex) {
      setDragOverIndex(index);
    }
  };

  const handleDragEnd = () => {
    setDragIndex(null);
    setDragOverIndex(null);
  };

  const handleDrop = (e, dropIndex) => {
    e.preventDefault();
    if (dragIndex === null || dragIndex === dropIndex) {
      setDragIndex(null);
      setDragOverIndex(null);
      return;
    }
    if (onReorder) {
      onReorder(dragIndex, dropIndex);
    }
    setDragIndex(null);
    setDragOverIndex(null);
  };

  return (
    <Box>
      <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 0.5 }}>
        {label}
      </Typography>
      {!disabled && (
        <Box sx={{ display: 'flex', gap: 0.5, mb: 0.5, alignItems: 'flex-start' }}>
          <Box sx={{ flex: 1 }}>
            <MentionsInput
              value={inputValue}
              onChange={(e, newValue) => setInputValue(newValue)}
              placeholder={placeholder}
              singleLine
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  e.preventDefault();
                  handleAdd();
                }
              }}
              style={{
                control: {
                  fontSize: 13,
                  fontFamily: '"Roboto","Helvetica","Arial",sans-serif',
                },
                input: {
                  padding: "7px 10px",
                  border: "1px solid #c4c4c4",
                  borderRadius: 4,
                  fontSize: 13,
                  lineHeight: 1.5,
                  outline: "none",
                  color: "#000",
                },
                highlighter: {
                  padding: "7px 10px",
                  border: "1px solid transparent",
                  color: "transparent",
                },
                suggestions: {
                  list: {
                    backgroundColor: "white",
                    border: "1px solid #e0e0e0",
                    borderRadius: 4,
                    boxShadow: "0 2px 8px rgba(0,0,0,0.15)",
                    fontSize: 13,
                    maxHeight: 200,
                    overflow: "auto",
                    zIndex: 1300,
                  },
                  item: {
                    padding: "8px 12px",
                    borderBottom: "1px solid #f0f0f0",
                    "&focused": {
                      backgroundColor: "#e3f2fd",
                    },
                  },
                },
              }}
            >
              <Mention
                trigger="#"
                data={fetchDemands}
                displayTransform={(id, display) => `${display}`}
                markup="#[__display__](__id__)"
                appendSpaceOnAdd
                renderSuggestion={(suggestion) => (
                  <div>
                    <div style={{ fontWeight: 500 }}>{suggestion.display}</div>
                    <div style={{ fontSize: 11, color: "#888" }}>
                      {suggestion.demandNo} — {suggestion.description}
                    </div>
                  </div>
                )}
              />
            </MentionsInput>
          </Box>
          <IconButton
            onClick={handleAdd}
            color="primary"
            size="small"
            disabled={!stripMentionMarkup(inputValue).trim()}
            sx={{ mt: '2px' }}
            title={editIndex !== null ? "Update reference" : "Add reference"}
          >
            <Add />
          </IconButton>
          {editIndex !== null && (
            <IconButton
              onClick={handleCancelEdit}
              size="small"
              sx={{ mt: '2px' }}
              title="Cancel edit"
            >
              <Close fontSize="small" />
            </IconButton>
          )}
        </Box>
      )}
      {!disabled && (
        <Typography variant="caption" color="text.secondary" sx={{ mb: 1, display: 'block' }}>
          {editIndex !== null
            ? `Editing reference #${editIndex + 1}. Press Enter or + to update.`
            : "Type # to search MMG demands. Press Enter or + to add. Drag to reorder."}
        </Typography>
      )}
      {items.length > 0 ? (
        <List dense disablePadding>
          {items.map((item, index) => {
            const isDragging = dragIndex === index;
            const isDragOver = dragOverIndex === index && dragIndex !== null && dragIndex !== index;
            return (
              <ListItem
                key={index}
                divider
                disableGutters
                draggable={!disabled && editIndex === null}
                onDragStart={() => handleDragStart(index)}
                onDragOver={(e) => handleDragOver(e, index)}
                onDragEnd={handleDragEnd}
                onDrop={(e) => handleDrop(e, index)}
                sx={{
                  py: 0.25,
                  opacity: isDragging ? 0.4 : 1,
                  borderTop: isDragOver && dragIndex > index ? '2px solid #1976d2' : undefined,
                  borderBottom: isDragOver && dragIndex < index ? '2px solid #1976d2' : undefined,
                  cursor: !disabled && editIndex === null ? 'move' : 'default',
                  backgroundColor: editIndex === index ? 'action.selected' : undefined,
                  pr: !disabled ? 9 : 0,
                }}
              >
                {!disabled && (
                  <DragIndicator
                    fontSize="small"
                    sx={{ color: 'text.disabled', mr: 0.5, flexShrink: 0 }}
                  />
                )}
                <ListItemText
                  primary={
                    <Typography variant="body2" sx={{ fontSize: 13 }}>
                      {index + 1}. {item}
                    </Typography>
                  }
                />
                {!disabled && (
                  <ListItemSecondaryAction>
                    <IconButton
                      edge="end"
                      onClick={() => handleEditClick(index)}
                      color="primary"
                      size="small"
                      disabled={editIndex !== null && editIndex !== index}
                      title="Edit reference"
                      sx={{ mr: 0.5 }}
                    >
                      <EditIcon fontSize="small" />
                    </IconButton>
                    <IconButton
                      edge="end"
                      onClick={() => onDelete(index)}
                      color="error"
                      size="small"
                      disabled={editIndex !== null}
                      title="Delete reference"
                    >
                      <Delete fontSize="small" />
                    </IconButton>
                  </ListItemSecondaryAction>
                )}
              </ListItem>
            );
          })}
        </List>
      ) : (
        <Typography variant="body2" color="text.secondary" sx={{ fontStyle: "italic", py: 1, textAlign: "center" }}>
          None added
        </Typography>
      )}
    </Box>
  );
});

const IONFormView = ({
  ionGuid = null,
  open = false,
  onSave,
  onCancel,
  mode = "create",
  templateData = null,
}) => {
  // Form state
  const [formData, setFormData] = useState({
    IONGUID: null,
    IONNumber: "",
    Office: "",
    GroupGUID: "",
    IONDate: new Date().toISOString().split("T")[0],
    DestinationId: "",
    Subject: "",
    CommunicationReference: "",
    IONBody: "",
    ToAddress: "",
    CopyTo: "",
    PreparedBy: "",
    PreparedByDesignation: "",
    SentThrough: "",
    Status: "Draft",
  });

  // Chip-based recipients
  const [toChips, setToChips] = useState([]);
  const [copyToChips, setCopyToChips] = useState([]);
  const [toInputValue, setToInputValue] = useState("");
  const [copyToInputValue, setCopyToInputValue] = useState("");

  // Communication reference items (displayed as list, stored as newline-separated string)
  const [commRefItems, setCommRefItems] = useState([]);

  // Configuration data
  const [officeConfig, setOfficeConfig] = useState([]);
  const [destinations, setDestinations] = useState([]);
  const [users, setUsers] = useState([]);
  const [enclosures, setEnclosures] = useState([]);
  const [fileGroups, setFileGroups] = useState([]);

  // UI state
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});
  const [deletedEnclosures, setDeletedEnclosures] = useState([]);
  const [enclosureAttachments, setEnclosureAttachments] = useState({});
  // Pending files for unsaved enclosures: { [localIndex]: [File, File, ...] }
  const [pendingEnclosureFiles, setPendingEnclosureFiles] = useState({});

  // Print preview state
  const [previewOpen, setPreviewOpen] = useState(false);
  const [previewBlobUrl, setPreviewBlobUrl] = useState(null);
  const [previewLoading, setPreviewLoading] = useState(false);

  // Save as Template state — admin-only feature, lets a power user
  // capture the current form as a reusable template.
  const [saveAsTplOpen, setSaveAsTplOpen] = useState(false);
  const [saveAsTplName, setSaveAsTplName] = useState("");
  const [saveAsTplDescription, setSaveAsTplDescription] = useState("");
  const [saveAsTplSaving, setSaveAsTplSaving] = useState(false);

  // Dirty tracking — becomes true when the user makes any local (unsaved) change.
  // Used to (1) show a warning banner in the preview dialog and
  // (2) prompt the user before closing the edit dialog without saving.
  const [isDirty, setIsDirty] = useState(false);

  // Refs for flushing pending input on save
  const enclosurePanelRef = useRef(null);
  const commRefPanelRef = useRef(null);

  // Date input ref for hidden native picker
  const dateInputRef = useRef(null);

  // Keep a ref to latest formData so handleSave always reads fresh values after flush
  const formDataRef = useRef(formData);
  formDataRef.current = formData;

  useEffect(() => {
    loadConfigurationData();

    if (mode === "create") {
      // Pre-fill from template if provided, else blank.
      // Template fields use ...Template suffix on the wire — strip it here.
      let tplToChips = [];
      let tplCopyChips = [OFFICE_COPY];
      let tplCommRefs = [];
      let tplEnclosures = [];

      if (templateData) {
        tplToChips = textToChips(templateData.ToAddressTemplate || "");
        const copyChipsFromTpl = textToChips(templateData.CopyToTemplate || "");
        // Always keep Office Copy at end if present anywhere
        tplCopyChips = reorderWithOfficeCopyLast(
          copyChipsFromTpl.includes(OFFICE_COPY) ? copyChipsFromTpl : [...copyChipsFromTpl, OFFICE_COPY]
        );
        tplCommRefs = textToChips(templateData.CommRefTemplate || "");
        try {
          const arr = JSON.parse(templateData.EnclosuresTemplate || "[]");
          if (Array.isArray(arr)) {
            tplEnclosures = arr.filter(Boolean).map(desc => ({
              EnclosureGUID: null,
              EnclosureDescription: String(desc),
              HasAttachment: false,
              isNew: true,
            }));
          }
        } catch {
          // Ignore malformed template enclosures JSON
        }

        // Bump usage counter (fire and forget)
        if (templateData.TemplateGUID) {
          ionApi.incrementIONTemplateUse(templateData.TemplateGUID).catch(() => {});
        }
      }

      setFormData({
        IONGUID: null,
        Office: "",
        GroupGUID: templateData?.GroupGUID || "",
        IONDate: new Date().toISOString().split("T")[0],
        DestinationId: "",
        Subject: templateData?.SubjectTemplate || "",
        CommunicationReference: chipsToText(tplCommRefs),
        IONBody: templateData?.IONBodyTemplate || "",
        ToAddress: chipsToText(tplToChips),
        CopyTo: chipsToText(tplCopyChips),
        PreparedBy: "",
        PreparedByDesignation: "",
        SentThrough: "",
        Status: "Draft",
      });
      setEnclosures(tplEnclosures);
      setDeletedEnclosures([]);
      setEnclosureAttachments({});
      setPendingEnclosureFiles({});
      setToChips(tplToChips);
      setCopyToChips(tplCopyChips);
      setCommRefItems(tplCommRefs);
      // Pre-filled state is treated as a fresh starting point — first edit will mark dirty.
      setIsDirty(false);
    } else if (ionGuid && mode === "edit") {
      loadIONData();
    }
  }, [open, ionGuid, mode, templateData]);

  const loadConfigurationData = async () => {
    try {
      const [officesResponse, destinationsResponse, fileGroupsResponse, internalUsersResponse] =
        await Promise.all([
          ionApi.getOfficeConfig(),
          ionApi.getDestinations(),
          ionApi.getFileGroups(),
          ionApi.getInternalUsers(),
        ]);

      setOfficeConfig(officesResponse.data || []);
      setDestinations(destinationsResponse.data || []);
      setFileGroups(fileGroupsResponse.data || []);
      setUsers(internalUsersResponse.data || []);
    } catch (error) {
      console.error("Error loading configuration data:", error);
    }
  };

  const loadIONData = async () => {
    try {
      setLoading(true);

      const [ionResponse, enclosuresResponse] = await Promise.all([
        ionApi.getIONNoteDetail(ionGuid),
        ionApi.getEnclosures(ionGuid),
      ]);

      if (ionResponse.data) {
        const ionData = ionResponse.data;

        const ionDate = ionData.IONDate
          ? ionData.IONDate.split('T')[0]
          : new Date().toISOString().split('T')[0];

        setFormData({
          ...ionData,
          IONDate: ionDate
        });

        setToChips(textToChips(ionData.ToAddress));
        setCopyToChips(textToChips(ionData.CopyTo));
        setCommRefItems(textToChips(ionData.CommunicationReference));
      }

      const loadedEnclosures = enclosuresResponse.data || [];
      setEnclosures(loadedEnclosures);
      setDeletedEnclosures([]);
      setEnclosureAttachments({});
      setPendingEnclosureFiles({});
      setIsDirty(false);

      // Load attachments for enclosures that have files
      const withAttachments = loadedEnclosures.filter(e => e.HasAttachment);
      if (withAttachments.length > 0) {
        const results = await Promise.all(
          withAttachments.map(e =>
            ionApi.getEnclosureAttachments(e.EnclosureId)
              .then(res => ({ id: e.EnclosureId, data: res.data || [] }))
              .catch(() => ({ id: e.EnclosureId, data: [] }))
          )
        );
        const attMap = {};
        results.forEach(r => { attMap[r.id] = r.data; });
        setEnclosureAttachments(attMap);
      }
    } catch (error) {
      console.error("Error loading ION data:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (field, value) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }));
    setIsDirty(true);
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  };

  // --- Chip handlers for To / Copy To ---
  const handleAddToChip = useCallback((chipValue) => {
    if (!chipValue || !chipValue.trim()) return;
    const val = chipValue.trim();
    setToChips(prev => {
      if (prev.includes(val)) return prev;
      const updated = [...prev, val];
      setFormData(f => ({ ...f, ToAddress: chipsToText(updated) }));
      return updated;
    });
    setToInputValue("");
    setIsDirty(true);
  }, []);

  const handleDeleteToChip = useCallback((index) => {
    setToChips(prev => {
      const updated = prev.filter((_, i) => i !== index);
      setFormData(f => ({ ...f, ToAddress: chipsToText(updated) }));
      return updated;
    });
    setIsDirty(true);
  }, []);

  const handleAddCopyToChip = useCallback((chipValue) => {
    if (!chipValue || !chipValue.trim()) return;
    const val = chipValue.trim();
    setCopyToChips(prev => {
      if (prev.includes(val)) return prev;
      // Insert new chip, then move "Office Copy" to the end if present
      const updated = reorderWithOfficeCopyLast([...prev, val]);
      setFormData(f => ({ ...f, CopyTo: chipsToText(updated) }));
      return updated;
    });
    setCopyToInputValue("");
    setIsDirty(true);
  }, []);

  const handleDeleteCopyToChip = useCallback((index) => {
    setCopyToChips(prev => {
      const updated = prev.filter((_, i) => i !== index);
      setFormData(f => ({ ...f, CopyTo: chipsToText(updated) }));
      return updated;
    });
    setIsDirty(true);
  }, []);

  const handleEditToChip = useCallback((index, newValue) => {
    setToChips(prev => {
      const updated = [...prev];
      updated[index] = newValue;
      setFormData(f => ({ ...f, ToAddress: chipsToText(updated) }));
      return updated;
    });
    setIsDirty(true);
  }, []);

  const handleEditCopyToChip = useCallback((index, newValue) => {
    setCopyToChips(prev => {
      const updated = [...prev];
      updated[index] = newValue;
      // Keep Office Copy at the end after edits
      const reordered = reorderWithOfficeCopyLast(updated);
      setFormData(f => ({ ...f, CopyTo: chipsToText(reordered) }));
      return reordered;
    });
    setIsDirty(true);
  }, []);

  // Search demands for # mention with debounce (300ms)
  const debounceTimerRef = useRef(null);
  const fetchDemands = useCallback((query, callback) => {
    if (!query || query.length < 2) return;

    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
    }

    debounceTimerRef.current = setTimeout(async () => {
      try {
        const response = await ionApi.searchDemands(query);
        const suggestions = (response.data || []).map((d) => ({
          id: d.DemandDbKey,
          display: d.MMG_File_No,
          demandNo: d.Demand_No,
          description: d.Item_Description,
        }));
        callback(suggestions);
      } catch (error) {
        console.error("Error searching demands:", error);
        callback([]);
      }
    }, 300);
  }, []);

  // --- Communication Reference handlers ---
  const handleAddCommRef = useCallback((value) => {
    setCommRefItems(prev => {
      const updated = [...prev, value];
      setFormData(f => ({ ...f, CommunicationReference: chipsToText(updated) }));
      return updated;
    });
    setIsDirty(true);
  }, []);

  const handleDeleteCommRef = useCallback((index) => {
    setCommRefItems(prev => {
      const updated = prev.filter((_, i) => i !== index);
      setFormData(f => ({ ...f, CommunicationReference: chipsToText(updated) }));
      return updated;
    });
    setIsDirty(true);
  }, []);

  const handleUpdateCommRef = useCallback((index, value) => {
    setCommRefItems(prev => {
      const updated = prev.map((item, i) => (i === index ? value : item));
      setFormData(f => ({ ...f, CommunicationReference: chipsToText(updated) }));
      return updated;
    });
    setIsDirty(true);
  }, []);

  const handleReorderCommRef = useCallback((fromIndex, toIndex) => {
    setCommRefItems(prev => {
      const updated = [...prev];
      const [moved] = updated.splice(fromIndex, 1);
      updated.splice(toIndex, 0, moved);
      setFormData(f => ({ ...f, CommunicationReference: chipsToText(updated) }));
      return updated;
    });
    setIsDirty(true);
  }, []);

  const validateForm = () => {
    const newErrors = {};

    if (!formData.GroupGUID) {
      newErrors.GroupGUID = "File Group is required";
    }
    if (!formData.IONDate) {
      newErrors.IONDate = "ION Date is required";
    }
    if (!formData.Subject || !formData.Subject.trim()) {
      newErrors.Subject = "Subject is required";
    }
    if (!formData.IONBody || !formData.IONBody.trim() || formData.IONBody.trim() === "<p><br></p>") {
      newErrors.IONBody = "ION Body is required";
    }
    if (toChips.length === 0) {
      newErrors.ToAddress = "At least one recipient is required";
    }
    if (!formData.PreparedBy) {
      newErrors.PreparedBy = "Prepared By is required";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = async (status = "Draft") => {
    // Flush any pending text in enclosure/comm ref inputs before validating
    enclosurePanelRef.current?.flushPendingInput();
    commRefPanelRef.current?.flushPendingInput();

    // Let React process the state updates from flush
    await new Promise(r => setTimeout(r, 0));

    if (!validateForm()) return;

    try {
      setLoading(true);

      // Use ref to get the latest formData (includes flushed values)
      const latestData = formDataRef.current;
      const saveData = {
        ...latestData,
        SentThrough: latestData.SentThrough || null,
        PreparedBy: latestData.PreparedBy || null,
        Status: status,
      };

      const response = await ionApi.saveIONNote(saveData);

      if (response.data) {
        for (const enclosureGuid of deletedEnclosures) {
          try {
            await ionApi.deleteEnclosure(enclosureGuid);
          } catch (error) {
            console.error("Error deleting enclosure:", error);
          }
        }

        setDeletedEnclosures([]);

        // Save new enclosures and upload any pending files
        for (let i = 0; i < enclosures.length; i++) {
          const enclosure = enclosures[i];
          let enclosureId = enclosure.EnclosureId;

          if (enclosure.isNew) {
            const encFiles = pendingEnclosureFiles[i] || [];
            const encResponse = await ionApi.saveEnclosure({
              EnclosureGUID: null,
              IONGUID: response.data.IONGUID,
              EnclosureDescription: enclosure.EnclosureDescription,
              HasAttachment: encFiles.length > 0,
            });
            if (encResponse.data) {
              enclosureId = encResponse.data.EnclosureId;
            }
          }

          // Upload any pending files for this enclosure (new or existing)
          const filesToUpload = pendingEnclosureFiles[i] || [];
          if (enclosureId && filesToUpload.length > 0) {
            for (const file of filesToUpload) {
              try {
                await ionApi.uploadEnclosureAttachment(enclosureId, file);
              } catch (uploadErr) {
                console.error("Error uploading pending file:", uploadErr);
              }
            }
          }
        }

        setPendingEnclosureFiles({});
        setIsDirty(false);
        onSave(response.data);
      }
    } catch (error) {
      console.error("Error saving ION:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleAddEnclosureInline = useCallback(async (text) => {
    // If ION already exists (edit mode), save enclosure immediately to get EnclosureId
    if (formData.IONGUID) {
      try {
        const response = await ionApi.saveEnclosure({
          EnclosureGUID: null,
          IONGUID: formData.IONGUID,
          EnclosureDescription: text,
          HasAttachment: false,
        });
        if (response.data) {
          // Add the saved enclosure with its EnclosureId from server
          setEnclosures(prev => [...prev, {
            ...response.data,
            isNew: false, // Already saved
          }]);
        }
      } catch (error) {
        console.error("Error saving enclosure:", error);
        // Fallback: add locally
        setEnclosures(prev => [...prev, {
          EnclosureGUID: null,
          EnclosureDescription: text,
          HasAttachment: false,
          isNew: true,
        }]);
      }
    } else {
      // Create mode: save locally, will be persisted when ION is saved
      setEnclosures(prev => [...prev, {
        EnclosureGUID: null,
        EnclosureDescription: text,
        HasAttachment: false,
        isNew: true,
      }]);
    }
    setIsDirty(true);
  }, [formData.IONGUID]);

  const handleEnclosureUpload = useCallback(async (enclosureId, file) => {
    try {
      await ionApi.uploadEnclosureAttachment(enclosureId, file);
      // Refresh attachments for this enclosure
      const res = await ionApi.getEnclosureAttachments(enclosureId);
      setEnclosureAttachments(prev => ({ ...prev, [enclosureId]: res.data || [] }));
      // Update HasAttachment flag
      setEnclosures(prev => prev.map(e =>
        e.EnclosureId === enclosureId ? { ...e, HasAttachment: true } : e
      ));
    } catch (error) {
      console.error("Error uploading attachment:", error);
      alert("Failed to upload attachment");
    }
  }, []);

  const handleDownloadAttachment = useCallback(async (attachmentId, fileName) => {
    try {
      const response = await ionApi.downloadEnclosureAttachment(attachmentId);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", fileName);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error("Error downloading attachment:", error);
      alert("Failed to download attachment");
    }
  }, []);

  const handleDeleteAttachment = useCallback(async (attachmentId, enclosureId) => {
    if (!window.confirm("Delete this attachment?")) return;
    try {
      await ionApi.deleteEnclosureAttachment(attachmentId);
      // Refresh attachments
      const res = await ionApi.getEnclosureAttachments(enclosureId);
      const updatedAtts = res.data || [];
      setEnclosureAttachments(prev => ({ ...prev, [enclosureId]: updatedAtts }));
      // Update HasAttachment flag if no more attachments
      if (updatedAtts.length === 0) {
        setEnclosures(prev => prev.map(e =>
          e.EnclosureId === enclosureId ? { ...e, HasAttachment: false } : e
        ));
      }
    } catch (error) {
      console.error("Error deleting attachment:", error);
      alert("Failed to delete attachment");
    }
  }, []);

  const handleDeleteEnclosure = useCallback((index) => {
    setEnclosures(prev => {
      const enclosureToDelete = prev[index];
      if (enclosureToDelete.EnclosureGUID) {
        setDeletedEnclosures(d => [...d, enclosureToDelete.EnclosureGUID]);
      }
      return prev.filter((_, i) => i !== index);
    });
    // Also clean up any pending files for this index and re-index remaining
    setPendingEnclosureFiles(prev => {
      const updated = {};
      Object.keys(prev).forEach(key => {
        const k = parseInt(key);
        if (k < index) updated[k] = prev[k];
        else if (k > index) updated[k - 1] = prev[k]; // Shift down
        // k === index is deleted
      });
      return updated;
    });
    setIsDirty(true);
  }, []);

  // Add a pending file for an unsaved enclosure
  const handlePendingFileAdd = useCallback((encIndex, file) => {
    setPendingEnclosureFiles(prev => ({
      ...prev,
      [encIndex]: [...(prev[encIndex] || []), file],
    }));
    setIsDirty(true);
  }, []);

  // Remove a pending file from an unsaved enclosure
  const handlePendingFileRemove = useCallback((encIndex, fileIndex) => {
    setPendingEnclosureFiles(prev => {
      const files = [...(prev[encIndex] || [])];
      files.splice(fileIndex, 1);
      if (files.length === 0) {
        const { [encIndex]: _, ...rest } = prev;
        return rest;
      }
      return { ...prev, [encIndex]: files };
    });
    setIsDirty(true);
  }, []);

  const { hasPermission } = usePermissions();
  const isAdmin = hasPermission("ION_Admin");
  const isDocHandler = hasPermission("ION_SupportOperator");
  const isReadOnly = mode === "view" || (!(isAdmin || isDocHandler) && formData.Status === "Approved");

  const activeFileGroups = fileGroups.filter(g => g.IsActive);

  // Build an ionData-shaped object from the current (possibly unsaved) form state
  // and generate a PDF preview using the same template as IONDetailView.
  const handlePreview = async () => {
    try {
      // Flush any pending text in inline comm ref / enclosure inputs so the preview
      // reflects what the user just typed but hasn't added as a chip yet.
      enclosurePanelRef.current?.flushPendingInput();
      commRefPanelRef.current?.flushPendingInput();
      await new Promise(r => setTimeout(r, 0));

      setPreviewLoading(true);
      const latest = formDataRef.current;

      // Resolve Prepared By / Sent Through user details from the loaded users list
      const preparedUser = users.find(u => u.UserDbkey === latest.PreparedBy) || null;
      const sentThroughUser = users.find(u => u.UserDbkey === latest.SentThrough) || null;

      const previewData = {
        IONNumber: latest.IONNumber || "",
        IONDate: latest.IONDate,
        Subject: latest.Subject || "",
        CommunicationReference: latest.CommunicationReference || "",
        IONBody: latest.IONBody || "",
        ToAddress: latest.ToAddress || "",
        CopyTo: latest.CopyTo || "",
        PreparedBy: latest.PreparedBy,
        PreparedByName: preparedUser?.UserName || "",
        PreparedByDesignation: preparedUser?.Designation || latest.PreparedByDesignation || "",
        SentThrough: latest.SentThrough,
        SentThroughName: sentThroughUser?.UserName || "",
        SentThroughDesignation: sentThroughUser?.Designation || "",
      };

      const blobUrl = await generateIONPdf(previewData, enclosures);

      // Release previous preview URL before replacing
      if (previewBlobUrl) {
        URL.revokeObjectURL(previewBlobUrl);
      }
      setPreviewBlobUrl(blobUrl);
      setPreviewOpen(true);
    } catch (error) {
      console.error("Error generating preview:", error);
      alert("Failed to generate preview. Please try again.");
    } finally {
      setPreviewLoading(false);
    }
  };

  const handleClosePreview = () => {
    if (previewBlobUrl) {
      URL.revokeObjectURL(previewBlobUrl);
      setPreviewBlobUrl(null);
    }
    setPreviewOpen(false);
  };

  // Save current form contents as a new reusable template.
  // Only fields that make sense as templates are persisted — the document
  // metadata (ION number, status, prepared by, dates, attachments) is excluded.
  const handleOpenSaveAsTemplate = () => {
    // Flush any pending inline input first so what the user sees is what gets saved.
    enclosurePanelRef.current?.flushPendingInput();
    commRefPanelRef.current?.flushPendingInput();
    // Pre-fill the template name from the current Subject as a sensible default.
    setSaveAsTplName(formDataRef.current.Subject || "");
    setSaveAsTplDescription("");
    setSaveAsTplOpen(true);
  };

  const handleCloseSaveAsTemplate = () => {
    if (saveAsTplSaving) return;
    setSaveAsTplOpen(false);
  };

  const handleConfirmSaveAsTemplate = async () => {
    const name = saveAsTplName.trim();
    if (!name) return;
    try {
      setSaveAsTplSaving(true);
      const latest = formDataRef.current;
      // Convert in-memory enclosure list (descriptions only) to JSON array string
      const enclosureDescriptions = (enclosures || []).map(e => e.EnclosureDescription).filter(Boolean);
      await ionApi.saveIONTemplate({
        TemplateGUID: null, // new template
        TemplateName: name,
        Description: saveAsTplDescription.trim(),
        GroupGUID: latest.GroupGUID || "",
        SubjectTemplate: latest.Subject || "",
        IONBodyTemplate: latest.IONBody || "",
        ToAddressTemplate: latest.ToAddress || "",
        CopyToTemplate: latest.CopyTo || "",
        CommRefTemplate: latest.CommunicationReference || "",
        EnclosuresTemplate: JSON.stringify(enclosureDescriptions),
        IsActive: true,
      });
      setSaveAsTplOpen(false);
      alert(`Template "${name}" saved. It will appear in the Create from Template picker.`);
    } catch (error) {
      console.error("Error saving template:", error);
      alert("Failed to save template. You may not have permission, or the server returned an error.");
    } finally {
      setSaveAsTplSaving(false);
    }
  };

  // Intercept close actions so users are warned about unsaved changes.
  // In view mode or when nothing has been changed, close immediately.
  const handleCancelClick = () => {
    if (isDirty && mode !== "view") {
      const ok = window.confirm(
        "You have unsaved changes.\n\nIf you close now, your changes will NOT be saved. Click Cancel to go back and then Save, or OK to close without saving."
      );
      if (!ok) return;
    }
    onCancel();
  };

  // Clean up preview blob URL if the edit dialog is closed while preview is open
  useEffect(() => {
    if (!open && previewBlobUrl) {
      URL.revokeObjectURL(previewBlobUrl);
      setPreviewBlobUrl(null);
      setPreviewOpen(false);
    }
  }, [open, previewBlobUrl]);

  return (
    <Dialog
      open={open}
      onClose={() => {}} // Prevent closing on backdrop click
      disableEscapeKeyDown    // Prevent closing on Escape
      maxWidth={false}
      PaperProps={{
        sx: {
          width: '90vw',
          height: '95vh',
          maxWidth: '90vw',
          maxHeight: '95vh',
        }
      }}
    >
      <DialogTitle sx={{
        borderBottom: 1,
        borderColor: 'divider',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        py: 1.5
      }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            {mode === "create"
              ? "Create Internal Office Note"
              : mode === "edit"
              ? "Edit Internal Office Note"
              : "View Internal Office Note"}
          </Typography>
          {formData.IONGUID && (
            <Chip
              label={formData.Status}
              color={
                formData.Status === "Approved"
                  ? "success"
                  : formData.Status === "Rejected"
                  ? "error"
                  : "default"
              }
              size="small"
            />
          )}
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Button
            startIcon={previewLoading ? <CircularProgress size={16} color="inherit" /> : <Print />}
            onClick={handlePreview}
            disabled={loading || previewLoading}
            variant="outlined"
            size="small"
          >
            Preview ION
          </Button>
          {!isReadOnly && isAdmin && (
            <Button
              startIcon={<BookmarkAdd />}
              onClick={handleOpenSaveAsTemplate}
              disabled={loading}
              variant="outlined"
              color="secondary"
              size="small"
              title="Save the current contents as a reusable template"
            >
              Save as new Template
            </Button>
          )}
          {!isReadOnly && (
            <Button
              startIcon={<Save />}
              onClick={() => handleSave(formData.Status === "Approved" ? "Approved" : "Draft")}
              disabled={loading}
              variant="contained"
              size="small"
            >
              Save
            </Button>
          )}
          <Button
            startIcon={<Cancel />}
            onClick={handleCancelClick}
            disabled={loading}
            size="small"
          >
            {mode === "view" ? "Close" : "Cancel"}
          </Button>
          <IconButton onClick={handleCancelClick} size="small">
            <Close />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent sx={{ p: 0 }}>
        {loading && mode === 'edit' ? (
          <Box sx={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            height: '100%',
            flexDirection: 'column',
            gap: 2
          }}>
            <CircularProgress />
            <Typography>Loading ION data...</Typography>
          </Box>
        ) : (
          <Box sx={{ p: 2, height: '100%', overflow: 'auto' }}>

            {/* ===== ROW 1: Core fields ===== */}
            <Box sx={{ display: 'flex', gap: 1.5, mb: 1.5, alignItems: 'flex-start', flexWrap: 'wrap' }}>
              {formData.IONGUID && (
                <TextField
                  label="ION Number"
                  value={formData.IONNumber || ""}
                  disabled
                  size="small"
                  sx={{
                    width: 300,
                    flexShrink: 0,
                    "& .MuiInputBase-input.Mui-disabled": {
                      WebkitTextFillColor: "text.primary",
                      fontFamily: "monospace",
                      fontWeight: 600,
                    },
                  }}
                />
              )}

              <Autocomplete
                options={activeFileGroups}
                getOptionLabel={(g) => `${g.GroupName} — ${g.ReferenceNo}`}
                value={fileGroups.find(g => g.GroupGUID === formData.GroupGUID) || null}
                onChange={(_, selected) => {
                  handleInputChange("GroupGUID", selected ? selected.GroupGUID : "");
                  if (selected) {
                    setToChips(prev => {
                      if (prev.includes(selected.GroupName)) return prev;
                      const updated = [...prev, selected.GroupName];
                      setFormData(f => ({ ...f, ToAddress: chipsToText(updated) }));
                      return updated;
                    });
                  }
                }}
                disabled={isReadOnly}
                size="small"
                sx={{ width: 380, flexShrink: 0 }}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="File Group"
                    required
                    size="small"
                    error={!!errors.GroupGUID}
                    helperText={errors.GroupGUID}
                  />
                )}
              />

              <TextField
                label="Subject"
                value={formData.Subject}
                onChange={(e) => handleInputChange("Subject", e.target.value)}
                required
                disabled={isReadOnly}
                size="small"
                error={!!errors.Subject}
                helperText={errors.Subject}
                sx={{ flex: 1, minWidth: 150 }}
              />

              {/* Date: display DD-MMM-YY, click opens native picker */}
              <Box sx={{ position: 'relative', width: 250, flexShrink: 0 }}>
                <TextField
                  label="ION Date"
                  value={formatDateDisplay(formData.IONDate)}
                  required
                  disabled={isReadOnly}
                  size="small"
                  error={!!errors.IONDate}
                  helperText={errors.IONDate}
                  fullWidth
                  InputLabelProps={{ shrink: true }}
                  onClick={() => !isReadOnly && dateInputRef.current?.showPicker?.()}
                  InputProps={{ readOnly: true, sx: { cursor: isReadOnly ? 'default' : 'pointer' } }}
                />
                <input
                  ref={dateInputRef}
                  type="date"
                  value={formData.IONDate}
                  onChange={(e) => handleInputChange("IONDate", e.target.value)}
                  style={{
                    position: 'absolute',
                    top: 0, left: 0,
                    width: '100%', height: '100%',
                    opacity: 0,
                    pointerEvents: 'none',
                  }}
                  tabIndex={-1}
                />
              </Box>
            </Box>

            {/* ===== ROW 2: Recipients + Routing ===== */}
            {/* Use a label row + inputs row approach so chip labels and MUI labels align */}
            <Box sx={{ display: 'flex', gap: 1.5, mb: 1.5, alignItems: 'flex-start', flexWrap: 'wrap' }}>
              <Box sx={{ flex: 1, minWidth: 200 }}>
                <ChipField
                  label="To"
                  chips={toChips}
                  onDelete={handleDeleteToChip}
                  onEdit={handleEditToChip}
                  inputValue={toInputValue}
                  onInputChange={setToInputValue}
                  onAdd={handleAddToChip}
                  options={activeFileGroups}
                  required
                  error={!!errors.ToAddress}
                  helperText={errors.ToAddress}
                  disabled={isReadOnly}
                />
              </Box>

              <Box sx={{ flex: 1, minWidth: 200 }}>
                <ChipField
                  label="Copy To"
                  chips={copyToChips}
                  onDelete={handleDeleteCopyToChip}
                  onEdit={handleEditCopyToChip}
                  inputValue={copyToInputValue}
                  onInputChange={setCopyToInputValue}
                  onAdd={handleAddCopyToChip}
                  options={activeFileGroups}
                  disabled={isReadOnly}
                />
              </Box>

              {/* Wrap Autocomplete fields so they align with ChipField label height */}
              <Box sx={{ width: 220, flexShrink: 0 }}>
                <Typography variant="caption" color={errors.PreparedBy ? "error" : "text.secondary"} sx={{ mb: 0.5, display: "block", fontWeight: 500 }}>
                  Prepared By *
                </Typography>
                <Autocomplete
                  options={users}
                  getOptionLabel={(u) => u.UserName || ''}
                  value={users.find(u => u.UserDbkey === formData.PreparedBy) || null}
                  onChange={(_, selected) => handleInputChange("PreparedBy", selected ? selected.UserDbkey : "")}
                  disabled={isReadOnly}
                  size="small"
                  renderOption={(props, u) => (
                    <li {...props} key={u.UserDbkey}>
                      <Box>
                        <Typography variant="body2" sx={{ fontWeight: 500 }}>
                          {u.UserName} {u.Designation ? `— ${u.Designation}` : ''}
                        </Typography>
                        {u.DepartmentName && (
                          <Typography variant="caption" color="text.secondary">
                            {u.DepartmentName}
                          </Typography>
                        )}
                      </Box>
                    </li>
                  )}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      size="small"
                      error={!!errors.PreparedBy}
                      helperText={errors.PreparedBy}
                    />
                  )}
                />
              </Box>

              <Box sx={{ width: 220, flexShrink: 0 }}>
                <Typography variant="caption" color="text.secondary" sx={{ mb: 0.5, display: "block", fontWeight: 500 }}>
                  Sent Through (Approval)
                </Typography>
                <Autocomplete
                  options={users}
                  getOptionLabel={(u) => u.UserName || ''}
                  value={users.find(u => u.UserDbkey === formData.SentThrough) || null}
                  onChange={(_, selected) => handleInputChange("SentThrough", selected ? selected.UserDbkey : "")}
                  disabled={isReadOnly}
                  size="small"
                  renderOption={(props, u) => (
                    <li {...props} key={u.UserDbkey}>
                      <Box>
                        <Typography variant="body2" sx={{ fontWeight: 500 }}>
                          {u.UserName} {u.Designation ? `— ${u.Designation}` : ''}
                        </Typography>
                        {u.DepartmentName && (
                          <Typography variant="caption" color="text.secondary">
                            {u.DepartmentName}
                          </Typography>
                        )}
                      </Box>
                    </li>
                  )}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      size="small"
                    />
                  )}
                />
              </Box>
            </Box>

            <Divider sx={{ mb: 1.5 }} />

            {/* ===== ROW 3: ION Body (left) + Reference & Enclosures (right) ===== */}
            <Grid container spacing={2}>
              <Grid size={{ xs: 12, md: 8 }}>
                <Typography variant="subtitle2" sx={{ mb: 0.5, fontWeight: 600 }}>
                  ION Body *
                </Typography>
                <JoditRichTextEditor
                  value={formData.IONBody}
                  onChange={(content) => handleInputChange("IONBody", content)}
                  disabled={isReadOnly}
                  placeholder="Start typing or paste content from MS Word, drag & drop images..."
                  preserveOfficeFormatting={true}
                  height="calc(95vh - 310px)"
                />
                {errors.IONBody && (
                  <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                    {errors.IONBody}
                  </Typography>
                )}
              </Grid>

              <Grid size={{ xs: 12, md: 4 }}>
                <Paper variant="outlined" sx={{ p: 1.5, height: '100%', display: 'flex', flexDirection: 'column' }}>

                  {/* Communication Reference — inline add with # mention search */}
                  <CommRefListField
                    ref={commRefPanelRef}
                    label="Communication Reference"
                    items={commRefItems}
                    onAdd={handleAddCommRef}
                    onDelete={handleDeleteCommRef}
                    onUpdate={handleUpdateCommRef}
                    onReorder={handleReorderCommRef}
                    placeholder="Type ref or # for MMG demands"
                    disabled={isReadOnly}
                    fetchDemands={fetchDemands}
                  />

                  <Divider sx={{ my: 1.5 }} />

                  {/* Enclosures with file upload */}
                  <Box sx={{ flex: 1, overflow: 'auto' }}>
                    <EnclosurePanel
                      ref={enclosurePanelRef}
                      enclosures={enclosures}
                      onAdd={handleAddEnclosureInline}
                      onDelete={handleDeleteEnclosure}
                      onUpload={handleEnclosureUpload}
                      onDownload={handleDownloadAttachment}
                      onDeleteAttachment={handleDeleteAttachment}
                      onPendingFileAdd={handlePendingFileAdd}
                      onPendingFileRemove={handlePendingFileRemove}
                      attachments={enclosureAttachments}
                      pendingFiles={pendingEnclosureFiles}
                      disabled={isReadOnly}
                    />
                  </Box>
                </Paper>
              </Grid>
            </Grid>

          </Box>
        )}
      </DialogContent>

      {/* Print Preview Dialog — renders current (unsaved) form state as PDF */}
      <Dialog
        open={previewOpen}
        onClose={handleClosePreview}
        maxWidth={false}
        PaperProps={{
          sx: {
            width: "85vw",
            height: "90vh",
            maxWidth: "85vw",
            maxHeight: "90vh",
          },
        }}
      >
        <DialogTitle
          sx={{
            borderBottom: 1,
            borderColor: "divider",
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            py: 1.5,
          }}
        >
          <Typography variant="h6">
            Print Preview{formData.IONNumber ? ` - ${formData.IONNumber}` : ""}
          </Typography>
          <IconButton onClick={handleClosePreview} size="small">
            <Close />
          </IconButton>
        </DialogTitle>

        <DialogContent sx={{ p: 0, overflow: "hidden", display: "flex", flexDirection: "column" }}>
          {isDirty && mode !== "view" ? (
            <Alert
              severity="warning"
              variant="filled"
              sx={{ borderRadius: 0, fontWeight: 500 }}
            >
              This is a preview only — your changes are NOT saved yet. Close this preview and click <strong>Save</strong> in the editor to persist them.
            </Alert>
          ) : (
            <Alert
              severity="info"
              sx={{ borderRadius: 0 }}
            >
              Preview only. Any edits made after this preview must be saved before closing the editor.
            </Alert>
          )}
          {previewBlobUrl && (
            <iframe
              src={previewBlobUrl}
              style={{
                width: "100%",
                flex: 1,
                border: "none",
              }}
              title="ION Print Preview"
            />
          )}
        </DialogContent>
      </Dialog>

      {/* Save as Template Dialog — admin captures current form as a reusable template */}
      <Dialog
        open={saveAsTplOpen}
        onClose={handleCloseSaveAsTemplate}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle sx={{ borderBottom: 1, borderColor: 'divider' }}>
          Save as Template
        </DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <Alert severity="info" variant="outlined">
              The current Subject, body, To/Copy To, Communication References, and
              enclosure descriptions will be saved as a new shared template. File
              attachments and document metadata are not included.
            </Alert>
            <TextField
              label="Template Name"
              value={saveAsTplName}
              onChange={(e) => setSaveAsTplName(e.target.value)}
              required
              autoFocus
              size="small"
              fullWidth
              helperText="Visible to all users in the Create from Template picker"
            />
            <TextField
              label="Description (optional)"
              value={saveAsTplDescription}
              onChange={(e) => setSaveAsTplDescription(e.target.value)}
              size="small"
              fullWidth
              multiline
              minRows={2}
              helperText="Short note about when to use this template"
            />
          </Box>
        </DialogContent>
        <DialogActions sx={{ borderTop: 1, borderColor: 'divider' }}>
          <Button onClick={handleCloseSaveAsTemplate} disabled={saveAsTplSaving} size="small">
            Cancel
          </Button>
          <Button
            onClick={handleConfirmSaveAsTemplate}
            disabled={!saveAsTplName.trim() || saveAsTplSaving}
            variant="contained"
            size="small"
            startIcon={saveAsTplSaving ? <CircularProgress size={16} color="inherit" /> : <BookmarkAdd />}
          >
            {saveAsTplSaving ? 'Saving...' : 'Save Template'}
          </Button>
        </DialogActions>
      </Dialog>
    </Dialog>
  );
};

export default IONFormView;
