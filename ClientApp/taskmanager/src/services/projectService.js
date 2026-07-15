import api from './api';

export const projectService = {
    getProjects: async () => {
        const response = await api.get('/projects/GetProjects');
        return response.data;
    },

    createProject: async (projectData) => {
        const response = await api.post('/projects/CreateProject', projectData);
        return response.data;
    },

    updateProject: async (projectGuid, projectData) => {
        const response = await api.put(`/projects/UpdateProject/${projectGuid}`, projectData);
        return response.data;
    },

     getProjectDetails: async (projectGuid) => {
        const response = await api.get(`/projects/GetProjectDetails?projectGuid=${projectGuid}`);
        return response.data;
    },
    getProjectTasks: async (projectGuid) => {
        const response = await api.get(`/projects/GetProjectTasks?projectGuid=${projectGuid}`);
        return response.data;
    },
    getTaskManagerUsers: async () => {
        const response = await api.get('/projects/GetTaskManagerUsers');
        return response.data;
    },
    getActiveTags: async () => {
        const response = await api.get('/projects/GetActiveTags');
        return response.data;
    },
};