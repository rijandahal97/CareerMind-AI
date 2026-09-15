import React, { useState, useEffect } from 'react';
import { Routes, Route, Link, useLocation, useNavigate } from 'react-router-dom';
import { candidateService } from '../../services/candidate';
import { useAuth } from '../../context/AuthContext';
import './Candidate.css';

// Components
import DashboardOverview from './DashboardOverview';
import CandidateJobs from './CandidateJobs';
import CandidateJobDetails from './CandidateJobDetails';
import CandidateApplications from './CandidateApplications';
import CandidateSavedJobs from './CandidateSavedJobs';
import CareerIntelligence from './CareerIntelligence';
import ResumeIntelligence from './ResumeIntelligence';
import CareerCommandCenter from './CareerCommandCenter';

// Icons
import {
    LayoutDashboard, User, Briefcase, BookOpen, Star, FileText, Bookmark, 
    Lightbulb, Map, FileStack, TrendingUp, Settings, LogOut, Bell, Search, Menu, X, ChevronDown, Bot, Target
} from 'lucide-react';

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
            <div className="section-header-simple">
                <h2>Personal & Professional Info</h2>
                <p>Complete your profile to get better job recommendations.</p>
            </div>
            {msg && <div className={`alert ${msg.type}`}>{msg.text}</div>}
            <form onSubmit={handleSave} className="form-card">
                <div className="form-group">
                    <label>Headline</label>
                    <input type="text" className="input-premium" value={formData.headline || ''} onChange={e => setFormData({...formData, headline: e.target.value})} placeholder="e.g. Senior Software Engineer" />
                </div>
                <div className="form-group">
                    <label>Bio</label>
                    <textarea rows="4" className="input-premium" value={formData.bio || ''} onChange={e => setFormData({...formData, bio: e.target.value})} placeholder="Write a short bio..."></textarea>
                </div>
                <div className="form-row">
                    <div className="form-group half">
                        <label>Current Job Title</label>
                        <input type="text" className="input-premium" value={formData.currentJobTitle || ''} onChange={e => setFormData({...formData, currentJobTitle: e.target.value})} />
                    </div>
                    <div className="form-group half">
                        <label>Current Company</label>
                        <input type="text" className="input-premium" value={formData.currentCompany || ''} onChange={e => setFormData({...formData, currentCompany: e.target.value})} />
                    </div>
                </div>
                <div className="form-row">
                    <div className="form-group half">
                        <label>Years of Experience</label>
                        <input type="number" className="input-premium" value={formData.yearsOfExperience || ''} onChange={e => setFormData({...formData, yearsOfExperience: parseInt(e.target.value) || 0})} />
                    </div>
                    <div className="form-group half">
                        <label>Availability Status</label>
                        <select className="select-premium" value={formData.availabilityStatus || ''} onChange={e => setFormData({...formData, availabilityStatus: e.target.value})}>
                            <option value="">Select Status</option>
                            <option value="Actively Looking">Actively Looking</option>
                            <option value="Open to Offers">Open to Offers</option>
                            <option value="Not Looking">Not Looking</option>
                        </select>
                    </div>
                </div>
                <div className="form-actions mt-4">
                    <button className="btn-primary" type="submit" disabled={loading}>
                        {loading ? 'Saving...' : 'Save Profile'}
                    </button>
                </div>
            </form>
        </div>
    );
};

const SkillsSection = ({ skills, fetchProfile }) => {
    const [query, setQuery] = useState('');
    const [available, setAvailable] = useState([]);
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
            <div className="section-header-simple">
                <h2>Skills Management</h2>
                <p>Add technical and soft skills to improve AI matching.</p>
            </div>
            
            <div className="skills-grid">
                {skills && skills.length === 0 ? <div className="empty-state-card w-100"><Star size={32} /> <p>No skills added yet.</p></div> :
                  skills.map(s => (
                    <div key={s.id} className="skill-card-premium">
                        <div className="skill-info">
                            <strong>{s.skillName}</strong>
                            <div className="skill-badges">
                                <span className={`badge-lvl lvl-${s.proficiencyLevel}`}>Lvl {s.proficiencyLevel}</span>
                                <span className="badge-exp">{s.yearsOfExperience} yrs</span>
                            </div>
                        </div>
                        <button className="btn-icon-danger" onClick={() => handleRemove(s.id)} aria-label="Remove"><X size={16} /></button>
                    </div>
                  ))
                }
            </div>

            <div className="form-card mt-6">
                <h3>Add a New Skill</h3>
                <form onSubmit={handleAdd} className="form-row align-end">
                    <div className="form-group search-container">
                        <label>Search Skill</label>
                        <div className="input-with-icon">
                            <Search size={16} />
                            <input type="text" className="input-premium pl-8" value={query} onChange={e => setQuery(e.target.value)} placeholder="Type to search..." />
                        </div>
                        {available.length > 0 && query && !selectedSkillId && (
                            <ul className="dropdown-premium">
                                {available.map(a => (
                                    <li key={a.id} onClick={() => { setSelectedSkillId(a.id); setQuery(a.name); }}>{a.name}</li>
                                ))}
                            </ul>
                        )}
                    </div>
                    <div className="form-group small">
                        <label>Level (1-4)</label>
                        <input type="number" className="input-premium" min="1" max="4" value={proficiency} onChange={e => setProficiency(e.target.value)} />
                    </div>
                    <div className="form-group small">
                        <label>Years Exp.</label>
                        <input type="number" className="input-premium" min="0" value={years} onChange={e => setYears(e.target.value)} />
                    </div>
                    <button className="btn-primary ml-2" type="submit" disabled={!selectedSkillId}>Add</button>
                </form>
            </div>
        </div>
    );
};

const EducationSection = ({ educations, fetchProfile }) => {
    return (
        <div className="candidate-section animated-fade">
            <div className="section-header-simple">
                <h2>Education History</h2>
                <p>List your academic achievements.</p>
            </div>
            <div className="education-list">
                {educations && educations.length === 0 ? <div className="empty-state-card"><BookOpen size={32} /> <p>No education added.</p></div> :
                    educations.map(e => (
                        <div key={e.id} className="education-card-premium">
                            <div className="edu-icon"><BookOpen size={24} /></div>
                            <div className="edu-details">
                                <h3>{e.degree} in {e.fieldOfStudy}</h3>
                                <p>{e.institution}</p>
                                <small>{new Date(e.startDate).getFullYear()} - {e.endDate ? new Date(e.endDate).getFullYear() : 'Present'}</small>
                            </div>
                        </div>
                    ))
                }
                <button className="btn-secondary mt-4">Add Education</button>
            </div>
        </div>
    );
};

// PLACEMAKERS FOR FUTURE ROUTES
const PlaceholderFeature = ({ title, icon }) => (
    <div className="placeholder-feature animated-fade">
        <div className="placeholder-icon">{icon}</div>
        <h2>{title}</h2>
        <p>This module is coming in a future update.</p>
        <div className="placeholder-tag">In Development</div>
    </div>
);

const CandidateDashboard = () => {
    const { user, logout } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();
    const [profile, setProfile] = useState(null);
    const [loading, setLoading] = useState(true);
    const [isMobileOpen, setIsMobileOpen] = useState(false);
    
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

    useEffect(() => { fetchProfile(); }, []);
    
    // Close mobile menu on route change
    useEffect(() => { setIsMobileOpen(false); }, [location.pathname]);

    const navGroups = [
        {
            title: "MAIN",
            items: [
                { path: '', label: 'Overview', icon: <LayoutDashboard size={20} /> },
                { path: 'profile', label: 'My Profile', icon: <User size={20} /> },
                { path: 'skills', label: 'Skills', icon: <Star size={20} /> },
            ]
        },
        {
            title: "CAREER",
            items: [
                { path: 'jobs', label: 'Jobs', icon: <Briefcase size={20} /> },
                { path: 'applications', label: 'Applications', icon: <FileText size={20} /> },
                { path: 'saved-jobs', label: 'Saved Jobs', icon: <Bookmark size={20} /> },
            ]
        },
        {
            title: "INTELLIGENCE 🚀",
            items: [
                { path: 'command-center', label: 'Command Center', icon: <Target size={20} /> },
                { path: 'career-intelligence', label: 'Career Intelligence', icon: <Lightbulb size={20} /> },
                { path: 'roadmap', label: 'Skill Roadmap', icon: <Map size={20} /> },
                { path: 'path', label: 'Career Path', icon: <TrendingUp size={20} /> }
            ]
        },
        {
            title: "TOOLS",
            items: [
                { path: 'resume', label: 'Resume Builder', icon: <FileStack size={20} /> },
                { path: 'assistant', label: 'AI Assistant', icon: <Bot size={20} /> },
                { path: 'settings', label: 'Settings', icon: <Settings size={20} /> },
            ]
        }
    ];

    if (loading) {
        return (
            <div className="app-loader">
                <LayoutDashboard size={48} className="pulse-icon" />
                <p>Loading CareerMind...</p>
            </div>
        );
    }

    const currentPath = location.pathname.split('/').pop();
    const activeNav = currentPath === 'candidate' ? '' : currentPath;

    // Get current page title for the header
    let pageTitle = "Dashboard Overview";
    navGroups.forEach(g => {
        const found = g.items.find(i => i.path === activeNav);
        if (found) pageTitle = found.label;
    });
    if (activeNav === 'education') pageTitle = "Education";
    if (activeNav === 'experience') pageTitle = "Experience";

    return (
        <div className="app-shell">
            {/* Sidebar */}
            <aside className={`app-sidebar ${isMobileOpen ? 'mobile-open' : ''}`}>
                <div className="sidebar-brand">
                    <div className="brand-logo">
                        <TrendingUp size={24} color="#3b82f6" />
                    </div>
                    <h2>CareerMind</h2>
                    <button className="mobile-close" onClick={() => setIsMobileOpen(false)}><X size={24} /></button>
                </div>
                
                <div className="sidebar-nav-container">
                    {navGroups.map((group, idx) => (
                        <div className="nav-group" key={idx}>
                            <h4 className="nav-group-title">{group.title}</h4>
                            <nav className="nav-menu">
                                {group.items.map(item => (
                                    <Link 
                                        key={item.path} 
                                        to={`/candidate/${item.path}`} 
                                        className={`nav-link-premium ${activeNav === item.path ? 'active' : ''}`}
                                    >
                                        <div className="nav-icon">{item.icon}</div>
                                        <span>{item.label}</span>
                                    </Link>
                                ))}
                            </nav>
                        </div>
                    ))}
                </div>

                <div className="sidebar-bottom">
                    <div className="user-profile-widget">
                        <div className="user-avatar">
                            {user?.firstName?.charAt(0) || 'U'}
                        </div>
                        <div className="user-info">
                            <strong>{user?.firstName} {user?.lastName}</strong>
                            {profile && (
                                <div className="completion-mini">
                                    <div className="comp-bar"><div className="comp-fill" style={{width: `${profile.completionPercentage || 0}%`}}></div></div>
                                    <span>{profile.completionPercentage || 0}%</span>
                                </div>
                            )}
                        </div>
                    </div>
                    <button className="btn-logout" onClick={() => { logout(); navigate('/'); }}>
                        <LogOut size={18} /> Logout
                    </button>
                </div>
            </aside>

            {/* Mobile Overlay */}
            {isMobileOpen && <div className="sidebar-overlay" onClick={() => setIsMobileOpen(false)}></div>}

            <main className="app-main">
                {/* Top Header */}
                <header className="app-header">
                    <div className="header-left">
                        <button className="mobile-menu-btn" onClick={() => setIsMobileOpen(true)}><Menu size={24} /></button>
                        <div className="page-title-block">
                            <h2>{pageTitle}</h2>
                            <p className="page-subtitle">Manage your professional journey</p>
                        </div>
                    </div>
                    <div className="header-right">
                        <div className="search-bar">
                            <Search size={18} className="search-icon" />
                            <input type="text" placeholder="Search jobs, skills..." />
                        </div>
                        <button className="icon-btn-header has-notification">
                            <Bell size={20} />
                            <span className="notif-badge"></span>
                        </button>
                        <div className="header-profile">
                            <div className="avatar-small">{user?.firstName?.charAt(0)}</div>
                            <ChevronDown size={16} />
                        </div>
                    </div>
                </header>

                {/* Main Content Area */}
                <div className="app-content">
                    <div className="content-container">
                        <Routes>
                            <Route path="/" element={<DashboardOverview />} />
                            
                            {/* Profile features */}
                            <Route path="profile" element={<ProfileSection initialData={profile} fetchProfile={fetchProfile} />} />
                            <Route path="skills" element={<SkillsSection skills={profile?.skills || []} fetchProfile={fetchProfile} />} />
                            <Route path="education" element={<EducationSection educations={profile?.educations || []} fetchProfile={fetchProfile} />} />
                            <Route path="experience" element={<PlaceholderFeature title="Experience Editor" icon={<Briefcase size={48} color="#8b5cf6" />} />} />
                            
                            {/* Jobs */}
                            <Route path="jobs" element={<CandidateJobs />} />
                            <Route path="jobs/:id" element={<CandidateJobDetails />} />
                            <Route path="applications" element={<CandidateApplications />} />
                            <Route path="saved-jobs" element={<CandidateSavedJobs />} />
                            
                            {/* Intelligence */}
                            <Route path="command-center" element={<CareerCommandCenter />} />
                            <Route path="career-intelligence" element={<CareerIntelligence />} />
                            <Route path="roadmap" element={<PlaceholderFeature title="Interactive Skill Roadmap" icon={<Map size={48} color="#10b981" />} />} />
                            <Route path="path" element={<PlaceholderFeature title="Career Path Simulator" icon={<TrendingUp size={48} color="#3b82f6" />} />} />
                            
                            {/* Tools */}
                            <Route path="resume" element={<ResumeIntelligence />} />
                            <Route path="assistant" element={<PlaceholderFeature title="AI Interview Prep" icon={<Bot size={48} color="#ef4444" />} />} />
                            <Route path="settings" element={<PlaceholderFeature title="Account Settings" icon={<Settings size={48} color="#64748b" />} />} />
                        </Routes>
                    </div>
                </div>
            </main>
        </div>
    );
};

export default CandidateDashboard;
