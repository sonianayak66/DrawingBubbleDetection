import React, { createContext, useContext, useState, useEffect } from 'react';
import { taskPlannerApi } from '../services/api';

const PermissionsContext = createContext();

export const PermissionsProvider = ({ children }) => {
  const [permissions, setPermissions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadPermissions();
  }, []);

  const loadPermissions = async () => {
    try {
      setLoading(true);
      const response = await taskPlannerApi.getPermissions();
      setPermissions(response.data.permissions || []);
      setError(null);
    } catch (err) {
      console.error('Error loading permissions:', err);
      setError(err.message);
      setPermissions([]);
    } finally {
      setLoading(false);
    }
  };

  const hasPermission = (claimValue) => {
    return permissions.some(p => p.claimValue === claimValue);
  };

  const hasAnyPermission = (claimValues) => {
    return claimValues.some(claimValue => hasPermission(claimValue));
  };

  const value = {
    permissions,
    loading,
    error,
    hasPermission,
    hasAnyPermission,
    reload: loadPermissions
  };

  return (
    <PermissionsContext.Provider value={value}>
      {children}
    </PermissionsContext.Provider>
  );
};

export const usePermissions = () => {
  const context = useContext(PermissionsContext);
  if (!context) {
    throw new Error('usePermissions must be used within a PermissionsProvider');
  }
  return context;
};