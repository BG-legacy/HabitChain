import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { PageTransition } from './AnimatedComponents';
import AiRecommendationsModal from './AiRecommendations';
import AiRecommendationsService, { HabitRecommendation, UserHabitAnalysis } from '../services/aiRecommendationsService';
import { useToast } from '../hooks/useToast';
import { useDashboard } from '../contexts/DashboardContext';
import './AiRecommendationsPage.css';

const AiRecommendationsPage: React.FC = () => {
  const [showRecommendationsModal, setShowRecommendationsModal] = useState(false);
  const [userAnalysis, setUserAnalysis] = useState<UserHabitAnalysis | null>(null);
  const [motivation, setMotivation] = useState<string>('');
  const [loading, setLoading] = useState(true);
  const { showSuccess, showError } = useToast();
  const { refreshDashboard } = useDashboard();
  const navigate = useNavigate();

  useEffect(() => {
    loadAiData();
  }, []);

  const loadAiData = async () => {
    setLoading(true);
    try {
      // Load user analysis and motivation in parallel
      const [analysisData, motivationData] = await Promise.all([
        AiRecommendationsService.getUserAnalysis(),
        AiRecommendationsService.getPersonalizedMotivation()
      ]);
      
      setUserAnalysis(analysisData);
      setMotivation(motivationData);
    } catch (error) {
      console.error('Error loading AI data:', error);
      showError('Failed to load AI insights. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleHabitSelect = async (recommendation: HabitRecommendation) => {
    try {
      // Refresh the dashboard to show the new habit
      await refreshDashboard();
      
      // Show success message
      showSuccess(`Great choice! "${recommendation.name}" has been added to your habits.`);
      
      // Optionally navigate to habits page to see the new habit
      setTimeout(() => {
        navigate('/habits');
      }, 1500);
    } catch (error) {
      console.error('Error refreshing dashboard:', error);
      showError('Habit created but failed to refresh dashboard. Please refresh the page.');
    }
  };

  const getPatternInsights = () => {
    if (!userAnalysis?.patterns) return [];
    
    const patterns = userAnalysis.patterns;
    const insights = [];

    if (patterns.strongCategories.length > 0) {
      insights.push({
        type: 'strength',
        title: 'Strong Categories',
        description: `You excel in ${patterns.strongCategories.join(', ')} habits`,
        icon: '💪'
      });
    }

    if (patterns.preferredTime) {
      insights.push({
        type: 'time',
        title: 'Optimal Time',
        description: `You perform best during ${patterns.preferredTime}`,
        icon: '⏰'
      });
    }

    if (patterns.averageCompletionRate > 0.8) {
      insights.push({
        type: 'consistency',
        title: 'High Consistency',
        description: `${Math.round(patterns.averageCompletionRate * 100)}% completion rate - excellent!`,
        icon: '🎯'
      });
    }

    return insights;
  };

  if (loading) {
    return (
      <PageTransition>
        <div className="main-container">
          <div className="page-header">
            <h1 className="page-title">AI Recommendations</h1>
            <p className="page-subtitle">Get personalized habit suggestions and insights</p>
          </div>
          <div className="glass-card p-lg">
            <div className="ai-loading">
              <div className="spinner"></div>
              <p>🤖 AI is analyzing your habit patterns...</p>
            </div>
          </div>
        </div>
      </PageTransition>
    );
  }

  const insights = getPatternInsights();

  return (
    <PageTransition>
      <div className="main-container">
        <div className="page-header">
          <h1 className="page-title">AI Recommendations</h1>
          <p className="page-subtitle">Get personalized habit suggestions and insights</p>
        </div>

        {/* Motivation Section */}
        {motivation && (
          <div className="glass-card p-lg mb-lg">
            <div className="motivation-section">
              <div className="motivation-header">
                <div className="motivation-icon">🌟</div>
                <h3>Your Personal Motivation</h3>
              </div>
              <p className="motivation-text">{motivation}</p>
            </div>
          </div>
        )}

        {/* Insights Grid */}
        {insights.length > 0 && (
          <div className="glass-card p-lg mb-lg">
            <h3 className="insights-title">Your Habit Insights</h3>
            <div className="insights-grid">
              {insights.map((insight, index) => (
                <div key={index} className="insight-card">
                  <div className="insight-icon">{insight.icon}</div>
                  <div className="insight-content">
                    <h4>{insight.title}</h4>
                    <p>{insight.description}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Stats Overview */}
        {userAnalysis?.patterns && (
          <div className="glass-card p-lg mb-lg">
            <h3 className="stats-title">Your Habit Journey</h3>
            <div className="stats-grid">
              <div className="stat-item">
                <div className="stat-value">{userAnalysis.patterns.totalActiveHabits}</div>
                <div className="stat-label">Active Habits</div>
              </div>
              <div className="stat-item">
                <div className="stat-value">{userAnalysis.patterns.totalCompletedHabits}</div>
                <div className="stat-label">Completed Habits</div>
              </div>
              <div className="stat-item">
                <div className="stat-value">{Math.round(userAnalysis.patterns.averageCompletionRate * 100)}%</div>
                <div className="stat-label">Success Rate</div>
              </div>
              <div className="stat-item">
                <div className="stat-value">{userAnalysis.patterns.preferredFrequency}</div>
                <div className="stat-label">Preferred Frequency</div>
              </div>
            </div>
          </div>
        )}

        {/* Action Section */}
        <div className="glass-card p-lg">
          <div className="action-section">
            <div className="action-header">
              <h3>Ready to Level Up?</h3>
              <p>Get AI-powered recommendations tailored specifically to your habit patterns and goals.</p>
            </div>
            <button 
              className="btn btn-primary btn-large"
              onClick={() => setShowRecommendationsModal(true)}
            >
              <span>🤖</span>
              Get AI Recommendations
            </button>
          </div>
        </div>

        {/* AI Recommendations Modal */}
        <AiRecommendationsModal
          isOpen={showRecommendationsModal}
          onClose={() => setShowRecommendationsModal(false)}
          onHabitSelect={handleHabitSelect}
        />
      </div>
    </PageTransition>
  );
};

export default AiRecommendationsPage; 