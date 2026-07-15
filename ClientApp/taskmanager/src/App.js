import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import HomePage from './components/home/HomePage';
import ProjectsList from './components/project/ProjectsList';
import ProjectDetail from './components/project/ProjectDetail';
import './App.css';

function App() {
    return (
        <AuthProvider>
            <Router>
                <div className="App">
                    <Routes>
                        <Route path="/" element={<HomePage />} />
                        <Route path="/projects" element={<ProjectsList />} />
                        <Route path="/projects/:projectId" element={<ProjectDetail />} />
                    </Routes>
                </div>
            </Router>
        </AuthProvider>
    );
}

export default App;