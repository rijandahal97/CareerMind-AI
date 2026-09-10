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
                } catch (matchErr) {
                    console.error("Match details error:", matchErr);
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
            </div>
        </div>
    );
};

export default CandidateJobDetails;
