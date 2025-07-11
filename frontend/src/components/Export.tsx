import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../hooks/useToast';
import { PageTransition } from './AnimatedComponents';
import exportService from '../services/exportService';
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

interface ExportPreview {
  habitsCount: number;
  checkInsCount: number;
  badgesCount: number;
  encouragementsCount: number;
  estimatedFileSize: string;
  statistics?: {
    totalHabits: number;
    activeHabits: number;
    totalCheckIns: number;
    totalBadges: number;
    longestStreak: number;
    completionRate30Days: number;
  };
}

const Export: React.FC = () => {
  const { user } = useAuth();
  const { showSuccess, showError } = useToast();
  const [exportData, setExportData] = useState<ExportData>({
    habits: [],
    checkIns: [],
    badges: [],
    encouragements: []
  });
  const [exportPreview, setExportPreview] = useState<ExportPreview | null>(null);
  const [loading, setLoading] = useState(true);
  const [previewLoading, setPreviewLoading] = useState(false);
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
    if (user?.id) {
      fetchData();
    }
  }, [user?.id]);

  useEffect(() => {
    if (user?.id) {
      updatePreview();
    }
  }, [exportOptions, user?.id]);

  const fetchData = async () => {
    if (!user?.id) return;
    
    try {
      setLoading(true);
      const data = await exportService.fetchUserData(user.id);
      setExportData(data);
    } catch (error) {
      console.error('Error fetching data:', error);
      showError('Failed to load your data. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const updatePreview = async () => {
    if (!user?.id) return;
    
    try {
      setPreviewLoading(true);
      const preview = await exportService.getExportPreview(user.id, exportOptions);
      setExportPreview(preview);
    } catch (error) {
      console.error('Error updating preview:', error);
      // Set fallback preview
      setExportPreview({
        habitsCount: exportData.habits.length,
        checkInsCount: exportData.checkIns.length,
        badgesCount: exportData.badges.length,
        encouragementsCount: exportData.encouragements.length,
        estimatedFileSize: 'Unknown'
      });
    } finally {
      setPreviewLoading(false);
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

    // Apply date filtering
    if (exportOptions.dateRange !== 'all') {
      const now = new Date();
      let startDate: Date | null = null;
      let endDate: Date | null = null;

      switch (exportOptions.dateRange) {
        case 'week':
          startDate = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
          break;
        case 'month':
          startDate = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
          break;
        case 'custom':
          startDate = exportOptions.startDate ? new Date(exportOptions.startDate) : null;
          endDate = exportOptions.endDate ? new Date(exportOptions.endDate) : null;
          break;
      }

      if (startDate || endDate) {
        filtered.checkIns = filtered.checkIns.filter(checkIn => {
          const checkInDate = new Date(checkIn.completedAt || checkIn.date);
          if (startDate && checkInDate < startDate) return false;
          if (endDate && checkInDate > endDate) return false;
          return true;
        });

        filtered.badges = filtered.badges.filter(badge => {
          const badgeDate = new Date(badge.earnedAt || badge.createdAt);
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

  const handleExport = async () => {
    if (!user?.id) return;
    
    setExporting(true);
    
    try {
      const filteredData = getFilteredData();
      
      if (exportOptions.format === 'csv') {
        await exportService.exportCSV(user.id, exportOptions, filteredData);
        showSuccess('CSV file downloaded successfully!');
      } else {
        await exportService.exportPDF(user.id, exportOptions, filteredData);
        showSuccess('PDF file downloaded successfully!');
      }
    } catch (error) {
      console.error('Error during export:', error);
      showError('Export failed. Please try again.');
    } finally {
      setExporting(false);
    }
  };

  const getDataSummary = () => {
    if (exportPreview) {
      return {
        habits: exportOptions.includeHabits ? exportPreview.habitsCount : 0,
        checkIns: exportOptions.includeCheckIns ? exportPreview.checkInsCount : 0,
        badges: exportOptions.includeBadges ? exportPreview.badgesCount : 0,
        encouragements: exportOptions.includeEncouragements ? exportPreview.encouragementsCount : 0
      };
    }

    const filteredData = getFilteredData();
    return {
      habits: filteredData.habits.length,
      checkIns: filteredData.checkIns.length,
      badges: filteredData.badges.length,
      encouragements: filteredData.encouragements.length
    };
  };

  const summary = getDataSummary();
  const hasData = summary.habits + summary.checkIns + summary.badges + summary.encouragements > 0;

  if (loading) {
    return (
      <PageTransition>
        <div className="export-loading">
          <div className="loading-spinner"></div>
          <h3>Loading your data...</h3>
          <p>Gathering all your habits, check-ins, badges, and encouragements</p>
        </div>
      </PageTransition>
    );
  }

  return (
    <PageTransition>
      <div className="export">
        {/* Header Section */}
        <div className="export-header">
          <div className="export-icon">📤</div>
          <h1>Export Your Data</h1>
          <p className="export-subtitle">
            Download your habit tracking journey in CSV or PDF format
          </p>
        </div>

        {/* Main Content */}
        <div className="export-grid">
          {/* Configuration Panel */}
          <div className="export-config-panel">
            <div className="config-card">
              <h2>Export Configuration</h2>
              
              {/* Format Selection */}
              <div className="config-section">
                <label className="config-label">Export Format</label>
                <div className="format-selector">
                  <button 
                    className={`format-option ${exportOptions.format === 'csv' ? 'active' : ''}`}
                    onClick={() => handleOptionChange('format', 'csv')}
                  >
                    <div className="format-icon">📊</div>
                    <div className="format-info">
                      <div className="format-name">CSV</div>
                      <div className="format-desc">Spreadsheet format</div>
                    </div>
                  </button>
                  <button 
                    className={`format-option ${exportOptions.format === 'pdf' ? 'active' : ''}`}
                    onClick={() => handleOptionChange('format', 'pdf')}
                  >
                    <div className="format-icon">📄</div>
                    <div className="format-info">
                      <div className="format-name">PDF</div>
                      <div className="format-desc">Formatted report</div>
                    </div>
                  </button>
                </div>
              </div>

              {/* Date Range */}
              <div className="config-section">
                <label className="config-label">Date Range</label>
                <select 
                  className="config-select"
                  value={exportOptions.dateRange}
                  onChange={(e) => handleOptionChange('dateRange', e.target.value)}
                >
                  <option value="all">All Time</option>
                  <option value="month">Last 30 Days</option>
                  <option value="week">Last 7 Days</option>
                  <option value="custom">Custom Range</option>
                </select>
              </div>

              {/* Custom Date Range */}
              {exportOptions.dateRange === 'custom' && (
                <div className="config-section">
                  <div className="date-range-inputs">
                    <div className="date-input-group">
                      <label className="config-label">Start Date</label>
                      <input 
                        type="date" 
                        className="config-input"
                        value={exportOptions.startDate || ''}
                        onChange={(e) => handleOptionChange('startDate', e.target.value)}
                      />
                    </div>
                    <div className="date-separator">to</div>
                    <div className="date-input-group">
                      <label className="config-label">End Date</label>
                      <input 
                        type="date" 
                        className="config-input"
                        value={exportOptions.endDate || ''}
                        onChange={(e) => handleOptionChange('endDate', e.target.value)}
                      />
                    </div>
                  </div>
                </div>
              )}

              {/* Data Selection */}
              <div className="config-section">
                <label className="config-label">Include Data</label>
                <div className="checkbox-grid">
                  <label className="checkbox-option">
                    <input
                      type="checkbox"
                      checked={exportOptions.includeHabits}
                      onChange={(e) => handleOptionChange('includeHabits', e.target.checked)}
                    />
                    <span className="checkbox-custom"></span>
                    <div className="checkbox-content">
                      <div className="checkbox-name">🎯 Habits</div>
                      <div className="checkbox-count">
                        ({previewLoading ? '...' : (exportPreview?.habitsCount ?? exportData.habits.length)})
                      </div>
                    </div>
                  </label>
                  <label className="checkbox-option">
                    <input
                      type="checkbox"
                      checked={exportOptions.includeCheckIns}
                      onChange={(e) => handleOptionChange('includeCheckIns', e.target.checked)}
                    />
                    <span className="checkbox-custom"></span>
                    <div className="checkbox-content">
                      <div className="checkbox-name">✅ Check-ins</div>
                      <div className="checkbox-count">
                        ({previewLoading ? '...' : (exportPreview?.checkInsCount ?? exportData.checkIns.length)})
                      </div>
                    </div>
                  </label>
                  <label className="checkbox-option">
                    <input
                      type="checkbox"
                      checked={exportOptions.includeBadges}
                      onChange={(e) => handleOptionChange('includeBadges', e.target.checked)}
                    />
                    <span className="checkbox-custom"></span>
                    <div className="checkbox-content">
                      <div className="checkbox-name">🏆 Badges</div>
                      <div className="checkbox-count">
                        ({previewLoading ? '...' : (exportPreview?.badgesCount ?? exportData.badges.length)})
                      </div>
                    </div>
                  </label>
                  <label className="checkbox-option">
                    <input
                      type="checkbox"
                      checked={exportOptions.includeEncouragements}
                      onChange={(e) => handleOptionChange('includeEncouragements', e.target.checked)}
                    />
                    <span className="checkbox-custom"></span>
                    <div className="checkbox-content">
                      <div className="checkbox-name">💝 Encouragements</div>
                      <div className="checkbox-count">
                        ({previewLoading ? '...' : (exportPreview?.encouragementsCount ?? exportData.encouragements.length)})
                      </div>
                    </div>
                  </label>
                  <label className="checkbox-option">
                    <input
                      type="checkbox"
                      checked={exportOptions.includeStats}
                      onChange={(e) => handleOptionChange('includeStats', e.target.checked)}
                    />
                    <span className="checkbox-custom"></span>
                    <div className="checkbox-content">
                      <div className="checkbox-name">📊 Statistics</div>
                      <div className="checkbox-count">(Summary)</div>
                    </div>
                  </label>
                </div>
              </div>
            </div>
          </div>

          {/* Preview Panel */}
          <div className="export-preview-panel">
            <div className="preview-card">
              <h2>Export Preview</h2>
              
              {/* Data Summary */}
              <div className="preview-summary">
                <div className="summary-stats">
                  <div className="stat-item">
                    <div className="stat-icon">🎯</div>
                    <div className="stat-value">{previewLoading ? '...' : summary.habits}</div>
                    <div className="stat-label">Habits</div>
                  </div>
                  <div className="stat-item">
                    <div className="stat-icon">✅</div>
                    <div className="stat-value">{previewLoading ? '...' : summary.checkIns}</div>
                    <div className="stat-label">Check-ins</div>
                  </div>
                  <div className="stat-item">
                    <div className="stat-icon">🏆</div>
                    <div className="stat-value">{previewLoading ? '...' : summary.badges}</div>
                    <div className="stat-label">Badges</div>
                  </div>
                  <div className="stat-item">
                    <div className="stat-icon">💝</div>
                    <div className="stat-value">{previewLoading ? '...' : summary.encouragements}</div>
                    <div className="stat-label">Encouragements</div>
                  </div>
                </div>

                {/* Additional Statistics */}
                {exportPreview?.statistics && (
                  <div className="additional-stats">
                    <div className="stat-row">
                      <span className="stat-metric">Estimated File Size:</span>
                      <span className="stat-val">{exportPreview.estimatedFileSize}</span>
                    </div>
                    {exportOptions.includeStats && (
                      <>
                        <div className="stat-row">
                          <span className="stat-metric">Longest Streak:</span>
                          <span className="stat-val">{exportPreview.statistics.longestStreak} days</span>
                        </div>
                        <div className="stat-row">
                          <span className="stat-metric">30-Day Completion Rate:</span>
                          <span className="stat-val">{exportPreview.statistics.completionRate30Days.toFixed(1)}%</span>
                        </div>
                      </>
                    )}
                  </div>
                )}
              </div>

              {/* Export Button */}
              <div className="export-action">
                <button 
                  className={`export-btn ${!hasData ? 'disabled' : ''}`}
                  onClick={handleExport}
                  disabled={exporting || !hasData || previewLoading}
                >
                  {exporting ? (
                    <>
                      <div className="btn-spinner"></div>
                      Generating {exportOptions.format.toUpperCase()}...
                    </>
                  ) : (
                    <>
                      <span className="btn-icon">📤</span>
                      Export as {exportOptions.format.toUpperCase()}
                    </>
                  )}
                </button>
                
                {!hasData && (
                  <p className="export-notice">
                    No data available for export. Start tracking habits to generate your report!
                  </p>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Information Section */}
        <div className="export-info">
          <div className="info-header">
            <h2>About Data Export</h2>
            <p>Learn more about our export formats and privacy practices</p>
          </div>
          
          <div className="info-cards">
            <div className="info-card">
              <div className="info-card-icon">📊</div>
              <h3>CSV Format</h3>
              <p>Perfect for data analysis in Excel, Google Sheets, or other spreadsheet applications. All your data in a structured, importable format.</p>
              <ul className="info-features">
                <li>Compatible with all spreadsheet software</li>
                <li>Easy data manipulation and analysis</li>
                <li>Raw data for custom reporting</li>
              </ul>
            </div>
            
            <div className="info-card">
              <div className="info-card-icon">📄</div>
              <h3>PDF Format</h3>
              <p>Beautifully formatted reports perfect for sharing, printing, or keeping digital records. Includes charts and visual summaries.</p>
              <ul className="info-features">
                <li>Professional formatting</li>
                <li>Charts and visualizations</li>
                <li>Print-ready layout</li>
              </ul>
            </div>
            
            <div className="info-card">
              <div className="info-card-icon">🔒</div>
              <h3>Privacy & Security</h3>
              <p>Your data is processed securely and downloads are generated on-demand. We never store your exported data on our servers.</p>
              <ul className="info-features">
                <li>On-demand generation</li>
                <li>Secure data handling</li>
                <li>No server-side storage</li>
              </ul>
            </div>
            
            <div className="info-card">
              <div className="info-card-icon">📅</div>
              <h3>Date Filtering</h3>
              <p>Filter your data by specific date ranges to focus on particular periods or export your complete habit tracking history.</p>
              <ul className="info-features">
                <li>Flexible date ranges</li>
                <li>Historical data access</li>
                <li>Period-specific analysis</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </PageTransition>
  );
};

export default Export; 