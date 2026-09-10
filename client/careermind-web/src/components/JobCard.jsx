import React from 'react';
import { Link } from 'react-router-dom';

const JobCard = ({ job, isEmployer = false }) => {
    return (
        <div style={{ border: '1px solid #ccc', borderRadius: '8px', padding: '16px', margin: '16px 0', background: '#fff' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <div>
                    <h3 style={{ margin: '0 0 8px 0', color: '#333' }}>{job.title}</h3>
                    <p style={{ margin: '0 0 4px 0', color: '#666', fontWeight: 'bold' }}>{job.companyName}</p>
                    <p style={{ margin: '0 0 12px 0', color: '#888', fontSize: '14px' }}>
                        {[job.location, job.workMode, job.employmentType].filter(Boolean).join(' • ')}
                    </p>
                    {job.overallMatchScore !== undefined && (
                        <div style={{ marginBottom: '12px', display: 'inline-block', background: job.overallMatchScore > 80 ? '#e8f5e9' : job.overallMatchScore > 50 ? '#fff3e0' : '#ffebee', padding: '4px 12px', borderRadius: '4px', border: `1px solid ${job.overallMatchScore > 80 ? '#4caf50' : job.overallMatchScore > 50 ? '#ff9800' : '#f44336'}` }}>
                            <strong style={{ color: job.overallMatchScore > 80 ? '#2e7d32' : job.overallMatchScore > 50 ? '#ef6c00' : '#c62828' }}>
                                {Math.round(job.overallMatchScore)}% Match - {job.overallMatchScore > 80 ? 'Strong Match' : job.overallMatchScore > 50 ? 'Moderate Match' : 'Weak Match'}
                            </strong>
                        </div>
                    )}
                </div>
                {job.companyLogoUrl && (
                    <img src={job.companyLogoUrl} alt={`${job.companyName} logo`} style={{ width: '50px', height: '50px', objectFit: 'contain' }} />
                )}
            </div>
            
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px', marginBottom: '16px' }}>
                {job.skills && job.skills.map(s => (
                    <span key={s.skillId} style={{ background: '#eef', color: '#44a', padding: '4px 8px', borderRadius: '16px', fontSize: '12px' }}>
                        {s.skillName}
                    </span>
                ))}
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontSize: '14px', color: '#666' }}>
                    {job.minimumSalary ? `$${job.minimumSalary.toLocaleString()}` : ''} {job.maximumSalary ? `- $${job.maximumSalary.toLocaleString()}` : ''} {job.salaryCurrency}
                </span>
                
                {isEmployer ? (
                    <div>
                        <Link to={`/employer/jobs/${job.id}/edit`} style={{ marginRight: '8px', textDecoration: 'none', color: '#0066cc' }}>Edit</Link>
                        <Link to={`/employer/jobs/${job.id}/applications`} style={{ textDecoration: 'none', color: '#0066cc' }}>Applications</Link>
                    </div>
                ) : (
                    <Link to={`/candidate/jobs/${job.id}`} style={{ background: '#0066cc', color: '#fff', padding: '8px 16px', borderRadius: '4px', textDecoration: 'none' }}>
                        View Details
                    </Link>
                )}
            </div>
        </div>
    );
};

export default JobCard;
