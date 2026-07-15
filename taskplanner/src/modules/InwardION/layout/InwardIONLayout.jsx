import React, { useState } from "react";
import {
  Box,
  AppBar,
  Toolbar,
  Typography,
  Button,
  IconButton,
  Drawer,
} from "@mui/material";
import { Add, Delete } from "@mui/icons-material";
import PermissionGuard from "../../shared/components/Common/PermissionGuard";
import { usePermissions } from "../../../context/PermissionsContext";
import { inwardIonApi } from "../../../services/inwardIonApi";

import InwardIONListView from "../views/InwardIONListView";
import InwardIONFormView from "../views/InwardIONFormView";
import InwardIONDetailView from "../views/InwardIONDetailView";

const DRAWER_WIDTH = "33.33vw"; // col-4

const InwardIONLayout = ({ selectedView, onViewChange, viewContext }) => {
  const { hasPermission } = usePermissions();
  const isAdmin = hasPermission("ION_Inward_Admin");
  const [selectedNote, setSelectedNote] = useState(null);
  const [formOpen, setFormOpen] = useState(false);
  const [formMode, setFormMode] = useState("create");
  const [detailOpen, setDetailOpen] = useState(false);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  const handleCreate = () => {
    setDetailOpen(false);
    setSelectedNote(null);
    setFormMode("create");
    setFormOpen(true);
  };

  const handleEdit = (note) => {
    setDetailOpen(false);
    setSelectedNote(note);
    setFormMode("edit");
    setFormOpen(true);
  };

  const handleView = (note) => {
    setFormOpen(false);
    setSelectedNote(note);
    setDetailOpen(true);
  };

  const handleSave = (savedNote) => {
    setFormOpen(false);
    setRefreshTrigger((prev) => prev + 1);
    if (savedNote && savedNote.InwardIONGUID) {
      setSelectedNote(savedNote);
      setDetailOpen(true);
    }
  };

  const handleDelete = async (note) => {
    if (window.confirm("Are you sure you want to delete this Inward ION? This action cannot be undone.")) {
      try {
        await inwardIonApi.deleteInwardNote(note.InwardIONGUID);
        setRefreshTrigger((prev) => prev + 1);
        setDetailOpen(false);
        setFormOpen(false);
        setSelectedNote(null);
      } catch (error) {
        console.error("Error deleting Inward ION:", error);
        alert("Failed to delete Inward ION");
      }
    }
  };

  const handleCloseDrawer = () => {
    setFormOpen(false);
    setDetailOpen(false);
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
            Inward IONs
          </Typography>

          <Box sx={{ flexGrow: 1 }} />

          <PermissionGuard permission="ION_Inward_Create">
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={handleCreate}
              sx={{ mr: 1 }}
            >
              Log Inward ION
            </Button>
          </PermissionGuard>
        </Toolbar>
      </AppBar>

      {/* Content Area — list is always visible */}
      <Box
        sx={{
          flexGrow: 1,
          overflow: "auto",
          bgcolor: "background.default",
        }}
      >
        <InwardIONListView
          onCreateNote={handleCreate}
          onEditNote={handleEdit}
          onViewNote={handleView}
          refreshTrigger={refreshTrigger}
        />
      </Box>

      {/* Form Drawer (create / edit) — no close on backdrop click */}
      <Drawer
        anchor="right"
        open={formOpen}
        onClose={() => {}} // prevent accidental close on backdrop click
        ModalProps={{ disableEscapeKeyDown: true }}
        PaperProps={{
          sx: { width: DRAWER_WIDTH, minWidth: 400, height: "100%", overflow: "hidden" },
        }}
      >
        {formOpen && (
          <InwardIONFormView
            key={formMode === "create" ? "new" : selectedNote?.InwardIONGUID}
            noteData={selectedNote}
            mode={formMode}
            onSave={handleSave}
            onCancel={handleCloseDrawer}
          />
        )}
      </Drawer>

      {/* Detail Drawer (view) */}
      <Drawer
        anchor="right"
        open={detailOpen}
        onClose={handleCloseDrawer}
        PaperProps={{
          sx: { width: DRAWER_WIDTH, minWidth: 400, height: "100%", overflow: "hidden" },
        }}
      >
        {detailOpen && selectedNote && (
          <InwardIONDetailView
            inwardIONGUID={selectedNote.InwardIONGUID}
            onEdit={handleEdit}
            onBack={handleCloseDrawer}
            onDelete={handleDelete}
          />
        )}
      </Drawer>
    </Box>
  );
};

export default InwardIONLayout;
