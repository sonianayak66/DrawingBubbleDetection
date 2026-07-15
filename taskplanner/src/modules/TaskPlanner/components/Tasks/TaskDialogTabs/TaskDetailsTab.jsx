import React from 'react';
import {
  Grid,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Box,
  Divider,
  Slider,
  Typography,
  Switch,
  FormControlLabel,
} from '@mui/material';
import { Add } from '@mui/icons-material';
import { Controller } from 'react-hook-form';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import PermissionGuard from '../../../../shared/components/Common/PermissionGuard';
import { usePermissions } from '../../../../../context/PermissionsContext';

const TaskDetailsTab = ({
  control,
  register,
  errors,
  watch,
  projects,
  allBuckets,
  assignments = [],
  onCreateNewBucket,
}) => {
  const { hasPermission } = usePermissions();
  const selectedProjectGuid = watch('ProjectGUID');
  
  // Filter buckets by selected project
  const availableBuckets = allBuckets;

  return (
    <Box sx={{ p: 3 }}>
      <Grid container spacing={3}>
        {/* Task Title */}
        <Grid item size={12}>
          <TextField
            {...register('TaskTitle')}
            fullWidth
            label="Task Title *"
            error={!!errors.TaskTitle}
            helperText={errors.TaskTitle?.message}
          />
        </Grid>

        {/* Project Selection */}
        <Grid item size ={{xs:12, sm:6}}>
          <FormControl fullWidth error={!!errors.ProjectGUID}>
            <InputLabel>Project *</InputLabel>
            <Controller
              name="ProjectGUID"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  label="Project *"
                  value={field.value || ''}
                >
                  {projects.map((project) => (
                    <MenuItem key={project.ProjectGUID} value={project.ProjectGUID}>
                      {project.ProjectName}
                    </MenuItem>
                  ))}
                </Select>
              )}
            />
            {errors.ProjectGUID && (
              <Typography variant="caption" color="error" sx={{ mt: 0.5, ml: 2 }}>
                {errors.ProjectGUID.message}
              </Typography>
            )}
          </FormControl>
        </Grid>

        {/* Bucket/Status Selection */}
        <Grid item size ={{xs:12, sm:6}}>
          <FormControl fullWidth error={!!errors.BucketGUID}>
            <InputLabel>Status *</InputLabel>
            <Controller
              name="BucketGUID"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  label="Status *"
                  value={field.value || ''}
                  disabled={!selectedProjectGuid}
                >
                  {availableBuckets.map((bucket) => (
                    <MenuItem key={bucket.BucketGUID} value={bucket.BucketGUID}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Box
                          sx={{
                            width: 12,
                            height: 12,
                            backgroundColor: bucket.BucketColor || '#1976d2',
                            borderRadius: '50%',
                          }}
                        />
                        {bucket.BucketName}
                      </Box>
                    </MenuItem>
                  ))}
                  <Divider />
                  {hasPermission('TaskPlanner_Projects_Write') && (
                    <MenuItem value="CREATE_NEW" onClick={onCreateNewBucket}>
                      <Add sx={{ mr: 1 }} /> Create New Status
                    </MenuItem>
                  )}
                </Select>
              )}
            />
            {errors.BucketGUID && (
              <Typography variant="caption" color="error" sx={{ mt: 0.5, ml: 2 }}>
                {errors.BucketGUID.message}
              </Typography>
            )}
          </FormControl>
        </Grid>

        {/* Priority */}
        <Grid item size ={{xs:12, sm:3}}>
          <FormControl fullWidth>
            <InputLabel>Priority</InputLabel>
            <Controller
              name="Priority"
              control={control}
              render={({ field }) => (
                <Select {...field} label="Priority">
                  <MenuItem value="Critical">Critical</MenuItem>
                  <MenuItem value="High">High</MenuItem>
                  <MenuItem value="Medium">Medium</MenuItem>
                  <MenuItem value="Low">Low</MenuItem>
                </Select>
              )}
            />
          </FormControl>
        </Grid>

        {/* Progress Percentage */}
        <Grid item size ={{xs:12, sm:9}}>
          <Typography gutterBottom>Progress</Typography>
          <Controller
            name="ProgressPercentage"
            control={control}
            render={({ field }) => (
              <Box sx={{ px: 2 }}>
                <Slider
                  {...field}
                  value={field.value || 0}
                  min={0}
                  max={100}
                  step={5}
                  marks
                  valueLabelDisplay="auto"
                  valueLabelFormat={(value) => `${value}%`}
                />
              </Box>
            )}
          />
          {errors.ProgressPercentage && (
            <Typography variant="caption" color="error">
              {errors.ProgressPercentage.message}
            </Typography>
          )}
        </Grid>

        {/* Description */}
        <Grid item size ={{xs:12}}>
          <TextField
            {...register('TaskDescription')}
            fullWidth
            label="Description"
            multiline
            rows={2}
            error={!!errors.TaskDescription}
            helperText={errors.TaskDescription?.message}
          />
        </Grid>

        {/* Start Date */}
        <Grid item size ={{xs:12, sm:4}}>
          <Controller
            name="StartDate"
            control={control}
            render={({ field }) => (
              <DatePicker
                {...field}
                label="Start Date"
                slotProps={{
                  textField: {
                    fullWidth: true,
                    error: !!errors.StartDate,
                    helperText: errors.StartDate?.message,
                  },
                }}
              />
            )}
          />
        </Grid>

        {/* Due Date */}
        <Grid item size ={{xs:12, sm:4}}>
          <Controller
            name="DueDate"
            control={control}
            render={({ field }) => (
              <DatePicker
                {...field}
                label="Due Date"
                slotProps={{
                  textField: {
                    fullWidth: true,
                    error: !!errors.DueDate,
                    helperText: errors.DueDate?.message,
                  },
                }}
              />
            )}
          />
        </Grid>

        {/* Estimated Hours */}
        <Grid item size ={{xs:12, sm:4}}>
          <TextField
            {...register('EstimatedHours', { valueAsNumber: true })}
            fullWidth
            label="Estimated Hours"
            type="number"
            inputProps={{ min: 0, step: 0.5 }}
            error={!!errors.EstimatedHours}
            helperText={errors.EstimatedHours?.message}
          />
        </Grid>

        {/* Tags */}
        <Grid item size ={{xs:12, sm:6}}>
          <TextField
            {...register('Tags')}
            fullWidth
            label="Tags"
            placeholder="Separate tags with commas"
            error={!!errors.Tags}
            helperText={errors.Tags?.message}
          />
        </Grid>

        {/* Private Task Toggle */}
 {/* Enhanced Private Task Toggle */}
<Grid item size={{xs:12}}>
  <Box sx={{ 
    p: 2, 
    border: '1px solid', 
    borderColor: field => field.value ? 'warning.main' : 'divider', 
    borderRadius: 1,
    bgcolor: field => field.value ? 'warning.light' : 'background.paper',
    transition: 'all 0.3s ease'
  }}>
    <Controller
      name="IsPrivate"
      control={control}
      render={({ field }) => {
        const isCurrentlyPrivate = field.value || false;
        const hasAssignments = assignments.length > 0;
        const totalAssignments = assignments.length;
        
        return (
          <FormControlLabel
            control={
              <Switch
                checked={isCurrentlyPrivate}
                onChange={(e) => {
                  const isPrivate = e.target.checked;
                  
                  // If making private and has assignments, show confirmation
                  if (isPrivate && hasAssignments) {
                    const confirmUnassign = window.confirm(
                      `Making this task private will remove all ${totalAssignments} current assignment${totalAssignments > 1 ? 's' : ''}. Continue?`
                    );
                    if (!confirmUnassign) {
                      return; // Don't change if cancelled
                    }
                  }
                  
                  field.onChange(isPrivate);
                }}
                color={isCurrentlyPrivate ? 'warning' : 'primary'}
              />
            }
            label={
              <Box>
                <Typography variant="body2" sx={{ fontWeight: 500, color: isCurrentlyPrivate ? 'warning.dark' : 'text.primary' }}>
                  {isCurrentlyPrivate 
                    ? '🔒 This task is currently private' 
                    : '🌐 This task is currently public'
                  }
                </Typography>
                <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
                  {isCurrentlyPrivate 
                    ? 'Only you can see this task. Toggle to make it public and allow assignments.'
                    : hasAssignments 
                      ? `Toggle to make private (will remove ${totalAssignments} current assignment${totalAssignments > 1 ? 's' : ''})`
                      : 'Toggle to make private (only visible to you)'
                  }
                </Typography>
                
                {/* Additional status indicators */}
                {isCurrentlyPrivate && (
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 1 }}>
                    <Typography variant="caption" sx={{ 
                      bgcolor: 'warning.main', 
                      color: 'white', 
                      px: 1, 
                      py: 0.25, 
                      borderRadius: '12px',
                      fontSize: '0.7rem',
                      fontWeight: 500
                    }}>
                      PRIVATE TASK
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      Assignments disabled
                    </Typography>
                  </Box>
                )}
                
                {!isCurrentlyPrivate && hasAssignments && (
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 1 }}>
                    <Typography variant="caption" sx={{ 
                      bgcolor: 'primary.main', 
                      color: 'white', 
                      px: 1, 
                      py: 0.25, 
                      borderRadius: '12px',
                      fontSize: '0.7rem',
                      fontWeight: 500
                    }}>
                      PUBLIC TASK
                    </Typography>
                    <Typography variant="caption" color="success.main">
                      {totalAssignments} user{totalAssignments > 1 ? 's' : ''} assigned
                    </Typography>
                  </Box>
                )}
              </Box>
            }
          />
        );
      }}
    />
  </Box>
</Grid>
      </Grid>
    </Box>
  );
};

export default TaskDetailsTab;
