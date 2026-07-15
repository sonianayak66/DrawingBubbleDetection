import React, { useState } from "react";
import {
  Box,
  Typography,
  Button,
  Tabs,
  Tab,
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
  Chip,
  Avatar,
} from "@mui/material";
import {
  Add,
  MoreVert,
  Edit,
  Delete,
  Folder,
  Share,
  Lock,
} from "@mui/icons-material";
import PermissionGuard from "../../shared/components/Common/PermissionGuard";

const ProjectsListView = ({
  projects = [],
  onCreateProject,
  onEditProject,
  onDeleteProject,
  onProjectClick,
}) => {
  const [selectedTab, setSelectedTab] = useState(0);
  const [menuAnchor, setMenuAnchor] = useState(null);
  const [selectedProject, setSelectedProject] = useState(null);

  const tabs = ["Recent", "Shared", "Personal", "Pinned"];

  const handleTabChange = (event, newValue) => {
    setSelectedTab(newValue);
  };

  const handleMenuOpen = (event, project) => {
    event.stopPropagation();
    setMenuAnchor(event.currentTarget);
    setSelectedProject(project);
  };

  const handleMenuClose = () => {
    setMenuAnchor(null);
    setSelectedProject(null);
  };

  const handleEdit = () => {
    onEditProject(selectedProject);
    handleMenuClose();
  };

  const handleDelete = () => {
    onDeleteProject(selectedProject);
    handleMenuClose();
  };

  const getStatusIcon = (status) => {
    switch (status?.toLowerCase()) {
      case "active":
        return <Folder sx={{ color: "success.main" }} />;
      case "completed":
        return <Folder sx={{ color: "primary.main" }} />;
      case "on hold":
        return <Folder sx={{ color: "warning.main" }} />;
      case "cancelled":
        return <Folder sx={{ color: "error.main" }} />;
      default:
        return <Folder sx={{ color: "text.secondary" }} />;
    }
  };

  const getPrivacyInfo = (project) => {
    // For now, we'll assume all projects are shared
    // You can enhance this based on your business logic
    return {
      icon: <Share fontSize="small" />,
      text: "Shared",
    };
  };

  const filteredProjects = projects; // For now, show all projects. You can add filtering logic here.

  return (
    <Box sx={{ p: 3 }}>
      {/* Filter Tabs */}
      <Box sx={{ borderBottom: 1, borderColor: "divider", mb: 3 }}>
        <Tabs value={selectedTab} onChange={handleTabChange}>
          {tabs.map((tab, index) => (
            <Tab key={tab} label={tab} />
          ))}
        </Tabs>
      </Box>

      {/* Projects Table */}
      {filteredProjects.length === 0 ? (
        <Box sx={{ textAlign: "center", py: 8 }}>
          <Folder sx={{ fontSize: 64, color: "text.secondary", mb: 2 }} />
          <Typography variant="h6" color="text.secondary" gutterBottom>
            No projects found
          </Typography>
          <PermissionGuard permission="TaskPlanner_Projects_Write">
            <Button variant="outlined" onClick={onCreateProject} sx={{ mt: 2 }}>
              Create your first project
            </Button>
          </PermissionGuard>
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
                <TableCell sx={{ fontWeight: 600 }}>Name</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Privacy</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>
                  Last accessed by you
                </TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Shared with</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 50 }}></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredProjects.map((project) => {
                const privacy = getPrivacyInfo(project);
                return (
                  <TableRow
                    key={project.ProjectGUID}
                    hover
                    sx={{ cursor: "pointer" }}
                    //onClick={() => onProjectClick(project.ProjectGUID)}
                  >
                    <TableCell>
                      <Box
                        sx={{ display: "flex", alignItems: "center", gap: 2 }}
                      >
                        {getStatusIcon(project.ProjectStatus)}
                        <Box>
                          <Typography variant="body1" sx={{ fontWeight: 500 }}>
                            {project.ProjectName}
                          </Typography>
                          {project.ProjectDescription && (
                            <Typography
                              variant="caption"
                              color="text.secondary"
                            >
                              {project.ProjectDescription.length > 50
                                ? `${project.ProjectDescription.substring(
                                    0,
                                    50
                                  )}...`
                                : project.ProjectDescription}
                            </Typography>
                          )}
                        </Box>
                      </Box>
                    </TableCell>

                    <TableCell>
                      <Box
                        sx={{ display: "flex", alignItems: "center", gap: 1 }}
                      >
                        {privacy.icon}
                        <Typography variant="body2">{privacy.text}</Typography>
                      </Box>
                    </TableCell>

                    <TableCell>
                      <Typography variant="body2">
                        {new Date(project.CreatedDate).toLocaleDateString()}
                      </Typography>
                    </TableCell>

                    <TableCell>
                      <Typography variant="body2" color="text.secondary">
                        {project.CreatedByName}
                      </Typography>
                    </TableCell>

                    <TableCell>
                      <PermissionGuard
                        permissions={[
                          "TaskPlanner_Projects_Write",
                          "TaskPlanner_Projects_Delete",
                        ]}
                      >
                        <IconButton
                          size="small"
                          onClick={(e) => handleMenuOpen(e, project)}
                        >
                          <MoreVert />
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

      {/* Context Menu */}
      <Menu
        anchorEl={menuAnchor}
        open={Boolean(menuAnchor)}
        onClose={handleMenuClose}
      >
        <PermissionGuard permission="TaskPlanner_Projects_Write">
          <MenuItem onClick={handleEdit}>
            <Edit fontSize="small" sx={{ mr: 1 }} />
            Edit
          </MenuItem>
        </PermissionGuard>
        <PermissionGuard permission="TaskPlanner_Projects_Delete">
          <MenuItem onClick={handleDelete}>
            <Delete fontSize="small" sx={{ mr: 1 }} />
            Delete
          </MenuItem>
        </PermissionGuard>
      </Menu>
    </Box>
  );
};

export default ProjectsListView;
