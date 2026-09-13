import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { candidateService } from '../../services/candidate';
import api from '../../services/api';
import { 
    Briefcase,
    Building2,
    MapPin,
    ArrowRight,
    TrendingUp,
    FileText,
    Bookmark,
    Award,
    CheckCircle,
    Clock,
    AlertCircle,
    ChevronRight,
    BookOpen
} from 'lucide-react';
import './DashboardOverview.css';

const ScoreRing = ({ score, size = 64, strokeWidth = 6 }) => {
    const radius = (size - strokeWidth) / 2;
    const circumference = radius * 2 * Math.PI;
    const offset = circumference - (score / 100) * circumference;
    const color = score >= 75 ? '#10b981' : score >= 50 ? '#f59e0b' : '#ef4444';

    return (
        <div className="score-ring-container" style={{ width: size, height: size }}>
            <svg width={size} height={size} className="score-ring-svg">
                <circle 
                    stroke="#e2e8f0" 
                    fill="transparent" 
                    strokeWidth={strokeWidth}
                    r={radius} 
                    cx={size / 2} 
                    cy={size / 2} 
                />
                <circle 
                    stroke={color} 
                    fill="transparent" 
                    strokeWidth={strokeWidth}
                    strokeLinecap="round"
                    strokeDasharray={`${circumference} ${circumference}`}
                    style={{ strokeDashoffset: offset }}
                    r={radius} 
                    cx={size / 2} 
                    cy={size / 2} 
                />
            </svg>
            <div className="score-ring-text">
                <span style={{ color }}>{score}%</span>
            </div>
        </div>
    );
};

const DashboardOverview = () => {
    const { user } = useAuth();
    const [loading, setLoading] = useState(true);
    
    const [profile, setProfile] = useState(null);
    const [applications, setApplications] = useState([]);
    const [savedJobs, setSavedJobs] = useState([]);
    const [recommendedJobs, setRecommendedJobs] = useState([]);
    const [careerIntelligence, setCareerIntelligence] = useState(null);

    useEffect(() => {
        const fetchDashboardData = async () => {
            try {
                const [
                    profileRes, 
                    appsRes, 
                    savedJobsRes, 
                    matchesRes,
                    intelligenceRes
                ] = await Promise.allSettled([
                    candidateService.getProfile(),
                    api.get('/Candidates/me/applications'),
                    api.get('/Candidates/me/saved-jobs'),
                    candidateService.getJobMatches(),
                    candidateService.getCareerPathIntelligence()
                ]);

                if (profileRes.status === 'fulfilled') setProfile(profileRes.value.data);
                if (appsRes.status === 'fulfilled') setApplications(appsRes.value.data);
                if (savedJobsRes.status === 'fulfilled') setSavedJobs(savedJobsRes.value.data);
                
                if (matchesRes.status === 'fulfilled') {
                    // Get top 4 matches
                    const jobs = matchesRes.value.data;
                    setRecommendedJobs(jobs.slice(0, 4));
                }

                if (intelligenceRes.status === 'fulfilled') {
                    setCareerIntelligence(intelligenceRes.value.data);
                }
            } catch (err) {
                console.error("Error fetching dashboard data", err);
            } finally {
                setLoading(false);
            }
        };

        fetchDashboardData();
    }, []);

    if (loading) {
        return (
            <div className="dashboard-loader">
                <div className="spinner"></div>
                <p>Loading your career workspace...</p>
            </div>
        );
    }

    const readinessScore = careerIntelligence?.overallCareerReadiness || (profile?.completionPercentage > 50 ? 50 : 25);
    const bestCareerRole = careerIntelligence?.bestCareer?.careerRole || 'Not yet calculated';
    
    // Calculate missing profile fields (mock logic based on completion percentage for display, if exact fields missing isn't returned by API)
    const renderNextBestAction = () => {
        if (!profile || profile.completionPercentage < 100) {
            return {
                title: "Complete Your Profile",
                description: "You're missing some profile details. A 100% complete profile boosts your visibility.",
                action: "Update Profile",
                link: "/candidate/profile",
                icon: <AlertCircle size={24} color="#f59e0b" />
            };
        }
        if (recommendedJobs.length > 0 && applications.length === 0) {
            return {
                title: "Start Applying",
                description: "You have strong matches! Start applying to recommended jobs.",
                action: "View Recommended Jobs",
                link: "/candidate/jobs",
                icon: <Briefcase size={24} color="#3b82f6" />
            };
        }
        if (careerIntelligence?.nextCareerMove?.focusNext?.length > 0) {
            return {
                title: "Develop New Skills",
                description: `Focus on learning ${careerIntelligence.nextCareerMove.focusNext[0].skillName} to advance.`,
                action: "View Roadmap",
                link: "/candidate/career-intelligence",
                icon: <TrendingUp size={24} color="#10b981" />
            };
        }
        
        return {
            title: "Explore Career Paths",
            description: "Check out AI career recommendations based on your current trajectory.",
            action: "Career Intelligence",
            link: "/candidate/career-intelligence",
            icon: <Award size={24} color="#8b5cf6" />
        };
    };

    const nextAction = renderNextBestAction();

    return (
        <div className="dashboard-overview animated-fade">
            <div className="dashboard-header-block">
                <div className="dashboard-welcome">
                    <h1>Good {new Date().getHours() < 12 ? 'morning' : new Date().getHours() < 18 ? 'afternoon' : 'evening'}, {user?.firstName}</h1>
                    <p>Here is what's happening with your career today.</p>
                </div>
                <div className="dashboard-action-card">
                    <div className="action-card-icon">{nextAction.icon}</div>
                    <div className="action-card-content">
                        <h3>{nextAction.title}</h3>
                        <p>{nextAction.description}</p>
                    </div>
                    <Link to={nextAction.link} className="btn-primary btn-sm">{nextAction.action}</Link>
                </div>
            </div>

            <div className="stats-grid mt-6">
                <div className="stat-card">
                    <div className="stat-header">
                        <span className="stat-label">Profile Completion</span>
                        <div className="stat-icon bg-blue-light"><CheckCircle size={20} color="#3b82f6" /></div>
                    </div>
                    <div className="stat-value">{profile?.completionPercentage || 0}%</div>
                    <div className="stat-progress-bar">
                        <div className="stat-progress-fill" style={{ width: `${profile?.completionPercentage || 0}%` }}></div>
                    </div>
                </div>

                <div className="stat-card">
                    <div className="stat-header">
                        <span className="stat-label">Career Readiness</span>
                        <div className="stat-icon bg-green-light"><Award size={20} color="#10b981" /></div>
                    </div>
                    <div className="stat-value">{Math.round(readinessScore)}/100</div>
                    <p className="stat-desc">Based on target roles</p>
                </div>

                <div className="stat-card">
                    <div className="stat-header">
                        <span className="stat-label">Total Applications</span>
                        <div className="stat-icon bg-purple-light"><FileText size={20} color="#8b5cf6" /></div>
                    </div>
                    <div className="stat-value">{applications.length}</div>
                    <p className="stat-desc">Active applications</p>
                </div>

                <div className="stat-card">
                    <div className="stat-header">
                        <span className="stat-label">Saved Jobs</span>
                        <div className="stat-icon bg-orange-light"><Bookmark size={20} color="#f59e0b" /></div>
                    </div>
                    <div className="stat-value">{savedJobs.length}</div>
                    <p className="stat-desc">Jobs to review later</p>
                </div>
            </div>

            <div className="dashboard-grid mt-6">
                <div className="dashboard-col-left">
                    {/* RECOMMENDED JOBS */}
                    <div className="dashboard-section-card">
                        <div className="section-header">
                            <h2>Recommended For You</h2>
                            <Link to="/candidate/jobs" className="link-action">View all <ArrowRight size={16} /></Link>
                        </div>
                        <div className="job-list-compact">
                            {recommendedJobs.length > 0 ? (
                                recommendedJobs.map(job => (
                                    <div className="job-card-compact" key={job.jobId}>
                                        <div className="job-card-main">
                                            <h4>{job.jobTitle}</h4>
                                            <div className="job-meta">
                                                <span><Building2 size={14} /> {job.companyName}</span>
                                                <span><MapPin size={14} /> {job.location}</span>
                                            </div>
                                            <div className="job-skills-preview">
                                                {job.matchedSkills && job.matchedSkills.slice(0, 3).map(s => (
                                                    <span key={s} className="skill-pill-sm">{s}</span>
                                                ))}
                                                {job.matchedSkills?.length > 3 && <span className="skill-pill-sm-more">+{job.matchedSkills.length - 3}</span>}
                                            </div>
                                        </div>
                                        <div className="job-card-actions">
                                            <div className="match-score">
                                                <ScoreRing score={Math.round(job.matchScore || job.compatibilityScore || 0)} size={48} strokeWidth={4} />
                                            </div>
                                            <Link to={`/candidate/jobs/${job.jobId}`} className="btn-secondary btn-sm">View Roles</Link>
                                        </div>
                                    </div>
                                ))
                            ) : (
                                <div className="empty-state-card">
                                    <Briefcase size={40} className="empty-icon" />
                                    <h4>No recommendations yet</h4>
                                    <p>Complete your profile to get personalized AI matched jobs.</p>
                                </div>
                            )}
                        </div>
                    </div>

                    {/* RECENT APPLICATIONS */}
                    <div className="dashboard-section-card mt-6">
                        <div className="section-header">
                            <h2>Recent Applications</h2>
                            <Link to="/candidate/applications" className="link-action">View all <ArrowRight size={16} /></Link>
                        </div>
                        <div className="applications-list-compact">
                            {applications.length > 0 ? (
                                applications.slice(0, 3).map(app => (
                                    <div className="app-card-compact" key={app.id}>
                                        <div className="app-card-main">
                                            <h4>{app.jobTitle}</h4>
                                            <p>{app.companyName}</p>
                                            <span className="app-date"><Clock size={12} /> {new Date(app.appliedAt).toLocaleDateString()}</span>
                                        </div>
                                        <div className={`status-pill status-${app.status?.toLowerCase() || 'pending'}`}>
                                            {app.status || 'Pending Review'}
                                        </div>
                                    </div>
                                ))
                            ) : (
                                <div className="empty-state-card">
                                    <FileText size={40} className="empty-icon" />
                                    <h4>No recent applications</h4>
                                    <p>You haven't applied to any jobs yet. Start exploring!</p>
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                <div className="dashboard-col-right">
                    {/* CAREER INTELLIGENCE PREVIEW */}
                    <div className="dashboard-section-card intelligence-preview">
                        <div className="intell-bg-decorator"></div>
                        <div className="section-header">
                            <h2>Career Intelligence</h2>
                            <Link to="/candidate/career-intelligence" className="link-action-white"><ArrowRight size={18} /></Link>
                        </div>
                        
                        <div className="intell-card-content">
                            <div className="best-career-block">
                                <span className="intell-label">Recommended Direction</span>
                                <h3>{bestCareerRole}</h3>
                                {careerIntelligence?.bestCareer && (
                                    <div className="intell-score-row mt-3">
                                        <div className="intell-score-item">
                                            <span>Readiness</span>
                                            <strong>{Math.round(readinessScore)}%</strong>
                                        </div>
                                        <div className="intell-score-item">
                                            <span>Match</span>
                                            <strong>{Math.round(careerIntelligence.bestCareer.compatibilityScore)}%</strong>
                                        </div>
                                    </div>
                                )}
                            </div>
                            
                            {careerIntelligence?.explainableInsights && careerIntelligence.explainableInsights.length > 0 && (
                                <div className="intell-insight">
                                    <div className="insight-icon">💡</div>
                                    <p>{careerIntelligence.explainableInsights[0]}</p>
                                </div>
                            )}

                            {!careerIntelligence && (
                                <div className="intell-empty">
                                    <p>Not enough data to map your career path. Add skills and experience to unlock AI insights.</p>
                                    <Link to="/candidate/profile" className="btn-light btn-sm mt-3">Update Profile</Link>
                                </div>
                            )}
                        </div>
                    </div>

                    {/* SKILL ROADMAP PREVIEW */}
                    <div className="dashboard-section-card mt-6 roadmap-preview">
                        <div className="section-header">
                            <h2>Skill Roadmap</h2>
                            <div className="roadmap-icon-label"><BookOpen size={16} /></div>
                        </div>
                        
                        {careerIntelligence?.nextCareerMove?.focusNext && careerIntelligence.nextCareerMove.focusNext.length > 0 ? (
                            <div className="roadmap-steps">
                                {careerIntelligence.nextCareerMove.focusNext.slice(0, 3).map((item, idx) => (
                                    <div className="roadmap-step" key={idx}>
                                        <div className="step-priority-dot" data-priority={item.priority}></div>
                                        <div className="step-content">
                                            <h4>{item.skillName}</h4>
                                            <span className="step-badge">{item.priority} Priority</span>
                                        </div>
                                    </div>
                                ))}
                                <Link to="/candidate/career-intelligence" className="btn-outline btn-block mt-4">
                                    View Full Roadmap
                                </Link>
                            </div>
                        ) : (
                            <div className="empty-state-card pb-4 pt-4">
                                <h4>No active roadmap</h4>
                                <p className="text-sm">Complete your profile to generate a learning roadmap.</p>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default DashboardOverview;
