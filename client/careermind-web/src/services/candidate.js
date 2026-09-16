import api from './api';

export const candidateService = {
    getProfile: () => api.get('/candidates/me/profile'),
    updateProfile: (data) => api.put('/candidates/me/profile', data),

    getAvailableSkills: (query) => api.get(`/candidates/available-skills?query=${query}`),

    getSkills: () => api.get('/candidates/me/skills'),
    addSkill: (data) => api.post('/candidates/me/skills', data),
    updateSkill: (id, data) => api.put(`/candidates/me/skills/${id}`, data),
    removeSkill: (id) => api.delete(`/candidates/me/skills/${id}`),

    getEducation: () => api.get('/candidates/me/education'),
    addEducation: (data) => api.post('/candidates/me/education', data),
    updateEducation: (id, data) => api.put(`/candidates/me/education/${id}`, data),
    removeEducation: (id) => api.delete(`/candidates/me/education/${id}`),

    getExperience: () => api.get('/candidates/me/experience'),
    addExperience: (data) => api.post('/candidates/me/experience', data),
    updateExperience: (id, data) => api.put(`/candidates/me/experience/${id}`, data),
    removeExperience: (id) => api.delete(`/candidates/me/experience/${id}`),

    getCertifications: () => api.get('/candidates/me/certifications'),
    addCertification: (data) => api.post('/candidates/me/certifications', data),
    updateCertification: (id, data) => api.put(`/candidates/me/certifications/${id}`, data),
    removeCertification: (id) => api.delete(`/candidates/me/certifications/${id}`),

    getLanguages: () => api.get('/candidates/me/languages'),
    addLanguage: (data) => api.post('/candidates/me/languages', data),
    updateLanguage: (id, data) => api.put(`/candidates/me/languages/${id}`, data),
    removeLanguage: (id) => api.delete(`/candidates/me/languages/${id}`),

    getPreferences: () => api.get('/candidates/me/preferences'),
    updatePreferences: (data) => api.put('/candidates/me/preferences', data),

    getJobMatches: () => api.get('/candidates/me/job-matches'),
    getJobMatchDetails: (id) => api.get(`/candidates/me/job-matches/${id}`),
    getCareerGapAnalysis: (id) => api.get(`/candidates/me/career-gap/${id}`),
    getCareerPathIntelligence: () => api.get('/candidates/me/career-path'),

    getCommandCenter: () => api.get('/candidates/me/career-command-center'),
    updateCommandCenterAction: (actionKey, isCompleted) =>
        api.put(`/candidates/me/career-command-center/actions/${encodeURIComponent(actionKey)}`, { isCompleted }),
    startInterview: (data) => api.post('/candidates/me/interviews', data),
    getInterviews: () => api.get('/candidates/me/interviews'),
    getInterview: (sessionId) => api.get(`/candidates/me/interviews/${sessionId}`),
    submitInterviewAnswer: (sessionId, questionId, data) => api.post(`/candidates/me/interviews/${sessionId}/answers?questionId=${questionId}`, data),
    getInterviewReadiness: (sessionId) => api.get(`/candidates/me/interviews/${sessionId}/readiness`),
    completeInterview: (sessionId) => api.post(`/candidates/me/interviews/${sessionId}/complete`)
};
