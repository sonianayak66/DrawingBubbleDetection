import React, { createContext, useContext, useState, useEffect } from 'react';
import { taskPlannerApi } from '../services/api';

const UserContext = createContext();

export const UserProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [permissions, setPermissions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadUserAndPermissions();
  }, []);

  const loadUserAndPermissions = async () => {
    try {
      setLoading(true);
      const response = await taskPlannerApi.getPermissions();
      const data = response.data || {};
      
      // Set user information
      setUser(data.user || null);
      
      // Set permissions
      setPermissions(data.permissions || []);
      
      setError(null);
    } catch (err) {
      console.error('Error loading user and permissions:', err);
      setError(err.message);
      setUser(null);
      setPermissions([]);
    } finally {
      setLoading(false);
    }
  };

  // Permission checking functions
  const hasPermission = (claimValue) => {
    return permissions.some(p => p.claimValue === claimValue);
  };

  const hasAnyPermission = (claimValues) => {
    return claimValues.some(claimValue => hasPermission(claimValue));
  };

  // User helper functions
  const getCurrentUserId = () => {
    return user?.userDbkey || null;
  };

  const getCurrentUserName = () => {
    return user?.userName || 'Unknown User';
  };

  const getCurrentUserEmail = () => {
    return user?.email || null;
  };

  const isUserLoaded = () => {
    return user !== null;
  };

  const value = {
    // User data
    user,
    getCurrentUserId,
    getCurrentUserName, 
    getCurrentUserEmail,
    isUserLoaded,
    
    // Permissions
    permissions,
    hasPermission,
    hasAnyPermission,
    
    // State
    loading,
    error,
    
    // Actions
    reload: loadUserAndPermissions
  };

  return (
    <UserContext.Provider value={value}>
      {children}
    </UserContext.Provider>
  );
};

export const useUser = () => {
  const context = useContext(UserContext);
  if (!context) {
    throw new Error('useUser must be used within a UserProvider');
  }
  return context;
};

// Backward compatibility - export permissions-specific hook
export const usePermissions = () => {
  const context = useUser();
  return {
    permissions: context.permissions,
    hasPermission: context.hasPermission,
    hasAnyPermission: context.hasAnyPermission,
    loading: context.loading,
    error: context.error,
    reload: context.reload,
  };
};