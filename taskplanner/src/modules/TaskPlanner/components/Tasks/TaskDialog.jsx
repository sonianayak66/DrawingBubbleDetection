import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  IconButton,
  Tab,
  Tabs,
  Alert,
} from '@mui/material';
import {
  Close,
  Assignment,
  Person,
  Comment,
  CheckBox,
  Timeline,
} from '@mui/icons-material';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { taskPlannerApi } from '../../../../services/api';
import PermissionGuard from '../../../shared/components/Common/PermissionGuard';
import BucketDialog from '../Projects/BucketDialog';

// Import our new hooks and components
import { useTaskForm } from './hooks/useTaskForm';
import { useTaskDetails } from './hooks/useTaskDetails';
import TaskDetailsTab from './TaskDialogTabs/TaskDetailsTab';
import TaskAssignmentsTab from './TaskDialogTabs/TaskAssignmentsTab';
import TaskCommentsTab from './TaskDialogTabs/TaskCommentsTab';
import TaskChecklistTab from './TaskDialogTabs/TaskChecklistTab';

 
import { useTaskActivity } from './hooks/useTaskActivity';
import TaskActivityTab from './TaskDialogTabs/TaskActivityTab';

const TaskDialog = ({ open, onClose, task, onSave, initialData, mode = 'edit' }) => {
  const [activeTab, setActiveTab] = useState(0);
  const [projects, setProjects] = useState([]);
  const [allBuckets, setAllBuckets] = useState([]);
  const [bucketDialogOpen, setBucketDialogOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // Use our custom hooks
  //const taskForm = useTaskForm(task, open);
  const taskForm = useTaskForm(task || initialData, open, mode);
  const taskDetails = useTaskDetails(task?.TaskGUID, open);
 const taskActivity = useTaskActivity(task?.TaskGUID, open);

  const {
    control,
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors, isValid },
    isEditMode,
  } = taskForm;

  // Load initial data when dialog opens
  useEffect(() => {
    if (open) {
      loadInitialData();
    }
  }, [open]);

  const loadInitialData = async () => {
    try {
      const [projectsResponse, bucketsResponse] = await Promise.all([
        taskPlannerApi.getProjects(),
        taskPlannerApi.getBuckets(),
      ]);

      setProjects(projectsResponse.data || []);
      setAllBuckets(bucketsResponse.data || []);
    } catch (err) {
      console.error('Error loading initial data:', err);
      setError('Failed to load project data');
    }
  };

  const handleCreateNewBucket = () => {
    setBucketDialogOpen(true);
  };

  const handleSaveBucket = async (bucketData) => {
    try {
      const result = await taskPlannerApi.saveBucket(bucketData);

      // Reload buckets
      const bucketsResponse = await taskPlannerApi.getBuckets();
      setAllBuckets(bucketsResponse.data || []);

      // Set the newly created bucket as selected
      if (result?.data?.BucketGUID) {
        setValue('BucketGUID', result.data.BucketGUID);
      }

      setBucketDialogOpen(false);
    } catch (error) {
      console.error('Error creating bucket:', error);
      setError('Error creating bucket: ' + error.message);
    }
  };

  const onSubmit = async (formData) => {
  try {
    setLoading(true);
    setError('');

    // Track changes if editing existing task
    if (isEditMode && task) {
      taskActivity.trackTaskChanges(task, formData);
    }

    // Save the task
    await onSave(formData);

    // Track task creation if it's a new task
    if (!isEditMode) {
      taskActivity.trackTaskCreation(formData);
    }

    onClose();
  } catch (error) {
    console.error('Error saving task:', error);
    setError('Error saving task: ' + error.message);
  } finally {
    setLoading(false);
  }
};

// Enhanced functions with activity tracking
const handleAddAssignmentWithTracking = async (user) => {
  try {
    await taskDetails.handleAddAssignment(user);
    taskActivity.trackUserAssignment(user.UserName);
  } catch (error) {
    throw error;
  }
};

const handleRemoveAssignmentWithTracking = async (assignment) => {
  try {
    await taskDetails.handleRemoveAssignment(assignment);
    taskActivity.trackUserUnassignment(assignment.AssignedUserName);
  } catch (error) {
    throw error;
  }
};

const handleAddCommentWithTracking = async () => {
  const commentText = taskDetails.newComment;
  try {
    await taskDetails.handleAddComment();
    taskActivity.trackCommentAdded(commentText);
  } catch (error) {
    throw error;
  }
};

const handleAddChecklistItemWithTracking = async () => {
  const itemText = taskDetails.newChecklistItem;
  try {
    await taskDetails.handleAddChecklistItem();
    taskActivity.trackChecklistItemAdded(itemText);
  } catch (error) {
    throw error;
  }
};

const handleChecklistToggleWithTracking = async (item) => {
  try {
    await taskDetails.handleChecklistToggle(item);
    if (!item.IsCompleted) {
      taskActivity.trackChecklistItemCompleted(item.ItemText);
    } else {
      taskActivity.trackChecklistItemUncompleted(item.ItemText);
    }
  } catch (error) {
    throw error;
  }
};

  const handleClose = () => {
    setError('');
    setActiveTab(0);
    onClose();
  };

  const canEditRelatedData = isEditMode && task?.TaskGUID;
  const isPrivate = watch('IsPrivate');

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns}>
      <Dialog
        open={open}
        onClose={handleClose}
        maxWidth="md"
        fullWidth
        PaperProps={{
          sx: { height: '90vh' },
        }}
      >
        {/* Header */}
        {/* Sleek Header */}
<Box
  sx={{
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    p: 2,
    pb: 1,
    background: 'linear-gradient(135deg, #1976d2 0%, #1565c0 100%)',
    color: 'white',
    borderBottom: 'none',
  }}
>
  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
    <Assignment sx={{ fontSize: 32 }} />
    <Box>
      <Typography variant="h5" sx={{ fontWeight: 600, mb: 0.5 }}>
        {isEditMode ? 'Edit Task' : 'Create New Task'}
      </Typography>
      <Typography variant="body2" sx={{ opacity: 0.9 }}>
        {watch('TaskTitle') || task?.TaskTitle || 'Enter task details below'}
      </Typography>
    </Box>
  </Box>
  <IconButton onClick={handleClose} sx={{ color: 'white' }}>
    <Close />
  </IconButton>
</Box>

        {/* Error Alert */}
        {error && (
          <Alert 
            severity="error" 
            sx={{ m: 2, mb: 0 }}
            onClose={() => setError('')}
          >
            {error}
          </Alert>
        )}

        {/* Tabs */}
     
<Box sx={{ borderBottom: 1, borderColor: 'divider',  }}>
  <Tabs 
    value={activeTab} 
    onChange={(e, newValue) => setActiveTab(newValue)}
    variant="fullWidth"
    sx={{
      '& .MuiTab-root': {
        minHeight: 60,
        fontWeight: 500,
      },
    }}
  >
    <Tab icon={<Assignment />} label="Details" />
    <Tab icon={<Person />} label={`Assignments (${taskDetails.assignments.length})`} />
    <Tab icon={<Comment />} label={`Comments (${taskDetails.comments.length})`} />
    <Tab icon={<CheckBox />} label={`Checklist (${taskDetails.checklists.length})`} />
    <Tab icon={<Timeline />} label={`Activity (${taskActivity.activities.length})`} />
  </Tabs>
</Box>

        {/* Tab Content */}
        <DialogContent sx={{ p: 0, height: '100%', overflow: 'auto' }}>
          {activeTab === 0 && (
            <TaskDetailsTab
              control={control}
              register={register}
              errors={errors}
              watch={watch}
              projects={projects}
              allBuckets={allBuckets}
              assignments={taskDetails.assignments}
              onCreateNewBucket={handleCreateNewBucket}
            />
          )}

          {activeTab === 1 && (
  <TaskAssignmentsTab
    assignments={taskDetails.assignments}
    isPrivate={isPrivate}
    canEditRelatedData={canEditRelatedData}
    onAddAssignment={handleAddAssignmentWithTracking}
    onRemoveAssignment={handleRemoveAssignmentWithTracking}
    loading={taskDetails.loading}
  />
)}

         {activeTab === 2 && (
  <TaskCommentsTab
    comments={taskDetails.comments}
    newComment={taskDetails.newComment}
    setNewComment={taskDetails.setNewComment}
    canEditRelatedData={canEditRelatedData}
    onAddComment={handleAddCommentWithTracking}
    loading={taskDetails.loading}
  />
)}

          {activeTab === 3 && (
  <TaskChecklistTab
    checklists={taskDetails.checklists}
    newChecklistItem={taskDetails.newChecklistItem}
    setNewChecklistItem={taskDetails.setNewChecklistItem}
    canEditRelatedData={canEditRelatedData}
    onAddChecklistItem={handleAddChecklistItemWithTracking}
    onChecklistToggle={handleChecklistToggleWithTracking}
    loading={taskDetails.loading}
  />
)}

          {activeTab === 4 && (
  <TaskActivityTab
    activities={taskActivity.activities}
    loading={taskActivity.loading}
    error={taskActivity.error}
    projects={projects}
    buckets={allBuckets}
  />
)}
        </DialogContent>

        {/* Actions */}
        <DialogActions sx={{ p: 2, borderTop: '1px solid', borderColor: 'divider' }}>
          <Button onClick={handleClose} disabled={loading}>
            Cancel
          </Button>
          <PermissionGuard permission="TaskPlanner_Tasks_Write">
            <Button
              onClick={handleSubmit(onSubmit)}
              variant="contained"
              disabled={loading || !isValid}
            >
              {loading ? 'Saving...' : isEditMode ? 'Update Task' : 'Create Task'}
            </Button>
          </PermissionGuard>
        </DialogActions>

        {/* Bucket Creation Dialog */}
        <PermissionGuard permission="TaskPlanner_Projects_Write">
          <BucketDialog
            open={bucketDialogOpen}
            onClose={() => setBucketDialogOpen(false)}
            bucket={null}
            onSave={handleSaveBucket}
          />
        </PermissionGuard>
      </Dialog>
    </LocalizationProvider>
  );
};

export default TaskDialog;
