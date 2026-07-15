import React from 'react';
import {
  Box,
  Typography,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Avatar,
  Chip,
  Divider,
  CircularProgress,
  Alert,
  Paper,
} from '@mui/material';
import {
  Timeline,
  TimelineItem,
  TimelineSeparator,
  TimelineConnector,
  TimelineContent,
  TimelineDot,
} from '@mui/lab';
import {
  Add,
  Edit,
  Delete,
  SwapHoriz,
  PriorityHigh,
  TrendingUp,
  Schedule,
  PersonAdd,
  PersonRemove,
  Comment,
  CommentBank,
  PlaylistAdd,
  CheckCircle,
  RadioButtonUnchecked,
  Lock,
  Public,
  Info,
} from '@mui/icons-material';

import { 
  formatActivityDescription,
  getActivityIcon,
  getActivityColor,
  formatRelativeTime,
  groupActivitiesByDate 
} from '../utils/activityFormatter';

// Icon mapping for Material-UI icons
const ICON_COMPONENTS = {
  Add,
  Edit,
  Delete,
  SwapHoriz,
  PriorityHigh,
  TrendingUp,
  Schedule,
  PersonAdd,
  PersonRemove,
  Comment,
  CommentBank,
  PlaylistAdd,
  CheckCircle,
  RadioButtonUnchecked,
  Lock,
  Public,
  Info,
};

const TaskActivityTab = ({ 
  activities, 
  loading, 
  error,
  projects = [],
  buckets = [] 
}) => {
  if (loading) {
    return (
      <Box sx={{ p: 3, display: 'flex', justifyContent: 'center' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="error">
          {error}
        </Alert>
      </Box>
    );
  }

  if (activities.length === 0) {
    return (
      <Box sx={{ p: 3, textAlign: 'center', color: 'text.secondary' }}>
        <Timeline sx={{ fontSize: 48, mb: 2, opacity: 0.5 }} />
        <Typography variant="h6" gutterBottom>
          No Activity Yet
        </Typography>
        <Typography variant="body2">
          Activity will appear here as you work on this task
        </Typography>
      </Box>
    );
  }

  // Group activities by date for better organization
  const groupedActivities = groupActivitiesByDate(activities);
  const dateGroups = Object.keys(groupedActivities).sort((a, b) => new Date(b) - new Date(a));

  const getIconComponent = (iconName) => {
    return ICON_COMPONENTS[iconName] || Info;
  };

  const formatDateHeader = (dateString) => {
    const date = new Date(dateString);
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);

    if (date.toDateString() === today.toDateString()) {
      return 'Today';
    } else if (date.toDateString() === yesterday.toDateString()) {
      return 'Yesterday';
    } else {
      return date.toLocaleDateString('en-US', { 
        weekday: 'long', 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric' 
      });
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        Activity Timeline ({activities.length})
      </Typography>

      <Timeline 
  position="right"
  sx={{
    width: '100%',
    '& .MuiTimelineItem-root': {
      '&:before': {
        flex: 0,
        padding: 0,
      },
    },
  }}
>
        {dateGroups.map((dateString, dateIndex) => (
          <Box key={dateString}>
            {/* Date Header */}
            <Box sx={{ mb: 2, mt: dateIndex > 0 ? 3 : 0 }}>
              <Divider>
                <Chip 
                  label={formatDateHeader(dateString)} 
                  size="small" 
                  color="primary"
                  variant="outlined"
                />
              </Divider>
            </Box>

            {/* Activities for this date */}
            {groupedActivities[dateString].map((activity, activityIndex) => {
              const IconComponent = getIconComponent(getActivityIcon(activity.type));
             const activityColor = getActivityColor(activity.type) || 'default';
              const description = formatActivityDescription(activity, projects, buckets);
              const relativeTime = formatRelativeTime(activity.timestamp);
              const isLastInGroup = activityIndex === groupedActivities[dateString].length - 1;
              const isLastOverall = dateIndex === dateGroups.length - 1 && isLastInGroup;

              return (
                <TimelineItem key={activity.id}>
                  <TimelineSeparator>
                    <TimelineDot 
                      color={activityColor}
                      sx={{
                        opacity: activity.pending ? 0.6 : 1,
                        border: activity.failed ? '2px solid red' : 'none',
                      }}
                    >
                      <IconComponent fontSize="small" />
                    </TimelineDot>
                    {!isLastOverall && <TimelineConnector />}
                  </TimelineSeparator>
                  
                  <TimelineContent>
                    <Paper 
                      elevation={activity.pending ? 0 : 1} 
                      sx={{ 
                        p: 1, 
                        mb: 0.2,
                        opacity: activity.pending ? 0.7 : 1,
                        border: activity.pending ? '1px dashed' : 'none',
                        borderColor: 'divider',
                      }}
                    >
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                        <Box sx={{ flex: 1 }}>
                          <Typography variant="body2" sx={{ fontWeight: 500 }}>
                            {description}
                          </Typography>
                          
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 1 }}>
                            <Avatar sx={{ width: 20, height: 20, fontSize: '0.75rem' }}>
                              {activity.userName?.charAt(0).toUpperCase() || 'U'}
                            </Avatar>
                            <Typography variant="caption" color="text.secondary">
                              {activity.userName || 'Unknown User'}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              •
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {relativeTime}
                            </Typography>
                            {activity.pending && (
                              <>
                                <Typography variant="caption" color="text.secondary">
                                  •
                                </Typography>
                                <Chip 
                                  label="Syncing..." 
                                  size="small" 
                                  variant="outlined" 
                                  color="info"
                                  sx={{ height: 16, '& .MuiChip-label': { px: 1, fontSize: '0.625rem' } }}
                                />
                              </>
                            )}
                            {activity.failed && (
                              <>
                                <Typography variant="caption" color="text.secondary">
                                  •
                                </Typography>
                                <Chip 
                                  label="Failed to sync" 
                                  size="small" 
                                  variant="outlined" 
                                  color="error"
                                  sx={{ height: 16, '& .MuiChip-label': { px: 1, fontSize: '0.625rem' } }}
                                />
                              </>
                            )}
                          </Box>
                        </Box>
                      </Box>
                    </Paper>
                  </TimelineContent>
                </TimelineItem>
              );
            })}
          </Box>
        ))}
      </Timeline>



      {activities.length > 0 && (
        <Box sx={{ mt: 3, p: 2, bgcolor: 'background.paper', borderRadius: 1, textAlign: 'center' }}>
          <Typography variant="caption" color="text.secondary">
            {activities.filter(a => !a.pending).length} of {activities.length} activities synced
          </Typography>
        </Box>
      )}
    </Box>
  );
};

export default TaskActivityTab;