import React, { useState, useEffect, useCallback } from 'react';
import {
    ShieldAlert, ShieldCheck, AlertTriangle, CheckCircle,
    Loader, Search, Clock, Info, ChevronDown, ChevronUp, XCircle
} from 'lucide-react';
import { candidateService } from '../../services/candidate';
import './JobScamDetector.css';

// ── Helper: category → CSS class ────────────────────────────────────────────
const catClass = (cat) => {
    if (!cat) return '';
    return `cat-${cat.toLowerCase().replace(' ', '-')}`;
};

// ── Helper: score → ring colour ─────────────────────────────────────────────
const scoreColor = (score) => {
    if (score <= 24) return '#10b981';
    if (score <= 49) return '#f59e0b';
    if (score <= 74) return '#f97316';
    return '#ef4444';
};

// ── Score ring SVG ───────────────────────────────────────────────────────────
const ScoreRing = ({ score }) => {
    const radius = 38;
    const circumference = 2 * Math.PI * radius;
    const filled = (score / 100) * circumference;
    const color = scoreColor(score);

    return (
        <div className="score-ring-wrap">
            <svg width="96" height="96" viewBox="0 0 96 96">
                <circle cx="48" cy="48" r={radius} fill="none" stroke="rgba(255,255,255,0.06)" strokeWidth="10" />
                <circle
                    cx="48" cy="48" r={radius}
                    fill="none"
                    stroke={color}
                    strokeWidth="10"
                    strokeDasharray={`${filled} ${circumference}`}
                    strokeLinecap="round"
                    style={{ transition: 'stroke-dasharray 0.6s ease' }}
                />
            </svg>
            <div className="score-text">
                <span className="score-num" style={{ color }}>{score}</span>
                <span className="score-lbl">/ 100</span>
            </div>
        </div>
    );
};

// ── Empty result placeholder ─────────────────────────────────────────────────
const EmptyResultState = () => (
    <div className="risk-empty-state">
        <ShieldAlert size={48} />
        <p>Submit a job posting above to receive an instant risk analysis report.</p>
    </div>
);

// ── Full analysis result ─────────────────────────────────────────────────────
const AnalysisResult = ({ result }) => {
    const [showIndicators, setShowIndicators] = useState(false);

    return (
        <div className="risk-result-panel">
            {/* Score header */}
            <div className="score-display">
                <ScoreRing score={result.riskScore} />
                <div className="score-meta">
                    <span className={`cat-pill ${catClass(result.riskCategory)}`}>
                        {result.riskCategory} Risk
                    </span>
                    <h3 style={{ color: scoreColor(result.riskScore) }}>{result.riskScore} / 100</h3>
                    <p>{result.jobTitle} · {result.companyName}</p>
                </div>
            </div>

            {/* Explanation */}
            <div className="result-section">
                <h4><Info size={12} style={{ marginRight: 4 }} />Explanation</h4>
                <p className="explanation-text">{result.explanation}</p>
            </div>

            {/* Warning signs */}
            {result.warningSigns && result.warningSigns.length > 0 && (
                <div className="result-section">
                    <h4><AlertTriangle size={12} style={{ marginRight: 4 }} />Warning Signs Detected ({result.warningSigns.length})</h4>
                    <ul className="warning-list">
                        {result.warningSigns.map((w, i) => (
                            <li key={i}><XCircle size={14} />{w}</li>
                        ))}
                    </ul>
                </div>
            )}

            {result.warningSigns && result.warningSigns.length === 0 && (
                <div className="result-section">
                    <h4><ShieldCheck size={12} style={{ marginRight: 4 }} />Warning Signs</h4>
                    <p style={{ fontSize: '0.85rem', color: '#6ee7b7' }}>No warning-sign patterns detected in the submitted text.</p>
                </div>
            )}

            {/* Verification checklist */}
            <div className="result-section">
                <h4><CheckCircle size={12} style={{ marginRight: 4 }} />Verification Steps</h4>
                <ul className="verify-list">
                    {result.verificationSteps.map((s, i) => (
                        <li key={i}>
                            <span className="vcheck"><CheckCircle size={11} /></span>
                            {s}
                        </li>
                    ))}
                </ul>
            </div>

            {/* Confidence note */}
            <div className="result-section">
                <h4><Info size={12} style={{ marginRight: 4 }} />Data Coverage</h4>
                <p className="explanation-text" style={{ fontSize: '0.8rem' }}>{result.confidenceNote}</p>
            </div>

            {/* Indicator breakdown (collapsible) */}
            <div className="result-section">
                <h4
                    style={{ cursor: 'pointer', userSelect: 'none', display: 'flex', alignItems: 'center', gap: '0.4rem' }}
                    onClick={() => setShowIndicators(v => !v)}
                    id="indicators-toggle"
                    aria-expanded={showIndicators}
                >
                    <ShieldAlert size={12} />Risk Indicator Details
                    {showIndicators ? <ChevronUp size={13} /> : <ChevronDown size={13} />}
                </h4>
                {showIndicators && (
                    <div className="indicators-grid">
                        {result.indicators.map(ind => (
                            <div
                                key={ind.key}
                                className={`indicator-row ${ind.triggered ? 'triggered' : 'clean'}`}
                            >
                                <span className="indicator-icon">
                                    {ind.triggered ? <XCircle size={14} /> : <CheckCircle size={14} color="#10b981" />}
                                </span>
                                <span className="indicator-label">{ind.label}</span>
                                <span className="indicator-category">{ind.category}</span>
                                {ind.triggered && (
                                    <span style={{ fontSize: '0.72rem', color: '#f87171' }}>+{ind.pointsAwarded}pts</span>
                                )}
                            </div>
                        ))}
                    </div>
                )}
            </div>

            {/* Disclaimer */}
            <div className="disclaimer-box">
                <Info size={14} style={{ flexShrink: 0, marginTop: 2 }} />
                <span>{result.disclaimer}</span>
            </div>
        </div>
    );
};

// ── History row ──────────────────────────────────────────────────────────────
const HistoryPanel = ({ history, onView }) => {
    if (!history || history.length === 0) {
        return (
            <div className="risk-empty-state">
                <Clock size={36} />
                <p>No previous analyses yet. Submit a posting to get started.</p>
            </div>
        );
    }

    return (
        <table className="history-table">
            <thead>
                <tr>
                    <th>Job Title</th>
                    <th>Company</th>
                    <th>Score</th>
                    <th>Risk</th>
                    <th>Date</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                {history.map(h => (
                    <tr key={h.analysisId}>
                        <td>{h.jobTitle}</td>
                        <td>{h.companyName}</td>
                        <td style={{ color: scoreColor(h.riskScore), fontWeight: 700 }}>{h.riskScore}</td>
                        <td><span className={`cat-pill ${catClass(h.riskCategory)}`}>{h.riskCategory}</span></td>
                        <td>{new Date(h.analyzedAt).toLocaleDateString()}</td>
                        <td>
                            <button
                                className="btn-view"
                                onClick={() => onView(h.analysisId)}
                                aria-label={`View analysis for ${h.jobTitle}`}
                            >
                                View
                            </button>
                        </td>
                    </tr>
                ))}
            </tbody>
        </table>
    );
};

// ── Main page ────────────────────────────────────────────────────────────────
const JobScamDetector = () => {
    const [form, setForm] = useState({
        jobTitle: '',
        companyName: '',
        jobDescription: '',
        salaryDetails: '',
        applicationInstructions: '',
        employerInformation: '',
    });
    const [analysing, setAnalysing] = useState(false);
    const [error, setError] = useState(null);
    const [result, setResult] = useState(null);
    const [history, setHistory] = useState([]);
    const [loadingHistory, setLoadingHistory] = useState(true);

    const fetchHistory = useCallback(async () => {
        try {
            setLoadingHistory(true);
            const res = await candidateService.getJobRiskHistory();
            setHistory(res.data || []);
        } catch {
            // History loading failure is non-critical
        } finally {
            setLoadingHistory(false);
        }
    }, []);

    useEffect(() => { fetchHistory(); }, [fetchHistory]);

    const handleChange = (e) =>
        setForm(f => ({ ...f, [e.target.name]: e.target.value }));

    const handleAnalyze = async (e) => {
        e.preventDefault();
        if (!form.jobTitle.trim() || !form.companyName.trim() || !form.jobDescription.trim()) {
            setError('Job title, company name, and job description are required.');
            return;
        }
        setError(null);
        setAnalysing(true);
        try {
            const res = await candidateService.analyzeJobRisk(form);
            setResult(res.data);
            fetchHistory();         // refresh history list
        } catch (err) {
            setError(err?.response?.data?.message || 'Analysis failed. Please try again.');
        } finally {
            setAnalysing(false);
        }
    };

    const handleViewHistory = async (id) => {
        try {
            const res = await candidateService.getJobRiskAnalysis(id);
            setResult(res.data);
            window.scrollTo({ top: 0, behavior: 'smooth' });
        } catch {
            setError('Could not load that analysis.');
        }
    };

    return (
        <div className="job-risk-container animated-fade">
            {/* Page header */}
            <div className="risk-page-header">
                <div className="header-icon">
                    <ShieldAlert size={26} color="#fff" />
                </div>
                <div>
                    <h1>Job Scam Risk Detector</h1>
                    <p>Paste a job posting's details below for an instant, explainable risk analysis powered by rule-based intelligence.</p>
                </div>
            </div>

            <div className="risk-layout">
                {/* ── Left: Input form ── */}
                <div className="risk-panel">
                    <h2><Search size={16} />Analyse a Job Posting</h2>

                    {error && (
                        <div className="risk-error-banner" role="alert">
                            <AlertTriangle size={16} style={{ flexShrink: 0 }} />
                            {error}
                        </div>
                    )}

                    <form className="risk-form" onSubmit={handleAnalyze} noValidate>
                        <div className="form-row">
                            <div className="form-group">
                                <label htmlFor="jobTitle">Job Title *</label>
                                <input
                                    id="jobTitle"
                                    name="jobTitle"
                                    className="risk-input"
                                    placeholder="e.g. Software Engineer"
                                    value={form.jobTitle}
                                    onChange={handleChange}
                                    required
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="companyName">Company Name *</label>
                                <input
                                    id="companyName"
                                    name="companyName"
                                    className="risk-input"
                                    placeholder="e.g. Acme Corp"
                                    value={form.companyName}
                                    onChange={handleChange}
                                    required
                                />
                            </div>
                        </div>

                        <div className="form-group">
                            <label htmlFor="jobDescription">Job Description * <span style={{ fontSize: '0.7rem', color: '#475569' }}>(paste full text)</span></label>
                            <textarea
                                id="jobDescription"
                                name="jobDescription"
                                className="risk-textarea"
                                rows={5}
                                placeholder="Paste the full job description here..."
                                value={form.jobDescription}
                                onChange={handleChange}
                                required
                            />
                        </div>

                        <div className="form-group">
                            <label htmlFor="salaryDetails">Salary Details</label>
                            <input
                                id="salaryDetails"
                                name="salaryDetails"
                                className="risk-input"
                                placeholder="e.g. $80,000 – $100,000 per year"
                                value={form.salaryDetails}
                                onChange={handleChange}
                            />
                        </div>

                        <div className="form-group">
                            <label htmlFor="applicationInstructions">Application Instructions</label>
                            <textarea
                                id="applicationInstructions"
                                name="applicationInstructions"
                                className="risk-textarea"
                                rows={2}
                                placeholder="How should candidates apply?"
                                value={form.applicationInstructions}
                                onChange={handleChange}
                            />
                        </div>

                        <div className="form-group">
                            <label htmlFor="employerInformation">Employer Information</label>
                            <textarea
                                id="employerInformation"
                                name="employerInformation"
                                className="risk-textarea"
                                rows={2}
                                placeholder="Company address, website, contact details..."
                                value={form.employerInformation}
                                onChange={handleChange}
                            />
                        </div>

                        <button
                            id="analyze-btn"
                            type="submit"
                            className="risk-submit-btn"
                            disabled={analysing}
                        >
                            {analysing
                                ? <><Loader size={16} className="spin" /> Analysing…</>
                                : <><ShieldAlert size={16} /> Analyse Risk</>}
                        </button>
                    </form>
                </div>

                {/* ── Right: Result ── */}
                <div className="risk-panel risk-result-panel">
                    <h2><ShieldAlert size={16} />Analysis Result</h2>
                    {result ? <AnalysisResult result={result} /> : <EmptyResultState />}
                </div>
            </div>

            {/* ── History ── */}
            <div className="risk-panel" style={{ marginTop: '1.5rem' }}>
                <h2><Clock size={16} />Your Analysis History</h2>
                {loadingHistory
                    ? <p style={{ color: '#475569', fontSize: '0.85rem' }}>Loading history…</p>
                    : <HistoryPanel history={history} onView={handleViewHistory} />}
            </div>
        </div>
    );
};

export default JobScamDetector;
