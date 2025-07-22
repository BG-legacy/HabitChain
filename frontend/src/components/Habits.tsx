import React, { useState, useEffect, useCallback } from 'react';
import { Link, useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useDashboard } from '../contexts/DashboardContext';
import { ApiService } from '../services/api';
import CompleteHabitButton from './CompleteHabitButton';
import CheckInButton from './CheckInButton';
import './Habits.css';
import HabitCard from './HabitCard';

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
  lastCompletedAt?: string;
}

interface HabitFormData {
  name: string;
  description: string;
  frequency: string;
  targetDays: number;
}

const Habits: React.FC = () => {
  const { user } = useAuth();
  const { refreshDashboard } = useDashboard();
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
      
      // Use ApiService instead of direct fetch
      const data = await ApiService.get<any[]>(`/habits/user/${user.id}`);
      
      // Convert frequency enum values to strings for frontend
      const convertedHabits = data.map(habit => ({
        ...habit,
        frequency: convertFrequencyToString(habit.frequency),
        targetDays: 1 // Default value since backend doesn't have this field
      }));
      
      // Deduplicate habits by ID to prevent duplicate key errors
      const uniqueHabits = convertedHabits.filter((habit, index, array) => 
        array.findIndex(h => h.id === habit.id) === index
      );
      
      setHabits(uniqueHabits);
    } catch (error) {
      console.error('Error fetching habits:', error);
      setHabits([]);
    } finally {
      setLoading(false);
    }
  }, [user?.id]);

  const fetchHabitById = useCallback(async (habitId: string) => {
    try {
      const habit = await ApiService.get<any>(`/habits/${habitId}`);
      
      // Convert frequency enum to string for frontend
      const convertedHabit = {
        ...habit,
        frequency: convertFrequencyToString(habit.frequency),
        targetDays: 1 // Default value since backend doesn't have this field
      };
      
      setEditingHabit(convertedHabit);
      setFormData({
        name: convertedHabit.name,
        description: convertedHabit.description,
        frequency: convertedHabit.frequency,
        targetDays: convertedHabit.targetDays
      });
      setShowForm(true);
    } catch (error) {
      console.error('Error fetching habit:', error);
      // Fallback to local data - use a ref to avoid dependency issues
      const currentHabits = habits;
      const habit = currentHabits.find(h => h.id === habitId);
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
    }
  }, []); // Remove habits dependency to prevent infinite loop

  useEffect(() => {
    fetchHabits();
  }, [fetchHabits]);

  // Separate useEffect for fetching habit by ID to avoid circular dependencies
  useEffect(() => {
    if (id && user?.id) {
      fetchHabitById(id);
    }
  }, [id, user?.id]); // Only depend on id and user.id, not the functions

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'targetDays' ? (value === '' ? 1 : parseInt(value) || 1) : value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      // Check if user is authenticated
      console.log('Current user:', user);
      console.log('Access token:', localStorage.getItem('accessToken') ? 'Present' : 'Missing');
      
      // Basic frontend validation to help debug
      if (!formData.name || formData.name.length < 3) {
        alert('Habit name must be at least 3 characters long');
        return;
      }
      
      if (formData.name.length > 100) {
        alert('Habit name cannot exceed 100 characters');
        return;
      }
      
      // Check for potentially problematic characters
      const namePattern = /^[a-zA-Z0-9\s\-_]+$/;
      if (!namePattern.test(formData.name)) {
        alert('Habit name can only contain letters, numbers, spaces, hyphens, and underscores. Please check for special characters.');
        return;
      }
      
      if (formData.description && formData.description.length > 500) {
        alert('Description cannot exceed 500 characters');
        return;
      }
      
      // Convert string frequency to enum value
      const frequencyMap: { [key: string]: number } = {
        'Daily': 1,
        'Weekly': 2,
        'Monthly': 3,
        'Custom': 4
      };

      // Prepare data for API - match CreateHabitDto structure
      const habitData = {
        name: formData.name.trim(),
        description: formData.description?.trim() || '',
        frequency: frequencyMap[formData.frequency] || 1, // Default to Daily if not found
        // UserId will be set automatically from JWT token in the backend
        // Don't include targetDays as it's not part of CreateHabitDto
      };

      console.log('Submitting habit data:', habitData); // Debug log

      if (editingHabit) {
        // Update existing habit
        const updatedHabit = await ApiService.put<any>(`/habits/${editingHabit.id}`, habitData);
        
        // Convert the response back to frontend format
        const convertedHabit = {
          ...updatedHabit,
          frequency: convertFrequencyToString(updatedHabit.frequency),
          targetDays: 1
        };
        
        setHabits(prev => prev.map(habit => 
          habit.id === editingHabit.id ? convertedHabit : habit
        ));
        
        // Refresh the dashboard to update the dashboard state
        await refreshDashboard();
      } else {
        // Create new habit
        const newHabit = await ApiService.post<any>('/habits', habitData);
        
        // Convert the response back to frontend format
        const convertedHabit = {
          ...newHabit,
          frequency: convertFrequencyToString(newHabit.frequency),
          targetDays: 1
        };
        
        setHabits(prev => [...prev, convertedHabit]);
        
        // Refresh the dashboard to update the dashboard state
        await refreshDashboard();
      }
      
      resetForm();
    } catch (error: any) {
      console.error('Error saving habit:', error);
      
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
          alert(`Failed to save habit: ${error.response.data.title || error.response.data.message || 'Unknown error'}`);
        }
      } else {
        alert('Failed to save habit. Please try again.');
      }
    }
  };

  const handleDelete = async (habitId: string) => {
    if (!window.confirm('Are you sure you want to delete this habit?')) {
      return;
    }
    
    try {
      await ApiService.delete(`/habits/${habitId}`);
      setHabits(prev => prev.filter(habit => habit.id !== habitId));
      
      // Refresh the dashboard to update the dashboard state
      await refreshDashboard();
    } catch (error) {
      console.error('Error deleting habit:', error);
      alert('Failed to delete habit. Please try again.');
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
            <HabitCard
              key={habit.id}
              habit={{
                ...habit,
                targetDays: habit.targetDays ?? 1,
                createdAt: habit.createdAt || new Date().toISOString(),
              }}
              onToggleActive={handleToggleActive}
              onDelete={handleDelete}
              onEdit={(id) => {
                setEditingHabit(habit);
                setFormData({
                  name: habit.name,
                  description: habit.description,
                  frequency: habit.frequency,
                  targetDays: habit.targetDays
                });
                setShowForm(true);
              }}
              showActions={true}
            />
          ))}
        </div>
      )}
    </div>
  );
};

export default Habits; 