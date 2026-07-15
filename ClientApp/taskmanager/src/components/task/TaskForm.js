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
    Snackbar,
    MenuItem,
    FormControl,
    InputLabel,
    Select,
    Chip,
    OutlinedInput,
    Card,
    CardContent,
    Avatar,
    Divider
} from '@mui/material';
import {
    Save as SaveIcon,
    Cancel as CancelIcon,
    Assignment as AssignmentIcon,
    Schedule as ScheduleIcon,
    Flag as FlagIcon,
    Description as DescriptionIcon,
    Person as PersonIcon
} from '@mui/icons-material';
import { projectService } from '../../services/projectService';
import { taskService } from '../../services/taskService';
import { useAuth } from '../../contexts/AuthContext';

function TaskForm({ task = null, projectId, projectMembers = [], onSuccess, onCancel }) {
    const { hasPermission, user } = useAuth();
    const isEditMode = !!task;

    const [formData, setFormData] = useState({
        taskTitle: '',
        taskDescription: '',
        priority: 2,
        dueDate: '',
        estimatedHours: '',
        assignedTo: '',
        tags: []
    });

    const [loading, setLoading] = useState(false);
    const [usersLoading, setUsersLoading] = useState(false);
    const [tagsLoading, setTagsLoading] = useState(false);
    const [error, setError] = useState(null);
    const [confirmDialog, setConfirmDialog] = useState(false);
    const [successMessage, setSuccessMessage] = useState('');
    const [availableUsers, setAvailableUsers] = useState([]);
    const [availableTags, setAvailableTags] = useState([]);

    useEffect(() => {
        fetchUsers();
        fetchTags();
        if (task) {
            console.log('Task data received:', task);

            // Parse tags properly
            let taskTagIds = [];
            if (task.tags && Array.isArray(task.tags)) {
                taskTagIds = task.tags.map(tag => {
                    if (typeof tag === 'object') {
                        return tag.tagId || tag.TagId;
                    }
                    return tag;
                }).filter(id => id != null);
            } else if (task.tags && typeof task.tags === 'string' && task.tags !== '[]') {
                try {
                    const parsedTags = JSON.parse(task.tags);
                    taskTagIds = parsedTags.map(tag => {
                        if (typeof tag === 'object') {
                            return tag.tagId || tag.TagId;
                        }
                        return tag;
                    }).filter(id => id != null);
                } catch (error) {
                    console.error('Error parsing tags:', error);
                }
            }

            const formDataToSet = {
                taskTitle: task.taskTitle || '',
                taskDescription: task.taskDescription || '',
                priority: task.priority || 2,
                dueDate: task.dueDate ? task.dueDate.split('T')[0] : '',
                estimatedHours: task.estimatedHours || '',
                assignedTo: task.assigneeGuid || '',
                tags: taskTagIds
            };

            console.log('Setting form data:', formDataToSet);
            setFormData(formDataToSet);
        }
    }, [task]);

    const fetchUsers = async () => {
        try {
            setUsersLoading(true);
            const response = await projectService.getTaskManagerUsers();
            if (response.success) {
                setAvailableUsers(response.data || []);
            }
        } catch (err) {
            console.error('Error fetching users:', err);
        } finally {
            setUsersLoading(false);
        }
    };

    const fetchTags = async () => {
        try {
            setTagsLoading(true);
            const response = await projectService.getActiveTags();
            if (response.success) {
                setAvailableTags(response.data || []);
            }
        } catch (err) {
            console.error('Error fetching tags:', err);
        } finally {
            setTagsLoading(false);
        }
    };

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData({
            ...formData,
            [name]: value
        });
    };

    const handleTagChange = (event) => {
        const value = event.target.value;
        setFormData({
            ...formData,
            tags: typeof value === 'string' ? value.split(',') : value
        });
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        if (!formData.taskTitle.trim()) {
            setError('Task title is required');
            return;
        }
        setConfirmDialog(true);
    };

    const handleConfirmSave = async () => {
        setConfirmDialog(false);
        setLoading(true);
        setError(null);

        try {
            const taskData = {
                ...formData,
                projectGuid: projectId
            };

            let response;
            if (isEditMode) {
                response = await taskService.updateTask(task.taskGuid, taskData);
            } else {
                response = await taskService.createTask(taskData);
            }

            if (response.success) {
                setSuccessMessage(`Task ${isEditMode ? 'updated' : 'created'} successfully!`);
                setTimeout(() => {
                    if (onSuccess) {
                        onSuccess();
                    }
                }, 1500);
            } else {
                setError(response.message || `Failed to ${isEditMode ? 'update' : 'create'} task`);
            }
        } catch (err) {
            setError(err.response?.data?.message || err.message || `Failed to ${isEditMode ? 'update' : 'create'} task`);
        } finally {
            setLoading(false);
        }
    };

    const getPriorityLabel = (priority) => {
        switch (priority) {
            case 1: return 'Low';
            case 2: return 'Medium';
            case 3: return 'High';
            case 4: return 'Critical';
            default: return 'Medium';
        }
    };

    const getPriorityColor = (priority) => {
        switch (priority) {
            case 1: return 'success';
            case 2: return 'info';
            case 3: return 'warning';
            case 4: return 'error';
            default: return 'info';
        }
    };

    const getSelectedUser = () => {
        return availableUsers.find(user => user.userGuid === formData.assignedTo);
    };

    return (
        <Container maxWidth="lg">
            <Paper elevation={3} sx={{ p: 4, mt: 2 }}>
                <Box display="flex" alignItems="center" gap={2} mb={3}>
                    <AssignmentIcon color="primary" sx={{ fontSize: 30 }} />
                    <Typography variant="h4" component="h1">
                        {isEditMode ? 'Edit Task' : 'Create New Task'}
                    </Typography>
                </Box>

                <Box component="form" onSubmit={handleSubmit}>
                    <Grid container spacing={4}>
                        {/* Task Basic Information */}
                        <Grid item xs={12}>
                            <Card variant="outlined">
                                <CardContent>
                                    <Box display="flex" alignItems="center" gap={1} mb={2}>
                                        <DescriptionIcon color="primary" />
                                        <Typography variant="h6">Task Information</Typography>
                                    </Box>

                                    <Grid container spacing={3}>
                                        <Grid item xs={12}>
                                            <TextField
                                                fullWidth
                                                required
                                                label="Task Title"
                                                name="taskTitle"
                                                value={formData.taskTitle}
                                                onChange={handleChange}
                                                variant="outlined"
                                                placeholder="Enter a clear and concise task title"
                                                sx={{ mb: 2 }}
                                            />
                                        </Grid>

                                        <Grid item xs={12}>
                                            <TextField
                                                fullWidth
                                                multiline
                                                rows={4}
                                                label="Task Description"
                                                name="taskDescription"
                                                value={formData.taskDescription}
                                                onChange={handleChange}
                                                variant="outlined"
                                                placeholder="Provide detailed description of the task requirements and acceptance criteria"
                                            />
                                        </Grid>
                                    </Grid>
                                </CardContent>
                            </Card>
                        </Grid>

                        {/* Task Priority and Scheduling */}
                        <Grid item xs={12} md={6}>
                            <Card variant="outlined">
                                <CardContent>
                                    <Box display="flex" alignItems="center" gap={1} mb={2}>
                                        <FlagIcon color="primary" />
                                        <Typography variant="h6">Priority & Timeline</Typography>
                                    </Box>

                                    <Grid container spacing={2}>
                                        <Grid item xs={12}>
                                            <FormControl fullWidth>
                                                <InputLabel>Priority Level</InputLabel>
                                                <Select
                                                    name="priority"
                                                    value={formData.priority}
                                                    onChange={handleChange}
                                                    label="Priority Level"
                                                >
                                                    <MenuItem value={1}>
                                                        <Box display="flex" alignItems="center" gap={1}>
                                                            <Chip label="Low" size="small" color="success" />
                                                            <Typography variant="body2">- Flexible timeline</Typography>
                                                        </Box>
                                                    </MenuItem>
                                                    <MenuItem value={2}>
                                                        <Box display="flex" alignItems="center" gap={1}>
                                                            <Chip label="Medium" size="small" color="info" />
                                                            <Typography variant="body2">- Standard priority</Typography>
                                                        </Box>
                                                    </MenuItem>
                                                    <MenuItem value={3}>
                                                        <Box display="flex" alignItems="center" gap={1}>
                                                            <Chip label="High" size="small" color="warning" />
                                                            <Typography variant="body2">- Important task</Typography>
                                                        </Box>
                                                    </MenuItem>
                                                    <MenuItem value={4}>
                                                        <Box display="flex" alignItems="center" gap={1}>
                                                            <Chip label="Critical" size="small" color="error" />
                                                            <Typography variant="body2">- Urgent attention</Typography>
                                                        </Box>
                                                    </MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>

                                        <Grid item xs={12}>
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

                                        <Grid item xs={12}>
                                            <TextField
                                                fullWidth
                                                type="number"
                                                label="Estimated Hours"
                                                name="estimatedHours"
                                                value={formData.estimatedHours}
                                                onChange={handleChange}
                                                variant="outlined"
                                                placeholder="e.g., 8.5"
                                                inputProps={{ min: 0, step: 0.5 }}
                                                helperText="Estimated time to complete this task"
                                            />
                                        </Grid>
                                    </Grid>
                                </CardContent>
                            </Card>
                        </Grid>

                        {/* Task Assignment */}
                        <Grid item xs={12} md={6}>
                            <Card variant="outlined">
                                <CardContent>
                                    <Box display="flex" alignItems="center" gap={1} mb={2}>
                                        <PersonIcon color="primary" />
                                        <Typography variant="h6">Assignment & Tags</Typography>
                                    </Box>

                                    <Grid container spacing={2}>
                                        <Grid item xs={12}>
                                            <FormControl fullWidth>
                                                <InputLabel>Assign To</InputLabel>
                                                <Select
                                                    name="assignedTo"
                                                    value={formData.assignedTo}
                                                    onChange={handleChange}
                                                    label="Assign To"
                                                    disabled={usersLoading}
                                                >
                                                    <MenuItem value="">
                                                        <Box display="flex" alignItems="center" gap={1}>
                                                            <Avatar sx={{ width: 24, height: 24 }}>
                                                                <PersonIcon />
                                                            </Avatar>
                                                            <em>Unassigned</em>
                                                        </Box>
                                                    </MenuItem>
                                                    {availableUsers.map((user) => (
                                                        <MenuItem key={user.userGuid} value={user.userGuid}>
                                                            <Box display="flex" alignItems="center" gap={1}>
                                                                <Avatar sx={{ width: 24, height: 24 }}>
                                                                    {user.userName?.charAt(0).toUpperCase()}
                                                                </Avatar>
                                                                <Box>
                                                                    <Typography variant="body2">
                                                                        {user.userName}
                                                                    </Typography>
                                                                    <Typography variant="caption" color="text.secondary">
                                                                        {user.email} • {user.role}
                                                                    </Typography>
                                                                </Box>
                                                            </Box>
                                                        </MenuItem>
                                                    ))}
                                                </Select>
                                                {usersLoading && (
                                                    <Box display="flex" alignItems="center" gap={1} mt={1}>
                                                        <CircularProgress size={16} />
                                                        <Typography variant="caption">Loading users...</Typography>
                                                    </Box>
                                                )}
                                            </FormControl>
                                        </Grid>

                                        {/* Show selected user info */}
                                        {formData.assignedTo && getSelectedUser() && (
                                            <Grid item xs={12}>
                                                <Box sx={{ p: 2, bgcolor: 'primary.50', borderRadius: 1, border: '1px solid', borderColor: 'primary.200' }}>
                                                    <Box display="flex" alignItems="center" gap={2}>
                                                        <Avatar>
                                                            {getSelectedUser().userName?.charAt(0).toUpperCase()}
                                                        </Avatar>
                                                        <Box>
                                                            <Typography variant="body2" fontWeight="medium">
                                                                Assigned to: {getSelectedUser().userName}
                                                            </Typography>
                                                            <Typography variant="caption" color="text.secondary">
                                                                {getSelectedUser().email} • {getSelectedUser().role}
                                                            </Typography>
                                                        </Box>
                                                    </Box>
                                                </Box>
                                            </Grid>
                                        )}

                                        <Grid item xs={12}>
                                            <FormControl fullWidth>
                                                <InputLabel>Tags</InputLabel>
                                                <Select
                                                    multiple
                                                    value={formData.tags}
                                                    onChange={handleTagChange}
                                                    input={<OutlinedInput label="Tags" />}
                                                    disabled={tagsLoading}
                                                    renderValue={(selected) => (
                                                        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                                                            {selected.map((tagId) => {
                                                                const tag = availableTags.find(t => t.tagId === tagId);
                                                                return (
                                                                    <Chip
                                                                        key={tagId}
                                                                        label={tag?.tagName || 'Unknown'}
                                                                        size="small"
                                                                        style={{ backgroundColor: tag?.colorCode || '#666', color: 'white' }}
                                                                    />
                                                                );
                                                            })}
                                                        </Box>
                                                    )}
                                                >
                                                    {availableTags.map((tag) => (
                                                        <MenuItem key={tag.tagId} value={tag.tagId}>
                                                            <Chip
                                                                label={tag.tagName}
                                                                size="small"
                                                                style={{ backgroundColor: tag.colorCode, color: 'white' }}
                                                            />
                                                        </MenuItem>
                                                    ))}
                                                </Select>
                                                {tagsLoading && (
                                                    <Box display="flex" alignItems="center" gap={1} mt={1}>
                                                        <CircularProgress size={16} />
                                                        <Typography variant="caption">Loading tags...</Typography>
                                                    </Box>
                                                )}
                                            </FormControl>
                                        </Grid>
                                    </Grid>
                                </CardContent>
                            </Card>
                        </Grid>
                    </Grid>

                    {error && (
                        <Alert severity="error" sx={{ mt: 3 }}>
                            {error}
                        </Alert>
                    )}

                    <Divider sx={{ my: 3 }} />

                    <Box display="flex" gap={2} justifyContent="flex-end">
                        {onCancel && (
                            <Button
                                variant="outlined"
                                size="large"
                                onClick={onCancel}
                                startIcon={<CancelIcon />}
                                sx={{ minWidth: 120 }}
                            >
                                Cancel
                            </Button>
                        )}
                        <Button
                            type="submit"
                            variant="contained"
                            size="large"
                            disabled={loading}
                            startIcon={loading ? <CircularProgress size={20} /> : <SaveIcon />}
                            sx={{ minWidth: 160 }}
                        >
                            {loading ? 'Saving...' : isEditMode ? 'Update Task' : 'Create Task'}
                        </Button>
                    </Box>
                </Box>
            </Paper>

            {/* Confirmation Dialog */}
            <Dialog
                open={confirmDialog}
                onClose={() => setConfirmDialog(false)}
                maxWidth="sm"
                fullWidth
            >
                <DialogTitle>
                    Confirm {isEditMode ? 'Update' : 'Create'} Task
                </DialogTitle>
                <DialogContent>
                    <DialogContentText sx={{ mb: 2 }}>
                        Are you sure you want to {isEditMode ? 'update' : 'create'} this task?
                    </DialogContentText>
                    <Box sx={{ bgcolor: 'grey.50', p: 2, borderRadius: 1 }}>
                        <Typography variant="body2"><strong>Title:</strong> {formData.taskTitle}</Typography>
                        <Typography variant="body2"><strong>Priority:</strong> {getPriorityLabel(formData.priority)}</Typography>
                        <Typography variant="body2"><strong>Assigned to:</strong> {getSelectedUser()?.userName || 'Unassigned'}</Typography>
                        {formData.dueDate && (
                            <Typography variant="body2"><strong>Due Date:</strong> {new Date(formData.dueDate).toLocaleDateString()}</Typography>
                        )}
                    </Box>
                </DialogContent>
                <DialogActions sx={{ p: 2 }}>
                    <Button onClick={() => setConfirmDialog(false)}>Cancel</Button>
                    <Button onClick={handleConfirmSave} variant="contained" autoFocus>
                        Confirm {isEditMode ? 'Update' : 'Create'}
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

export default TaskForm;