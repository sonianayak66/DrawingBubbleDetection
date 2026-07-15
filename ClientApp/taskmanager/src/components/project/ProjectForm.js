import React, { useState, useEffect } from 'react';
import {
    Box,
    TextField,
    Button,
    Paper,
    Typography,
    Grid,
    Alert,
    CircularProgress,
    Container,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    Snackbar
} from '@mui/material';
import { Save as SaveIcon, Cancel as CancelIcon } from '@mui/icons-material';
import { projectService } from '../../services/projectService';
import { useAuth } from '../../contexts/AuthContext';

function ProjectForm({ project = null, onSuccess, onCancel }) {
    const { hasPermission } = useAuth();
    const isEditMode = !!project;

    const [formData, setFormData] = useState({
        projectName: '',
        projectDescription: '',
        projectCode: '',
        startDate: '',
        dueDate: ''
    });
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [confirmDialog, setConfirmDialog] = useState(false);
    const [successMessage, setSuccessMessage] = useState('');

    useEffect(() => {
        if (project) {
            setFormData({
                projectName: project.ProjectName || project.projectName || '',
                projectDescription: project.ProjectDescription || project.projectDescription || '',
                projectCode: project.ProjectCode || project.projectCode || '',
                startDate: project.StartDate ? project.StartDate.split('T')[0] : project.startDate ? project.startDate.split('T')[0] : '',
                dueDate: project.DueDate ? project.DueDate.split('T')[0] : project.dueDate ? project.dueDate.split('T')[0] : ''
            });
        }
    }, [project]);

    if (!hasPermission('isAdmin')) {
        return (
            <Container maxWidth="sm">
                <Alert severity="error" sx={{ mt: 2 }}>
                    You don't have permission to {isEditMode ? 'edit' : 'create'} projects.
                </Alert>
            </Container>
        );
    }

    const handleChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        setConfirmDialog(true);
    };

    const handleConfirmSave = async () => {
        setConfirmDialog(false);
        setLoading(true);
        setError(null);

        try {
            let response;
            if (isEditMode) {
                // Use the correct property name for projectGuid
                const projectGuid = project.ProjectGuid || project.projectGuid;
                console.log('Updating project with GUID:', projectGuid);
                response = await projectService.updateProject(projectGuid, formData);
            } else {
                response = await projectService.createProject(formData);
            }

            console.log('Save response:', response);

            // Handle both success and Success
            if (response.success || response.Success) {
                setSuccessMessage(`Project ${isEditMode ? 'updated' : 'created'} successfully!`);
                setTimeout(() => {
                    if (onSuccess) {
                        onSuccess();
                    }
                }, 1500);
            } else {
                // If success is false, show the error message
                setError(response.message || response.Message || `Failed to ${isEditMode ? 'update' : 'create'} project`);
            }
        } catch (err) {
            console.error('Error saving project:', err);
            setError(err.response?.data?.message || err.message || `Failed to ${isEditMode ? 'update' : 'create'} project`);
        } finally {
            setLoading(false);
        }
    };

    return (
        <Container maxWidth="md">
            <Paper elevation={3} sx={{ p: 4, mt: 2 }}>
                <Typography variant="h5" component="h2" gutterBottom>
                    {isEditMode ? 'Edit Project' : 'Create New Project'}
                </Typography>

                <Box component="form" onSubmit={handleSubmit} sx={{ mt: 3 }}>
                    <Grid container spacing={3}>
                        <Grid item xs={12} sm={6}>
                            <TextField
                                fullWidth
                                required
                                label="Project Name"
                                name="projectName"
                                value={formData.projectName}
                                onChange={handleChange}
                                variant="outlined"
                            />
                        </Grid>

                        <Grid item xs={12} sm={6}>
                            <TextField
                                fullWidth
                                label="Project Code"
                                name="projectCode"
                                value={formData.projectCode}
                                onChange={handleChange}
                                variant="outlined"
                            />
                        </Grid>

                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                multiline
                                rows={4}
                                label="Project Description"
                                name="projectDescription"
                                value={formData.projectDescription}
                                onChange={handleChange}
                                variant="outlined"
                            />
                        </Grid>

                        <Grid item xs={12} sm={6}>
                            <TextField
                                fullWidth
                                type="date"
                                label="Start Date"
                                name="startDate"
                                value={formData.startDate}
                                onChange={handleChange}
                                variant="outlined"
                                InputLabelProps={{ shrink: true }}
                            />
                        </Grid>

                        <Grid item xs={12} sm={6}>
                            <TextField
                                fullWidth
                                type="date"
                                label="Due Date"
                                name="dueDate"
                                value={formData.dueDate}
                                onChange={handleChange}
                                variant="outlined"
                                InputLabelProps={{ shrink: true }}
                            />
                        </Grid>
                    </Grid>

                    {error && (
                        <Alert severity="error" sx={{ mt: 2 }}>
                            {error}
                        </Alert>
                    )}

                    <Box sx={{ mt: 3, display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
                        {onCancel && (
                            <Button
                                variant="outlined"
                                onClick={onCancel}
                                startIcon={<CancelIcon />}
                            >
                                Cancel
                            </Button>
                        )}
                        <Button
                            type="submit"
                            variant="contained"
                            disabled={loading}
                            startIcon={loading ? <CircularProgress size={20} /> : <SaveIcon />}
                        >
                            {loading ? 'Saving...' : isEditMode ? 'Update Project' : 'Create Project'}
                        </Button>
                    </Box>
                </Box>
            </Paper>

            {/* Confirmation Dialog */}
            <Dialog
                open={confirmDialog}
                onClose={() => setConfirmDialog(false)}
            >
                <DialogTitle>Confirm {isEditMode ? 'Update' : 'Create'}</DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        Are you sure you want to {isEditMode ? 'update' : 'create'} this project?
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setConfirmDialog(false)}>Cancel</Button>
                    <Button onClick={handleConfirmSave} variant="contained" autoFocus>
                        Confirm
                    </Button>
                </DialogActions>
            </Dialog>

            {/* Success Message */}
            <Snackbar
                open={!!successMessage}
                autoHideDuration={3000}
                onClose={() => setSuccessMessage('')}
                anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
            >
                <Alert onClose={() => setSuccessMessage('')} severity="success" sx={{ width: '100%' }}>
                    {successMessage}
                </Alert>
            </Snackbar>
        </Container>
    );
}

export default ProjectForm;