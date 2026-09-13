import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../../services/api';
import { LoadingState, EmptyState, ErrorState } from '../../components/StateComponents';
import { FileText, Building2, Clock, ChevronRight } from 'lucide-react';

const statusConfigMap = {
    Applied: { bg: '#eff6ff', color: '#3b82f6', border: '#dbeafe' },
    Shortlisted: { bg: '#ecfdf5', color: '#10b981', border: '#d1fae5' },
    Interviewing: { bg: '#fef3c7', color: '#d97706', border: '#fed7aa' },
    Rejected: { bg: '#fff1f2', color: '#f43f5e', border: '#fecdd3' },
    Hired: { bg: '#f0fdf4', color: '#166534', border: '#bbf7d0' },
    Pending: { bg: '#f8fafc', color: '#64748b', border: '#e2e8f0' },
};

const StatusBadge = ({ status }) => {
    const cfg = statusConfigMap[status] || statusConfigMap['Pending'];
    return (
        <span style={{
            padding: '4px 14px',
            borderRadius: 20,
            fontSize: 12,
            fontWeight: 600,
            background: cfg.bg,
            color: cfg.color,
            border: `1px solid ${cfg.border}`,
            whiteSpace: 'nowrap',
        }}>
            {status || 'Pending Review'}
        </span>
    );
};

const CandidateApplications = () => {
    const [applications, setApplications] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const fetchApps = async () => {
        setLoading(true);
        setError(null);
        try {
            const response = await api.get('/Candidates/me/applications');
            setApplications(response.data);
        } catch (err) {
            setError('Failed to load applications. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { fetchApps(); }, []);

    if (loading) return <LoadingState message="Loading your applications..." />;
    if (error) return <ErrorState message={error} onRetry={fetchApps} />;
    if (applications.length === 0) return (
        <div style={{ paddingTop: 8 }}>
            <EmptyState
                message="You haven't applied to any jobs yet."
                icon={<FileText size={40} color="#94a3b8" />}
            />
        </div>
    );

    return (
        <div className="animated-fade">
            <div style={{ marginBottom: 24 }}>
                <h2 style={{ margin: '0 0 4px 0', fontSize: 22, color: '#0f172a' }}>My Applications</h2>
                <p style={{ margin: 0, color: '#64748b', fontSize: 14 }}>Track the status of all your job applications</p>
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
                {applications.map(app => (
                    <div key={app.id} style={{
                        background: 'white',
                        border: '1px solid #e2e8f0',
                        borderRadius: 12,
                        padding: '20px 24px',
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'center',
                        transition: 'border-color 0.2s, box-shadow 0.2s',
                    }}
                        onMouseEnter={e => { e.currentTarget.style.borderColor = '#94a3b8'; e.currentTarget.style.boxShadow = '0 4px 12px rgba(0,0,0,0.06)'; }}
                        onMouseLeave={e => { e.currentTarget.style.borderColor = '#e2e8f0'; e.currentTarget.style.boxShadow = 'none'; }}
                    >
                        <div style={{ display: 'flex', gap: 16, alignItems: 'center' }}>
                            <div style={{
                                width: 44, height: 44, background: '#eff6ff', borderRadius: 10,
                                display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0,
                            }}>
                                <FileText size={22} color="#3b82f6" />
                            </div>
                            <div>
                                <h3 style={{ margin: '0 0 4px 0', fontSize: 16, color: '#0f172a' }}>{app.jobTitle}</h3>
                                <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
                                    <span style={{ display: 'flex', alignItems: 'center', gap: 4, fontSize: 13, color: '#64748b', fontWeight: 500 }}>
                                        <Building2 size={13} /> {app.companyName}
                                    </span>
                                    <span style={{ display: 'flex', alignItems: 'center', gap: 4, fontSize: 12, color: '#94a3b8' }}>
                                        <Clock size={12} /> {new Date(app.appliedAt).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}
                                    </span>
                                </div>
                            </div>
                        </div>
                        <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
                            <StatusBadge status={app.status} />
                            <Link
                                to={`/candidate/jobs/${app.jobId}`}
                                style={{
                                    display: 'flex', alignItems: 'center', gap: 4,
                                    padding: '7px 14px', background: '#f8fafc',
                                    border: '1px solid #e2e8f0', borderRadius: 8,
                                    fontSize: 13, fontWeight: 600, color: '#475569',
                                    textDecoration: 'none', transition: 'all 0.2s',
                                }}
                                onMouseEnter={e => { e.currentTarget.style.background = '#f1f5f9'; }}
                                onMouseLeave={e => { e.currentTarget.style.background = '#f8fafc'; }}
                            >
                                View <ChevronRight size={14} />
                            </Link>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default CandidateApplications;
