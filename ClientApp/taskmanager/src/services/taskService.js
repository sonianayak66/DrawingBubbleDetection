import api from './api';

export const taskService = {
    createTask: async (taskData) => {
        const response = await api.post('/tasks/CreateTask', {
            projectGuid: taskData.projectGuid,
            taskTitle: taskData.taskTitle,
            taskDescription: taskData.taskDescription,
            priority: taskData.priority,
            dueDate: taskData.dueDate,
            estimatedHours: taskData.estimatedHours ? parseFloat(taskData.estimatedHours) : null,
            assignedTo: taskData.assignedTo || null,
            tags: taskData.tags || []
        });
        return response.data;
    },


    updateTask: async (taskGuid, taskData) => {
        const response = await api.put(`/tasks/UpdateTask/${taskGuid}`, {
            TaskTitle: taskData.taskTitle,            // PascalCase
            TaskDescription: taskData.taskDescription, // PascalCase
            Priority: taskData.priority,              // PascalCase
            DueDate: taskData.dueDate,               // PascalCase
            EstimatedHours: taskData.estimatedHours ? parseFloat(taskData.estimatedHours) : null, // PascalCase
            AssignedTo: taskData.assignedTo || null,  // PascalCase
            Tags: taskData.tags || []                // PascalCase
        });
        return response.data;
    },

    getTasks: async (filterType = 'User', filterValue = 'All') => {
        const response = await api.get('/tasks/GetTasks', {
            params: {
                filterType,
                filterValue
            }
        });
        return response.data;
    },
 
};