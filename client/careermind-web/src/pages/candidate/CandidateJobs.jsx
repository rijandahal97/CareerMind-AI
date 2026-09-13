import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../../services/api';
import { candidateService } from '../../services/candidate';
import { LoadingState, EmptyState, ErrorState } from '../../components/StateComponents';
import { Search, MapPin, Building2, Bookmark, Star, Briefcase, Filter, ChevronDown, SlidersHorizontal } from 'lucide-react';
import './CandidateJobs.css';

const JobCard = ({ job, showMatch = false }) => {
    const [saved, setSaved] = useState(false);
    const [saving, setSaving] = useState(false);

    const handleSave = async (e) => {
        e.preventDefault();
        setSaving(true);
        try {
            await api.post(`/Jobs/${job.id || job.jobId}/save`);
            setSaved(true);
        } catch {
            // ignore
        } finally {
            setSaving(false);
        }
    };

    const matchPercent = job.overallMatchScore || job.matchScore || job.compatibilityScore || 0;
    const matchColor = matchPercent >= 75 ? '#10b981' : matchPercent >= 50 ? '#f59e0b' : '#ef4444';

    return (
        <div className="job-listing-card">
            <div className="jlc-top">
                <div className="jlc-info">
                    <h3 className="jlc-title">{job.title || job.jobTitle}</h3>
                    <div className="jlc-meta">
                        <span><Building2 size={14} />{job.companyName}</span>
                        {job.location && <span><MapPin size={14} />{job.location}</span>}
                        {job.employmentType && <span className="jlc-type">{job.employmentType}</span>}
                    </div>
                </div>
                <div className="jlc-right">
                    {showMatch && matchPercent > 0 && (
                        <div className="jlc-match" style={{ borderColor: matchColor, color: matchColor }}>
                            <span>{Math.round(matchPercent)}%</span>
                            <small>Match</small>
                        </div>
                    )}
                    <button 
                        className={`jlc-save-btn ${saved ? 'saved' : ''}`}
                        onClick={handleSave}
                        disabled={saving || saved}
                        title={saved ? 'Saved' : 'Save job'}
                    >
                        <Bookmark size={18} />
                    </button>
                </div>
            </div>

            {job.salaryMin && (
                <div className="jlc-salary">
                    ${job.salaryMin?.toLocaleString()} – ${job.salaryMax?.toLocaleString()} / yr
                </div>
            )}

            {job.requiredSkills && job.requiredSkills.length > 0 && (
                <div className="jlc-skills">
                    {job.requiredSkills.slice(0, 4).map((s, i) => (
                        <span key={i} className="jlc-skill-pill">{typeof s === 'object' ? s.skillName : s}</span>
                    ))}
                    {job.requiredSkills.length > 4 && <span className="jlc-skill-more">+{job.requiredSkills.length - 4}</span>}
                </div>
            )}

            <div className="jlc-actions">
                <Link to={`/candidate/jobs/${job.id || job.jobId}`} className="jlc-btn-primary">View Details</Link>
            </div>
        </div>
    );
};

const CandidateJobs = () => {
    const [jobs, setJobs] = useState([]);
    const [matches, setMatches] = useState([]);
    const [activeTab, setActiveTab] = useState('search');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [filters, setFilters] = useState({ keyword: '', location: '', employmentType: '', isRemote: false });
    const [showFilters, setShowFilters] = useState(false);

    const fetchJobs = async (f = filters) => {
        setLoading(true);
        setError(null);
        try {
            const params = new URLSearchParams();
            if (f.keyword) params.append('keyword', f.keyword);
            if (f.location) params.append('location', f.location);
            if (f.isRemote) params.append('isRemote', true);
            if (f.employmentType) params.append('employmentType', f.employmentType);
            const response = await api.get(`/Jobs?${params.toString()}`);
            setJobs(response.data);
        } catch (err) {
            setError(err.response?.data?.message || 'Failed to load jobs. Please try again.');
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
            setError(err.response?.data?.message || 'Failed to load job matches. Complete your profile to get recommendations.');
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

    const handleSearch = (e) => {
        e.preventDefault();
        fetchJobs(filters);
    };

    return (
        <div className="animated-fade jobs-page">
            <div className="jobs-header">
                <div>
                    <h2 className="jobs-title">Find Your Next Role</h2>
                    <p className="jobs-subtitle">Browse all jobs or let AI find perfect matches for you</p>
                </div>
            </div>

            {/* Tab Bar */}
            <div className="jobs-tab-bar">
                <button
                    className={`jobs-tab ${activeTab === 'search' ? 'active' : ''}`}
                    onClick={() => setActiveTab('search')}
                    id="tab-search"
                >
                    <Briefcase size={18} /> All Jobs
                </button>
                <button
                    className={`jobs-tab ${activeTab === 'matches' ? 'active' : ''}`}
                    onClick={() => setActiveTab('matches')}
                    id="tab-matches"
                >
                    <Star size={18} /> AI Recommendations
                    <span className="tab-badge">{matches.length > 0 ? matches.length : ''}</span>
                </button>
            </div>

            {/* Search bar & Filters (Search tab only) */}
            {activeTab === 'search' && (
                <div className="jobs-search-section">
                    <form className="jobs-search-bar" onSubmit={handleSearch}>
                        <div className="search-field">
                            <Search size={18} className="search-field-icon" />
                            <input
                                type="text"
                                placeholder="Job title, keyword..."
                                value={filters.keyword}
                                onChange={e => setFilters(f => ({ ...f, keyword: e.target.value }))}
                            />
                        </div>
                        <div className="search-field">
                            <MapPin size={18} className="search-field-icon" />
                            <input
                                type="text"
                                placeholder="Location"
                                value={filters.location}
                                onChange={e => setFilters(f => ({ ...f, location: e.target.value }))}
                            />
                        </div>
                        <button type="button" className="filter-toggle-btn" onClick={() => setShowFilters(!showFilters)}>
                            <SlidersHorizontal size={18} /> Filters <ChevronDown size={14} style={{ transform: showFilters ? 'rotate(180deg)' : '', transition: 'transform 0.2s' }} />
                        </button>
                        <button type="submit" className="search-submit-btn">Search</button>
                    </form>

                    {showFilters && (
                        <div className="jobs-filter-panel">
                            <select
                                value={filters.employmentType}
                                onChange={e => setFilters(f => ({ ...f, employmentType: e.target.value }))}
                            >
                                <option value="">Any Employment Type</option>
                                <option value="FullTime">Full Time</option>
                                <option value="PartTime">Part Time</option>
                                <option value="Contract">Contract</option>
                                <option value="Internship">Internship</option>
                            </select>
                            <label className="filter-checkbox">
                                <input
                                    type="checkbox"
                                    checked={filters.isRemote}
                                    onChange={e => setFilters(f => ({ ...f, isRemote: e.target.checked }))}
                                />
                                Remote Only
                            </label>
                        </div>
                    )}
                </div>
            )}

            {/* Results */}
            <div className="jobs-results">
                {loading && <LoadingState message="Searching for jobs..." />}
                {error && <ErrorState message={error} onRetry={activeTab === 'search' ? fetchJobs : fetchMatches} />}

                {!loading && !error && activeTab === 'search' && (
                    jobs.length === 0
                        ? <EmptyState message="No jobs found. Try broader search terms." />
                        : <>
                            <p className="results-count">{jobs.length} job{jobs.length !== 1 ? 's' : ''} found</p>
                            <div className="jobs-list">
                                {jobs.map(job => <JobCard key={job.id} job={job} showMatch={false} />)}
                            </div>
                          </>
                )}

                {!loading && !error && activeTab === 'matches' && (
                    matches.length === 0
                        ? <EmptyState message="No AI matches yet. Complete your profile and add skills to get personalized recommendations." />
                        : <>
                            <p className="results-count">{matches.length} recommended role{matches.length !== 1 ? 's' : ''} for you</p>
                            <div className="jobs-list">
                                {matches.map(match => (
                                    <JobCard
                                        key={match.jobId}
                                        job={{
                                            id: match.jobId,
                                            jobId: match.jobId,
                                            title: match.jobTitle,
                                            jobTitle: match.jobTitle,
                                            companyName: match.companyName,
                                            location: match.location,
                                            overallMatchScore: match.overallMatchScore,
                                            matchScore: match.matchScore,
                                            compatibilityScore: match.compatibilityScore,
                                            requiredSkills: match.matchedSkills?.map(s => ({ skillName: s })),
                                        }}
                                        showMatch={true}
                                    />
                                ))}
                            </div>
                          </>
                )}
            </div>
        </div>
    );
};

export default CandidateJobs;
