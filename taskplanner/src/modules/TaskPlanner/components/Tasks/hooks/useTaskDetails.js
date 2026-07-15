import { useState, useEffect } from 'react';
import { taskPlannerApi } from '../../../../../services/api';

export const useTaskDetails = (taskGuid, open) => {
  const [assignments, setAssignments] = useState([]);
  const [comments, setComments] = useState([]);
  const [checklists, setChecklists] = useState([]);
  const [newComment, setNewComment] = useState('');
  const [newChecklistItem, setNewChecklistItem] = useState('');
  const [loading, setLoading] = useState(false);

  // Load task details when dialog opens with existing task
  useEffect(() => {
    if (open && taskGuid) {
      loadTaskDetails();
    } else if (open && !taskGuid) {
      // Reset for new task
      setAssignments([]);
      setComments([]);
      setChecklists([]);
      setNewComment('');
      setNewChecklistItem('');
    }
  }, [taskGuid, open]);

  const loadTaskDetails = async () => {
    if (!taskGuid) return;

    try {
      setLoading(true);
      const [assignmentsRes, commentsRes, checklistsRes] = await Promise.all([
        taskPlannerApi.getTaskAssignments(taskGuid).catch(() => ({ data: [] })),
        taskPlannerApi.getTaskComments(taskGuid).catch(() => ({ data: [] })),
        taskPlannerApi.getTaskChecklists(taskGuid).catch(() => ({ data: [] })),
      ]);

      setAssignments(assignmentsRes.data || []);
      setComments(commentsRes.data || []);
      setChecklists(checklistsRes.data || []);
    } catch (err) {
      console.error('Error loading task details:', err);
    } finally {
      setLoading(false);
    }
  };

  // Assignment functions
  const handleAddAssignment = async (user) => {
    if (!taskGuid) {
      throw new Error('Please save the task first before assigning users');
    }

    try {
      const assignmentData = {
        TaskGUID: taskGuid,
        AssignedUserDbkey: user.UserDbkey,
        AssignedUserName: user.UserName,
      };

      await taskPlannerApi.saveTaskAssignment(assignmentData);
      await loadTaskDetails(); // Refresh data
    } catch (err) {
      console.error('Error adding assignment:', err);
      throw new Error('Error adding assignment: ' + err.message);
    }
  };

  const handleRemoveAssignment = async (assignment) => {
    try {
      await taskPlannerApi.deleteTaskAssignment({
        AssignmentId: assignment.AssignmentId,
      });
      await loadTaskDetails(); // Refresh data
    } catch (err) {
      console.error('Error removing assignment:', err);
      throw new Error('Error removing assignment: ' + err.message);
    }
  };

  // Comment functions
  const handleAddComment = async () => {
    if (!newComment.trim() || !taskGuid) return;

    try {
      const commentData = {
        TaskGUID: taskGuid,
        CommentText: newComment,
        CommentType: 'Comment',
        CommentGUID: null,
      };

      await taskPlannerApi.saveTaskComment(commentData);
      setNewComment('');
      await loadTaskDetails(); // Refresh data
    } catch (err) {
      console.error('Error adding comment:', err);
      throw new Error('Error adding comment: ' + err.message);
    }
  };

  // Checklist functions
  const handleAddChecklistItem = async () => {
    if (!newChecklistItem.trim() || !taskGuid) return;

    try {
      const checklistData = {
        TaskGUID: taskGuid,
        ItemText: newChecklistItem,
        IsCompleted: false,
        SortOrder: checklists.length,
        ChecklistGUID: null,
      };

      await taskPlannerApi.saveTaskChecklist(checklistData);
      setNewChecklistItem('');
      await loadTaskDetails(); // Refresh data
    } catch (err) {
      console.error('Error adding checklist item:', err);
      throw new Error('Error adding checklist item: ' + err.message);
    }
  };

  const handleChecklistToggle = async (checklistItem) => {
    try {
      const updatedItem = {
        ...checklistItem,
        IsCompleted: !checklistItem.IsCompleted,
      };

      await taskPlannerApi.saveTaskChecklist(updatedItem);
      await loadTaskDetails(); // Refresh data
    } catch (err) {
      console.error('Error updating checklist item:', err);
      throw new Error('Error updating checklist item: ' + err.message);
    }
  };

  return {
    // Data
    assignments,
    comments,
    checklists,
    newComment,
    newChecklistItem,
    loading,
    
    // Setters
    setNewComment,
    setNewChecklistItem,
    
    // Functions
    loadTaskDetails,
    handleAddAssignment,
    handleRemoveAssignment,
    handleAddComment,
    handleAddChecklistItem,
    handleChecklistToggle,
  };
};
