import api from './api';

export const authService = {
    getCurrentUser: async () => {
        const response = await api.get('/auth/userPermissions');
        return response.data;
    }
};