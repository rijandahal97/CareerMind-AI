import React from 'react';
import { AlertCircle, SearchX, RefreshCcw, Inbox } from 'lucide-react';

const baseStyles = {
    padding: '48px 24px',
    textAlign: 'center',
    borderRadius: '12px',
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    gap: '12px',
    fontFamily: 'Inter, sans-serif',
};

export const LoadingState = ({ message = "Loading data..." }) => (
    <div style={{ ...baseStyles, background: '#f8fafc', border: '1px solid #e2e8f0' }}>
        <div style={{ display: 'flex', gap: '6px', alignItems: 'center' }}>
            {[0, 1, 2].map(i => (
                <div key={i} style={{
                    width: 8, height: 8,
                    borderRadius: '50%',
                    background: '#3b82f6',
                    animation: `blink 1.2s ${i * 0.2}s infinite ease-in-out`,
                }} />
            ))}
        </div>
        <p style={{ margin: 0, color: '#64748b', fontSize: 14 }}>{message}</p>
        <style>{`@keyframes blink { 0%, 80%, 100% { opacity: 0.2; transform: scale(0.8); } 40% { opacity: 1; transform: scale(1); } }`}</style>
    </div>
);

export const EmptyState = ({ message = "No records found.", icon }) => (
    <div style={{ ...baseStyles, background: '#f8fafc', border: '1px dashed #cbd5e1' }}>
        {icon || <Inbox size={40} color="#94a3b8" />}
        <p style={{ margin: 0, color: '#64748b', fontWeight: 500, fontSize: 15 }}>{message}</p>
    </div>
);

export const ErrorState = ({ message = "An error occurred.", onRetry }) => (
    <div style={{ ...baseStyles, background: '#fff5f5', border: '1px solid #fca5a5' }}>
        <AlertCircle size={40} color="#ef4444" />
        <p style={{ margin: 0, color: '#c00', fontWeight: 500, fontSize: 15 }}>{message}</p>
        {onRetry && (
            <button 
                onClick={onRetry} 
                style={{ 
                    display: 'flex', alignItems: 'center', gap: 6,
                    marginTop: 8, padding: '8px 16px',
                    background: '#ef4444', color: 'white', border: 'none',
                    borderRadius: 8, cursor: 'pointer', fontSize: 14, fontWeight: 500 
                }}
            >
                <RefreshCcw size={14} /> Try Again
            </button>
        )}
    </div>
);

export const NoResultsState = ({ message = "No results found.", suggestion }) => (
    <div style={{ ...baseStyles, background: '#f8fafc', border: '1px dashed #cbd5e1' }}>
        <SearchX size={40} color="#94a3b8" />
        <p style={{ margin: 0, color: '#64748b', fontWeight: 500, fontSize: 15 }}>{message}</p>
        {suggestion && <p style={{ margin: 0, color: '#94a3b8', fontSize: 13 }}>{suggestion}</p>}
    </div>
);
