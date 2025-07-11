import React, { useState, useEffect, useCallback } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { ApiService } from '../services/api';
import CompleteHabitButton from './CompleteHabitButton';
import CheckInButton from './CheckInButton';
import './Calendar.css';

interface CheckIn {
  id: string;
  habitId: string;
  habitName: string;
  completedAt: string;
  notes?: string;
  streakDay: number;
}

interface Habit {
  id: string;
  name: string;
  description: string;
  frequency: string;
  isActive: boolean;
  color?: string;
  currentStreak: number;
  longestStreak: number;
}

const Calendar: React.FC = () => {
  const { user } = useAuth();
  const [checkIns, setCheckIns] = useState<CheckIn[]>([]);
  const [habits, setHabits] = useState<Habit[]>([]);
  const [loading, setLoading] = useState(true);
  const [currentDate, setCurrentDate] = useState(new Date());
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);
  const [viewMode, setViewMode] = useState<'month' | 'week'>('month');

  const fetchData = useCallback(async () => {
    if (!user?.id) return;
    
    try {
      setLoading(true);
      // Fetch habits and check-ins in parallel
      const [habitsResponse, checkInsResponse] = await Promise.all([
        ApiService.get<any[]>(`/habits/user/${user.id}`),
        ApiService.get<any[]>(`/check-ins/user/${user.id}`)
      ]);
      
      // Convert habits with frequency enum to string
      const convertedHabits = habitsResponse.map(habit => ({
        ...habit,
        frequency: convertFrequencyToString(habit.frequency),
        color: habit.color || getDefaultColor(habit.name)
      }));
      
      // Convert check-ins with habit name
      const checkInsWithHabitName = checkInsResponse.map(checkIn => {
        const habit = convertedHabits.find(h => h.id === checkIn.habitId);
        return {
          ...checkIn,
          habitName: habit?.name || 'Unknown Habit'
        };
      });
      
      setHabits(convertedHabits);
      setCheckIns(checkInsWithHabitName);
    } catch (error) {
      console.error('Error fetching calendar data:', error);
      setHabits([]);
      setCheckIns([]);
    } finally {
      setLoading(false);
    }
  }, [user?.id]);

  useEffect(() => {
    if (user?.id) {
      fetchData();
    }
  }, [fetchData, currentDate]);

  const convertFrequencyToString = (frequency: number): string => {
    const frequencyMap: { [key: number]: string } = {
      1: 'Daily',
      2: 'Weekly', 
      3: 'Monthly',
      4: 'Custom'
    };
    return frequencyMap[frequency] || 'Daily';
  };

  const getDefaultColor = (habitName: string): string => {
    const colors = ['#667eea', '#764ba2', '#f093fb', '#f5576c', '#4facfe', '#00f2fe', '#43e97b', '#38f9d7'];
    const index = habitName.length % colors.length;
    return colors[index];
  };

  const getDaysInMonth = (date: Date) => {
    const year = date.getFullYear();
    const month = date.getMonth();
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const daysInMonth = lastDay.getDate();
    const startingDayOfWeek = firstDay.getDay();
    
    return { daysInMonth, startingDayOfWeek };
  };

  const getDaysInWeek = (date: Date) => {
    const startOfWeek = new Date(date);
    startOfWeek.setDate(date.getDate() - date.getDay());
    
    const days = [];
    for (let i = 0; i < 7; i++) {
      const day = new Date(startOfWeek);
      day.setDate(startOfWeek.getDate() + i);
      days.push(day);
    }
    
    return days;
  };

  const formatDate = (date: Date) => {
    return date.toISOString().split('T')[0];
  };

  const getCheckInsForDate = (date: string) => {
    return checkIns.filter(checkIn => {
      const checkInDate = new Date(checkIn.completedAt).toISOString().split('T')[0];
      return checkInDate === date;
    });
  };

  const getHabitsForDate = (date: string) => {
    const dayCheckIns = getCheckInsForDate(date);
    const completedHabitIds = dayCheckIns.map(checkIn => checkIn.habitId);
    
    return {
      completed: habits.filter(habit => completedHabitIds.includes(habit.id)),
      pending: habits.filter(habit => habit.isActive && !completedHabitIds.includes(habit.id))
    };
  };



  const navigateMonth = (direction: 'prev' | 'next') => {
    const newDate = new Date(currentDate);
    if (direction === 'prev') {
      newDate.setMonth(newDate.getMonth() - 1);
    } else {
      newDate.setMonth(newDate.getMonth() + 1);
    }
    setCurrentDate(newDate);
  };

  const navigateWeek = (direction: 'prev' | 'next') => {
    const newDate = new Date(currentDate);
    if (direction === 'prev') {
      newDate.setDate(newDate.getDate() - 7);
    } else {
      newDate.setDate(newDate.getDate() + 7);
    }
    setCurrentDate(newDate);
  };

  if (loading) {
    return (
      <div className="calendar-loading">
        <div className="spinner"></div>
        <p>Loading calendar...</p>
      </div>
    );
  }

  return (
    <div className="calendar">
      <div className="calendar-header">
        <h1>Habit Calendar</h1>
        <div className="calendar-controls">
          <div className="view-toggle">
            <button 
              className={`view-btn ${viewMode === 'month' ? 'active' : ''}`}
              onClick={() => setViewMode('month')}
            >
              📅 Month
            </button>
            <button 
              className={`view-btn ${viewMode === 'week' ? 'active' : ''}`}
              onClick={() => setViewMode('week')}
            >
              📊 Week
            </button>
          </div>
          <div className="navigation">
            <button 
              className="nav-btn"
              onClick={() => viewMode === 'month' ? navigateMonth('prev') : navigateWeek('prev')}
            >
              ◀
            </button>
            <span className="current-period">
              {viewMode === 'month' 
                ? currentDate.toLocaleDateString('en-US', { month: 'long', year: 'numeric' })
                : `${getDaysInWeek(currentDate)[0].toLocaleDateString()} - ${getDaysInWeek(currentDate)[6].toLocaleDateString()}`
              }
            </span>
            <button 
              className="nav-btn"
              onClick={() => viewMode === 'month' ? navigateMonth('next') : navigateWeek('next')}
            >
              ▶
            </button>
          </div>
        </div>
      </div>

      {/* Calendar Grid */}
      <div className="calendar-container">
        {viewMode === 'month' ? (
          <div className="month-view">
            {/* Day Headers */}
            <div className="calendar-grid-header">
              {['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].map(day => (
                <div key={day} className="day-header">{day}</div>
              ))}
            </div>
            
            {/* Calendar Days */}
            <div className="calendar-grid">
              {(() => {
                const { daysInMonth, startingDayOfWeek } = getDaysInMonth(currentDate);
                const days = [];
                
                // Add empty cells for days before the first day of the month
                for (let i = 0; i < startingDayOfWeek; i++) {
                  days.push(<div key={`empty-${i}`} className="calendar-day empty"></div>);
                }
                
                // Add days of the month
                for (let day = 1; day <= daysInMonth; day++) {
                  const date = new Date(currentDate.getFullYear(), currentDate.getMonth(), day);
                  const dateString = formatDate(date);
                  const dayHabits = getHabitsForDate(dateString);
                  const isToday = formatDate(new Date()) === dateString;
                  const isSelected = selectedDate && formatDate(selectedDate) === dateString;
                  
                  days.push(
                    <div 
                      key={day} 
                      className={`calendar-day ${isToday ? 'today' : ''} ${isSelected ? 'selected' : ''}`}
                      onClick={() => setSelectedDate(date)}
                    >
                      <div className="day-number">{day}</div>
                      <div className="habits-indicators">
                        {/* Show completed habits */}
                        {dayHabits.completed.map(habit => (
                          <div 
                            key={`completed-${habit.id}`}
                            className="habit-dot completed"
                            style={{ backgroundColor: habit.color }}
                            title={`✅ ${habit.name} - Completed`}
                          >
                            ✅
                          </div>
                        ))}
                        {/* Show pending habits for today */}
                        {isToday && dayHabits.pending.slice(0, 3).map(habit => (
                          <div 
                            key={`pending-${habit.id}`}
                            className="habit-dot pending"
                            style={{ backgroundColor: habit.color }}
                            title={`⏳ ${habit.name} - Pending`}
                          >
                            ⏳
                          </div>
                        ))}
                        {isToday && dayHabits.pending.length > 3 && (
                          <div className="habit-dot more" title={`+${dayHabits.pending.length - 3} more habits`}>
                            +{dayHabits.pending.length - 3}
                          </div>
                        )}
                      </div>
                    </div>
                  );
                }
                
                return days;
              })()}
            </div>
          </div>
        ) : (
          <div className="week-view">
            {/* Day Headers */}
            <div className="week-grid-header">
              {getDaysInWeek(currentDate).map(day => (
                <div key={day.toISOString()} className="week-day-header">
                  <div className="day-name">{day.toLocaleDateString('en-US', { weekday: 'short' })}</div>
                  <div className="day-date">{day.getDate()}</div>
                </div>
              ))}
            </div>
            
            {/* Week Days */}
            <div className="week-grid">
              {getDaysInWeek(currentDate).map(day => {
                const dateString = formatDate(day);
                const dayHabits = getHabitsForDate(dateString);
                const isToday = formatDate(new Date()) === dateString;
                
                return (
                  <div key={dateString} className={`week-day ${isToday ? 'today' : ''}`}>
                    {/* Completed habits */}
                    {dayHabits.completed.map(habit => (
                      <div 
                        key={`week-completed-${habit.id}`}
                        className="week-habit completed"
                        style={{ borderColor: habit.color }}
                      >
                        <div className="habit-header">
                          <span className="habit-name">✅ {habit.name}</span>
                          <span className="habit-streak">🔥 {habit.currentStreak}</span>
                        </div>
                      </div>
                    ))}
                    
                    {/* Pending habits for today */}
                    {isToday && dayHabits.pending.map(habit => (
                      <div 
                        key={`week-pending-${habit.id}`}
                        className="week-habit pending"
                        style={{ borderColor: habit.color }}
                      >
                        <div className="habit-header">
                          <span className="habit-name">⏳ {habit.name}</span>
                          <div className="habit-actions">
                            <CompleteHabitButton 
                              habitId={habit.id}
                              habitName={habit.name}
                              size="small"
                              variant="minimal"
                              onComplete={fetchData}
                            />
                            <CheckInButton 
                              habitId={habit.id}
                              habitName={habit.name}
                              size="small"
                              variant="detailed"
                              showModal={true}
                              onCheckIn={fetchData}
                            />
                          </div>
                        </div>
                      </div>
                    ))}
                    
                    {dayHabits.completed.length === 0 && (!isToday || dayHabits.pending.length === 0) && (
                      <div className="no-habits">
                        {isToday ? 'All habits completed! 🎉' : 'No habits'}
                      </div>
                    )}
                  </div>
                );
              })}
            </div>
          </div>
        )}
      </div>

      {/* Selected Date Details */}
      {selectedDate && (
        <div className="date-details">
          <div className="details-header">
            <h3>{selectedDate.toLocaleDateString('en-US', { 
              weekday: 'long', 
              year: 'numeric', 
              month: 'long', 
              day: 'numeric' 
            })}</h3>
            <button 
              className="close-details"
              onClick={() => setSelectedDate(null)}
            >
              ✕
            </button>
          </div>
          
          <div className="details-content">
            {(() => {
              const dateString = formatDate(selectedDate);
              const dayHabits = getHabitsForDate(dateString);
              const isToday = formatDate(new Date()) === dateString;
              
              return (
                <div className="date-habits">
                  {/* Completed Habits */}
                  {dayHabits.completed.length > 0 && (
                    <div className="habits-section">
                      <h4>✅ Completed Habits ({dayHabits.completed.length})</h4>
                      <div className="habits-list">
                        {dayHabits.completed.map(habit => (
                          <div key={habit.id} className="habit-item completed">
                            <div 
                              className="habit-color" 
                              style={{ backgroundColor: habit.color }}
                            ></div>
                            <div className="habit-info">
                              <span className="habit-name">{habit.name}</span>
                              <span className="habit-frequency">{habit.frequency}</span>
                            </div>
                            <div className="habit-streak">🔥 {habit.currentStreak}</div>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                  
                  {/* Pending Habits (only for today) */}
                  {isToday && dayHabits.pending.length > 0 && (
                    <div className="habits-section">
                      <h4>⏳ Pending Habits ({dayHabits.pending.length})</h4>
                      <div className="habits-list">
                        {dayHabits.pending.map(habit => (
                          <div key={habit.id} className="habit-item pending">
                            <div 
                              className="habit-color" 
                              style={{ backgroundColor: habit.color }}
                            ></div>
                            <div className="habit-info">
                              <span className="habit-name">{habit.name}</span>
                              <span className="habit-frequency">{habit.frequency}</span>
                            </div>
                            <div className="habit-actions">
                              <CompleteHabitButton 
                                habitId={habit.id}
                                habitName={habit.name}
                                size="small"
                                variant="outlined"
                                onComplete={fetchData}
                              />
                              <CheckInButton 
                                habitId={habit.id}
                                habitName={habit.name}
                                size="small"
                                variant="minimal"
                                onCheckIn={fetchData}
                              />
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                  
                  {dayHabits.completed.length === 0 && (!isToday || dayHabits.pending.length === 0) && (
                    <div className="no-habits-detail">
                      <div className="empty-icon">📅</div>
                      <p>{isToday ? 'All habits completed for today! 🎉' : 'No habit activity for this date'}</p>
                    </div>
                  )}
                </div>
              );
            })()}
          </div>
        </div>
      )}

      {/* Calendar Legend */}
      <div className="calendar-legend">
        <h3>Legend</h3>
        <div className="legend-items">
          {habits.slice(0, 8).map(habit => (
            <div key={habit.id} className="legend-item">
              <div 
                className="legend-color"
                style={{ backgroundColor: habit.color }}
              ></div>
              <span>{habit.name}</span>
            </div>
          ))}
          {habits.length > 8 && (
            <div className="legend-item">
              <div className="legend-color more"></div>
              <span>+{habits.length - 8} more habits</span>
            </div>
          )}
          <div className="legend-item">
            <div className="legend-symbol">✅</div>
            <span>Completed</span>
          </div>
          <div className="legend-item">
            <div className="legend-symbol">⏳</div>
            <span>Pending (Today)</span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Calendar; 