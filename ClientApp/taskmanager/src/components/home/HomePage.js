import React, { useState, useEffect } from 'react';
import {
    Box,
    Typography,
    Container,
    Paper,
    Grid,
    Card,
    CardContent,
    Chip,
    CircularProgress
} from '@mui/material';
import {
    Person as PersonIcon
} from '@mui/icons-material';
import * as Icons from '@mui/icons-material';
import { useAuth } from '../../contexts/AuthContext';
import { homeService } from '../../services/homeService';
import TaskList from '../../components/task/TaskList';
import TaskForm from '../../components/task/TaskForm';
import { useNavigate } from 'react-router-dom';

function HomePage() {
    const { user, hasPermission } = useAuth();
    const navigate = useNavigate();
    const [loading, setLoading] = useState(true);
    const [taskCounts, setTaskCounts] = useState([]);
    const [showTaskForm, setShowTaskForm] = useState(false);

    useEffect(() => {
        loadTaskCounts();
    }, []);

    const loadTaskCounts = async () => {
        try {
            setLoading(true);
            const response = await homeService.getTaskCounts();
            if (response.success) {
                setTaskCounts(response.data);
            }
        } catch (error) {
            console.error('Error loading task counts:', error);
        } finally {
            setLoading(false);
        }
    };

    const getIconComponent = (iconName) => {
        return Icons[iconName] || Icons.Task;
    };

    const getStatusIconColor = (statusName) => {
        switch (statusName?.toLowerCase()) {
            case 'to do': return 'action';
            case 'in progress': return 'primary';
            case 'review': return 'warning';
            case 'done': return 'success';
            case 'blocked': return 'error';
            default: return 'action';
        }
    };

    const handleCreateTask = () => {
        setShowTaskForm(true);
    };

    const handleTaskFormSuccess = () => {
        setShowTaskForm(false);
        loadTaskCounts();
    };

    const handleTaskFormCancel = () => {
        setShowTaskForm(false);
    };

    const renderCountCards = () => {
        if (loading) {
            return (
                <Box display="flex" justifyContent="center" my={4}>
                    <CircularProgress />
                </Box>
            );
        }

        return (
            <Grid container spacing={2} sx={{ mb: 3 }}>
                {taskCounts.map((statusCount, index) => {
                    const IconComponent = getIconComponent(statusCount.iconName);
                    return (
                        <Grid item xs={6} sm={4} md={2.4} key={index}>
                            <Card elevation={2} sx={{ height: '100%' }}>
                                <CardContent sx={{ py: 2, px: 3 }}>
                                    <Box display="flex" alignItems="flex-start" gap={2}>
                                        <IconComponent
                                            color={getStatusIconColor(statusCount.statusName)}
                                            sx={{ fontSize: 32, mt: 0.5 }}
                                        />
                                        <Box flex={1}>
                                            <Typography variant="body1" color="text.secondary" sx={{ fontSize: '0.875rem', mb: 0.5 }}>
                                                {statusCount.statusName}
                                            </Typography>
                                            <Typography variant="h5" fontWeight="bold" color="text.primary">
                                                {statusCount.taskCount}
                                            </Typography>
                                        </Box>
                                    </Box>
                                </CardContent>
                            </Card>
                        </Grid>
                    );
                })}
            </Grid>
        );
    };

    // Show task form if creating task
    if (showTaskForm) {
        return (
            <TaskForm
                onSuccess={handleTaskFormSuccess}
                onCancel={handleTaskFormCancel}
            />
        );
    }

    return (
        <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50', p: 3 }}>
            <Container maxWidth="xl">
                {/* User Info Header */}
                <Paper elevation={1} sx={{ p: 2, mb: 3 }}>
                    <Box display="flex" alignItems="center" gap={2}>
                        <PersonIcon color="action" />
                        {/*<Typography variant="h6">*/}
                        {/*    Welcome, {user?.username || user?.userGuid}*/}
                        {/*</Typography>*/}
                        <Chip
                            label={hasPermission('isAdmin') ? 'Admin' : 'Team Member'}
                            color={hasPermission('isAdmin') ? 'primary' : 'default'}
                            size="small"
                        />
                    </Box>
                </Paper>

                {/* Count Cards */}
                {renderCountCards()}

                {/* Tasks Table */}
                <TaskList
                    filterType="User"
                    filterValue={hasPermission('isAdmin') ? 'All' : user?.userGuid}
                    title={hasPermission('isAdmin') ? 'All Tasks' : 'My Tasks'}
                    showProjectColumn={true}
                    onCreateTask={handleCreateTask}
                    onViewProjects={() => navigate('/projects')}
                />
            </Container>
        </Box>
    );
}

export default HomePage;