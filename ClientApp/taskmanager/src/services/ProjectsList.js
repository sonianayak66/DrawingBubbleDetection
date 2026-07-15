import React, { useState, useEffect } from 'react';
import { projectService } from '../services/projectService';

function ProjectsList() {
    const [projects, setProjects] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        fetchProjects();
    }, []);

    const fetchProjects = async () => {
        try {
            const response = await projectService.getProjects();
            console.log('API Response:', response);
            setProjects(response.data || []);
        } catch (err) {
            console.error('Error:', err);
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    if (loading) return <div>Loading...</div>;
    if (error) return <div>Error: {error}</div>;

    return (
        <div>
            <h2>Projects</h2>
            <p>Total projects: {projects.length}</p>
        </div>
    );
}

export default ProjectsList;