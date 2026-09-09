import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../../services/api';
import JobCard from '../../components/JobCard';
import { LoadingState, EmptyState, ErrorState } from '../../components/StateComponents';

const EmployerJobs = () => {
    const [jobs, setJobs] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchJobs = async () => {
            try {
                const response = await api.get('/Employers/me/jobs');
                setJobs(response.data);
            } catch (err) {
                setError('Failed to load jobs.');
            } finally {
                setLoading(false);
            }
        };
        fetchJobs();
    }, []);

    if (loading) return <LoadingState />;
    if (error) return <ErrorState message={error} />;

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <h2>My Posted Jobs</h2>
                <Link to="/employer/jobs/create" style={{ background: '#0066cc', color: '#fff', padding: '10px 20px', borderRadius: '4px', textDecoration: 'none' }}>
                    Post a New Job
                </Link>
            </div>
            
            {jobs.length === 0 ? (
                <EmptyState message="You haven't posted any jobs yet." />
            ) : (
                <div style={{ marginTop: '20px' }}>
                    {jobs.map(job => (
                        <JobCard key={job.id} job={job} isEmployer={true} />
                    ))}
                </div>
            )}
        </div>
    );
};

export default EmployerJobs;
