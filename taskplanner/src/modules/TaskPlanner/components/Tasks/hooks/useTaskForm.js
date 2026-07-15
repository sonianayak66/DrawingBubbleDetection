import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useEffect } from 'react';
import { taskValidationSchema } from '../validation/taskValidationSchema';

export const useTaskForm = (taskOrInitialData, open, mode = 'edit') => {
  const form = useForm({
    resolver: yupResolver(taskValidationSchema),
    defaultValues: {
      TaskGUID: null,
      ProjectGUID: null,
      BucketGUID: null,
      TaskTitle: '',
      TaskDescription: '',
      Priority: 'Medium',
      StartDate: null,
      DueDate: null,
      StartDateTime: null,
      EndDateTime: null,
      Tags: '',
      ProgressPercentage: 0,
      EstimatedHours: null,
      IsPrivate: false,
    },
    mode: 'onChange' // Validate as user types
  });

  const { reset, formState: { errors, isValid, isDirty } } = form;

  useEffect(() => {
  if (open) {
    if (taskOrInitialData) {
      // Helper function to safely parse dates
      const parseDate = (dateValue) => {
        if (!dateValue) return null;
        if (dateValue instanceof Date) return dateValue;
        const parsed = new Date(dateValue);
        return isNaN(parsed.getTime()) ? null : parsed;
      };
      
      // Edit mode - populate with existing task data or initial data
      reset({
        TaskGUID: taskOrInitialData.TaskGUID || null,
        ProjectGUID: taskOrInitialData.ProjectGUID || null,
        BucketGUID: taskOrInitialData.BucketGUID || null,
        TaskTitle: taskOrInitialData.TaskTitle || '',
        TaskDescription: taskOrInitialData.TaskDescription || '',
        Priority: taskOrInitialData.Priority || 'Medium',
        StartDate: parseDate(taskOrInitialData.StartDate),
        DueDate: parseDate(taskOrInitialData.DueDate),
        StartDateTime: parseDate(taskOrInitialData.StartDateTime),
        EndDateTime: parseDate(taskOrInitialData.EndDateTime),
        Tags: taskOrInitialData.Tags || '',
        ProgressPercentage: taskOrInitialData.ProgressPercentage || 0,
        EstimatedHours: taskOrInitialData.EstimatedHours || null,
        IsPrivate: taskOrInitialData.IsPrivate || false,
      });
    } else {
      // Create mode - reset to defaults
      reset({
        TaskGUID: null,
        ProjectGUID: null,
        BucketGUID: null,
        TaskTitle: '',
        TaskDescription: '',
        Priority: 'Medium',
        StartDate: null,
        DueDate: null,
        StartDateTime: null,
        EndDateTime: null,
        Tags: '',
        ProgressPercentage: 0,
        EstimatedHours: null,
        IsPrivate: false,
      });
    }
  }
}, [taskOrInitialData, open, reset]);

  const isEditMode = !!(taskOrInitialData?.TaskGUID);

  return {
    ...form,
    errors,
    isValid,
    isDirty,
    isEditMode,
  };
}