import React, { createContext, useContext, useState, useEffect } from 'react';
import { authService } from '../services/authService';

const AuthContext = createContext();

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within AuthProvider');
    }
    return context;
};

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        console.log('Current URL:', window.location.href);
        console.log('Search params:', window.location.search);

        // Get params from URL
        const urlParams = new URLSearchParams(window.location.search);
        const userGuid = urlParams.get('userGuid');

        console.log('UserGuid from URL:', userGuid);
        fetchUserPermissions();
    }, []);

    const fetchUserPermissions = async () => {
        try {
            const response = await authService.getCurrentUser();
            if (response.success) {
                setUser(response.data);
            }
        } catch (error) {
            console.error('Failed to fetch user permissions:', error);
        } finally {
            setLoading(false);
        }
    };

    const hasPermission = (permission) => {
        return user?.permissions?.[permission] || false;
    };

    if (loading) {
        return <div>Loading permissions...</div>;
    }

    return (
        <AuthContext.Provider value={{ user, hasPermission }}>
            {children}
        </AuthContext.Provider>
    );
};