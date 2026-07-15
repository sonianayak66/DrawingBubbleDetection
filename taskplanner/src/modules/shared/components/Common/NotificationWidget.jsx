import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
} from '@mui/material';
import {
  Notifications,
  Email,
  Task,
  CheckCircle,
  MarkEmailRead,
  Visibility,
} from '@mui/icons-material';

const NotificationList = () => {
  const [notifications, setNotifications] = useState([]);
  const [summary, setSummary] = useState({
    TotalNotifications: 0,
    UnreadCount: 0,
    RelatedEmailCount: 0,
    NewEmailCount: 0,
  });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadNotificationSummary();
    loadNotifications();
  }, []);

  const loadNotificationSummary = async () => {
    try {
      const response = await fetch('/api/taskplanner/notifications/summary', {
        credentials: 'include'
      });
      
      if (response.ok) {
        const data = await response.json();
        setSummary(data);
      }
    } catch (error) {
      console.error('Error loading notification summary:', error);
    }
  };

  const loadNotifications = async () => {
    try {
      setLoading(true);
      const response = await fetch('/api/taskplanner/notifications?pageSize=50', {
        credentials: 'include'
      });
      
      if (response.ok) {
        const data = await response.json();
        setNotifications(data.data || []);
      }
    } catch (error) {
      console.error('Error loading notifications:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleViewTask = (taskGuid) => {
  // Navigate to task details - you can implement this based on your routing
  console.log('Navigate to task:', taskGuid);
  // For example: window.open(`/taskplanner/tasks/${taskGuid}`, '_blank');
};

const handleViewEmail = (notification) => {
  // Switch to email view
  console.log('View email:', notification.EmailGUID);
};

  const markAsRead = async (notificationGuid) => {
    try {
      await fetch('/api/taskplanner/notifications/mark-read', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ NotificationGUID: notificationGuid })
      });
      
      // Refresh notifications and summary
      loadNotifications();
      loadNotificationSummary();
    } catch (error) {
      console.error('Error marking notification as read:', error);
    }
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffInHours = (now - date) / (1000 * 60 * 60);
    
    if (diffInHours < 1) {
      return 'Just now';
    } else if (diffInHours < 24) {
      return `${Math.floor(diffInHours)}h ago`;
    } else {
      return date.toLocaleDateString();
    }
  };

  return (
    <Box>
      {/* Notification Summary */}
      <Box sx={{ mb: 3, p: 2, bgcolor: 'background.paper', borderRadius: 1, border: '1px solid', borderColor: 'divider' }}>
        <Typography variant="h6" gutterBottom>
          Notification Summary
        </Typography>
        <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
          <Chip 
            icon={<Notifications />}
            label={`${summary.TotalNotifications} Total`}
            color="default"
          />
          <Chip 
            icon={<CheckCircle />}
            label={`${summary.UnreadCount} Unread`}
            color="error"
          />
          <Chip 
            icon={<Task />}
            label={`${summary.RelatedEmailCount} Related`}
            color="primary"
          />
          <Chip 
            icon={<Email />}
            label={`${summary.NewEmailCount} New`}
            color="warning"
          />
        </Box>
      </Box>

      {/* Notifications List */}
      {loading ? (
        <Typography>Loading notifications...</Typography>
      ) : notifications.length === 0 ? (
        <Box sx={{ textAlign: 'center', py: 8 }}>
          <Notifications sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
          <Typography variant="h6" color="text.secondary" gutterBottom>
            No notifications found
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Email notifications will appear here when new emails arrive
          </Typography>
        </Box>
      ) : (
        <TableContainer component={Paper} elevation={0} sx={{ border: '1px solid', borderColor: 'divider' }}>
          <Table>
            <TableHead>
              <TableRow sx={{ bgcolor: 'action.hover' }}>
                <TableCell sx={{ fontWeight: 600 }}>Type</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Email</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Message</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Date</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 120 }}>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {notifications.map((notification) => (
                <TableRow 
                  key={notification.NotificationGUID} 
                  hover
                  sx={{ 
                    bgcolor: notification.IsRead ? 'transparent' : 'action.hover',
                    opacity: notification.IsRead ? 0.7 : 1
                  }}
                >
                  <TableCell>
                    <Chip
                      icon={notification.NotificationType === 'RelatedEmail' ? <Task /> : <Email />}
                      label={notification.NotificationType === 'RelatedEmail' ? 'Related' : 'New'}
                      color={notification.NotificationType === 'RelatedEmail' ? 'primary' : 'warning'}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>
                    <Box>
                      <Typography 
                        variant="body2" 
                        sx={{ 
                          fontWeight: notification.IsRead ? 'normal' : 'bold',
                          mb: 0.5
                        }}
                      >
                        {notification.EmailSubject}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {notification.FromEmail}
                      </Typography>
                    </Box>
                  </TableCell>
                  <TableCell>
  <Box>
    <Typography variant="body2" sx={{ mb: 1 }}>
      {notification.Message}
    </Typography>
    {notification.RelatedTaskTitle && (
      <Chip
        icon={<Task />}
        label={`Task: ${notification.RelatedTaskTitle}`}
        size="small"
        color="primary"
        variant="outlined"
        sx={{ fontSize: '0.75rem' }}
      />
    )}
  </Box>
</TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {formatDate(notification.CreatedDate)}
                    </Typography>
                  </TableCell>
                <TableCell>
  <Box sx={{ display: 'flex', gap: 1 }}>
    {!notification.IsRead && (
      <IconButton
        size="small"
        onClick={() => markAsRead(notification.NotificationGUID)}
        title="Mark as read"
      >
        <MarkEmailRead />
      </IconButton>
    )}
    {notification.RelatedTaskGUID && (
      <IconButton
        size="small"
        onClick={() => handleViewTask(notification.RelatedTaskGUID)}
        title={`View task: ${notification.RelatedTaskTitle}`}
        color="primary"
      >
        <Task />
      </IconButton>
    )}
    <IconButton
      size="small"
      onClick={() => handleViewEmail(notification)}
      title="View email"
    >
      <Visibility />
    </IconButton>
  </Box>
</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
};

export default NotificationList;