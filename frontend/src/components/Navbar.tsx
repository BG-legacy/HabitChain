import React, { useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './Navbar.css';

const Navbar: React.FC = () => {
  const { user, isAuthenticated, logout, isLoading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);

  const handleLogout = async () => {
    try {
      await logout();
      navigate('/login');
    } catch (error) {
      console.error('Logout error:', error);
    }
  };

  const toggleMenu = () => {
    setIsMenuOpen(!isMenuOpen);
  };

  const toggleDropdown = () => {
    setIsDropdownOpen(!isDropdownOpen);
  };

  // Hide navbar on landing page
  if (location.pathname === '/') {
    return null;
  }

  if (isLoading) {
    return (
      <nav className="navbar">
        <div className="navbar-container">
          <div className="navbar-brand">
            HabitChain
          </div>
          <div className="navbar-links">
            <div className="loading-placeholder"></div>
          </div>
        </div>
      </nav>
    );
  }

  return (
    <nav className="navbar">
      <div className="navbar-container">
        <Link to="/" className="navbar-brand">
          🔗 HabitChain
        </Link>
        
        {/* Mobile menu button */}
        <button 
          className={`mobile-menu-btn ${isMenuOpen ? 'open' : ''}`}
          onClick={toggleMenu}
          aria-label="Toggle menu"
        >
          <span className="hamburger"></span>
        </button>

        <div className={`navbar-links ${isMenuOpen ? 'open' : ''}`}>
          {isAuthenticated ? (
            <>
              <Link to="/dashboard" className="nav-link" onClick={() => setIsMenuOpen(false)}>
                📊 Dashboard
              </Link>
              <Link to="/habits" className="nav-link" onClick={() => setIsMenuOpen(false)}>
                🎯 Habits
              </Link>
              <Link to="/check-in" className="nav-link" onClick={() => setIsMenuOpen(false)}>
                ✅ Check-In
              </Link>
              
              {/* Dropdown Menu */}
              <div className="dropdown-container">
                <button 
                  className="nav-link dropdown-toggle"
                  onClick={toggleDropdown}
                >
                  More ▼
                </button>
                <div className={`dropdown-menu ${isDropdownOpen ? 'open' : ''}`}>
                  <Link to="/badges" className="dropdown-item" onClick={() => { setIsMenuOpen(false); setIsDropdownOpen(false); }}>
                    🏆 Badges
                  </Link>
                  <Link to="/calendar" className="dropdown-item" onClick={() => { setIsMenuOpen(false); setIsDropdownOpen(false); }}>
                    📅 Calendar
                  </Link>
                  <Link to="/export" className="dropdown-item" onClick={() => { setIsMenuOpen(false); setIsDropdownOpen(false); }}>
                    📤 Export
                  </Link>
                  <Link to="/ai-recommendations" className="dropdown-item" onClick={() => { setIsMenuOpen(false); setIsDropdownOpen(false); }}>
                    🤖 AI Recommendations
                  </Link>
                </div>
              </div>

              <div className="nav-user">
                <span className="nav-username">
                  Welcome, {user?.firstName || user?.username || 'User'}
                </span>
                <Link to="/profile" className="nav-link" onClick={() => setIsMenuOpen(false)}>
                  👤 Profile
                </Link>
                <button 
                  onClick={handleLogout}
                  className="nav-link logout-btn"
                >
                  🚪 Logout
                </button>
              </div>
            </>
          ) : (
            <>
              <Link to="/login" className="nav-link" onClick={() => setIsMenuOpen(false)}>
                🔑 Login
              </Link>
              <Link to="/register" className="nav-link" onClick={() => setIsMenuOpen(false)}>
                📝 Register
              </Link>
            </>
          )}
        </div>
      </div>
    </nav>
  );
};

export default Navbar; 