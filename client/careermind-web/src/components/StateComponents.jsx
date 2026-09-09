import React from 'react';

export const LoadingState = () => (
    <div style={{ padding: '40px', textAlign: 'center', color: '#666' }}>
        <p>Loading data...</p>
    </div>
);

export const EmptyState = ({ message = "No records found." }) => (
    <div style={{ padding: '40px', textAlign: 'center', color: '#888', border: '1px dashed #ccc', borderRadius: '8px', margin: '20px 0' }}>
        <p>{message}</p>
    </div>
);

export const ErrorState = ({ message = "An error occurred." }) => (
    <div style={{ padding: '20px', textAlign: 'center', color: '#c00', background: '#fee', border: '1px solid #fcc', borderRadius: '8px', margin: '20px 0' }}>
        <p>{message}</p>
    </div>
);
