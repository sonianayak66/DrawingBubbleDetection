// Helper functions to manage My Day in localStorage
const MY_DAY_STORAGE_KEY = 'taskplanner_my_day';

export const myDayStorage = {
  // Get current user's My Day task list
  getMyDayTasks: (userId) => {
    try {
      const myDayData = JSON.parse(localStorage.getItem(MY_DAY_STORAGE_KEY) || '{}');
      const userTasks = myDayData[userId] || [];
      
      // Clean up old entries (older than 7 days)
      const weekAgo = new Date();
      weekAgo.setDate(weekAgo.getDate() - 7);
      
      const cleanTasks = userTasks.filter(item => 
        new Date(item.addedDate) > weekAgo
      );
      
      // Save cleaned data back
      if (cleanTasks.length !== userTasks.length) {
        myDayData[userId] = cleanTasks;
        localStorage.setItem(MY_DAY_STORAGE_KEY, JSON.stringify(myDayData));
      }
      
      return cleanTasks.map(item => item.taskGuid);
    } catch (err) {
      console.error('Error reading My Day from localStorage:', err);
      return [];
    }
  },

  // Add task to current user's My Day
  addToMyDay: (userId, taskGuid) => {
    try {
      const myDayData = JSON.parse(localStorage.getItem(MY_DAY_STORAGE_KEY) || '{}');
      
      if (!myDayData[userId]) {
        myDayData[userId] = [];
      }
      
      // Check if already exists
      const exists = myDayData[userId].some(item => item.taskGuid === taskGuid);
      
      if (!exists) {
        myDayData[userId].push({
          taskGuid: taskGuid,
          addedDate: new Date().toISOString()
        });
        
        localStorage.setItem(MY_DAY_STORAGE_KEY, JSON.stringify(myDayData));
      }
      
      return true;
    } catch (err) {
      console.error('Error adding to My Day:', err);
      return false;
    }
  },

  // Remove task from current user's My Day
  removeFromMyDay: (userId, taskGuid) => {
    try {
      const myDayData = JSON.parse(localStorage.getItem(MY_DAY_STORAGE_KEY) || '{}');
      
      if (myDayData[userId]) {
        myDayData[userId] = myDayData[userId].filter(item => item.taskGuid !== taskGuid);
        localStorage.setItem(MY_DAY_STORAGE_KEY, JSON.stringify(myDayData));
      }
      
      return true;
    } catch (err) {
      console.error('Error removing from My Day:', err);
      return false;
    }
  },

  // Check if task is in current user's My Day
  isInMyDay: (userId, taskGuid) => {
    const myDayTaskGuids = myDayStorage.getMyDayTasks(userId);
    return myDayTaskGuids.includes(taskGuid);
  }
};