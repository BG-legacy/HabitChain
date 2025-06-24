import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import './Calendar.css';

interface CheckIn {
  id: string;
  habitId: string;
  habitName: string;
  date: string;
  completed: boolean;
  notes?: string;
  mood?: string;
}

interface Habit {
  id: string;
  name: string;
  color: string;
}

const Calendar: React.FC = () => {
  const { user } = useAuth();
  const [checkIns, setCheckIns] = useState<CheckIn[]>([]);
  const [habits, setHabits] = useState<Habit[]>([]);
  const [loading, setLoading] = useState(true);
  const [currentDate, setCurrentDate] = useState(new Date());
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);
  const [viewMode, setViewMode] = useState<'month' | 'week'>('month');

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      // TODO: Replace with actual API calls
      // const checkInsResponse = await fetch('/api/check-ins');
      // const habitsResponse = await fetch('/api/habits');
      
      // Initialize with empty data
      setHabits([]);
      setCheckIns([]);
    } catch (error) {
      console.error('Error fetching data:', error);
    } finally {
      setLoading(false);
    }
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
    return checkIns.filter(checkIn => checkIn.date === date);
  };

  const getHabitColor = (habitId: string) => {
    const habit = habits.find(h => h.id === habitId);
    return habit?.color || '#6c757d';
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

  const getMoodIcon = (mood?: string) => {
    switch (mood) {
      case 'excellent': return '😊';
      case 'good': return '🙂';
      case 'okay': return '😐';
      case 'tough': return '😔';
      case 'struggling': return '😞';
      default: return '📝';
    }
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
        <h1>Check-in Calendar</h1>
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
                  const dayCheckIns = getCheckInsForDate(dateString);
                  const isToday = formatDate(new Date()) === dateString;
                  const isSelected = selectedDate && formatDate(selectedDate) === dateString;
                  
                  days.push(
                    <div 
                      key={day} 
                      className={`calendar-day ${isToday ? 'today' : ''} ${isSelected ? 'selected' : ''}`}
                      onClick={() => setSelectedDate(date)}
                    >
                      <div className="day-number">{day}</div>
                      <div className="check-ins-indicators">
                        {dayCheckIns.map(checkIn => (
                          <div 
                            key={checkIn.id}
                            className={`check-in-dot ${checkIn.completed ? 'completed' : 'missed'}`}
                            style={{ backgroundColor: getHabitColor(checkIn.habitId) }}
                            title={`${checkIn.habitName}: ${checkIn.completed ? 'Completed' : 'Missed'}`}
                          >
                            {checkIn.mood && getMoodIcon(checkIn.mood)}
                          </div>
                        ))}
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
                const dayCheckIns = getCheckInsForDate(dateString);
                const isToday = formatDate(new Date()) === dateString;
                
                return (
                  <div key={dateString} className={`week-day ${isToday ? 'today' : ''}`}>
                    {dayCheckIns.length > 0 ? (
                      <div className="week-check-ins">
                        {dayCheckIns.map(checkIn => (
                          <div 
                            key={checkIn.id}
                            className={`week-check-in ${checkIn.completed ? 'completed' : 'missed'}`}
                            style={{ borderColor: getHabitColor(checkIn.habitId) }}
                          >
                            <div className="check-in-header">
                              <span className="habit-name">{checkIn.habitName}</span>
                              <span className="mood-icon">{checkIn.mood && getMoodIcon(checkIn.mood)}</span>
                            </div>
                            {checkIn.notes && (
                              <div className="check-in-notes">{checkIn.notes}</div>
                            )}
                          </div>
                        ))}
                      </div>
                    ) : (
                      <div className="no-check-ins">No check-ins</div>
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
              const dayCheckIns = getCheckInsForDate(dateString);
              
              if (dayCheckIns.length === 0) {
                return (
                  <div className="no-check-ins-detail">
                    <div className="empty-icon">📅</div>
                    <p>No check-ins for this date</p>
                  </div>
                );
              }
              
              return (
                <div className="check-ins-list">
                  {dayCheckIns.map(checkIn => (
                    <div key={checkIn.id} className="check-in-detail">
                      <div className="check-in-header">
                        <div className="habit-info">
                          <div 
                            className="habit-color"
                            style={{ backgroundColor: getHabitColor(checkIn.habitId) }}
                          ></div>
                          <span className="habit-name">{checkIn.habitName}</span>
                        </div>
                        <div className="check-in-status">
                          <span className={`status ${checkIn.completed ? 'completed' : 'missed'}`}>
                            {checkIn.completed ? '✅ Completed' : '❌ Missed'}
                          </span>
                          {checkIn.mood && (
                            <span className="mood">{getMoodIcon(checkIn.mood)}</span>
                          )}
                        </div>
                      </div>
                      {checkIn.notes && (
                        <div className="check-in-notes">{checkIn.notes}</div>
                      )}
                    </div>
                  ))}
                </div>
              );
            })()}
          </div>
        </div>
      )}

      {/* Legend */}
      <div className="calendar-legend">
        <h3>Legend</h3>
        <div className="legend-items">
          {habits.map(habit => (
            <div key={habit.id} className="legend-item">
              <div 
                className="legend-color"
                style={{ backgroundColor: habit.color }}
              ></div>
              <span>{habit.name}</span>
            </div>
          ))}
          <div className="legend-item">
            <div className="legend-color completed"></div>
            <span>Completed</span>
          </div>
          <div className="legend-item">
            <div className="legend-color missed"></div>
            <span>Missed</span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Calendar; 