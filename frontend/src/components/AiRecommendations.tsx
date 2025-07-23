import React, { useState, useEffect } from 'react';
import Modal, { ModalFooter, ModalActionButton } from './Modal';
import AiRecommendationsService, { HabitRecommendation } from '../services/aiRecommendationsService';
import { useToast } from '../hooks/useToast';
import './AiRecommendations.css';

interface AiRecommendationsProps {
  isOpen: boolean;
  onClose: () => void;
  onHabitSelect?: (recommendation: HabitRecommendation) => void;
  habitId?: string; // For complementary habits
}

const AiRecommendations: React.FC<AiRecommendationsProps> = ({
  isOpen,
  onClose,
  onHabitSelect,
  habitId
}) => {
  const [recommendations, setRecommendations] = useState<HabitRecommendation[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedRecommendation, setSelectedRecommendation] = useState<HabitRecommendation | null>(null);
  const [creatingHabit, setCreatingHabit] = useState(false);
  const { showError, showSuccess } = useToast();

  useEffect(() => {
    if (isOpen) {
      fetchRecommendations();
    }
  }, [isOpen, habitId]);

  const fetchRecommendations = async () => {
    setLoading(true);
    try {
      let data: HabitRecommendation[];
      
      if (habitId) {
        data = await AiRecommendationsService.getComplementaryHabits(habitId);
      } else {
        data = await AiRecommendationsService.getHabitRecommendations();
      }
      
      setRecommendations(data);
    } catch (error) {
      console.error('Error fetching recommendations:', error);
      showError('Failed to load recommendations. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleRecommendationSelect = (recommendation: HabitRecommendation) => {
    setSelectedRecommendation(recommendation);
  };

  const handleCreateHabit = async () => {
    if (!selectedRecommendation) return;

    setCreatingHabit(true);
    try {
      // Create the habit from the recommendation
      const createdHabit = await AiRecommendationsService.createHabitFromRecommendation(selectedRecommendation);
      
      showSuccess(`🎉 "${selectedRecommendation.name}" has been added to your habits!`);
      
      // Call the onHabitSelect callback if provided
      if (onHabitSelect) {
        onHabitSelect(selectedRecommendation);
      }
      
      onClose();
    } catch (error) {
      console.error('Error creating habit:', error);
      showError('Failed to create habit. Please try again.');
    } finally {
      setCreatingHabit(false);
    }
  };

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty.toLowerCase()) {
      case 'easy': return '#28a745';
      case 'medium': return '#ffc107';
      case 'hard': return '#dc3545';
      default: return '#6c757d';
    }
  };

  const getCategoryIcon = (category: string) => {
    switch (category.toLowerCase()) {
      case 'fitness': return '💪';
      case 'health': return '🏥';
      case 'learning': return '📚';
      case 'wellness': return '🧘';
      case 'sleep': return '😴';
      case 'reflection': return '🤔';
      case 'organization': return '📋';
      case 'productivity': return '⚡';
      case 'social': return '👥';
      case 'creativity': return '🎨';
      case 'finance': return '💰';
      case 'nutrition': return '🍎';
      case 'career': return '💼';
      case 'hobbies': return '🎮';
      case 'spirituality': return '🙏';
      case 'relationships': return '💕';
      default: return '🎯';
    }
  };

  const getConfidenceColor = (confidence: number) => {
    if (confidence >= 0.8) return '#28a745';
    if (confidence >= 0.6) return '#ffc107';
    return '#dc3545';
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={habitId ? "Complementary Habits" : "AI Habit Recommendations"}
      size="large"
    >
      <div className="ai-recommendations">
        {loading ? (
          <div className="recommendations-loading">
            <div className="spinner"></div>
            <p>🤖 AI is analyzing your habits and generating personalized recommendations...</p>
          </div>
        ) : (
          <>
            <div className="recommendations-header">
              <p className="recommendations-subtitle">
                {habitId 
                  ? "Here are some habits that would work well with your current routine:"
                  : "Based on your habit patterns, here are some suggestions to enhance your routine:"
                }
              </p>
            </div>

            <div className="recommendations-grid">
              {recommendations.map((recommendation, index) => (
                <div
                  key={index}
                  className={`recommendation-card ${selectedRecommendation?.name === recommendation.name ? 'selected' : ''}`}
                  onClick={() => handleRecommendationSelect(recommendation)}
                >
                  <div className="recommendation-header">
                    <div className="recommendation-icon">
                      {getCategoryIcon(recommendation.category)}
                    </div>
                    <div className="recommendation-meta">
                      <h3>{recommendation.name}</h3>
                      <div className="recommendation-tags">
                        <span 
                          className="difficulty-tag"
                          style={{ backgroundColor: getDifficultyColor(recommendation.difficulty) }}
                        >
                          {recommendation.difficulty}
                        </span>
                        <span className="category-tag">
                          {recommendation.category}
                        </span>
                        <span 
                          className="confidence-tag"
                          style={{ backgroundColor: getConfidenceColor(recommendation.confidence) }}
                        >
                          {Math.round(recommendation.confidence * 100)}% match
                        </span>
                      </div>
                    </div>
                  </div>

                  <p className="recommendation-description">
                    {recommendation.description}
                  </p>

                  <div className="recommendation-reasoning">
                    <strong>Why this habit?</strong>
                    <p>{recommendation.reasoning}</p>
                  </div>

                  <div className="recommendation-details">
                    <div className="detail-item">
                      <span className="detail-label">Frequency:</span>
                      <span className="detail-value">{recommendation.frequency}</span>
                    </div>
                    <div className="detail-item">
                      <span className="detail-label">Best Time:</span>
                      <span className="detail-value">{recommendation.suggestedTime}</span>
                    </div>
                    {recommendation.relatedHabits.length > 0 && (
                      <div className="detail-item">
                        <span className="detail-label">Works well with:</span>
                        <span className="detail-value">
                          {recommendation.relatedHabits.join(', ')}
                        </span>
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>

            {selectedRecommendation && (
              <div className="selected-recommendation">
                <h4>Ready to add this habit?</h4>
                <p>
                  <strong>{selectedRecommendation.name}</strong> - {selectedRecommendation.description}
                </p>
              </div>
            )}
          </>
        )}
      </div>

      <ModalFooter>
        <ModalActionButton variant="secondary" onClick={onClose}>
          Cancel
        </ModalActionButton>
        {selectedRecommendation && (
          <ModalActionButton 
            variant="primary" 
            onClick={handleCreateHabit}
            disabled={creatingHabit}
          >
            {creatingHabit ? 'Creating Habit...' : 'Add This Habit'}
          </ModalActionButton>
        )}
      </ModalFooter>
    </Modal>
  );
};

export default AiRecommendations; 