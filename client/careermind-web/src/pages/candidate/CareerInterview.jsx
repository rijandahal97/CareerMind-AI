import React, { useState, useEffect } from 'react';
import { candidateService } from '../../services/candidate';
import {
    Bot, Play, CheckCircle, Target, MessageSquare, AlertCircle,
    ArrowRight, Activity, Award, Briefcase, ChevronRight
} from 'lucide-react';
import './CareerInterview.css';

const CareerInterview = () => {
    const [jobs, setJobs] = useState([]);
    const [history, setHistory] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // Active session state
    const [activeSession, setActiveSession] = useState(null);
    const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
    const [answerText, setAnswerText] = useState('');
    const [feedback, setFeedback] = useState(null);
    const [submitting, setSubmitting] = useState(false);

    // Readiness state
    const [readiness, setReadiness] = useState(null);

    // Setup form
    const [setupForm, setSetupForm] = useState({
        targetJobId: '',
        interviewType: 'Mixed',
        difficulty: 'Intermediate',
        numberOfQuestions: 5
    });

    useEffect(() => {
        fetchInitialData();
    }, []);

    const fetchInitialData = async () => {
        setLoading(true);
        setError(null);
        try {
            const [jobsRes, historyRes] = await Promise.all([
                candidateService.getJobMatches(), // Use job matches or all jobs for target
                candidateService.getInterviews()
            ]);
            // If getJobMatches returns array of matches, map to job
            setJobs(jobsRes.data.map(m => m.job || m) || []);
            setHistory(historyRes.data || []);
        } catch (err) {
            console.error(err);
            setError("Failed to load interview dashboard data.");
        } finally {
            setLoading(false);
        }
    };

    const handleStartInterview = async (e) => {
        e.preventDefault();
        setLoading(true);
        try {
            const req = {
                targetJobId: setupForm.targetJobId || null,
                interviewType: setupForm.interviewType,
                difficulty: setupForm.difficulty,
                numberOfQuestions: setupForm.numberOfQuestions
            };
            const res = await candidateService.startInterview(req);
            setActiveSession(res.data);
            setCurrentQuestionIndex(0);
            setFeedback(null);
            setAnswerText('');
            // fetch history again to update
            const hRes = await candidateService.getInterviews();
            setHistory(hRes.data || []);
        } catch (err) {
            setError(err.response?.data?.message || "Failed to start interview");
        } finally {
            setLoading(false);
        }
    };

    const resumeSession = async (sessionId) => {
        setLoading(true);
        try {
            const res = await candidateService.getInterview(sessionId);
            if (res.data.status === 'Completed') {
                const readRes = await candidateService.getInterviewReadiness(sessionId);
                setReadiness(readRes.data);
                setActiveSession(res.data);
            } else {
                setActiveSession(res.data);
                // Find first unanswered question
                const lastIdx = res.data.questions.findIndex(q => !q.answers || q.answers.length === 0);
                setCurrentQuestionIndex(lastIdx >= 0 ? lastIdx : 0);
                setFeedback(null);
                setAnswerText('');
            }
        } catch(err) {
            setError("Failed to resume session.");
        } finally {
            setLoading(false);
        }
    };

    const submitAnswer = async () => {
        if (!answerText.trim()) return;
        setSubmitting(true);
        try {
            const currentQ = activeSession.questions[currentQuestionIndex];
            const res = await candidateService.submitInterviewAnswer(activeSession.id, currentQ.id, { answerText });
            setFeedback(res.data);

            // update local session
            const updatedSession = {...activeSession};
            updatedSession.questions[currentQuestionIndex].answers = [res.data]; // just a placeholder to know it's answered
            setActiveSession(updatedSession);

        } catch (err) {
            alert("Failed to submit answer: " + (err.response?.data?.message || "Error"));
        } finally {
            setSubmitting(false);
        }
    };

    const nextQuestion = async () => {
        setFeedback(null);
        setAnswerText('');
        if (currentQuestionIndex < activeSession.questions.length - 1) {
            setCurrentQuestionIndex(prev => prev + 1);
        } else {
            // Complete
            setLoading(true);
            try {
                const res = await candidateService.completeInterview(activeSession.id);
                setReadiness(res.data);
                // Update history
                fetchInitialData();
            } catch (err) {
                alert("Failed to complete.");
            } finally {
                setLoading(false);
            }
        }
    };

    if (loading && !activeSession && !history.length) {
        return (
            <div className="empty-state-card mt-6">
                <Bot className="pulse-icon" size={48} color="#3b82f6" />
                <p>Initializing AI Interview Coach...</p>
            </div>
        );
    }

    if (error) {
        return <div className="alert error">{error}</div>;
    }

    // STATE 4: Completion / Readiness
    if (readiness) {
        return (
            <div className="interview-workspace animated-fade">
                <div className="workspace-header">
                    <h2>Interview Completed</h2>
                    <button className="btn-secondary" onClick={() => { setActiveSession(null); setReadiness(null); }}>Back to Dashboard</button>
                </div>

                <div className="readiness-dashboard-grid mt-6">
                    <div className="readiness-main-score">
                        <div className="score-circle large">
                            <span>{readiness.overallReadinessScore}</span>
                        </div>
                        <h3>Overall Readiness Score</h3>
                    </div>

                    <div className="readiness-metrics">
                        <div className="metric-box">
                            <span className="metric-label">Technical</span>
                            <div className="comp-bar"><div className="comp-fill" style={{width: `${readiness.technicalScore}%`, background: 'var(--primary)'}}></div></div>
                            <span className="metric-val">{readiness.technicalScore}</span>
                        </div>
                        <div className="metric-box">
                            <span className="metric-label">Communication</span>
                            <div className="comp-bar"><div className="comp-fill" style={{width: `${readiness.communicationScore}%`, background: 'var(--success)'}}></div></div>
                            <span className="metric-val">{readiness.communicationScore}</span>
                        </div>
                        <div className="metric-box">
                            <span className="metric-label">Role Alignment</span>
                            <div className="comp-bar"><div className="comp-fill" style={{width: `${readiness.roleAlignmentScore}%`, background: 'var(--warning)'}}></div></div>
                            <span className="metric-val">{readiness.roleAlignmentScore}</span>
                        </div>
                        <div className="metric-box">
                            <span className="metric-label">Completeness</span>
                            <div className="comp-bar"><div className="comp-fill" style={{width: `${readiness.completenessScore}%`, background: 'var(--secondary)'}}></div></div>
                            <span className="metric-val">{readiness.completenessScore}</span>
                        </div>
                    </div>
                </div>

                <div className="gap-analysis-wrapper mt-6">
                    <div className="gap-card positive">
                        <h3><CheckCircle size={18} color="var(--success)"/> Top Strengths</h3>
                        <ul>
                            {readiness.strengths?.map((str, i) => <li key={i}>{str}</li>)}
                        </ul>
                    </div>
                    <div className="gap-card negative">
                        <h3><AlertCircle size={18} color="var(--danger)"/> Key Weaknesses</h3>
                        <ul>
                            {readiness.weaknesses?.map((w, i) => <li key={i}>{w}</li>)}
                        </ul>
                    </div>
                </div>

                <div className="form-card mt-6">
                    <h3>Recommendations</h3>
                    <ul style={{listStyle:'disc', marginLeft: '20px', marginTop: '10px'}}>
                        {readiness.recommendations?.map((r, i) => <li key={i} style={{marginBottom: '5px'}}>{r}</li>)}
                    </ul>
                </div>
            </div>
        );
    }

    // STATE 2: Interview Workspace
    if (activeSession) {
        const question = activeSession.questions[currentQuestionIndex];
        const isLast = currentQuestionIndex === activeSession.questions.length - 1;
        const progress = ((currentQuestionIndex) / activeSession.questions.length) * 100;

        return (
            <div className="interview-workspace animated-fade">
                <div className="workspace-header">
                    <div>
                        <h2>{activeSession.sessionTitle}</h2>
                        <div className="workspace-meta">
                            <span className="badge-lvl lvl-2">{question.category}</span>
                            <span className="badge-lvl lvl-3">{question.difficulty}</span>
                        </div>
                    </div>
                    <button className="btn-secondary" onClick={() => setActiveSession(null)}>Exit</button>
                </div>

                <div className="progress-container mt-4 mb-6">
                    <div className="progress-labels">
                        <span>Question {currentQuestionIndex + 1} of {activeSession.questions.length}</span>
                        <span>{Math.round(progress)}% Completed</span>
                    </div>
                    <div className="comp-bar"><div className="comp-fill" style={{width: `${progress}%`}}></div></div>
                </div>

                <div className="form-card question-box">
                    <div className="q-icon"><MessageSquare size={24} color="var(--primary)" /></div>
                    <h3 className="q-text">{question.questionText}</h3>
                </div>

                {!feedback ? (
                    <div className="form-card mt-6">
                        <label className="text-sm font-semibold mb-2 block">Your Answer</label>
                        <textarea
                            className="input-premium resize-none"
                            rows={8}
                            placeholder="Type your answer here... Be as specific and structured as possible."
                            value={answerText}
                            onChange={(e) => setAnswerText(e.target.value)}
                        />
                        <div className="form-actions mt-4">
                            <button className="btn-primary" onClick={submitAnswer} disabled={submitting || !answerText.trim()}>
                                {submitting ? 'Analyzing Answer...' : 'Submit Answer'}
                            </button>
                        </div>
                    </div>
                ) : (
                    // STATE 3: Feedback
                    <div className="feedback-card mt-6 animated-fade">
                        <div className="feedback-header">
                            <div className="feedback-score">
                                <span className="score-val">{Math.round(feedback.score)}</span>
                                <span className="score-max">/ 100</span>
                            </div>
                            <h3>Answer Evaluation</h3>
                        </div>
                        <div className="gap-analysis-wrapper mt-4">
                            <div className="gap-card positive">
                                <strong>Strengths</strong>
                                <p>{feedback.strengths}</p>
                            </div>
                            <div className="gap-card negative">
                                <strong>Weaknesses</strong>
                                <p>{feedback.weaknesses}</p>
                            </div>
                        </div>
                        <div className="feedback-details mt-4">
                            <p><strong>What to Improve:</strong> {feedback.whatToImprove}</p>
                            {feedback.topicsToRevise && <p><strong>Expected Topics:</strong> {feedback.topicsToRevise}</p>}
                        </div>
                        <div className="form-actions mt-6">
                            <button className="btn-primary" onClick={nextQuestion}>
                                {isLast ? 'Complete Interview' : 'Next Question'} <ChevronRight size={16} />
                            </button>
                        </div>
                    </div>
                )}
            </div>
        );
    }

    // STATE 1: Dashboard
    const lastSession = history.length > 0 ? history[0] : null;

    return (
        <div className="career-interview-dashboard animated-fade">

            {/* Top Stats */}
            {history.length > 0 && (
                <div className="dashboard-top-stats mb-6">
                    <div className="stat-card-premium">
                        <div className="stat-icon-wrapper"><Award size={24} color="var(--primary)" /></div>
                        <div className="stat-info">
                            <span className="stat-label">Last Score</span>
                            <h3>{lastSession.overallReadinessScore || 'N/A'}</h3>
                        </div>
                    </div>
                    <div className="stat-card-premium">
                        <div className="stat-icon-wrapper"><Activity size={24} color="var(--success)" /></div>
                        <div className="stat-info">
                            <span className="stat-label">Completed Sessions</span>
                            <h3>{history.filter(h => h.status === 'Completed').length}</h3>
                        </div>
                    </div>
                </div>
            )}

            <div className="dashboard-split">

                {/* Setup New Interview */}
                <div className="setup-section form-card">
                    <div className="section-header-simple">
                        <Bot size={24} color="var(--primary)" className="mr-2 inline" />
                        <h2 className="inline ml-2">New Mock Interview</h2>
                    </div>
                    <p className="text-secondary mb-6 text-sm">Configure a personalized AI-driven interview session based on your profile and target role.</p>

                    <form onSubmit={handleStartInterview}>
                        <div className="form-group">
                            <label>Target Job <span className="text-xs text-secondary">(Optional)</span></label>
                            <select
                                className="select-premium"
                                value={setupForm.targetJobId}
                                onChange={e => setSetupForm({...setupForm, targetJobId: e.target.value})}
                            >
                                <option value="">General Interview</option>
                                {jobs.map(j => (
                                    <option key={j.id} value={j.id}>{j.title}</option>
                                ))}
                            </select>
                        </div>
                        <div className="form-row">
                            <div className="form-group half">
                                <label>Interview Type</label>
                                <select
                                    className="select-premium"
                                    value={setupForm.interviewType}
                                    onChange={e => setSetupForm({...setupForm, interviewType: e.target.value})}
                                >
                                    <option value="Technical">Technical</option>
                                    <option value="Behavioral">Behavioral</option>
                                    <option value="Mixed">Mixed</option>
                                </select>
                            </div>
                            <div className="form-group half">
                                <label>Difficulty</label>
                                <select
                                    className="select-premium"
                                    value={setupForm.difficulty}
                                    onChange={e => setSetupForm({...setupForm, difficulty: e.target.value})}
                                >
                                    <option value="Beginner">Beginner</option>
                                    <option value="Intermediate">Intermediate</option>
                                    <option value="Advanced">Advanced</option>
                                </select>
                            </div>
                        </div>
                        <div className="form-group">
                            <label>Number of Questions</label>
                            <input
                                type="number"
                                className="input-premium"
                                value={setupForm.numberOfQuestions}
                                onChange={e => setSetupForm({...setupForm, numberOfQuestions: parseInt(e.target.value) || 5})}
                                min="3" max="15"
                            />
                        </div>
                        <button type="submit" className="btn-primary w-full mt-4" disabled={loading}>
                            {loading ? 'Preparing...' : <><Play size={16} /> Start Interview Session</>}
                        </button>
                    </form>
                </div>

                {/* History */}
                <div className="history-section form-card">
                    <h3>Interview History</h3>
                    <div className="history-list mt-4">
                        {history.length === 0 ? (
                            <div className="empty-state-card p-4">
                                <p className="text-sm">No interviews completed yet.</p>
                            </div>
                        ) : (
                            history.map(session => (
                                <div key={session.id} className="history-item">
                                    <div className="hist-info">
                                        <h4>{session.sessionTitle}</h4>
                                        <div className="hist-meta">
                                            <span className={`status-badge ${session.status.toLowerCase()}`}>{session.status}</span>
                                            <span>{new Date(session.createdAt).toLocaleDateString()}</span>
                                        </div>
                                    </div>
                                    <div className="hist-action">
                                        {session.status === 'Completed' ? (
                                            <div className="hist-score">{session.overallReadinessScore}</div>
                                        ) : (
                                            <button className="btn-secondary text-sm" onClick={() => resumeSession(session.id)}>
                                                Resume
                                            </button>
                                        )}
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </div>
            </div>

        </div>
    );
};

export default CareerInterview;
