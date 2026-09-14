import api from './api';

export const resumeService = {
    // Resumes
    uploadResume: async (file) => {
        const formData = new FormData();
        formData.append('file', file);
        return api.post('/candidates/me/resumes', formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
    },
    getResumes: async () => api.get('/candidates/me/resumes'),
    getResume: async (id) => api.get(`/candidates/me/resumes/${id}`),
    deleteResume: async (id) => api.delete(`/candidates/me/resumes/${id}`),
    setPrimary: async (id) => api.put(`/candidates/me/resumes/${id}/primary`),
    
    // Analysis
    analyzeResume: async (resumeId) => api.post(`/candidates/me/resumes/${resumeId}/analyze`),
    analyzeResumeForJob: async (resumeId, jobId) => api.post(`/candidates/me/resumes/${resumeId}/analyze/${jobId}`),
    getAnalyses: async (resumeId) => api.get(`/candidates/me/resumes/${resumeId}/analyses`),
    getLatestAnalysis: async (resumeId) => api.get(`/candidates/me/resumes/${resumeId}/analyses/latest`)
};
