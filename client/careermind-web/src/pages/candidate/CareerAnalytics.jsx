import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { candidateService } from '../../services/candidate';
import {
    Activity, ArrowRight, ShieldCheck, ShieldAlert,
    BarChart2, BookOpen, Briefcase, FileText,
    PieChart, Target, Zap, ChevronRight, MessageSquare, AlertCircle
} from 'lucide-react';
import './CareerAnalytics.css';

const CareerAnalytics = () => {
    const navigate = useNavigate();
    const [analytics, setAnalytics] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        fetchAnalytics();
    }, []);

    const fetchAnalytics = async () => {
        try {
            setLoading(true);
            const res = await candidateService.getCareerAnalytics();
            setAnalytics(res.data);
        } catch (err) {
            console.error("Failed to load analytics", err);
            setError("Could not load your career analytics at this time.");
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="analytics-loading animated-fade">
                <Activity size={48} className="pulse-icon" />
                <h2>Compiling Career Intelligence...</h2>
                <p>Aggregating data across your profile, skills, and applications.</p>
            </div>
        );
    }

    if (error || !analytics) {
        return (
            <div className="empty-state-card animated-fade">
                <ShieldAlert size={48} color="#ef4444" />
                <h2>Data Not Available</h2>
                <p>{error}</p>
                <button className="btn-primary mt-4" onClick={fetchAnalytics}>Retry</button>
            </div>
        );
    }

    const {
        OverallProgressScore,
        ProfileAnalytics,
        SkillAnalytics,
        JobMatchingAnalytics,
        ResumeAnalytics,
        InterviewAnalytics,
        ApplicationAnalytics,
        ActionPlanAnalytics,
        Insights
    } = analytics;

    const getScoreColor = (score) => {
        if (score >= 80) return '#10b981'; // Green
        if (score >= 50) return '#f59e0b'; // Yellow
        return '#ef4444'; // Red
    };

    return (
        <div className="career-analytics-container animated-fade">
            <header className="analytics-header">
                <div>
                    <h1>Career Intelligence Analytics <BarChart2 className="inline ml-2 text-blue-500" /></h1>
                    <p>Comprehensive evaluation of your professional progression, readiness, and alignment.</p>
                </div>
            </header>

            {/* Insights Highlight Strip */}
            {Insights && Insights.length > 0 && (
                <div className="insights-strip">
                    {Insights.map(insight => (
                        <div key={insight.InsightId} className={`insight-card insight-${insight.Type.toLowerCase()}`}>
                            <div className="insight-icon">
                                {insight.Type === "Success" ? <ShieldCheck size={20} /> :
                                 insight.Type === "Warning" ? <ShieldAlert size={20} /> : <Target size={20} />}
                            </div>
                            <div className="insight-content">
                                <h4>{insight.Title}</h4>
                                <p>{insight.Description}</p>
                            </div>
                            {insight.ActionLink && (
                                <button className="insight-action" onClick={() => navigate(insight.ActionLink)}>
                                    <ArrowRight size={16} />
                                </button>
                            )}
                        </div>
                    ))}
                </div>
            )}

            <div className="analytics-grid">
                
                {/* 1. Overall Career Progress */}
                <div className="glass-panel main-score-panel">
                    <h3>Overall Career Readiness</h3>
                    <div className="main-score-display">
                        <div 
                            className="score-circle-large"
                            style={{
                                '--progress': `${OverallProgressScore}%`,
                                '--color': getScoreColor(OverallProgressScore)
                            }}
                        >
                            <div className="score-inner">
                                <span className="score-val">{Math.round(OverallProgressScore)}%</span>
                                <span className="score-lbl">Readiness</span>
                            </div>
                        </div>
                    </div>
                </div>

                {/* 2. Profile Progress */}
                <div className="glass-panel analytics-card">
                    <div className="card-header">
                        <Target className="card-icon text-indigo-500" />
                        <h3>Profile Strength</h3>
                    </div>
                    <div className="stat-group">
                        <div className="stat-main">
                            <span className="stat-val" style={{ color: getScoreColor(ProfileAnalytics.CompletionPercentage)}}>
                                {Math.round(ProfileAnalytics.CompletionPercentage)}%
                            </span>
                            <span className="stat-lbl">{ProfileAnalytics.CompletionStatus}</span>
                        </div>
                        <div className="stat-details">
                            <p><strong>Completed:</strong> {ProfileAnalytics.CompletedSections.join(', ') || 'None'}</p>
                            {ProfileAnalytics.MissingSections.length > 0 && (
                                <p className="text-red-400"><strong>Missing:</strong> {ProfileAnalytics.MissingSections.join(', ')}</p>
                            )}
                        </div>
                    </div>
                </div>

                {/* 3. Skill Intelligence */}
                <div className="glass-panel analytics-card">
                    <div className="card-header">
                        <Zap className="card-icon text-yellow-500" />
                        <h3>Skill Intelligence</h3>
                    </div>
                    {SkillAnalytics.TotalSkills === 0 ? (
                        <div className="empty-sub">
                            <AlertCircle size={24} />
                            <p>No skills added yet.</p>
                            <button className="btn-small" onClick={() => navigate('/candidate/skills')}>Add Skills</button>
                        </div>
                    ) : (
                        <div className="stat-group">
                             <div className="stat-grid-2">
                                <div className="stat-box">
                                    <span className="box-val text-green-500">{SkillAnalytics.StrongSkills}</span>
                                    <span className="box-lbl">Strong</span>
                                </div>
                                <div className="stat-box">
                                    <span className="box-val text-orange-500">{SkillAnalytics.DevelopingSkills}</span>
                                    <span className="box-lbl">Developing</span>
                                </div>
                            </div>
                            <div className="progress-row mt-4">
                                <div className="prog-lbl"><span>Overall Proficiency</span> <span>{Math.round(SkillAnalytics.SkillDevelopmentProgress)}%</span></div>
                                <div className="prog-bar"><div className="prog-fill bg-yellow-500" style={{width: `${SkillAnalytics.SkillDevelopmentProgress}%`}}></div></div>
                            </div>
                        </div>
                    )}
                </div>

                {/* 4. Resume & ATS */}
                <div className="glass-panel analytics-card">
                    <div className="card-header">
                        <FileText className="card-icon text-blue-500" />
                        <h3>Resume ATS Performance</h3>
                    </div>
                    {!ResumeAnalytics.HasResume ? (
                        <div className="empty-sub">
                            <AlertCircle size={24} />
                            <p>Upload a resume for ATS analysis.</p>
                            <button className="btn-small" onClick={() => navigate('/candidate/resume')}>Upload Resume</button>
                        </div>
                    ) : ResumeAnalytics.LatestATSScore === 0 ? (
                        <div className="empty-sub">
                            <Activity size={24} />
                            <p>Resume not analyzed yet.</p>
                            <button className="btn-small" onClick={() => navigate('/candidate/resume')}>Run Analysis</button>
                        </div>
                    ) : (
                        <div className="stat-group">
                            <div className="stat-main">
                                <span className="stat-val" style={{ color: getScoreColor(ResumeAnalytics.LatestATSScore)}}>
                                    {Math.round(ResumeAnalytics.LatestATSScore)}%
                                </span>
                                <span className="stat-lbl">Latest ATS Score</span>
                            </div>
                            {ResumeAnalytics.TopImprovementAreas.length > 0 && (
                                <div className="stat-tags mt-3">
                                    <strong>Improvement Areas:</strong>
                                    {ResumeAnalytics.TopImprovementAreas.map(t => <span key={t} className="tag-weak">{t}</span>)}
                                </div>
                            )}
                        </div>
                    )}
                </div>

                {/* 5. Job Matching */}
                <div className="glass-panel analytics-card">
                    <div className="card-header">
                        <Briefcase className="card-icon text-indigo-400" />
                        <h3>Job Market Fit</h3>
                    </div>
                    {JobMatchingAnalytics.TotalJobsEvaluated === 0 ? (
                        <div className="empty-sub">
                            <PieChart size={24} />
                            <p>No job matches analyzed yet.</p>
                        </div>
                    ) : (
                        <div className="stat-group">
                            <div className="stat-grid-3">
                                <div className="stat-box">
                                    <span className="box-val text-green-500">{JobMatchingAnalytics.StrongMatches}</span>
                                    <span className="box-lbl">Strong</span>
                                </div>
                                <div className="stat-box">
                                    <span className="box-val text-yellow-500">{JobMatchingAnalytics.ModerateMatches}</span>
                                    <span className="box-lbl">Moderate</span>
                                </div>
                                <div className="stat-box">
                                    <span className="box-val text-red-500">{JobMatchingAnalytics.WeakMatches}</span>
                                    <span className="box-lbl">Weak</span>
                                </div>
                            </div>
                            <div className="text-center mt-3 text-sm text-gray-400">
                                Average match score: <strong>{Math.round(JobMatchingAnalytics.AverageMatchScore)}%</strong>
                            </div>
                        </div>
                    )}
                </div>

                {/* 6. Interview Intelligence */}
                <div className="glass-panel analytics-card">
                    <div className="card-header">
                        <MessageSquare className="card-icon text-purple-500" />
                        <h3>Interview Readiness</h3>
                    </div>
                    {InterviewAnalytics.TotalInterviewSessions === 0 ? (
                        <div className="empty-sub">
                            <MessageSquare size={24} />
                            <p>No mock interviews taken.</p>
                            <button className="btn-small" onClick={() => navigate('/candidate/interviews')}>Practice Now</button>
                        </div>
                    ) : (
                        <div className="stat-group">
                            <div className="stat-main">
                                <span className="stat-val" style={{ color: getScoreColor(InterviewAnalytics.LatestReadinessScore)}}>
                                    {Math.round(InterviewAnalytics.LatestReadinessScore)}%
                                </span>
                                <span className="stat-lbl">Latest Mock Score</span>
                            </div>
                            <p className="mt-2 text-sm text-gray-400">Completed Sessions: <strong>{InterviewAnalytics.CompletedSessions}</strong></p>
                        </div>
                    )}
                </div>

                {/* 7. Action Plan Progress */}
                <div className="glass-panel analytics-card">
                    <div className="card-header">
                        <BookOpen className="card-icon text-green-500" />
                        <h3>Career Action Plan</h3>
                    </div>
                    {ActionPlanAnalytics.TotalPlannedActions === 0 ? (
                        <div className="empty-sub">
                            <BookOpen size={24} />
                            <p>No career actions tracked.</p>
                            <button className="btn-small" onClick={() => navigate('/candidate/command-center')}>View Plan</button>
                        </div>
                    ) : (
                        <div className="stat-group">
                            <div className="progress-row mb-3">
                                <div className="prog-lbl"><span>Completion Rate</span> <span>{Math.round(ActionPlanAnalytics.OverallCompletionPercentage)}%</span></div>
                                <div className="prog-bar"><div className="prog-fill bg-green-500" style={{width: `${ActionPlanAnalytics.OverallCompletionPercentage}%`}}></div></div>
                            </div>
                            <div className="stat-grid-2">
                                <div className="stat-box">
                                    <span className="box-val text-green-500">{ActionPlanAnalytics.CompletedActions}</span>
                                    <span className="box-lbl">Completed</span>
                                </div>
                                <div className="stat-box">
                                    <span className="box-val text-gray-300">{ActionPlanAnalytics.PendingActions}</span>
                                    <span className="box-lbl">Pending</span>
                                </div>
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default CareerAnalytics;
