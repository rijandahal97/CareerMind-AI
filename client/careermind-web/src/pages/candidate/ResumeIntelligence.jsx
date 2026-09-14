import React, { useState, useEffect } from 'react';
import { resumeService } from '../../services/resume';
import { 
    UploadCloud, FileText, CheckCircle, AlertTriangle, Star, 
    Trash2, RefreshCw, Activity, Layers, Target, BookOpen, AlertCircle, Sparkles 
} from 'lucide-react';
import './Candidate.css';

const ResumeIntelligence = () => {
    const [resumes, setResumes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [uploading, setUploading] = useState(false);
    const [analyzingId, setAnalyzingId] = useState(null);
    const [selectedAnalysis, setSelectedAnalysis] = useState(null);
    const [activeTab, setActiveTab] = useState('list'); // 'list' | 'analysis'
    const [errorMsg, setErrorMsg] = useState('');

    useEffect(() => {
        fetchResumes();
    }, []);

    const fetchResumes = async () => {
        setLoading(true);
        try {
            const res = await resumeService.getResumes();
            setResumes(res.data);
        } catch (err) {
            console.error('Error fetching resumes:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleUpload = async (e) => {
        const file = e.target.files[0];
        if (!file) return;

        if (file.size > 10 * 1024 * 1024) {
            setErrorMsg('File exceeds 10MB limit.');
            return;
        }

        setUploading(true);
        setErrorMsg('');
        try {
            await resumeService.uploadResume(file);
            await fetchResumes();
        } catch (err) {
            setErrorMsg(err.response?.data?.message || 'Error uploading resume.');
        } finally {
            setUploading(false);
            e.target.value = null; // reset input
        }
    };

    const handleAnalyze = async (resumeId) => {
        setAnalyzingId(resumeId);
        setErrorMsg('');
        try {
            const res = await resumeService.analyzeResume(resumeId);
            setSelectedAnalysis(res.data);
            setActiveTab('analysis');
            fetchResumes(); // Update latest score in list
        } catch (err) {
            setErrorMsg('Failed to run intelligence analysis.');
        } finally {
            setAnalyzingId(null);
        }
    };

    const handleViewAnalysis = async (resumeId) => {
        try {
            const res = await resumeService.getLatestAnalysis(resumeId);
            if (res.data) {
                setSelectedAnalysis(res.data);
                setActiveTab('analysis');
            } else {
                handleAnalyze(resumeId);
            }
        } catch (error) {
            handleAnalyze(resumeId);
        }
    };

    const handleDelete = async (resumeId) => {
        if (!window.confirm('Delete this resume?')) return;
        try {
            await resumeService.deleteResume(resumeId);
            if (selectedAnalysis?.resumeId === resumeId) {
                setSelectedAnalysis(null);
                setActiveTab('list');
            }
            fetchResumes();
        } catch (err) {
            setErrorMsg('Failed to delete resume.');
        }
    };

    const getScoreColor = (score) => {
        if (score >= 85) return '#10b981'; // Green
        if (score >= 70) return '#3b82f6'; // Blue
        if (score >= 50) return '#f59e0b'; // Amber
        return '#ef4444'; // Red
    };

    const ScoreCircle = ({ score, label, color }) => (
        <div className="score-circle-container">
            <div className="score-circle" style={{ borderColor: color, color: color }}>
                <span className="score-value">{score}</span>
            </div>
            <div className="score-label">{label}</div>
        </div>
    );

    return (
        <div className="career-intelligence animated-fade">
            <div className="section-header-premium">
                <div className="header-icon-wrap" style={{ backgroundColor: 'rgba(56, 189, 248, 0.1)' }}>
                    <FileText color="#38bdf8" size={32} />
                </div>
                <div className="header-text-wrap">
                    <h2>Resume Intelligence Engine</h2>
                    <p>Upload your resume. Our AI Engine extracts, parses, and scores your profile against deterministic career metrics.</p>
                </div>
            </div>

            {errorMsg && <div className="alert error">{errorMsg}</div>}

            {activeTab === 'analysis' && selectedAnalysis && (
                <button className="btn-secondary mb-4" onClick={() => setActiveTab('list')}>
                    ← Back to Resumes
                </button>
            )}

            {activeTab === 'list' && (
                <div className="resume-manager">
                    <div className="upload-zone mb-6">
                        <label className={`upload-card ${uploading ? 'disabled' : ''}`}>
                            <input type="file" accept=".pdf,.docx" onChange={handleUpload} disabled={uploading} hidden />
                            <div className="upload-content">
                                {uploading ? <RefreshCw className="spin" size={48} color="#3b82f6" /> : <UploadCloud size={48} color="#3b82f6" />}
                                <h3>{uploading ? 'Processing Resume...' : 'Upload New Resume'}</h3>
                                <p>Supports PDF and DOCX (Max 10MB)</p>
                            </div>
                        </label>
                    </div>

                    {loading ? (
                        <div className="app-loader"><p>Loading resumes...</p></div>
                    ) : (
                        <div className="resumes-list">
                            {resumes.length === 0 ? (
                                <div className="empty-state-card w-100">
                                    <FileText size={48} color="#94a3b8" />
                                    <p>No resumes uploaded. Upload one to get AI insights.</p>
                                </div>
                            ) : (
                                resumes.map(resume => (
                                    <div key={resume.id} className="resume-card-premium">
                                        <div className="card-top">
                                            <div className="resume-icon">
                                                <FileText size={24} color="#3b82f6" />
                                            </div>
                                            <div className="resume-details">
                                                <h3>{resume.originalFileName}</h3>
                                                <p>Uploaded: {new Date(resume.uploadedAt).toLocaleDateString()}</p>
                                                <p className="size-text">{(resume.fileSize / 1024).toFixed(0)} KB</p>
                                            </div>
                                            <div className="resume-score-mini">
                                                {resume.hasAnalysis ? (
                                                    <div className="mini-score" style={{ color: getScoreColor(resume.latestATSScore) }}>
                                                        {resume.latestATSScore} <span>ATS Score</span>
                                                    </div>
                                                ) : (
                                                    <span className="unscored-badge">Not Analyzed</span>
                                                )}
                                            </div>
                                        </div>
                                        <div className="card-actions">
                                            <button 
                                                className="btn-primary-sm" 
                                                onClick={() => handleViewAnalysis(resume.id)}
                                                disabled={analyzingId === resume.id}
                                            >
                                                {analyzingId === resume.id ? <RefreshCw className="spin" size={16} /> : <Activity size={16} />}
                                                {resume.hasAnalysis ? 'View Analysis' : 'Run Intelligence'}
                                            </button>
                                            <button className="btn-icon-danger" onClick={() => handleDelete(resume.id)} title="Delete Resume">
                                                <Trash2 size={16} />
                                            </button>
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                    )}
                </div>
            )}

            {activeTab === 'analysis' && selectedAnalysis && (
                <div className="resume-analysis-dashboard animated-fade">
                    <div className="analysis-hero" style={{ background: `linear-gradient(135deg, ${getScoreColor(selectedAnalysis.atsScore)}22, transparent)` }}>
                        <div className="hero-left">
                            <div className="score-massive" style={{ color: getScoreColor(selectedAnalysis.atsScore) }}>
                                {selectedAnalysis.atsScore}
                            </div>
                            <div className="hero-text">
                                <h3>Overall ATS & Intelligence Score</h3>
                                <p>Interpretation: <strong>{selectedAnalysis.atsInterpretation}</strong></p>
                                <p className="small-date">Analyzed on {new Date(selectedAnalysis.analyzedAt).toLocaleString()}</p>
                            </div>
                        </div>
                        <div className="hero-right">
                            <Sparkles size={64} style={{ opacity: 0.2, color: getScoreColor(selectedAnalysis.atsScore) }} />
                        </div>
                    </div>

                    <div className="score-breakdown-grid mt-6">
                        <ScoreCircle score={selectedAnalysis.structureScore} label="Structure" color={getScoreColor(selectedAnalysis.structureScore)} />
                        <ScoreCircle score={selectedAnalysis.skillsScore} label="Skills Match" color={getScoreColor(selectedAnalysis.skillsScore)} />
                        <ScoreCircle score={selectedAnalysis.keywordScore} label="Keywords" color={getScoreColor(selectedAnalysis.keywordScore)} />
                        <ScoreCircle score={selectedAnalysis.experienceScore} label="Experience" color={getScoreColor(selectedAnalysis.experienceScore)} />
                        <ScoreCircle score={selectedAnalysis.educationScore} label="Education" color={getScoreColor(selectedAnalysis.educationScore)} />
                        <ScoreCircle score={selectedAnalysis.completenessScore} label="Completeness" color={getScoreColor(selectedAnalysis.completenessScore)} />
                    </div>

                    <div className="analysis-details-grid mt-6">
                        <div className="insight-card strengths-card">
                            <div className="card-header"><CheckCircle color="#10b981" /> <h3>Identified Strengths</h3></div>
                            <ul>
                                {selectedAnalysis.strengths.map((s, i) => <li key={i}>{s}</li>)}
                            </ul>
                        </div>
                        
                        <div className="insight-card skills-card">
                            <div className="card-header"><Star color="#3b82f6" /> <h3>Extracted Skills ({selectedAnalysis.skillsFound.length})</h3></div>
                            <div className="tag-cloud">
                                {selectedAnalysis.skillsFound.length === 0 ? <p className="empty-text">No hard skills extracted.</p> :
                                    selectedAnalysis.skillsFound.map(s => <span key={s} className="tag tag-blue">{s}</span>)
                                }
                            </div>
                        </div>
                    </div>

                    <div className="suggestions-section mt-6">
                        <h3><Target size={20} /> Actionable AI Suggestions</h3>
                        <div className="suggestions-list">
                            {selectedAnalysis.suggestions.length === 0 ? (
                                <div className="empty-state-card mt-2"><CheckCircle size={32} color="#10b981" /> <p>Resume is in fantastic shape. No actionable improvements found.</p></div>
                            ) : (
                                selectedAnalysis.suggestions.map((s, i) => (
                                    <div key={i} className={`suggestion-item ${s.priority.toLowerCase()}`}>
                                        <div className="sugg-icon">
                                            {s.priority === 'Critical' ? <AlertTriangle size={20} /> : <AlertCircle size={20} />}
                                        </div>
                                        <div className="sugg-content">
                                            <div className="sugg-meta">
                                                <span className={`priority-badge ${s.priority.toLowerCase()}`}>{s.priority}</span>
                                                <span className="category-text">{s.category}</span>
                                            </div>
                                            <p>{s.text}</p>
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default ResumeIntelligence;
