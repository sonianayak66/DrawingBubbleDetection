import React from 'react';
import IONLayout from './IONLayout';

const IONRouter = ({ selectedView = 'ion-list', onViewChange, viewContext = {} }) => {
  return (
    <IONLayout
      selectedView={selectedView}
      onViewChange={onViewChange}
      viewContext={viewContext}
    />
  );
};

export default IONRouter;