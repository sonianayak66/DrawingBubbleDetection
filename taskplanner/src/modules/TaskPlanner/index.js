// TaskPlanner Module Entry Point
export { default as TaskPlannerLayout } from './layout/TaskPlannerLayout';
export { default as TaskPlannerRouter } from './layout/TaskPlannerRouter';
export { default as TaskPlannerSidebar } from './layout/TaskPlannerSidebar';

// Export module metadata
export const MODULE_INFO = {
  name: 'TaskPlanner',
  displayName: 'Task Planner',
  path: '/taskplanner',
  icon: 'Assignment',
  permissions: ['TaskPlanner_Tasks_Read'],
};