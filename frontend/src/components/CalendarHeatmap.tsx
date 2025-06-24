import React from 'react';
import './CalendarHeatmap.css';

interface CalendarData {
  date: string;
  count: number;
  level: 0 | 1 | 2 | 3 | 4; // 0 = no activity, 4 = maximum activity
}

interface CalendarHeatmapProps {
  data: CalendarData[];
  startDate?: string;
  endDate?: string;
  title?: string;
  subtitle?: string;
  showLegend?: boolean;
  showTooltip?: boolean;
  className?: string;
  onDayClick?: (date: string, count: number) => void;
  colorScheme?: 'default' | 'green' | 'blue' | 'purple' | 'orange';
}

const CalendarHeatmap: React.FC<CalendarHeatmapProps> = ({
  data,
  startDate,
  endDate,
  title,
  subtitle,
  showLegend = true,
  showTooltip = true,
  className = '',
  onDayClick,
  colorScheme = 'default'
}) => {
  const getColorForLevel = (level: number) => {
    const colors = {
      default: ['#ebedf0', '#9be9a8', '#40c463', '#30a14e', '#216e39'],
      green: ['#ebedf0', '#9be9a8', '#40c463', '#30a14e', '#216e39'],
      blue: ['#ebedf0', '#b3d8ff', '#4dabf7', '#1971c2', '#0c4a6e'],
      purple: ['#ebedf0', '#d3b3ff', '#ae8cff', '#8b5cf6', '#6d28d9'],
      orange: ['#ebedf0', '#fed7aa', '#fb923c', '#ea580c', '#9a3412']
    };
    return colors[colorScheme][level] || colors.default[level];
  };

  const getLevelForCount = (count: number): 0 | 1 | 2 | 3 | 4 => {
    if (count === 0) return 0;
    if (count <= 1) return 1;
    if (count <= 3) return 2;
    if (count <= 6) return 3;
    return 4;
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  const getWeekdayLabel = (dayIndex: number) => {
    const weekdays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    return weekdays[dayIndex];
  };

  const getMonthLabel = (date: Date) => {
    return date.toLocaleDateString('en-US', { month: 'short' });
  };

  // Generate calendar grid
  const generateCalendarGrid = () => {
    const end = endDate ? new Date(endDate) : new Date();
    const start = startDate ? new Date(startDate) : new Date(end.getTime() - 365 * 24 * 60 * 60 * 1000);
    
    const weeks: Array<Array<{ date: string; count: number; level: number }>> = [];
    let currentDate = new Date(start);
    let currentWeek: Array<{ date: string; count: number; level: number }> = [];
    
    // Fill in missing days at the start of the first week
    const startDayOfWeek = currentDate.getDay();
    for (let i = 0; i < startDayOfWeek; i++) {
      const fillDate = new Date(currentDate);
      fillDate.setDate(fillDate.getDate() - (startDayOfWeek - i));
      currentWeek.push({
        date: fillDate.toISOString().split('T')[0],
        count: 0,
        level: 0
      });
    }

    while (currentDate <= end) {
      const dateString = currentDate.toISOString().split('T')[0];
      const dayData = data.find(d => d.date === dateString);
      
      currentWeek.push({
        date: dateString,
        count: dayData?.count || 0,
        level: dayData?.level || getLevelForCount(dayData?.count || 0)
      });

      if (currentWeek.length === 7) {
        weeks.push([...currentWeek]);
        currentWeek = [];
      }

      currentDate.setDate(currentDate.getDate() + 1);
    }

    // Fill in remaining days of the last week
    while (currentWeek.length < 7) {
      const fillDate = new Date(end);
      fillDate.setDate(fillDate.getDate() + (7 - currentWeek.length));
      currentWeek.push({
        date: fillDate.toISOString().split('T')[0],
        count: 0,
        level: 0
      });
    }

    if (currentWeek.length > 0) {
      weeks.push(currentWeek);
    }

    return weeks;
  };

  const calendarGrid = generateCalendarGrid();
  const months = getMonthLabels(calendarGrid);

  function getMonthLabels(grid: Array<Array<{ date: string; count: number; level: number }>>) {
    const months: Array<{ label: string; colSpan: number }> = [];
    let currentMonth = '';
    let colSpan = 0;

    for (let weekIndex = 0; weekIndex < grid.length; weekIndex++) {
      const week = grid[weekIndex];
      for (let dayIndex = 0; dayIndex < week.length; dayIndex++) {
        const day = week[dayIndex];
        const date = new Date(day.date);
        const month = date.getMonth();
        const monthLabel = date.toLocaleDateString('en-US', { month: 'short' });

        if (monthLabel !== currentMonth) {
          if (currentMonth !== '') {
            months.push({ label: currentMonth, colSpan });
          }
          currentMonth = monthLabel;
          colSpan = 1;
        } else {
          colSpan++;
        }
      }
    }

    if (currentMonth !== '') {
      months.push({ label: currentMonth, colSpan });
    }

    return months;
  }

  const handleDayClick = (date: string, count: number) => {
    if (onDayClick) {
      onDayClick(date, count);
    }
  };

  return (
    <div className={`calendar-heatmap ${className}`}>
      {title && (
        <div className="calendar-header">
          <h3 className="calendar-title">{title}</h3>
          {subtitle && <p className="calendar-subtitle">{subtitle}</p>}
        </div>
      )}

      <div className="calendar-container">
        {/* Weekday labels */}
        <div className="weekday-labels">
          {Array.from({ length: 7 }, (_, i) => (
            <div key={i} className="weekday-label">
              {getWeekdayLabel(i)}
            </div>
          ))}
        </div>

        {/* Calendar grid */}
        <div className="calendar-grid">
          {/* Month labels */}
          <div className="month-labels">
            {months.map((month, index) => (
              <div 
                key={index} 
                className="month-label"
                style={{ gridColumn: `span ${Math.min(month.colSpan, 12)}` }}
              >
                {month.label}
              </div>
            ))}
          </div>

          {/* Calendar days */}
          <div className="calendar-days">
            {calendarGrid.map((week, weekIndex) => (
              <div key={weekIndex} className="calendar-week">
                {week.map((day, dayIndex) => (
                  <div
                    key={`${weekIndex}-${dayIndex}`}
                    className={`calendar-day level-${day.level} ${onDayClick ? 'clickable' : ''}`}
                    style={{ backgroundColor: getColorForLevel(day.level) }}
                    onClick={() => handleDayClick(day.date, day.count)}
                    title={showTooltip ? `${formatDate(day.date)}: ${day.count} activities` : undefined}
                  >
                    {showTooltip && (
                      <div className="day-tooltip">
                        <div className="tooltip-date">{formatDate(day.date)}</div>
                        <div className="tooltip-count">{day.count} activities</div>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            ))}
          </div>
        </div>
      </div>

      {showLegend && (
        <div className="calendar-legend">
          <span className="legend-text">Less</span>
          <div className="legend-items">
            {[0, 1, 2, 3, 4].map((level) => (
              <div
                key={level}
                className="legend-item"
                style={{ backgroundColor: getColorForLevel(level) }}
              ></div>
            ))}
          </div>
          <span className="legend-text">More</span>
        </div>
      )}
    </div>
  );
};

export default CalendarHeatmap; 