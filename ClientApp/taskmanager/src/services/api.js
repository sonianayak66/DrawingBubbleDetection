import axios from 'axios';

const API_BASE_URL = 'https://localhost:7029/api';

const api = axios.create({
    baseURL: API_BASE_URL,
    withCredentials: true, // Important for cookie authentication
});

export default api;