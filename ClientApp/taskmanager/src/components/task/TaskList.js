import React, { useState, useEffect } from 'react';
import {
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Chip,
    Select,
    MenuItem,
    FormControl,
    Typography,
    Box,
    CircularProgress,
    Alert,
    Button,
    IconButton
} from '@mui/material';
import {
    Add as AddIcon,
    List as ListIcon,
    Edit as EditIcon
} from '@mui/icons-material';
import { useAuth } from '../../contexts/AuthContext';
import { taskService } from '../../services/taskService';

function TaskList({
    filterType = 'User',
    filterValue = 'All',
    title = 'Tasks',
    showProjectColumn = true,
    onCreateTask = () => { },
    onViewProjects = () => { },
    onEditTask = () => { }
}) {
    const { hasPermission } = useAuth();
    const [tasks, setTasks] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        loadTasks();
    }, [filterType, filterValue]);

    const loadTasks = async () => {
        try {
            setLoading(true);
            setError(null);
            const response = await taskService.getTasks(filterType, filterValue);
            if (response.success) {
                setTasks(response.data);
            } else {
                setError(response.message || 'Failed to load tasks');
            }
        } catch (error) {
            console.error('Error loading tasks:', error);
            setError('Failed to load tasks');
        } finally {
            setLoading(false);
        }
    };

    const handleStatusChange = async (taskId, newStatusId) => {
        try {
            const response = await taskService.updateTaskStatus(taskId, newStatusId);
            if (response.success) {
                // Refresh the task list
                loadTasks();
            } else {
                alert(response.message || 'Failed to update task status');
            }
        } catch (error) {
            console.error('Error updating task status:', error);
            alert('Failed to update task status');
        }
    };

    const getPriorityColor = (priority) => {
        switch (parseInt(priority)) {
            case 4: return 'error';    // Critical
            case 3: return 'error';    // High
            case 2: return 'warning';  // Medium
            case 1: return 'success';  // Low
            default: return 'default';
        }
    };

    const getPriorityText = (priority) => {
        switch (parseInt(priority)) {
            case 4: return 'Critical';
            case 3: return 'High';
            case 2: return 'Medium';
            case 1: return 'Low';
            default: return 'Unknown';
        }
    };

    const getStatusColor = (status) => {
        switch (status?.toLowerCase()) {
            case 'to do': return 'default';
            case 'in progress': return 'primary';
            case 'review': return 'warning';
            case 'done': return 'success';
            case 'blocked': return 'error';
            default: return 'default';
        }
    };

    const formatDate = (dateString) => {
        if (!dateString) return '-';
        return new Date(dateString).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    };

    const isAdmin = hasPermission('isAdmin');
    const showAssignedToColumn = isAdmin && filterType !== 'User';

    if (loading) {
        return (
            <Paper elevation={2} sx={{ p: 4, textAlign: 'center' }}>
                <CircularProgress />
                <Typography variant="body2" sx={{ mt: 2 }}>
                    Loading tasks...
                </Typography>
            </Paper>
        );
    }

    if (error) {
        return (
            <Paper elevation={2} sx={{ p: 2 }}>
                <Alert severity="error">{error}</Alert>
            </Paper>
        );
    }

    return (
        <Paper elevation={2}>
            <Box sx={{ p: 2, borderBottom: 1, borderColor: 'divider' }}>
                <Box display="flex" justifyContent="space-between" alignItems="center">
                    <Typography variant="h6" sx={{ textAlign: 'left' }}>
                        {title}
                    </Typography>
                    {isAdmin && (
                        <Box display="flex" gap={1}>
                            <Button
                                variant="outlined"
                                size="small"
                                startIcon={<AddIcon />}
                                onClick={() => onCreateTask && onCreateTask()}
                            >
                                Create Task
                            </Button>
                            <Button
                                variant="outlined"
                                size="small"
                                startIcon={<ListIcon />}
                                onClick={() => onViewProjects && onViewProjects()}
                            >
                                View Projects
                            </Button>
                        </Box>
                    )}
                </Box>
            </Box>

            <TableContainer>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Task Title</TableCell>
                            {showProjectColumn && <TableCell>Project</TableCell>}
                            {showAssignedToColumn && <TableCell>Assigned To</TableCell>}
                            <TableCell>Status</TableCell>
                            <TableCell>Priority</TableCell>
                            <TableCell>Due Date</TableCell>
                            <TableCell>Actions</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {tasks.length === 0 ? (
                            <TableRow>
                                <TableCell
                                    colSpan={6 + (showProjectColumn ? 1 : 0) + (showAssignedToColumn ? 1 : 0)}
                                    align="center"
                                >
                                    <Typography variant="body2" color="text.secondary" py={4}>
                                        No tasks found
                                    </Typography>
                                </TableCell>
                            </TableRow>
                        ) : (
                            tasks.map((task) => (
                                <TableRow key={task.taskId} hover>
                                    <TableCell>
                                        <Box>
                                            <Typography variant="body2" fontWeight="medium">
                                                {task.taskTitle}
                                            </Typography>
                                            {task.taskDescription && (
                                                <Typography variant="caption" color="text.secondary" display="block">
                                                    {task.taskDescription.length > 100
                                                        ? task.taskDescription.substring(0, 100) + '...'
                                                        : task.taskDescription
                                                    }
                                                </Typography>
                                            )}
                                        </Box>
                                    </TableCell>
                                    {showProjectColumn && (
                                        <TableCell>
                                            <Typography variant="body2" color="text.secondary">
                                                {task.projectName}
                                            </Typography>
                                        </TableCell>
                                    )}
                                    {showAssignedToColumn && (
                                        <TableCell>
                                            <Typography variant="body2" color="text.secondary">
                                                {task.userName || task.assignedTo || 'Unassigned'}
                                            </Typography>
                                        </TableCell>
                                    )}
                                    <TableCell>
                                        <Chip
                                            label={task.statusName}
                                            color={getStatusColor(task.statusName)}
                                            size="small"
                                        />
                                    </TableCell>
                                    <TableCell>
                                        <Chip
                                            label={getPriorityText(task.priority)}
                                            color={getPriorityColor(task.priority)}
                                            size="small"
                                            variant="outlined"
                                        />
                                    </TableCell>
                                    <TableCell>
                                        <Typography variant="body2">
                                            {formatDate(task.dueDate)}
                                        </Typography>
                                    </TableCell>
                                    <TableCell>
                                        <Box display="flex" gap={1}>
                                            <FormControl size="small" sx={{ minWidth: 120 }}>
                                                <Select
                                                    value={task.statusId}
                                                    onChange={(e) => handleStatusChange(task.taskId, e.target.value)}
                                                    displayEmpty
                                                >
                                                    <MenuItem value={1}>To Do</MenuItem>
                                                    <MenuItem value={2}>In Progress</MenuItem>
                                                    <MenuItem value={3}>Review</MenuItem>
                                                    <MenuItem value={4}>Done</MenuItem>
                                                    <MenuItem value={5}>Blocked</MenuItem>
                                                </Select>
                                            </FormControl>
                                            {typeof onEditTask === 'function' && (
                                                <IconButton
                                                    size="small"
                                                    onClick={() => onEditTask(task)}
                                                    color="primary"
                                                    title="Edit Task"
                                                >
                                                    <EditIcon />
                                                </IconButton>
                                            )}
                                        </Box>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </TableContainer>
        </Paper>
    );
}

export default TaskList;