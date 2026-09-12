import React, { useState, useEffect } from 'react';
import { Routes, Route, Link, useLocation } from 'react-router-dom';
import { candidateService } from '../../services/candidate';
import { useAuth } from '../../context/AuthContext';
import './Candidate.css';
import CandidateJobs from './CandidateJobs';
import CandidateJobDetails from './CandidateJobDetails';
import CandidateApplications from './CandidateApplications';
import CandidateSavedJobs from './CandidateSavedJobs';
import CareerIntelligence from './CareerIntelligence';

const ProfileSection = ({ initialData, fetchProfile }) => {
    const [formData, setFormData] = useState(initialData || {});
    const [loading, setLoading] = useState(false);
    const [msg, setMsg] = useState(null);

    useEffect(() => { setFormData(initialData || {}) }, [initialData]);

    const handleSave = async (e) => {
        e.preventDefault();
        setLoading(true);
        try {
            await candidateService.updateProfile(formData);
            setMsg({ type: 'success', text: 'Profile updated successfully!' });
            fetchProfile();
        } catch (err) {
            setMsg({ type: 'error', text: 'Failed to update profile.' });
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="candidate-section animated-fade">
            <h2>Personal & Professional Info</h2>
            {msg && <div className={`alert ${msg.type}`}>{msg.text}</div>}
            <form onSubmit={handleSave}>
                <div className="form-group">
                    <label>Headline</label>
                    <input type="text" value={formData.headline || ''} onChange={e => setFormData({...formData, headline: e.target.value})} placeholder="e.g. Senior Software Engineer" />
                </div>
                <div className="form-group">
                    <label>Bio</label>
                    <textarea rows="4" value={formData.bio || ''} onChange={e => setFormData({...formData, bio: e.target.value})}></textarea>
                </div>
                <div className="form-row">
                    <div className="form-group half">
                        <label>Current Job Title</label>
                        <input type="text" value={formData.currentJobTitle || ''} onChange={e => setFormData({...formData, currentJobTitle: e.target.value})} />
                    </div>
                    <div className="form-group half">
                        <label>Current Company</label>
                        <input type="text" value={formData.currentCompany || ''} onChange={e => setFormData({...formData, currentCompany: e.target.value})} />
                    </div>
                </div>
                <div className="form-row">
                    <div className="form-group half">
                        <label>Years of Experience</label>
                        <input type="number" value={formData.yearsOfExperience || ''} onChange={e => setFormData({...formData, yearsOfExperience: parseInt(e.target.value)})} />
                    </div>
                    <div className="form-group half">
                        <label>Availability Status</label>
                        <select value={formData.availabilityStatus || ''} onChange={e => setFormData({...formData, availabilityStatus: e.target.value})}>
                            <option value="">Select Status</option>
                            <option value="Actively Looking">Actively Looking</option>
                            <option value="Open to Offers">Open to Offers</option>
                            <option value="Not Looking">Not Looking</option>
                        </select>
                    </div>
                </div>
                <button className="btn-primary" type="submit" disabled={loading}>
                    {loading ? 'Saving...' : 'Save Profile'}
                </button>
            </form>
        </div>
    );
};

const SkillsSection = ({ skills, fetchProfile }) => {
    const [query, setQuery] = useState('');
    const [available, setAvailable] = useState([]);
    
    // Add Skill form
    const [selectedSkillId, setSelectedSkillId] = useState('');
    const [proficiency, setProficiency] = useState(2);
    const [years, setYears] = useState(1);
    
    useEffect(() => {
        if(query.length > 1) {
            candidateService.getAvailableSkills(query).then(res => setAvailable(res.data)).catch(console.error);
        } else {
            setAvailable([]);
        }
    }, [query]);

    const handleAdd = async (e) => {
        e.preventDefault();
        try {
            await candidateService.addSkill({ skillId: selectedSkillId, proficiencyLevel: parseInt(proficiency), yearsOfExperience: parseInt(years), isPrimary: true });
            fetchProfile();
            setSelectedSkillId('');
            setQuery('');
        } catch (err) {
            alert('Error adding skill');
        }
    };
    
    const handleRemove = async (id) => {
        if(window.confirm('Are you sure?')) {
            await candidateService.removeSkill(id);
            fetchProfile();
        }
    };

    return (
        <div className="candidate-section animated-fade">
            <h2>Skills</h2>
            
            <div className="skills-list">
                {skills && skills.length === 0 ? <p className="empty-state">No skills added yet.</p> :
                  skills.map(s => (
                    <div key={s.id} className="skill-card">
                        <div>
                            <strong>{s.skillName}</strong>
                            <span className="badge">Lvl {s.proficiencyLevel}</span>
                            <span className="badge subtle">{s.yearsOfExperience} yrs</span>
                        </div>
                        <button className="btn-danger-small" onClick={() => handleRemove(s.id)}>Remove</button>
                    </div>
                  ))
                }
            </div>

            <div className="card-box mt-20">
                <h3>Add a Skill</h3>
                <form onSubmit={handleAdd} className="form-row align-end">
                    <div className="form-group search-container">
                        <label>Search Skill</label>
                        <input type="text" value={query} onChange={e => setQuery(e.target.value)} placeholder="Type to search..." />
                        {available.length > 0 && query && !selectedSkillId && (
                            <ul className="dropdown">
                                {available.map(a => (
                                    <li key={a.id} onClick={() => { setSelectedSkillId(a.id); setQuery(a.name); }}>{a.name}</li>
                                ))}
                            </ul>
                        )}
                    </div>
                    <div className="form-group small">
                        <label>Proficiency (1-4)</label>
                        <input type="number" min="1" max="4" value={proficiency} onChange={e => setProficiency(e.target.value)} />
                    </div>
                    <div className="form-group small">
                        <label>Years Exp.</label>
                        <input type="number" min="0" value={years} onChange={e => setYears(e.target.value)} />
                    </div>
                    <button className="btn-primary" type="submit" disabled={!selectedSkillId}>Add</button>
                </form>
            </div>
        </div>
    );
};

const EducationSection = ({ educations, fetchProfile }) => {
    // Basic structural implementation similar to skills
    return (
        <div className="candidate-section animated-fade">
            <h2>Education</h2>
            <div className="list-wrapper">
                {educations && educations.length === 0 ? <p className="empty-state">No education added.</p> :
                    educations.map(e => (
                        <div key={e.id} className="list-card">
                            <h3>{e.degree} in {e.fieldOfStudy}</h3>
                            <p>{e.institution}</p>
                            <small>{new Date(e.startDate).getFullYear()} - {e.endDate ? new Date(e.endDate).getFullYear() : 'Present'}</small>
                        </div>
                    ))
                }
                <button className="btn-secondary mt-10">Add Education</button>
            </div>
        </div>
    );
};

const CandidateDashboard = () => {
    const { user } = useAuth();
    const location = useLocation();
    const [profile, setProfile] = useState(null);
    const [loading, setLoading] = useState(true);
    
    const fetchProfile = async () => {
        try {
            const res = await candidateService.getProfile();
            setProfile(res.data);
        } catch (err) {
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchProfile();
    }, []);

    const navItems = [
        { path: '', label: 'Dashboard Overview' },
        { path: 'profile', label: 'Professional Profile' },
        { path: 'skills', label: 'Skills' },
        { path: 'education', label: 'Education' },
        { path: 'experience', label: 'Experience' },
        { path: 'jobs', label: 'Find Jobs' },
        { path: 'applications', label: 'My Applications' },
        { path: 'saved-jobs', label: 'Saved Jobs' },
        { path: 'career-intelligence', label: '🧠 Career Intelligence' },
    ];

    if (loading) return <div className="loader">Loading...</div>;

    const currentPath = location.pathname.split('/').pop();
    const activeNav = currentPath === 'candidate' ? '' : currentPath;

    return (
        <div className="candidate-layout">
            <aside className="candidate-sidebar">
                <div className="sidebar-header">
                    <h2>CareerMind</h2>
                    <p className="welcome-text">Hi, {user?.firstName}</p>
                    {profile && (
                        <div className="completion-bar-container">
                            <div className="completion-bar">
                                <div className="completion-fill" style={{ width: `${profile.completionPercentage}%`}}></div>
                            </div>
                            <small>{profile.completionPercentage}% Complete</small>
                        </div>
                    )}
                </div>
                <nav>
                    {navItems.map(item => (
                        <Link 
                            key={item.path} 
                            to={`/candidate/${item.path}`} 
                            className={`nav-link ${activeNav === item.path ? 'active' : ''}`}
                        >
                            {item.label}
                        </Link>
                    ))}
                </nav>
            </aside>
            <main className="candidate-main">
                <div className="glass-panel">
                    <Routes>
                        <Route path="/" element={<div className="animated-fade"><h1>Welcome to your Dashboard</h1><p>Start completing your profile to unlock AI matching!</p></div>} />
                        <Route path="profile" element={<ProfileSection initialData={profile} fetchProfile={fetchProfile} />} />
                        <Route path="skills" element={<SkillsSection skills={profile?.skills || []} fetchProfile={fetchProfile} />} />
                        <Route path="education" element={<EducationSection educations={profile?.educations || []} fetchProfile={fetchProfile} />} />
                        <Route path="experience" element={<h2>Experience Section (Coming Soon)</h2>} />
                        <Route path="jobs" element={<CandidateJobs />} />
                        <Route path="jobs/:id" element={<CandidateJobDetails />} />
                        <Route path="applications" element={<CandidateApplications />} />
                        <Route path="saved-jobs" element={<CandidateSavedJobs />} />
                        <Route path="career-intelligence" element={<CareerIntelligence />} />
                    </Routes>
                </div>
            </main>
        </div>
    );
};

export default CandidateDashboard;
