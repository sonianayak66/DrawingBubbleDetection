import React, { useState, useEffect } from "react";
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
  TextField,
  InputAdornment,
  Toolbar,
  Dialog, // ADD THIS
  DialogTitle, // ADD THIS
  DialogContent, // ADD THIS
  DialogActions, // ADD THIS
} from "@mui/material";
import {
  Add,
  MoreVert,
  Edit,
  Delete,
  Visibility,
  CheckCircle,
  Pending,
  Search,
  FileDownload,
  Upload,
  Cancel,
} from "@mui/icons-material";
import { ionApi } from "../../../services/ionApi";
import PermissionGuard from "../../shared/components/Common/PermissionGuard";
import { usePermissions } from "../../../context/PermissionsContext";
import { useUser } from "../../../context/UserContext";

const IONListView = ({
  onCreateION,
  onEditION,
  onViewION,
  refreshTrigger = 0,
}) => {
  const { hasPermission } = usePermissions();
  const { getCurrentUserId } = useUser();
  const isAdmin = hasPermission("ION_Admin");
  const isDocHandler = hasPermission("ION_SupportOperator");
  const currentUserId = getCurrentUserId();
  const [ionNotes, setIONNotes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState(null); // null = 'All'
  const [searchTerm, setSearchTerm] = useState("");
  const [menuAnchor, setMenuAnchor] = useState(null);
  const [selectedION, setSelectedION] = useState(null);
  const [pdfPreviewOpen, setPdfPreviewOpen] = useState(false);
  const [pdfPreviewUrl, setPdfPreviewUrl] = useState(null);

  const statusFilters = [
    { label: "All", value: null },
    { label: "Draft", value: "Draft" },
    { label: "Approved", value: "Approved" },
  ];

  useEffect(() => {
    loadIONNotes();
  }, [statusFilter, refreshTrigger]);

  const loadIONNotes = async () => {
    try {
      setLoading(true);
      const response = await ionApi.getIONNotes({
        status: statusFilter,
      });
      setIONNotes(response.data || []);
    } catch (error) {
      console.error("Error loading ION notes:", error);
      setIONNotes([]);
    } finally {
      setLoading(false);
    }
  };

  const handleStatusFilterChange = (filterValue) => {
    setStatusFilter(filterValue);
  };

  const handleMenuOpen = (event, ion) => {
    event.stopPropagation();
    setMenuAnchor(event.currentTarget);
    setSelectedION(ion);
  };

  const handleMenuClose = () => {
    setMenuAnchor(null);
    setSelectedION(null);
  };

  const handleView = () => {
    onViewION(selectedION);
    handleMenuClose();
  };

  const handleEdit = () => {
    onEditION(selectedION);
    handleMenuClose();
  };

  const handleDelete = async () => {
    if (
      selectedION &&
      window.confirm("Are you sure you want to delete this ION?")
    ) {
      try {
        await ionApi.deleteIONNote(selectedION.IONGUID);
        loadIONNotes();
      } catch (error) {
        console.error("Error deleting ION:", error);
      }
    }
    handleMenuClose();
  };

  const handleDownloadScannedCopy = async () => {
    try {
      const response = await ionApi.downloadScannedCopy(selectedION.IONGUID);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", `${selectedION.IONNumber}_Scanned.pdf`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error("Error downloading scanned copy:", error);
      alert("Failed to download scanned copy");
    }
    handleMenuClose();
  };

  const getStatusColor = (status) => {
    switch (status) {
      case "Draft":
        return "default";
      case "Awaiting Approval":
        return "warning";
      case "Approved":
        return "success";
      case "Rejected":
        return "error";
      default:
        return "default";
    }
  };

  const getStatusIcon = (status) => {
    switch (status) {
      case "Draft":
        return <Edit fontSize="small" />;
      case "Awaiting Approval":
        return <Pending fontSize="small" />;
      case "Approved":
        return <CheckCircle fontSize="small" />;
      case "Rejected":
        return <Delete fontSize="small" />;
      default:
        return null;
    }
  };

  const filteredIONs = ionNotes.filter(
    (ion) =>
      (ion.IONNumber || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
      (ion.Subject || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
      (ion.GroupName || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
      (ion.PreparedByName || '').toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleScannedCopyView = async (ion) => {
    try {
      const response = await ionApi.downloadScannedCopy(ion.IONGUID);
      const blob = new Blob([response.data], { type: "application/pdf" });
      const fileUrl = window.URL.createObjectURL(blob);

      // Show in dialog
      setPdfPreviewUrl(fileUrl);
      setPdfPreviewOpen(true);
    } catch (error) {
      console.error("Error loading scanned copy:", error);
      alert("Failed to load scanned copy. Please try again.");
    }
  };

  const handlePdfPreviewClose = () => {
    if (pdfPreviewUrl) {
      window.URL.revokeObjectURL(pdfPreviewUrl);
    }
    setPdfPreviewOpen(false);
    setPdfPreviewUrl(null);
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      {/* Search and Filter Bar */}
      {/* Search and Filter Bar - Combined */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Box
          sx={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            gap: 2,
            flexWrap: "wrap",
          }}
        >
          {/* Status Filter Chips - Left Side */}
          <Box sx={{ display: "flex", gap: 1, flexWrap: "wrap", flex: 1 }}>
            {statusFilters.map((filter) => (
              <Chip
                key={filter.value || "all"}
                label={filter.label}
                onClick={() => handleStatusFilterChange(filter.value)}
                color={statusFilter === filter.value ? "primary" : "default"}
                variant={statusFilter === filter.value ? "filled" : "outlined"}
                size="small"
                sx={{
                  fontWeight: statusFilter === filter.value ? 600 : 400,
                  cursor: "pointer",
                  "&:hover": {
                    backgroundColor:
                      statusFilter === filter.value
                        ? "primary.dark"
                        : "action.hover",
                  },
                }}
              />
            ))}
          </Box>

          {/* Search Box - Right Side */}
          <TextField
            placeholder="Search ION..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            size="small"
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <Search fontSize="small" />
                </InputAdornment>
              ),
            }}
            sx={{
              minWidth: 250,
              maxWidth: 350,
              "& .MuiOutlinedInput-root": {
                backgroundColor: "background.paper",
              },
            }}
          />
        </Box>
      </Paper>
      {/* ION Table */}
      {/* ION Table */}
      {loading ? (
        <Typography>Loading...</Typography>
      ) : filteredIONs.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: "center" }}>
          <Typography variant="h6" color="text.secondary">
            No ION notes found
          </Typography>
          <Typography color="text.secondary" sx={{ mt: 1 }}>
            {statusFilter
              ? `No ION notes with status "${statusFilter}"`
              : "Create your first ION note to get started"}
          </Typography>
        </Paper>
      ) : (
        <TableContainer
          component={Paper}
          sx={{
            maxHeight: "calc(100vh - 280px)", // Adjust based on your layout
            overflow: "auto",
          }}
        >
          <Table stickyHeader>
            <TableHead>
              <TableRow>
                <TableCell
                  sx={{
                    fontWeight: 600,
                    backgroundColor: "background.paper",
                  }}
                >
                  ION Number
                </TableCell>
                <TableCell
                  sx={{
                    fontWeight: 600,
                    backgroundColor: "background.paper",
                  }}
                >
                  Date
                </TableCell>
                <TableCell
                  sx={{
                    fontWeight: 600,
                    backgroundColor: "background.paper",
                  }}
                >
                  Subject
                </TableCell>
                <TableCell
                  sx={{
                    fontWeight: 600,
                    backgroundColor: "background.paper",
                  }}
                >
                  Group
                </TableCell>
                <TableCell
                  sx={{
                    fontWeight: 600,
                    backgroundColor: "background.paper",
                  }}
                >
                  Prepared By
                </TableCell>
                <TableCell
                  sx={{
                    fontWeight: 600,
                    backgroundColor: "background.paper",
                  }}
                >
                  Status
                </TableCell>
                <TableCell
                  sx={{
                    fontWeight: 600,
                    backgroundColor: "background.paper",
                  }}
                >
                  Scanned Copy
                </TableCell>
                <TableCell
                  align="center"
                  sx={{
                    fontWeight: 600,
                    backgroundColor: "background.paper",
                  }}
                >
                  Actions
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredIONs.map((ion) => (
                <TableRow
                  key={ion.IONGUID}
                  hover
                  sx={{ cursor: "pointer" }}
                  onClick={() => onViewION(ion)}
                >
                  <TableCell>
                    <Typography
                      variant="body2"
                      sx={{ fontFamily: "monospace", fontWeight: 600 }}
                    >
                      {ion.IONNumber}
                    </Typography>
                  </TableCell>

                  <TableCell>
                    <Typography variant="body2">
                      {new Date(ion.IONDate).toLocaleDateString()}
                    </Typography>
                  </TableCell>

                  <TableCell>
                    <Typography variant="body2" sx={{ maxWidth: 300 }}>
                      {ion.Subject.length > 50
                        ? `${ion.Subject.substring(0, 50)}...`
                        : ion.Subject}
                    </Typography>
                  </TableCell>

                  <TableCell>
                    <Typography variant="body2">
                      {ion.GroupName || '-'}
                    </Typography>
                  </TableCell>

                  <TableCell>
                    <Typography variant="body2">
                      {ion.PreparedByName}
                    </Typography>
                  </TableCell>

                  <TableCell>
                    <Chip
                      label={ion.Status}
                      color={getStatusColor(ion.Status)}
                      size="small"
                      icon={getStatusIcon(ion.Status)}
                    />
                  </TableCell>

                  <TableCell>
                    {ion.ScannedCopyUploaded ? (
                      <Button
                        size="small"
                        variant="outlined"
                        startIcon={<Visibility />}
                          onClick={(e) => {
                          e.stopPropagation();  // ADD THIS - Stop row click
                          handleScannedCopyView(ion);
                        }}
                        sx={{ textTransform: "none" }}
                      >
                        View
                      </Button>
                    ) : (
                      <Chip
                        label="Pending"
                        color="default"
                        size="small"
                        icon={<Upload />}
                      />
                    )}
                  </TableCell>

                  <TableCell align="center">
                    <IconButton
                      size="small"
                      onClick={(e) => handleMenuOpen(e, ion)}
                    >
                      <MoreVert />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {/* Context Menu */}
      {/* Context Menu */}
      <Menu
        anchorEl={menuAnchor}
        open={Boolean(menuAnchor)}
        onClose={handleMenuClose}
      >
        {/* View Details - Always available */}
        <MenuItem onClick={handleView}>
          <Visibility fontSize="small" sx={{ mr: 1 }} />
          View Details
        </MenuItem>

        {/* Edit - only for preparer, approver (sent through), document handler, or admin */}
        {(isAdmin || isDocHandler || (
          (selectedION?.Status === "Draft" || selectedION?.Status === "Rejected") &&
          (String(selectedION?.PreparedBy) === String(currentUserId) || String(selectedION?.SentThrough) === String(currentUserId))
        )) && (
          <PermissionGuard permission="ION_Edit">
            <MenuItem onClick={handleEdit}>
              <Edit fontSize="small" sx={{ mr: 1 }} />
              Edit
            </MenuItem>
          </PermissionGuard>
        )}

        {/* Delete - Admin only */}
        {isAdmin && (
          <MenuItem onClick={handleDelete}>
            <Delete fontSize="small" sx={{ mr: 1 }} />
            Delete
          </MenuItem>
        )}

        {/* Download Scanned Copy - Only if uploaded */}
        {selectedION?.ScannedCopyUploaded && (
          <MenuItem onClick={handleDownloadScannedCopy}>
            <FileDownload fontSize="small" sx={{ mr: 1 }} />
            Download Scanned Copy
          </MenuItem>
        )}
      </Menu>

      {/* PDF Preview Dialog */}
      <Dialog
        open={pdfPreviewOpen}
        onClose={handlePdfPreviewClose}
        maxWidth="lg"
        fullWidth
        PaperProps={{
          sx: { height: "90vh" },
        }}
      >
        <DialogTitle>
          <Box
            sx={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
            }}
          >
            <Typography variant="h6">Scanned Copy Preview</Typography>
            <IconButton onClick={handlePdfPreviewClose} size="small">
              <Cancel />
            </IconButton>
          </Box>
        </DialogTitle>
        <DialogContent sx={{ p: 0 }}>
          {pdfPreviewUrl && (
            <iframe
              src={pdfPreviewUrl}
              style={{
                width: "100%",
                height: "100%",
                border: "none",
              }}
              title="PDF Preview"
            />
          )}
        </DialogContent>
         
      </Dialog>
    </Box>
  );
};

export default IONListView;
