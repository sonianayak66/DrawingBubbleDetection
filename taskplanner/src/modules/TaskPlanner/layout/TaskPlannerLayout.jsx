import React, { useState, useEffect } from 'react';
import { Box, AppBar, Toolbar, Typography, Button } from '@mui/material';
import { Add, Edit, Delete } from '@mui/icons-material';
import { taskPlannerApi } from '../../../services/api';
import PermissionGuard from '../../../modules/shared/components/Common/PermissionGuard';

// NEW IMPORTS - Use these instead
import ProjectDialog from '../components/Projects/ProjectDialog';
import ProjectsListView from '../views/ProjectsListView';
import ProjectDetailView from '../views/ProjectDetailView';
import MyTasksView from '../views/MyTasksView';
import MyDayView from '../views/MyDayView';
import EmailManagementView from '../views/EmailManagementView';

// Placeholder for views we haven't moved yet
const PlaceholderView = ({ title, description }) => (
  <Box sx={{ p: 3, textAlign: 'center' }}>
    <Typography variant="h4" gutterBottom>{title}</Typography>
    <Typography variant="body1" color="text.secondary">{description}</Typography>
  </Box>
);

const TaskPlannerLayout = ({ selectedView, onViewChange, viewContext }) => {
  const [projects, setProjects] = useState([]);
  const [projectDialogOpen, setProjectDialogOpen] = useState(false);
  const [selectedProject, setSelectedProject] = useState(null);

  useEffect(() => {
    loadProjects();
  }, []);

  const loadProjects = async () => {
    try {
      const response = await taskPlannerApi.getProjects();
      setProjects(response.data || []);
    } catch (err) {
      console.error('Error loading projects:', err);
    }
  };

  const handleSaveProject = async (projectData) => {
    await taskPlannerApi.saveProject(projectData);
    await loadProjects();
    setProjectDialogOpen(false);
    setSelectedProject(null);
  };

  const handleEditProject = (project) => {
    setSelectedProject(project);
    setProjectDialogOpen(true);
  };

  const renderPageActions = () => {
    switch (selectedView) {
      case 'projects':
        return (
          <PermissionGuard permission="TaskPlanner_Projects_Write">
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => setProjectDialogOpen(true)}
              sx={{ mr: 2 }}
            >
              New Project
            </Button>
          </PermissionGuard>
        );
      default:
        return null;
    }
  };

  const getPageTitle = () => {
    switch (selectedView) {
      case 'my-day': return 'My Day';
      case 'my-tasks': return 'My Tasks';
      case 'projects': return 'Projects';
      case 'project-detail': 
        const project = projects.find(p => p.ProjectGUID === viewContext?.projectGuid);
        return project ? project.ProjectName : 'Project';
      case 'pinned': return 'Pinned';
      case 'emails': return 'Email Integration';
      case 'reports': return 'Reports';
      default: return 'Task Planner';
    }
  };

  const renderContent = () => {
    switch (selectedView) {
      case 'my-day':
        return <MyDayView />;
      
      case 'my-tasks':
        return (
          <PermissionGuard permission="TaskPlanner_Tasks_Read">
            <MyTasksView onCreateTask={() => {}} />
          </PermissionGuard>
        );
      
      case 'projects':
        return (
          <ProjectsListView 
            projects={projects}
            onCreateProject={() => setProjectDialogOpen(true)}
            onEditProject={handleEditProject}
            onDeleteProject={async (project) => {
              if (window.confirm(`Are you sure you want to delete "${project.ProjectName}"?`)) {
                try {
                  await taskPlannerApi.deleteProject(project.ProjectGUID);
                  await loadProjects();
                } catch (err) {
                  console.error('Error deleting project:', err);
                  alert('Error deleting project: ' + err.message);
                }
              }
            }}
            onProjectClick={(projectGuid) => onViewChange('project-detail', { projectGuid })}
          />
        );
      
      case 'project-detail':
        return (
          <ProjectDetailView 
            projectGuid={viewContext?.projectGuid}
            projects={projects}
          />
        );
      
      case 'pinned':
        return (
          <PlaceholderView 
            title="Pinned Items" 
            description="Your favorite projects and tasks - Coming soon!"
          />
        );
      
      case 'emails':
        return <EmailManagementView />;
      
      case 'reports':
        return (
          <PlaceholderView 
            title="Reports & Analytics" 
            description="Project insights and team performance - Coming soon!"
          />
        );
      
      default:
        return (
          <PlaceholderView 
            title="Welcome to Task Planner" 
            description="Select a view from the sidebar to get started."
          />
        );
    }
  };

  return (
    <Box sx={{ 
      flexGrow: 1, 
      display: 'flex', 
      flexDirection: 'column',
      overflow: 'hidden'
    }}>
      {/* Top App Bar */}
      <AppBar 
        position="static" 
        elevation={0}
        sx={{ 
          borderBottom: '1px solid',
          borderColor: 'divider',
          bgcolor: 'background.paper',
          color: 'text.primary'
        }}
      >
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1, fontWeight: 600 }}>
            {getPageTitle()}
          </Typography>
          
          {renderPageActions()}
        </Toolbar>
      </AppBar>

      {/* Content Area */}
      <Box sx={{ 
        flexGrow: 1, 
        overflow: 'auto',
        bgcolor: 'background.default'
      }}>
        {renderContent()}
      </Box>

      {/* Project Dialog */}
      <ProjectDialog
        open={projectDialogOpen}
        onClose={() => setProjectDialogOpen(false)}
        project={selectedProject}
        onSave={handleSaveProject}
      />
    </Box>
  );
};

export default TaskPlannerLayout;