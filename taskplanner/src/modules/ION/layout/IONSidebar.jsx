import React from "react";
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
} from "@mui/material";
import {
  Description,
  Assignment,
  CheckCircle,
} from "@mui/icons-material";
import { usePermissions } from "../../../context/PermissionsContext";
import PermissionGuard from '../../../modules/shared/components/Common/PermissionGuard';

const SIDEBAR_WIDTH = 250;

const IONSidebar = ({ selectedView, onViewChange }) => {
  const { hasPermission } = usePermissions();

  const navigationItems = [
    {
      id: "ion-list",
      label: "All ION Notes",
      icon: <Description />,
      permission: "ION_View",
    },
  ];

  const secondaryItems = [
    {
      id: "ion-drafts",
      label: "My ION Drafts",
      icon: <Assignment />,
      permission: "ION_Create",
    },
    {
      id: "ion-approved",
      label: "Approved IONs",
      icon: <CheckCircle />,
      permission: "ION_View",
    },
  ];

  const handleNavClick = (viewId) => {
    onViewChange(viewId);
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
            Inter Office Notes
          </Typography>
        </Box>

        {/* Main Navigation */}
        <List sx={{ pt: 1 }}>
          {navigationItems.map((item) => (
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

export default IONSidebar;