import React, { useState, useEffect } from 'react';
import { PageTransition } from './AnimatedComponents';
import exportService from '../services/exportService';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../hooks/useToast';
import './Export.css';

interface ExportOptions {
  format: 'csv';
  dateRange: 'all' | 'month' | 'week' | 'custom';
  startDate?: string;
  endDate?: string;
  includeHabits: boolean;
  includeCheckIns: boolean;
  includeBadges: boolean;
  includeEncouragements: boolean;
  includeStats: boolean;
}

interface ExportData {
  habits: any[];
  checkIns: any[];
  badges: any[];
  encouragements: any[];
}

interface ExportPreview {
  habitsCount: number;
  checkInsCount: number;
  badgesCount: number;
  encouragementsCount: number;
  estimatedFileSize: string;
  statistics?: {
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

  const getFilteredData = () => {
    const filtered = {
      habits: [...exportData.habits],
      checkIns: [...exportData.checkIns],
      badges: [...exportData.badges],
      encouragements: [...exportData.encouragements]
    };

    // Apply date filtering
    if (exportOptions.dateRange !== 'all') {
      let startDate: Date | null = null;
      let endDate: Date | null = null;

      if (exportOptions.dateRange === 'week') {
        startDate = new Date(Date.now() - 7 * 24 * 60 * 60 * 1000);
        endDate = new Date();
      } else if (exportOptions.dateRange === 'month') {
        startDate = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000);
        endDate = new Date();
      } else if (exportOptions.dateRange === 'custom') {
        if (exportOptions.startDate) startDate = new Date(exportOptions.startDate);
        if (exportOptions.endDate) endDate = new Date(exportOptions.endDate);
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
      await exportService.exportCSV(user.id, exportOptions, filteredData);
      showSuccess('CSV file downloaded successfully!');
    } catch (error) {
      console.error('Error during export:', error);
      showError('Export failed. Please try again.');
    } finally {
      setExporting(false);
    }
  };

  const handleOptionChange = (key: keyof ExportOptions, value: any) => {
    setExportOptions(prev => ({
      ...prev,
      [key]: value
    }));
  };

  const getDataSummary = () => {
    const filtered = getFilteredData();
    return {
      habits: filtered.habits.length,
      checkIns: filtered.checkIns.length,
      badges: filtered.badges.length,
      encouragements: filtered.encouragements.length
    };
  };

  const hasData = () => {
    const summary = getDataSummary();
    return summary.habits > 0 || summary.checkIns > 0 || summary.badges > 0 || summary.encouragements > 0;
  };

  if (loading) {
    return (
      <PageTransition>
        <div className="export">
          <div className="loading-container">
            <div className="loading-spinner"></div>
            <p>Loading your data...</p>
          </div>
        </div>
      </PageTransition>
    );
  }

  const summary = getDataSummary();

  return (
    <PageTransition>
      <div className="export">
        {/* Header Section */}
        <div className="export-header">
          <div className="export-icon">📤</div>
          <h1>Export Your Data</h1>
          <p className="export-subtitle">
            Download your habit tracking journey in CSV format
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
                    className="format-option active"
                    disabled
                  >
                    <div className="format-icon">📊</div>
                    <div className="format-info">
                      <div className="format-name">CSV</div>
                      <div className="format-desc">Spreadsheet format</div>
                    </div>
                  </button>
                </div>
              </div>

              {/* Date Range */}
              <div className="config-section">
                <label className="config-label">Date Range</label>
                <div className="date-range-selector">
                  <button 
                    className={`range-option ${exportOptions.dateRange === 'all' ? 'active' : ''}`}
                    onClick={() => handleOptionChange('dateRange', 'all')}
                  >
                    All Time
                  </button>
                  <button 
                    className={`range-option ${exportOptions.dateRange === 'month' ? 'active' : ''}`}
                    onClick={() => handleOptionChange('dateRange', 'month')}
                  >
                    Last Month
                  </button>
                  <button 
                    className={`range-option ${exportOptions.dateRange === 'week' ? 'active' : ''}`}
                    onClick={() => handleOptionChange('dateRange', 'week')}
                  >
                    Last Week
                  </button>
                  <button 
                    className={`range-option ${exportOptions.dateRange === 'custom' ? 'active' : ''}`}
                    onClick={() => handleOptionChange('dateRange', 'custom')}
                  >
                    Custom
                  </button>
                </div>

                {exportOptions.dateRange === 'custom' && (
                  <div className="custom-date-inputs">
                    <div className="date-input-group">
                      <label>Start Date</label>
                      <input
                        type="date"
                        value={exportOptions.startDate || ''}
                        onChange={(e) => handleOptionChange('startDate', e.target.value)}
                      />
                    </div>
                    <div className="date-input-group">
                      <label>End Date</label>
                      <input
                        type="date"
                        value={exportOptions.endDate || ''}
                        onChange={(e) => handleOptionChange('endDate', e.target.value)}
                      />
                    </div>
                  </div>
                )}
              </div>

              {/* Data Selection */}
              <div className="config-section">
                <label className="config-label">Include in Export</label>
                <div className="data-selection">
                  <label className="checkbox-item">
                    <input
                      type="checkbox"
                      checked={exportOptions.includeHabits}
                      onChange={(e) => handleOptionChange('includeHabits', e.target.checked)}
                    />
                    <span className="checkmark"></span>
                    Habits ({summary.habits})
                  </label>
                  <label className="checkbox-item">
                    <input
                      type="checkbox"
                      checked={exportOptions.includeCheckIns}
                      onChange={(e) => handleOptionChange('includeCheckIns', e.target.checked)}
                    />
                    <span className="checkmark"></span>
                    Check-ins ({summary.checkIns})
                  </label>
                  <label className="checkbox-item">
                    <input
                      type="checkbox"
                      checked={exportOptions.includeBadges}
                      onChange={(e) => handleOptionChange('includeBadges', e.target.checked)}
                    />
                    <span className="checkmark"></span>
                    Badges ({summary.badges})
                  </label>
                  <label className="checkbox-item">
                    <input
                      type="checkbox"
                      checked={exportOptions.includeEncouragements}
                      onChange={(e) => handleOptionChange('includeEncouragements', e.target.checked)}
                    />
                    <span className="checkmark"></span>
                    Encouragements ({summary.encouragements})
                  </label>
                  <label className="checkbox-item">
                    <input
                      type="checkbox"
                      checked={exportOptions.includeStats}
                      onChange={(e) => handleOptionChange('includeStats', e.target.checked)}
                    />
                    <span className="checkmark"></span>
                    Statistics Summary
                  </label>
                </div>
              </div>
            </div>
          </div>

          {/* Preview Panel */}
          <div className="export-preview-panel">
            <div className="preview-card">
              <h2>Export Preview</h2>
              
              {previewLoading ? (
                <div className="preview-loading">
                  <div className="loading-spinner"></div>
                  <p>Updating preview...</p>
                </div>
              ) : (
                <div className="preview-content">
                  <div className="data-summary">
                    <div className="summary-item">
                      <span className="summary-label">Habits:</span>
                      <span className="summary-value">{exportPreview?.habitsCount || summary.habits}</span>
                    </div>
                    <div className="summary-item">
                      <span className="summary-label">Check-ins:</span>
                      <span className="summary-value">{exportPreview?.checkInsCount || summary.checkIns}</span>
                    </div>
                    <div className="summary-item">
                      <span className="summary-label">Badges:</span>
                      <span className="summary-value">{exportPreview?.badgesCount || summary.badges}</span>
                    </div>
                    <div className="summary-item">
                      <span className="summary-label">Encouragements:</span>
                      <span className="summary-value">{exportPreview?.encouragementsCount || summary.encouragements}</span>
                    </div>
                  </div>

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
              )}

              {/* Export Button */}
              <div className="export-action">
                <button 
                  className={`export-btn ${!hasData() ? 'disabled' : ''}`}
                  onClick={handleExport}
                  disabled={exporting || !hasData() || previewLoading}
                >
                  {exporting ? (
                    <>
                      <div className="btn-spinner"></div>
                      Generating CSV...
                    </>
                  ) : (
                    <>
                      <span className="btn-icon">📤</span>
                      Export as CSV
                    </>
                  )}
                </button>
                
                {!hasData() && (
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
            <p>Learn more about our export format and privacy practices</p>
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
              <div className="info-card-icon">🔒</div>
              <h3>Privacy & Security</h3>
              <p>Your data is processed securely and downloads are generated on-demand. We never store your exported data on our servers.</p>
              <ul className="info-features">
                <li>On-demand generation</li>
                <li>Secure data handling</li>
                <li>No server-side storage</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </PageTransition>
  );
};

export default Export; 