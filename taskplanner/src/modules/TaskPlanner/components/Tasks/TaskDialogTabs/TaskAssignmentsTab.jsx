import React, { useState } from 'react';
import {
  Box,
  Typography,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  ListItemSecondaryAction,
  Avatar,
  IconButton,
  Button,
  Alert,
  CircularProgress,
} from '@mui/material';
import {
  Person,
  Delete,
  Add as AddIcon,
} from '@mui/icons-material';
import UserSelector from '../../../../shared/components/Common/UserSelector';
import PermissionGuard from '../../../../shared/components/Common/PermissionGuard';

const TaskAssignmentsTab = ({
  assignments,
  isPrivate,
  canEditRelatedData,
  onAddAssignment,
  onRemoveAssignment,
  loading,
}) => {
  const [userSelectorOpen, setUserSelectorOpen] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState('');

  const handleAddAssignment = async (user) => {
    try {
      setActionLoading(true);
      setError('');
      await onAddAssignment(user);
      setUserSelectorOpen(false);
    } catch (err) {
      setError(err.message || 'Failed to add assignment');
    } finally {
      setActionLoading(false);
    }
  };

  const handleRemoveAssignment = async (assignment) => {
    try {
      setActionLoading(true);
      setError('');
      await onRemoveAssignment(assignment);
    } catch (err) {
      setError(err.message || 'Failed to remove assignment');
    } finally {
      setActionLoading(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ p: 3, display: 'flex', justifyContent: 'center' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h6">Assignments</Typography>
        {canEditRelatedData && !isPrivate && (
          <PermissionGuard permission="TaskPlanner_Tasks_Write">
            <Button
              startIcon={<AddIcon />}
              onClick={() => setUserSelectorOpen(true)}
              disabled={actionLoading}
            >
              Assign User
            </Button>
          </PermissionGuard>
        )}
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>
          {error}
        </Alert>
      )}

      {isPrivate && (
        <Alert severity="info" sx={{ mb: 2 }}>
          Private tasks cannot be assigned to other users.
        </Alert>
      )}

      {!canEditRelatedData && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          Save the task first to manage assignments.
        </Alert>
      )}

      {assignments.length === 0 ? (
        <Box sx={{ textAlign: 'center', py: 4, color: 'text.secondary' }}>
          <Person sx={{ fontSize: 48, mb: 1, opacity: 0.5 }} />
          <Typography variant="body2">
            No users assigned to this task
          </Typography>
        </Box>
      ) : (
        <List>
          {assignments.map((assignment) => (
            <ListItem key={assignment.AssignmentId} divider>
              <ListItemAvatar>
                <Avatar>
                  {assignment.AssignedUserName?.charAt(0).toUpperCase() || 'U'}
                </Avatar>
              </ListItemAvatar>
              <ListItemText
                primary={assignment.AssignedUserName || 'Unknown User'}
                secondary={
                  assignment.AssignedDate
                    ? `Assigned on ${new Date(assignment.AssignedDate).toLocaleDateString()}`
                    : 'Assignment date unknown'
                }
              />
              
              {canEditRelatedData && !isPrivate && (
                <ListItemSecondaryAction>
                  <PermissionGuard permission="TaskPlanner_Tasks_Write">
                    <IconButton
                      edge="end"
                      onClick={() => handleRemoveAssignment(assignment)}
                      disabled={actionLoading}
                    >
                      <Delete />
                    </IconButton>
                  </PermissionGuard>
                </ListItemSecondaryAction>
              )}
            </ListItem>
          ))}
        </List>
      )}

   
      {/* User Selector Dialog */}
       {canEditRelatedData && !isPrivate && (
        <UserSelector
          open={userSelectorOpen}
          onClose={() => setUserSelectorOpen(false)}
          onUserSelect={handleAddAssignment}
          excludeUserIds={assignments.map(a => a.AssignedUserDbkey)}
          loading={actionLoading}
        />
       )}
    </Box>
  );
};

export default TaskAssignmentsTab;
