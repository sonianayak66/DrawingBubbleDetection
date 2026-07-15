import React, { useState, useEffect } from "react";
import {
  Box,
  Typography,
  Button,
  Tabs,
  Tab,
  Card,
  CardContent,
  CardActions,
  Grid,
  Chip,
  IconButton,
  Menu,
  MenuItem,
  Divider,
  LinearProgress,TableContainer , Paper , Table, TableHead, TableRow, TableCell, TableBody
} from "@mui/material";
import {
  Add,
  MoreVert,
  Edit,
  Delete,
  DragIndicator,
  Task,
  ViewKanban,
  List as ListIcon,
} from "@mui/icons-material";
import { taskPlannerApi } from "../../../services/api";
import PermissionGuard from "../../shared/components/Common/PermissionGuard";
import BucketDialog from "../components/Projects/BucketDialog";
import TaskDialog from '../components/Tasks/TaskDialog';

const ProjectDetailView = ({ projectGuid, projects }) => {
  const [project, setProject] = useState(null);
  const [buckets, setBuckets] = useState([]);
  const [selectedTab, setSelectedTab] = useState(0); // 0: Board, 1: Buckets
  const [loading, setLoading] = useState(true);
  const [menuAnchor, setMenuAnchor] = useState(null);
  const [selectedBucket, setSelectedBucket] = useState(null);

  // State for bucket dialog
  const [bucketDialogOpen, setBucketDialogOpen] = useState(false);
  const [selectedBucketForEdit, setSelectedBucketForEdit] = useState(null);

  // State for task management
  const [tasks, setTasks] = useState([]);
  const [taskDialogOpen, setTaskDialogOpen] = useState(false);
  const [selectedTask, setSelectedTask] = useState(null);

  useEffect(() => {
    if (projectGuid) {
      loadProjectData();
    }
  }, [projectGuid]);

  const loadProjectData = async () => {
    try {
      setLoading(true);

      // Find project in existing data
      const currentProject = projects.find(
        (p) => p.ProjectGUID === projectGuid
      );
      setProject(currentProject);

       // Load buckets and tasks for this project
    const [bucketsResponse, tasksResponse] = await Promise.all([
      taskPlannerApi.getBuckets(projectGuid),
      taskPlannerApi.getTasks(projectGuid)
    ]);

    setBuckets(bucketsResponse.data || []);
    setTasks(tasksResponse.data || []);

    } catch (err) {
      console.error("Error loading project data:", err);
    } finally {
      setLoading(false);
    }
  };

  // Update the handleCreateBucket function
  const handleCreateBucket = () => {
    setSelectedBucketForEdit(null);
    setBucketDialogOpen(true);
  };

  const handleSaveBucket = async (bucketData) => {
    await taskPlannerApi.saveBucket(bucketData);
    await loadProjectData(); // Reload buckets
  };

  const handleEditBucket = (bucket) => {
    setSelectedBucketForEdit(bucket);
    setBucketDialogOpen(true);
    setMenuAnchor(null);
  };

  const handleDeleteBucket = async (bucket) => {
    if (
      window.confirm(
        `Are you sure you want to delete bucket "${bucket.BucketName}"?`
      )
    ) {
      try {
        await taskPlannerApi.deleteBucket(bucket.BucketGUID);
        await loadProjectData(); // Reload buckets
      } catch (err) {
        console.error("Error deleting bucket:", err);
        alert("Error deleting bucket: " + err.message);
      }
    }
    setMenuAnchor(null);
  };


  // Add task management functions
const handleCreateTask = () => {
  setSelectedTask(null);
  setTaskDialogOpen(true);
};

const handleEditTask = (task) => {
  setSelectedTask(task);
  setTaskDialogOpen(true);
};

const handleSaveTask = async (taskData) => {
  const result = await taskPlannerApi.saveTask(taskData);
  await loadProjectData(); // Reload tasks and buckets
  return result;
};

const handleDeleteTask = async (task) => {
  if (window.confirm(`Are you sure you want to delete task "${task.TaskTitle}"?`)) {
    try {
      await taskPlannerApi.deleteTask(task.TaskGUID);
      await loadProjectData(); // Reload tasks
    } catch (err) {
      console.error('Error deleting task:', err);
      alert('Error deleting task: ' + err.message);
    }
  }
};

  const handleMenuOpen = (event, bucket) => {
    setMenuAnchor(event.currentTarget);
    setSelectedBucket(bucket);
  };

  const handleMenuClose = () => {
    setMenuAnchor(null);
    setSelectedBucket(null);
  };

  const getBucketColor = (color) => {
    return color || "#1976d2"; // Default blue if no color set
  };

  if (loading) {
    return (
      <Box sx={{ p: 3 }}>
        <LinearProgress />
        <Typography sx={{ mt: 2 }}>Loading project...</Typography>
      </Box>
    );
  }

  if (!project) {
    return (
      <Box sx={{ p: 3, textAlign: "center" }}>
        <Typography variant="h6" color="error">
          Project not found
        </Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      {/* Project Header */}
      <Box sx={{ mb: 3 }}>
        <Typography variant="h4" gutterBottom>
          {project.ProjectName}
        </Typography>
        {project.ProjectDescription && (
          <Typography variant="body1" color="text.secondary" gutterBottom>
            {project.ProjectDescription}
          </Typography>
        )}
        <Box sx={{ display: "flex", gap: 1, mt: 2 }}>
          <Chip label={project.ProjectStatus} color="primary" size="small" />
          {project.Priority && (
            <Chip label={project.Priority} variant="outlined" size="small" />
          )}
        </Box>
      </Box>

      <Divider sx={{ mb: 3 }} />

      {/* View Tabs */}
      <Box sx={{ borderBottom: 1, borderColor: "divider", mb: 3 }}>
        <Tabs
          value={selectedTab}
          onChange={(e, newValue) => setSelectedTab(newValue)}
        >
          <Tab icon={<ViewKanban />} label="Board" />
          <Tab icon={<ListIcon />} label="Buckets" />
        </Tabs>
      </Box>

      {/* Content based on selected tab */}
      {selectedTab === 0 && (
  // Board View - Simple task list for now
  <Box>
    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
      <Typography variant="h6">
        Tasks ({tasks.length})
      </Typography>
      <PermissionGuard permission="TaskPlanner_Tasks_Write">
        <Button
          variant="contained"
          startIcon={<Add />}
          onClick={handleCreateTask}
        >
          Add Task
        </Button>
      </PermissionGuard>
    </Box>

    {tasks.length === 0 ? (
      <Box sx={{ textAlign: 'center', py: 8 }}>
        <Task sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
        <Typography variant="h6" color="text.secondary" gutterBottom>
          No tasks found
        </Typography>
        <Typography variant="body2" color="text.secondary" gutterBottom>
          Create your first task to get started
        </Typography>
        <PermissionGuard permission="TaskPlanner_Tasks_Write">
          <Button variant="outlined" onClick={handleCreateTask} sx={{ mt: 2 }}>
            Create your first task
          </Button>
        </PermissionGuard>
      </Box>
    ) : (
      <TableContainer component={Paper} elevation={0} sx={{ border: '1px solid', borderColor: 'divider' }}>
        <Table>
          <TableHead>
            <TableRow sx={{ bgcolor: 'action.hover' }}>
              <TableCell sx={{ fontWeight: 600 }}>Task</TableCell>
              <TableCell sx={{ fontWeight: 600 }}>Bucket</TableCell>
              <TableCell sx={{ fontWeight: 600 }}>Priority</TableCell>
              <TableCell sx={{ fontWeight: 600 }}>Due Date</TableCell>
              <TableCell sx={{ fontWeight: 600 }}>Progress</TableCell>
              <TableCell sx={{ fontWeight: 600, width: 80 }}>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {tasks.map((task) => {
              const bucket = buckets.find(b => b.BucketGUID === task.BucketGUID);
              return (
                <TableRow key={task.TaskGUID} hover>
                  <TableCell>
                    <Box>
                      <Typography variant="body1" sx={{ fontWeight: 500 }}>
                        {task.TaskTitle}
                      </Typography>
                      {task.TaskDescription && (
                        <Typography variant="body2" color="text.secondary">
                          {task.TaskDescription.length > 60 
                            ? `${task.TaskDescription.substring(0, 60)}...`
                            : task.TaskDescription
                          }
                        </Typography>
                      )}
                    </Box>
                  </TableCell>
                  
                  <TableCell>
                    {bucket && (
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Box
                          sx={{
                            width: 12,
                            height: 12,
                            backgroundColor: bucket.BucketColor,
                            borderRadius: '50%'
                          }}
                        />
                        <Typography variant="body2">{bucket.BucketName}</Typography>
                      </Box>
                    )}
                  </TableCell>
                  
                  <TableCell>
                    <Chip 
                      label={task.Priority} 
                      size="small"
                      color={
                        task.Priority === 'Critical' ? 'error' :
                        task.Priority === 'High' ? 'warning' :
                        task.Priority === 'Medium' ? 'primary' : 'default'
                      }
                    />
                  </TableCell>
                  
                  <TableCell>
                    {task.DueDate && (
                      <Typography variant="body2">
                        {new Date(task.DueDate).toLocaleDateString()}
                      </Typography>
                    )}
                  </TableCell>
                  
                  <TableCell>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <LinearProgress 
                        variant="determinate" 
                        value={task.ProgressPercentage || 0} 
                        sx={{ flexGrow: 1, height: 6, borderRadius: 3 }}
                      />
                      <Typography variant="caption">
                        {task.ProgressPercentage || 0}%
                      </Typography>
                    </Box>
                  </TableCell>
                  
                  <TableCell>
                    <PermissionGuard permission="TaskPlanner_Tasks_Write">
                      <IconButton 
                        size="small" 
                        onClick={() => handleEditTask(task)}
                      >
                        <Edit />
                      </IconButton>
                    </PermissionGuard>
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </TableContainer>
    )}
  </Box>
)}

      {selectedTab === 1 && (
        // Buckets Management View
        <Box>
          <Box
            sx={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
              mb: 3,
            }}
          >
            <Typography variant="h6">
              Project Buckets ({buckets.length})
            </Typography>
            <PermissionGuard permission="TaskPlanner_Projects_Write">
              <Button
                variant="contained"
                startIcon={<Add />}
                onClick={handleCreateBucket}
              >
                Add Bucket
              </Button>
            </PermissionGuard>
          </Box>

          {buckets.length === 0 ? (
            <Box sx={{ textAlign: "center", py: 8 }}>
              <Task sx={{ fontSize: 64, color: "text.secondary", mb: 2 }} />
              <Typography variant="h6" color="text.secondary" gutterBottom>
                No buckets found
              </Typography>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Create buckets to organize your tasks (e.g., "To Do", "In
                Progress", "Done")
              </Typography>
              <PermissionGuard permission="TaskPlanner_Projects_Write">
                <Button
                  variant="outlined"
                  onClick={handleCreateBucket}
                  sx={{ mt: 2 }}
                >
                  Create your first bucket
                </Button>
              </PermissionGuard>
            </Box>
          ) : (
            // Replace the Grid container section with this table:
            <TableContainer
              component={Paper}
              elevation={0}
              sx={{ border: "1px solid", borderColor: "divider" }}
            >
              <Table>
                <TableHead>
                  <TableRow sx={{ bgcolor: "action.hover" }}>
                    <TableCell sx={{ fontWeight: 600, width: 50 }}>
                      Color
                    </TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Bucket Name</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Description</TableCell>
                    <TableCell sx={{ fontWeight: 600, width: 120 }}>
                      Sort Order
                    </TableCell>
                    <TableCell sx={{ fontWeight: 600, width: 80 }}>
                      Actions
                    </TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {buckets
                    .sort((a, b) => (a.SortOrder || 0) - (b.SortOrder || 0)) // Sort by SortOrder
                    .map((bucket, index) => (
                      <TableRow key={bucket.BucketGUID} hover>
                        <TableCell>
                          <Box
                            sx={{
                              width: 20,
                              height: 20,
                              backgroundColor: getBucketColor(
                                bucket.BucketColor
                              ),
                              borderRadius: "50%",
                              border: "2px solid",
                              borderColor: "divider",
                            }}
                          />
                        </TableCell>

                        <TableCell>
                          <Typography variant="body1" sx={{ fontWeight: 500 }}>
                            {bucket.BucketName}
                          </Typography>
                        </TableCell>

                        <TableCell>
                          <Typography variant="body2" color="text.secondary">
                            {bucket.BucketDescription || "-"}
                          </Typography>
                        </TableCell>

                        <TableCell>
                          <Chip
                            label={bucket.SortOrder || index + 1}
                            size="small"
                            variant="outlined"
                          />
                        </TableCell>

                        <TableCell>
                          <PermissionGuard permission="TaskPlanner_Projects_Write">
                            <IconButton
                              size="small"
                              onClick={(e) => handleMenuOpen(e, bucket)}
                            >
                              <MoreVert />
                            </IconButton>
                          </PermissionGuard>
                        </TableCell>
                      </TableRow>
                    ))}
                </TableBody>
              </Table>
            </TableContainer>
          )} 
        </Box>
      )}

      {/* Context Menu */}
      <Menu
        anchorEl={menuAnchor}
        open={Boolean(menuAnchor)}
        onClose={handleMenuClose}
      >
        <PermissionGuard permission="TaskPlanner_Projects_Write">
          <MenuItem onClick={() => handleEditBucket(selectedBucket)}>
            <Edit fontSize="small" sx={{ mr: 1 }} />
            Edit
          </MenuItem>
        </PermissionGuard>
        <PermissionGuard permission="TaskPlanner_Projects_Delete">
          <MenuItem onClick={() => handleDeleteBucket(selectedBucket)}>
            <Delete fontSize="small" sx={{ mr: 1 }} />
            Delete
          </MenuItem>
        </PermissionGuard>
      </Menu>

       <TaskDialog
      open={taskDialogOpen}
      onClose={() => setTaskDialogOpen(false)}
      task={selectedTask}
      projectGuid={projectGuid}
      buckets={buckets}
      onSave={handleSaveTask}
    />

    {/* Bucket Dialog - Move here too for consistency */}
    <BucketDialog
      open={bucketDialogOpen}
      onClose={() => setBucketDialogOpen(false)}
      bucket={selectedBucketForEdit}
      projectGuid={projectGuid}
      onSave={handleSaveBucket}
    />
    
    </Box>
  );
};

export default ProjectDetailView;
