import React, { useState, useEffect } from 'react';
import {
    Container,
    Paper,
    Typography,
    Box,
    Grid,
    Chip,
    Card,
    CardContent,
    CircularProgress,
    Alert,
    IconButton
} from '@mui/material';
import {
    ArrowBack as ArrowBackIcon,
    Edit as EditIcon,
    Delete as DeleteIcon
} from '@mui/icons-material';
import { useAuth } from '../../contexts/AuthContext';
import { projectService } from '../../services/projectService';
import TaskList from '../task/TaskList';
import TaskForm from '../task/TaskForm';
import ProjectForm from './ProjectForm';
import { useParams, useNavigate } from 'react-router-dom';

function ProjectDetail() {
    const { projectId } = useParams(); // Get projectId from URL
    const navigate = useNavigate(); // For navigation
    const { hasPermission } = useAuth();

    // State variables
    const [project, setProject] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [showEditForm, setShowEditForm] = useState(false);
    const [showTaskForm, setShowTaskForm] = useState(false);
    const [editingTask, setEditingTask] = useState(null);

    // Load project details
    useEffect(() => {
        if (projectId) {
            fetchProjectDetails();
        }
    }, [projectId]);

    const fetchProjectDetails = async () => {
        try {
            setLoading(true);
            setError(null);

            const projectResponse = await projectService.getProjectDetails(projectId);

            if (!projectResponse.success) {
                throw new Error(projectResponse.message);
            }

            setProject(projectResponse.data);

        } catch (err) {
            setError(err.message || 'Failed to load project details');
            console.error('Error fetching project details:', err);
        } finally {
            setLoading(false);
        }
    };

    const getProjectStats = () => {
        if (project) {
            return {
                total: project.totalTasks || 0,
                todo: project.todoTasks || 0,
                inProgress: project.inProgressTasks || 0,
                done: project.doneTasks || 0,
                blocked: project.blockedTasks || 0
            };
        }
        return { total: 0, todo: 0, inProgress: 0, done: 0, blocked: 0 };
    };

    const handleBackToProjects = () => {
        navigate('/projects');
    };

    const handleEditProject = () => {
        setShowEditForm(true);
    };

    const handleEditSuccess = () => {
        setShowEditForm(false);
        fetchProjectDetails(); // Refresh project data
    };

    const handleEditCancel = () => {
        setShowEditForm(false);
    };

    const handleCreateTask = () => {
        setEditingTask(null);
        setShowTaskForm(true);
    };

    const handleEditTask = (task) => {
        setEditingTask(task);
        setShowTaskForm(true);
    };

    const handleTaskFormSuccess = () => {
        setShowTaskForm(false);
        setEditingTask(null);
        fetchProjectDetails(); // Refresh project data
    };

    const handleTaskFormCancel = () => {
        setShowTaskForm(false);
        setEditingTask(null);
    };

    // Loading state
    if (loading) {
        return (
            <Container sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
                <CircularProgress />
            </Container>
        );
    }

    // Error state
    if (error) {
        return (
            <Container sx={{ mt: 2 }}>
                <Alert severity="error">{error}</Alert>
                <Box sx={{ mt: 2 }}>
                    <IconButton onClick={handleBackToProjects} color="primary">
                        <ArrowBackIcon />
                    </IconButton>
                </Box>
            </Container>
        );
    }

    // Show task form
    if (showTaskForm) {
        return (
            <TaskForm
                task={editingTask}
                projectId={projectId}
                onSuccess={handleTaskFormSuccess}
                onCancel={handleTaskFormCancel}
            />
        );
    }

    // Show edit form
    if (showEditForm) {
        return (
            <ProjectForm
                project={project}
                onSuccess={handleEditSuccess}
                onCancel={handleEditCancel}
            />
        );
    }

    const stats = getProjectStats();

    return (
        <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50', p: 3 }}>
            <Container maxWidth="xl">
                {/* Header with back button and project info */}
                <Paper elevation={1} sx={{ p: 2, mb: 3 }}>
                    <Box display="flex" justifyContent="space-between" alignItems="flex-start">
                        <Box display="flex" alignItems="center" gap={2} flex={1}>
                            <IconButton
                                onClick={handleBackToProjects}
                                size="small"
                                sx={{ color: 'primary.main' }}
                            >
                                <ArrowBackIcon />
                            </IconButton>
                            <Box flex={1}>
                                <Typography variant="h5" gutterBottom>
                                    {project?.projectName || 'Project Name'}
                                </Typography>
                                <Box display="flex" gap={2} alignItems="center" flexWrap="wrap">
                                    <Typography variant="body2" color="text.secondary">
                                        <strong>Code:</strong> {project?.projectCode || '-'}
                                    </Typography>
                                    <Chip
                                        label={project?.isActive ? 'Active' : 'Inactive'}
                                        color={project?.isActive ? 'success' : 'default'}
                                        size="small"
                                    />
                                    {project?.startDate && (
                                        <Typography variant="body2" color="text.secondary">
                                            <strong>Start:</strong> {new Date(project.startDate).toLocaleDateString()}
                                        </Typography>
                                    )}
                                    {project?.dueDate && (
                                        <Typography variant="body2" color="text.secondary">
                                            <strong>Due:</strong> {new Date(project.dueDate).toLocaleDateString()}
                                        </Typography>
                                    )}
                                </Box>
                                {project?.projectDescription && (
                                    <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                                        {project.projectDescription}
                                    </Typography>
                                )}
                            </Box>
                        </Box>
                        {hasPermission('isAdmin') && (
                            <Box display="flex" gap={1}>
                                <IconButton onClick={handleEditProject} color="primary" size="small">
                                    <EditIcon />
                                </IconButton>
                                <IconButton color="error" size="small">
                                    <DeleteIcon />
                                </IconButton>
                            </Box>
                        )}
                    </Box>
                </Paper>

                {/* Statistics Cards */}
                <Grid container spacing={2} sx={{ mb: 3 }}>
                    <Grid item xs={6} sm={3} md={2.4}>
                        <Card elevation={2} sx={{ height: '100%' }}>
                            <CardContent sx={{ textAlign: 'center', py: 2 }}>
                                <Typography variant="h5" fontWeight="bold" color="text.primary">
                                    {stats.total}
                                </Typography>
                                <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.75rem' }}>
                                    Total Tasks
                                </Typography>
                            </CardContent>
                        </Card>
                    </Grid>
                    <Grid item xs={6} sm={3} md={2.4}>
                        <Card elevation={2} sx={{ height: '100%' }}>
                            <CardContent sx={{ textAlign: 'center', py: 2 }}>
                                <Typography variant="h5" fontWeight="bold" color="text.secondary">
                                    {stats.todo}
                                </Typography>
                                <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.75rem' }}>
                                    To Do
                                </Typography>
                            </CardContent>
                        </Card>
                    </Grid>
                    <Grid item xs={6} sm={3} md={2.4}>
                        <Card elevation={2} sx={{ height: '100%' }}>
                            <CardContent sx={{ textAlign: 'center', py: 2 }}>
                                <Typography variant="h5" fontWeight="bold" color="primary.main">
                                    {stats.inProgress}
                                </Typography>
                                <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.75rem' }}>
                                    In Progress
                                </Typography>
                            </CardContent>
                        </Card>
                    </Grid>
                    <Grid item xs={6} sm={3} md={2.4}>
                        <Card elevation={2} sx={{ height: '100%' }}>
                            <CardContent sx={{ textAlign: 'center', py: 2 }}>
                                <Typography variant="h5" fontWeight="bold" color="success.main">
                                    {stats.done}
                                </Typography>
                                <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.75rem' }}>
                                    Done
                                </Typography>
                            </CardContent>
                        </Card>
                    </Grid>
                    <Grid item xs={6} sm={3} md={2.4}>
                        <Card elevation={2} sx={{ height: '100%' }}>
                            <CardContent sx={{ textAlign: 'center', py: 2 }}>
                                <Typography variant="h5" fontWeight="bold" color="error.main">
                                    {stats.blocked}
                                </Typography>
                                <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.75rem' }}>
                                    Blocked
                                </Typography>
                            </CardContent>
                        </Card>
                    </Grid>
                </Grid>

                {/* Tasks Section using TaskList Component */}
                <TaskList
                    filterType="Project"
                    filterValue={projectId}
                    title="Project Tasks"
                    showProjectColumn={false}
                    onCreateTask={handleCreateTask}
                    onViewProjects={() => {
                        navigate('/projects');
                    }}
                    onEditTask={handleEditTask}
                />
            </Container>
        </Box>
    );
}

export default ProjectDetail;