// ION Module Entry Point  
export { default as IONLayout } from './layout/IONLayout';
export { default as IONRouter } from './layout/IONRouter';
 

// Export module metadata
export const MODULE_INFO = {
  name: 'ION',
  displayName: 'Inter Office Notes',
  path: '/ion',
  icon: 'Description',
  permissions: ['ION_View'],
};