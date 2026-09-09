import React, { useState, useEffect } from 'react';
import api from '../../services/api';
import { LoadingState, EmptyState, ErrorState } from '../../components/StateComponents';

const CandidateApplications = () => {
    const [applications, setApplications] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchApps = async () => {
            try {
                const response = await api.get('/Candidates/me/applications');
                setApplications(response.data);
            } catch (err) {
                setError('Failed to load applications.');
            } finally {
                setLoading(false);
            }
        };
        fetchApps();
    }, []);

    if (loading) return <LoadingState />;
    if (error) return <ErrorState message={error} />;
    if (applications.length === 0) return <EmptyState message="You haven't applied to any jobs yet." />;

    return (
        <div>
            <h2>My Applications</h2>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
                {applications.map(app => (
                    <div key={app.id} style={{ border: '1px solid #ccc', borderRadius: '8px', padding: '16px', background: '#fff' }}>
                        <h3>{app.jobTitle}</h3>
                        <p style={{ color: '#666', fontWeight: 'bold' }}>{app.companyName}</p>
                        <p style={{ color: '#888', fontSize: '14px' }}>Applied on: {new Date(app.appliedAt).toLocaleDateString()}</p>
                        <div style={{ marginTop: '12px' }}>
                            <span style={{ 
                                padding: '4px 12px', 
                                borderRadius: '16px', 
                                fontSize: '14px', 
                                fontWeight: 'bold',
                                background: app.status === 'Rejected' ? '#fee' : app.status === 'Hired' ? '#efe' : '#eef',
                                color: app.status === 'Rejected' ? '#c00' : app.status === 'Hired' ? '#080' : '#00c'
                            }}>
                                {app.status}
                            </span>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default CandidateApplications;
