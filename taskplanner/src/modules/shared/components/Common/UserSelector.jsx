import React, { useState, useEffect } from 'react';
import {
  Autocomplete,
  TextField,
  Avatar,
  Box,
  Typography,
  CircularProgress
} from '@mui/material';
import { Person } from '@mui/icons-material';
import { taskPlannerApi } from '../../../../services/api';

const UserSelector = ({ onUserSelect, excludeUserIds = [] }) => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [selectedUser, setSelectedUser] = useState(null);

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      setLoading(true);
      const response = await taskPlannerApi.getUsers();
      setUsers(response.data || []);
    } catch (err) {
      console.error('Error loading users:', err);
      setUsers([]); // Fallback to empty array
    } finally {
      setLoading(false);
    }
  };

  const availableUsers = users.filter(user => !excludeUserIds.includes(user.UserDbkey));

  const handleUserSelection = (event, newValue) => {
    if (newValue) {
      onUserSelect(newValue);
      setSelectedUser(null); // Clear selection after adding
    }
  };

  return (
    <Autocomplete
      value={selectedUser}
      onChange={handleUserSelection}
      options={availableUsers}
      getOptionLabel={(option) => option.UserName}
      loading={loading}
      renderInput={(params) => (
        <TextField
          {...params}
          placeholder="Search users to assign..."
          variant="outlined"
          InputProps={{
            ...params.InputProps,
            endAdornment: (
              <>
                {loading ? <CircularProgress color="inherit" size={20} /> : null}
                {params.InputProps.endAdornment}
              </>
            ),
          }}
        />
      )}
      renderOption={(props, option) => (
        <Box component="li" {...props} sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Avatar sx={{ width: 32, height: 32 }}>
            {option.UserName.charAt(0)}
          </Avatar>
          <Box>
            <Typography variant="body2">{option.UserName}</Typography>
            <Typography variant="caption" color="text.secondary">
              {option.Email}
            </Typography>
          </Box>
        </Box>
      )}
      noOptionsText="No users found"
    />
  );
};

export default UserSelector;
