import React, { useState, useEffect } from "react";
import {
  Box,
  Typography,
  Tabs,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  TextField,
  InputAdornment,
  ToggleButtonGroup,
  ToggleButton,
  Chip,
  LinearProgress,
  IconButton,
  Button,
  Menu,
  MenuItem,
  Avatar,
  AvatarGroup,
} from "@mui/material";
import {
  List as ListIcon,
  ViewKanban,
  Add,
  FilterList,
  Sort,
  Assignment,
  Edit,
  Search,
  CalendarToday,
  Lock,
} from "@mui/icons-material";
import {
  DndContext,
  DragOverlay,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
} from "@dnd-kit/core";
import {
  SortableContext,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
} from "@dnd-kit/sortable";
import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";

// Add useDroppable import
import { useDroppable } from "@dnd-kit/core";
import { taskPlannerApi } from "../../../services/api";
import PermissionGuard from "../../shared/components/Common/PermissionGuard";
import TaskDialog from "../components/Tasks/TaskDialog";
//import TaskDialog from '../components/Tasks/TaskDialogAccordion';

const KanbanColumn = ({
  status,
  statusColor,
  tasks,
  projects,
  getProjectName,
  getProjectColor,
  onTaskClick,
}) => {
  const { setNodeRef, isOver } = useDroppable({
    id: status,
  });

  return (
    <Box
      ref={setNodeRef}
      sx={{
        flex: 1,
        minWidth: 300,
        backgroundColor: isOver ? "action.hover" : "background.paper",
        borderRadius: 2,
        p: 2,
        border: "1px solid",
        borderColor: "divider",
        transition: "background-color 0.2s",
      }}
    >
      {/* Column Header */}
      <Box sx={{ mb: 2 }}>
        <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
          <Box
            sx={{
              width: 12,
              height: 12,
              backgroundColor: statusColor, // Use dynamic color
              borderRadius: "50%",
            }}
          />
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            {status}
          </Typography>
          <Chip label={tasks.length} size="small" variant="outlined" />
        </Box>
      </Box>

      {/* Task Cards */}
      <SortableContext
        items={tasks.map((t) => t.TaskGUID)}
        strategy={verticalListSortingStrategy}
      >
        <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
          {tasks.map((task) => (
            <DraggableTaskCard
              key={task.TaskGUID}
              task={task}
              projectName={getProjectName(task.ProjectGUID)}
              projectColor={getProjectColor(task.ProjectGUID)}
              onClick={() => onTaskClick(task)}
            />
          ))}

          {tasks.length === 0 && (
            <Box
              sx={{
                p: 3,
                textAlign: "center",
                color: "text.secondary",
                border: "2px dashed",
                borderColor: "divider",
                borderRadius: 2,
                minHeight: 100,
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
              }}
            >
              <Typography variant="body2">Drop tasks here</Typography>
            </Box>
          )}
        </Box>
      </SortableContext>
    </Box>
  );
};

const DraggableTaskCard = ({ task, projectName, projectColor, onClick }) => {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: task.TaskGUID });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  return (
    <div ref={setNodeRef} style={style} {...attributes} {...listeners}>
      <TaskCard
        task={task}
        projectName={projectName}
        projectColor={projectColor}
        onClick={onClick}
        isDragging={isDragging}
      />
    </div>
  );
};

const TaskCard = ({
  task,
  projectName,
  projectColor,
  onClick,
  isDragging = false,
}) => {
  // Helper function to generate avatar color based on name
  const getAvatarColor = (name) => {
    const colors = [
      "#1976d2",
      "#388e3c",
      "#f57c00",
      "#d32f2f",
      "#7b1fa2",
      "#616161",
      "#0288d1",
      "#689f38",
    ];
    const hash = name
      ? name.split("").reduce((a, b) => {
          a = (a << 5) - a + b.charCodeAt(0);
          return a & a;
        }, 0)
      : 0;
    return colors[Math.abs(hash) % colors.length];
  };

  // Parse assigned users (assuming comma-separated string from backend)
  const assignedUsers = task.AssignedUsers
    ? task.AssignedUsers.split(", ").filter((name) => name.trim())
    : [];
  const maxAvatars = 3;
  const visibleUsers = assignedUsers.slice(0, maxAvatars);
  const extraUsersCount = Math.max(0, assignedUsers.length - maxAvatars);

  return (
    <Paper
      elevation={isDragging ? 8 : 2}
      sx={{
        p: 1.5,
        cursor: isDragging ? "grabbing" : "pointer",
        opacity: isDragging ? 0.8 : 1,
        borderRadius: 2,
        position: "relative",
        border: "1px solid",
        borderColor: "divider",
        "&:hover": {
          elevation: 4,
          backgroundColor: "action.hover",
          borderColor: "primary.light",
          transform: "translateY(-2px)",
          transition: "all 0.2s ease-in-out",
        },
        transition: "all 0.2s ease-in-out",
      }}
      onClick={!isDragging ? onClick : undefined}
    >
      {/* Private Task Indicator */}
      {task.IsPrivate && (
        <Box
          sx={{
            position: "absolute",
            top: 8,
            right: 8,
            color: "text.secondary",
          }}
        >
          <Lock fontSize="small" />
        </Box>
      )}

      {/* Priority Bar */}
      <Box
        sx={{
          width: "100%",
          height: 4,
          backgroundColor:
            task.Priority === "Critical"
              ? "#d32f2f"
              : task.Priority === "High"
              ? "#f57c00"
              : task.Priority === "Medium"
              ? "#1976d2"
              : "#616161",
          borderRadius: 2,
          mb: 2,
        }}
      />

      {/* Task Title */}
      <Typography
        variant="body1"
        sx={{
          fontWeight: 600,
          mb: 1,
          pr: task.IsPrivate ? 3 : 0, // Add padding if private icon exists
          lineHeight: 1.3,
        }}
      >
        {task.TaskTitle}
      </Typography>

      {/* Task Description */}
      {task.TaskDescription && (
        <Typography
          variant="body2"
          color="text.secondary"
          sx={{
            mb: 2,
            display: "-webkit-box",
            WebkitLineClamp: 2,
            WebkitBoxOrient: "vertical",
            overflow: "hidden",
            lineHeight: 1.4,
          }}
        >
          {task.TaskDescription}
        </Typography>
      )}

      {/* Project Indicator */}
      <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 2 }}>
        <Box
          sx={{
            width: 8,
            height: 8,
            backgroundColor: projectColor,
            borderRadius: "50%",
          }}
        />
        <Typography
          variant="caption"
          color="text.secondary"
          sx={{ fontWeight: 500 }}
        >
          {projectName}
        </Typography>
      </Box>

      {/* Progress Bar */}
      {task.ProgressPercentage > 0 && (
        <Box sx={{ mb: 2 }}>
          <Box
            sx={{
              display: "flex",
              alignItems: "center",
              justifyContent: "space-between",
              mb: 0.5,
            }}
          >
            <Typography variant="caption" color="text.secondary">
              Progress
            </Typography>
            <Typography variant="caption" sx={{ fontWeight: 600 }}>
              {task.ProgressPercentage}%
            </Typography>
          </Box>
          <LinearProgress
            variant="determinate"
            value={task.ProgressPercentage}
            sx={{
              height: 6,
              borderRadius: 3,
              backgroundColor: "action.hover",
              "& .MuiLinearProgress-bar": {
                borderRadius: 3,
              },
            }}
          />
        </Box>
      )}

      {/* Bottom Section: Due Date, Created By, Assigned Users */}
      <Box sx={{ mt: 2 }}>
        {/* Due Date */}
        {task.DueDate && (
          <Box
            sx={{ display: "flex", alignItems: "center", gap: 0.5, mb: 1.5 }}
          >
            <CalendarToday sx={{ fontSize: 14, color: "text.secondary" }} />
            <Typography
              variant="caption"
              color={
                new Date(task.DueDate) < new Date() ? "error" : "text.secondary"
              }
              sx={{ fontWeight: 500 }}
            >
              Due {new Date(task.DueDate).toLocaleDateString()}
            </Typography>
          </Box>
        )}

        {/* Bottom Row: Created By and Assigned Users */}
        <Box
          sx={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          {/* Created By */}
          <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
            <Avatar
              sx={{
                width: 24,
                height: 24,
                fontSize: "0.75rem",
                backgroundColor: getAvatarColor(task.CreatedByName),
              }}
            >
              {task.CreatedByName?.charAt(0) || "U"}
            </Avatar>
            <Typography variant="caption" color="text.secondary">
              {task.CreatedByName}
            </Typography>
          </Box>

          {/* Assigned Users */}
          {assignedUsers.length > 0 && (
            <Box sx={{ display: "flex", alignItems: "center", gap: 0.5 }}>
              <AvatarGroup
                max={maxAvatars}
                sx={{
                  "& .MuiAvatar-root": {
                    width: 24,
                    height: 24,
                    fontSize: "0.75rem",
                  },
                }}
              >
                {visibleUsers.map((userName, index) => (
                  <Avatar
                    key={index}
                    sx={{ backgroundColor: getAvatarColor(userName) }}
                  >
                    {userName.charAt(0)}
                  </Avatar>
                ))}
              </AvatarGroup>
              {extraUsersCount > 0 && (
                <Typography variant="caption" color="text.secondary">
                  +{extraUsersCount}
                </Typography>
              )}
            </Box>
          )}
        </Box>
      </Box>
    </Paper>
  );
};

const MyTasksView = () => {
  const [viewMode, setViewMode] = useState(0); // 0: List, 1: Board
  const [filterTab, setFilterTab] = useState(0); // 0: All, 1: Private, 2: Assigned to me, 3: Flagged
  const [tasks, setTasks] = useState([]);
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [taskDialogOpen, setTaskDialogOpen] = useState(false);
  const [selectedTask, setSelectedTask] = useState(null);
  const [buckets, setBuckets] = useState([]);
  const [searchText, setSearchText] = useState("");
  // Add state for drag and drop
  const [activeTask, setActiveTask] = useState(null);

  const filterTabs = [
    "All",
    "Private tasks",
    "Assigned to me",
    "Flagged emails",
  ];
  const viewTabs = ["List", "Board"];

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);

      // Load all tasks across all projects and projects list
      const [tasksResponse, projectsResponse, bucketsResponse] =
        await Promise.all([
          taskPlannerApi.getTasks(), // Get all tasks (no project filter)
          taskPlannerApi.getProjects(),
          taskPlannerApi.getBuckets(), // Get all buckets from all projects
        ]);

      setTasks(tasksResponse.data || []);
      setProjects(projectsResponse.data || []);
      setBuckets(bucketsResponse.data || []);
    } catch (err) {
      console.error("Error loading my tasks data:", err);
    } finally {
      setLoading(false);
    }
  };

  const getTasksByBucket = (bucketName) => {
    return filteredTasks.filter((task) => {
      const taskBucket = buckets.find((b) => b.BucketGUID === task.BucketGUID);
      return taskBucket?.BucketName === bucketName;
    });
  };

  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const handleDragStart = (event) => {
    const { active } = event;
    setActiveTask(filteredTasks.find((task) => task.TaskGUID === active.id));
  };

  const handleDragEnd = async (event) => {
    const { active, over } = event;

    if (!over) {
      setActiveTask(null);
      return;
    }

    const taskId = active.id;
    const newBucketName = over.id;

    // Find the task and the target bucket
    const task = filteredTasks.find((t) => t.TaskGUID === taskId);
    const targetBucket = buckets.find((b) => b.BucketName === newBucketName);

    if (!task || !targetBucket) {
      setActiveTask(null);
      return;
    }

    // Don't update if it's the same bucket
    if (task.BucketGUID === targetBucket.BucketGUID) {
      setActiveTask(null);
      return;
    }

    // Update task bucket
    try {
      const updatedTask = {
        ...task,
        BucketGUID: targetBucket.BucketGUID,
      };

      await handleSaveTask(updatedTask);
    } catch (err) {
      console.error("Error updating task bucket:", err);
    }

    setActiveTask(null);
  };

  const getUniqueBucketNames = () => {
    const bucketNames = [...new Set(buckets.map((b) => b.BucketName))];

    // Sort buckets by common order (To Do, In Progress, Done, then others)
    const commonOrder = ["To Do", "In Progress", "Done"];
    const sortedBuckets = [];

    // Add common buckets first
    commonOrder.forEach((name) => {
      if (bucketNames.includes(name)) {
        sortedBuckets.push(name);
      }
    });

    // Add remaining buckets
    bucketNames.forEach((name) => {
      if (!commonOrder.includes(name)) {
        sortedBuckets.push(name);
      }
    });

    return sortedBuckets;
  };

  // Get bucket color for column headers
  const getBucketColor = (bucketName) => {
    // Find any bucket with this name and return its color
    const bucket = buckets.find((b) => b.BucketName === bucketName);
    return bucket?.BucketColor || "#616161";
  };

  const statusColumns = ["To Do", "In Progress", "Done"];

   const handleAddTask = () => {
    setSelectedTask(null);
    setTaskDialogOpen(true);
  };

  const handleEditTask = (task) => {
    setSelectedTask(task);
    setTaskDialogOpen(true);
  };

  const handleSaveTask = async (taskData) => {
    const result = await taskPlannerApi.saveTask(taskData);
    await loadData(); // Reload all data
    return result;
  };

  const getProjectName = (projectGuid) => {
    const project = projects.find((p) => p.ProjectGUID === projectGuid);
    return project ? project.ProjectName : "Unknown Project";
  };

  const getProjectColor = (projectGuid) => {
    // Simple hash function to generate consistent colors per project
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

  const getFilteredTasks = () => {
    let filtered = [];

    // First, filter by tab
    switch (filterTab) {
      case 0: // All
        filtered = tasks;
        break;
      case 1: // Private tasks
        filtered = tasks.filter((task) => task.IsPrivate && task.IsCreatedByMe);
        break;
      case 2: // Assigned to me
        filtered = tasks.filter((task) => task.IsAssignedToMe);
        break;
      case 3: // Flagged emails
        filtered = []; // Placeholder for future email integration
        break;
      default:
        filtered = tasks;
    }

    // Then apply search filter
    if (searchText && searchText.trim() !== "") {
      const searchLower = searchText.toLowerCase();
      filtered = filtered.filter((task) => {
        return (
          task.TaskTitle?.toLowerCase().includes(searchLower) ||
          task.TaskDescription?.toLowerCase().includes(searchLower) ||
          task.ProjectName?.toLowerCase().includes(searchLower)
        );
      });
    }

    return filtered;
  };

  const filteredTasks = getFilteredTasks();

  if (loading) {
    return (
      <Box sx={{ p: 3 }}>
        <Typography>Loading tasks...</Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 1.5 }}>
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          mb: 1,
        }}
      >
        <Tabs
          value={filterTab}
          onChange={(e, newValue) => setFilterTab(newValue)}
        >
          {filterTabs.map((tab, index) => (
            <Tab key={tab} label={tab} />
          ))}
        </Tabs>

        <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
          {/* Search Bar */}
          <TextField
            placeholder="Search tasks..."
            variant="outlined"
            size="small"
            value={searchText}
            onChange={(e) => setSearchText(e.target.value)}
            sx={{ minWidth: 250 }}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <Search />
                </InputAdornment>
              ),
            }}
          />

          {/* Board/List Toggle */}
          <ToggleButtonGroup
            value={viewMode}
            exclusive
            onChange={(e, newValue) =>
              newValue !== null && setViewMode(newValue)
            }
            size="small"
          >
            <ToggleButton value={0}>
              <ListIcon />
            </ToggleButton>
            <ToggleButton value={1}>
              <ViewKanban />
            </ToggleButton>
          </ToggleButtonGroup>

          <PermissionGuard permission="TaskPlanner_Tasks_Write">
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={handleAddTask}
              sx={{ mr: 2 }}
            >
              New Task
            </Button>
          </PermissionGuard>
        </Box>
      </Box>

      {viewMode === 0 && (
        // List View
        <Box>
          {/* Tasks Table */}
          {filteredTasks.length === 0 ? (
            <Box sx={{ textAlign: "center", py: 5 }}>
              <Assignment
                sx={{ fontSize: 64, color: "text.secondary", mb: 2 }}
              />
              <Typography variant="h6" color="text.secondary" gutterBottom>
                No tasks found
              </Typography>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Tasks you create or get assigned will appear here
              </Typography>
            </Box>
          ) : (
            <TableContainer
              component={Paper}
              elevation={0}
              sx={{ border: "1px solid", borderColor: "divider" }}
            >
              <Table>
                <TableHead>
                  <TableRow sx={{ bgcolor: "action.hover" }}>
                    <TableCell sx={{ fontWeight: 600 }}>Task Name</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Project</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Assigned to</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Due date</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Priority</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Progress</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Created by</TableCell>
                  </TableRow>
                </TableHead>

                <TableBody>
                  {filteredTasks.map((task) => {
                    // Helper function to generate avatar color based on name
                    const getAvatarColor = (name) => {
                      const colors = [
                        "#1976d2",
                        "#388e3c",
                        "#f57c00",
                        "#d32f2f",
                        "#7b1fa2",
                        "#616161",
                        "#0288d1",
                        "#689f38",
                      ];
                      const hash = name
                        ? name.split("").reduce((a, b) => {
                            a = (a << 5) - a + b.charCodeAt(0);
                            return a & a;
                          }, 0)
                        : 0;
                      return colors[Math.abs(hash) % colors.length];
                    };

                    // Parse assigned users
                    const assignedUsers = task.AssignedUsers
                      ? task.AssignedUsers.split(", ").filter((name) =>
                          name.trim()
                        )
                      : [];
                    const maxAvatars = 3;
                    const visibleUsers = assignedUsers.slice(0, maxAvatars);
                    const extraUsersCount = Math.max(
                      0,
                      assignedUsers.length - maxAvatars
                    );

                    return (
                      <TableRow
                        key={task.TaskGUID}
                        hover
                        sx={{ cursor: "pointer" }}
                        onClick={() => handleEditTask(task)}
                      >
                        <TableCell>
                          <Box
                            sx={{
                              display: "flex",
                              alignItems: "center",
                              gap: 1,
                            }}
                          >
                            {/* Private Task Indicator */}
                            {task.IsPrivate && (
                              <Lock
                                sx={{
                                  fontSize: 16,
                                  color: "text.secondary",
                                  mr: 0.5,
                                }}
                              />
                            )}

                            {/* Priority Bar */}
                            <Box
                              sx={{
                                width: 4,
                                height: 40,
                                backgroundColor:
                                  task.Priority === "Critical"
                                    ? "#d32f2f"
                                    : task.Priority === "High"
                                    ? "#f57c00"
                                    : task.Priority === "Medium"
                                    ? "#1976d2"
                                    : "#616161",
                                mr: 1,
                                borderRadius: 2,
                              }}
                            />

                            <Box sx={{ flexGrow: 1 }}>
                              <Typography
                                variant="body1"
                                sx={{ fontWeight: 500 }}
                              >
                                {task.TaskTitle}
                              </Typography>
                              {task.TaskDescription && (
                                <Typography
                                  variant="body2"
                                  color="text.secondary"
                                >
                                  {task.TaskDescription.length > 60
                                    ? `${task.TaskDescription.substring(
                                        0,
                                        60
                                      )}...`
                                    : task.TaskDescription}
                                </Typography>
                              )}
                            </Box>
                          </Box>
                        </TableCell>

                        <TableCell>
                          <Box
                            sx={{
                              display: "flex",
                              alignItems: "center",
                              gap: 1,
                            }}
                          >
                            <Box
                              sx={{
                                width: 12,
                                height: 12,
                                backgroundColor: getProjectColor(
                                  task.ProjectGUID
                                ),
                                borderRadius: "50%",
                              }}
                            />
                            <Typography variant="body2">
                              {getProjectName(task.ProjectGUID)}
                            </Typography>
                          </Box>
                        </TableCell>

                        {/* New Assigned To Column */}
                        <TableCell>
                          {assignedUsers.length > 0 ? (
                            <Box
                              sx={{
                                display: "flex",
                                alignItems: "center",
                                gap: 1,
                              }}
                            >
                              <AvatarGroup
                                max={maxAvatars}
                                sx={{
                                  "& .MuiAvatar-root": {
                                    width: 28,
                                    height: 28,
                                    fontSize: "0.8rem",
                                  },
                                }}
                              >
                                {visibleUsers.map((userName, index) => (
                                  <Avatar
                                    key={index}
                                    sx={{
                                      backgroundColor: getAvatarColor(userName),
                                    }}
                                    title={userName} // Tooltip on hover
                                  >
                                    {userName.charAt(0)}
                                  </Avatar>
                                ))}
                              </AvatarGroup>
                              {extraUsersCount > 0 && (
                                <Typography
                                  variant="caption"
                                  color="text.secondary"
                                >
                                  +{extraUsersCount}
                                </Typography>
                              )}
                            </Box>
                          ) : (
                            <Typography variant="body2" color="text.secondary">
                              Unassigned
                            </Typography>
                          )}
                        </TableCell>

                        <TableCell>
                          {task.DueDate ? (
                            <Box
                              sx={{
                                display: "flex",
                                alignItems: "center",
                                gap: 0.5,
                              }}
                            >
                              <CalendarToday
                                sx={{ fontSize: 14, color: "text.secondary" }}
                              />
                              <Typography
                                variant="body2"
                                color={
                                  new Date(task.DueDate) < new Date()
                                    ? "error"
                                    : "text.primary"
                                }
                              >
                                {new Date(task.DueDate).toLocaleDateString()}
                              </Typography>
                            </Box>
                          ) : (
                            <Typography variant="body2" color="text.secondary">
                              -
                            </Typography>
                          )}
                        </TableCell>

                        <TableCell>
                          <Chip
                            label={task.Priority}
                            size="small"
                            color={
                              task.Priority === "Critical"
                                ? "error"
                                : task.Priority === "High"
                                ? "warning"
                                : task.Priority === "Medium"
                                ? "primary"
                                : "default"
                            }
                          />
                        </TableCell>

                        {/* Status/Bucket Column - Add this after the Plan column */}
                        <TableCell>
                          {task.BucketName ? (
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
                                  backgroundColor: getBucketColor(
                                    task.BucketName
                                  ),
                                  borderRadius: "50%",
                                }}
                              />
                              <Typography
                                variant="body2"
                                sx={{ fontWeight: 500 }}
                              >
                                {task.BucketName}
                              </Typography>
                            </Box>
                          ) : (
                            <Typography variant="body2" color="text.secondary">
                              No bucket
                            </Typography>
                          )}
                        </TableCell>

                        <TableCell>
                          <Box
                            sx={{
                              display: "flex",
                              alignItems: "center",
                              gap: 1,
                              minWidth: 100,
                            }}
                          >
                            <LinearProgress
                              variant="determinate"
                              value={task.ProgressPercentage || 0}
                              sx={{
                                flexGrow: 1,
                                height: 6,
                                borderRadius: 3,
                                backgroundColor: "action.hover",
                                "& .MuiLinearProgress-bar": {
                                  borderRadius: 3,
                                },
                              }}
                            />
                            <Typography
                              variant="caption"
                              sx={{ fontWeight: 600 }}
                            >
                              {task.ProgressPercentage || 0}%
                            </Typography>
                          </Box>
                        </TableCell>

                        <TableCell>
                          {/* Created By */}
                          <Box
                            sx={{
                              display: "flex",
                              alignItems: "center",
                              gap: 1,
                            }}
                          >
                            <Avatar
                              sx={{
                                width: 24,
                                height: 24,
                                fontSize: "0.75rem",
                                backgroundColor: getAvatarColor(
                                  task.CreatedByName
                                ),
                              }}
                              title={task.CreatedByName} // Tooltip
                            >
                              {task.CreatedByName?.charAt(0) || "U"}
                            </Avatar>
                            <Typography variant="body2" color="text.secondary">
                              {task.CreatedByName}
                            </Typography>
                          </Box>
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

      {viewMode === 1 && (
        // Board View - Dynamic Bucket Columns
        <Box>
          {getUniqueBucketNames().length === 0 ? (
            <Box sx={{ textAlign: "center", py: 2 }}>
              <ViewKanban
                sx={{ fontSize: 64, color: "text.secondary", mb: 1 }}
              />
              <Typography variant="h6" color="text.secondary" gutterBottom>
                No buckets found
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Create some projects with buckets to see the board view
              </Typography>
            </Box>
          ) : (
            <DndContext
              sensors={sensors}
              collisionDetection={closestCenter}
              onDragStart={handleDragStart}
              onDragEnd={handleDragEnd}
            >
              <Box
                sx={{
                  display: "flex",
                  gap: 1,
                  overflow: "auto",
                  minHeight: "78vh",
                }}
              >
                {getUniqueBucketNames().map((bucketName) => (
                  <KanbanColumn
                    key={bucketName}
                    status={bucketName}
                    statusColor={getBucketColor(bucketName)}
                    tasks={getTasksByBucket(bucketName)}
                    projects={projects}
                    getProjectName={getProjectName}
                    getProjectColor={getProjectColor}
                    onTaskClick={handleEditTask}
                  />
                ))}
              </Box>

              <DragOverlay>
                {activeTask ? (
                  <TaskCard
                    task={activeTask}
                    projectName={getProjectName(activeTask.ProjectGUID)}
                    projectColor={getProjectColor(activeTask.ProjectGUID)}
                    isDragging
                  />
                ) : null}
              </DragOverlay>
            </DndContext>
          )}
        </Box>
      )}

      {/* Task Dialog */}
      <TaskDialog
        open={taskDialogOpen}
        onClose={() => setTaskDialogOpen(false)}
        task={selectedTask}
        projectGuid={selectedTask?.ProjectGUID}
        buckets={[]} // We'll need to load buckets for the specific project
        onSave={handleSaveTask}
      />
    </Box>
  );
};

export default MyTasksView;
