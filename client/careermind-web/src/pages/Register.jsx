import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate, Link } from 'react-router-dom';

const Register = () => {
    const [formData, setFormData] = useState({
        firstName: '',
        lastName: '',
        email: '',
        password: '',
        confirmPassword: '',
        role: 'Candidate' // Default allowed role
    });
    const [error, setError] = useState(null);
    const [loadingState, setLoadingState] = useState(false);
    const { register } = useAuth();
    const navigate = useNavigate();

    const handleChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError(null);
        
        if (formData.password !== formData.confirmPassword) {
            return setError("Passwords do not match");
        }
        
        setLoadingState(true);
        try {
            const registeredUser = await register(formData);
            if (registeredUser?.role === 'Candidate') {
                navigate('/candidate/dashboard');
            } else if (registeredUser?.role === 'Employer') {
                navigate('/employer/dashboard');
            } else {
                navigate('/');
            }
        } catch (err) {
            setError(err.response?.data?.message || 'Registration failed');
        } finally {
            setLoadingState(false);
        }
    };

    return (
        <div style={{ maxWidth: '400px', margin: '50px auto', padding: '20px', border: '1px solid #ccc', borderRadius: '5px' }}>
            <h2>Register</h2>
            {error && <div style={{ color: 'red', marginBottom: '10px' }}>{error}</div>}
            <form onSubmit={handleSubmit}>
                <div style={{ marginBottom: '15px' }}>
                    <label>First Name</label>
                    <input 
                        type="text" name="firstName" value={formData.firstName} onChange={handleChange} required 
                        style={{ width: '100%', padding: '8px', marginTop: '5px' }}
                    />
                </div>
                <div style={{ marginBottom: '15px' }}>
                    <label>Last Name</label>
                    <input 
                        type="text" name="lastName" value={formData.lastName} onChange={handleChange} required 
                        style={{ width: '100%', padding: '8px', marginTop: '5px' }}
                    />
                </div>
                <div style={{ marginBottom: '15px' }}>
                    <label>Email</label>
                    <input 
                        type="email" name="email" value={formData.email} onChange={handleChange} required 
                        style={{ width: '100%', padding: '8px', marginTop: '5px' }}
                    />
                </div>
                <div style={{ marginBottom: '15px' }}>
                    <label>Password</label>
                    <input 
                        type="password" name="password" value={formData.password} onChange={handleChange} required 
                        style={{ width: '100%', padding: '8px', marginTop: '5px' }}
                    />
                </div>
                <div style={{ marginBottom: '15px' }}>
                    <label>Confirm Password</label>
                    <input 
                        type="password" name="confirmPassword" value={formData.confirmPassword} onChange={handleChange} required 
                        style={{ width: '100%', padding: '8px', marginTop: '5px' }}
                    />
                </div>
                <div style={{ marginBottom: '15px' }}>
                    <label>Register As:</label>
                    <select name="role" value={formData.role} onChange={handleChange} style={{ width: '100%', padding: '8px', marginTop: '5px' }}>
                        <option value="Candidate">Candidate</option>
                        <option value="Employer">Employer</option>
                    </select>
                </div>
                <button type="submit" disabled={loadingState} style={{ width: '100%', padding: '10px' }}>
                    {loadingState ? 'Registering...' : 'Register'}
                </button>
            </form>
            <p style={{ marginTop: '15px', textAlign: 'center' }}>
                Already have an account? <Link to="/login">Login here</Link>
            </p>
        </div>
    );
};

export default Register;
