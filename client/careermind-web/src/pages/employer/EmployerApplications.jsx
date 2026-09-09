import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import api from '../../services/api';
import { LoadingState, EmptyState, ErrorState } from '../../components/StateComponents';

const EmployerApplications = () => {
    const { id: jobId } = useParams();
    const [applications, setApplications] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchApps = async () => {
            try {
                const response = await api.get(`/Employers/me/jobs/${jobId}/applications`);
                setApplications(response.data);
            } catch (err) {
                setError('Failed to load applications.');
            } finally {
                setLoading(false);
            }
        };
        fetchApps();
    }, [jobId]);

    const handleStatusChange = async (appId, newStatus) => {
        try {
            await api.put(`/Employers/me/applications/${appId}/status`, { status: newStatus });
            setApplications(applications.map(app => app.id === appId ? { ...app, status: newStatus } : app));
        } catch (err) {
            alert('Failed to update status');
        }
    };

    if (loading) return <LoadingState />;
    if (error) return <ErrorState message={error} />;

    return (
        <div style={{ background: '#fff', padding: '30px', borderRadius: '8px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <h2>Job Applications</h2>
                <Link to="/employer/jobs" style={{ color: '#0066cc', textDecoration: 'none' }}>&larr; Back to Jobs</Link>
            </div>
            
            {applications.length === 0 ? (
                <EmptyState message="No applications received yet." />
            ) : (
                <div style={{ marginTop: '20px', display: 'flex', flexDirection: 'column', gap: '16px' }}>
                    {applications.map(app => (
                        <div key={app.id} style={{ border: '1px solid #ddd', padding: '16px', borderRadius: '8px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <div>
                                <h3 style={{ margin: '0 0 8px 0' }}>{app.candidateName}</h3>
                                <p style={{ margin: '0 0 4px 0', color: '#666', fontSize: '14px' }}>Applied on: {new Date(app.appliedAt).toLocaleDateString()}</p>
                                {app.resumeUrl && <a href={app.resumeUrl} target="_blank" rel="noopener noreferrer" style={{ color: '#0066cc', fontSize: '14px' }}>View Resume</a>}
                            </div>
                            
                            <div>
                                <label style={{ marginRight: '8px', fontWeight: 'bold', fontSize: '14px' }}>Status:</label>
                                <select 
                                    value={app.status} 
                                    onChange={(e) => handleStatusChange(app.id, e.target.value)}
                                    style={{ padding: '6px 12px', borderRadius: '4px', border: '1px solid #ccc' }}
                                >
                                    <option value="Applied">Applied</option>
                                    <option value="UnderReview">Under Review</option>
                                    <option value="Shortlisted">Shortlisted</option>
                                    <option value="Interview">Interview</option>
                                    <option value="Rejected">Rejected</option>
                                    <option value="Hired">Hired</option>
                                </select>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default EmployerApplications;
