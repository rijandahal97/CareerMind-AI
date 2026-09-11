import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../../services/api';
import { candidateService } from '../../services/candidate';
import { LoadingState, ErrorState } from '../../components/StateComponents';

const CandidateJobDetails = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    
    const [job, setJob] = useState(null);
    const [matchDetails, setMatchDetails] = useState(null);
    const [gapDetails, setGapDetails] = useState(null);
    const [loading, setLoading] = useState(true);
    const [loadingMatch, setLoadingMatch] = useState(false);
    const [error, setError] = useState(null);
    const [applying, setApplying] = useState(false);
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        const fetchDetails = async () => {
            try {
                const response = await api.get(`/Jobs/${id}`);
                setJob(response.data);
                
                // Fetch match details
                try {
                    setLoadingMatch(true);
                    const matchRes = await candidateService.getJobMatchDetails(id);
                    setMatchDetails(matchRes.data);
                    
                    const gapRes = await candidateService.getCareerGapAnalysis(id);
                    setGapDetails(gapRes.data);
                } catch (matchErr) {
                    console.error("Match/Gap details error:", matchErr);
                } finally {
                    setLoadingMatch(false);
                }
                
            } catch (err) {
                setError('Failed to load job details.');
            } finally {
                setLoading(false);
            }
        };
        fetchDetails();
    }, [id]);

    const handleApply = async () => {
        setApplying(true);
        try {
            await api.post(`/Jobs/${id}/apply`, { coverLetter: '' });
            alert('Application submitted successfully!');
            navigate('/candidate/applications');
        } catch (err) {
            alert(err.response?.data?.message || 'Failed to apply.');
        } finally {
            setApplying(false);
        }
    };

    const handleSave = async () => {
        setSaving(true);
        try {
            await api.post(`/Jobs/${id}/save`);
            alert('Job saved successfully!');
        } catch (err) {
            alert(err.response?.data?.message || 'Failed to save job.');
        } finally {
            setSaving(false);
        }
    };

    if (loading) return <LoadingState />;
    if (error) return <ErrorState message={error} />;
    if (!job) return null;

    return (
        <div style={{ background: '#fff', padding: '24px', borderRadius: '8px', border: '1px solid #ddd' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <div>
                    <h1 style={{ margin: '0 0 8px 0' }}>{job.title}</h1>
                    <h3 style={{ margin: '0 0 16px 0', color: '#555' }}>{job.companyName} - {job.location}</h3>
                    <p style={{ margin: '0 0 16px 0', color: '#666' }}>
                        {job.employmentType} • {job.workMode} • {job.experienceLevel}
                    </p>
                </div>
                {job.companyLogoUrl && (
                    <img src={job.companyLogoUrl} alt="Logo" style={{ width: '80px' }} />
                )}
            </div>

            <div style={{ margin: '24px 0', display: 'flex', gap: '12px' }}>
                <button onClick={handleApply} disabled={applying} style={{ padding: '12px 24px', background: '#0066cc', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}>
                    {applying ? 'Applying...' : 'Apply Now'}
                </button>
                <button onClick={handleSave} disabled={saving} style={{ padding: '12px 24px', background: '#eee', color: '#333', border: '1px solid #ccc', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}>
                    {saving ? 'Saving...' : 'Save Job'}
                </button>
            </div>

            <div style={{ marginTop: '24px' }}>
                <h3>Description</h3>
                <p style={{ whiteSpace: 'pre-wrap' }}>{job.description}</p>
                
                {job.responsibilities && (
                    <>
                        <h3>Responsibilities</h3>
                        <p style={{ whiteSpace: 'pre-wrap' }}>{job.responsibilities}</p>
                    </>
                )}

                {job.requirements && (
                    <>
                        <h3>Requirements</h3>
                        <p style={{ whiteSpace: 'pre-wrap' }}>{job.requirements}</p>
                    </>
                )}

                {job.skills && job.skills.length > 0 && (
                    <>
                        <h3>Required Skills</h3>
                        <ul style={{ paddingLeft: '20px' }}>
                            {job.skills.map(s => (
                                <li key={s.skillId}>{s.skillName} {s.isRequired ? '(Required)' : ''}</li>
                            ))}
                        </ul>
                    </>
                )}

                {/* AI Match Analysis */}
                {loadingMatch ? (
                    <div style={{ marginTop: '30px' }}><p>Loading AI Match Analysis...</p></div>
                ) : matchDetails ? (
                    <div style={{ marginTop: '30px', background: '#f5f7fa', padding: '20px', borderRadius: '8px', border: '1px solid #d2d6dc' }}>
                        <h2 style={{ margin: '0 0 16px 0', color: '#1f2937' }}>CareerMind AI Match Analysis</h2>
                        
                        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '16px', marginBottom: '20px' }}>
                            <div style={{ background: '#fff', padding: '16px', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', textAlign: 'center' }}>
                                <h4 style={{ margin: '0 0 8px 0', color: '#6b7280' }}>Overall Match</h4>
                                <div style={{ fontSize: '32px', fontWeight: 'bold', color: matchDetails.overallMatchScore >= 80 ? '#10b981' : matchDetails.overallMatchScore >= 50 ? '#f59e0b' : '#ef4444' }}>
                                    {Math.round(matchDetails.overallMatchScore)}%
                                </div>
                            </div>
                            <div style={{ background: '#fff', padding: '16px', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)' }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
                                    <span style={{ color: '#4b5563' }}>Skill Match:</span>
                                    <strong style={{ color: '#10b981' }}>{Math.round(matchDetails.skillMatchScore)}%</strong>
                                </div>
                                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
                                    <span style={{ color: '#4b5563' }}>Experience Match:</span>
                                    <strong style={{ color: '#10b981' }}>{Math.round(matchDetails.experienceMatchScore)}%</strong>
                                </div>
                                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
                                    <span style={{ color: '#4b5563' }}>Education Match:</span>
                                    <strong style={{ color: '#10b981' }}>{Math.round(matchDetails.educationMatchScore)}%</strong>
                                </div>
                                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                                    <span style={{ color: '#4b5563' }}>Preference Match:</span>
                                    <strong style={{ color: '#10b981' }}>{Math.round(matchDetails.preferenceMatchScore)}%</strong>
                                </div>
                            </div>
                        </div>

                        {matchDetails.explanations && matchDetails.explanations.length > 0 && (
                            <div style={{ marginBottom: '20px' }}>
                                <h4 style={{ margin: '0 0 8px 0', color: '#374151' }}>Summary</h4>
                                <ul style={{ margin: 0, paddingLeft: '20px', color: '#4b5563' }}>
                                    {matchDetails.explanations.map((exp, idx) => (
                                        <li key={idx} style={{ marginBottom: '4px' }}>{exp}</li>
                                    ))}
                                </ul>
                            </div>
                        )}

                        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '20px' }}>
                            <div>
                                <h4 style={{ margin: '0 0 12px 0', color: '#059669' }}>Top Strengths</h4>
                                {matchDetails.strengths && matchDetails.strengths.length > 0 ? (
                                    <ul style={{ paddingLeft: '20px', color: '#4b5563' }}>
                                        {matchDetails.strengths.map((str, i) => <li key={i}>{str}</li>)}
                                    </ul>
                                ) : <p style={{ color: '#9ca3af', fontStyle: 'italic' }}>No particular strengths identified.</p>}

                                <h4 style={{ margin: '16px 0 12px 0', color: '#059669' }}>Matched Skills</h4>
                                {matchDetails.matchedSkills && matchDetails.matchedSkills.length > 0 ? (
                                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
                                        {matchDetails.matchedSkills.map((skill, i) => (
                                            <span key={i} style={{ background: '#d1fae5', color: '#065f46', padding: '4px 10px', borderRadius: '16px', fontSize: '12px', fontWeight: '500' }}>
                                                {skill} ✓
                                            </span>
                                        ))}
                                    </div>
                                ) : <p style={{ color: '#9ca3af', fontStyle: 'italic' }}>No matching skills found.</p>}
                            </div>
                            
                            <div>
                                <h4 style={{ margin: '0 0 12px 0', color: '#dc2626' }}>Skill Gaps & Missing Requirements</h4>
                                
                                {matchDetails.gaps && matchDetails.gaps.length > 0 && (
                                    <ul style={{ paddingLeft: '20px', color: '#b91c1c', marginBottom: '16px' }}>
                                        {matchDetails.gaps.map((gap, i) => <li key={i}>{gap}</li>)}
                                    </ul>
                                )}
                                
                                <h4 style={{ margin: '0 0 12px 0', color: '#dc2626' }}>Missing Skills</h4>
                                {matchDetails.missingSkills && matchDetails.missingSkills.length > 0 ? (
                                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
                                        {matchDetails.missingSkills.map((ms, i) => (
                                            <span key={i} style={{ background: '#fee2e2', color: '#991b1b', padding: '4px 10px', borderRadius: '16px', fontSize: '12px', fontWeight: '500' }}>
                                                {ms.skillName} (Req: {ms.requiredProficiency}) ✗
                                            </span>
                                        ))}
                                    </div>
                                ) : <p style={{ color: '#9ca3af', fontStyle: 'italic' }}>No missing skills!</p>}
                            </div>
                        </div>
                    </div>
                ) : null}

                {/* Career Gap Analysis */}
                {loadingMatch ? null : gapDetails ? (
                    <div style={{ marginTop: '30px', background: '#fff', padding: '24px', borderRadius: '8px', border: '1px solid #e5e7eb' }}>
                        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '24px' }}>
                            <h2 style={{ margin: 0, color: '#111827' }}>Personalized Career Gap Analysis</h2>
                            <div style={{ background: '#f8fafc', padding: '12px 20px', borderRadius: '8px', border: '1px solid #e2e8f0', textAlign: 'center' }}>
                                <span style={{ display: 'block', fontSize: '14px', color: '#64748b', fontWeight: '500', marginBottom: '4px' }}>Career Readiness</span>
                                <span style={{ display: 'block', fontSize: '28px', color: gapDetails.currentReadinessScore >= 80 ? '#10b981' : gapDetails.currentReadinessScore >= 50 ? '#f59e0b' : '#ef4444', fontWeight: 'bold' }}>
                                    {Math.round(gapDetails.currentReadinessScore)}%
                                </span>
                            </div>
                        </div>

                        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: '20px', marginBottom: '32px' }}>
                            <div style={{ background: '#f0fdf4', border: '1px solid #bbf7d0', padding: '16px', borderRadius: '8px' }}>
                                <h4 style={{ margin: '0 0 12px 0', color: '#15803d' }}>Strong Skills</h4>
                                {gapDetails.strongSkills && gapDetails.strongSkills.length > 0 ? (
                                    <ul style={{ margin: 0, paddingLeft: '0', listStyle: 'none' }}>
                                        {gapDetails.strongSkills.map((s, i) => <li key={i} style={{ color: '#166534', marginBottom: '4px' }}>✓ {s}</li>)}
                                    </ul>
                                ) : <p style={{ margin: 0, color: '#166534', fontStyle: 'italic', fontSize: '14px' }}>No strong skills listed.</p>}
                            </div>
                            <div style={{ background: '#fffbeb', border: '1px solid #fde68a', padding: '16px', borderRadius: '8px' }}>
                                <h4 style={{ margin: '0 0 12px 0', color: '#b45309' }}>Developing Skills</h4>
                                {gapDetails.developingSkills && gapDetails.developingSkills.length > 0 ? (
                                    <ul style={{ margin: 0, paddingLeft: '0', listStyle: 'none' }}>
                                        {gapDetails.developingSkills.map((s, i) => <li key={i} style={{ color: '#92400e', marginBottom: '4px' }}>⚠ {s}</li>)}
                                    </ul>
                                ) : <p style={{ margin: 0, color: '#92400e', fontStyle: 'italic', fontSize: '14px' }}>No developing skills listed.</p>}
                            </div>
                            <div style={{ background: '#fef2f2', border: '1px solid #fecaca', padding: '16px', borderRadius: '8px' }}>
                                <h4 style={{ margin: '0 0 12px 0', color: '#b91c1c' }}>Missing Skills</h4>
                                {gapDetails.missingSkills && gapDetails.missingSkills.length > 0 ? (
                                    <ul style={{ margin: 0, paddingLeft: '0', listStyle: 'none' }}>
                                        {gapDetails.missingSkills.map((s, i) => <li key={i} style={{ color: '#991b1b', marginBottom: '4px' }}>✕ {s}</li>)}
                                    </ul>
                                ) : <p style={{ margin: 0, color: '#991b1b', fontStyle: 'italic', fontSize: '14px' }}>No missing skills listed.</p>}
                            </div>
                        </div>

                        {gapDetails.careerInsights && gapDetails.careerInsights.length > 0 && (
                            <div style={{ marginBottom: '32px' }}>
                                <h3 style={{ borderBottom: '2px solid #e5e7eb', paddingBottom: '8px', marginBottom: '16px', color: '#374151' }}>Personalized Insights</h3>
                                <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
                                    {gapDetails.careerInsights.map((insight, idx) => (
                                        <div key={idx} style={{ display: 'flex', alignItems: 'center', gap: '12px', background: '#f9fafb', padding: '12px 16px', borderRadius: '8px', borderLeft: '4px solid #6366f1' }}>
                                            <span style={{ fontSize: '20px' }}>💡</span>
                                            <span style={{ color: '#4b5563', fontSize: '15px' }}>{insight}</span>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}

                        {gapDetails.roadmap && gapDetails.roadmap.length > 0 && (
                            <div>
                                <h3 style={{ borderBottom: '2px solid #e5e7eb', paddingBottom: '8px', marginBottom: '16px', color: '#374151' }}>Personalized Skill Roadmap</h3>
                                <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
                                    {gapDetails.roadmap.map((step, idx) => (
                                        <div key={idx} style={{ display: 'flex', background: '#fff', border: '1px solid #e5e7eb', borderRadius: '8px', overflow: 'hidden' }}>
                                            <div style={{ background: '#4f46e5', color: '#fff', padding: '16px 24px', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '24px', fontWeight: 'bold' }}>
                                                {step.stepNumber < 10 ? `0${step.stepNumber}` : step.stepNumber}
                                            </div>
                                            <div style={{ padding: '16px', flex: 1 }}>
                                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '8px' }}>
                                                    <h4 style={{ margin: 0, fontSize: '18px', color: '#111827' }}>{step.skillName}</h4>
                                                    <span style={{ background: step.priority === 'High' ? '#fee2e2' : step.priority === 'Medium' ? '#fef3c7' : '#f3f4f6', color: step.priority === 'High' ? '#991b1b' : step.priority === 'Medium' ? '#92400e' : '#4b5563', padding: '4px 12px', borderRadius: '12px', fontSize: '12px', fontWeight: 'bold' }}>
                                                        {step.priority.toUpperCase()} PRIORITY
                                                    </span>
                                                </div>
                                                <div style={{ display: 'flex', gap: '24px', marginBottom: '12px', fontSize: '14px' }}>
                                                    <div><span style={{ color: '#6b7280' }}>Current:</span> <span style={{ fontWeight: '500', color: '#374151' }}>{step.currentLevel}</span></div>
                                                    <div><span style={{ color: '#6b7280' }}>Target:</span> <span style={{ fontWeight: '500', color: '#374151' }}>{step.targetLevel}</span></div>
                                                </div>
                                                <p style={{ margin: 0, color: '#4b5563', fontSize: '14px', fontStyle: 'italic' }}>Why: "{step.reason}"</p>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>
                ) : null}
            </div>
        </div>
    );
};

export default CandidateJobDetails;
