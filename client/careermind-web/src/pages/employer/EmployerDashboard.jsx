import React, { useState, useEffect } from 'react';
import { Routes, Route, Link, useLocation } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import EmployerJobs from './EmployerJobs';
import EmployerJobForm from './EmployerJobForm';
import EmployerApplications from './EmployerApplications';

const EmployerDashboard = () => {
    const { user, logout } = useAuth();
    const location = useLocation();

    const navItems = [
        { path: '', label: 'Dashboard Overview' },
        { path: 'profile', label: 'Company Profile' },
        { path: 'jobs', label: 'My Jobs' },
        { path: 'jobs/create', label: 'Post a Job' },
    ];

    const currentPath = location.pathname.split('/').slice(2).join('/');
    const activeNav = currentPath === '' ? '' : currentPath;

    return (
        <div style={{ display: 'flex', minHeight: '100vh', background: '#f5f7fa' }}>
            <aside style={{ width: '250px', background: '#2c3e50', color: '#ecf0f1', padding: '20px' }}>
                <div style={{ marginBottom: '40px' }}>
                    <h2>Employer Portal</h2>
                    <p>Welcome, {user?.firstName}</p>
                </div>
                <nav style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
                    {navItems.map(item => (
                        <Link 
                            key={item.path} 
                            to={`/employer/${item.path}`} 
                            style={{ 
                                color: (activeNav === item.path || (activeNav.startsWith('jobs') && item.path === 'jobs' && activeNav !== 'jobs/create')) ? '#3498db' : '#ecf0f1', 
                                textDecoration: 'none',
                                padding: '8px',
                                borderRadius: '4px',
                                background: (activeNav === item.path || (activeNav.startsWith('jobs') && item.path === 'jobs' && activeNav !== 'jobs/create')) ? 'rgba(52, 152, 219, 0.2)' : 'transparent'
                            }}
                        >
                            {item.label}
                        </Link>
                    ))}
                    <button onClick={logout} style={{ marginTop: 'auto', background: 'transparent', color: '#ff6b6b', border: '1px solid #ff6b6b', padding: '8px', cursor: 'pointer', borderRadius: '4px' }}>
                        Logout
                    </button>
                </nav>
            </aside>
            <main style={{ flex: 1, padding: '40px' }}>
                <Routes>
                    <Route path="/" element={<div style={{ background: '#fff', padding: '40px', borderRadius: '8px' }}><h2>Dashboard Overview</h2><p>Overview of active jobs and applications will go here.</p></div>} />
                    <Route path="profile" element={<div style={{ background: '#fff', padding: '40px', borderRadius: '8px' }}><h2>Company Profile</h2><p>Profile form will go here.</p></div>} />
                    <Route path="jobs" element={<EmployerJobs />} />
                    <Route path="jobs/create" element={<EmployerJobForm />} />
                    <Route path="jobs/:id/edit" element={<EmployerJobForm />} />
                    <Route path="jobs/:id/applications" element={<EmployerApplications />} />
                </Routes>
            </main>
        </div>
    );
};

export default EmployerDashboard;
