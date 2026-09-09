import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../../services/api';
import { LoadingState, EmptyState, ErrorState } from '../../components/StateComponents';

const CandidateSavedJobs = () => {
    const [savedJobs, setSavedJobs] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchSaved = async () => {
            try {
                const response = await api.get('/Candidates/me/saved-jobs');
                setSavedJobs(response.data);
            } catch (err) {
                setError('Failed to load saved jobs.');
            } finally {
                setLoading(false);
            }
        };
        fetchSaved();
    }, []);

    const handleRemove = async (jobId) => {
        try {
            await api.delete(`/Jobs/${jobId}/save`);
            setSavedJobs(savedJobs.filter(j => j.jobId !== jobId));
        } catch (err) {
            alert('Failed to remove saved job.');
        }
    };

    if (loading) return <LoadingState />;
    if (error) return <ErrorState message={error} />;
    if (savedJobs.length === 0) return <EmptyState message="You have no saved jobs." />;

    return (
        <div>
            <h2>Saved Jobs</h2>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
                {savedJobs.map(job => (
                    <div key={job.jobId} style={{ border: '1px solid #ccc', borderRadius: '8px', padding: '16px', background: '#fff', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <div>
                            <h3 style={{ margin: '0 0 8px 0' }}>{job.jobTitle}</h3>
                            <p style={{ margin: '0 0 4px 0', color: '#666', fontWeight: 'bold' }}>{job.companyName}</p>
                            <p style={{ margin: '0', color: '#888', fontSize: '14px' }}>{job.location} • {job.employmentType}</p>
                        </div>
                        <div style={{ display: 'flex', gap: '8px' }}>
                            <Link to={`/candidate/jobs/${job.jobId}`} style={{ background: '#0066cc', color: '#fff', padding: '8px 16px', borderRadius: '4px', textDecoration: 'none' }}>View</Link>
                            <button onClick={() => handleRemove(job.jobId)} style={{ background: '#fee', color: '#c00', border: '1px solid #fcc', padding: '8px 16px', borderRadius: '4px', cursor: 'pointer' }}>Remove</button>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default CandidateSavedJobs;
