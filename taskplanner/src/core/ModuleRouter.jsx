import React, { useState, useEffect } from 'react';
import { Routes, Route, Navigate, useNavigate, useLocation } from 'react-router-dom';
import { Box } from '@mui/material';
import { TaskPlannerRouter, TaskPlannerSidebar } from '../modules/TaskPlanner';
import { IONRouter } from '../modules/ION';
import { InwardIONRouter } from '../modules/InwardION';
import { taskPlannerApi } from '../services/api';

const ModuleRouter = ({ activeModule = 'taskplanner' }) => {
  const [projects, setProjects] = useState([]);
  const [selectedView, setSelectedView] = useState('my-day');
  const [viewContext, setViewContext] = useState({});
  const navigate = useNavigate();
  const location = useLocation();

  // Reset view when module changes
  useEffect(() => {
    switch (activeModule) {
      case 'taskplanner':
        if (selectedView.startsWith('ion-')) {
          setSelectedView('my-day');
        }
        loadProjects();
        break;
      case 'ion':
        if (!selectedView.startsWith('ion-')) {
          setSelectedView('ion-list');
        }
        break;
      case 'inward-ion':
        if (!selectedView.startsWith('inward-ion-')) {
          setSelectedView('inward-ion-list');
        }
        break;
      default:
        setSelectedView('my-day');
    }
    setViewContext({});
  }, [activeModule]);

  const loadProjects = async () => {
    try {
      const response = await taskPlannerApi.getProjects();
      setProjects(response.data || []);
    } catch (err) {
      console.error('Error loading projects:', err);
    }
  };

  const handleViewChange = (viewId, context = {}) => {
    setSelectedView(viewId);
    setViewContext(context);
    
    // Update URL for specific views if needed
    // This is optional - you can add sub-routing here later if desired
  };

  const renderSidebar = () => {
    switch (activeModule) {
      case 'taskplanner':
        return (
          <TaskPlannerSidebar 
            selectedView={selectedView}
            onViewChange={handleViewChange}
            projects={projects} 
          />
        );
      case 'ion':
        return null;
      case 'inward-ion':
        return null;
      default:
        return null;
    }
  };

  const renderContent = () => {
    switch (activeModule) {
      case 'taskplanner':
        return (
          <TaskPlannerRouter 
            selectedView={selectedView}
            onViewChange={handleViewChange}
            viewContext={viewContext}
          />
        );
      case 'ion':
        return (
          <IONRouter
            selectedView={selectedView}
            onViewChange={handleViewChange}
            viewContext={viewContext}
          />
        );
      case 'inward-ion':
        return (
          <InwardIONRouter
            selectedView={selectedView}
            onViewChange={handleViewChange}
            viewContext={viewContext}
          />
        );
      default:
        return (
          <TaskPlannerRouter 
            selectedView={selectedView}
            onViewChange={handleViewChange}
            viewContext={viewContext}
          />
        );
    }
  };

  return (
    <Box sx={{ display: 'flex', height: '100vh' }}>
      {renderSidebar()}
      {renderContent()}
    </Box>
  );
};

export default ModuleRouter;