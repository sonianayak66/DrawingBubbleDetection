import React, { useState, useEffect } from 'react';
import {
  Box,
  Paper,
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
} from '@mui/material';
import { Add, Edit, ArrowBack, ToggleOn, ToggleOff } from '@mui/icons-material';
import { ionApi } from '../../../services/ionApi';

const emptyForm = {
  GroupGUID: null,
  GroupName: '',
  FileNo: '',
  ReferenceNo: '',
};

const FileGroupsView = ({ onBack }) => {
  const [groups, setGroups] = useState([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editMode, setEditMode] = useState(false);
  const [formData, setFormData] = useState(emptyForm);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    loadGroups();
  }, []);

  const loadGroups = async () => {
    try {
      setLoading(true);
      const response = await ionApi.getFileGroups();
      setGroups(response.data || []);
    } catch (error) {
      console.error('Error loading file groups:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleAdd = () => {
    setFormData(emptyForm);
    setEditMode(false);
    setDialogOpen(true);
  };

  const handleEdit = (group) => {
    setFormData({
      GroupGUID: group.GroupGUID,
      GroupName: group.GroupName,
      FileNo: group.FileNo,
      ReferenceNo: group.ReferenceNo,
    });
    setEditMode(true);
    setDialogOpen(true);
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      await ionApi.saveFileGroup(formData);
      setDialogOpen(false);
      loadGroups();
    } catch (error) {
      console.error('Error saving file group:', error);
      alert('Failed to save file group');
    } finally {
      setSaving(false);
    }
  };

  const handleToggleActive = async (group) => {
    const action = group.IsActive ? 'deactivate' : 'activate';
    if (!window.confirm(`Are you sure you want to ${action} "${group.GroupName}"?`)) return;
    try {
      await ionApi.toggleFileGroupActive({ GroupGUID: group.GroupGUID });
      loadGroups();
    } catch (error) {
      console.error('Error toggling file group:', error);
      alert('Failed to update file group status');
    }
  };

  const handleInputChange = (field, value) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const isFormValid = formData.GroupName.trim() && formData.FileNo !== '' && formData.ReferenceNo.trim();

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h5" sx={{ fontWeight: 600 }}>
          File numbering system for STFE
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Button startIcon={<Add />} variant="contained" onClick={handleAdd} size="small">
            Add
          </Button>
          <Button startIcon={<ArrowBack />} variant="outlined" onClick={onBack} size="small">
            Back to ION List
          </Button>
        </Box>
      </Box>

      {/* Table */}
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Group Name</TableCell>
              <TableCell>File No.</TableCell>
              <TableCell>Reference No</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={5} align="center">Loading...</TableCell>
              </TableRow>
            ) : groups.length === 0 ? (
              <TableRow>
                <TableCell colSpan={5} align="center">
                  No file groups found. Click "Add File Group" to create one.
                </TableCell>
              </TableRow>
            ) : (
              groups.map((group) => (
                <TableRow key={group.GroupId} hover>
                  <TableCell>{group.GroupName}</TableCell>
                  <TableCell>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 600 }}>
                      {group.FileNo}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                      {group.ReferenceNo}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={group.IsActive ? 'Active' : 'Inactive'}
                      color={group.IsActive ? 'success' : 'default'}
                      size="small"
                    />
                  </TableCell>
                  <TableCell align="right">
                    <IconButton size="small" onClick={() => handleEdit(group)} color="primary">
                      <Edit fontSize="small" />
                    </IconButton>
                    <IconButton
                      size="small"
                      onClick={() => handleToggleActive(group)}
                      color={group.IsActive ? 'warning' : 'success'}
                      title={group.IsActive ? 'Deactivate' : 'Activate'}
                    >
                      {group.IsActive
                        ? <ToggleOff fontSize="small" />
                        : <ToggleOn fontSize="small" />}
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Add/Edit Dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editMode ? 'Edit File Group' : 'Add File Group'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              label="Group Name"
              value={formData.GroupName}
              onChange={(e) => handleInputChange('GroupName', e.target.value)}
              fullWidth
              required
              size="small"
              helperText="e.g., Group1"
            />
            <TextField
              label="File No."
              value={formData.FileNo}
              onChange={(e) => {
                const val = e.target.value;
                if (val === '' || /^\d+$/.test(val)) handleInputChange('FileNo', val === '' ? '' : parseInt(val));
              }}
              fullWidth
              required
              size="small"
              inputProps={{ inputMode: 'numeric' }}
              helperText="e.g., 1"
            />
            <TextField
              label="Reference No"
              value={formData.ReferenceNo}
              onChange={(e) => handleInputChange('ReferenceNo', e.target.value)}
              fullWidth
              required
              size="small"
              helperText="e.g., REF/MOD/TECH/0010"
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)} size="small">Cancel</Button>
          <Button
            onClick={handleSave}
            variant="contained"
            disabled={!isFormValid || saving}
            size="small"
          >
            {saving ? 'Saving...' : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default FileGroupsView;