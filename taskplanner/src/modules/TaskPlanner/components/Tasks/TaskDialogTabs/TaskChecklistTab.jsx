import React, { useState } from 'react';
import {
  Box,
  Typography,
  TextField,
  Button,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Checkbox,
  Paper,
  Alert,
  CircularProgress,
  InputAdornment,
  IconButton,
  LinearProgress,
} from '@mui/material';
import {
  Add as AddIcon,
  CheckBox as CheckBoxIcon,
} from '@mui/icons-material';
import PermissionGuard from '../../../../shared/components/Common/PermissionGuard';

const TaskChecklistTab = ({
  checklists,
  newChecklistItem,
  setNewChecklistItem,
  canEditRelatedData,
  onAddChecklistItem,
  onChecklistToggle,
  loading,
}) => {
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState('');

  // Calculate completion percentage
  const completedItems = checklists.filter(item => item.IsCompleted).length;
  const totalItems = checklists.length;
  const completionPercentage = totalItems > 0 ? (completedItems / totalItems) * 100 : 0;

  const handleAddItem = async () => {
    if (!newChecklistItem.trim()) {
      setError('Please enter a checklist item');
      return;
    }

    if (newChecklistItem.length > 500) {
      setError('Checklist item is too long (max 500 characters)');
      return;
    }

    try {
      setActionLoading(true);
      setError('');
      await onAddChecklistItem();
    } catch (err) {
      setError(err.message || 'Failed to add checklist item');
    } finally {
      setActionLoading(false);
    }
  };

  const handleToggleItem = async (item) => {
    try {
      setActionLoading(true);
      setError('');
      await onChecklistToggle(item);
    } catch (err) {
      setError(err.message || 'Failed to update checklist item');
    } finally {
      setActionLoading(false);
    }
  };

  const handleKeyPress = (event) => {
    if (event.key === 'Enter') {
      event.preventDefault();
      handleAddItem();
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
        <Typography variant="h6">
          Checklist ({completedItems}/{totalItems})
        </Typography>
        {totalItems > 0 && (
          <Typography variant="body2" color="text.secondary">
            {Math.round(completionPercentage)}% complete
          </Typography>
        )}
      </Box>

      {/* Progress bar */}
      {totalItems > 0 && (
        <Box sx={{ mb: 3 }}>
          <LinearProgress 
            variant="determinate" 
            value={completionPercentage} 
            sx={{ height: 8, borderRadius: 4 }}
          />
        </Box>
      )}

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>
          {error}
        </Alert>
      )}

      {!canEditRelatedData && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          Save the task first to manage checklist items.
        </Alert>
      )}

      {/* Add Checklist Item Section */}
      {canEditRelatedData && (
        <PermissionGuard permission="TaskPlanner_Tasks_Write">
          <Paper sx={{ p: 2, mb: 3 }}>
            <TextField
              fullWidth
              placeholder="Add a checklist item..."
              value={newChecklistItem}
              onChange={(e) => setNewChecklistItem(e.target.value)}
              onKeyPress={handleKeyPress}
              disabled={actionLoading}
              inputProps={{ maxLength: 500 }}
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      onClick={handleAddItem}
                      disabled={!newChecklistItem.trim() || actionLoading}
                      color="primary"
                    >
                      {actionLoading ? <CircularProgress size={20} /> : <AddIcon />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />
            <Box sx={{ mt: 1, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="caption" color="text.secondary">
                {newChecklistItem.length}/500 characters
              </Typography>
              <Button
                variant="contained"
                size="small"
                startIcon={actionLoading ? <CircularProgress size={16} /> : <AddIcon />}
                onClick={handleAddItem}
                disabled={!newChecklistItem.trim() || actionLoading}
              >
                Add Item
              </Button>
            </Box>
          </Paper>
        </PermissionGuard>
      )}

      {/* Checklist Items */}
      {checklists.length === 0 ? (
        <Box sx={{ textAlign: 'center', py: 4, color: 'text.secondary' }}>
          <CheckBoxIcon sx={{ fontSize: 48, mb: 1, opacity: 0.5 }} />
          <Typography variant="body2">
            No checklist items yet
          </Typography>
          {canEditRelatedData && (
            <Typography variant="caption">
              Add items to break down this task!
            </Typography>
          )}
        </Box>
      ) : (
        <List sx={{ bgcolor: 'background.paper', borderRadius: 1 }}>
          {checklists
            .sort((a, b) => a.SortOrder - b.SortOrder)
            .map((item, index) => (
              <ListItem
                key={item.ChecklistGUID || index}
                dense
                divider={index < checklists.length - 1}
                sx={{
                  opacity: item.IsCompleted ? 0.7 : 1,
                  transition: 'opacity 0.2s',
                }}
              >
                <ListItemIcon>
                  <Checkbox
                    checked={item.IsCompleted || false}
                    onChange={() => handleToggleItem(item)}
                    disabled={!canEditRelatedData || actionLoading}
                    color="primary"
                  />
                </ListItemIcon>
                <ListItemText
                  primary={
                    <Typography
                      sx={{
                        textDecoration: item.IsCompleted ? 'line-through' : 'none',
                        color: item.IsCompleted ? 'text.secondary' : 'text.primary',
                        wordBreak: 'break-word',
                      }}
                    >
                      {item.ItemText}
                    </Typography>
                  }
                  secondary={
                    item.CompletedDate && (
                      <Typography variant="caption" color="text.secondary">
                        Completed on {new Date(item.CompletedDate).toLocaleDateString()}
                      </Typography>
                    )
                  }
                />
              </ListItem>
            ))}
        </List>
      )}
    </Box>
  );
};

export default TaskChecklistTab;
