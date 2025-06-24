import React, { useState, useEffect } from 'react';
import { Link, useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './Habits.css';

interface Habit {
  id: string;
  name: string;
  description: string;
  frequency: string;
  targetDays: number;
  currentStreak: number;
  longestStreak: number;
  totalCheckIns: number;
  createdAt: string;
  isActive: boolean;
}

interface HabitFormData {
  name: string;
  description: string;
  frequency: string;
  targetDays: number;
}

const Habits: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [habits, setHabits] = useState<Habit[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingHabit, setEditingHabit] = useState<Habit | null>(null);
  const [formData, setFormData] = useState<HabitFormData>({
    name: '',
    description: '',
    frequency: 'Daily',
    targetDays: 1
  });

  useEffect(() => {
    fetchHabits();
    if (id) {
      fetchHabitById(id);
    }
  }, [id]);

  const fetchHabits = async () => {
    try {
      // TODO: Replace with actual API call
      // const response = await fetch('/api/habits');
      // const data = await response.json();
      
      // Initialize with empty data
      setHabits([]);
    } catch (error) {
      console.error('Error fetching habits:', error);
    } finally {
      setLoading(false);
    }
  };

  const fetchHabitById = async (habitId: string) => {
    try {
      // TODO: Replace with actual API call
      // const response = await fetch(`/api/habits/${habitId}`);
      // const habit = await response.json();
      
      const habit = habits.find(h => h.id === habitId);
      if (habit) {
        setEditingHabit(habit);
        setFormData({
          name: habit.name,
          description: habit.description,
          frequency: habit.frequency,
          targetDays: habit.targetDays
        });
        setShowForm(true);
      }
    } catch (error) {
      console.error('Error fetching habit:', error);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'targetDays' ? parseInt(value) : value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      if (editingHabit) {
        // TODO: Replace with actual API call
        // await fetch(`/api/habits/${editingHabit.id}`, {
        //   method: 'PUT',
        //   headers: { 'Content-Type': 'application/json' },
        //   body: JSON.stringify(formData)
        // });
        
        // Update local state
        setHabits(prev => prev.map(habit => 
          habit.id === editingHabit.id 
            ? { ...habit, ...formData }
            : habit
        ));
      } else {
        // TODO: Replace with actual API call
        // const response = await fetch('/api/habits', {
        //   method: 'POST',
        //   headers: { 'Content-Type': 'application/json' },
        //   body: JSON.stringify(formData)
        // });
        // const newHabit = await response.json();
        
        // Add to local state
        const newHabit: Habit = {
          id: Date.now().toString(),
          ...formData,
          currentStreak: 0,
          longestStreak: 0,
          totalCheckIns: 0,
          createdAt: new Date().toISOString(),
          isActive: true
        };
        
        setHabits(prev => [...prev, newHabit]);
      }
      
      resetForm();
    } catch (error) {
      console.error('Error saving habit:', error);
    }
  };

  const handleDelete = async (habitId: string) => {
    if (!window.confirm('Are you sure you want to delete this habit?')) {
      return;
    }
    
    try {
      // TODO: Replace with actual API call
      // await fetch(`/api/habits/${habitId}`, { method: 'DELETE' });
      
      setHabits(prev => prev.filter(habit => habit.id !== habitId));
    } catch (error) {
      console.error('Error deleting habit:', error);
    }
  };

  const handleToggleActive = async (habitId: string) => {
    try {
      // TODO: Replace with actual API call
      // await fetch(`/api/habits/${habitId}/toggle`, { method: 'PATCH' });
      
      setHabits(prev => prev.map(habit => 
        habit.id === habitId 
          ? { ...habit, isActive: !habit.isActive }
          : habit
      ));
    } catch (error) {
      console.error('Error toggling habit:', error);
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      description: '',
      frequency: 'Daily',
      targetDays: 1
    });
    setEditingHabit(null);
    setShowForm(false);
    navigate('/habits');
  };

  if (loading) {
    return (
      <div className="habits-loading">
        <div className="spinner"></div>
        <p>Loading your habits...</p>
      </div>
    );
  }

  return (
    <div className="habits">
      <div className="habits-header">
        <h1>My Habits</h1>
        <button 
          className="btn-primary"
          onClick={() => setShowForm(true)}
        >
          ➕ Add New Habit
        </button>
      </div>

      {/* Habit Form */}
      {showForm && (
        <div className="habit-form-overlay">
          <div className="habit-form-container">
            <div className="habit-form-header">
              <h2>{editingHabit ? 'Edit Habit' : 'Create New Habit'}</h2>
              <button 
                className="close-btn"
                onClick={resetForm}
              >
                ✕
              </button>
            </div>
            
            <form onSubmit={handleSubmit} className="habit-form">
              <div className="form-group">
                <label htmlFor="name">Habit Name *</label>
                <input
                  type="text"
                  id="name"
                  name="name"
                  value={formData.name}
                  onChange={handleInputChange}
                  required
                  placeholder="e.g., Morning Exercise"
                />
              </div>
              
              <div className="form-group">
                <label htmlFor="description">Description</label>
                <textarea
                  id="description"
                  name="description"
                  value={formData.description}
                  onChange={handleInputChange}
                  placeholder="Describe your habit..."
                  rows={3}
                />
              </div>
              
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="frequency">Frequency *</label>
                  <select
                    id="frequency"
                    name="frequency"
                    value={formData.frequency}
                    onChange={handleInputChange}
                    required
                  >
                    <option value="Daily">Daily</option>
                    <option value="Weekly">Weekly</option>
                    <option value="Monthly">Monthly</option>
                  </select>
                </div>
                
                <div className="form-group">
                  <label htmlFor="targetDays">Target Days *</label>
                  <input
                    type="number"
                    id="targetDays"
                    name="targetDays"
                    value={formData.targetDays}
                    onChange={handleInputChange}
                    min="1"
                    max={formData.frequency === 'Daily' ? 1 : formData.frequency === 'Weekly' ? 7 : 31}
                    required
                  />
                </div>
              </div>
              
              <div className="form-actions">
                <button type="button" onClick={resetForm} className="btn-secondary">
                  Cancel
                </button>
                <button type="submit" className="btn-primary">
                  {editingHabit ? 'Update Habit' : 'Create Habit'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Habits List */}
      {habits.length === 0 ? (
        <div className="empty-state">
          <div className="empty-icon">🎯</div>
          <h3>No habits yet</h3>
          <p>Start building your habit chain by creating your first habit!</p>
          <button 
            className="btn-primary"
            onClick={() => setShowForm(true)}
          >
            Create First Habit
          </button>
        </div>
      ) : (
        <div className="habits-grid">
          {habits.map(habit => (
            <div key={habit.id} className={`habit-card ${!habit.isActive ? 'inactive' : ''}`}>
              <div className="habit-header">
                <div className="habit-title">
                  <h3>{habit.name}</h3>
                  <span className={`status-badge ${habit.isActive ? 'active' : 'inactive'}`}>
                    {habit.isActive ? 'Active' : 'Inactive'}
                  </span>
                </div>
                <div className="habit-menu">
                  <button 
                    className="menu-btn"
                    onClick={() => handleToggleActive(habit.id)}
                  >
                    {habit.isActive ? '⏸️' : '▶️'}
                  </button>
                  <button 
                    className="menu-btn"
                    onClick={() => {
                      setEditingHabit(habit);
                      setFormData({
                        name: habit.name,
                        description: habit.description,
                        frequency: habit.frequency,
                        targetDays: habit.targetDays
                      });
                      setShowForm(true);
                    }}
                  >
                    ✏️
                  </button>
                  <button 
                    className="menu-btn delete"
                    onClick={() => handleDelete(habit.id)}
                  >
                    🗑️
                  </button>
                </div>
              </div>
              
              <p className="habit-description">{habit.description}</p>
              
              <div className="habit-meta">
                <span className="frequency-badge">{habit.frequency}</span>
                <span className="target-badge">{habit.targetDays} day{habit.targetDays > 1 ? 's' : ''}</span>
              </div>
              
              <div className="habit-stats">
                <div className="stat-item">
                  <span className="stat-label">Current Streak</span>
                  <span className="stat-value streak">🔥 {habit.currentStreak}</span>
                </div>
                <div className="stat-item">
                  <span className="stat-label">Longest Streak</span>
                  <span className="stat-value longest">🏆 {habit.longestStreak}</span>
                </div>
                <div className="stat-item">
                  <span className="stat-label">Total Check-ins</span>
                  <span className="stat-value total">✅ {habit.totalCheckIns}</span>
                </div>
              </div>
              
              <div className="habit-actions">
                <Link to={`/check-in?habit=${habit.id}`} className="btn-checkin">
                  Check-in
                </Link>
                <Link to={`/habits/${habit.id}`} className="btn-view">
                  View Details
                </Link>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Habits; 