import { useState, useEffect, useRef } from 'react';
import { taskPlannerApi } from '../../../../../services/api';
import { ACTIVITY_TYPES } from '../constants/activityTypes';
import { formatActivityDescription } from '../utils/activityFormatter';
import { useUser } from '../../../../../context/UserContext'; // Add this impor

// Generate unique temporary IDs for optimistic updates
const generateTempId = () => `temp_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;

 


// Generate better descriptions for activities
const generateActivityDescription = (type, details, projects = [], buckets = []) => {
  // Import the formatter if needed
  try {
    const { formatActivityDescription } = require('../utils/activityFormatter');
    return formatActivityDescription({ type, ...details }, projects, buckets);
  } catch (error) {
    // Fallback to simple descriptions
    switch (type) {
      case ACTIVITY_TYPES.TASK_CREATED:
        return 'Task created';
      case ACTIVITY_TYPES.FIELD_CHANGED:
        return `${details.fieldName} changed`;
      case ACTIVITY_TYPES.USER_ASSIGNED:
        return `${details.userName} was assigned`;
      case ACTIVITY_TYPES.COMMENT_ADDED:
        return 'Comment added';
      default:
        return 'Activity occurred';
    }
  }
};

export const useTaskActivity = (taskGuid, open) => {
  const [activities, setActivities] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
   const { getCurrentUserId, getCurrentUserName } = useUser();


     // Get current user info from context (moved inside hook)
  const getCurrentUser = () => {
    return {
      id: getCurrentUserId(),
      name: getCurrentUserName(),
    };
  };

  // Refs to track previous values for change detection
  const previousTaskDataRef = useRef(null);

  // Load activities when dialog opens
  useEffect(() => {
    if (open && taskGuid) {
      loadActivities();
    } else if (open && !taskGuid) {
      // Reset for new task
      setActivities([]);
      setError('');
    }
  }, [taskGuid, open]);

  const loadActivities = async () => {
  if (!taskGuid) return;

  try {
    setLoading(true);
    setError('');
    
    const response = await taskPlannerApi.getTaskActivities(taskGuid);
    const activitiesData = response.data || [];
    
    // Transform backend data to frontend format
    const transformedActivities = activitiesData.map(activity => ({
      id: activity.ActivityGUID,
      taskGuid: activity.TaskGUID,
      type: activity.ActivityType,
      timestamp: activity.CreatedDate,
      userId: activity.CreatedBy,
      userName: activity.CreatedByName,
      fieldName: activity.FieldName,
      oldValue: activity.OldValue,
      newValue: activity.NewValue,
      targetName: activity.TargetName,
      description: activity.Description,
      pending: false, // Loaded from backend, so not pending
    }));
    
    setActivities(transformedActivities);
  } catch (err) {
    console.error('Error loading activities:', err);
    setError('Failed to load activities');
  } finally {
    setLoading(false);
  }
};

  // Create a new activity entry
  const createActivity = (type, details = {}) => {
    const currentUser = getCurrentUser();
    const activity = {
      id: generateTempId(),
      taskGuid: taskGuid,
      type: type,
      timestamp: new Date().toISOString(),
      userId: currentUser.id,
      userName: currentUser.name,
      pending: true, // Mark as pending until synced with backend
      ...details,
    };

    return activity;
  };

  // Add activity to local state immediately (optimistic update)
  const addActivity = (activity) => {
    setActivities(prev => [activity, ...prev]); // Add to beginning for chronological order
  };

 const syncActivityToBackend = async (activity) => {
  try {
    // Prepare data for backend
    const activityData = {
      ActivityGUID: null, // Let backend generate
      TaskGUID: activity.taskGuid,
      ActivityType: activity.type,
      FieldName: activity.fieldName || null,
      OldValue: activity.oldValue || null,
      NewValue: activity.newValue || null,
      Description: activity.description || null,
      TargetName: activity.targetName || null,
      CreatedBy: getCurrentUserId(), // Use real user ID from context
    };
    
    const response = await taskPlannerApi.saveTaskActivity(activityData);
    const savedActivityGuid = response.data?.ActivityGUID;
    
    // Update local activity with backend ID and mark as synced
    setActivities(prev => 
      prev.map(a => 
        a.id === activity.id 
          ? { 
              ...a, 
              id: savedActivityGuid || a.id, 
              pending: false,
              failed: false
            }
          : a
      )
    );
    
  } catch (err) {
    console.error('Error syncing activity:', err);
    // Mark activity as failed but keep it in the list
    setActivities(prev => 
      prev.map(a => 
        a.id === activity.id 
          ? { ...a, pending: false, failed: true }
          : a
      )
    );
  }
};

  // Main function to log any activity
  const logActivity = (type, details = {}) => {
    const activity = createActivity(type, details);
    addActivity(activity);
    
    // Sync to backend if we have a task GUID
    if (taskGuid) {
      syncActivityToBackend(activity);
    }
    
    return activity;
  };

  // Track task creation
  const trackTaskCreation = (taskData) => {
    return logActivity(ACTIVITY_TYPES.TASK_CREATED, {
      newValue: taskData.TaskTitle,
    });
  };

  // Track field changes by comparing old and new task data
  const trackTaskChanges = (oldTaskData, newTaskData) => {
    if (!oldTaskData || !newTaskData) return;

    const fieldsToTrack = [
      'TaskTitle',
      'TaskDescription', 
      'Priority',
      'BucketGUID',
      'ProjectGUID',
      'DueDate',
      'StartDate',
      'ProgressPercentage',
      'EstimatedHours',
      'Tags',
      'IsPrivate'
    ];

    const activities = [];

    fieldsToTrack.forEach(fieldName => {
      const oldValue = oldTaskData[fieldName];
      const newValue = newTaskData[fieldName];

      // Compare values (handle dates and null values)
     const normalizeValue = (val) => {
        if (val === null || val === undefined) return null;
        if (val instanceof Date) return val.getTime();
        if (typeof val === 'string' && !isNaN(Date.parse(val))) {
          // Handle date strings
          return new Date(val).getTime();
        }
        return val;
      };

const normalizedOld = normalizeValue(oldValue);
const normalizedNew = normalizeValue(newValue);

// Only track if there's an actual change
if (normalizedOld !== normalizedNew) {
  // Additional check for date fields to ensure meaningful change
  if ((fieldName === 'DueDate' || fieldName === 'StartDate') && normalizedOld && normalizedNew) {
    // For dates, ignore changes less than 1 minute (to handle timezone/formatting issues)
    const timeDifference = Math.abs(normalizedOld - normalizedNew);
    if (timeDifference < 60000) { // 60000 ms = 1 minute
      return; // Skip this change as it's not meaningful
    }
  }
}

      if (normalizeValue(oldValue) !== normalizeValue(newValue)) {
        let activityType = ACTIVITY_TYPES.FIELD_CHANGED;
        
        // Use specific activity types for certain fields
        if (fieldName === 'BucketGUID') {
          activityType = ACTIVITY_TYPES.STATUS_CHANGED;
        } else if (fieldName === 'Priority') {
          activityType = ACTIVITY_TYPES.PRIORITY_CHANGED;
        } else if (fieldName === 'ProgressPercentage') {
          activityType = ACTIVITY_TYPES.PROGRESS_CHANGED;
        } else if (fieldName === 'DueDate') {
          activityType = ACTIVITY_TYPES.DUE_DATE_CHANGED;
        } else if (fieldName === 'StartDate') {
          activityType = ACTIVITY_TYPES.START_DATE_CHANGED;
        } else if (fieldName === 'IsPrivate') {
          activityType = newValue ? ACTIVITY_TYPES.MADE_PRIVATE : ACTIVITY_TYPES.MADE_PUBLIC;
        }

        const activity = logActivity(activityType, {
          fieldName,
          oldValue,
          newValue,
        });
        
        activities.push(activity);
      }
    });

    return activities;
  };

  // Track assignment changes
  const trackUserAssignment = (userName) => {
    return logActivity(ACTIVITY_TYPES.USER_ASSIGNED, {
      userName,
      targetName: userName,
    });
  };

  const trackUserUnassignment = (userName) => {
    return logActivity(ACTIVITY_TYPES.USER_UNASSIGNED, {
      userName,
      targetName: userName,
    });
  };

  // Track comment activities
  const trackCommentAdded = (commentText) => {
    return logActivity(ACTIVITY_TYPES.COMMENT_ADDED, {
      newValue: commentText,
    });
  };

  // Track checklist activities
  const trackChecklistItemAdded = (itemText) => {
    return logActivity(ACTIVITY_TYPES.CHECKLIST_ITEM_ADDED, {
      newValue: itemText,
    });
  };

  const trackChecklistItemCompleted = (itemText) => {
    return logActivity(ACTIVITY_TYPES.CHECKLIST_ITEM_COMPLETED, {
      targetName: itemText,
    });
  };

  const trackChecklistItemUncompleted = (itemText) => {
    return logActivity(ACTIVITY_TYPES.CHECKLIST_ITEM_UNCOMPLETED, {
      targetName: itemText,
    });
  };

  // Update previous task data reference
  const updatePreviousTaskData = (taskData) => {
    previousTaskDataRef.current = taskData;
  };

  return {
    // State
    activities,
    loading,
    error,
    
    // Functions
    loadActivities,
    logActivity,
    
    // Specific tracking functions
    trackTaskCreation,
    trackTaskChanges,
    trackUserAssignment,
    trackUserUnassignment,
    trackCommentAdded,
    trackChecklistItemAdded,
    trackChecklistItemCompleted,
    trackChecklistItemUncompleted,
    
    // Utility
    updatePreviousTaskData,
    previousTaskData: previousTaskDataRef.current,
  };
};
