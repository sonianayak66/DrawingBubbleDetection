import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  IconButton,
  Alert,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Badge,
  Chip,
  Grid,
} from '@mui/material';
import {
  Close,
  Assignment,
  Person,
  Comment,
  CheckBox,
  Timeline,
  ExpandMore,
  Save,
  Cancel,
} from '@mui/icons-material';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { taskPlannerApi } from '../../services/api';
import PermissionGuard from '../Common/PermissionGuard';
import BucketDialog from '../Projects/BucketDialog';

// Import our hooks and components
import { useTaskForm } from './hooks/useTaskForm';
import { useTaskDetails } from './hooks/useTaskDetails';
import { useTaskActivity } from './hooks/useTaskActivity';
import TaskDetailsTab from './TaskDialogTabs/TaskDetailsTab';
import TaskAssignmentsTab from './TaskDialogTabs/TaskAssignmentsTab';
import TaskCommentsTab from './TaskDialogTabs/TaskCommentsTab';
import TaskChecklistTab from './TaskDialogTabs/TaskChecklistTab';
import TaskActivityTab from './TaskDialogTabs/TaskActivityTab';

const TaskDialogAccordion = ({ open, onClose, task, onSave }) => {
  const [expanded, setExpanded] = useState('details'); // Default open section
  const [projects, setProjects] = useState([]);
  const [allBuckets, setAllBuckets] = useState([]);
  const [bucketDialogOpen, setBucketDialogOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // Use our custom hooks
  const taskForm = useTaskForm(task, open);
  const taskDetails = useTaskDetails(task?.TaskGUID, open);
  const taskActivity = useTaskActivity(task?.TaskGUID, open);

  const {
    control,
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors, isValid, isDirty },
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

  const handleClose = () => {
    setError('');
    setExpanded('details');
    onClose();
  };

  const handleAccordionChange = (panel) => (event, isExpanded) => {
    setExpanded(isExpanded ? panel : false);
  };

  // Enhanced activity tracking functions
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

  const canEditRelatedData = isEditMode && task?.TaskGUID;
  const isPrivate = watch('IsPrivate');

  // Get task title for header
  const taskTitle = watch('TaskTitle') || task?.TaskTitle || 'New Task';

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns}>
      <Dialog
        open={open}
        onClose={handleClose}
        maxWidth="lg"
        fullWidth
        PaperProps={{
          sx: { 
            height: '95vh',
            maxHeight: '95vh',
          },
        }}
      >
        {/* Sleek Header */}
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            p: 3,
            pb: 2,
            borderBottom: '1px solid',
            borderColor: 'divider',
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            color: 'white',
          }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flex: 1 }}>
            <Assignment sx={{ fontSize: 28 }} />
            <Box>
              <Typography variant="h5" sx={{ fontWeight: 600, mb: 0.5 }}>
                {isEditMode ? 'Edit Task' : 'Create Task'}
              </Typography>
              <Typography variant="body2" sx={{ opacity: 0.9 }}>
                {taskTitle}
              </Typography>
            </Box>
          </Box>
          
          {/* Quick Action Chips */}
          <Box sx={{ display: 'flex', gap: 1, mr: 2 }}>
            {isEditMode && (
              <>
                <Chip 
                  icon={<Person />} 
                  label={taskDetails.assignments.length}
                  size="small" 
                  sx={{ bgcolor: 'rgba(255,255,255,0.2)', color: 'white' }}
                />
                <Chip 
                  icon={<Comment />} 
                  label={taskDetails.comments.length}
                  size="small" 
                  sx={{ bgcolor: 'rgba(255,255,255,0.2)', color: 'white' }}
                />
                <Chip 
                  icon={<CheckBox />} 
                  label={taskDetails.checklists.length}
                  size="small" 
                  sx={{ bgcolor: 'rgba(255,255,255,0.2)', color: 'white' }}
                />
              </>
            )}
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

        {/* Accordion Content */}
        <DialogContent sx={{ p: 0, overflow: 'auto', bgcolor: 'grey.50' }}>
          <Box sx={{ p: 2 }}>
            {/* Task Details Accordion */}
            <Accordion 
              expanded={expanded === 'details'} 
              onChange={handleAccordionChange('details')}
              sx={{ mb: 1, boxShadow: 2 }}
            >
              <AccordionSummary 
                expandIcon={<ExpandMore />}
                sx={{ bgcolor: 'primary.main', color: 'white' }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, width: '100%' }}>
                  <Assignment />
                  <Typography sx={{ fontWeight: 600 }}>Task Details</Typography>
                  {!isValid && (
                    <Chip 
                      label="Required fields missing" 
                      size="small" 
                      color="error"
                      sx={{ ml: 'auto', mr: 2 }}
                    />
                  )}
                </Box>
              </AccordionSummary>
              <AccordionDetails sx={{ p: 0 }}>
                <TaskDetailsTab
                  control={control}
                  register={register}
                  errors={errors}
                  watch={watch}
                  projects={projects}
                  allBuckets={allBuckets}
                  onCreateNewBucket={handleCreateNewBucket}
                />
              </AccordionDetails>
            </Accordion>

            {/* Assignments Accordion */}
            {isEditMode && (
              <Accordion 
                expanded={expanded === 'assignments'} 
                onChange={handleAccordionChange('assignments')}
                sx={{ mb: 1, boxShadow: 2 }}
              >
                <AccordionSummary expandIcon={<ExpandMore />}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Badge badgeContent={taskDetails.assignments.length} color="primary">
                      <Person />
                    </Badge>
                    <Typography sx={{ fontWeight: 600 }}>
                      Assignments ({taskDetails.assignments.length})
                    </Typography>
                  </Box>
                </AccordionSummary>
                <AccordionDetails sx={{ p: 0 }}>
                  <TaskAssignmentsTab
                    assignments={taskDetails.assignments}
                    isPrivate={isPrivate}
                    canEditRelatedData={canEditRelatedData}
                    onAddAssignment={handleAddAssignmentWithTracking}
                    onRemoveAssignment={handleRemoveAssignmentWithTracking}
                    loading={taskDetails.loading}
                  />
                </AccordionDetails>
              </Accordion>
            )}

            {/* Comments Accordion */}
            {isEditMode && (
              <Accordion 
                expanded={expanded === 'comments'} 
                onChange={handleAccordionChange('comments')}
                sx={{ mb: 1, boxShadow: 2 }}
              >
                <AccordionSummary expandIcon={<ExpandMore />}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Badge badgeContent={taskDetails.comments.length} color="primary">
                      <Comment />
                    </Badge>
                    <Typography sx={{ fontWeight: 600 }}>
                      Comments ({taskDetails.comments.length})
                    </Typography>
                  </Box>
                </AccordionSummary>
                <AccordionDetails sx={{ p: 0 }}>
                  <TaskCommentsTab
                    comments={taskDetails.comments}
                    newComment={taskDetails.newComment}
                    setNewComment={taskDetails.setNewComment}
                    canEditRelatedData={canEditRelatedData}
                    onAddComment={handleAddCommentWithTracking}
                    loading={taskDetails.loading}
                  />
                </AccordionDetails>
              </Accordion>
            )}

            {/* Checklist Accordion */}
            {isEditMode && (
              <Accordion 
                expanded={expanded === 'checklist'} 
                onChange={handleAccordionChange('checklist')}
                sx={{ mb: 1, boxShadow: 2 }}
              >
                <AccordionSummary expandIcon={<ExpandMore />}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Badge badgeContent={taskDetails.checklists.length} color="primary">
                      <CheckBox />
                    </Badge>
                    <Typography sx={{ fontWeight: 600 }}>
                      Checklist ({taskDetails.checklists.length})
                    </Typography>
                  </Box>
                </AccordionSummary>
                <AccordionDetails sx={{ p: 0 }}>
                  <TaskChecklistTab
                    checklists={taskDetails.checklists}
                    newChecklistItem={taskDetails.newChecklistItem}
                    setNewChecklistItem={taskDetails.setNewChecklistItem}
                    canEditRelatedData={canEditRelatedData}
                    onAddChecklistItem={handleAddChecklistItemWithTracking}
                    onChecklistToggle={handleChecklistToggleWithTracking}
                    loading={taskDetails.loading}
                  />
                </AccordionDetails>
              </Accordion>
            )}

            {/* Activity Timeline Accordion */}
            {isEditMode && (
              <Accordion 
                expanded={expanded === 'activity'} 
                onChange={handleAccordionChange('activity')}
                sx={{ mb: 1, boxShadow: 2 }}
              >
                <AccordionSummary expandIcon={<ExpandMore />}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Badge badgeContent={taskActivity.activities.length} color="primary">
                      <Timeline />
                    </Badge>
                    <Typography sx={{ fontWeight: 600 }}>
                      Activity Timeline ({taskActivity.activities.length})
                    </Typography>
                  </Box>
                </AccordionSummary>
                <AccordionDetails sx={{ p: 0 }}>
                  <TaskActivityTab
                    activities={taskActivity.activities}
                    loading={taskActivity.loading}
                    error={taskActivity.error}
                    projects={projects}
                    buckets={allBuckets}
                  />
                </AccordionDetails>
              </Accordion>
            )}
          </Box>
        </DialogContent>

        {/* Modern Action Bar */}
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            p: 2,
            borderTop: '1px solid',
            borderColor: 'divider',
            bgcolor: 'background.paper',
          }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            {isDirty && (
              <Chip 
                label="Unsaved changes" 
                size="small" 
                color="warning" 
                variant="outlined" 
              />
            )}
            {loading && (
              <Chip 
                label="Saving..." 
                size="small" 
                color="info" 
                variant="outlined" 
              />
            )}
          </Box>

          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button 
              onClick={handleClose} 
              disabled={loading}
              startIcon={<Cancel />}
              variant="outlined"
            >
              Cancel
            </Button>
            <PermissionGuard permission="TaskPlanner_Tasks_Write">
              <Button
                onClick={handleSubmit(onSubmit)}
                variant="contained"
                disabled={loading || !isValid}
                startIcon={<Save />}
                sx={{ 
                  minWidth: 120,
                  background: 'linear-gradient(45deg, #2196F3 30%, #21CBF3 90%)',
                }}
              >
                {loading ? 'Saving...' : isEditMode ? 'Update Task' : 'Create Task'}
              </Button>
            </PermissionGuard>
          </Box>
        </Box>

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

export default TaskDialogAccordion;