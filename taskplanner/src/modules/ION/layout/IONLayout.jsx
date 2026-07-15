import React, { useState, useEffect } from "react";
import {
  Box,
  AppBar,
  Toolbar,
  Typography,
  Button,
  IconButton,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItemButton,
  CircularProgress,
} from "@mui/material";
import {
  Add,
  Edit,
  Delete,
  Settings,
  Business,
  LocationOn,
  FolderCopy,
  Description,
} from "@mui/icons-material";
import PermissionGuard from "../../../modules/shared/components/Common/PermissionGuard";
import { usePermissions } from "../../../context/PermissionsContext";
import { ionApi } from "../../../services/ionApi";
import FileGroupsView from "../views/FileGroupsView";
import IONTemplatesView from "../views/IONTemplatesView";

// NEW IMPORTS - Use these instead
import IONListView from "../views/IONListView";
import IONFormView from "../views/IONFormView";
import IONDetailView from "../views/IONDetailView";
import OfficeConfigView from "../views/OfficeConfigView";
import DestinationsView from "../views/DestinationsView";

const IONLayout = ({ selectedView, onViewChange, viewContext }) => {
  const { hasPermission } = usePermissions();
  const isAdmin = hasPermission("ION_Admin");
  const [selectedION, setSelectedION] = useState(null);
  const [ionFormOpen, setIONFormOpen] = useState(false);
  const [ionFormMode, setIONFormMode] = useState("create");
  const [settingsMenuAnchor, setSettingsMenuAnchor] = useState(null); // ADD THIS
  const [refreshTrigger, setRefreshTrigger] = useState(0); // ADD THIS

  // Template picker state — shown when user clicks "Create ION".
  // If active templates exist, the picker lets the user start blank or pre-fill
  // from a shared template. Otherwise we skip the picker and create blank.
  const [templatePickerOpen, setTemplatePickerOpen] = useState(false);
  const [availableTemplates, setAvailableTemplates] = useState([]);
  const [templatesLoading, setTemplatesLoading] = useState(false);
  // The template selected from the picker (or null for blank). Passed to IONFormView.
  const [pendingCreateTemplate, setPendingCreateTemplate] = useState(null);

  // Blank create — opens the empty form directly. Bound to the "Create ION" button.
  const handleCreateION = () => {
    startCreateForm(null);
  };

  // Template create — loads templates and shows the picker. Bound to "Create from Template".
  const handleOpenTemplatePicker = async () => {
    try {
      setTemplatesLoading(true);
      setTemplatePickerOpen(true);
      const response = await ionApi.getIONTemplates();
      const active = (response.data || []).filter(t => t.IsActive);
      setAvailableTemplates(active);
    } catch (error) {
      console.error("Error loading templates:", error);
      setAvailableTemplates([]);
    } finally {
      setTemplatesLoading(false);
    }
  };

  const startCreateForm = (templateData) => {
    setPendingCreateTemplate(templateData);
    setSelectedION(null);
    setIONFormMode("create");
    setIONFormOpen(true);
  };

  const handlePickTemplate = (templateData) => {
    setTemplatePickerOpen(false);
    startCreateForm(templateData);
  };

  const handleClosePicker = () => {
    setTemplatePickerOpen(false);
  };

  const handleCloseCreateForm = () => {
    setIONFormOpen(false);
    // Clear pending template so the next open is blank unless picker chooses one
    setPendingCreateTemplate(null);
  };

  const handleEditION = (ion) => {
    setSelectedION(ion);
    setIONFormMode("edit");
    setIONFormOpen(true);
  };

  const handleViewION = (ion) => {
    setSelectedION(ion);
    onViewChange("ion-detail");
  };

  const handleSaveION = (savedION) => {
    setIONFormOpen(false);
    setPendingCreateTemplate(null);
    setRefreshTrigger((prev) => prev + 1);
    // Stay on detail view so user can print immediately
    if (savedION && savedION.IONGUID) {
      setSelectedION(savedION);
      onViewChange("ion-detail");
    }
  };

  const handleDeleteION = async (ion) => {
    if (window.confirm("Are you sure you want to delete this ION?")) {
      try {
        await ionApi.deleteIONNote(ion.IONGUID);
        setRefreshTrigger((prev) => prev + 1); // Trigger refresh
        onViewChange("ion-list");
      } catch (error) {
        console.error("Error deleting ION:", error);
        alert("Failed to delete ION");
      }
    }
  };

  const handleBackToIONList = () => {
    onViewChange("ion-list");
    setSelectedION(null);
  };

  const handleSettingsMenuOpen = (event) => {
    setSettingsMenuAnchor(event.currentTarget);
  };

  const handleSettingsMenuClose = () => {
    setSettingsMenuAnchor(null);
  };

  const handleSettingsNavigation = (settingType) => {
    handleSettingsMenuClose();
    if (settingType === "offices") {
      onViewChange("ion-settings-offices");
    } else if (settingType === "destinations") {
      onViewChange("ion-settings-destinations");
    } else if (settingType === "file-groups") {
      onViewChange("ion-settings-file-groups");
    } else if (settingType === "templates") {
      onViewChange("ion-settings-templates");
    }
  };

  const renderPageActions = () => {
    switch (selectedView) {
      case "ion-list":
      case "ion-drafts":
      case "ion-approved":
        return (
          <PermissionGuard permission="ION_Create">
            <Box sx={{ display: 'flex', gap: 1, mr: 1 }}>
              <Button
                variant="contained"
                startIcon={<Add />}
                onClick={handleCreateION}
              >
                Create ION
              </Button>
              <Button
                variant="outlined"
                startIcon={<Description />}
                onClick={handleOpenTemplatePicker}
              >
                Create from Template
              </Button>
            </Box>
          </PermissionGuard>
        );

      case "ion-detail":
        return (
          <Box sx={{ display: "flex", gap: 1 }}>
            <PermissionGuard permission="ION_Edit">
              <IconButton
                onClick={() => handleEditION(selectedION)}
                disabled={!isAdmin && selectedION?.Status === "Approved"}
              >
                <Edit />
              </IconButton>
            </PermissionGuard>
            <PermissionGuard permission="ION_Admin">
              <IconButton
                onClick={() => handleDeleteION(selectedION)}
                color="error"
              >
                <Delete />
              </IconButton>
            </PermissionGuard>
          </Box>
        );

      default:
        return null;
    }
  };

  const getPageTitle = () => {
    switch (selectedView) {
      case "ion-list":
        return "Inter Office Notes";
      case "ion-drafts":
        return "My ION Drafts";
      case "ion-approved":
        return "Approved IONs";
      case "ion-detail":
        return selectedION ? `ION - ${selectedION.IONNumber}` : "ION Details";
      default:
        return "Inter Office Notes";
    }
  };

  const renderContent = () => {
    switch (selectedView) {
      case "ion-list":
      case "ion-drafts":
      case "ion-approved":
        return (
          <IONListView
            viewType={selectedView}
            onCreateION={handleCreateION}
            onEditION={handleEditION}
            onViewION={handleViewION}
            refreshTrigger={refreshTrigger} // ADD THIS
          />
        );

      case "ion-detail":
        return (
          <IONDetailView
            ionGuid={selectedION?.IONGUID}
            onEdit={handleEditION}
            onBack={handleBackToIONList}
            onDelete={handleDeleteION}
            refreshTrigger={refreshTrigger}
          />
        );

      case "ion-settings-offices":
        return <OfficeConfigView onBack={handleBackToIONList} />;

      case "ion-settings-destinations":
        return <DestinationsView onBack={handleBackToIONList} />;

      case "ion-settings-file-groups":
        return <FileGroupsView onBack={handleBackToIONList} />;

      case "ion-settings-templates":
        return <IONTemplatesView onBack={handleBackToIONList} />;

      default:
        return (
          <Box sx={{ p: 3, textAlign: "center" }}>
            <Typography variant="h4" gutterBottom>
              Welcome to ION
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Select a view from the sidebar to get started.
            </Typography>
          </Box>
        );
    }
  };

  return (
    <Box
      sx={{
        flexGrow: 1,
        display: "flex",
        flexDirection: "column",
        overflow: "hidden",
      }}
    >
      {/* Top App Bar */}
      <AppBar
        position="static"
        elevation={0}
        sx={{
          borderBottom: "1px solid",
          borderColor: "divider",
          bgcolor: "background.paper",
          color: "text.primary",
        }}
      >
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ fontWeight: 600 }}>
            Inter Office Notes
          </Typography>

          <Box sx={{ flexGrow: 1 }} />

          {/* Settings Menu */}
          <PermissionGuard permission="ION_Admin">
            <IconButton
              onClick={handleSettingsMenuOpen}
              size="small"
              sx={{ mr: 2 }}
            >
              <Settings />
            </IconButton>
            <Menu
              anchorEl={settingsMenuAnchor}
              open={Boolean(settingsMenuAnchor)}
              onClose={handleSettingsMenuClose}
            >
              {/* <MenuItem onClick={() => handleSettingsNavigation("offices")}>
                <ListItemIcon>
                  <Business fontSize="small" />
                </ListItemIcon>
                <ListItemText>Manage Offices</ListItemText>
              </MenuItem>
              <MenuItem
                onClick={() => handleSettingsNavigation("destinations")}
              >
                <ListItemIcon>
                  <LocationOn fontSize="small" />
                </ListItemIcon>
                <ListItemText>Manage Destinations</ListItemText>
              </MenuItem> */}

              <MenuItem onClick={() => handleSettingsNavigation("file-groups")}>
                <ListItemIcon>
                  <FolderCopy fontSize="small" />
                </ListItemIcon>
                <ListItemText>File numbering system for STFE</ListItemText>
              </MenuItem>
              <MenuItem onClick={() => handleSettingsNavigation("templates")}>
                <ListItemIcon>
                  <Description fontSize="small" />
                </ListItemIcon>
                <ListItemText>Manage ION Templates</ListItemText>
              </MenuItem>
            </Menu>
          </PermissionGuard>

          {renderPageActions()}
        </Toolbar>
      </AppBar>

      {/* Content Area */}
      <Box
        sx={{
          flexGrow: 1,
          overflow: "auto",
          bgcolor: "background.default",
        }}
      >
        {renderContent()}
      </Box>

      {/* ION Form Dialog */}
      <IONFormView
        key={ionFormMode === "create" ? `new-${pendingCreateTemplate?.TemplateGUID || "blank"}` : selectedION?.IONGUID}
        ionGuid={selectedION?.IONGUID}
        open={ionFormOpen}
        mode={ionFormMode}
        templateData={ionFormMode === "create" ? pendingCreateTemplate : null}
        onSave={handleSaveION}
        onCancel={handleCloseCreateForm}
      />

      {/* Create from Template Picker — shown when user clicks "Create from Template" */}
      <Dialog
        open={templatePickerOpen}
        onClose={handleClosePicker}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle sx={{ borderBottom: 1, borderColor: 'divider' }}>
          Select Template
        </DialogTitle>
        <DialogContent sx={{ p: 0 }}>
          {templatesLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', py: 6 }}>
              <CircularProgress size={32} />
            </Box>
          ) : availableTemplates.length === 0 ? (
            <Box sx={{ p: 4, textAlign: 'center' }}>
              <Typography variant="body1" color="text.secondary" sx={{ mb: 1 }}>
                No templates available yet.
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {isAdmin
                  ? 'Use Settings → Manage ION Templates to create one, or click "Create ION" to start from blank.'
                  : 'Click "Create ION" to start from blank, or ask an admin to set up a template.'}
              </Typography>
            </Box>
          ) : (
            <List disablePadding>
              {availableTemplates.map((tpl) => (
                <ListItemButton
                  key={tpl.TemplateGUID}
                  onClick={() => handlePickTemplate(tpl)}
                  sx={{ alignItems: 'flex-start' }}
                >
                  <ListItemIcon sx={{ mt: 0.5 }}>
                    <Description color="secondary" />
                  </ListItemIcon>
                  <ListItemText
                    primary={
                      <Typography variant="body1" sx={{ fontWeight: 600 }}>
                        {tpl.TemplateName}
                      </Typography>
                    }
                    secondary={
                      <Box>
                        {tpl.Description && (
                          <Typography variant="body2" color="text.secondary">
                            {tpl.Description}
                          </Typography>
                        )}
                        {tpl.SubjectTemplate && (
                          <Typography variant="caption" color="text.secondary" sx={{ fontStyle: 'italic' }}>
                            Subject: {tpl.SubjectTemplate}
                          </Typography>
                        )}
                      </Box>
                    }
                  />
                </ListItemButton>
              ))}
            </List>
          )}
        </DialogContent>
        <DialogActions sx={{ borderTop: 1, borderColor: 'divider' }}>
          <Button onClick={handleClosePicker} size="small">Cancel</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default IONLayout;
