import React from 'react';

export const Spinner = ({ center = true }) => {
  if (center) {
    return (
      <div className="loading-center">
        <div className="spinner"></div>
      </div>
    );
  }
  return <div className="spinner"></div>;
};
