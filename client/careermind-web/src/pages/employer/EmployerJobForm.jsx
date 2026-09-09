import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../../services/api';
import { LoadingState, ErrorState } from '../../components/StateComponents';

const EmployerJobForm = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const isEdit = Boolean(id);

    const [loading, setLoading] = useState(isEdit);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState(null);

    const [formData, setFormData] = useState({
        jobCategoryId: '00000000-0000-0000-0000-000000000000', // default empty GUID for now
        title: '',
        description: '',
        responsibilities: '',
        requirements: '',
        employmentType: 'Full-time',
        workMode: 'On-site',
        experienceLevel: 'Entry',
        minimumSalary: '',
        maximumSalary: '',
        salaryCurrency: 'USD',
        location: '',
        isRemote: false,
        skills: []
    });

    useEffect(() => {
        if (isEdit) {
            const fetchJob = async () => {
                try {
                    const response = await api.get(`/Employers/me/jobs/${id}`);
                    setFormData({
                        ...response.data,
                        minimumSalary: response.data.minimumSalary || '',
                        maximumSalary: response.data.maximumSalary || '',
                    });
                } catch (err) {
                    setError('Failed to load job details.');
                } finally {
                    setLoading(false);
                }
            };
            fetchJob();
        }
    }, [id, isEdit]);

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setSubmitting(true);
        setError(null);
        
        try {
            const payload = {
                ...formData,
                minimumSalary: formData.minimumSalary ? Number(formData.minimumSalary) : null,
                maximumSalary: formData.maximumSalary ? Number(formData.maximumSalary) : null,
            };

            if (isEdit) {
                await api.put(`/Employers/me/jobs/${id}`, payload);
                alert('Job updated successfully');
            } else {
                await api.post(`/Employers/me/jobs`, payload);
                alert('Job created successfully');
            }
            navigate('/employer/jobs');
        } catch (err) {
            setError(err.response?.data?.message || 'Failed to save job.');
        } finally {
            setSubmitting(false);
        }
    };

    if (loading) return <LoadingState />;

    return (
        <div style={{ background: '#fff', padding: '30px', borderRadius: '8px' }}>
            <h2>{isEdit ? 'Edit Job' : 'Post a New Job'}</h2>
            
            {error && <ErrorState message={error} />}

            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '20px', marginTop: '20px' }}>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                    <label style={{ fontWeight: 'bold' }}>Job Title *</label>
                    <input name="title" value={formData.title} onChange={handleChange} required style={{ padding: '10px', borderRadius: '4px', border: '1px solid #ccc' }} />
                </div>
                
                <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                    <label style={{ fontWeight: 'bold' }}>Job Description *</label>
                    <textarea name="description" value={formData.description} onChange={handleChange} required rows={5} style={{ padding: '10px', borderRadius: '4px', border: '1px solid #ccc' }} />
                </div>

                <div style={{ display: 'flex', gap: '20px' }}>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', flex: 1 }}>
                        <label style={{ fontWeight: 'bold' }}>Employment Type</label>
                        <select name="employmentType" value={formData.employmentType} onChange={handleChange} style={{ padding: '10px', borderRadius: '4px', border: '1px solid #ccc' }}>
                            <option value="Full-time">Full-time</option>
                            <option value="Part-time">Part-time</option>
                            <option value="Contract">Contract</option>
                        </select>
                    </div>
                    
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', flex: 1 }}>
                        <label style={{ fontWeight: 'bold' }}>Work Mode</label>
                        <select name="workMode" value={formData.workMode} onChange={handleChange} style={{ padding: '10px', borderRadius: '4px', border: '1px solid #ccc' }}>
                            <option value="On-site">On-site</option>
                            <option value="Hybrid">Hybrid</option>
                            <option value="Remote">Remote</option>
                        </select>
                    </div>

                     <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', flex: 1 }}>
                        <label style={{ fontWeight: 'bold' }}>Experience Level</label>
                        <select name="experienceLevel" value={formData.experienceLevel} onChange={handleChange} style={{ padding: '10px', borderRadius: '4px', border: '1px solid #ccc' }}>
                            <option value="Entry">Entry Level</option>
                            <option value="Mid">Mid Level</option>
                            <option value="Senior">Senior Level</option>
                        </select>
                    </div>
                </div>

                <div style={{ display: 'flex', gap: '20px' }}>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', flex: 1 }}>
                        <label style={{ fontWeight: 'bold' }}>Location</label>
                        <input name="location" value={formData.location} onChange={handleChange} style={{ padding: '10px', borderRadius: '4px', border: '1px solid #ccc' }} />
                    </div>
                    
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px', flex: 1, marginTop: '25px' }}>
                        <input type="checkbox" name="isRemote" checked={formData.isRemote} onChange={handleChange} id="remoteCb" />
                        <label htmlFor="remoteCb" style={{ fontWeight: 'bold' }}>Is Fully Remote</label>
                    </div>
                </div>

                <div style={{ display: 'flex', gap: '20px' }}>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', flex: 1 }}>
                        <label style={{ fontWeight: 'bold' }}>Minimum Salary</label>
                        <input type="number" name="minimumSalary" value={formData.minimumSalary} onChange={handleChange} style={{ padding: '10px', borderRadius: '4px', border: '1px solid #ccc' }} />
                    </div>
                    
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', flex: 1 }}>
                        <label style={{ fontWeight: 'bold' }}>Maximum Salary</label>
                        <input type="number" name="maximumSalary" value={formData.maximumSalary} onChange={handleChange} style={{ padding: '10px', borderRadius: '4px', border: '1px solid #ccc' }} />
                    </div>
                </div>

                {isEdit && (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                        <label style={{ fontWeight: 'bold' }}>Job Status</label>
                        <select name="status" value={formData.status} onChange={handleChange} style={{ padding: '10px', borderRadius: '4px', border: '1px solid #ccc', width: '200px' }}>
                            <option value="Active">Active</option>
                            <option value="Closed">Closed</option>
                            <option value="Draft">Draft</option>
                        </select>
                    </div>
                )}

                <button type="submit" disabled={submitting} style={{ background: '#2c3e50', color: '#fff', padding: '12px', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '16px', fontWeight: 'bold', marginTop: '20px' }}>
                    {submitting ? 'Saving...' : 'Save Job'}
                </button>
            </form>
        </div>
    );
};

export default EmployerJobForm;
