import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './CheckIn.css';
import { ApiService } from '../services/api';

interface Habit {
  id: string;
  name: string;
  description: string;
  frequency: string;
  targetDays?: number;
  currentStreak: number;
  longestStreak: number;
  lastCheckIn?: string;
}

interface CheckInData {
  habitId: string;
  notes: string;
  mood: string;
  completed: boolean;
}

const CheckIn: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [habits, setHabits] = useState<Habit[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedHabit, setSelectedHabit] = useState<string>('');
  const [checkInData, setCheckInData] = useState<CheckInData>({
    habitId: '',
    notes: '',
    mood: 'good',
    completed: true
  });
  const [submitting, setSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    fetchHabits();
    const habitParam = searchParams.get('habit');
    if (habitParam) {
      setSelectedHabit(habitParam);
      setCheckInData(prev => ({ ...prev, habitId: habitParam }));
    }
  }, [searchParams]);

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

  const fetchHabits = async () => {
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
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    setCheckInData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? (e.target as HTMLInputElement).checked : value
    }));
  };

  const handleHabitSelect = (habitId: string) => {
    setSelectedHabit(habitId);
    setCheckInData(prev => ({ ...prev, habitId }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!checkInData.habitId) {
      alert('Please select a habit to check in');
      return;
    }
    
    setSubmitting(true);
    
    try {
      // Submit check-in to API
      const response = await fetch('/api/check-ins', {
        method: 'POST',
        headers: { 
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`,
          'Content-Type': 'application/json' 
        },
        body: JSON.stringify(checkInData)
      });
      
      if (response.ok) {
        setSuccess(true);
      } else {
        throw new Error('Failed to submit check-in');
      }
      
      // Reset form after success
      setTimeout(() => {
        setCheckInData({
          habitId: '',
          notes: '',
          mood: 'good',
          completed: true
        });
        setSelectedHabit('');
        setSuccess(false);
        navigate('/dashboard');
      }, 2000);
      
    } catch (error) {
      console.error('Error submitting check-in:', error);
      alert('Failed to submit check-in. Please try again.');
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
                  name="completed"
                  checked={checkInData.completed}
                  onChange={handleInputChange}
                />
                <span className="checkmark"></span>
                I completed this habit today
              </label>
            </div>

            <div className="form-group">
              <label htmlFor="mood">How are you feeling?</label>
              <select
                id="mood"
                name="mood"
                value={checkInData.mood}
                onChange={handleInputChange}
              >
                <option value="excellent">😊 Excellent</option>
                <option value="good">🙂 Good</option>
                <option value="okay">😐 Okay</option>
                <option value="tough">😔 Tough</option>
                <option value="struggling">😞 Struggling</option>
              </select>
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
              <h3>0</h3>
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