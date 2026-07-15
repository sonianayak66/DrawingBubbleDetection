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
  Switch,
  FormControlLabel,
  Typography,
  Chip,
} from '@mui/material';
import { Add, Edit, Delete, ArrowBack } from '@mui/icons-material';
import { ionApi } from '../../../services/ionApi';

const OfficeConfigView = ({ onBack }) => {
  const [offices, setOffices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editMode, setEditMode] = useState(false);
  const [formData, setFormData] = useState({
    ConfigId: null,
    Office: '',
    RefNoPrefix: '',
    IsActive: true,
  });

  useEffect(() => {
    loadOffices();
  }, []);

  const loadOffices = async () => {
    try {
      setLoading(true);
      const response = await ionApi.getOfficeConfig();
      setOffices(response.data || []);
    } catch (error) {
      console.error('Error loading offices:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleAdd = () => {
    setFormData({
      ConfigId: null,
      Office: '',
      RefNoPrefix: '',
      IsActive: true,
    });
    setEditMode(false);
    setDialogOpen(true);
  };

  const handleEdit = (office) => {
    setFormData({
      ConfigId: office.ConfigId,
      Office: office.Office,
      RefNoPrefix: office.RefNoPrefix,
      IsActive: office.IsActive,
    });
    setEditMode(true);
    setDialogOpen(true);
  };

  const handleDelete = async (configId) => {
    if (window.confirm('Are you sure you want to delete this office configuration?')) {
      try {
        await ionApi.deleteOfficeConfig(configId);
        loadOffices();
      } catch (error) {
        console.error('Error deleting office:', error);
        alert('Failed to delete office configuration');
      }
    }
  };

  const handleSave = async () => {
    try {
      await ionApi.saveOfficeConfig(formData);
      setDialogOpen(false);
      loadOffices();
    } catch (error) {
      console.error('Error saving office:', error);
      alert('Failed to save office configuration');
    }
  };

  const handleInputChange = (field, value) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <IconButton onClick={onBack} size="small">
            <ArrowBack />
          </IconButton>
          <Typography variant="h5" sx={{ fontWeight: 600 }}>
            Office Configuration
          </Typography>
        </Box>
        <Button
          startIcon={<Add />}
          variant="contained"
          onClick={handleAdd}
          size="small"
        >
          Add Office
        </Button>
      </Box>

      {/* Table */}
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Office Name</TableCell>
              <TableCell>Reference Prefix</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={4} align="center">Loading...</TableCell>
              </TableRow>
            ) : offices.length === 0 ? (
              <TableRow>
                <TableCell colSpan={4} align="center">
                  No office configurations found. Click "Add Office" to create one.
                </TableCell>
              </TableRow>
            ) : (
              offices.map((office) => (
                <TableRow key={office.ConfigId} hover>
                  <TableCell>{office.Office}</TableCell>
                  <TableCell>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 600 }}>
                      {office.RefNoPrefix}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={office.IsActive ? 'Active' : 'Inactive'}
                      color={office.IsActive ? 'success' : 'default'}
                      size="small"
                    />
                  </TableCell>
                  <TableCell align="right">
                    <IconButton
                      size="small"
                      onClick={() => handleEdit(office)}
                      color="primary"
                    >
                      <Edit fontSize="small" />
                    </IconButton>
                    <IconButton
                      size="small"
                      onClick={() => handleDelete(office.ConfigId)}
                      color="error"
                    >
                      <Delete fontSize="small" />
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
        <DialogTitle>
          {editMode ? 'Edit Office Configuration' : 'Add Office Configuration'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              label="Office Name"
              value={formData.Office}
              onChange={(e) => handleInputChange('Office', e.target.value)}
              fullWidth
              required
              size="small"
              helperText="e.g., Branch Office - Delhi"
            />
            <TextField
              label="Reference Number Prefix"
              value={formData.RefNoPrefix}
              onChange={(e) => handleInputChange('RefNoPrefix', e.target.value.toUpperCase())}
              fullWidth
              required
              size="small"
              inputProps={{ maxLength: 10 }}
              helperText="e.g., BOD (will be used in ION numbers like BOD/FIN/001)"
            />
            <FormControlLabel
              control={
                <Switch
                  checked={formData.IsActive}
                  onChange={(e) => handleInputChange('IsActive', e.target.checked)}
                />
              }
              label="Active"
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)} size="small">
            Cancel
          </Button>
          <Button
            onClick={handleSave}
            variant="contained"
            disabled={!formData.Office || !formData.RefNoPrefix}
            size="small"
          >
            Save
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default OfficeConfigView;