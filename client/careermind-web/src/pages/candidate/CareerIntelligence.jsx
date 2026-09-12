import React, { useState, useEffect } from 'react';
import { candidateService } from '../../services/candidate';
import './CareerIntelligence.css';

const ScoreCircle = ({ score, label, size = 120 }) => {
    const radius = (size - 16) / 2;
    const circumference = 2 * Math.PI * radius;
    const offset = circumference - (score / 100) * circumference;
    const color = score >= 75 ? '#10b981' : score >= 50 ? '#f59e0b' : '#ef4444';

    return (
        <div className="score-circle" style={{ width: size, height: size }}>
            <svg width={size} height={size}>
                <circle cx={size/2} cy={size/2} r={radius} stroke="#334155" strokeWidth="8" fill="none" />
                <circle cx={size/2} cy={size/2} r={radius} stroke={color} strokeWidth="8" fill="none"
                    strokeDasharray={circumference} strokeDashoffset={offset}
                    strokeLinecap="round" style={{ transition: 'stroke-dashoffset 1s ease-in-out', transform: 'rotate(-90deg)', transformOrigin: '50% 50%' }} />
            </svg>
            <div className="score-circle-text">
                <span className="score-value" style={{ color }}>{Math.round(score)}%</span>
                {label && <span className="score-label">{label}</span>}
            </div>
        </div>
    );
};

const SeverityBadge = ({ level }) => {
    const cls = level === 'HIGH' ? 'severity-high' : level === 'MEDIUM' ? 'severity-medium' : 'severity-low';
    return <span className={`severity-badge ${cls}`}>{level}</span>;
};

const StatusBadge = ({ status }) => {
    const cls = status === 'STRONG' ? 'status-strong' : status === 'DEVELOPING' ? 'status-developing' : 'status-missing';
    return <span className={`status-badge ${cls}`}>{status}</span>;
};

const CareerIntelligence = () => {
    const [data, setData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const res = await candidateService.getCareerPathIntelligence();
                setData(res.data);
            } catch (err) {
                setError(err.response?.data?.message || err.message || 'Failed to load career intelligence.');
            } finally {
                setLoading(false);
            }
        };
        fetchData();
    }, []);

    if (loading) return <div className="ci-loader"><div className="ci-spinner" /><p>Analyzing your career path...</p></div>;
    if (error) return <div className="ci-error"><h2>⚠️ Career Intelligence Unavailable</h2><p>{error}</p><p className="ci-error-hint">Complete your profile, add skills, and work experience to unlock career intelligence.</p></div>;
    if (!data || !data.bestCareer) return <div className="ci-empty"><h2>🎯 Career Intelligence</h2><p>Add more skills and experience to your profile to generate career recommendations.</p></div>;

    const { bestCareer, recommendedCareers, careerPath, nextCareerMove, explainableInsights, overallCareerReadiness } = data;

    return (
        <div className="ci-container animated-fade">
            <div className="ci-header">
                <h1>🧠 Career Intelligence</h1>
                <p className="ci-subtitle">AI-powered career path analysis based on your profile</p>
            </div>

            {/* Overview Cards */}
            <div className="ci-overview-grid">
                <div className="ci-card ci-card-readiness">
                    <h3>Career Readiness</h3>
                    <ScoreCircle score={overallCareerReadiness} label="Ready" size={140} />
                </div>
                <div className="ci-card ci-card-best">
                    <h3>Recommended Career</h3>
                    <div className="ci-best-role">{bestCareer.careerRole}</div>
                    <ScoreCircle score={bestCareer.compatibilityScore} label="Match" size={100} />
                    <div className="ci-transition">
                        <span>Transition Difficulty: </span>
                        <SeverityBadge level={bestCareer.transitionDifficulty} />
                    </div>
                    <div className="ci-skill-coverage">
                        <span>Skill Coverage: {Math.round(bestCareer.skillCoveragePercentage)}%</span>
                        <div className="ci-progress-bar"><div className="ci-progress-fill" style={{ width: `${bestCareer.skillCoveragePercentage}%` }} /></div>
                    </div>
                </div>
                <div className="ci-card ci-card-insights">
                    <h3>💡 Key Insights</h3>
                    <ul className="ci-insights-list">
                        {explainableInsights.map((insight, i) => <li key={i}>{insight}</li>)}
                    </ul>
                </div>
            </div>

            {/* Best Career - Why this career */}
            <div className="ci-card ci-why-section">
                <h3>Why {bestCareer.careerRole}?</h3>
                <div className="ci-why-grid">
                    <div className="ci-why-col">
                        <h4>✅ Strengths</h4>
                        {bestCareer.strengths.length > 0 ? bestCareer.strengths.map((s, i) => <div key={i} className="ci-strength-item">✓ {s}</div>) : <p className="ci-muted">No strong matches yet.</p>}
                    </div>
                    <div className="ci-why-col">
                        <h4>📋 Explanation</h4>
                        {bestCareer.explanation.map((e, i) => <p key={i} className="ci-explanation-text">{e}</p>)}
                    </div>
                </div>
            </div>

            {/* Skills Analysis */}
            <div className="ci-card">
                <h3>🔧 Skills Analysis for {bestCareer.careerRole}</h3>
                <div className="ci-skills-table">
                    <div className="ci-skills-header">
                        <span>Skill</span><span>Status</span><span>Priority</span>
                    </div>
                    {bestCareer.requiredSkillsAnalysis.map((skill, i) => (
                        <div key={i} className="ci-skills-row">
                            <span className="ci-skill-name">{skill.skillName}</span>
                            <StatusBadge status={skill.status} />
                            <SeverityBadge level={skill.priority} />
                        </div>
                    ))}
                </div>
            </div>

            {/* Career Path Timeline */}
            {careerPath && careerPath.length > 0 && (
                <div className="ci-card">
                    <h3>🛤️ Career Path</h3>
                    <div className="ci-timeline">
                        {careerPath.map((stage, i) => (
                            <div key={i} className={`ci-timeline-item ${stage.isCurrentRole ? 'ci-current' : ''}`}>
                                <div className="ci-timeline-dot" />
                                {i < careerPath.length - 1 && <div className="ci-timeline-line" />}
                                <div className="ci-timeline-content">
                                    <div className="ci-timeline-header">
                                        <span className="ci-timeline-role">{stage.role}</span>
                                        <span className="ci-timeline-level">{stage.level}</span>
                                        {stage.isCurrentRole && <span className="ci-current-badge">← You are here</span>}
                                    </div>
                                    <div className="ci-timeline-readiness">
                                        <span>Readiness: {Math.round(stage.readinessPercentage)}%</span>
                                        <div className="ci-progress-bar small"><div className="ci-progress-fill" style={{ width: `${stage.readinessPercentage}%` }} /></div>
                                    </div>
                                    {stage.missingSkills.length > 0 && (
                                        <div className="ci-timeline-missing">
                                            <span>Missing: </span>{stage.missingSkills.map((s, j) => <span key={j} className="ci-missing-chip">{s}</span>)}
                                        </div>
                                    )}
                                    <p className="ci-timeline-step">{stage.suggestedNextStep}</p>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {/* Next Career Move */}
            {nextCareerMove && (
                <div className="ci-card ci-next-move">
                    <h3>🚀 Next Career Move</h3>
                    <div className="ci-next-move-header">
                        <div className="ci-next-move-role">{nextCareerMove.targetRole}</div>
                        <ScoreCircle score={nextCareerMove.readinessPercentage} label="Ready" size={90} />
                    </div>
                    <div className="ci-next-move-body">
                        <div className="ci-next-move-col">
                            <h4>Why</h4>
                            {nextCareerMove.whyReasons.map((r, i) => <p key={i}>• {r}</p>)}
                        </div>
                        <div className="ci-next-move-col">
                            <h4>Focus Next</h4>
                            {nextCareerMove.focusNext.map((f, i) => (
                                <div key={i} className="ci-focus-item">
                                    <span>{f.skillName}</span>
                                    <SeverityBadge level={f.priority} />
                                </div>
                            ))}
                        </div>
                    </div>
                    <div className="ci-action-box">
                        <h4>📌 Recommended Action</h4>
                        <p>{nextCareerMove.action}</p>
                    </div>
                </div>
            )}

            {/* All Recommended Careers */}
            <div className="ci-card">
                <h3>📊 All Career Recommendations</h3>
                <div className="ci-recommendations-grid">
                    {recommendedCareers.map((rec, i) => (
                        <div key={i} className="ci-rec-card">
                            <div className="ci-rec-header">
                                <span className="ci-rec-rank">#{i + 1}</span>
                                <span className="ci-rec-role">{rec.careerRole}</span>
                            </div>
                            <div className="ci-rec-scores">
                                <div><span>Compatibility</span><strong>{Math.round(rec.compatibilityScore)}%</strong></div>
                                <div><span>Readiness</span><strong>{Math.round(rec.readinessScore)}%</strong></div>
                                <div><span>Coverage</span><strong>{Math.round(rec.skillCoveragePercentage)}%</strong></div>
                            </div>
                            <div className="ci-rec-difficulty">
                                <SeverityBadge level={rec.transitionDifficulty} />
                            </div>
                            <p className="ci-rec-action">{rec.recommendedNextAction}</p>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
};

export default CareerIntelligence;
