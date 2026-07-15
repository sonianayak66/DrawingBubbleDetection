import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  CardActions,
  Button,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Menu,
  MenuItem,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  Grid,
  Pagination,
  Tabs,
  Tab,
  Avatar,
} from '@mui/material';
import {
  Email,
  Task,
  MoreVert,Sync ,
  Visibility,
  Transform,
  Schedule,
  CheckCircle,
  Person,
} from '@mui/icons-material';
import { taskPlannerApi } from '../../../services/api';
import PermissionGuard from '../../shared/components/Common/PermissionGuard';
import TaskDialog from '../components/Tasks/TaskDialog';
import NotificationList from '../../shared/components/Common/NotificationWidget';

const EmailManagementView = () => {
  const [emails, setEmails] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedTab, setSelectedTab] = useState(0); // 0: All, 1: Unconverted, 2: Converted
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [selectedEmail, setSelectedEmail] = useState(null);
  const [menuAnchor, setMenuAnchor] = useState(null);
  const [viewDialogOpen, setViewDialogOpen] = useState(false);
  const [convertDialogOpen, setConvertDialogOpen] = useState(false); 
  const [buckets, setBuckets] = useState([]);
const [syncLoading, setSyncLoading] = useState(false);
  const [taskDialogOpen, setTaskDialogOpen] = useState(false);
  const [taskDialogMode, setTaskDialogMode] = useState('create');
  const [initialTaskData, setInitialTaskData] = useState(null); 
  const [projects, setProjects] = useState([]);

 
  const tabs = ['All Emails', 'Unconverted', 'Converted', 'Notifications'];

  useEffect(() => {
     console.log('Loading emails, selectedTab:', selectedTab, 'page:', page);
    loadEmails();
    loadProjects();
  }, [selectedTab, page]);

 const loadEmails = async () => {
  try {
    setLoading(true);
    const isConverted = selectedTab === 2 ? true : selectedTab === 1 ? false : null;
    
    console.log('Calling getEmails API with params:', {
      isConverted,
      pageSize: 20,
      pageNumber: page
    });
    
    const response = await taskPlannerApi.getEmails({
      isConverted,
      pageSize: 20,
      pageNumber: page
    });

    console.log('API response:', response);
    console.log('Email data:', response.data); // Changed this line
    console.log('Total count:', response.totalCount); // Changed this line

    setEmails(response.data || []); // Fixed: use response.data directly
    setTotalPages(Math.ceil((response.totalCount || 0) / 20)); // Fixed: use response.totalCount
  } catch (error) {
    console.error('Error loading emails:', error);
    console.error('Error details:', error.message);
  } finally {
    setLoading(false);
  }
};
  const loadProjects = async () => {
    try {
      const response = await taskPlannerApi.getProjects();
      setProjects(response.data || []);
    } catch (error) {
      console.error('Error loading projects:', error);
    }
  };

  const loadBuckets = async (projectGuid) => {
    if (!projectGuid) return;
    try {
      const response = await taskPlannerApi.getProjectBuckets(projectGuid);
      setBuckets(response.data || []);
    } catch (error) {
      console.error('Error loading buckets:', error);
    }
  };

  const handleTabChange = (event, newValue) => {
    setSelectedTab(newValue);
    setPage(1);
  };

  const handlePageChange = (event, newPage) => {
    setPage(newPage);
  };

  const handleSyncEmails = async () => {
  try {
    setSyncLoading(true);
    
    console.log('Manual email sync triggered');
    const response = await taskPlannerApi.syncEmailsNow();
    
    if (response.success) {
      console.log('Email sync completed successfully');
      
      // Refresh the email list to show new emails
      await loadEmails();
      
      // Show success message (you can implement a snackbar here)
      alert('Email sync completed successfully!');
    } else {
      console.error('Email sync failed:', response.message);
      alert('Email sync failed: ' + (response.message || 'Unknown error'));
    }
  } catch (error) {
    console.error('Error during manual email sync:', error);
    alert('Error during email sync: ' + error.message);
  } finally {
    setSyncLoading(false);
  }
};

  const handleMenuOpen = (event, email) => {
    event.stopPropagation();
  setMenuAnchor(event.currentTarget);
  setSelectedEmail(email);
  console.log('Menu opened for email:', email); // Debug log
  };

  const handleMenuClose = () => {
   
    setMenuAnchor(null);
    setSelectedEmail(null);
  };

  const handleViewEmail = () => {
    console.log('handleViewEmail called');
  console.log('selectedEmail:', selectedEmail);
    setViewDialogOpen(true); 
    setMenuAnchor(null); 
  };

 const handleConvertToTask = () => {
    if (!selectedEmail) return;
    
    // Prepare initial task data with email information
    const emailTaskData = {
      TaskTitle: selectedEmail.Subject || '',
      TaskDescription: `Email from: ${selectedEmail.FromEmail}\nReceived: ${formatDate(selectedEmail.ReceivedDate)}\n\n--- Original Email Content ---\n${selectedEmail.EmailBodyText || selectedEmail.EmailBodyHtml || ''}`,
      Priority: 'Medium',
      Tags: 'email-conversion',
      // Add any other default values you want
    };

    setInitialTaskData(emailTaskData);
    setTaskDialogMode('create');
    setTaskDialogOpen(true);
    setMenuAnchor(null);
  };


  const handleTaskSaved = async (formData) => {
  try {
    console.log('handleTaskSaved called with:', formData);
    
    // First, create the actual task using the task API
    const taskResponse = await taskPlannerApi.saveTask(formData);
    console.log('Task created:', taskResponse);
    
    // If task creation was successful, then link email to the existing task
    if (selectedEmail && taskResponse?.data.TaskGUID) {
      const conversionData = {
        EmailGUID: selectedEmail.EmailGUID,
        TaskGUID: taskResponse.data.TaskGUID // Pass the existing task GUID
      };

      console.log('Linking email to existing task:', conversionData);
      await taskPlannerApi.convertEmailToTask(conversionData);
      
      // Refresh the email list to reflect the conversion
      loadEmails();
      
      // Show success message
      console.log('Email linked to task successfully!');
    }
  } catch (error) {
    console.error('Error in handleTaskSaved:', error);
    throw error; // Re-throw so TaskDialog can handle the error
  }
};
  

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleString();
  };

  const getEmailStatusChip = (email) => {
    if (email.IsConverted) {
      return (
        <Chip
          icon={<CheckCircle />}
          label="Converted"
          color="success"
          size="small"
        />
      );
    }
    return (
      <Chip
        icon={<Email />}
        label="Pending"
        color="warning"
        size="small"
      />
    );
  };


  // Add this function to reset selectedEmail when dialogs close
const handleCloseEmailDialog = () => {
  setViewDialogOpen(false);
  setSelectedEmail(null); // Reset selectedEmail when dialog closes
};

const handleCloseConvertDialog = () => {
  setConvertDialogOpen(false);
  setSelectedEmail(null); // Reset selectedEmail when dialog closes
};

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
<Box sx={{ mb: 3 }}>
  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
    <Box>
      <Typography variant="h4" gutterBottom>
        Email Management
      </Typography>
      <Typography variant="body1" color="text.secondary">
        Manage and convert emails to tasks
      </Typography>
    </Box>
    
    {/* Sync Button */}
    <PermissionGuard permission="TaskPlanner_Admin">
      <Button
        variant="contained"
        startIcon={<Sync />}
        onClick={handleSyncEmails}
        disabled={syncLoading}
        sx={{ 
          minWidth: 140,
          background: 'linear-gradient(45deg, #2196F3 30%, #21CBF3 90%)',
        }}
      >
        {syncLoading ? 'Syncing...' : 'Sync Emails'}
      </Button>
    </PermissionGuard>
  </Box>
</Box>

      {/* Filter Tabs */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 3 }}>
        <Tabs value={selectedTab} onChange={handleTabChange}>
          {tabs.map((tab, index) => (
            <Tab key={tab} label={tab} />
          ))}
        </Tabs>
      </Box>

    {/* Email List */}
{selectedTab === 3 ? (
  // Notifications Tab - Use the component
  <NotificationList />
) : (
  // Existing Email List Content
  loading ? (
    <Typography>Loading emails...</Typography>
  ) : emails.length === 0 ? (
    <Box sx={{ textAlign: 'center', py: 8 }}>
      <Email sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
      <Typography variant="h6" color="text.secondary" gutterBottom>
        No emails found
      </Typography>
      <Typography variant="body2" color="text.secondary">
        {selectedTab === 1 ? 'All emails have been converted to tasks' : 
         selectedTab === 2 ? 'No emails have been converted yet' : 
         'No emails available'}
      </Typography>
    </Box>
  ) : (
    <>
      <TableContainer component={Paper} elevation={0} sx={{ border: '1px solid', borderColor: 'divider' }}>
        <Table>
          <TableHead>
            <TableRow sx={{ bgcolor: 'action.hover' }}>
              <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
              <TableCell sx={{ fontWeight: 600 }}>Subject</TableCell>
              <TableCell sx={{ fontWeight: 600 }}>From</TableCell>
              <TableCell sx={{ fontWeight: 600 }}>Received</TableCell>
              <TableCell sx={{ fontWeight: 600, width: 80 }}>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {emails.map((email) => (
              <TableRow key={email.EmailGUID} hover>
                <TableCell>
                  {getEmailStatusChip(email)}
                </TableCell>
                <TableCell>
                  <Box>
                    <Typography variant="body1" sx={{ fontWeight: 500 }}>
                      {email.Subject}
                    </Typography>
                    {email.HasAttachments && (
                      <Chip label="📎" size="small" variant="outlined" />
                    )}
                  </Box>
                </TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Avatar sx={{ width: 24, height: 24 }}>
                      <Person fontSize="small" />
                    </Avatar>
                    <Box>
                      <Typography variant="body2">
                        {email.FromName || email.FromEmail}
                      </Typography>
                      {email.FromName && (
                        <Typography variant="caption" color="text.secondary">
                          {email.FromEmail}
                        </Typography>
                      )}
                    </Box>
                  </Box>
                </TableCell>
                <TableCell>
                  <Typography variant="body2">
                    {formatDate(email.ReceivedDate)}
                  </Typography>
                </TableCell>
                <TableCell>
                  <IconButton
                    size="small"
                    onClick={(e) => handleMenuOpen(e, email)}
                  >
                    <MoreVert />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Pagination */}
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
        <Pagination
          count={totalPages}
          page={page}
          onChange={handlePageChange}
          color="primary"
        />
      </Box>
    </>
  )
)}

      {/* Email Actions Menu */}
      <Menu
        anchorEl={menuAnchor}
        open={Boolean(menuAnchor)}
        onClose={handleMenuClose}
      >
        <MenuItem onClick={handleViewEmail}>
          <Visibility sx={{ mr: 1 }} />
          View Email
        </MenuItem>
        {!selectedEmail?.IsConverted && (
          <PermissionGuard permission="TaskPlanner_Tasks_Write">
            <MenuItem onClick={handleConvertToTask}>
              <Transform sx={{ mr: 1 }} />
              Convert to Task
            </MenuItem>
          </PermissionGuard>
        )}
      </Menu>

   {/* View Email Dialog */}
<Dialog
  open={viewDialogOpen}
  onClose={handleCloseEmailDialog}
  maxWidth="md"
  fullWidth
>
  <DialogTitle>
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
      <Email />
      Email Details
    </Box>
  </DialogTitle>
  <DialogContent>
    {selectedEmail ? (
      <Box sx={{ mt: 2 }}>
        {/* Subject */}
        <Box sx={{ mb: 2 }}>
          <Typography variant="subtitle2" gutterBottom>
            Subject
          </Typography>
          <Typography variant="body1" sx={{ p: 1, bgcolor: 'background.default', borderRadius: 1 }}>
            {selectedEmail.Subject || 'No subject'}
          </Typography>
        </Box>

        {/* From */}
        <Box sx={{ mb: 2 }}>
          <Typography variant="subtitle2" gutterBottom>
            From
          </Typography>
          <Typography variant="body1" sx={{ p: 1, bgcolor: 'background.default', borderRadius: 1 }}>
            {selectedEmail.FromName ? `${selectedEmail.FromName} <${selectedEmail.FromEmail}>` : selectedEmail.FromEmail}
          </Typography>
        </Box>

        {/* Received Date */}
        <Box sx={{ mb: 2 }}>
          <Typography variant="subtitle2" gutterBottom>
            Received
          </Typography>
          <Typography variant="body1" sx={{ p: 1, bgcolor: 'background.default', borderRadius: 1 }}>
            {formatDate(selectedEmail.ReceivedDate)}
          </Typography>
        </Box>

        {/* Email Content */}
        <Box sx={{ mb: 2 }}>
          <Typography variant="subtitle2" gutterBottom>
            Email Content
          </Typography>
          <Box
            sx={{
              border: '1px solid',
              borderColor: 'divider',
              borderRadius: 1,
              p: 2,
              maxHeight: 400,
              overflow: 'auto',
              bgcolor: 'background.paper'
            }}
          >
            {selectedEmail.EmailBodyHtml ? (
              <div dangerouslySetInnerHTML={{ __html: selectedEmail.EmailBodyHtml }} />
            ) : (
              <Typography
                component="pre"
                sx={{ 
                  whiteSpace: 'pre-wrap', 
                  fontFamily: 'monospace',
                  fontSize: '0.875rem',
                  margin: 0 
                }}
              >
                {selectedEmail.EmailBodyText || 'No content available'}
              </Typography>
            )}
          </Box>
        </Box>

        {/* Attachments Info */}
        {selectedEmail.HasAttachments && (
          <Box sx={{ mb: 2 }}>
            <Chip
              icon={<Email />}
              label={`${selectedEmail.AttachmentCount || 'Unknown'} Attachment(s)`}
              variant="outlined"
            />
          </Box>
        )}
      </Box>
    ) : (
      <Box sx={{ p: 3, textAlign: 'center' }}>
        <Typography>No email selected</Typography>
      </Box>
    )}
  </DialogContent>
  <DialogActions>
    <Button onClick={handleCloseEmailDialog}>Close</Button>
    {selectedEmail && !selectedEmail.IsConverted && (
      <PermissionGuard permission="TaskPlanner_Tasks_Write">
        <Button
          variant="contained"
          onClick={() => {
            handleCloseEmailDialog();
            handleConvertToTask();
          }}
          startIcon={<Transform />}
        >
          Convert to Task
        </Button>
      </PermissionGuard>
    )}
  </DialogActions>
</Dialog>


<TaskDialog
  open={taskDialogOpen}
  onClose={() => {
    setTaskDialogOpen(false);
    setSelectedEmail(null);
    setInitialTaskData(null);
  }}
  initialData={initialTaskData}
  mode="create"
  onSave={handleTaskSaved}
/>

 
    </Box>
  );
};

export default EmailManagementView;
