import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { AnimatePresence } from 'framer-motion';
import { Toaster } from 'react-hot-toast';
import { AuthProvider } from './contexts/AuthContext';
import ErrorBoundary from './components/ErrorBoundary';
import ProtectedRoute from './components/ProtectedRoute';
import PublicRoute from './components/PublicRoute';
import { AnimatedNavbar, PageTransition } from './components/AnimatedComponents';
import Navbar from './components/Navbar';
import Landing from './components/Landing';
import Login from './components/Login';
import Register from './components/Register';
import Dashboard from './components/Dashboard';
import Habits from './components/Habits';
import CheckIn from './components/CheckIn';
import Encouragements from './components/Encouragements';
import Badges from './components/Badges';
import Calendar from './components/Calendar';
import Export from './components/Export';
import SessionExpiration from './components/SessionExpiration';
import './App.css';

// Placeholder components for features not yet implemented
const Profile = () => (
  <PageTransition>
    <div className="main-container">
      <div className="page-header">
        <h1 className="page-title">Profile</h1>
        <p className="page-subtitle">Manage your account settings and preferences</p>
      </div>
      <div className="glass-card p-lg">
        <div className="empty-state">
          <div className="empty-icon">👤</div>
          <h3 className="empty-title">Profile Management</h3>
          <p className="empty-description">Profile management features are coming soon. You'll be able to update your personal information, preferences, and account settings.</p>
        </div>
      </div>
    </div>
  </PageTransition>
);

// Layout component that can access location
const Layout: React.FC = () => {
  const location = useLocation();
  const isLandingPage = location.pathname === '/';

  return (
    <div className="App">
      <div className="App-content">
        {!isLandingPage && (
          <AnimatedNavbar>
            <Navbar />
          </AnimatedNavbar>
        )}
        <main className={isLandingPage ? 'landing-main' : ''}>
          <AnimatePresence mode="wait">
            <Routes location={location} key={location.pathname}>
              {/* Landing page - accessible to everyone */}
              <Route path="/" element={<Landing />} />
              
              {/* Public routes - only accessible when not authenticated */}
              <Route 
                path="/login" 
                element={
                  <PublicRoute>
                    <Login />
                  </PublicRoute>
                } 
              />
              <Route 
                path="/register" 
                element={
                  <PublicRoute>
                    <Register />
                  </PublicRoute>
                } 
              />
              
              {/* Protected routes - only accessible when authenticated */}
              <Route 
                path="/dashboard" 
                element={
                  <ProtectedRoute>
                    <Dashboard />
                  </ProtectedRoute>
                } 
              />
              <Route 
                path="/habits" 
                element={
                  <ProtectedRoute>
                    <Habits />
                  </ProtectedRoute>
                } 
              />
              <Route 
                path="/check-in" 
                element={
                  <ProtectedRoute>
                    <CheckIn />
                  </ProtectedRoute>
                } 
              />
              <Route 
                path="/encouragements" 
                element={
                  <ProtectedRoute>
                    <Encouragements />
                  </ProtectedRoute>
                } 
              />
              <Route 
                path="/badges" 
                element={
                  <ProtectedRoute>
                    <Badges />
                  </ProtectedRoute>
                } 
              />
              <Route 
                path="/calendar" 
                element={
                  <ProtectedRoute>
                    <Calendar />
                  </ProtectedRoute>
                } 
              />
              <Route 
                path="/export" 
                element={
                  <ProtectedRoute>
                    <Export />
                  </ProtectedRoute>
                } 
              />
              <Route 
                path="/ai-recommendations" 
                element={
                  <ProtectedRoute>
                    <PageTransition>
                      <div className="main-container">
                        <div className="page-header">
                          <h1 className="page-title">AI Recommendations</h1>
                          <p className="page-subtitle">Get personalized habit suggestions and insights</p>
                        </div>
                        <div className="glass-card p-lg">
                          <div className="empty-state">
                            <div className="empty-icon">🤖</div>
                            <h3 className="empty-title">AI Recommendations</h3>
                            <p className="empty-description">AI-powered habit recommendations and insights are coming soon. Get personalized suggestions to improve your habit-building journey.</p>
                          </div>
                        </div>
                      </div>
                    </PageTransition>
                  </ProtectedRoute>
                } 
              />
              <Route 
                path="/profile" 
                element={
                  <ProtectedRoute>
                    <Profile />
                  </ProtectedRoute>
                } 
              />
              
              {/* Catch all route - redirect to landing */}
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </AnimatePresence>
        </main>
        
        {/* Session expiration warning - only show when authenticated */}
        {!isLandingPage && <SessionExpiration warningMinutes={5} />}
      </div>
    </div>
  );
};

function App() {
  return (
    <ErrorBoundary>
      <AuthProvider>
        <Router>
          <Layout />
          <Toaster
            position="top-right"
            toastOptions={{
              duration: 4000,
              style: {
                background: '#363636',
                color: '#fff',
                borderRadius: '12px',
                padding: '16px',
                fontSize: '14px',
                fontWeight: '500',
                boxShadow: '0 8px 32px rgba(0, 0, 0, 0.12)',
                border: '1px solid rgba(255, 255, 255, 0.1)',
              },
              success: {
                iconTheme: {
                  primary: '#10b981',
                  secondary: '#fff',
                },
              },
              error: {
                iconTheme: {
                  primary: '#ef4444',
                  secondary: '#fff',
                },
              },
              loading: {
                iconTheme: {
                  primary: '#3b82f6',
                  secondary: '#fff',
                },
              },
            }}
          />
        </Router>
      </AuthProvider>
    </ErrorBoundary>
  );
}

export default App;
