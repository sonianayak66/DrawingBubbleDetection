import React, { useState, useEffect, useRef, useCallback } from 'react';
import {
  Box,
  Paper,
  Grid,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Typography,
  Chip,
  Autocomplete,
  Divider,
  CircularProgress,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
} from '@mui/material';
import {
  Add,
  Edit,
  ArrowBack,
  ToggleOn,
  ToggleOff,
  Delete,
  Close,
  Save as SaveIcon,
} from '@mui/icons-material';
import { ionApi } from '../../../services/ionApi';
import JoditRichTextEditor from '../../shared/components/Common/JoditRichTextEditor';

// Default chip that should always appear LAST in Copy To (matches IONFormView convention)
const OFFICE_COPY = 'Office Copy';

const emptyForm = {
  TemplateGUID: null,
  TemplateName: '',
  Description: '',
  GroupGUID: '',
  SubjectTemplate: '',
  IONBodyTemplate: '',
  ToAddressTemplate: '',
  CopyToTemplate: '',
  CommRefTemplate: '',
  EnclosuresTemplate: '[]', // JSON-encoded array of strings
  IsActive: true,
};

// Helpers — chips ↔ newline-separated text (matches IONFormView convention)
const textToChips = (text) => {
  if (!text) return [];
  return text.split('\n').map((s) => s.trim()).filter(Boolean);
};
const chipsToText = (arr) => (arr || []).join('\n');

// Keeps "Office Copy" pinned to the end of the Copy To list if present
const reorderWithOfficeCopyLast = (chips) => {
  const without = chips.filter((c) => c !== OFFICE_COPY);
  return chips.includes(OFFICE_COPY) ? [...without, OFFICE_COPY] : without;
};

// Lightweight chip-input field — mirrors the look of IONFormView's ChipField
// without the autocomplete suggestions (templates don't bind to file groups for recipients).
const TemplateChipField = ({ label, chips, onAdd, onDelete, onEdit, disabled, helperText }) => {
  const [inputValue, setInputValue] = useState('');
  const [editingIndex, setEditingIndex] = useState(null);
  const [editingValue, setEditingValue] = useState('');
  const inputRef = useRef(null);
  const editInputRef = useRef(null);

  const commitAdd = () => {
    const v = inputValue.trim();
    if (!v) return;
    onAdd(v);
    setInputValue('');
  };

  const startEdit = (i) => {
    setEditingIndex(i);
    setEditingValue(chips[i]);
    setTimeout(() => editInputRef.current?.focus(), 0);
  };

  const commitEdit = () => {
    if (editingIndex === null) return;
    const v = editingValue.trim();
    if (v && v !== chips[editingIndex]) onEdit(editingIndex, v);
    setEditingIndex(null);
    setEditingValue('');
  };

  return (
    <Box>
      <Typography
        variant="caption"
        color="text.secondary"
        sx={{ mb: 0.5, display: 'block', fontWeight: 500 }}
      >
        {label}
      </Typography>
      <Paper
        variant="outlined"
        sx={{
          p: 0.75,
          minHeight: 40,
          display: 'flex',
          flexWrap: 'wrap',
          gap: 0.5,
          alignItems: 'center',
          '&:focus-within': { borderColor: 'primary.main', borderWidth: 2 },
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
                if (e.key === 'Enter') {
                  e.preventDefault();
                  commitEdit();
                }
                if (e.key === 'Escape') {
                  setEditingIndex(null);
                  setEditingValue('');
                }
              }}
              onBlur={commitEdit}
              size="small"
              variant="standard"
              sx={{
                width: Math.max(80, editingValue.length * 8 + 20),
                '& .MuiInput-input': { fontSize: 13, py: 0.25 },
              }}
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
        )}
        {!disabled && (
          <TextField
            inputRef={inputRef}
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter' && inputValue.trim()) {
                e.preventDefault();
                commitAdd();
              }
            }}
            onBlur={commitAdd}
            placeholder={chips.length === 0 ? 'Type and press Enter...' : 'Add more...'}
            variant="standard"
            size="small"
            sx={{
              flex: 1,
              minWidth: 120,
              '& .MuiInput-underline:before': { borderBottom: 'none' },
              '& .MuiInput-underline:hover:before': { borderBottom: 'none' },
              '& .MuiInput-underline:after': { borderBottom: 'none' },
              '& .MuiInput-input': { fontSize: 13, py: 0.5 },
            }}
          />
        )}
      </Paper>
      {helperText && (
        <Typography variant="caption" color="text.secondary" sx={{ mt: 0.25, display: 'block' }}>
          {helperText}
        </Typography>
      )}
    </Box>
  );
};

// Simple list editor for items like Communication References / Enclosure descriptions.
// No # mention search, no drag reorder, no file uploads — templates are description-only.
const TemplateListEditor = ({ label, items, onAdd, onUpdate, onDelete, placeholder, helperText }) => {
  const [inputValue, setInputValue] = useState('');
  const [editIndex, setEditIndex] = useState(null);
  const [editValue, setEditValue] = useState('');

  const commitAdd = () => {
    const v = inputValue.trim();
    if (!v) return;
    if (editIndex !== null) {
      onUpdate(editIndex, v);
      setEditIndex(null);
    } else {
      onAdd(v);
    }
    setInputValue('');
  };

  const startEdit = (i) => {
    setEditIndex(i);
    setInputValue(items[i]);
    setEditValue(items[i]);
  };

  const cancelEdit = () => {
    setEditIndex(null);
    setInputValue('');
  };

  return (
    <Box>
      <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 0.5 }}>
        {label}
      </Typography>
      <Box sx={{ display: 'flex', gap: 0.5, mb: 0.5 }}>
        <TextField
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          placeholder={placeholder}
          size="small"
          fullWidth
          onKeyDown={(e) => {
            if (e.key === 'Enter') {
              e.preventDefault();
              commitAdd();
            }
          }}
        />
        <IconButton onClick={commitAdd} color="primary" size="small" disabled={!inputValue.trim()}>
          <Add />
        </IconButton>
        {editIndex !== null && (
          <IconButton onClick={cancelEdit} size="small" title="Cancel edit">
            <Close fontSize="small" />
          </IconButton>
        )}
      </Box>
      {helperText && (
        <Typography variant="caption" color="text.secondary" sx={{ mb: 1, display: 'block' }}>
          {editIndex !== null
            ? `Editing item #${editIndex + 1}. Press Enter or + to update.`
            : helperText}
        </Typography>
      )}
      {items.length > 0 ? (
        <List dense disablePadding>
          {items.map((item, i) => (
            <ListItem
              key={i}
              divider
              disableGutters
              sx={{
                py: 0.25,
                pr: 9,
                backgroundColor: editIndex === i ? 'action.selected' : undefined,
              }}
            >
              <ListItemText
                primary={
                  <Typography variant="body2" sx={{ fontSize: 13 }}>
                    {i + 1}. {item}
                  </Typography>
                }
              />
              <ListItemSecondaryAction>
                <IconButton
                  edge="end"
                  size="small"
                  color="primary"
                  onClick={() => startEdit(i)}
                  disabled={editIndex !== null && editIndex !== i}
                  title="Edit"
                  sx={{ mr: 0.5 }}
                >
                  <Edit fontSize="small" />
                </IconButton>
                <IconButton
                  edge="end"
                  size="small"
                  color="error"
                  onClick={() => onDelete(i)}
                  disabled={editIndex !== null}
                  title="Delete"
                >
                  <Delete fontSize="small" />
                </IconButton>
              </ListItemSecondaryAction>
            </ListItem>
          ))}
        </List>
      ) : (
        <Typography variant="body2" color="text.secondary" sx={{ fontStyle: 'italic', py: 1, textAlign: 'center' }}>
          None added
        </Typography>
      )}
    </Box>
  );
};

const IONTemplatesView = ({ onBack }) => {
  const [templates, setTemplates] = useState([]);
  const [fileGroups, setFileGroups] = useState([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editMode, setEditMode] = useState(false);
  const [formData, setFormData] = useState(emptyForm);
  const [saving, setSaving] = useState(false);

  // Chip / list state — mirrors IONFormView shape
  const [toChips, setToChips] = useState([]);
  const [copyToChips, setCopyToChips] = useState([]);
  const [commRefItems, setCommRefItems] = useState([]);
  const [enclosureItems, setEnclosureItems] = useState([]);

  useEffect(() => {
    loadAll();
  }, []);

  const loadAll = async () => {
    try {
      setLoading(true);
      const [tplRes, grpRes] = await Promise.all([
        ionApi.getIONTemplates(),
        ionApi.getFileGroups(),
      ]);
      setTemplates(tplRes.data || []);
      setFileGroups(grpRes.data || []);
    } catch (error) {
      console.error('Error loading templates:', error);
    } finally {
      setLoading(false);
    }
  };

  // ---- dialog open / reset ----
  const openBlank = () => {
    setFormData(emptyForm);
    setToChips([]);
    setCopyToChips([]);
    setCommRefItems([]);
    setEnclosureItems([]);
  };

  const handleAdd = () => {
    openBlank();
    setEditMode(false);
    setDialogOpen(true);
  };

  const handleEdit = (tpl) => {
    setFormData({
      TemplateGUID: tpl.TemplateGUID,
      TemplateName: tpl.TemplateName || '',
      Description: tpl.Description || '',
      GroupGUID: tpl.GroupGUID || '',
      SubjectTemplate: tpl.SubjectTemplate || '',
      IONBodyTemplate: tpl.IONBodyTemplate || '',
      ToAddressTemplate: tpl.ToAddressTemplate || '',
      CopyToTemplate: tpl.CopyToTemplate || '',
      CommRefTemplate: tpl.CommRefTemplate || '',
      EnclosuresTemplate: tpl.EnclosuresTemplate || '[]',
      IsActive: tpl.IsActive !== false,
    });
    setToChips(textToChips(tpl.ToAddressTemplate));
    setCopyToChips(textToChips(tpl.CopyToTemplate));
    setCommRefItems(textToChips(tpl.CommRefTemplate));
    let encArr = [];
    try {
      const parsed = JSON.parse(tpl.EnclosuresTemplate || '[]');
      if (Array.isArray(parsed)) encArr = parsed.filter(Boolean).map(String);
    } catch {
      // ignore malformed enclosures JSON
    }
    setEnclosureItems(encArr);
    setEditMode(true);
    setDialogOpen(true);
  };

  // ---- save / delete / toggle ----
  const handleSave = async () => {
    try {
      setSaving(true);
      const payload = {
        ...formData,
        ToAddressTemplate: chipsToText(toChips),
        CopyToTemplate: chipsToText(copyToChips),
        CommRefTemplate: chipsToText(commRefItems),
        EnclosuresTemplate: JSON.stringify(enclosureItems),
      };
      await ionApi.saveIONTemplate(payload);
      setDialogOpen(false);
      loadAll();
    } catch (error) {
      console.error('Error saving template:', error);
      alert('Failed to save template');
    } finally {
      setSaving(false);
    }
  };

  const handleToggleActive = async (tpl) => {
    const action = tpl.IsActive ? 'deactivate' : 'activate';
    if (!window.confirm(`${action.charAt(0).toUpperCase()}${action.slice(1)} template "${tpl.TemplateName}"?`)) return;
    try {
      // Re-save with all fields preserved + flipped IsActive
      await ionApi.saveIONTemplate({
        TemplateGUID: tpl.TemplateGUID,
        TemplateName: tpl.TemplateName,
        Description: tpl.Description,
        GroupGUID: tpl.GroupGUID,
        SubjectTemplate: tpl.SubjectTemplate,
        IONBodyTemplate: tpl.IONBodyTemplate,
        ToAddressTemplate: tpl.ToAddressTemplate,
        CopyToTemplate: tpl.CopyToTemplate,
        CommRefTemplate: tpl.CommRefTemplate,
        EnclosuresTemplate: tpl.EnclosuresTemplate,
        IsActive: !tpl.IsActive,
      });
      loadAll();
    } catch (error) {
      console.error('Error toggling template:', error);
      alert('Failed to update template');
    }
  };

  const handleDelete = async (tpl) => {
    if (!window.confirm(`Delete template "${tpl.TemplateName}"? This cannot be undone.`)) return;
    try {
      await ionApi.deleteIONTemplate(tpl.TemplateGUID);
      loadAll();
    } catch (error) {
      console.error('Error deleting template:', error);
      alert('Failed to delete template');
    }
  };

  const handleInputChange = (field, value) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  // ---- chip handlers ----
  const handleAddTo = useCallback((v) => {
    setToChips((prev) => (prev.includes(v) ? prev : [...prev, v]));
  }, []);
  const handleDeleteTo = useCallback((i) => {
    setToChips((prev) => prev.filter((_, idx) => idx !== i));
  }, []);
  const handleEditTo = useCallback((i, v) => {
    setToChips((prev) => prev.map((c, idx) => (idx === i ? v : c)));
  }, []);

  const handleAddCopyTo = useCallback((v) => {
    setCopyToChips((prev) => {
      if (prev.includes(v)) return prev;
      return reorderWithOfficeCopyLast([...prev, v]);
    });
  }, []);
  const handleDeleteCopyTo = useCallback((i) => {
    setCopyToChips((prev) => prev.filter((_, idx) => idx !== i));
  }, []);
  const handleEditCopyTo = useCallback((i, v) => {
    setCopyToChips((prev) => reorderWithOfficeCopyLast(prev.map((c, idx) => (idx === i ? v : c))));
  }, []);

  // ---- comm ref / enclosure handlers ----
  const handleAddCommRef = useCallback((v) => {
    setCommRefItems((prev) => [...prev, v]);
  }, []);
  const handleUpdateCommRef = useCallback((i, v) => {
    setCommRefItems((prev) => prev.map((it, idx) => (idx === i ? v : it)));
  }, []);
  const handleDeleteCommRef = useCallback((i) => {
    setCommRefItems((prev) => prev.filter((_, idx) => idx !== i));
  }, []);

  const handleAddEnclosure = useCallback((v) => {
    setEnclosureItems((prev) => [...prev, v]);
  }, []);
  const handleUpdateEnclosure = useCallback((i, v) => {
    setEnclosureItems((prev) => prev.map((it, idx) => (idx === i ? v : it)));
  }, []);
  const handleDeleteEnclosure = useCallback((i) => {
    setEnclosureItems((prev) => prev.filter((_, idx) => idx !== i));
  }, []);

  const isFormValid = formData.TemplateName.trim().length > 0;
  const activeFileGroups = fileGroups.filter((g) => g.IsActive);

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          {/* <IconButton onClick={onBack} size="small">
            <ArrowBack />
          </IconButton> */}
          <Typography variant="h5" sx={{ fontWeight: 600 }}>
            ION Templates
          </Typography>
           
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <Button startIcon={<Add />} variant="contained" onClick={handleAdd} size="small">
          Add Template
        </Button>
        <Button startIcon={<ArrowBack />} variant="outlined" onClick={onBack} size="small">
                      Back to ION List
                    </Button>
        </Box>
      </Box>

      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Templates are shared across all users. Anyone creating a new ION can pick a template
        to pre-fill the form, then edit only what's different.
      </Typography>

      {/* Table */}
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Template Name</TableCell>
              <TableCell>Description</TableCell>
              <TableCell>Subject</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={5} align="center"><CircularProgress size={24} /></TableCell>
              </TableRow>
            ) : templates.length === 0 ? (
              <TableRow>
                <TableCell colSpan={5} align="center">
                  No templates yet. Click "Add Template" to create one.
                </TableCell>
              </TableRow>
            ) : (
              templates.map((tpl) => (
                <TableRow key={tpl.TemplateId} hover>
                  <TableCell sx={{ fontWeight: 600 }}>{tpl.TemplateName}</TableCell>
                  <TableCell>
                    <Typography variant="body2" color="text.secondary" sx={{ maxWidth: 300 }} noWrap>
                      {tpl.Description || '—'}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" sx={{ maxWidth: 300 }} noWrap>
                      {tpl.SubjectTemplate || '—'}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={tpl.IsActive ? 'Active' : 'Inactive'}
                      color={tpl.IsActive ? 'success' : 'default'}
                      size="small"
                    />
                  </TableCell>
                  <TableCell align="right">
                    <IconButton size="small" onClick={() => handleEdit(tpl)} color="primary" title="Edit">
                      <Edit fontSize="small" />
                    </IconButton>
                    <IconButton
                      size="small"
                      onClick={() => handleToggleActive(tpl)}
                      color={tpl.IsActive ? 'warning' : 'success'}
                      title={tpl.IsActive ? 'Deactivate' : 'Activate'}
                    >
                      {tpl.IsActive ? <ToggleOff fontSize="small" /> : <ToggleOn fontSize="small" />}
                    </IconButton>
                    <IconButton size="small" onClick={() => handleDelete(tpl)} color="error" title="Delete">
                      <Delete fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Add / Edit Dialog — laid out to mirror the actual ION form */}
      <Dialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        maxWidth={false}
        PaperProps={{ sx: { width: '90vw', height: '95vh', maxWidth: '90vw', maxHeight: '95vh' } }}
      >
        <DialogTitle
          sx={{
            borderBottom: 1,
            borderColor: 'divider',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            py: 1.5,
          }}
        >
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            {editMode ? 'Edit Template' : 'Add Template'}
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              startIcon={<SaveIcon />}
              onClick={handleSave}
              variant="contained"
              size="small"
              disabled={!isFormValid || saving}
            >
              {saving ? 'Saving...' : 'Save Template'}
            </Button>
            <Button onClick={() => setDialogOpen(false)} size="small" disabled={saving}>
              Cancel
            </Button>
            <IconButton onClick={() => setDialogOpen(false)} size="small" disabled={saving}>
              <Close />
            </IconButton>
          </Box>
        </DialogTitle>

        <DialogContent sx={{ px: 2, pt: 3, pb: 2, overflow: 'auto' }}>
          {/* ===== Template metadata: Name + Description (only template-specific fields) ===== */}
          <Box sx={{ display: 'flex', gap: 1.5, mb: 1.5,mt: 2, alignItems: 'flex-start', flexWrap: 'wrap' }}>
            <TextField
              label="Template Name"
              value={formData.TemplateName}
              onChange={(e) => handleInputChange('TemplateName', e.target.value)}
              required
              size="small"
              sx={{ width: 320, flexShrink: 0 }}
              helperText="e.g., Monthly Status Report"
            />
            <TextField
              label="Description"
              value={formData.Description}
              onChange={(e) => handleInputChange('Description', e.target.value)}
              size="small"
              sx={{ flex: 1, minWidth: 280 }}
              helperText="Short description shown in the picker — what is this template for?"
            />
          </Box>

          <Divider sx={{ mb: 2 }}>
            <Chip label="ION Form Defaults" size="small" />
          </Divider>

          {/* ===== ROW 1: File Group + Subject ===== */}
          <Box sx={{ display: 'flex', gap: 1.5, mb: 1.5, alignItems: 'flex-start', flexWrap: 'wrap' }}>
            <Autocomplete
              options={activeFileGroups}
              getOptionLabel={(g) => `${g.GroupName} — ${g.ReferenceNo}`}
              value={fileGroups.find((g) => g.GroupGUID === formData.GroupGUID) || null}
              onChange={(_, sel) => handleInputChange('GroupGUID', sel ? sel.GroupGUID : '')}
              size="small"
              sx={{ width: 380, flexShrink: 0 }}
              renderInput={(params) => (
                <TextField {...params} label="Default File Group (optional)" size="small" />
              )}
            />
            <TextField
              label="Subject"
              value={formData.SubjectTemplate}
              onChange={(e) => handleInputChange('SubjectTemplate', e.target.value)}
              size="small"
              placeholder="Default subject line (user can edit when applying)"
              sx={{ flex: 1, minWidth: 200 }}
            />
          </Box>

          {/* ===== ROW 2: To / Copy To chip fields ===== */}
          <Box sx={{ display: 'flex', gap: 1.5, mb: 1.5, flexWrap: 'wrap' }}>
            <Box sx={{ flex: 1, minWidth: 280 }}>
              <TemplateChipField
                label="To Address"
                chips={toChips}
                onAdd={handleAddTo}
                onDelete={handleDeleteTo}
                onEdit={handleEditTo}
                helperText="Default recipients — user can add/remove on apply"
              />
            </Box>
            <Box sx={{ flex: 1, minWidth: 280 }}>
              <TemplateChipField
                label="Copy To"
                chips={copyToChips}
                onAdd={handleAddCopyTo}
                onDelete={handleDeleteCopyTo}
                onEdit={handleEditCopyTo}
                helperText='"Office Copy" is added by default when template is used'
              />
            </Box>  
          </Box>

          <Divider sx={{ mb: 1.5 }} />

          {/* ===== ROW 3: ION Body (left) + Comm Refs & Enclosures (right) ===== */}
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, md: 8 }}>
              <Typography variant="subtitle2" sx={{ mb: 0.5, fontWeight: 600 }}>
                ION Body
              </Typography>
              <JoditRichTextEditor
                value={formData.IONBodyTemplate}
                onChange={(content) => handleInputChange('IONBodyTemplate', content)}
                placeholder="Default body content for this template..."
                preserveOfficeFormatting={true}
                height="calc(95vh - 420px)"
              />
            </Grid>

            <Grid size={{ xs: 12, md: 4 }}>
              <Paper variant="outlined" sx={{ p: 1.5, height: '100%', display: 'flex', flexDirection: 'column' }}>
                <TemplateListEditor
                  label="Communication Reference"
                  items={commRefItems}
                  onAdd={handleAddCommRef}
                  onUpdate={handleUpdateCommRef}
                  onDelete={handleDeleteCommRef}
                  placeholder="Type reference and press Enter..."
                  helperText="Press Enter or + to add."
                />

                <Divider sx={{ my: 1.5 }} />

                <Box sx={{ flex: 1, overflow: 'auto' }}>
                  <TemplateListEditor
                    label="Enclosures"
                    items={enclosureItems}
                    onAdd={handleAddEnclosure}
                    onUpdate={handleUpdateEnclosure}
                    onDelete={handleDeleteEnclosure}
                    placeholder="Enclosure description..."
                    helperText="Descriptions only — file uploads are added when creating an ION."
                  />
                </Box>
              </Paper>
            </Grid>
          </Grid>
        </DialogContent>
      </Dialog>
    </Box>
  );
};

export default IONTemplatesView;
