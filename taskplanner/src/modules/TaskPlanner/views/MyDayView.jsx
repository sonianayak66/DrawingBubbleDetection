import React, { useState, useEffect } from "react";
import {
  Box,
  Typography,
  Button,
  Card,
  CardContent,
  Divider,
  Chip,
  IconButton,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Paper,
  Avatar,
  LinearProgress,
  TextField,
  InputAdornment,
} from "@mui/material";
import {
  Add,
  WbSunny,
  Task,
  Schedule,
  TrendingUp,
  CheckCircle,
  RadioButtonUnchecked,
  Search,
  Star,
  Warning,
} from "@mui/icons-material";
import { taskPlannerApi } from "../../../services/api";
import TaskDialog from "../components/Tasks/TaskDialog";
import PermissionGuard from "../../shared/components/Common/PermissionGuard";
import { myDayStorage } from "../../../utils/myDayStorage";
import { useUser } from '../../../context/UserContext'; // Add this impor

const MyDayView = () => {
  const [myDayTasks, setMyDayTasks] = useState([]);
  const [overdueTasks, setOverdueTasks] = useState([]);
  const [upcomingTasks, setUpcomingTasks] = useState([]);
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [taskDialogOpen, setTaskDialogOpen] = useState(false);
  const [quickTaskText, setQuickTaskText] = useState("");
  const [selectedTask, setSelectedTask] = useState(null);
  const {getCurrentUserName } = useUser();

  // Add state for buckets and quick task pre-fill
  const [buckets, setBuckets] = useState([]);
  const [quickTaskForDialog, setQuickTaskForDialog] = useState(null);

  useEffect(() => {
    loadMyDayData();
  }, []);

  const loadMyDayData1 = async () => {
    try {
      setLoading(true);

      const [tasksResponse, projectsResponse, bucketsResponse] =
        await Promise.all([
          taskPlannerApi.getTasks(),
          taskPlannerApi.getProjects(),
          taskPlannerApi.getBuckets(), // Get all buckets
        ]);

      const allTasks = tasksResponse.data || [];
      setProjects(projectsResponse.data || []);
      setBuckets(bucketsResponse.data || []); // Store buckets

      const today = new Date();
      today.setHours(0, 0, 0, 0);

      const tomorrow = new Date(today);
      tomorrow.setDate(tomorrow.getDate() + 1);

      const yesterday = new Date(today);
      yesterday.setDate(yesterday.getDate() - 1);

      // My Day tasks - due today or manually added to My Day
      const todayTasks = allTasks.filter((task) => {
        if (task.IsInMyDay) return true; // Manually added to My Day

        if (task.DueDate) {
          const dueDate = new Date(task.DueDate);
          dueDate.setHours(0, 0, 0, 0);
          return dueDate.getTime() === today.getTime();
        }

        return false;
      });

      // Overdue tasks
      const overdue = allTasks.filter((task) => {
        if (task.ProgressPercentage === 100) return false; // Completed
        if (!task.DueDate) return false;

        const dueDate = new Date(task.DueDate);
        dueDate.setHours(23, 59, 59, 999);
        return dueDate < today;
      });

      // Upcoming tasks (next 7 days)
      const nextWeek = new Date(today);
      nextWeek.setDate(nextWeek.getDate() + 7);

      const upcoming = allTasks
        .filter((task) => {
          if (!task.DueDate) return false;

          const dueDate = new Date(task.DueDate);
          dueDate.setHours(0, 0, 0, 0);

          return dueDate > today && dueDate <= nextWeek;
        })
        .slice(0, 5); // Limit to 5 upcoming tasks

      setMyDayTasks(todayTasks);
      setOverdueTasks(overdue.slice(0, 3)); // Limit to 3 overdue
      setUpcomingTasks(upcoming);
    } catch (err) {
      console.error("Error loading My Day data:", err);
    } finally {
      setLoading(false);
    }
  };

  const handleEditTask = (task) => {
    setQuickTaskForDialog(task);
    
    setTaskDialogOpen(true);
  };

  const handleTaskComplete = async (task) => {
    try {
      const updatedTask = {
        ...task,
        ProgressPercentage: task.ProgressPercentage === 100 ? 0 : 100,
      };
      await taskPlannerApi.saveTask(updatedTask);
      await loadMyDayData();
    } catch (err) {
      console.error("Error updating task completion:", err);
    }
  };

  const handleQuickAddTask = async () => {
    if (!quickTaskText.trim()) {
      console.log("No text, returning early");
      return;
    }
    try {
      // Find "To Do" bucket from all buckets
      const todoeBucket = buckets.find(
        (b) => b.BucketName?.toLowerCase() === "to do"
      );

      // Create pre-filled task data
      const quickTaskData = {
        TaskGUID: null, // New task, no GUID yet
        TaskTitle: quickTaskText,
        BucketGUID: todoeBucket?.BucketGUID || null,
        Priority: "Medium",
        DueDate: new Date(),
        StartDate: new Date(),
        IsInMyDay: true,
        ProgressPercentage: 0,
      };

      setQuickTaskForDialog(quickTaskData);
      setTaskDialogOpen(true);
      setQuickTaskText("");
    } catch (err) {
      console.error("Error preparing quick task:", err);
    }
  };

  const getGreeting = () => {
    const userName = getCurrentUserName();
    const hour = new Date().getHours();
    if (hour < 12) return "Good morning, " + userName;
    if (hour < 17) return "Good afternoon, " + userName;
    return "Good evening, "+ userName;
  };

  const getProjectName = (projectGuid) => {
    const project = projects.find((p) => p.ProjectGUID === projectGuid);
    return project ? project.ProjectName : "Unknown Project";
  };

  const getProjectColor = (projectGuid) => {
    const colors = [
      "#1976d2",
      "#388e3c",
      "#f57c00",
      "#d32f2f",
      "#7b1fa2",
      "#616161",
    ];
    const hash = projectGuid
      ? projectGuid.split("").reduce((a, b) => {
          a = (a << 5) - a + b.charCodeAt(0);
          return a & a;
        }, 0)
      : 0;
    return colors[Math.abs(hash) % colors.length];
  };

  const formatDate = (date) => {
    return new Date(date).toLocaleDateString("en-US", {
      weekday: "long",
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  };

  const getCurrentUserId = () => {
    // Get from your auth system - for now return a placeholder
    return 1018; // Replace with actual current user ID logic
  };

  // Update loadMyDayData function
  const loadMyDayData = async () => {
    try {
      setLoading(true);

      const [tasksResponse, projectsResponse, bucketsResponse] =
        await Promise.all([
          taskPlannerApi.getTasks(),
          taskPlannerApi.getProjects(),
          taskPlannerApi.getBuckets(),
        ]);

      const allTasks = tasksResponse.data || [];
      setProjects(projectsResponse.data || []);
      setBuckets(bucketsResponse.data || []);

      const today = new Date();
      today.setHours(0, 0, 0, 0);

      const currentUserId = getCurrentUserId();
      const myDayTaskGuids = myDayStorage.getMyDayTasks(currentUserId);

      // My Day tasks - due today OR manually added to My Day
      const todayTasks = allTasks.filter((task) => {
        // Check if manually added to My Day
        if (myDayTaskGuids.includes(task.TaskGUID)) return true;

        // Check if due today
        if (task.DueDate) {
          const dueDate = new Date(task.DueDate);
          dueDate.setHours(0, 0, 0, 0);
          return dueDate.getTime() === today.getTime();
        }

        return false;
      });

      // Overdue tasks (not completed, past due date)
      const overdue = allTasks.filter((task) => {
        if (task.ProgressPercentage === 100) return false;
        if (!task.DueDate) return false;

        const dueDate = new Date(task.DueDate);
        dueDate.setHours(23, 59, 59, 999);
        return dueDate < today;
      });

      // Upcoming tasks (next 7 days)
      const nextWeek = new Date(today);
      nextWeek.setDate(nextWeek.getDate() + 7);

      const upcoming = allTasks
        .filter((task) => {
          if (!task.DueDate) return false;

          const dueDate = new Date(task.DueDate);
          dueDate.setHours(0, 0, 0, 0);

          return dueDate > today && dueDate <= nextWeek;
        })
        .slice(0, 5);

      setMyDayTasks(todayTasks);
      setOverdueTasks(overdue.slice(0, 3));
      setUpcomingTasks(upcoming);
    } catch (err) {
      console.error("Error loading My Day data:", err);
    } finally {
      setLoading(false);
    }
  };

  // Update handleAddToMyDay function
  const handleAddToMyDay = async (task) => {
    try {
      const currentUserId = getCurrentUserId();
      const success = myDayStorage.addToMyDay(currentUserId, task.TaskGUID);

      if (success) {
        await loadMyDayData(); // Refresh the view
      } else {
        alert("Error adding task to My Day");
      }
    } catch (err) {
      console.error("Error adding task to My Day:", err);
    }
  };

  // Update handleRemoveFromMyDay function
  const handleRemoveFromMyDay = async (task) => {
    try {
      const currentUserId = getCurrentUserId();
      const success = myDayStorage.removeFromMyDay(
        currentUserId,
        task.TaskGUID
      );

      if (success) {
        await loadMyDayData(); // Refresh the view
      } else {
        alert("Error removing task from My Day");
      }
    } catch (err) {
      console.error("Error removing task from My Day:", err);
    }
  };

  if (loading) {
    return (
      <Box sx={{ p: 3 }}>
        <Typography>Loading your day...</Typography>
      </Box>
    );
  }

  const completedCount = myDayTasks.filter(
    (t) => t.ProgressPercentage === 100
  ).length;
  const totalCount = myDayTasks.length;

  return (
    <Box sx={{ p: 3, maxWidth: 1200, mx: "auto" }}>
      {/* Header Section */}
      <Box sx={{ mb: 4 }}>
        <Box sx={{ display: "flex", alignItems: "center", gap: 2, mb: 2 }}>
          <WbSunny sx={{ fontSize: 32, color: "#f57c00" }} />
          <Box>
            <Typography variant="h4" sx={{ fontWeight: 600, mb: 0.5 }}>
              {getGreeting()}!
            </Typography>
            <Typography variant="h6" color="text.secondary">
              {formatDate(new Date())}
            </Typography>
          </Box>
        </Box>

        {/* Progress Summary */}
        {totalCount > 0 && (
          <Box sx={{ mt: 3 }}>
            <Box sx={{ display: "flex", alignItems: "center", gap: 2, mb: 1 }}>
              <Typography variant="body1">
                Today's Progress: {completedCount} of {totalCount} tasks
                completed
              </Typography>
              <Chip
                label={`${Math.round((completedCount / totalCount) * 100)}%`}
                color="primary"
                size="small"
              />
            </Box>
            <LinearProgress
              variant="determinate"
              value={(completedCount / totalCount) * 100}
              sx={{ height: 8, borderRadius: 4 }}
            />
          </Box>
        )}
      </Box>

      <Box sx={{ display: "flex", gap: 3, flexWrap: "wrap" }}>
        {/* Main Content - My Day Tasks */}
        <Box sx={{ flex: 2, minWidth: 400 }}>
          {/* Quick Add Task */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <TextField
                fullWidth
                placeholder="Add a task to My Day..."
                value={quickTaskText}
                onChange={(e) => setQuickTaskText(e.target.value)}
                onKeyPress={(e) => e.key === "Enter" && handleQuickAddTask()}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Add />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <InputAdornment position="end">
                      <PermissionGuard permission="TaskPlanner_Tasks_Write">
                        <Button
                          variant="contained"
                          size="small"
                          onClick={() => handleQuickAddTask()}
                        >
                          Add Task
                        </Button>
                      </PermissionGuard>
                    </InputAdornment>
                  ),
                }}
              />
            </CardContent>
          </Card>

          {/* My Day Tasks */}
          <Card>
            <CardContent>
              <Typography
                variant="h6"
                sx={{ mb: 2, display: "flex", alignItems: "center", gap: 1 }}
              >
                <Task />
                My Day Tasks ({myDayTasks.length})
              </Typography>

              {myDayTasks.length === 0 ? (
                <Box sx={{ textAlign: "center", py: 4 }}>
                  <Task sx={{ fontSize: 48, color: "text.secondary", mb: 2 }} />
                  <Typography
                    variant="body1"
                    color="text.secondary"
                    gutterBottom
                  >
                    No tasks planned for today
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Add tasks above or drag them from the suggestions
                  </Typography>
                </Box>
              ) : (
                <List>
                  {myDayTasks.map((task, index) => (
                    <React.Fragment key={task.TaskGUID}>
                      <ListItem
                        sx={{
                          px: 0,
                          cursor: "pointer",
                          "&:hover": {
                            backgroundColor: "action.hover",
                            borderRadius: 1,
                          },
                        }}
                        onClick={() => handleEditTask(task)} // Add click handler
                      >
                        <IconButton
                          onClick={(e) => {
                            e.stopPropagation(); // Prevent triggering the list item click
                            handleTaskComplete(task);
                          }}
                          sx={{ mr: 2 }}
                        >
                          {task.ProgressPercentage === 100 ? (
                            <CheckCircle color="success" />
                          ) : (
                            <RadioButtonUnchecked />
                          )}
                        </IconButton>

                        <ListItemText
                          primary={
                            <Box>
                              <Typography
                                variant="body1"
                                sx={{
                                  textDecoration:
                                    task.ProgressPercentage === 100
                                      ? "line-through"
                                      : "none",
                                  fontWeight: 500,
                                  mb: 0.5,
                                }}
                              >
                                {task.TaskTitle}
                              </Typography>
                              <Box
                                sx={{
                                  display: "flex",
                                  alignItems: "center",
                                  gap: 1,
                                }}
                              >
                                <Box
                                  sx={{
                                    width: 8,
                                    height: 8,
                                    backgroundColor: getProjectColor(
                                      task.ProjectGUID
                                    ),
                                    borderRadius: "50%",
                                  }}
                                />
                                <Typography variant="caption">
                                  {getProjectName(task.ProjectGUID)}
                                </Typography>
                                {task.Priority !== "Medium" && (
                                  <Chip
                                    label={task.Priority}
                                    size="small"
                                    variant="outlined"
                                    color={
                                      task.Priority === "Critical"
                                        ? "error"
                                        : task.Priority === "High"
                                        ? "warning"
                                        : "default"
                                    }
                                  />
                                )}
                              </Box>
                            </Box>
                          }
                        />

                        <ListItemSecondaryAction>
                          <IconButton
                            size="small"
                            onClick={(e) => {
                              e.stopPropagation(); // Prevent triggering the list item click
                              handleRemoveFromMyDay(task);
                            }}
                            title="Remove from My Day"
                          >
                            <Star />
                          </IconButton>
                        </ListItemSecondaryAction>
                      </ListItem>
                      {index < myDayTasks.length - 1 && <Divider />}
                    </React.Fragment>
                  ))}
                </List>
              )}
            </CardContent>
          </Card>
        </Box>

        {/* Sidebar - Suggestions */}
        <Box sx={{ flex: 1, minWidth: 300 }}>
          {/* Overdue Tasks */}
          {overdueTasks.length > 0 && (
            <Card sx={{ mb: 3 }}>
              <CardContent>
                <Typography
                  variant="h6"
                  sx={{
                    mb: 2,
                    display: "flex",
                    alignItems: "center",
                    gap: 1,
                    color: "error.main",
                  }}
                >
                  <Warning />
                  Overdue ({overdueTasks.length})
                </Typography>
                <List dense>
                  {overdueTasks.map((task) => (
                    <ListItem key={task.TaskGUID} sx={{ px: 0 }}>
                      <ListItemText
                        primary={task.TaskTitle}
                        secondary={`Due: ${new Date(
                          task.DueDate
                        ).toLocaleDateString()}`}
                      />
                      <Button
                        size="small"
                        onClick={() => handleAddToMyDay(task)}
                      >
                        Add
                      </Button>
                    </ListItem>
                  ))}
                </List>
              </CardContent>
            </Card>
          )}

          {/* Upcoming Tasks */}
          {upcomingTasks.length > 0 && (
            <Card>
              <CardContent>
                <Typography
                  variant="h6"
                  sx={{ mb: 2, display: "flex", alignItems: "center", gap: 1 }}
                >
                  <Schedule />
                  Upcoming
                </Typography>
                <List dense>
                  {upcomingTasks.map((task) => (
                    <ListItem key={task.TaskGUID} sx={{ px: 0 }}>
                      <ListItemText
                        primary={task.TaskTitle}
                        secondary={`Due: ${new Date(
                          task.DueDate
                        ).toLocaleDateString()}`}
                      />
                      <Button
                        size="small"
                        onClick={() => handleAddToMyDay(task)}
                      >
                        Add
                      </Button>
                    </ListItem>
                  ))}
                </List>
              </CardContent>
            </Card>
          )}
        </Box>
      </Box>

      <TaskDialog
        open={taskDialogOpen}
        onClose={() => {
          setTaskDialogOpen(false);
          setQuickTaskForDialog(null);
        }}
        task={quickTaskForDialog}
        projectGuid={quickTaskForDialog?.ProjectGUID}
        buckets={buckets} // Pass all buckets since we don't have a specific project
        onSave={async (taskData) => {
          console.log("Saving task:", taskData);
          const result = await taskPlannerApi.saveTask({
            ...taskData,
            IsInMyDay: true,
          });
          await loadMyDayData();
          return result;
        }}
      />
    </Box>
  );
};

export default MyDayView;
