import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../../services/api';
import { LoadingState, EmptyState, ErrorState } from '../../components/StateComponents';
import { Bookmark, Building2, MapPin, Briefcase, Trash2, ExternalLink } from 'lucide-react';

const CandidateSavedJobs = () => {
    const [savedJobs, setSavedJobs] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [removing, setRemoving] = useState(null);

    const fetchSaved = async () => {
        setLoading(true);
        setError(null);
        try {
            const response = await api.get('/Candidates/me/saved-jobs');
            setSavedJobs(response.data);
        } catch (err) {
            setError('Failed to load saved jobs. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { fetchSaved(); }, []);

    const handleRemove = async (jobId) => {
        setRemoving(jobId);
        try {
            await api.delete(`/Jobs/${jobId}/save`);
            setSavedJobs(savedJobs.filter(j => j.jobId !== jobId));
        } catch {
            alert('Failed to remove saved job.');
        } finally {
            setRemoving(null);
        }
    };

    if (loading) return <LoadingState message="Loading your saved jobs..." />;
    if (error) return <ErrorState message={error} onRetry={fetchSaved} />;
    if (savedJobs.length === 0) return (
        <div>
            <div style={{ marginBottom: 24 }}>
                <h2 style={{ margin: '0 0 4px 0', fontSize: 22, color: '#0f172a' }}>Saved Jobs</h2>
                <p style={{ margin: 0, color: '#64748b', fontSize: 14 }}>Jobs you've bookmarked for later</p>
            </div>
            <EmptyState
                message="You have no saved jobs yet."
                icon={<Bookmark size={40} color="#94a3b8" />}
            />
        </div>
    );

    return (
        <div className="animated-fade">
            <div style={{ marginBottom: 24 }}>
                <h2 style={{ margin: '0 0 4px 0', fontSize: 22, color: '#0f172a' }}>Saved Jobs</h2>
                <p style={{ margin: 0, color: '#64748b', fontSize: 14 }}>{savedJobs.length} saved job{savedJobs.length !== 1 ? 's' : ''}</p>
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
                {savedJobs.map(job => (
                    <div key={job.jobId} style={{
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
                                width: 44, height: 44, background: '#fef3c7', borderRadius: 10,
                                display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0,
                            }}>
                                <Bookmark size={22} color="#d97706" />
                            </div>
                            <div>
                                <h3 style={{ margin: '0 0 6px 0', fontSize: 16, color: '#0f172a' }}>{job.jobTitle}</h3>
                                <div style={{ display: 'flex', gap: 14, flexWrap: 'wrap' }}>
                                    <span style={{ display: 'flex', alignItems: 'center', gap: 4, fontSize: 13, color: '#64748b', fontWeight: 500 }}>
                                        <Building2 size={13} /> {job.companyName}
                                    </span>
                                    {job.location && (
                                        <span style={{ display: 'flex', alignItems: 'center', gap: 4, fontSize: 13, color: '#94a3b8' }}>
                                            <MapPin size={13} /> {job.location}
                                        </span>
                                    )}
                                    {job.employmentType && (
                                        <span style={{ display: 'flex', alignItems: 'center', gap: 4, fontSize: 12, color: '#94a3b8' }}>
                                            <Briefcase size={12} /> {job.employmentType}
                                        </span>
                                    )}
                                </div>
                            </div>
                        </div>
                        <div style={{ display: 'flex', gap: 10, flexShrink: 0, marginLeft: 16 }}>
                            <Link
                                to={`/candidate/jobs/${job.jobId}`}
                                style={{
                                    display: 'flex', alignItems: 'center', gap: 6,
                                    padding: '8px 16px', background: '#3b82f6',
                                    color: 'white', borderRadius: 8,
                                    fontSize: 13, fontWeight: 600, textDecoration: 'none',
                                    transition: 'background 0.2s',
                                }}
                                onMouseEnter={e => { e.currentTarget.style.background = '#2563eb'; }}
                                onMouseLeave={e => { e.currentTarget.style.background = '#3b82f6'; }}
                            >
                                <ExternalLink size={14} /> View
                            </Link>
                            <button
                                onClick={() => handleRemove(job.jobId)}
                                disabled={removing === job.jobId}
                                style={{
                                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                                    width: 36, height: 36, background: '#fff1f2',
                                    color: '#f43f5e', border: '1px solid #fecdd3',
                                    borderRadius: 8, cursor: removing === job.jobId ? 'wait' : 'pointer',
                                    transition: 'all 0.2s',
                                }}
                                title="Remove saved job"
                            >
                                <Trash2 size={16} />
                            </button>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default CandidateSavedJobs;
