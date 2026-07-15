import React, { useState, useEffect, useCallback } from "react";
import {
  Box,
  Typography,
  Button,
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
  Pagination,
  Dialog,
  DialogTitle,
  DialogContent,
} from "@mui/material";
import {
  MoreVert,
  Edit,
  Delete,
  Visibility,
  Search,
  Cancel,
  AttachFile,
  CheckCircle,
} from "@mui/icons-material";
import { inwardIonApi } from "../../../services/inwardIonApi";
import PermissionGuard from "../../shared/components/Common/PermissionGuard";
import { usePermissions } from "../../../context/PermissionsContext";
import { useUser } from "../../../context/UserContext";

const PAGE_SIZE = 50;

// Default date range: today minus 31 days → today
const todayStr = () => new Date().toISOString().slice(0, 10);
const monthAgoStr = () => {
  const d = new Date();
  d.setDate(d.getDate() - 31);
  return d.toISOString().slice(0, 10);
};

const InwardIONListView = ({
  onCreateNote,
  onEditNote,
  onViewNote,
  refreshTrigger = 0,
}) => {
  const { hasPermission } = usePermissions();
  const { getCurrentUserName } = useUser();
  const isAdmin = hasPermission("ION_Inward_Admin");
  const canEdit = hasPermission("ION_Inward_Edit");
  const canDelete = hasPermission("ION_Inward_Delete");
  const isSupportOp = hasPermission("ION_SupportOperator");

  const [notes, setNotes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterFrom, setFilterFrom] = useState("");
  const [filterTo, setFilterTo] = useState("");
  const [dateFrom, setDateFrom] = useState(monthAgoStr());
  const [dateTo, setDateTo] = useState(todayStr());
  const [menuAnchor, setMenuAnchor] = useState(null);
  const [selectedNote, setSelectedNote] = useState(null);
  const [activeTab, setActiveTab] = useState("all"); // "all" | "my"

  // Debounced search
  const [searchDebounce, setSearchDebounce] = useState("");
  useEffect(() => {
    const timer = setTimeout(() => setSearchDebounce(searchTerm), 400);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  useEffect(() => {
    loadNotes();
  }, [page, searchDebounce, filterFrom, filterTo, dateFrom, dateTo, activeTab, refreshTrigger]);

  const loadNotes = async () => {
    try {
      setLoading(true);
      // For "My IONs" tab, filter by current user name in AddressedTo
      const addressedToFilter = activeTab === "my"
        ? getCurrentUserName()
        : (filterTo || null);

      const response = await inwardIonApi.getInwardNotes({
        PageNumber: page,
        SearchText: searchDebounce || null,
        FromDepartment: filterFrom || null,
        AddressedTo: addressedToFilter,
        DateFrom: dateFrom || null,
        DateTo: dateTo || null,
      });
      const data = response.data || [];
      setNotes(data);
      setTotalCount(data.length > 0 ? data[0].TotalCount : 0);
    } catch (error) {
      console.error("Error loading inward notes:", error);
      setNotes([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  };

  const totalPages = Math.ceil(totalCount / PAGE_SIZE);

  const handleMenuOpen = (event, note) => {
    event.stopPropagation();
    setMenuAnchor(event.currentTarget);
    setSelectedNote(note);
  };

  const handleMenuClose = () => {
    setMenuAnchor(null);
    setSelectedNote(null);
  };

  const handleView = () => {
    onViewNote(selectedNote);
    handleMenuClose();
  };

  const handleEdit = () => {
    onEditNote(selectedNote);
    handleMenuClose();
  };

  const handleDelete = async () => {
    if (
      selectedNote &&
      window.confirm("Are you sure you want to permanently delete this Inward ION?")
    ) {
      try {
        await inwardIonApi.deleteInwardNote(selectedNote.InwardIONGUID);
        loadNotes();
      } catch (error) {
        console.error("Error deleting Inward ION:", error);
      }
    }
    handleMenuClose();
  };

  const formatDate = (dateStr) => {
    if (!dateStr) return "-";
    return new Date(dateStr).toLocaleDateString("en-IN", {
      day: "2-digit",
      month: "short",
      year: "numeric",
    });
  };

  // Render newline-separated text as tiny chips. Each chip caps at 140px and
  // uses ellipsis so a single long entry can't blow the column out horizontally.
  const renderMiniChips = (text) => {
    if (!text) return null;
    const items = text.split("\n").map((s) => s.trim()).filter((s) => s);
    if (items.length === 0) return null;
    return (
      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.25 }}>
        {items.map((item, i) => (
          <Chip
            key={i}
            label={item}
            size="small"
            variant="outlined"
            title={item}
            sx={{
              height: 20,
              maxWidth: 140,
              "& .MuiChip-label": {
                px: 0.5,
                fontSize: "0.7rem",
                overflow: "hidden",
                textOverflow: "ellipsis",
                whiteSpace: "nowrap",
              },
            }}
          />
        ))}
      </Box>
    );
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* Tab Chips + Filters */}
      <Paper sx={{ p: 2, mb: 3 }}>
        {/* Row 1: Tabs */}
        <Box sx={{ display: "flex", gap: 1, mb: 1.5 }}>
          {[
            { key: "all", label: "All IONs" },
            { key: "my", label: "My IONs" },
          ].map((tab) => (
            <Chip
              key={tab.key}
              label={tab.label}
              onClick={() => { setActiveTab(tab.key); setPage(1); }}
              color={activeTab === tab.key ? "primary" : "default"}
              variant={activeTab === tab.key ? "filled" : "outlined"}
              size="small"
              sx={{
                fontWeight: activeTab === tab.key ? 600 : 400,
                cursor: "pointer",
              }}
            />
          ))}
        </Box>

        {/* Row 2: Search + Date filters */}
        <Box
          sx={{
            display: "flex",
            gap: 2,
            flexWrap: "wrap",
            alignItems: "center",
          }}
        >
          <TextField
            placeholder="Search..."
            value={searchTerm}
            onChange={(e) => { setSearchTerm(e.target.value); setPage(1); }}
            size="small"
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <Search fontSize="small" />
                </InputAdornment>
              ),
            }}
            sx={{ minWidth: 220, maxWidth: 300 }}
          />

          <TextField
            label="Date From"
            type="date"
            value={dateFrom}
            onChange={(e) => { setDateFrom(e.target.value); setPage(1); }}
            size="small"
            InputLabelProps={{ shrink: true }}
            sx={{
              width: 170,
              // Force the native Chrome calendar icon to be visible — by default
              // it can render with very low opacity in some themes.
              '& input::-webkit-calendar-picker-indicator': {
                cursor: 'pointer',
                opacity: 0.7,
              },
            }}
          />

          <TextField
            label="Date To"
            type="date"
            value={dateTo}
            onChange={(e) => { setDateTo(e.target.value); setPage(1); }}
            size="small"
            InputLabelProps={{ shrink: true }}
            sx={{
              width: 170,
              '& input::-webkit-calendar-picker-indicator': {
                cursor: 'pointer',
                opacity: 0.7,
              },
            }}
          />

          {(searchTerm || dateFrom !== monthAgoStr() || dateTo !== todayStr()) && (
            <Button
              size="small"
              onClick={() => {
                setSearchTerm("");
                setDateFrom(monthAgoStr());
                setDateTo(todayStr());
                setFilterFrom("");
                setFilterTo("");
                setPage(1);
              }}
            >
              Reset Filters
            </Button>
          )}

          <Box sx={{ flexGrow: 1 }} />

          <Typography variant="body2" color="text.secondary">
            {totalCount} record{totalCount !== 1 ? "s" : ""}
          </Typography>
        </Box>
      </Paper>

      {/* Table */}
      {loading ? (
        <Typography>Loading...</Typography>
      ) : notes.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: "center" }}>
          <Typography variant="h6" color="text.secondary">
            No inward IONs found
          </Typography>
          <Typography color="text.secondary" sx={{ mt: 1 }}>
            {searchTerm || dateFrom || dateTo
              ? "No records match your filters"
              : "Log your first inward ION to get started"}
          </Typography>
        </Paper>
      ) : (
        <>
          <TableContainer
            component={Paper}
            sx={{
              maxHeight: "calc(100vh - 340px)",
              overflow: "auto",
            }}
          >
            <Table
              stickyHeader
              size="small"
              sx={{
                // Fixed layout makes the browser honor the explicit column widths
                // below — without this, long content (e.g. Subject) blows columns
                // out and overlaps neighbouring cells.
                tableLayout: "fixed",
                "& .MuiTableCell-root": { py: 0.5, px: 1, fontSize: "0.8rem" },
              }}
            >
              <colgroup>
                <col style={{ width: 100 }} />{/* Rcvd Date */}
                <col style={{ width: 140 }} />{/* Ref. No. */}
                <col style={{ width: 130 }} />{/* From */}
                <col />{/* Subject — flexible */}
                <col style={{ width: 200 }} />{/* Addressed To */}
                <col style={{ width: 180 }} />{/* Copy To */}
                <col style={{ width: 50 }} />{/* Attachment */}
                <col style={{ width: 50 }} />{/* Ack */}
                <col style={{ width: 44 }} />{/* Actions */}
              </colgroup>
              <TableHead>
                <TableRow>
                  <TableCell sx={{ fontWeight: 600, backgroundColor: "background.paper", whiteSpace: "nowrap" }}>
                    Rcvd Date
                  </TableCell>
                  <TableCell sx={{ fontWeight: 600, backgroundColor: "background.paper", whiteSpace: "nowrap" }}>
                    Ref. No.
                  </TableCell>
                  <TableCell sx={{ fontWeight: 600, backgroundColor: "background.paper", whiteSpace: "nowrap" }}>
                    From
                  </TableCell>
                  <TableCell sx={{ fontWeight: 600, backgroundColor: "background.paper" }}>
                    Subject
                  </TableCell>
                  <TableCell sx={{ fontWeight: 600, backgroundColor: "background.paper", whiteSpace: "nowrap" }}>
                    Addressed To
                  </TableCell>
                  <TableCell sx={{ fontWeight: 600, backgroundColor: "background.paper", whiteSpace: "nowrap" }}>
                    Copy To
                  </TableCell>
                  <TableCell sx={{ fontWeight: 600, backgroundColor: "background.paper" }} align="center">
                    <AttachFile sx={{ fontSize: 14 }} />
                  </TableCell>
                  <TableCell sx={{ fontWeight: 600, backgroundColor: "background.paper" }} align="center">
                    Ack
                  </TableCell>
                  <TableCell align="center" sx={{ fontWeight: 600, backgroundColor: "background.paper" }}>
                  </TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {notes.map((note) => (
                  <TableRow
                    key={note.InwardIONGUID}
                    hover
                    sx={{ cursor: "pointer" }}
                    onClick={() => onViewNote(note)}
                  >
                    <TableCell sx={{ whiteSpace: "nowrap" }}>
                      {formatDate(note.ReceivedDate)}
                    </TableCell>

                    <TableCell sx={{ overflow: "hidden" }}>
                      <Box
                        sx={{
                          fontFamily: "monospace",
                          fontWeight: 600,
                          fontSize: "0.75rem",
                          overflow: "hidden",
                          textOverflow: "ellipsis",
                          whiteSpace: "nowrap",
                        }}
                        title={note.IONReferenceNumber || ""}
                      >
                        {note.IONReferenceNumber || "-"}
                      </Box>
                    </TableCell>

                    <TableCell sx={{ overflow: "hidden" }} title={note.FromDepartment}>
                      <Box
                        sx={{
                          fontSize: "0.75rem",
                          overflow: "hidden",
                          textOverflow: "ellipsis",
                          whiteSpace: "nowrap",
                        }}
                      >
                        {note.FromDepartment}
                      </Box>
                    </TableCell>

                    <TableCell sx={{ overflow: "hidden" }} title={note.Subject}>
                      <Box
                        sx={{
                          fontSize: "0.75rem",
                          // Line clamp to 2 lines — anything beyond is hidden
                          // and the second line ends with an ellipsis.
                          display: "-webkit-box",
                          WebkitLineClamp: 2,
                          WebkitBoxOrient: "vertical",
                          overflow: "hidden",
                          textOverflow: "ellipsis",
                          whiteSpace: "normal",
                          wordBreak: "break-word",
                          lineHeight: 1.3,
                        }}
                      >
                        {note.Subject}
                      </Box>
                    </TableCell>

                    <TableCell sx={{ overflow: "hidden" }}>
                      {renderMiniChips(note.AddressedTo)}
                    </TableCell>

                    <TableCell sx={{ overflow: "hidden" }}>
                      {renderMiniChips(note.CopyTo)}
                    </TableCell>

                    <TableCell align="center">
                      {note.AttachmentCount > 0 ? (
                        <Chip label={note.AttachmentCount} size="small" variant="outlined" sx={{ height: 20, "& .MuiChip-label": { px: 0.75, fontSize: "0.7rem" } }} />
                      ) : null}
                    </TableCell>

                    <TableCell align="center">
                      {note.AcknowledgmentSent ? (
                        <CheckCircle sx={{ fontSize: 16 }} color="success" />
                      ) : null}
                    </TableCell>

                    <TableCell align="center" sx={{ px: 0 }}>
                      <IconButton
                        size="small"
                        onClick={(e) => handleMenuOpen(e, note)}
                        sx={{ p: 0.25 }}
                      >
                        <MoreVert sx={{ fontSize: 18 }} />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          {/* Pagination */}
          {totalPages > 1 && (
            <Box sx={{ display: "flex", justifyContent: "center", mt: 2 }}>
              <Pagination
                count={totalPages}
                page={page}
                onChange={(_, newPage) => setPage(newPage)}
                color="primary"
                showFirstButton
                showLastButton
              />
            </Box>
          )}
        </>
      )}

      {/* Context Menu */}
      <Menu
        anchorEl={menuAnchor}
        open={Boolean(menuAnchor)}
        onClose={handleMenuClose}
      >
        <MenuItem onClick={handleView}>
          <Visibility fontSize="small" sx={{ mr: 1 }} />
          View Details
        </MenuItem>

        {(isAdmin || canEdit || isSupportOp) && (
          <MenuItem onClick={handleEdit}>
            <Edit fontSize="small" sx={{ mr: 1 }} />
            Edit
          </MenuItem>
        )}

        {(isAdmin || canDelete) && (
          <MenuItem onClick={handleDelete}>
            <Delete fontSize="small" sx={{ mr: 1 }} color="error" />
            Delete
          </MenuItem>
        )}
      </Menu>
    </Box>
  );
};

export default InwardIONListView;
