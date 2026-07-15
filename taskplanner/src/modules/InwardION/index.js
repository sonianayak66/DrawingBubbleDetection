// Inward ION Module Entry Point
export { default as InwardIONLayout } from './layout/InwardIONLayout';
export { default as InwardIONRouter } from './layout/InwardIONRouter';

export const MODULE_INFO = {
  name: 'InwardION',
  displayName: 'Inward IONs',
  path: '/inward-ion',
  icon: 'MoveToInbox',
  permissions: ['ION_Inward_View'],
};
