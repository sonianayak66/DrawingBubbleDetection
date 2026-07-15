import React from 'react';
import { BrowserRouter as Router } from 'react-router-dom';
import { CustomThemeProvider } from './context/ThemeContext';
import { PermissionsProvider, usePermissions } from './context/PermissionsContext';
import { UserProvider, useUser } from './context/UserContext';
import { Box, Typography, CircularProgress } from '@mui/material';
import AppShell from './core/AppShell';

const TaskPlannerApp = () => {
  const { loading: permissionsLoading, error: permissionsError } = usePermissions();
  const { loading: userLoading, error: userError, user } = useUser();

  // Show loading if either user or permissions are loading
  const isLoading = permissionsLoading || userLoading;
  const hasError = permissionsError || userError;

  if (isLoading) {
    return (
      <Box sx={{ 
        display: 'flex', 
        flexDirection: 'column',
        justifyContent: 'center', 
        alignItems: 'center', 
        height: '100vh',
        gap: 2
      }}>
        <CircularProgress />
        <Typography>Loading user information...</Typography>
      </Box>
    );
  }

  if (hasError) {
    return (
      <Box sx={{ 
        display: 'flex', 
        flexDirection: 'column',
        justifyContent: 'center', 
        alignItems: 'center', 
        height: '100vh',
        gap: 2,
        p: 3,
        textAlign: 'center'
      }}>
        <Typography color="error" variant="h6">
          Authentication Error
        </Typography>
        <Typography color="error">
          {permissionsError || userError}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Please refresh the page or contact support if the issue persists.
        </Typography>
      </Box>
    );
  }

  if (!user) {
    return (
      <Box sx={{ 
        display: 'flex', 
        justifyContent: 'center', 
        alignItems: 'center', 
        height: '100vh'
      }}>
        <Typography color="warning.main">
          No user information available
        </Typography>
      </Box>
    );
  }

  // Set basename to match vite.config.js base path
  return (
    <Router basename="/v3modules">
      <AppShell />
    </Router>
  );
};

function App() {
  return (
    <CustomThemeProvider>
      <UserProvider>
        <PermissionsProvider>
          <TaskPlannerApp />
        </PermissionsProvider>
      </UserProvider>
    </CustomThemeProvider>
  );
}

export default App;