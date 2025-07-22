import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { AnimatePresence } from 'framer-motion';
import { Toaster } from 'react-hot-toast';
import { AuthProvider } from './contexts/AuthContext';
import { DashboardProvider } from './contexts/DashboardContext';
import ErrorBoundary from './components/ErrorBoundary';
import ProtectedRoute from './components/ProtectedRoute';
import PublicRoute from './components/PublicRoute';
import { AnimatedNavbar } from './components/AnimatedComponents';
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
import AiRecommendationsPage from './components/AiRecommendationsPage';
import CompletionRatesPage from './components/CompletionRatesPage';
import SessionExpiration from './components/SessionExpiration';
import './App.css';

// Layout component that can access location
const Layout: React.FC = () => {
  const location = useLocation();
  const isLandingPage = location.pathname === '/';

  return (
    <DashboardProvider>
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
                  path="/habits/:id" 
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
                      <AiRecommendationsPage />
                    </ProtectedRoute>
                  } 
                />
                <Route 
                  path="/completion-rates" 
                  element={
                    <ProtectedRoute>
                      <CompletionRatesPage />
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
    </DashboardProvider>
  );
};

function App() {
  console.log("🚀 HabitChain App is loading! If you see this, your console is working.");
  
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
