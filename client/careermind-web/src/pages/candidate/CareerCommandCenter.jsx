import React, { useState, useEffect } from 'react';
import { candidateService } from '../../services/candidate';
import { 
    Target, ShieldAlert, CheckCircle, Circle, 
    ArrowRight, Map, BookOpen, Briefcase, ChevronRight, Activity, Flame, ShieldCheck, Flag
} from 'lucide-react';
import './CareerCommandCenter.css';

const CareerCommandCenter = () => {
    const [data, setData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [updatingAction, setUpdatingAction] = useState(null);

    useEffect(() => {
        fetchCommandCenter();
    }, []);

    const fetchCommandCenter = async () => {
        try {
            setLoading(true);
            const res = await candidateService.getCommandCenter();
            setData(res.data);
        } catch (err) {
            console.error(err);
            setError("Failed to load command center data.");
        } finally {
            setLoading(false);
        }
    };

    const toggleActionCompletion = async (actionKey, currentStatus) => {
        try {
            setUpdatingAction(actionKey);
            await candidateService.updateCommandCenterAction(actionKey, !currentStatus);
            // Optimistically update UI
            setData(prev => {
                if (!prev) return prev;
                const newData = { ...prev };
                if (newData.ActionPlan) {
                    ['ThirtyDayActions', 'SixtyDayActions', 'NinetyDayActions'].forEach(period => {
                        if (newData.ActionPlan[period]) {
                            const idx = newData.ActionPlan[period].findIndex(a => a.ActionKey === actionKey);
                            if (idx !== -1) {
                                newData.ActionPlan[period][idx].IsCompleted = !currentStatus;
                            }
                        }
                    });
                }
                return newData;
            });
        } catch (err) {
            console.error(err);
        } finally {
            setUpdatingAction(null);
        }
    };

    if (loading) {
        return (
            <div className="command-center-loading animated-fade">
                <Activity size={48} className="pulse-icon" />
                <h2>Initializing Command Center...</h2>
                <p>Analyzing your career footprint, matching algorithms, and skill matrices.</p>
            </div>
        );
    }

    if (error || !data) {
        return (
            <div className="empty-state-card animated-fade">
                <ShieldAlert size={48} color="#ef4444" />
                <h2>Error Loading Data</h2>
                <p>{error || "Could not retrieve command center insights at this time."}</p>
                <button className="btn-primary mt-4" onClick={fetchCommandCenter}>Try Again</button>
            </div>
        );
    }

    const { 
        Readiness, GoalSummary, TopBlockers, ActionPlan, NextBestAction 
    } = data;

    const renderActionList = (actions, title) => {
        if (!actions || actions.length === 0) return null;
        
        return (
            <div className="action-period-card">
                <h4>{title}</h4>
                <div className="action-list">
                    {actions.map(action => (
                        <div 
                            key={action.ActionKey} 
                            className={`action-item ${action.IsCompleted ? 'completed' : ''}`}
                            onClick={() => toggleActionCompletion(action.ActionKey, action.IsCompleted)}
                        >
                            <div className="action-checkbox">
                                {updatingAction === action.ActionKey ? (
                                    <Activity size={20} className="spinning-icon text-blue-500" />
                                ) : action.IsCompleted ? (
                                    <CheckCircle size={20} className="text-green-500" />
                                ) : (
                                    <Circle size={20} className="text-gray-400" />
                                )}
                            </div>
                            <div className="action-content">
                                <span className="action-title">{action.Title}</span>
                                {action.Description && <p className="action-desc">{action.Description}</p>}
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        );
    };

    const getScoreColor = (score) => {
        if (score >= 80) return '#10b981'; // Green
        if (score >= 50) return '#f59e0b'; // Yellow
        return '#ef4444'; // Red
    };

    return (
        <div className="command-center-container animated-fade">
            <div className="command-header">
                <div className="command-header-text">
                    <h1>Command Center <Flame size={28} className="text-orange-500 inline-block ml-2 mb-1" /></h1>
                    <p>Your personalized AI-driven strategic career headquarters.</p>
                </div>
                
                {GoalSummary && (
                    <div className="goal-flag">
                        <Flag size={20} />
                        <div>
                            <span className="goal-label">Current Target</span>
                            <strong className="goal-target">{GoalSummary.TargetRole || 'Not Set'}</strong>
                        </div>
                    </div>
                )}
            </div>

            <div className="command-grid">
                
                {/* Readiness Score Panel */}
                <div className="glass-panel readiness-panel">
                    <h3>Overall Readiness</h3>
                    <div className="readiness-score-display">
                        <div 
                            className="radial-progress-large" 
                            style={{
                                '--progress': `${Readiness.OverallScore}%`,
                                '--color': getScoreColor(Readiness.OverallScore)
                            }}
                        >
                            <div className="radial-inner">
                                <span className="score-number">{Math.round(Readiness.OverallScore)}</span>
                                <span className="score-label">/100</span>
                            </div>
                        </div>
                        <div className="readiness-breakdown">
                            <div className="breakdown-item">
                                <span>Profile</span>
                                <div className="mini-bar">
                                    <div className="mini-fill" style={{ width: `${Readiness.ProfileScore}%`, backgroundColor: getScoreColor(Readiness.ProfileScore) }}></div>
                                </div>
                            </div>
                            <div className="breakdown-item">
                                <span>Skills</span>
                                <div className="mini-bar">
                                    <div className="mini-fill" style={{ width: `${Readiness.SkillScore}%`, backgroundColor: getScoreColor(Readiness.SkillScore) }}></div>
                                </div>
                            </div>
                            <div className="breakdown-item">
                                <span>Resume</span>
                                <div className="mini-bar">
                                    <div className="mini-fill" style={{ width: `${Readiness.ResumeScore}%`, backgroundColor: getScoreColor(Readiness.ResumeScore) }}></div>
                                </div>
                            </div>
                            <div className="breakdown-item">
                                <span>Job Match</span>
                                <div className="mini-bar">
                                    <div className="mini-fill" style={{ width: `${Readiness.JobMatchScore}%`, backgroundColor: getScoreColor(Readiness.JobMatchScore) }}></div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div className="readiness-level-badge" style={{ borderColor: getScoreColor(Readiness.OverallScore), color: getScoreColor(Readiness.OverallScore) }}>
                        {Readiness.ReadinessLevel}
                    </div>
                </div>

                {/* Next Best Action */}
                {NextBestAction && (
                    <div className="glass-panel nba-panel highlight-panel">
                        <div className="nba-badge"><Target size={16} /> NEXT BEST ACTION</div>
                        <h2>{NextBestAction.Title}</h2>
                        <p>{NextBestAction.Description}</p>
                        <button 
                            className={`btn-action-primary ${NextBestAction.IsCompleted ? 'completed-btn' : ''}`}
                            onClick={() => toggleActionCompletion(NextBestAction.ActionKey, NextBestAction.IsCompleted)}
                            disabled={updatingAction === NextBestAction.ActionKey}
                        >
                            {NextBestAction.IsCompleted ? <><CheckCircle size={18} /> Completed</> : <><ChevronRight size={18} /> Mark as Complete</>}
                        </button>
                    </div>
                )}

                {/* Top Blockers */}
                <div className="glass-panel blockers-panel">
                    <h3><ShieldAlert size={20} className="text-red-500" /> Current Blockers</h3>
                    {TopBlockers && TopBlockers.length > 0 ? (
                        <ul className="blocker-list">
                            {TopBlockers.map(blocker => (
                                <li key={blocker.Id} className={`blocker-item severity-${blocker.Severity.toLowerCase()}`}>
                                    <span className="blocker-dot"></span>
                                    <div>
                                        <strong>{blocker.Title}</strong>
                                        <p>{blocker.Description}</p>
                                    </div>
                                </li>
                            ))}
                        </ul>
                    ) : (
                        <div className="no-blockers">
                            <ShieldCheck size={48} className="text-green-500" />
                            <p>No critical blockers identified. You're on track!</p>
                        </div>
                    )}
                </div>

                {/* 30/60/90 Action Plan */}
                {ActionPlan && (
                    <div className="glass-panel action-plan-panel full-width">
                        <div className="panel-header-flex">
                            <h3><Map size={20} /> Strategic Action Plan</h3>
                            <div className="plan-stats">
                                <span>{ActionPlan.CompletedActions} / {ActionPlan.TotalActions} Actions Completed</span>
                            </div>
                        </div>
                        
                        <div className="action-plan-grid">
                            {renderActionList(ActionPlan.ThirtyDayActions, "Next 30 Days")}
                            {renderActionList(ActionPlan.SixtyDayActions, "30-60 Days")}
                            {renderActionList(ActionPlan.NinetyDayActions, "60-90 Days")}
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default CareerCommandCenter;
