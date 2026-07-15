// Activity type constants
export const ACTIVITY_TYPES = {
  // Task lifecycle
  TASK_CREATED: 'task_created',
  TASK_UPDATED: 'task_updated',
  TASK_DELETED: 'task_deleted',
  
  // Field changes
  FIELD_CHANGED: 'field_changed',
  
  // Status and priority
  STATUS_CHANGED: 'status_changed',
  PRIORITY_CHANGED: 'priority_changed',
  
  // Progress
  PROGRESS_CHANGED: 'progress_changed',
  
  // Dates
  DUE_DATE_CHANGED: 'due_date_changed',
  START_DATE_CHANGED: 'start_date_changed',
  
  // Assignments
  USER_ASSIGNED: 'user_assigned',
  USER_UNASSIGNED: 'user_unassigned',
  
  // Comments
  COMMENT_ADDED: 'comment_added',
  COMMENT_DELETED: 'comment_deleted',
  
  // Checklist
  CHECKLIST_ITEM_ADDED: 'checklist_item_added',
  CHECKLIST_ITEM_COMPLETED: 'checklist_item_completed',
  CHECKLIST_ITEM_UNCOMPLETED: 'checklist_item_uncompleted',
  
  // Privacy
  MADE_PRIVATE: 'made_private',
  MADE_PUBLIC: 'made_public',
};

// Field display names for better UX
export const FIELD_DISPLAY_NAMES = {
  TaskTitle: 'Title',
  TaskDescription: 'Description',
  Priority: 'Priority',
  BucketGUID: 'Status',
  ProjectGUID: 'Project',
  DueDate: 'Due Date',
  StartDate: 'Start Date',
  ProgressPercentage: 'Progress',
  EstimatedHours: 'Estimated Hours',
  Tags: 'Tags',
  IsPrivate: 'Privacy',
};

// Activity icons mapping
export const ACTIVITY_ICONS = {
  [ACTIVITY_TYPES.TASK_CREATED]: 'Add',
  [ACTIVITY_TYPES.TASK_UPDATED]: 'Edit',
  [ACTIVITY_TYPES.TASK_DELETED]: 'Delete',
  [ACTIVITY_TYPES.FIELD_CHANGED]: 'Edit',
  [ACTIVITY_TYPES.STATUS_CHANGED]: 'SwapHoriz',
  [ACTIVITY_TYPES.PRIORITY_CHANGED]: 'PriorityHigh',
  [ACTIVITY_TYPES.PROGRESS_CHANGED]: 'TrendingUp',
  [ACTIVITY_TYPES.DUE_DATE_CHANGED]: 'Schedule',
  [ACTIVITY_TYPES.START_DATE_CHANGED]: 'Schedule',
  [ACTIVITY_TYPES.USER_ASSIGNED]: 'PersonAdd',
  [ACTIVITY_TYPES.USER_UNASSIGNED]: 'PersonRemove',
  [ACTIVITY_TYPES.COMMENT_ADDED]: 'Comment',
  [ACTIVITY_TYPES.COMMENT_DELETED]: 'CommentBank',
  [ACTIVITY_TYPES.CHECKLIST_ITEM_ADDED]: 'PlaylistAdd',
  [ACTIVITY_TYPES.CHECKLIST_ITEM_COMPLETED]: 'CheckCircle',
  [ACTIVITY_TYPES.CHECKLIST_ITEM_UNCOMPLETED]: 'RadioButtonUnchecked',
  [ACTIVITY_TYPES.MADE_PRIVATE]: 'Lock',
  [ACTIVITY_TYPES.MADE_PUBLIC]: 'Public',
};

// Activity colors for different types
// Activity colors for different types - use valid MUI color names
export const ACTIVITY_COLORS = {
  [ACTIVITY_TYPES.TASK_CREATED]: 'success',
  [ACTIVITY_TYPES.TASK_UPDATED]: 'primary',
  [ACTIVITY_TYPES.TASK_DELETED]: 'error',
  [ACTIVITY_TYPES.FIELD_CHANGED]: 'primary',
  [ACTIVITY_TYPES.STATUS_CHANGED]: 'primary',
  [ACTIVITY_TYPES.PRIORITY_CHANGED]: 'warning',
  [ACTIVITY_TYPES.PROGRESS_CHANGED]: 'success',
  [ACTIVITY_TYPES.DUE_DATE_CHANGED]: 'info',
  [ACTIVITY_TYPES.START_DATE_CHANGED]: 'info',
  [ACTIVITY_TYPES.USER_ASSIGNED]: 'success',
  [ACTIVITY_TYPES.USER_UNASSIGNED]: 'warning',
  [ACTIVITY_TYPES.COMMENT_ADDED]: 'primary',
  [ACTIVITY_TYPES.COMMENT_DELETED]: 'secondary',
  [ACTIVITY_TYPES.CHECKLIST_ITEM_ADDED]: 'info',
  [ACTIVITY_TYPES.CHECKLIST_ITEM_COMPLETED]: 'success',
  [ACTIVITY_TYPES.CHECKLIST_ITEM_UNCOMPLETED]: 'secondary',
  [ACTIVITY_TYPES.MADE_PRIVATE]: 'secondary',
  [ACTIVITY_TYPES.MADE_PUBLIC]: 'primary',
};