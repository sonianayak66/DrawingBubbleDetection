// ionApi.js (fixed)
import axios from "axios";

const API_BASE = import.meta.env.DEV
  ? "https://localhost:7030/api/ion"
  : "/api/ion";

const api = axios.create({ baseURL: API_BASE, withCredentials: true });

api.interceptors.request.use(
  (config) => config,
  (error) => Promise.reject(error),
);
api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error("ION API Error:", error.response?.data || error.message);
    return Promise.reject(error);
  },
);

export const ionApi = {
  // Permissions & Users (adjust if these actually belong to taskplanner namespace)
  getPermissions: () => api.get("/permissions"),
  getUsers: () => api.get("/users"),
  getInternalUsers: () => api.get("/internal-users"),

  // ION Notes
  getIONNotes: (params = {}) => api.get("/notes", { params }),
  saveIONNote: (noteData) => api.post("/notes/save", noteData),
  deleteIONNote: (ionGuid) => api.post("/notes/delete", { IONGUID: ionGuid }),
  approveIONNote: (approvalData) => api.post("/notes/approve", approvalData),
  getIONNoteDetail: (ionGuid) =>
    api.get("/notes/detail", { params: { ionGuid } }),

  // Configuration
  getOfficeConfig: () => api.get("/office-config"),
  saveOfficeConfig: (configData) => api.post("/office-config/save", configData), // ADD THIS
  deleteOfficeConfig: (configId) =>
    api.post("/office-config/delete", { ConfigId: configId }), // ADD THIS

  getDestinations: (destinationGuid = null) =>
    api.get("/destinations", { params: { destinationGuid } }),
  saveDestination: (destinationData) =>
    api.post("/destinations/save", destinationData), // ADD THIS
  deleteDestination: (destinationId) =>
    api.post("/destinations/delete", { DestinationId: destinationId }), // ADD THIS

  // Demands search (for Communication Reference mentions)
  searchDemands: (query) =>
    api.get("/search-demands", { params: { query } }),

  // Enclosures
  getEnclosures: (ionGuid = null, enclosureGuid = null) =>
    api.get("/enclosures", { params: { ionGuid, enclosureGuid } }),
  saveEnclosure: (enclosureData) => api.post("/enclosures/save", enclosureData),
  deleteEnclosure: (enclosureGuid) =>
    api.post("/enclosures/delete", { EnclosureGUID: enclosureGuid }),

  // Enclosure Attachments
  uploadEnclosureAttachment: (enclosureId, file) => {
    const formData = new FormData();
    formData.append("file", file);
    formData.append("enclosureId", enclosureId);
    return api.post("/enclosures/upload", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });
  },
  getEnclosureAttachments: (enclosureId) =>
    api.get(`/enclosures/attachments/${enclosureId}`),
  downloadEnclosureAttachment: (attachmentId) =>
    api.get(`/enclosures/download/${attachmentId}`, { responseType: "blob" }),
  deleteEnclosureAttachment: (attachmentId) =>
    api.post(`/enclosures/delete-attachment/${attachmentId}`),
  getScannedCopy: (ionGuid) => api.get(`/scanned-copy/${ionGuid}`),

  uploadScannedCopy: (ionGuid, file, approvedBy = null) => {
    const formData = new FormData();
    formData.append("file", file);
    formData.append("ionGuid", ionGuid);
    if (approvedBy) {
      formData.append("approvedBy", approvedBy);
    }
    return api.post("/notes/upload-scanned-copy", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
  },
  downloadScannedCopy: (ionGuid) =>
    api.get(`/notes/download-scanned-copy/${ionGuid}`, {
      responseType: "blob",
    }),

  // File Groups
  getFileGroups: () => api.get("/file-groups"),
  saveFileGroup: (data) => api.post("/file-groups/save", data),
  toggleFileGroupActive: (data) => api.post("/file-groups/toggle-active", data),

  // ION Templates (shared library — admin manages, anyone with create can apply)
  getIONTemplates: () => api.get("/templates"),
  saveIONTemplate: (data) => api.post("/templates/save", data),
  deleteIONTemplate: (templateGuid) =>
    api.post("/templates/delete", { TemplateGUID: templateGuid }),
  incrementIONTemplateUse: (templateGuid) =>
    api.post("/templates/increment-use", { TemplateGUID: templateGuid }),
};
