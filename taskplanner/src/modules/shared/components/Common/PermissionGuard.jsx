import React from 'react';
import { usePermissions } from '../../../../context/PermissionsContext';

const PermissionGuard = ({ 
  permission, 
  permissions, // For multiple permissions (OR logic)
  requireAll = false, // For AND logic when using multiple permissions
  children, 
  fallback = null 
}) => {
  const { hasPermission, hasAnyPermission } = usePermissions();

  // Single permission check
  if (permission) {
    return hasPermission(permission) ? children : fallback;
  }

  // Multiple permissions check
  if (permissions && permissions.length > 0) {
    const hasAccess = requireAll 
      ? permissions.every(p => hasPermission(p))
      : hasAnyPermission(permissions);
    
    return hasAccess ? children : fallback;
  }

  // No permissions specified, show children
  return children;
};

export default PermissionGuard;
