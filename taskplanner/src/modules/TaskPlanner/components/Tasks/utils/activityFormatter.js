import { 
  ACTIVITY_TYPES, 
  FIELD_DISPLAY_NAMES,
  ACTIVITY_ICONS,
  ACTIVITY_COLORS 
} from '../constants/activityTypes';

// Format different types of values for display
export const formatValue = (value, fieldName) => {
  if (value === null || value === undefined) return 'None';
  
  switch (fieldName) {
    case 'DueDate':
    case 'StartDate':
      return new Date(value).toLocaleDateString();
    
    case 'ProgressPercentage':
      return `${value}%`;
    
    case 'EstimatedHours':
      return `${value} hours`;
    
    case 'IsPrivate':
      return value ? 'Private' : 'Public';
    
    case 'Priority':
      return value;
    
    case 'BucketGUID':
      // We'll need to resolve bucket name from GUID
      return value; // For now, will enhance later
    
    case 'ProjectGUID':
      // We'll need to resolve project name from GUID
      return value; // For now, will enhance later
    
    default:
      return String(value);
  }
};

// Generate human-readable activity descriptions
export const formatActivityDescription = (activity, projects = [], buckets = []) => {
  const { type, fieldName, oldValue, newValue, targetName, userName } = activity;

  switch (type) {
    case ACTIVITY_TYPES.TASK_CREATED:
      return 'Task created';

    case ACTIVITY_TYPES.TASK_UPDATED:
      return 'Task updated';

    case ACTIVITY_TYPES.TASK_DELETED:
      return 'Task deleted';

    case ACTIVITY_TYPES.FIELD_CHANGED:
      const displayName = FIELD_DISPLAY_NAMES[fieldName] || fieldName;
      const formattedOldValue = formatValue(oldValue, fieldName);
      const formattedNewValue = formatValue(newValue, fieldName);
      
      if (fieldName === 'BucketGUID') {
        const oldBucket = buckets.find(b => b.BucketGUID === oldValue);
        const newBucket = buckets.find(b => b.BucketGUID === newValue);
        return `Status changed from ${oldBucket?.BucketName || 'Unknown'} to ${newBucket?.BucketName || 'Unknown'}`;
      }
      
      if (fieldName === 'ProjectGUID') {
        const oldProject = projects.find(p => p.ProjectGUID === oldValue);
        const newProject = projects.find(p => p.ProjectGUID === newValue);
        return `Project changed from ${oldProject?.ProjectName || 'Unknown'} to ${newProject?.ProjectName || 'Unknown'}`;
      }
      
      return `${displayName} changed from ${formattedOldValue} to ${formattedNewValue}`;

    case ACTIVITY_TYPES.STATUS_CHANGED:
      const oldBucket = buckets.find(b => b.BucketGUID === oldValue);
      const newBucket = buckets.find(b => b.BucketGUID === newValue);
      return `Status changed from ${oldBucket?.BucketName || 'Unknown'} to ${newBucket?.BucketName || 'Unknown'}`;

    case ACTIVITY_TYPES.PRIORITY_CHANGED:
      return `Priority changed from ${oldValue} to ${newValue}`;

    case ACTIVITY_TYPES.PROGRESS_CHANGED:
      return `Progress updated from ${oldValue}% to ${newValue}%`;

    case ACTIVITY_TYPES.DUE_DATE_CHANGED:
      const formattedOldDue = oldValue ? new Date(oldValue).toLocaleDateString() : 'None';
      const formattedNewDue = newValue ? new Date(newValue).toLocaleDateString() : 'None';
      return `Due date changed from ${formattedOldDue} to ${formattedNewDue}`;

    case ACTIVITY_TYPES.START_DATE_CHANGED:
      const formattedOldStart = oldValue ? new Date(oldValue).toLocaleDateString() : 'None';
      const formattedNewStart = newValue ? new Date(newValue).toLocaleDateString() : 'None';
      return `Start date changed from ${formattedOldStart} to ${formattedNewStart}`;

    case ACTIVITY_TYPES.USER_ASSIGNED:
      return `${targetName ||  userName || 'User'} was assigned to this task`;

    case ACTIVITY_TYPES.USER_UNASSIGNED:
      return `${ targetName || userName || 'User'} was unassigned from this task`;

    case ACTIVITY_TYPES.COMMENT_ADDED:
      return `Comment added: "${newValue?.substring(0, 50)}${newValue?.length > 50 ? '...' : ''}"`;

    case ACTIVITY_TYPES.COMMENT_DELETED:
      return 'Comment deleted';

    case ACTIVITY_TYPES.CHECKLIST_ITEM_ADDED:
      return `Checklist item added: "${newValue?.substring(0, 50)}${newValue?.length > 50 ? '...' : ''}"`;

    case ACTIVITY_TYPES.CHECKLIST_ITEM_COMPLETED:
      return `Completed checklist item: "${targetName?.substring(0, 50)}${targetName?.length > 50 ? '...' : ''}"`;

    case ACTIVITY_TYPES.CHECKLIST_ITEM_UNCOMPLETED:
      return `Unchecked checklist item: "${targetName?.substring(0, 50)}${targetName?.length > 50 ? '...' : ''}"`;

    case ACTIVITY_TYPES.MADE_PRIVATE:
      return 'Task made private';

    case ACTIVITY_TYPES.MADE_PUBLIC:
      return 'Task made public';

    default:
      return 'Unknown activity';
  }
};

// Get activity icon name
export const getActivityIcon = (type) => {
  return ACTIVITY_ICONS[type] || 'Info';
};

// Get activity color
export const getActivityColor = (type) => {
  return ACTIVITY_COLORS[type] || 'default';
};

// Format relative time (e.g., "2 minutes ago")
export const formatRelativeTime = (timestamp) => {
  const now = new Date();
  const activityTime = new Date(timestamp);
  const diffInSeconds = Math.floor((now - activityTime) / 1000);

  if (diffInSeconds < 60) {
    return 'Just now';
  }

  const diffInMinutes = Math.floor(diffInSeconds / 60);
  if (diffInMinutes < 60) {
    return `${diffInMinutes} minute${diffInMinutes > 1 ? 's' : ''} ago`;
  }

  const diffInHours = Math.floor(diffInMinutes / 60);
  if (diffInHours < 24) {
    return `${diffInHours} hour${diffInHours > 1 ? 's' : ''} ago`;
  }

  const diffInDays = Math.floor(diffInHours / 24);
  if (diffInDays < 7) {
    return `${diffInDays} day${diffInDays > 1 ? 's' : ''} ago`;
  }

  // For older activities, show the actual date
  return activityTime.toLocaleDateString();
};

// Group activities by date for better organization
export const groupActivitiesByDate = (activities) => {
  const groups = {};
  
  activities.forEach(activity => {
    const date = new Date(activity.timestamp).toDateString();
    if (!groups[date]) {
      groups[date] = [];
    }
    groups[date].push(activity);
  });

  return groups;
};