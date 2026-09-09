import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../../services/api';
import { LoadingState, ErrorState } from '../../components/StateComponents';

const CandidateJobDetails = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    
    const [job, setJob] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [applying, setApplying] = useState(false);
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        const fetchDetails = async () => {
            try {
                const response = await api.get(`/Jobs/${id}`);
                setJob(response.data);
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
            </div>
        </div>
    );
};

export default CandidateJobDetails;
