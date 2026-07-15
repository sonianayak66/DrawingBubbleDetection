import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Box
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';

const ProjectDialog = ({ open, onClose, project, onSave }) => {
  const [formData, setFormData] = useState({
    ProjectGUID: null,
    ProjectName: '',
    ProjectDescription: '',
    ProjectStatus: 'Active',
    Priority: 'Medium',
    StartDate: null,
    EndDate: null
  });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (project) {
      // Edit mode
       setFormData({
      ProjectGUID: project.ProjectGUID, // Changed from projectGUID
      ProjectName: project.ProjectName || '', // Changed from projectName
      ProjectDescription: project.ProjectDescription || '', // Changed from projectDescription
      ProjectStatus: project.ProjectStatus || 'Active', // Changed from projectStatus
      Priority: project.Priority || 'Medium', // Changed from priority
      StartDate: project.StartDate ? new Date(project.StartDate) : null, // Changed from startDate
      EndDate: project.EndDate ? new Date(project.EndDate) : null // Changed from endDate
    });
    } else {
      // Create mode
      setFormData({
        ProjectGUID: null,
        ProjectName: '',
        ProjectDescription: '',
        ProjectStatus: 'Active',
        Priority: 'Medium',
        StartDate: null,
        EndDate: null
      });
    }
  }, [project, open]);

  const handleChange = (field) => (event) => {
    setFormData(prev => ({
      ...prev,
      [field]: event.target.value
    }));
  };

  const handleDateChange = (field) => (date) => {
    setFormData(prev => ({
      ...prev,
      [field]: date
    }));
  };

  const handleSubmit = async () => {
    if (!formData.ProjectName.trim()) {
      alert('Project name is required');
      return;
    }

    setLoading(true);
    try {
      await onSave(formData);
      onClose();
    } catch (error) {
      console.error('Error saving project:', error);
      alert('Error saving project: ' + error.message);
    } finally {
      setLoading(false);
    }
  };

  const isEditMode = !!project;

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns}>
      <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
        <DialogTitle>
          {isEditMode ? 'Edit Project' : 'Create New Project'}
        </DialogTitle>
        
        <DialogContent>
          <Box sx={{ pt: 1 }}>
            <Grid container spacing={3}>
              <Grid item size={{ xs: 12 }}>
                <TextField
                  fullWidth
                  label="Project Name"
                  value={formData.ProjectName}
                  onChange={handleChange('ProjectName')}
                  required
                />
              </Grid>
              
              <Grid item size={{ xs: 12}}>
                <TextField
                  fullWidth
                  label="Description"
                  value={formData.ProjectDescription}
                  onChange={handleChange('ProjectDescription')}
                  multiline
                  rows={3}
                />
              </Grid>
              
              <Grid item size={{ xs: 12, sm: 6 }}>
                <FormControl fullWidth>
                  <InputLabel>Status</InputLabel>
                  <Select
                    value={formData.ProjectStatus}
                    onChange={handleChange('ProjectStatus')}
                    label="Status"
                  >
                    <MenuItem value="Active">Active</MenuItem>
                    <MenuItem value="Completed">Completed</MenuItem>
                    <MenuItem value="On Hold">On Hold</MenuItem>
                    <MenuItem value="Cancelled">Cancelled</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              
              <Grid item size={{ xs: 12, sm: 6 }}>
                <FormControl fullWidth>
                  <InputLabel>Priority</InputLabel>
                  <Select
                    value={formData.Priority}
                    onChange={handleChange('Priority')}
                    label="Priority"
                  >
                    <MenuItem value="Low">Low</MenuItem>
                    <MenuItem value="Medium">Medium</MenuItem>
                    <MenuItem value="High">High</MenuItem>
                    <MenuItem value="Critical">Critical</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              
              <Grid item size={{ xs: 12, sm: 6 }}>
                <DatePicker
                  label="Start Date"
                  value={formData.StartDate}
                  onChange={handleDateChange('StartDate')}
                  renderInput={(params) => <TextField {...params} fullWidth />}
                />
              </Grid>
              
              <Grid item size={{ xs: 12, sm: 6 }}>
                <DatePicker
                  label="End Date"
                  value={formData.EndDate}
                  onChange={handleDateChange('EndDate')}
                  renderInput={(params) => <TextField {...params} fullWidth />}
                />
              </Grid>
            </Grid>
          </Box>
        </DialogContent>
        
        <DialogActions>
          <Button onClick={onClose}>Cancel</Button>
          <Button 
            onClick={handleSubmit} 
            variant="contained" 
            disabled={loading}
          >
            {loading ? 'Saving...' : (isEditMode ? 'Update' : 'Create')}
          </Button>
        </DialogActions>
      </Dialog>
    </LocalizationProvider>
  );
};

export default ProjectDialog;