import React from 'react';
import TaskPlannerLayout from './TaskPlannerLayout';

const TaskPlannerRouter = ({ selectedView = 'my-day', onViewChange, viewContext = {} }) => {
  return (
    <TaskPlannerLayout
      selectedView={selectedView}
      onViewChange={onViewChange}
      viewContext={viewContext}
    />
  );
};

export default TaskPlannerRouter;