// inwardIonApi.js — API client for Inward ION module
import axios from "axios";

const API_BASE = import.meta.env.DEV
  ? "https://localhost:7030/api/inwardion"
  : "/api/inwardion";

const api = axios.create({ baseURL: API_BASE, withCredentials: true });

api.interceptors.request.use(
  (config) => config,
  (error) => Promise.reject(error),
);
api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error("Inward ION API Error:", error.response?.data || error.message);
    return Promise.reject(error);
  },
);

export const inwardIonApi = {
  // Inward Notes CRUD
  saveInwardNote: (noteData) => api.post("/save", noteData),

  getInwardNotes: (params = {}) => api.post("/list", params),

  getInwardNoteDetail: (inwardIONGUID) =>
    api.post("/detail", { InwardIONGUID: inwardIONGUID }),

  deleteInwardNote: (inwardIONGUID) =>
    api.post("/delete", { InwardIONGUID: inwardIONGUID }),

  // Attachments
  uploadAttachment: (inwardNoteId, file) => {
    const formData = new FormData();
    formData.append("file", file);
    formData.append("inwardNoteId", inwardNoteId);
    return api.post("/attachment/upload", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });
  },

  getAttachments: (inwardNoteId) =>
    api.get(`/attachment/list/${inwardNoteId}`),

  downloadAttachment: (attachmentId) =>
    api.get(`/attachment/download/${attachmentId}`, { responseType: "blob" }),

  deleteAttachment: (attachmentId) =>
    api.post("/attachment/delete", { AttachmentId: attachmentId }),
};
