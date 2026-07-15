import api from './api';

export const homeService = {
    getTaskCounts: async () => {
        const response = await api.get('/home/GetTaskCounts');
        return response.data;
    }
};