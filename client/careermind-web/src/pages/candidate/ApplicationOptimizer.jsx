import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { candidateService } from '../../services/candidate';
import api from '../../services/api';
import { 
    Briefcase, FileText, TrendingUp, AlertCircle, 
    CheckCircle, List, ArrowLeft, RefreshCw, 
    Award, Star, GraduationCap, Copy, Search
} from 'lucide-react';
import './ApplicationOptimizer.css';

const ApplicationOptimizer = () => {
    const { jobId } = useParams();
    const navigate = useNavigate();
    
    const [job, setJob] = useState(null);
    const [optimization, setOptimization] = useState(null);
    const [readiness, setReadiness] = useState(null);
    const [loading, setLoading] = useState(true);
    const [generating, setGenerating] = useState(false);
    const [activeTab, setActiveTab] = useState('All');

    useEffect(() => {
        loadData();
    }, [jobId]);

    const loadData = async () => {
        setLoading(true);
        try {
            const jRes = await api.get(`/jobs/${jobId}`);
            setJob(jRes.data);

            const optRes = await candidateService.getApplicationOptimization(jobId).catch(() => null);
            if (optRes && optRes.data) {
                setOptimization(optRes.data);
                const readRes = await candidateService.getApplicationReadiness(jobId);
                setReadiness(readRes.data);
            }
        } catch (error) {
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    const runOptimization = async () => {
        setGenerating(true);
        try {
            await candidateService.createApplicationOptimization(jobId);
            const optRes = await candidateService.getApplicationOptimization(jobId);
            setOptimization(optRes.data);
            const readRes = await candidateService.getApplicationReadiness(jobId);
            setReadiness(readRes.data);
        } catch (error) {
            console.error(error);
        } finally {
            setGenerating(false);
        }
    };

    const toggleSuggestion = async (suggestion) => {
        try {
            await candidateService.updateApplicationSuggestion(jobId, suggestion.id, { isCompleted: !suggestion.isCompleted });
            const optRes = await candidateService.getApplicationOptimization(jobId);
            setOptimization(optRes.data);
        } catch (error) {
            console.error("Failed to update suggestion", error);
        }
    };

    if (loading) {
        return <div className="opt-loading">Loading application optimizer...</div>;
    }

    if (!job) {
        return <div className="opt-error">Job not found.</div>;
    }

    if (!optimization) {
        return (
            <div className="opt-empty-state">
                <div className="opt-empty-card">
                    <TrendingUp size={48} className="opt-empty-icon" />
                    <h2>AI Application Optimizer</h2>
                    <p>Analyze your readiness for <strong>{job.title}</strong> at <strong>{job.employerProfile?.companyName}</strong>.</p>
                    <button className="opt-action-btn primary" onClick={runOptimization} disabled={generating}>
                        {generating ? "Analyzing Profile..." : "Run AI Analysis"}
                    </button>
                </div>
            </div>
        );
    }

    // Determine Ring Color based on Readiness Score
    const getRingColor = (score) => {
        if (score >= 75) return '#10b981'; // Green
        if (score >= 40) return '#f59e0b'; // Yellow
        return '#ef4444'; // Red
    };

    const score = readiness?.score || 0;
    const currentTabSuggestions = activeTab === 'All' 
        ? optimization.suggestions 
        : optimization.suggestions.filter(s => {
            if (activeTab === 'Resume') return s.category === 1;
            if (activeTab === 'Skills') return s.category === 2;
            if (activeTab === 'Profile') return s.category === 0;
            if (activeTab === 'Interview') return s.category === 5;
            if (activeTab === 'Career Gap') return s.category === 6;
            return false;
        });

    return (
        <div className="optimizer-container">
            <header className="opt-header">
                <button className="opt-back-btn" onClick={() => navigate('/candidate')}>
                    <ArrowLeft size={18} /> Back to Dashboard
                </button>
                <div className="opt-header-title">
                    <h1>Application Optimizer</h1>
                    <button className="opt-refresh-btn" onClick={runOptimization} disabled={generating}>
                        <RefreshCw size={16} className={generating ? "spinning" : ""} /> {generating ? "Updating..." : "Refresh Analysis"}
                    </button>
                </div>
            </header>

            <div className="opt-grid">
                <div className="opt-main-col">
                    <section className="opt-job-card">
                        <div className="opt-job-details">
                            <h2>{job.title}</h2>
                            <p className="opt-job-employer">{job.employerProfile?.companyName}</p>
                            <div className="opt-job-meta">
                                <span><Briefcase size={14}/> {job.experienceLevel || "Any Experience"}</span>
                                <span><Search size={14}/> {job.employmentType || "Full-time"}</span>
                            </div>
                        </div>
                        <div className="opt-readiness-circle">
                            <svg viewBox="0 0 36 36" className="circular-chart">
                                <path className="circle-bg"
                                    d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
                                />
                                <path className="circle"
                                    strokeDasharray={`${score}, 100`}
                                    stroke={getRingColor(score)}
                                    d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
                                />
                                <text x="18" y="20.35" className="percentage">{Math.round(score)}</text>
                            </svg>
                            <span className="opt-readiness-label">{readiness?.readinessLevel}</span>
                        </div>
                    </section>

                    <section className="opt-breakdown">
                        <h3>Intelligence Breakdown</h3>
                        <div className="opt-breakdown-grid">
                            <div className="opt-b-card">
                                <h4><Star size={16}/> Profile</h4>
                                <div className="opt-b-score">{Math.round(readiness?.profileAlignmentScore || 0)}/100</div>
                            </div>
                            <div className="opt-b-card">
                                <h4><Award size={16}/> Skills</h4>
                                <div className="opt-b-score">{Math.round(readiness?.skillAlignmentScore || 0)}/100</div>
                            </div>
                            <div className="opt-b-card">
                                <h4><Briefcase size={16}/> Experience</h4>
                                <div className="opt-b-score">{Math.round(readiness?.experienceAlignmentScore || 0)}/100</div>
                            </div>
                            <div className="opt-b-card">
                                <h4><GraduationCap size={16}/> Education</h4>
                                <div className="opt-b-score">{Math.round(readiness?.educationAlignmentScore || 0)}/100</div>
                            </div>
                            <div className="opt-b-card">
                                <h4><FileText size={16}/> Resume</h4>
                                <div className="opt-b-score">{Math.round(readiness?.resumeAlignmentScore || 0)}/100</div>
                            </div>
                            <div className="opt-b-card">
                                <h4><TrendingUp size={16}/> Interview</h4>
                                <div className="opt-b-score">{Math.round(readiness?.interviewReadinessScore || 0)}/100</div>
                            </div>
                        </div>
                    </section>

                    {readiness?.missingDataBlockers && (
                        <section className="opt-blockers">
                            <h4><AlertCircle size={18}/> Critical Blockers</h4>
                            <p>{readiness.missingDataBlockers}</p>
                        </section>
                    )}

                    <section className="opt-suggestions">
                        <h3>Optimization Suggestions</h3>
                        <div className="opt-tabs">
                            {['All', 'Profile', 'Resume', 'Skills', 'Interview', 'Career Gap'].map(tab => (
                                <button 
                                    key={tab} 
                                    className={`opt-tab ${activeTab === tab ? 'active' : ''}`}
                                    onClick={() => setActiveTab(tab)}
                                >
                                    {tab}
                                </button>
                            ))}
                        </div>
                        <div className="opt-suggestion-list">
                            {currentTabSuggestions.length === 0 ? (
                                <p className="opt-no-sugg">No suggestions in this category.</p>
                            ) : (
                                currentTabSuggestions.map(s => (
                                    <div key={s.id} className={`opt-suggestion-item ${s.isCompleted ? 'completed' : ''}`}>
                                        <button className="opt-check-btn" onClick={() => toggleSuggestion(s)}>
                                            {s.isCompleted ? <CheckCircle size={24} color="#10b981" /> : <div className="opt-check-circle" />}
                                        </button>
                                        <div className="opt-sugg-content">
                                            <h5>{s.title}</h5>
                                            <p>{s.description}</p>
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                    </section>
                </div>

                <div className="opt-side-col">
                    <section className="opt-profile-content">
                        <h3>Optimized Profile</h3>
                        <div className="opt-content-box">
                            <label>Headline</label>
                            <input 
                                type="text" 
                                value={optimization.optimizedHeadline || ''} 
                                readOnly 
                                className="opt-copy-input"
                            />
                        </div>
                        <div className="opt-content-box">
                            <label>Summary</label>
                            <textarea 
                                value={optimization.optimizedSummary || ''} 
                                readOnly 
                                rows={4}
                                className="opt-copy-textarea"
                            />
                        </div>
                    </section>
                    <section className="opt-profile-content">
                        <h3>Cover Letter</h3>
                        <div className="opt-content-box">
                            <textarea 
                                value={optimization.coverLetter || ''} 
                                readOnly 
                                rows={10}
                                className="opt-copy-textarea"
                            />
                            <button className="opt-copy-btn" onClick={() => navigator.clipboard.writeText(optimization.coverLetter)}>
                                <Copy size={14}/> Copy Cover Letter
                            </button>
                        </div>
                    </section>

                    <section className="opt-final-prep">
                        <h3>Your Application Preparation</h3>
                        <div className="opt-prep-meta">
                            <div className="opt-prep-score">Score: {Math.round(score)}</div>
                            <div className="opt-prep-next">
                                <strong>Next Action:</strong> {optimization.suggestions.find(s => !s.isCompleted)?.title || "Apply Now!"}
                            </div>
                        </div>
                        <button className="opt-action-btn primary full-width" disabled={score < 75}>
                            {score >= 75 ? "Submit Application" : "Improve Score to Apply"}
                        </button>
                    </section>
                </div>
            </div>
        </div>
    );
};

export default ApplicationOptimizer;
