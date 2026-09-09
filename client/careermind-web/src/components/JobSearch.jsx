import React, { useState } from 'react';

const JobSearch = ({ onSearch }) => {
    const [keyword, setKeyword] = useState('');
    const [location, setLocation] = useState('');
    const [isRemote, setIsRemote] = useState(false);
    const [employmentType, setEmploymentType] = useState('');
    const [experienceLevel, setExperienceLevel] = useState('');

    const handleSearch = (e) => {
        e.preventDefault();
        onSearch({ keyword, location, isRemote, employmentType, experienceLevel });
    };

    return (
        <div style={{ background: '#f5f5f5', padding: '20px', borderRadius: '8px', marginBottom: '20px' }}>
            <form onSubmit={handleSearch} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
                <div style={{ display: 'flex', gap: '16px' }}>
                    <input 
                        type="text" 
                        placeholder="Job title, keywords, or company..." 
                        value={keyword}
                        onChange={(e) => setKeyword(e.target.value)}
                        style={{ flex: 1, padding: '12px', borderRadius: '4px', border: '1px solid #ccc' }}
                    />
                    <input 
                        type="text" 
                        placeholder="Location..." 
                        value={location}
                        onChange={(e) => setLocation(e.target.value)}
                        style={{ padding: '12px', borderRadius: '4px', border: '1px solid #ccc' }}
                    />
                    <button type="submit" style={{ padding: '12px 24px', background: '#0066cc', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                        Search
                    </button>
                </div>
                
                <div style={{ display: 'flex', gap: '16px', alignItems: 'center', fontSize: '14px' }}>
                    <label>
                        <input type="checkbox" checked={isRemote} onChange={(e) => setIsRemote(e.target.checked)} /> Remote Only
                    </label>
                    <select value={employmentType} onChange={(e) => setEmploymentType(e.target.value)} style={{ padding: '8px', borderRadius: '4px' }}>
                        <option value="">Any Employment Type</option>
                        <option value="Full-time">Full-time</option>
                        <option value="Part-time">Part-time</option>
                        <option value="Contract">Contract</option>
                    </select>
                    <select value={experienceLevel} onChange={(e) => setExperienceLevel(e.target.value)} style={{ padding: '8px', borderRadius: '4px' }}>
                        <option value="">Any Experience Level</option>
                        <option value="Entry">Entry Level</option>
                        <option value="Mid">Mid Level</option>
                        <option value="Senior">Senior Level</option>
                    </select>
                </div>
            </form>
        </div>
    );
};

export default JobSearch;
