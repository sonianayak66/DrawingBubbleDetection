import React from 'react';
import InwardIONLayout from './InwardIONLayout';

const InwardIONRouter = ({ selectedView = 'inward-ion-list', onViewChange, viewContext = {} }) => {
  return (
    <InwardIONLayout
      selectedView={selectedView}
      onViewChange={onViewChange}
      viewContext={viewContext}
    />
  );
};

export default InwardIONRouter;
