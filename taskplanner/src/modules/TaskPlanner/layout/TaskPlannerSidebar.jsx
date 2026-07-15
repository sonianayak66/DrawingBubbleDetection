import React, { useState } from "react";
import {
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Divider,
  Box,
  Typography,
  Collapse,
} from "@mui/material";
import {
  Today,
  Assignment,
  Folder,
  Star,
  Email,
  Analytics,
  ExpandLess,
  ExpandMore,
} from "@mui/icons-material";
import { usePermissions } from "../../../context/PermissionsContext";
import PermissionGuard from '../../../modules/shared/components/Common/PermissionGuard';

const SIDEBAR_WIDTH = 250;

const TaskPlannerSidebar = ({ selectedView, onViewChange, projects = [] }) => {
  const [projectsExpanded, setProjectsExpanded] = useState(true);
  const { hasPermission } = usePermissions();

  const navigationItems = [
    {
      id: "my-day",
      label: "My Day",
      icon: <Today />,
      permission: "TaskPlanner_Tasks_Read",
    },
    {
      id: "my-tasks",
      label: "My Tasks",
      icon: <Assignment />,
      permission: "TaskPlanner_Tasks_Read",
    },
    {
      id: "projects",
      label: "Projects",
      icon: <Folder />,
      permission: "TaskPlanner_Projects_Read",
      expandable: true,
      expanded: projectsExpanded,
      onToggle: () => setProjectsExpanded(!projectsExpanded),
    },
  ];

  const secondaryItems = [
    {
      id: "pinned",
      label: "Pinned",
      icon: <Star />,
      permission: "TaskPlanner_Tasks_Read",
    },
    {
      id: "emails",
      label: "Email Management",
      icon: <Email />,
      permission: "TaskPlanner_Emails_Read",
    },
    {
      id: "reports",
      label: "Reports",
      icon: <Analytics />,
      permission: "TaskPlanner_Reports_View",
    },
  ];

  const handleNavClick = (viewId) => {
    if (onViewChange) {
      onViewChange(viewId);
    }
  };

  const handleProjectClick = (projectGuid) => {
    if (onViewChange) {
      onViewChange("project-detail", { projectGuid });
    }
  };

  return (
    <Drawer
      variant="permanent"
      sx={{
        width: SIDEBAR_WIDTH,
        flexShrink: 0,
        "& .MuiDrawer-paper": {
          width: SIDEBAR_WIDTH,
          boxSizing: "border-box",
          borderRight: "1px solid",
          borderColor: "divider",
        },
      }}
    >
      <Box sx={{ overflow: "auto", height: "100%" }}>
        {/* Header */}
        <Box sx={{ p: 2, borderBottom: "1px solid", borderColor: "divider" }}>
          <Typography variant="h6" component="div" sx={{ fontWeight: "bold" }}>
            Task Planner
          </Typography>
        </Box>

        {/* Main Navigation */}
        <List sx={{ pt: 1 }}>
          {navigationItems.map((item) => (
            <PermissionGuard key={item.id} permission={item.permission}>
              <ListItem disablePadding>
                <ListItemButton
                  selected={selectedView === item.id}
                  onClick={() => {
                    if (item.expandable) {
                      if (item.id === "projects") {
                        handleNavClick(item.id);
                        setProjectsExpanded(!projectsExpanded);
                      }
                    } else {
                      handleNavClick(item.id);
                    }
                  }}
                  sx={{ py: 1.5 }}
                >
                  <ListItemIcon sx={{ minWidth: 40 }}>{item.icon}</ListItemIcon>
                  <ListItemText
                    primary={item.label}
                    primaryTypographyProps={{
                      fontWeight: selectedView === item.id ? 600 : 400,
                    }}
                  />
                  {item.expandable && (
                    projectsExpanded ? <ExpandLess /> : <ExpandMore />
                  )}
                </ListItemButton>
              </ListItem>

              {/* Projects submenu */}
              {item.id === "projects" && (
                <Collapse in={projectsExpanded} timeout="auto" unmountOnExit>
                  <List component="div" disablePadding>
                    {projects.slice(0, 10).map((project) => (
                      <ListItem key={project.ProjectGUID} disablePadding>
                        <ListItemButton
                          sx={{ pl: 4, py: 1 }}
                          selected={selectedView === "project-detail"}
                          onClick={() => handleProjectClick(project.ProjectGUID)}
                        >
                          <ListItemIcon sx={{ minWidth: 40 }}>
                            <Folder fontSize="small" />
                          </ListItemIcon>
                          <ListItemText
                            primary={project.ProjectName}
                            primaryTypographyProps={{
                              fontSize: "0.875rem",
                              noWrap: true,
                            }}
                          />
                        </ListItemButton>
                      </ListItem>
                    ))}
                  </List>
                </Collapse>
              )}
            </PermissionGuard>
          ))}
        </List>

        <Divider sx={{ my: 1 }} />

        {/* Secondary Navigation */}
        <List>
          {secondaryItems.map((item) => (
            <PermissionGuard key={item.id} permission={item.permission}>
              <ListItem disablePadding>
                <ListItemButton
                  selected={selectedView === item.id}
                  onClick={() => handleNavClick(item.id)}
                  sx={{ py: 1.5 }}
                >
                  <ListItemIcon sx={{ minWidth: 40 }}>{item.icon}</ListItemIcon>
                  <ListItemText
                    primary={item.label}
                    primaryTypographyProps={{
                      fontWeight: selectedView === item.id ? 600 : 400,
                    }}
                  />
                </ListItemButton>
              </ListItem>
            </PermissionGuard>
          ))}
        </List>
      </Box>
    </Drawer>
  );
};

export default TaskPlannerSidebar;