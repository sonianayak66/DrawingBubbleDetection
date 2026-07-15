import React, { useState, useEffect } from "react";
import {
  Box,
  Typography,
  Paper,
  Grid,
  Button,
  Chip,
  List,
  ListItem,
  ListItemText,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Alert,
  CircularProgress,
  Autocomplete,
  Collapse,
} from "@mui/material";
import {
  Edit,
  Delete,
  CheckCircle,
  Cancel as CancelIcon,
  FileDownload,
  Upload,
  AttachFile,
  Print,
  ArrowBack,
  ExpandMore,
  ExpandLess,
} from "@mui/icons-material";
import { ionApi } from "../../../services/ionApi";
import PermissionGuard from "../../shared/components/Common/PermissionGuard";
import { usePermissions } from "../../../context/PermissionsContext";
import { useUser } from "../../../context/UserContext";
import { generateIONPdf } from "../utils/generateIONPdf";

const IONDetailView = ({ ionGuid, onEdit, onBack, onDelete, refreshTrigger }) => {
  const { hasPermission } = usePermissions();
  const { getCurrentUserId } = useUser();
  const isAdmin = hasPermission("ION_Admin");
  const isDocHandler = hasPermission("ION_SupportOperator");
  const currentUserId = getCurrentUserId();
  const [ionData, setIonData] = useState(null);
  const [enclosures, setEnclosures] = useState([]);
  const [loading, setLoading] = useState(true);
  const [pdfDialogOpen, setPdfDialogOpen] = useState(false);
  const [pdfBlobUrl, setPdfBlobUrl] = useState(null);
  const [uploadingScannedCopy, setUploadingScannedCopy] = useState(false);
  const [users, setUsers] = useState([]);
  const [approvedByUser, setApprovedByUser] = useState("");
  // Old: recipients from ION_NoteRecipients table - now using ToAddress/CopyTo text
  // const [toRecipients, setToRecipients] = useState([]);
  // const [copyToRecipients, setCopyToRecipients] = useState([]);
  const [expandedEnclosure, setExpandedEnclosure] = useState(null);
  const [enclosureAttachments, setEnclosureAttachments] = useState({});
  const [uploadingEnclosure, setUploadingEnclosure] = useState(null);
  const [printingEnclosures, setPrintingEnclosures] = useState(false);

  useEffect(() => {
    if (ionGuid) {
      loadIONDetails();
    }
    // refreshTrigger forces a reload after an edit-save round trip when the
    // IONGUID is unchanged (same record being viewed before and after edit).
  }, [ionGuid, refreshTrigger]);

  // Temporarily change document.title while the PDF dialog is open so the browser's
  // "Save as PDF" / Print dialog uses the ION number as the default filename.
  useEffect(() => {
    if (!pdfDialogOpen || !ionData?.IONNumber) return;

    const originalTitle = document.title;
    // Sanitize ION number for use as filename (remove slashes, etc.)
    const safeName = String(ionData.IONNumber).replace(/[\\/:*?"<>|]/g, '-');
    document.title = safeName;

    return () => {
      document.title = originalTitle;
    };
  }, [pdfDialogOpen, ionData?.IONNumber]);

  const loadIONDetails = async () => {
    try {
      setLoading(true);
      const [ionResponse, enclosuresResponse, usersResponse] = await Promise.all([
        ionApi.getIONNoteDetail(ionGuid),
        ionApi.getEnclosures(ionGuid),
        ionApi.getInternalUsers(),
      ]);

      if (ionResponse.data) {
        setIonData(ionResponse.data);

        // Old: Parse recipients from ION_NoteRecipients table
        // const data = ionResponse.data;
        // setToRecipients(
        //   data.ToGroupNames
        //     ? data.ToGroupNames.split(',').map((name, i) => ({
        //         GroupName: name.trim(),
        //         GroupGUID: data.ToGroupGUIDs ? data.ToGroupGUIDs.split(',')[i]?.trim() : ''
        //       }))
        //     : []
        // );
        // setCopyToRecipients(
        //   data.CopyToGroupNames
        //     ? data.CopyToGroupNames.split(',').map((name, i) => ({
        //         GroupName: name.trim(),
        //         GroupGUID: data.CopyToGroupGUIDs ? data.CopyToGroupGUIDs.split(',')[i]?.trim() : ''
        //       }))
        //     : []
        // );
      }
      const loadedEnclosures = enclosuresResponse.data || [];
      setEnclosures(loadedEnclosures);
      setUsers(usersResponse.data || []);

      // Preload attachments for enclosures that have files
      const enclosuresWithAttachments = loadedEnclosures.filter(e => e.HasAttachment);
      if (enclosuresWithAttachments.length > 0) {
        const attachmentPromises = enclosuresWithAttachments.map(e =>
          ionApi.getEnclosureAttachments(e.EnclosureId)
            .then(res => ({ id: e.EnclosureId, data: res.data || [] }))
            .catch(() => ({ id: e.EnclosureId, data: [] }))
        );
        const results = await Promise.all(attachmentPromises);
        const attachmentsMap = {};
        results.forEach(r => { attachmentsMap[r.id] = r.data; });
        setEnclosureAttachments(attachmentsMap);
      }
    } catch (error) {
      console.error("Error loading ION details:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleScannedCopyUpload = async (event) => {
    const file = event.target.files[0];
    if (!file) return;

    // Validate file type
    if (file.type !== "application/pdf") {
      alert("Please upload a PDF file only");
      return;
    }

    // Validate file size (10MB)
    if (file.size > 10 * 1024 * 1024) {
      alert("File size must be less than 10MB");
      return;
    }

    if (!approvedByUser) {
      alert("Please select 'Approved By' person before uploading");
      // Reset file input
      event.target.value = "";
      return;
    }

    try {
      setUploadingScannedCopy(true);
      await ionApi.uploadScannedCopy(ionGuid, file, approvedByUser);
      alert("Scanned copy uploaded and ION marked as approved successfully");
      loadIONDetails(); // Refresh to show uploaded status
    } catch (error) {
      console.error("Error uploading scanned copy:", error);
      alert("Failed to upload scanned copy. Please try again.");
    } finally {
      setUploadingScannedCopy(false);
    }
  };

  const handleScannedCopyDownload = async () => {
    try {
      const response = await ionApi.downloadScannedCopy(ionGuid);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", `${ionData.IONNumber}_Scanned.pdf`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error("Error downloading scanned copy:", error);
      alert("Failed to download scanned copy");
    }
  };

  const handleGeneratePDF = async () => {
    try {
      const blobUrl = await generateIONPdf(ionData, enclosures);
      setPdfBlobUrl(blobUrl);
      setPdfDialogOpen(true);
    } catch (error) {
      console.error("Error generating PDF:", error);
      alert("Failed to generate PDF. Please try again.");
    }
  };

  const handlePdfDialogClose = () => {
    // Cleanup the blob URL to free memory
    if (pdfBlobUrl) {
      URL.revokeObjectURL(pdfBlobUrl);
      setPdfBlobUrl(null);
    }
    setPdfDialogOpen(false);
  };

  const handleDownloadPdf = () => {
    if (pdfBlobUrl) {
      const link = document.createElement("a");
      link.href = pdfBlobUrl;
      link.download = `${ionData.IONNumber}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  };

  // Print all enclosure attachments that are PDF or image.
  // Word documents (.doc / .docx) are intentionally NOT printed — server-side
  // conversion to HTML did not reliably reproduce the original layout, so users
  // should open those files in Word and print from there.
  const handlePrintEnclosures = async () => {
    try {
      setPrintingEnclosures(true);

      // Step 1: Load attachments for all enclosures that have files
      const withAttachments = enclosures.filter(e => e.HasAttachment);
      if (withAttachments.length === 0) {
        alert("No enclosure attachments found.");
        return;
      }

      const attResults = await Promise.all(
        withAttachments.map(e =>
          ionApi.getEnclosureAttachments(e.EnclosureId)
            .then(res => res.data || [])
            .catch(() => [])
        )
      );

      // Flatten all attachments
      const allAttachments = attResults.flat();

      // Step 2: Filter for printable file types (PDF + images only)
      const printableExtensions = ['.pdf', '.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp'];

      const printableAttachments = allAttachments.filter(att => {
        const name = (att.Orginal_File_Name || '').toLowerCase();
        return printableExtensions.some(ext => name.endsWith(ext));
      });

      // Collect Word files so we can tell the user they were skipped
      const skippedWordFiles = allAttachments.filter(att => {
        const name = (att.Orginal_File_Name || '').toLowerCase();
        return name.endsWith('.doc') || name.endsWith('.docx');
      });

      if (printableAttachments.length === 0) {
        let msg = "No printable enclosures found. Only PDF and image files can be printed.";
        if (skippedWordFiles.length > 0) {
          msg += "\n\nWord documents (.doc/.docx) are not supported here — please open them in Word and print from there.";
        }
        alert(msg);
        return;
      }

      // Step 3: Download PDF / image files as blobs
      const fileResults = await Promise.all(
        printableAttachments.map(att => {
          const name = att.Orginal_File_Name || '';
          const isPdf = name.toLowerCase().endsWith('.pdf');
          return ionApi.downloadEnclosureAttachment(att.Attachment_Db_Key)
            .then(res => ({
              type: isPdf ? 'pdf' : 'image',
              blob: new Blob([res.data]),
              name: name,
            }))
            .catch(() => null);
        })
      );

      const validFiles = fileResults.filter(f => f !== null);
      if (validFiles.length === 0) {
        alert("Failed to prepare enclosure files for printing.");
        return;
      }

      // If any Word files were skipped, let the user know so they aren't confused
      // about missing pages in the print output.
      if (skippedWordFiles.length > 0) {
        const names = skippedWordFiles.map(a => a.Orginal_File_Name).join(", ");
        alert(
          `The following Word document(s) will NOT be printed:\n\n${names}\n\n` +
          `Please open them in Word and print from there.`
        );
      }

      // Step 4: Open a print window with all files rendered
      const printWindow = window.open('', '_blank');
      if (!printWindow) {
        alert("Please allow pop-ups to print enclosures.");
        return;
      }

      // Build HTML content with embedded images and PDFs
      let htmlContent = `
        <html>
        <head>
          <title>Enclosures - ${ionData.IONNumber}</title>
          <style>
            @media print {
              .enclosure-item { page-break-after: always; }
              .enclosure-item:last-child { page-break-after: avoid; }
            }
            body { margin: 0; padding: 20px; font-family: Arial, sans-serif; }
            .enclosure-item { padding: 10px; }
            .enclosure-item.image-item { text-align: center; }
            .enclosure-item img { max-width: 100%; max-height: 90vh; object-fit: contain; }
            .enclosure-label { font-size: 11px; color: #666; margin-bottom: 8px; text-align: left; border-bottom: 1px solid #ddd; padding-bottom: 4px; }
            .pdf-container { width: 100%; min-height: 90vh; }
            .pdf-container canvas { max-width: 100%; }
          </style>
        </head>
        <body>
      `;

      // Separate by type
      const imageFiles = validFiles.filter(f => f.type === 'image');
      const pdfFiles = validFiles.filter(f => f.type === 'pdf');

      // Add images directly
      for (const file of imageFiles) {
        const dataUrl = await blobToDataUrl(file.blob);
        htmlContent += `
          <div class="enclosure-item image-item">
            <div class="enclosure-label">${file.name}</div>
            <img src="${dataUrl}" alt="${file.name}" />
          </div>
        `;
      }

      // For PDFs, render each page as an image using pdf.js from CDN
      if (pdfFiles.length > 0) {
        htmlContent += `
          <script src="https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.min.js"><\/script>
          <script>
            pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';

            async function renderPdfs() {
              const pdfDataArray = [${pdfFiles.map((f, i) => `window.__pdfData${i}`).join(',')}];
              const pdfNames = [${pdfFiles.map(f => `"${f.name.replace(/"/g, '\\"')}"`).join(',')}];

              for (let p = 0; p < pdfDataArray.length; p++) {
                try {
                  const pdfDoc = await pdfjsLib.getDocument({ data: pdfDataArray[p] }).promise;
                  for (let i = 1; i <= pdfDoc.numPages; i++) {
                    const page = await pdfDoc.getPage(i);
                    const scale = 2;
                    const viewport = page.getViewport({ scale });
                    const canvas = document.createElement('canvas');
                    canvas.width = viewport.width;
                    canvas.height = viewport.height;
                    const ctx = canvas.getContext('2d');
                    await page.render({ canvasContext: ctx, viewport }).promise;

                    const div = document.createElement('div');
                    div.className = 'enclosure-item';
                    const label = document.createElement('div');
                    label.className = 'enclosure-label';
                    label.textContent = pdfNames[p] + (pdfDoc.numPages > 1 ? ' (Page ' + i + ' of ' + pdfDoc.numPages + ')' : '');
                    div.appendChild(label);
                    const img = document.createElement('img');
                    img.src = canvas.toDataURL('image/png');
                    img.style.maxWidth = '100%';
                    div.appendChild(img);
                    document.body.appendChild(div);
                  }
                } catch (e) {
                  console.error('Error rendering PDF:', pdfNames[p], e);
                }
              }
              // Auto-print after rendering
              setTimeout(() => window.print(), 500);
            }
          <\/script>
        `;
      }

      htmlContent += `</body></html>`;

      if (pdfFiles.length > 0) {
        // Convert PDF blobs to Uint8Array before writing to the print window
        const pdfDataArrays = [];
        for (let i = 0; i < pdfFiles.length; i++) {
          const arrayBuffer = await pdfFiles[i].blob.arrayBuffer();
          pdfDataArrays.push(new Uint8Array(arrayBuffer));
        }

        printWindow.document.open();
        printWindow.document.write(htmlContent);
        printWindow.document.close();

        // Pass PDF data to the print window
        for (let i = 0; i < pdfDataArrays.length; i++) {
          printWindow.window[`__pdfData${i}`] = pdfDataArrays[i];
        }

        // Wait for pdf.js CDN script to load, then render
        const waitForPdfJs = () => {
          return new Promise((resolve) => {
            const check = () => {
              if (printWindow.pdfjsLib) {
                resolve();
              } else {
                setTimeout(check, 100);
              }
            };
            check();
          });
        };
        await waitForPdfJs();
        printWindow.eval('renderPdfs()');
      } else {
        // Images only — just print
        printWindow.document.open();
        printWindow.document.write(htmlContent);
        printWindow.document.close();

        // Wait for images to load, then print
        printWindow.onload = () => {
          setTimeout(() => printWindow.print(), 300);
        };
        // Fallback if onload already fired
        setTimeout(() => printWindow.print(), 1000);
      }

    } catch (error) {
      console.error("Error printing enclosures:", error);
      alert("Failed to print enclosures. Please try again.");
    } finally {
      setPrintingEnclosures(false);
    }
  };

  // Helper: convert Blob to data URL
  const blobToDataUrl = (blob) => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result);
      reader.onerror = reject;
      reader.readAsDataURL(blob);
    });
  };

  // Enclosure attachment handlers
  const loadEnclosureAttachments = async (enclosureId) => {
    try {
      const response = await ionApi.getEnclosureAttachments(enclosureId);
      setEnclosureAttachments((prev) => ({
        ...prev,
        [enclosureId]: response.data || [],
      }));
    } catch (error) {
      console.error("Error loading enclosure attachments:", error);
    }
  };

  const handleToggleEnclosure = (enclosureId) => {
    if (expandedEnclosure === enclosureId) {
      setExpandedEnclosure(null);
    } else {
      setExpandedEnclosure(enclosureId);
      if (!enclosureAttachments[enclosureId]) {
        loadEnclosureAttachments(enclosureId);
      }
    }
  };

  const handleEnclosureFileUpload = async (event, enclosureId) => {
    const file = event.target.files[0];
    if (!file) return;

    try {
      setUploadingEnclosure(enclosureId);
      await ionApi.uploadEnclosureAttachment(enclosureId, file);
      await loadEnclosureAttachments(enclosureId);
      // Refresh enclosures to update HasAttachment
      const encResponse = await ionApi.getEnclosures(ionGuid);
      setEnclosures(encResponse.data || []);
    } catch (error) {
      console.error("Error uploading attachment:", error);
      alert("Failed to upload attachment");
    } finally {
      setUploadingEnclosure(null);
      event.target.value = "";
    }
  };

  const handleDownloadAttachment = async (attachmentId, fileName) => {
    try {
      const response = await ionApi.downloadEnclosureAttachment(attachmentId);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", fileName);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error("Error downloading attachment:", error);
      alert("Failed to download attachment");
    }
  };

  const handleDeleteAttachment = async (attachmentId, enclosureId) => {
    if (!window.confirm("Are you sure you want to delete this attachment?")) return;
    try {
      await ionApi.deleteEnclosureAttachment(attachmentId);
      await loadEnclosureAttachments(enclosureId);
      // Refresh enclosures to update HasAttachment
      const encResponse = await ionApi.getEnclosures(ionGuid);
      setEnclosures(encResponse.data || []);
    } catch (error) {
      console.error("Error deleting attachment:", error);
      alert("Failed to delete attachment");
    }
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

  if (loading) {
    return (
      <Box
        sx={{
          p: 3,
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          height: "50vh",
        }}
      >
        <CircularProgress />
      </Box>
    );
  }

  if (!ionData) {
    return (
      <Box sx={{ p: 3, textAlign: "center" }}>
        <Typography color="error">ION not found</Typography>
        <Button startIcon={<ArrowBack />} onClick={onBack} sx={{ mt: 2 }}>
          Back to List
        </Button>
      </Box>
    );
  }

  // Edit allowed only for: the person who prepared it, the person approving (sent through), or admin
  const isPreparerOrApprover =
    String(ionData.PreparedBy) === String(currentUserId) ||
    String(ionData.SentThrough) === String(currentUserId);
  const canEdit = isAdmin || isDocHandler || (isPreparerOrApprover && ionData.Status !== "Approved");

  return (
    <Box sx={{ p: 2, maxWidth: 1400, mx: "auto" }}>
      {/* Header with Actions */}
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          mb: 2,
        }}
      >
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 600 }}>
            {ionData.IONNumber}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {ionData.Subject}
          </Typography>
        </Box>

        <Box sx={{ display: "flex", gap: 1, alignItems: "center" }}>
          <Chip
            label={ionData.Status}
            color={getStatusColor(ionData.Status)}
            size="medium"
          />

          <Button
            startIcon={<Print />}
            variant="outlined"
            size="small"
            onClick={handleGeneratePDF}
          >
            Print ION
          </Button>

          {enclosures.length > 0 && (
            <Button
              startIcon={<Print />}
              variant="outlined"
              size="small"
              onClick={handlePrintEnclosures}
              disabled={printingEnclosures}
            >
              {printingEnclosures ? "Loading..." : "Print Enclosures"}
            </Button>
          )}

          <Button
            startIcon={<ArrowBack />}
            variant="outlined"
            size="small"
            onClick={onBack}
          >
            Back to ION List
          </Button>
        </Box>
      </Box>

      {/* Action Buttons */}
      <Box sx={{ display: "flex", gap: 1, mb: 2 }}>
        <PermissionGuard permission="ION_Edit">
          <Button
            startIcon={<Edit />}
            onClick={() => onEdit(ionData)}
            disabled={!canEdit}
            variant="outlined"
            size="small"
          >
            Edit
          </Button>
        </PermissionGuard>

        <PermissionGuard permission="ION_Admin">
          <Button
            startIcon={<Delete />}
            onClick={() => onDelete(ionData)}
            color="error"
            variant="outlined"
            size="small"
          >
            Delete
          </Button>
        </PermissionGuard>

      </Box>

      {/* ION Details */}
      <div id="ion-content-for-pdf">
        <Grid container spacing={2} sx={{ pb: 6 }}>
          {/* Basic Information */}
          <Grid size={{ xs: 12 }}>
            <Paper sx={{ p: 2 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1.5 }}>
                Basic Information
              </Typography>

              <Grid container spacing={2}>
                <Grid size={{ xs: 12, md: 6 }}>
                  <Typography variant="caption" color="text.secondary">
                    Group
                  </Typography>
                  <Typography variant="body2">
                    {ionData.GroupName ? `${ionData.GroupName} — ${ionData.ReferenceNo || ''}` : '-'}
                  </Typography>
                </Grid>

                <Grid size={{ xs: 12, md: 3 }}>
                  <Typography variant="caption" color="text.secondary">
                    ION Date
                  </Typography>
                  <Typography variant="body2">
                    {new Date(ionData.IONDate).toLocaleDateString()}
                  </Typography>
                </Grid>

                <Grid size={{ xs: 12, md: 6 }}>
                  <Typography variant="caption" color="text.secondary">
                    Subject
                  </Typography>
                  <Typography variant="body2">{ionData.Subject}</Typography>
                </Grid>

                {ionData.CommunicationReference && (
                  <Grid size={{ xs: 12 }}>
                    <Typography variant="caption" color="text.secondary">
                      Reference
                    </Typography>
                    <Box sx={{ mt: 0.5 }}>
                      {ionData.CommunicationReference
                        .split("\n")
                        .map((line) => line.trim())
                        .filter((line) => line)
                        .map((line, index) => (
                          <Typography key={index} variant="body2" sx={{ mb: 0.25 }}>
                            {index + 1}. {line}
                          </Typography>
                        ))}
                    </Box>
                  </Grid>
                )}
              </Grid>
            </Paper>
          </Grid>

          {/* ION Body */}

          <Grid size={{ xs: 12 }}>
            <Paper sx={{ p: 2 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1.5 }}>
                ION Content
              </Typography>
              <Box
                sx={{
                  border: "1px solid",
                  borderColor: "divider",
                  borderRadius: 1,
                  p: 2,
                  minHeight: 200,
                  backgroundColor: "background.paper",
                }}
                dangerouslySetInnerHTML={{ __html: ionData.IONBody }}
              />
            </Paper>
          </Grid>

          {/* Enclosures */}
          {enclosures.length > 0 && (
            <Grid size={{ xs: 12 }}>
              <Paper sx={{ p: 2 }}>
                <Typography
                  variant="subtitle2"
                  sx={{ fontWeight: 600, mb: 1.5 }}
                >
                  Enclosures ({enclosures.length})
                </Typography>
                <List dense disablePadding>
                  {enclosures.map((enclosure, index) => (
                    <React.Fragment key={enclosure.EnclosureGUID || index}>
                      <ListItem
                        divider={index < enclosures.length - 1 && expandedEnclosure !== enclosure.EnclosureId}
                        secondaryAction={
                          <Box sx={{ display: "flex", alignItems: "center", gap: 0.5 }}>
                            {enclosure.HasAttachment ? (
                              <Chip
                                label="Files Attached"
                                size="small"
                                color="success"
                                icon={<AttachFile />}
                              />
                            ) : null}
                            <IconButton
                              size="small"
                              onClick={() => handleToggleEnclosure(enclosure.EnclosureId)}
                            >
                              {expandedEnclosure === enclosure.EnclosureId ? <ExpandLess /> : <ExpandMore />}
                            </IconButton>
                          </Box>
                        }
                        sx={{ cursor: "pointer" }}
                        onClick={() => handleToggleEnclosure(enclosure.EnclosureId)}
                      >
                        <ListItemText
                          primary={enclosure.EnclosureDescription}
                        />
                      </ListItem>

                      <Collapse
                        in={expandedEnclosure === enclosure.EnclosureId}
                        timeout="auto"
                        unmountOnExit
                      >
                        <Box sx={{ pl: 3, pr: 2, py: 1.5, bgcolor: "grey.50", borderBottom: index < enclosures.length - 1 ? "1px solid" : "none", borderColor: "divider" }}>
                          {/* Upload button */}
                          <PermissionGuard permission="ION_Edit">
                            <Box sx={{ mb: 1.5 }}>
                              <input
                                accept=".pdf,.doc,.docx,.xls,.xlsx,.jpg,.jpeg,.png,.gif"
                                style={{ display: "none" }}
                                id={`enclosure-upload-${enclosure.EnclosureId}`}
                                type="file"
                                onChange={(e) => handleEnclosureFileUpload(e, enclosure.EnclosureId)}
                              />
                              <label htmlFor={`enclosure-upload-${enclosure.EnclosureId}`}>
                                <Button
                                  component="span"
                                  startIcon={<Upload />}
                                  size="small"
                                  variant="outlined"
                                  disabled={uploadingEnclosure === enclosure.EnclosureId}
                                >
                                  {uploadingEnclosure === enclosure.EnclosureId ? "Uploading..." : "Upload File"}
                                </Button>
                              </label>
                            </Box>
                          </PermissionGuard>

                          {/* Attachment list */}
                          {enclosureAttachments[enclosure.EnclosureId]?.length > 0 ? (
                            <List dense disablePadding>
                              {enclosureAttachments[enclosure.EnclosureId].map((att) => (
                                <ListItem
                                  key={att.Attachment_Db_Key}
                                  disablePadding
                                  sx={{ py: 0.5 }}
                                  secondaryAction={
                                    <Box sx={{ display: "flex", gap: 0.5 }}>
                                      <IconButton
                                        size="small"
                                        color="primary"
                                        onClick={() => handleDownloadAttachment(att.Attachment_Db_Key, att.Orginal_File_Name)}
                                        title="Download"
                                      >
                                        <FileDownload fontSize="small" />
                                      </IconButton>
                                      <PermissionGuard permission="ION_Edit">
                                        <IconButton
                                          size="small"
                                          color="error"
                                          onClick={() => handleDeleteAttachment(att.Attachment_Db_Key, enclosure.EnclosureId)}
                                          title="Delete"
                                        >
                                          <Delete fontSize="small" />
                                        </IconButton>
                                      </PermissionGuard>
                                    </Box>
                                  }
                                >
                                  <ListItemText
                                    primary={
                                      <Typography variant="body2" sx={{ fontSize: 13 }}>
                                        <AttachFile sx={{ fontSize: 14, mr: 0.5, verticalAlign: "middle" }} />
                                        {att.Orginal_File_Name}
                                      </Typography>
                                    }
                                    secondary={
                                      <Typography variant="caption" color="text.secondary">
                                        {att.UploadedByName && `${att.UploadedByName} • `}
                                        {att.Updated_on ? new Date(att.Updated_on).toLocaleDateString() : ''}
                                      </Typography>
                                    }
                                  />
                                </ListItem>
                              ))}
                            </List>
                          ) : (
                            <Typography variant="caption" color="text.secondary" sx={{ fontStyle: "italic" }}>
                              No files attached
                            </Typography>
                          )}
                        </Box>
                      </Collapse>
                    </React.Fragment>
                  ))}
                </List>
              </Paper>
            </Grid>
          )}

          {/* To Address, Prepared By, Sent Through */}
          <Grid size={{ xs: 12 }}>
            <Paper sx={{ p: 2 }}>
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, md: 3 }}>
                  <Typography variant="caption" color="text.secondary">
                    To,
                  </Typography>
                  <Typography variant="body2" sx={{ whiteSpace: "pre-line" }}>
                    {ionData.ToAddress || '-'}
                  </Typography>
                </Grid>

                {ionData.CopyTo && (
                  <Grid size={{ xs: 12, md: 3 }}>
                    <Typography variant="caption" color="text.secondary">
                      Copy To
                    </Typography>
                    <Box sx={{ mt: 0.5 }}>
                      {ionData.CopyTo
                        .split("\n")
                        .map((line) => line.trim())
                        .filter((line) => line)
                        .map((line, index) => (
                          <Typography key={index} variant="body2">
                            {index + 1}. {line}
                          </Typography>
                        ))}
                    </Box>
                  </Grid>
                )}

                <Grid size={{ xs: 12, md: 3 }}>
                  <Typography variant="caption" color="text.secondary">
                    Prepared By
                  </Typography>
                  <Typography variant="body2">{ionData.PreparedByName}</Typography>
                  {ionData.PreparedByDesignation && (
                    <Typography variant="caption" color="text.secondary" display="block">
                      {ionData.PreparedByDesignation}
                    </Typography>
                  )}
                </Grid>

                <Grid size={{ xs: 12, md: 3 }}>
                  <Typography variant="caption" color="text.secondary">
                    Sent Through (For Approval)
                  </Typography>
                  <Typography variant="body2">
                    {ionData.SentThroughName || '-'}
                  </Typography>
                  {ionData.SentThroughDesignation && (
                    <Typography variant="caption" color="text.secondary" display="block">
                      {ionData.SentThroughDesignation}
                    </Typography>
                  )}
                </Grid>
              </Grid>
            </Paper>
          </Grid>

          {/* Approval Information */}
          {ionData.Status === "Approved" && (
            <Grid size={{ xs: 12 }}>
              <Paper sx={{ p: 2 }}>
                <Typography
                  variant="subtitle2"
                  sx={{ fontWeight: 600, mb: 1.5 }}
                >
                  Approval Information
                </Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, md: 3 }}>
                    <Typography variant="caption" color="text.secondary">
                      Approved By
                    </Typography>
                    <Typography variant="body2">
                      {ionData.ApprovedByName}
                    </Typography>
                  </Grid>

                  <Grid size={{ xs: 12, md: 3 }}>
                    <Typography variant="caption" color="text.secondary">
                      Approved Date
                    </Typography>
                    <Typography variant="body2">
                      {ionData.ApprovedDate ? new Date(ionData.ApprovedDate).toLocaleDateString() : '-'}
                    </Typography>
                  </Grid>

                  <Grid size={{ xs: 12, md: 3 }}>
                    <Typography variant="caption" color="text.secondary">
                      Scanned Copy Uploaded By
                    </Typography>
                    <Typography variant="body2">
                      {ionData.ScannedCopyUploadedByName || '-'}
                    </Typography>
                  </Grid>

                  <Grid size={{ xs: 12, md: 3 }}>
                    <Typography variant="caption" color="text.secondary">
                      Uploaded Date
                    </Typography>
                    <Typography variant="body2">
                      {ionData.ScannedCopyUploadedDate ? new Date(ionData.ScannedCopyUploadedDate).toLocaleDateString() : '-'}
                    </Typography>
                  </Grid>
                </Grid>
              </Paper>
            </Grid>
          )}

          {/* Scanned Copy & Approval Section */}
          <Grid size={{ xs: 12 }}>
            <Paper sx={{ p: 2, mb: 10 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1.5 }}>
                Scanned Copy {ionData.ScannedCopyUploaded ? "" : "& Approval"}
              </Typography>
              {!ionData.ScannedCopyUploaded && (
                <Typography variant="caption" color="text.secondary" display="block" sx={{ mb: 2 }}>
                  Upload the signed and scanned copy of the ION. Selecting the approver and uploading will mark this ION as approved.
                </Typography>
              )}

              <Box sx={{ display: "flex", alignItems: "center", gap: 2, flexWrap: "wrap" }}>
                <Chip
                  label={ionData.ScannedCopyUploaded ? "File Attached" : "Pending"}
                  color={ionData.ScannedCopyUploaded ? "success" : "warning"}
                  icon={ionData.ScannedCopyUploaded ? <CheckCircle /> : <Upload />}
                  size="small"
                />

                {ionData.ScannedCopyUploaded ? (
                  <Button
                    startIcon={<FileDownload />}
                    size="small"
                    variant="outlined"
                    onClick={handleScannedCopyDownload}
                  >
                    Download
                  </Button>
                ) : (
                  <PermissionGuard permission="ION_SupportOperator">
                    <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
                      <Autocomplete
                        options={users}
                        getOptionLabel={(u) => u.UserName || ''}
                        value={users.find(u => u.UserDbkey === approvedByUser) || null}
                        onChange={(_, selected) => setApprovedByUser(selected ? selected.UserDbkey : "")}
                        size="small"
                        sx={{ minWidth: 250 }}
                        renderOption={(props, u) => (
                          <li {...props} key={u.UserDbkey}>
                            <Box>
                              <Typography variant="body2" sx={{ fontWeight: 500 }}>
                                {u.UserName} {u.Designation ? `— ${u.Designation}` : ''}
                              </Typography>
                              {u.DepartmentName && (
                                <Typography variant="caption" color="text.secondary">
                                  {u.DepartmentName}
                                </Typography>
                              )}
                            </Box>
                          </li>
                        )}
                        renderInput={(params) => (
                          <TextField
                            {...params}
                            label="Approved By"
                            required
                            size="small"
                          />
                        )}
                      />

                      <input
                        accept="application/pdf"
                        style={{ display: "none" }}
                        id="scanned-copy-upload"
                        type="file"
                        onChange={handleScannedCopyUpload}
                      />
                      <label htmlFor="scanned-copy-upload">
                        <Button
                          component="span"
                          startIcon={<Upload />}
                          size="small"
                          variant="contained"
                          disabled={uploadingScannedCopy || !approvedByUser}
                        >
                          {uploadingScannedCopy ? "Uploading..." : "Upload & Approve"}
                        </Button>
                      </label>
                    </Box>
                  </PermissionGuard>
                )}
              </Box>
            </Paper>
          </Grid>
        </Grid>
      </div>
      {/* PDF Preview Dialog */}
      <Dialog
        open={pdfDialogOpen}
        onClose={handlePdfDialogClose}
        maxWidth={false}
        PaperProps={{
          sx: {
            width: "90vw",
            height: "90vh",
            maxWidth: "90vw",
            maxHeight: "90vh",
          },
        }}
      >
        <DialogTitle
          sx={{
            borderBottom: 1,
            borderColor: "divider",
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            py: 1.5,
          }}
        >
          <Typography variant="h6">
            ION Preview - {ionData.IONNumber}
          </Typography>
          <Box sx={{ display: "flex", gap: 1 }}>
            <Button
              startIcon={<FileDownload />}
              onClick={handleDownloadPdf}
              variant="contained"
              size="small"
            >
              Download
            </Button>
            <IconButton onClick={handlePdfDialogClose} size="small">
              <CancelIcon />
            </IconButton>
          </Box>
        </DialogTitle>

        <DialogContent sx={{ p: 0, overflow: "hidden" }}>
          {pdfBlobUrl && (
            <iframe
              src={pdfBlobUrl}
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

export default IONDetailView;
