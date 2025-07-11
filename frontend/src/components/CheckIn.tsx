import React, { useState, useEffect, useCallback } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useDashboard } from '../contexts/DashboardContext';
import './CheckIn.css';
import { ApiService } from '../services/api';
import { CheckInsService } from '../services/checkInsService';

interface Habit {
  id: string;
  name: string;
  description: string;
  frequency: string;
  targetDays?: number;
  currentStreak: number;
  longestStreak: number;
  lastCheckIn?: string;
  totalCheckIns?: number;
}

interface CheckInData {
  userId: string;
  habitId: string;
  completedAt: string;
  notes?: string;
  streakDay: number;
  isManualEntry: boolean;
}

const CheckIn: React.FC = () => {
  const { user } = useAuth();
  const { refreshDashboard, updateHabitCompletion } = useDashboard();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [habits, setHabits] = useState<Habit[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedHabit, setSelectedHabit] = useState<string>('');
  const [checkInData, setCheckInData] = useState<CheckInData>({
    userId: '',
    habitId: '',
    completedAt: new Date().toISOString(),
    notes: '',
    streakDay: 1,
    isManualEntry: true
  });
  const [submitting, setSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);

  // Convert backend enum values to frontend string values
  const convertFrequencyToString = (frequency: number): string => {
    const frequencyMap: { [key: number]: string } = {
      1: 'Daily',
      2: 'Weekly', 
      3: 'Monthly',
      4: 'Custom'
    };
    return frequencyMap[frequency] || 'Daily';
  };

  const fetchHabits = useCallback(async () => {
    try {
      if (!user?.id) {
        setHabits([]);
        return;
      }
      
      const data = await ApiService.get<any[]>(`/habits/user/${user.id}/active`);
      
      // Convert frequency enum values to strings for frontend
      const convertedHabits = data.map(habit => ({
        ...habit,
        frequency: convertFrequencyToString(habit.frequency)
      }));
      
      setHabits(convertedHabits);
    } catch (error) {
      console.error('Error fetching active habits:', error);
      setHabits([]);
    } finally {
      setLoading(false);
    }
  }, [user?.id]);

  useEffect(() => {
    fetchHabits();
    const habitParam = searchParams.get('habit');
    if (habitParam) {
      setSelectedHabit(habitParam);
      setCheckInData(prev => ({ ...prev, habitId: habitParam }));
    }
  }, [searchParams, fetchHabits]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    setCheckInData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? (e.target as HTMLInputElement).checked : value
    }));
  };

  const handleHabitSelect = (habitId: string) => {
    setSelectedHabit(habitId);
    setCheckInData(prev => ({ 
      ...prev, 
      habitId,
      userId: user?.id || '',
      completedAt: new Date().toISOString()
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!checkInData.habitId) {
      alert('Please select a habit to check in');
      return;
    }
    
    // Validate that habitId is a valid GUID
    const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
    if (!guidRegex.test(checkInData.habitId)) {
      console.error('Invalid habitId format:', checkInData.habitId);
      alert('Invalid habit ID format. Please try again.');
      return;
    }
    
    setSubmitting(true);
    
    try {
      // Prepare the check-in data with proper structure for backend
      const checkInPayload = {
        habitId: checkInData.habitId, // This should be a valid GUID string
        completedAt: new Date(Date.now() - 60000).toISOString(), // Use 1 minute ago to avoid timezone issues
        notes: checkInData.notes || '',
        streakDay: checkInData.streakDay || 1, // Ensure it's at least 1
        isManualEntry: checkInData.isManualEntry
      };

      console.log('Submitting check-in payload:', checkInPayload);
      console.log('Payload habitId type:', typeof checkInPayload.habitId);
      console.log('Payload habitId value:', checkInPayload.habitId);
      console.log('Payload completedAt type:', typeof checkInPayload.completedAt);
      console.log('Payload completedAt value:', checkInPayload.completedAt);
      console.log('Payload streakDay type:', typeof checkInPayload.streakDay);
      console.log('Payload streakDay value:', checkInPayload.streakDay);

      // Submit check-in to API using CheckInsService
      const createdCheckIn = await CheckInsService.createCheckIn(checkInPayload);
      
      // Update dashboard immediately for better UX using the actual streak from backend
      const selectedHabitData = habits.find(h => h.id === checkInData.habitId);
      if (selectedHabitData) {
        const actualStreak = createdCheckIn.streakDay;
        
        // Use the new updateHabitCompletion method for immediate dashboard updates
        updateHabitCompletion(checkInData.habitId, createdCheckIn.completedAt, actualStreak);
        
        // Also update the local habits state for immediate UI feedback
        setHabits(prevHabits => 
          prevHabits.map(habit => 
            habit.id === checkInData.habitId 
              ? { 
                  ...habit, 
                  currentStreak: actualStreak,
                  longestStreak: Math.max(habit.longestStreak, actualStreak),
                  totalCheckIns: (habit.totalCheckIns || 0) + 1,
                  lastCheckIn: createdCheckIn.completedAt
                }
              : habit
          )
        );
      }
      
      setSuccess(true);
      
      // Refresh dashboard data to ensure everything is in sync with backend
      // Use a shorter delay for more responsive updates
      setTimeout(async () => {
        await refreshDashboard();
      }, 300);
      
      // Reset form after success
      setTimeout(() => {
        setCheckInData({
          userId: '',
          habitId: '',
          completedAt: new Date().toISOString(),
          notes: '',
          streakDay: 1,
          isManualEntry: true
        });
        setSelectedHabit('');
        setSuccess(false);
        navigate('/dashboard');
      }, 2000);
      
    } catch (error: any) {
      console.error('Error submitting check-in:', error);
      
      // Show detailed error information
      if (error.response?.data) {
        console.error('Full API Error Response:', JSON.stringify(error.response.data, null, 2));
        
        // If there are validation errors, show them
        if (error.response.data.errors) {
          console.error('Validation Errors:', error.response.data.errors);
          const errorMessages = Object.entries(error.response.data.errors)
            .map(([field, messages]) => `${field}: ${Array.isArray(messages) ? messages.join(', ') : messages}`)
            .join('\n');
          alert(`Validation Errors:\n${errorMessages}`);
        } else {
          alert(`Failed to submit check-in: ${error.response.data.title || error.response.data.message || 'Unknown error'}`);
        }
      } else {
        alert('Failed to submit check-in. Please try again.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  const getTodayDate = () => {
    return new Date().toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  const getLastCheckInDate = (habit: Habit) => {
    if (!habit.lastCheckIn) return 'Never';
    return new Date(habit.lastCheckIn).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  if (loading) {
    return (
      <div className="checkin-loading">
        <div className="spinner"></div>
        <p>Loading your habits...</p>
      </div>
    );
  }

  return (
    <div className="checkin">
      <div className="checkin-header">
        <h1>Daily Check-in</h1>
        <p className="checkin-date">{getTodayDate()}</p>
      </div>

      {success && (
        <div className="success-message">
          <div className="success-icon">✅</div>
          <h3>Check-in Successful!</h3>
          <p>Great job! Your habit has been recorded.</p>
        </div>
      )}

      <div className="checkin-container">
        <div className="habits-selection">
          <h2>Select a Habit</h2>
          <div className="habits-grid">
            {habits.map(habit => (
              <div 
                key={habit.id} 
                className={`habit-option ${selectedHabit === habit.id ? 'selected' : ''}`}
                onClick={() => handleHabitSelect(habit.id)}
              >
                <div className="habit-info">
                  <h3>{habit.name}</h3>
                  <p>{habit.description}</p>
                  <div className="habit-stats">
                    <span className="streak">🔥 {habit.currentStreak} day streak</span>
                    <span className="last-checkin">Last: {getLastCheckInDate(habit)}</span>
                  </div>
                </div>
                <div className="habit-selector">
                  <div className={`radio-button ${selectedHabit === habit.id ? 'selected' : ''}`}>
                    {selectedHabit === habit.id && <div className="radio-dot"></div>}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>

        {selectedHabit && (
          <form onSubmit={handleSubmit} className="checkin-form">
            <h2>Check-in Details</h2>
            
            <div className="form-group">
              <label className="checkbox-label">
                <input
                  type="checkbox"
                  name="isManualEntry"
                  checked={checkInData.isManualEntry}
                  onChange={handleInputChange}
                />
                <span className="checkmark"></span>
                This is a manual entry
              </label>
            </div>

            <div className="form-group">
              <label htmlFor="notes">Notes (optional)</label>
              <textarea
                id="notes"
                name="notes"
                value={checkInData.notes}
                onChange={handleInputChange}
                placeholder="How did it go? Any challenges or victories?"
                rows={4}
              />
            </div>

            <div className="form-actions">
              <button 
                type="button" 
                onClick={() => navigate('/dashboard')}
                className="btn-secondary"
                disabled={submitting}
              >
                Cancel
              </button>
              <button 
                type="submit" 
                className="btn-primary"
                disabled={submitting}
              >
                {submitting ? 'Submitting...' : 'Submit Check-in'}
              </button>
            </div>
          </form>
        )}

        {!selectedHabit && habits.length > 0 && (
          <div className="checkin-prompt">
            <div className="prompt-icon">📝</div>
            <h3>Select a habit above to check in</h3>
            <p>Choose the habit you'd like to record for today</p>
          </div>
        )}
      </div>

      {/* Quick Stats */}
      <div className="quick-stats">
        <h2>Today's Progress</h2>
        <div className="stats-grid">
          <div className="stat-card">
            <div className="stat-icon">📊</div>
            <div className="stat-content">
              <h3>{habits.length}</h3>
              <p>Total Habits</p>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">✅</div>
            <div className="stat-content">
              <h3>{habits.filter(h => h.lastCheckIn && new Date(h.lastCheckIn).toDateString() === new Date().toDateString()).length}</h3>
              <p>Completed Today</p>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">🔥</div>
            <div className="stat-content">
              <h3>{Math.max(...habits.map(h => h.currentStreak))}</h3>
              <p>Longest Active Streak</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CheckIn; 