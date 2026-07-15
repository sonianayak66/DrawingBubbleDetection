import React, { useState } from 'react';
import {
  Box,
  Typography,
  TextField,
  Button,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Avatar,
  Paper,
  Alert,
  CircularProgress,
  InputAdornment,
  IconButton,
} from '@mui/material';
import {
  Send,
  Comment as CommentIcon,
} from '@mui/icons-material';
import PermissionGuard from '../../../../shared/components/Common/PermissionGuard';

const TaskCommentsTab = ({
  comments,
  newComment,
  setNewComment,
  canEditRelatedData,
  onAddComment,
  loading,
}) => {
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState('');

  const handleAddComment = async () => {
    if (!newComment.trim()) {
      setError('Please enter a comment');
      return;
    }

    try {
      setActionLoading(true);
      setError('');
      await onAddComment();
    } catch (err) {
      setError(err.message || 'Failed to add comment');
    } finally {
      setActionLoading(false);
    }
  };

  const handleKeyPress = (event) => {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      handleAddComment();
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
      <Typography variant="h6" gutterBottom>
        Comments ({comments.length})
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>
          {error}
        </Alert>
      )}

      {!canEditRelatedData && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          Save the task first to add comments.
        </Alert>
      )}

      {/* Add Comment Section */}
      {canEditRelatedData && (
        <PermissionGuard permission="TaskPlanner_Tasks_Write">
          <Paper sx={{ p: 2, mb: 3 }}>
            <TextField
              fullWidth
              multiline
              rows={3}
              placeholder="Add a comment..."
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              onKeyPress={handleKeyPress}
              disabled={actionLoading}
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      onClick={handleAddComment}
                      disabled={!newComment.trim() || actionLoading}
                      color="primary"
                    >
                      {actionLoading ? <CircularProgress size={20} /> : <Send />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />
            <Box sx={{ mt: 1, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="caption" color="text.secondary">
                Press Enter to send, Shift+Enter for new line
              </Typography>
              <Button
                variant="contained"
                size="small"
                startIcon={actionLoading ? <CircularProgress size={16} /> : <Send />}
                onClick={handleAddComment}
                disabled={!newComment.trim() || actionLoading}
              >
                Add Comment
              </Button>
            </Box>
          </Paper>
        </PermissionGuard>
      )}

      {/* Comments List */}
      {comments.length === 0 ? (
        <Box sx={{ textAlign: 'center', py: 4, color: 'text.secondary' }}>
          <CommentIcon sx={{ fontSize: 48, mb: 1, opacity: 0.5 }} />
          <Typography variant="body2">
            No comments yet
          </Typography>
          {canEditRelatedData && (
            <Typography variant="caption">
              Be the first to add a comment!
            </Typography>
          )}
        </Box>
      ) : (
        <List sx={{ bgcolor: 'background.paper', borderRadius: 1 }}>
          {comments.map((comment, index) => (
            <ListItem
              key={comment.CommentGUID || index}
              alignItems="flex-start"
              divider={index < comments.length - 1}
              sx={{ py: 2 }}
            >
              <ListItemAvatar>
                <Avatar>
                  {comment.CreatedByName?.charAt(0).toUpperCase() || 'U'}
                </Avatar>
              </ListItemAvatar>
              <ListItemText
                primary={
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Typography variant="subtitle2">
                      {comment.CreatedByName || 'Unknown User'}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {comment.CreatedDate
                        ? new Date(comment.CreatedDate).toLocaleString()
                        : 'Unknown date'
                      }
                    </Typography>
                  </Box>
                }
                secondary={
                  <Typography
                    variant="body2"
                    sx={{ 
                      mt: 1, 
                      whiteSpace: 'pre-wrap', // Preserve line breaks
                      wordBreak: 'break-word'
                    }}
                  >
                    {comment.CommentText}
                  </Typography>
                }
              />
            </ListItem>
          ))}
        </List>
      )}
    </Box>
  );
};

export default TaskCommentsTab;
