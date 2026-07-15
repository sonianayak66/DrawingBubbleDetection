import React, { useState, useEffect } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Grid,
  Box,
  Typography,
  IconButton,
  Chip,
  Alert,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from "@mui/material";
import { Close, Palette, Add } from "@mui/icons-material";

// Predefined color palette for buckets
const BUCKET_COLORS = [
  "#1976d2", // Blue
  "#388e3c", // Green
  "#f57c00", // Orange
  "#d32f2f", // Red
  "#7b1fa2", // Purple
  "#616161", // Grey
  "#0288d1", // Light Blue
  "#689f38", // Light Green
  "#fbc02d", // Yellow
  "#e64a19", // Deep Orange
  "#8e24aa", // Purple
  "#5d4037", // Brown
];

const BucketDialog = ({ open, onClose, bucket, onSave }) => {
  const [formData, setFormData] = useState({
    BucketGUID: null,
    BucketName: "",
    BucketDescription: "",
    BucketColor: "#1976d2",
    SortOrder: 0,
  });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (bucket && bucket.BucketGUID) {
      // Edit mode - load existing bucket data
      setFormData({
        BucketGUID: bucket.BucketGUID,
        BucketName: bucket.BucketName || "",
        BucketDescription: bucket.BucketDescription || "",
        BucketColor: bucket.BucketColor || "#1976d2",
        SortOrder: bucket.SortOrder || 0,
      });
    } else {
      // Create mode - reset form
      setFormData({
        BucketGUID: null,
        BucketName: "",
        BucketDescription: "",
        BucketColor: "#1976d2",
        SortOrder: 0,
      });
    }
  }, [bucket, open]);

  const handleChange = (field) => (event) => {
    setFormData((prev) => ({
      ...prev,
      [field]: event.target.value,
    }));
  };

  const handleColorSelect = (color) => {
    setFormData((prev) => ({
      ...prev,
      BucketColor: color,
    }));
  };

  const handleSubmit = async () => {
    if (!formData.BucketName.trim()) {
      alert("Bucket name is required");
      return;
    }

    setLoading(true);
    try {
      await onSave(formData);
      // onClose will be handled by parent component after successful save
    } catch (error) {
      console.error("Error saving bucket:", error);
      alert("Error saving bucket: " + error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    if (!loading) {
      onClose();
    }
  };

  const isEditMode = !!(bucket && bucket.BucketGUID);
  const isSystemDefault = bucket?.IsSystemDefault;

  // Suggested bucket names
  const suggestedNames = [
    "To Do",
    "In Progress",
    "Done",
    "Review",
    "Testing",
    "Backlog",
    "On Hold",
    "Completed",
    "Blocked",
    "Ready",
    "Planning",
    "Development",
    "QA",
    "Deployment",
    "Archive",
  ];

  // Sort order options
  const sortOrderOptions = Array.from({ length: 20 }, (_, i) => i + 1);

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <Box>
          <Typography variant="h6">
            {isEditMode ? "Edit Global Bucket" : "Create Global Bucket"}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {isEditMode
              ? "Modify this system-wide bucket"
              : "Create a new bucket available across all projects"}
          </Typography>
        </Box>
        <IconButton onClick={handleClose} disabled={loading}>
          <Close />
        </IconButton>
      </DialogTitle>

      <DialogContent dividers>
        {/* System Default Warning */}
        {isSystemDefault && (
          <Alert severity="warning" sx={{ mb: 3 }}>
            <Typography variant="body2">
              ⚠️ <strong>System Default:</strong> This is a system default
              bucket and cannot be modified
            </Typography>
          </Alert>
        )}

        <Grid container spacing={3}>
          {/* Bucket Name */}
          <Grid item size={{ xs: 9 }}>
            <TextField
              fullWidth
              label="Bucket Name"
              value={formData.BucketName}
              onChange={handleChange("BucketName")}
              disabled={loading || isSystemDefault}
              required
              placeholder="Enter bucket name..."
              helperText={
                isSystemDefault
                  ? "System default buckets cannot be renamed"
                  : ""
              }
            />
          </Grid>

          {/* Sort Order */}
          <Grid item size={{ xs: 12, sm: 3 }}>
            <FormControl fullWidth disabled={loading || isSystemDefault}>
              <InputLabel>Sort Order</InputLabel>
              <Select
                value={formData.SortOrder}
                onChange={handleChange("SortOrder")}
                label="Sort Order"
              >
                {sortOrderOptions.map((order) => (
                  <MenuItem key={order} value={order}>
                    {order}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>

          {/* Quick Name Suggestions */}
          {!isEditMode && (
            <Grid item size={{ xs: 12 }}>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                Quick suggestions:
              </Typography>
              <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1 }}>
                {suggestedNames.slice(0, 8).map((name) => (
                  <Chip
                    key={name}
                    label={name}
                    size="small"
                    variant="outlined"
                    onClick={() =>
                      setFormData((prev) => ({ ...prev, BucketName: name }))
                    }
                    sx={{
                      cursor: "pointer",
                      "&:hover": { backgroundColor: "action.hover" },
                    }}
                  />
                ))}
              </Box>
            </Grid>
          )}

          {/* Bucket Description */}
          <Grid item size={{ xs: 12 }}>
            <TextField
              fullWidth
              label="Description (Optional)"
              value={formData.BucketDescription}
              onChange={handleChange("BucketDescription")}
              disabled={loading || isSystemDefault}
              multiline
              rows={2}
              placeholder="Describe what tasks belong in this bucket..."
            />
          </Grid>

          {/* Bucket Color */}
          <Grid item size={{ xs: 12 }}>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Bucket Color:
            </Typography>
            <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1.5 }}>
              {BUCKET_COLORS.map((color) => (
                <Box
                  key={color}
                  onClick={() => !isSystemDefault && handleColorSelect(color)}
                  sx={{
                    width: 28,
                    height: 28,
                    backgroundColor: color,
                    borderRadius: "8px",
                    cursor: isSystemDefault ? "not-allowed" : "pointer",
                    border: "3px solid",
                    borderColor:
                      formData.BucketColor === color
                        ? "primary.main"
                        : "transparent",
                    opacity: isSystemDefault ? 0.5 : 1,
                    "&:hover": {
                      transform: isSystemDefault ? "none" : "scale(1.1)",
                      transition: "transform 0.2s ease-in-out",
                    },
                    transition: "all 0.2s ease-in-out",
                  }}
                />
              ))}
            </Box>
          </Grid>

          <Grid item xs={12} sm={6}>
            <Box
              sx={{
                p: 2,
                border: "1px solid",
                borderColor: "divider",
                borderRadius: 2,
                backgroundColor: "background.paper",
              }}
            >
              <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                <Typography variant="body2" color="text.secondary">
                  Preview:
                </Typography>
                <Box
                  sx={{
                    width: 16,
                    height: 16,
                    backgroundColor: formData.BucketColor || "#ccc",
                    borderRadius: "50%",
                  }}
                />
                <Typography variant="body1" sx={{ fontWeight: 500 }}>
                  {formData.BucketName || "Bucket Name"}
                </Typography>
              </Box>
            </Box>
          </Grid>
        </Grid>
      </DialogContent>

      <DialogActions sx={{ p: 3 }}>
        <Button onClick={handleClose} disabled={loading} color="inherit">
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={loading || !formData.BucketName.trim() || isSystemDefault}
          startIcon={loading ? null : <Add />}
        >
          {loading
            ? "Saving..."
            : isEditMode
            ? "Update Bucket"
            : "Create Bucket"}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default BucketDialog;
