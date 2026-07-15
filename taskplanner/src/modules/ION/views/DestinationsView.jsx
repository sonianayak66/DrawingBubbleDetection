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

const DestinationsView = ({ onBack }) => {
  const [destinations, setDestinations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editMode, setEditMode] = useState(false);
  const [formData, setFormData] = useState({
    DestinationId: null,
    DestinationGUID: null,
    DestinationName: '',
    DestinationCode: '',
    IsActive: true,
  });

  useEffect(() => {
    loadDestinations();
  }, []);

  const loadDestinations = async () => {
    try {
      setLoading(true);
      const response = await ionApi.getDestinations();
      setDestinations(response.data || []);
    } catch (error) {
      console.error('Error loading destinations:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleAdd = () => {
    setFormData({
      DestinationId: null,
      DestinationGUID: null,
      DestinationName: '',
      DestinationCode: '',
      IsActive: true,
    });
    setEditMode(false);
    setDialogOpen(true);
  };

  const handleEdit = (destination) => {
    setFormData({
      DestinationId: destination.DestinationId,
      DestinationGUID: destination.DestinationGUID,
      DestinationName: destination.DestinationName,
      DestinationCode: destination.DestinationCode,
      IsActive: destination.IsActive,
    });
    setEditMode(true);
    setDialogOpen(true);
  };

  const handleDelete = async (destinationId) => {
    if (window.confirm('Are you sure you want to delete this destination?')) {
      try {
        await ionApi.deleteDestination(destinationId);
        loadDestinations();
      } catch (error) {
        console.error('Error deleting destination:', error);
        alert('Failed to delete destination');
      }
    }
  };

  const handleSave = async () => {
    try {
      await ionApi.saveDestination(formData);
      setDialogOpen(false);
      loadDestinations();
    } catch (error) {
      console.error('Error saving destination:', error);
      alert('Failed to save destination');
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
            Destinations Management
          </Typography>
        </Box>
        <Button
          startIcon={<Add />}
          variant="contained"
          onClick={handleAdd}
          size="small"
        >
          Add Destination
        </Button>
      </Box>

      {/* Table */}
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Destination Name</TableCell>
              <TableCell>Code</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={4} align="center">Loading...</TableCell>
              </TableRow>
            ) : destinations.length === 0 ? (
              <TableRow>
                <TableCell colSpan={4} align="center">
                  No destinations found. Click "Add Destination" to create one.
                </TableCell>
              </TableRow>
            ) : (
              destinations.map((destination) => (
                <TableRow key={destination.DestinationId} hover>
                  <TableCell>{destination.DestinationName}</TableCell>
                  <TableCell>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 600 }}>
                      {destination.DestinationCode}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={destination.IsActive ? 'Active' : 'Inactive'}
                      color={destination.IsActive ? 'success' : 'default'}
                      size="small"
                    />
                  </TableCell>
                  <TableCell align="right">
                    <IconButton
                      size="small"
                      onClick={() => handleEdit(destination)}
                      color="primary"
                    >
                      <Edit fontSize="small" />
                    </IconButton>
                    <IconButton
                      size="small"
                      onClick={() => handleDelete(destination.DestinationId)}
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
          {editMode ? 'Edit Destination' : 'Add Destination'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              label="Destination Name"
              value={formData.DestinationName}
              onChange={(e) => handleInputChange('DestinationName', e.target.value)}
              fullWidth
              required
              size="small"
              helperText="e.g., Finance Department"
            />
            <TextField
              label="Destination Code"
              value={formData.DestinationCode}
              onChange={(e) => handleInputChange('DestinationCode', e.target.value.toUpperCase())}
              fullWidth
              required
              size="small"
              inputProps={{ maxLength: 10 }}
              helperText="e.g., FIN (will be used in ION numbers like BOD/FIN/001)"
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
            disabled={!formData.DestinationName || !formData.DestinationCode}
            size="small"
          >
            Save
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default DestinationsView;