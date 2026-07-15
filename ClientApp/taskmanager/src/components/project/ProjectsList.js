import React, { useState, useEffect } from 'react';
import {
    Container,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Typography,
    Chip,
    IconButton,
    Box,
    CircularProgress,
    Alert,
    Button
} from '@mui/material';
import {
    Edit as EditIcon,
    Visibility as ViewIcon,
    ArrowBack as ArrowBackIcon,
    Add as AddIcon
} from '@mui/icons-material';
import { projectService } from '../../services/projectService';
import { useAuth } from '../../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import ProjectForm from './ProjectForm';

function ProjectsList() {
    const navigate = useNavigate();
    const { hasPermission } = useAuth();

    const [projects, setProjects] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [showCreateForm, setShowCreateForm] = useState(false);
    const [editingProject, setEditingProject] = useState(null);

    useEffect(() => {
        fetchProjects();
    }, []);

    const fetchProjects = async () => {
        try {
            setLoading(true);
            const response = await projectService.getProjects();

            if (response && response.success) {
                setProjects(response.data || []);
            } else {
                setError(response?.message || 'Failed to fetch projects');
            }
        } catch (err) {
            console.error('Error fetching projects:', err);
            setError(err.response?.data?.message || err.message || 'Failed to fetch projects');
        } finally {
            setLoading(false);
        }
    };

    const handleViewProject = (project) => {
        const projectGuid = project.projectGuid || project.ProjectGuid;
        navigate(`/projects/${projectGuid}`);
    };

    const handleEditProject = (project) => {
        setEditingProject(project);
    };

    const handleBackToHome = () => {
        navigate('/');
    };

    const handleCreateProject = () => {
        setShowCreateForm(true);
    };

    const handleFormSuccess = () => {
        setShowCreateForm(false);
        setEditingProject(null);
        fetchProjects(); // Refresh the projects list
    };

    const handleFormCancel = () => {
        setShowCreateForm(false);
        setEditingProject(null);
    };

    // Show create form
    if (showCreateForm) {
        return (
            <ProjectForm
                onSuccess={handleFormSuccess}
                onCancel={handleFormCancel}
            />
        );
    }

    // Show edit form
    if (editingProject) {
        return (
            <ProjectForm
                project={editingProject}
                onSuccess={handleFormSuccess}
                onCancel={handleFormCancel}
            />
        );
    }

    if (loading) {
        return (
            <Container sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
                <CircularProgress />
            </Container>
        );
    }

    if (error) {
        return (
            <Container sx={{ mt: 2 }}>
                <Alert severity="error">{error}</Alert>
            </Container>
        );
    }

    return (
        <Container maxWidth="lg" sx={{ mt: 2 }}>
            <Paper elevation={3} sx={{ p: 3 }}>
                {/* Header with back button, title and create button */}
                <Box display="flex" alignItems="center" justifyContent="space-between" mb={3}>
                    <Box display="flex" alignItems="center" gap={2}>
                        <IconButton
                            onClick={handleBackToHome}
                            size="small"
                            sx={{ color: 'primary.main' }}
                        >
                            <ArrowBackIcon />
                        </IconButton>
                        <Typography variant="h5" sx={{ textAlign: 'left' }}>
                            Projects
                        </Typography>
                    </Box>
                    {hasPermission('isAdmin') && (
                        <Button
                            variant="contained"
                            startIcon={<AddIcon />}
                            onClick={handleCreateProject}
                        >
                            Create Project
                        </Button>
                    )}
                </Box>

                {(!projects || projects.length === 0) ? (
                    <Alert severity="info">No projects found. Create your first project!</Alert>
                ) : (
                    <TableContainer>
                        <Table>
                            <TableHead>
                                <TableRow>
                                    <TableCell>Project Name</TableCell>
                                    <TableCell>Code</TableCell>
                                    <TableCell>Status</TableCell>
                                    <TableCell>Tasks</TableCell>
                                    <TableCell>Members</TableCell>
                                    <TableCell>Start Date</TableCell>
                                    <TableCell>Due Date</TableCell>
                                    <TableCell>Actions</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {projects.map((project) => (
                                    <TableRow key={project.projectGuid || project.ProjectGuid || project.projectId || project.ProjectId}>
                                        <TableCell>
                                            <Typography variant="body2" fontWeight="medium">
                                                {project.projectName || project.ProjectName || '-'}
                                            </Typography>
                                            {(project.projectDescription || project.ProjectDescription) && (
                                                <Typography variant="caption" color="text.secondary" display="block">
                                                    {(project.projectDescription || project.ProjectDescription).length > 50
                                                        ? (project.projectDescription || project.ProjectDescription).substring(0, 50) + '...'
                                                        : (project.projectDescription || project.ProjectDescription)
                                                    }
                                                </Typography>
                                            )}
                                        </TableCell>
                                        <TableCell>{project.projectCode || project.ProjectCode || '-'}</TableCell>
                                        <TableCell>
                                            <Chip
                                                label={(project.isActive || project.IsActive) ? 'Active' : 'Inactive'}
                                                color={(project.isActive || project.IsActive) ? 'success' : 'default'}
                                                size="small"
                                            />
                                        </TableCell>
                                        <TableCell>
                                            <Typography variant="body2">
                                                {project.completedTasks || project.CompletedTasks || 0}/{project.totalTasks || project.TotalTasks || 0}
                                            </Typography>
                                        </TableCell>
                                        <TableCell>
                                            <Typography variant="body2">
                                                {project.memberCount || project.MemberCount || 0}
                                            </Typography>
                                        </TableCell>
                                        <TableCell>
                                            <Typography variant="body2">
                                                {(project.startDate || project.StartDate) ? new Date(project.startDate || project.StartDate).toLocaleDateString() : '-'}
                                            </Typography>
                                        </TableCell>
                                        <TableCell>
                                            <Typography variant="body2">
                                                {(project.dueDate || project.DueDate) ? new Date(project.dueDate || project.DueDate).toLocaleDateString() : '-'}
                                            </Typography>
                                        </TableCell>
                                        <TableCell>
                                            <Box display="flex" gap={0.5}>
                                                <IconButton
                                                    size="small"
                                                    title="View Project"
                                                    onClick={() => handleViewProject(project)}
                                                    color="primary"
                                                >
                                                    <ViewIcon />
                                                </IconButton>
                                                {hasPermission('isAdmin') && (
                                                    <IconButton
                                                        size="small"
                                                        title="Edit Project"
                                                        onClick={() => handleEditProject(project)}
                                                        color="secondary"
                                                    >
                                                        <EditIcon />
                                                    </IconButton>
                                                )}
                                            </Box>
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </TableContainer>
                )}
            </Paper>
        </Container>
    );
}

export default ProjectsList;