import axios from 'axios';

const api = axios.create({
  baseURL: '/api/taskplanner',
   //baseURL: `${window.location.origin}/api/taskplanner`,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Important for cookie authentication
});

// Request interceptor
api.interceptors.request.use(
  (config) => {
    //console.log('API Request:', config.method?.toUpperCase(), config.url);
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor
api.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    console.error('API Error:', error.response?.data || error.message);
    return Promise.reject(error);
  }
);

export const taskPlannerApi = {

  getPermissions: () => api.get('/permissions'),

  getUsers: () => api.get('/getUsers'),
  // Projects
  getProjects: (projectGuid = null) => api.get('/projects', { params: { projectGuid } }),
  saveProject: (projectData) => api.post('/projects/save', projectData),
  deleteProject: (projectGuid) => api.post('/projects/delete', { ProjectGUID: projectGuid }),

  // Project Buckets
  getBuckets: (projectGuid = null, bucketGuid = null) => 
    api.get('/buckets', { params: { projectGuid, bucketGuid } }),
  saveBucket: (bucketData) => api.post('/buckets/save', bucketData),
  deleteBucket: (bucketGuid) => api.post('/buckets/delete', { BucketGUID: bucketGuid }),
  // Add to taskPlannerApi object


  // Tasks
  getTasks: (projectGuid = null, bucketGuid = null, taskGuid = null) => 
    api.get('/tasks', { params: { projectGuid, bucketGuid, taskGuid } }),
  saveTask: (taskData) => api.post('/tasks/save', taskData),
  deleteTask: (taskGuid) => api.post('/tasks/delete', { TaskGUID: taskGuid }),


  // Add to taskPlannerApi object
getTaskAssignments: (taskGuid) => api.get('/assignments', { params: { taskGuid } }),
saveTaskAssignment: (assignmentData) => api.post('/assignments/save', assignmentData),
deleteTaskAssignment: (deleteData) => api.post('/assignments/delete', deleteData),

getTaskComments: (taskGuid) => api.get('/comments', { params: { taskGuid } }),
saveTaskComment: (commentData) => api.post('/comments/save', commentData),
deleteTaskComment: (deleteData) => api.post('/comments/delete', deleteData),

getTaskChecklists: (taskGuid) => api.get('/checklists', { params: { taskGuid } }),
saveTaskChecklist: (checklistData) => api.post('/checklists/save', checklistData),
deleteTaskChecklist: (deleteData) => api.post('/checklists/delete', deleteData),

getTaskActivities: (taskGuid) => api.get('/activities', { params: { taskGuid } }),
saveTaskActivity: (activityData) => api.post('/activities/save', activityData),

// Email Management APIs
getEmails: async (params = {}) => {
  const queryParams = new URLSearchParams();
  
  if (params.emailGuid) queryParams.append('emailGuid', params.emailGuid);
  if (params.isConverted !== null && params.isConverted !== undefined) 
    queryParams.append('isConverted', params.isConverted);
  if (params.convertedTaskGuid) queryParams.append('convertedTaskGuid', params.convertedTaskGuid);
  if (params.fromDate) queryParams.append('fromDate', params.fromDate);
  if (params.toDate) queryParams.append('toDate', params.toDate);
  if (params.pageSize) queryParams.append('pageSize', params.pageSize);
  if (params.pageNumber) queryParams.append('pageNumber', params.pageNumber);

  const response = await fetch(`/api/taskplanner/emails?${queryParams}`, {
    method: 'GET',
    credentials: 'include',
  });

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }

  return await response.json();
},

convertEmailToTask: async (conversionData) => {
  const response = await fetch('/api/taskplanner/emails/convert', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify(conversionData),
  });

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }

  return await response.json();
},

getEmailConfigurations: async (params = {}) => {
  const queryParams = new URLSearchParams();
  
  if (params.configGuid) queryParams.append('configGuid', params.configGuid);
  if (params.isActive !== null && params.isActive !== undefined) 
    queryParams.append('isActive', params.isActive);

  const response = await fetch(`/api/taskplanner/email-configs?${queryParams}`, {
    method: 'GET',
    credentials: 'include',
  });

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }

  return await response.json();
},

saveEmailConfiguration: async (configData) => {
  const response = await fetch('/api/taskplanner/email-configs/save', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify(configData),
  });

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }

  return await response.json();
},

testEmailConfiguration: async (testData) => {
  const response = await fetch('/api/taskplanner/email-configs/test', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify(testData),
  });

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }

  return await response.json();
},

syncEmailsNow: async () => {
  const response = await fetch('/api/taskplanner/emails/sync-now', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify({}),
  });

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }

  return await response.json();
},

};

export default api;