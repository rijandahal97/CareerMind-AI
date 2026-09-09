import React, { useState, useEffect } from 'react';
import api from '../../services/api';
import JobCard from '../../components/JobCard';
import JobSearch from '../../components/JobSearch';
import { LoadingState, EmptyState, ErrorState } from '../../components/StateComponents';

const CandidateJobs = () => {
    const [jobs, setJobs] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const fetchJobs = async (filters = {}) => {
        setLoading(true);
        setError(null);
        try {
            // Mapping filters to query params
            const params = new URLSearchParams();
            if (filters.keyword) params.append('keyword', filters.keyword);
            if (filters.location) params.append('location', filters.location);
            if (filters.isRemote) params.append('isRemote', true);
            if (filters.employmentType) params.append('employmentType', filters.employmentType);
            if (filters.experienceLevel) params.append('experienceLevel', filters.experienceLevel);
            
            const response = await api.get(`/Jobs?${params.toString()}`);
            setJobs(response.data);
        } catch (err) {
            setError(err.response?.data?.message || 'Failed to load jobs');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchJobs();
    }, []);

    return (
        <div>
            <h2>Find Your Next Job</h2>
            <JobSearch onSearch={fetchJobs} />
            
            {loading && <LoadingState />}
            {error && <ErrorState message={error} />}
            
            {!loading && !error && jobs.length === 0 && (
                <EmptyState message="No jobs found matching your criteria." />
            )}
            
            {!loading && !error && jobs.length > 0 && (
                <div>
                    {jobs.map(job => (
                        <JobCard key={job.id} job={job} />
                    ))}
                </div>
            )}
        </div>
    );
};

export default CandidateJobs;
