import React from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ProtectedRoute } from './routes/ProtectedRoutes';
import Login from './pages/Login';
import Register from './pages/Register';

const Home = () => {
    const { user, logout } = useAuth();
    return (
        <div style={{ padding: '20px' }}>
            <h1>Welcome to CareerMind 🚀</h1>
            {user ? (
                <div>
                    <p>Hello, {user.firstName} {user.lastName}! Role: {user.role}</p>
                    {user.role === 'Admin' && <Link to="/admin" style={{ marginRight: '10px' }}>Admin Dashboard</Link>}
                    {user.role === 'Candidate' && <Link to="/candidate" style={{ marginRight: '10px' }}>Candidate Dashboard</Link>}
                    {user.role === 'Employer' && <Link to="/employer" style={{ marginRight: '10px' }}>Employer Dashboard</Link>}
                    <button onClick={logout}>Logout</button>
                </div>
            ) : (
                <div>
                    <Link to="/login" style={{ marginRight: '10px' }}>Login</Link>
                    <Link to="/register">Register</Link>
                </div>
            )}
        </div>
    );
};

// Placeholder dashboards
const AdminDashboard = () => <h2>Admin Dashboard (Protected)</h2>;
const CandidateDashboard = () => <h2>Candidate Dashboard (Protected)</h2>;
const EmployerDashboard = () => <h2>Employer Dashboard (Protected)</h2>;

function App() {
    return (
        <AuthProvider>
            <Router>
                <Routes>
                    <Route path="/" element={<Home />} />
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />
                    
                    {/* Protected Routes using roles */}
                    <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
                        <Route path="/admin/*" element={<AdminDashboard />} />
                    </Route>
                    
                    <Route element={<ProtectedRoute allowedRoles={['Candidate']} />}>
                        <Route path="/candidate/*" element={<CandidateDashboard />} />
                    </Route>
                    
                    <Route element={<ProtectedRoute allowedRoles={['Employer']} />}>
                        <Route path="/employer/*" element={<EmployerDashboard />} />
                    </Route>

                </Routes>
            </Router>
        </AuthProvider>
    );
}

export default App;
