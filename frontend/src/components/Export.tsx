import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import './Export.css';

interface ExportData {
  habits: any[];
  checkIns: any[];
  badges: any[];
  encouragements: any[];
}

interface ExportOptions {
  format: 'csv' | 'pdf';
  dateRange: 'all' | 'month' | 'week' | 'custom';
  startDate?: string;
  endDate?: string;
  includeHabits: boolean;
  includeCheckIns: boolean;
  includeBadges: boolean;
  includeEncouragements: boolean;
  includeStats: boolean;
}

const Export: React.FC = () => {
  const { user } = useAuth();
  const [exportData, setExportData] = useState<ExportData>({
    habits: [],
    checkIns: [],
    badges: [],
    encouragements: []
  });
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);
  const [exportOptions, setExportOptions] = useState<ExportOptions>({
    format: 'csv',
    dateRange: 'month',
    includeHabits: true,
    includeCheckIns: true,
    includeBadges: true,
    includeEncouragements: false,
    includeStats: true
  });

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      // TODO: Replace with actual API calls
      // const habitsResponse = await fetch('/api/habits');
      // const checkInsResponse = await fetch('/api/check-ins');
      // const badgesResponse = await fetch('/api/badges');
      // const encouragementsResponse = await fetch('/api/encouragements');
      
      // Initialize with empty data
      const emptyData: ExportData = {
        habits: [],
        checkIns: [],
        badges: [],
        encouragements: []
      };

      setExportData(emptyData);
    } catch (error) {
      console.error('Error fetching data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleOptionChange = (key: keyof ExportOptions, value: any) => {
    setExportOptions(prev => ({ ...prev, [key]: value }));
  };

  const getFilteredData = () => {
    const filtered: ExportData = {
      habits: exportOptions.includeHabits ? exportData.habits : [],
      checkIns: exportOptions.includeCheckIns ? exportData.checkIns : [],
      badges: exportOptions.includeBadges ? exportData.badges : [],
      encouragements: exportOptions.includeEncouragements ? exportData.encouragements : []
    };

    // Apply date filtering if needed
    if (exportOptions.dateRange !== 'all' && (exportOptions.startDate || exportOptions.endDate)) {
      const startDate = exportOptions.startDate ? new Date(exportOptions.startDate) : null;
      const endDate = exportOptions.endDate ? new Date(exportOptions.endDate) : null;

      if (startDate || endDate) {
        filtered.checkIns = filtered.checkIns.filter(checkIn => {
          const checkInDate = new Date(checkIn.date);
          if (startDate && checkInDate < startDate) return false;
          if (endDate && checkInDate > endDate) return false;
          return true;
        });

        filtered.badges = filtered.badges.filter(badge => {
          const badgeDate = new Date(badge.earnedDate);
          if (startDate && badgeDate < startDate) return false;
          if (endDate && badgeDate > endDate) return false;
          return true;
        });

        filtered.encouragements = filtered.encouragements.filter(encouragement => {
          const encouragementDate = new Date(encouragement.createdAt);
          if (startDate && encouragementDate < startDate) return false;
          if (endDate && encouragementDate > endDate) return false;
          return true;
        });
      }
    }

    return filtered;
  };

  const generateCSV = (data: ExportData) => {
    let csv = '';

    if (data.habits.length > 0) {
      csv += 'HABITS\n';
      csv += 'Name,Description,Frequency,Created Date,Current Streak,Longest Streak\n';
      data.habits.forEach(habit => {
        csv += `"${habit.name}","${habit.description}","${habit.frequency}","${habit.createdAt}","${habit.currentStreak}","${habit.longestStreak}"\n`;
      });
      csv += '\n';
    }

    if (data.checkIns.length > 0) {
      csv += 'CHECK-INS\n';
      csv += 'Date,Habit,Completed,Notes,Mood\n';
      data.checkIns.forEach(checkIn => {
        csv += `"${checkIn.date}","${checkIn.habitName}","${checkIn.completed ? 'Yes' : 'No'}","${checkIn.notes || ''}","${checkIn.mood || ''}"\n`;
      });
      csv += '\n';
    }

    if (data.badges.length > 0) {
      csv += 'BADGES\n';
      csv += 'Name,Description,Earned Date,Rarity\n';
      data.badges.forEach(badge => {
        csv += `"${badge.name}","${badge.description}","${badge.earnedDate}","${badge.rarity}"\n`;
      });
      csv += '\n';
    }

    if (data.encouragements.length > 0) {
      csv += 'ENCOURAGEMENTS\n';
      csv += 'Date,Sender,Message,Type\n';
      data.encouragements.forEach(encouragement => {
        csv += `"${encouragement.createdAt}","${encouragement.senderName}","${encouragement.message}","${encouragement.type}"\n`;
      });
    }

    return csv;
  };

  const downloadCSV = (csv: string) => {
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `habitchain-export-${new Date().toISOString().split('T')[0]}.csv`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  };

  const generatePDF = async (data: ExportData) => {
    // TODO: Implement PDF generation
    // For now, we'll just show a message
    alert('PDF export functionality will be implemented with a PDF library like jsPDF or react-pdf');
  };

  const handleExport = async () => {
    setExporting(true);
    
    try {
      const filteredData = getFilteredData();
      
      if (exportOptions.format === 'csv') {
        const csv = generateCSV(filteredData);
        downloadCSV(csv);
      } else {
        await generatePDF(filteredData);
      }
      
      // Show success message
      alert('Export completed successfully!');
    } catch (error) {
      console.error('Error during export:', error);
      alert('Export failed. Please try again.');
    } finally {
      setExporting(false);
    }
  };

  const getDataSummary = () => {
    const filteredData = getFilteredData();
    return {
      habits: filteredData.habits.length,
      checkIns: filteredData.checkIns.length,
      badges: filteredData.badges.length,
      encouragements: filteredData.encouragements.length
    };
  };

  if (loading) {
    return (
      <div className="export-loading">
        <div className="spinner"></div>
        <p>Loading your data...</p>
      </div>
    );
  }

  const summary = getDataSummary();

  return (
    <div className="export">
      <div className="export-header">
        <h1>Export Your Data</h1>
        <p className="export-subtitle">Download your habit tracking data in CSV or PDF format</p>
      </div>

      <div className="export-container">
        <div className="export-options">
          <h2>Export Options</h2>
          
          <div className="option-group">
            <label>Export Format</label>
            <div className="format-buttons">
              <button 
                className={`format-btn ${exportOptions.format === 'csv' ? 'active' : ''}`}
                onClick={() => handleOptionChange('format', 'csv')}
              >
                📊 CSV
              </button>
              <button 
                className={`format-btn ${exportOptions.format === 'pdf' ? 'active' : ''}`}
                onClick={() => handleOptionChange('format', 'pdf')}
              >
                📄 PDF
              </button>
            </div>
          </div>

          <div className="option-group">
            <label>Date Range</label>
            <select 
              value={exportOptions.dateRange}
              onChange={(e) => handleOptionChange('dateRange', e.target.value)}
            >
              <option value="all">All Time</option>
              <option value="month">Last Month</option>
              <option value="week">Last Week</option>
              <option value="custom">Custom Range</option>
            </select>
          </div>

          {exportOptions.dateRange === 'custom' && (
            <div className="date-range-inputs">
              <div className="option-group">
                <label>Start Date</label>
                <input 
                  type="date" 
                  value={exportOptions.startDate || ''}
                  onChange={(e) => handleOptionChange('startDate', e.target.value)}
                />
              </div>
              <div className="option-group">
                <label>End Date</label>
                <input 
                  type="date" 
                  value={exportOptions.endDate || ''}
                  onChange={(e) => handleOptionChange('endDate', e.target.value)}
                />
              </div>
            </div>
          )}

          <div className="option-group">
            <label>Include Data</label>
            <div className="checkbox-group">
              <label className="checkbox-label">
                <input
                  type="checkbox"
                  checked={exportOptions.includeHabits}
                  onChange={(e) => handleOptionChange('includeHabits', e.target.checked)}
                />
                <span className="checkmark"></span>
                Habits ({exportData.habits.length})
              </label>
              <label className="checkbox-label">
                <input
                  type="checkbox"
                  checked={exportOptions.includeCheckIns}
                  onChange={(e) => handleOptionChange('includeCheckIns', e.target.checked)}
                />
                <span className="checkmark"></span>
                Check-ins ({exportData.checkIns.length})
              </label>
              <label className="checkbox-label">
                <input
                  type="checkbox"
                  checked={exportOptions.includeBadges}
                  onChange={(e) => handleOptionChange('includeBadges', e.target.checked)}
                />
                <span className="checkmark"></span>
                Badges ({exportData.badges.length})
              </label>
              <label className="checkbox-label">
                <input
                  type="checkbox"
                  checked={exportOptions.includeEncouragements}
                  onChange={(e) => handleOptionChange('includeEncouragements', e.target.checked)}
                />
                <span className="checkmark"></span>
                Encouragements ({exportData.encouragements.length})
              </label>
              <label className="checkbox-label">
                <input
                  type="checkbox"
                  checked={exportOptions.includeStats}
                  onChange={(e) => handleOptionChange('includeStats', e.target.checked)}
                />
                <span className="checkmark"></span>
                Summary Statistics
              </label>
            </div>
          </div>
        </div>

        <div className="export-preview">
          <h2>Export Preview</h2>
          <div className="preview-summary">
            <div className="summary-item">
              <div className="summary-icon">🎯</div>
              <div className="summary-content">
                <h3>{summary.habits}</h3>
                <p>Habits</p>
              </div>
            </div>
            <div className="summary-item">
              <div className="summary-icon">✅</div>
              <div className="summary-content">
                <h3>{summary.checkIns}</h3>
                <p>Check-ins</p>
              </div>
            </div>
            <div className="summary-item">
              <div className="summary-icon">🏆</div>
              <div className="summary-content">
                <h3>{summary.badges}</h3>
                <p>Badges</p>
              </div>
            </div>
            <div className="summary-item">
              <div className="summary-icon">💝</div>
              <div className="summary-content">
                <h3>{summary.encouragements}</h3>
                <p>Encouragements</p>
              </div>
            </div>
          </div>

          <div className="export-actions">
            <button 
              className="btn-export"
              onClick={handleExport}
              disabled={exporting || (summary.habits + summary.checkIns + summary.badges + summary.encouragements === 0)}
            >
              {exporting ? 'Exporting...' : `Export as ${exportOptions.format.toUpperCase()}`}
            </button>
          </div>
        </div>
      </div>

      <div className="export-info">
        <h2>About Data Export</h2>
        <div className="info-grid">
          <div className="info-item">
            <div className="info-icon">📊</div>
            <h3>CSV Format</h3>
            <p>Perfect for data analysis in Excel, Google Sheets, or other spreadsheet applications. Includes all your data in a structured format.</p>
          </div>
          <div className="info-item">
            <div className="info-icon">📄</div>
            <h3>PDF Format</h3>
            <p>Great for sharing reports, printing, or keeping digital records. Includes formatted summaries and visual elements.</p>
          </div>
          <div className="info-item">
            <div className="info-icon">🔒</div>
            <h3>Privacy</h3>
            <p>Your data is processed locally and never stored on our servers. Downloads are generated on-demand for your security.</p>
          </div>
          <div className="info-item">
            <div className="info-icon">📅</div>
            <h3>Date Filtering</h3>
            <p>Filter your data by date range to focus on specific periods or export your complete history for comprehensive analysis.</p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Export; 