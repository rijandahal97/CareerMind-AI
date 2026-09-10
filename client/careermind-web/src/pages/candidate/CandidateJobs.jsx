import React, { useState, useEffect } from 'react';
import api from '../../services/api';
import { candidateService } from '../../services/candidate';
import JobCard from '../../components/JobCard';
import JobSearch from '../../components/JobSearch';
import { LoadingState, EmptyState, ErrorState } from '../../components/StateComponents';

const CandidateJobs = () => {
    const [jobs, setJobs] = useState([]);
    const [matches, setMatches] = useState([]);
    const [activeTab, setActiveTab] = useState('search'); // 'search' or 'matches'
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const fetchJobs = async (filters = {}) => {
        setLoading(true);
        setError(null);
        try {
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

    const fetchMatches = async () => {
        setLoading(true);
        setError(null);
        try {
            const response = await candidateService.getJobMatches();
            setMatches(response.data);
        } catch (err) {
            setError(err.response?.data?.message || 'Failed to load recommended jobs');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (activeTab === 'search') {
            fetchJobs();
        } else {
            fetchMatches();
        }
    }, [activeTab]);

    return (
        <div>
            <h2>Find Your Next Job</h2>
            <div style={{ display: 'flex', gap: '16px', marginBottom: '24px' }}>
                <button 
                    onClick={() => setActiveTab('search')} 
                    style={{ padding: '8px 16px', background: activeTab === 'search' ? '#0066cc' : '#eee', color: activeTab === 'search' ? '#fff' : '#333', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}
                >
                    Search Jobs
                </button>
                <button 
                    onClick={() => setActiveTab('matches')} 
                    style={{ padding: '8px 16px', background: activeTab === 'matches' ? '#0066cc' : '#eee', color: activeTab === 'matches' ? '#fff' : '#333', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}
                >
                    AI Recommended Matches
                </button>
            </div>

            {activeTab === 'search' && <JobSearch onSearch={fetchJobs} />}
            
            {loading && <LoadingState />}
            {error && <ErrorState message={error} />}
            
            {!loading && !error && activeTab === 'search' && jobs.length === 0 && (
                <EmptyState message="No jobs found matching your criteria." />
            )}
            
            {!loading && !error && activeTab === 'matches' && matches.length === 0 && (
                <EmptyState message="No matches found. Build your profile." />
            )}
            
            {!loading && !error && activeTab === 'search' && jobs.length > 0 && (
                <div>
                    {jobs.map(job => (
                        <JobCard key={job.id} job={job} />
                    ))}
                </div>
            )}

            {!loading && !error && activeTab === 'matches' && matches.length > 0 && (
                <div>
                    {matches.map(match => (
                        <JobCard key={match.jobId} job={{
                            id: match.jobId,
                            title: match.jobTitle,
                            companyName: match.companyName,
                            overallMatchScore: match.overallMatchScore
                        }} />
                    ))}
                </div>
            )}
        </div>
    );
};

export default CandidateJobs;
