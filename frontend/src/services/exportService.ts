import { ApiService } from './api';
import { getApiUrl } from '../config/environment';

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

class ExportService {
  /**
   * Fetch all user data for export
   */
  async fetchUserData(userId: string): Promise<ExportData> {
    try {
      // Fetch all data in parallel
      const [habits, checkIns, badges, encouragements] = await Promise.all([
        this.fetchHabits(userId),
        this.fetchCheckIns(userId),
        this.fetchBadges(userId),
        this.fetchEncouragements(userId)
      ]);

      return {
        habits,
        checkIns,
        badges,
        encouragements
      };
    } catch (error) {
      console.error('Error fetching user data:', error);
      throw new Error('Failed to fetch user data for export');
    }
  }

  /**
   * Fetch user habits
   */
  private async fetchHabits(userId: string): Promise<any[]> {
    try {
      return await ApiService.get(`/habits/user/${userId}`);
    } catch (error) {
      console.warn('Error fetching habits:', error);
      return [];
    }
  }

  /**
   * Fetch user check-ins
   */
  private async fetchCheckIns(userId: string): Promise<any[]> {
    try {
      return await ApiService.get(`/check-ins/user/${userId}`);
    } catch (error) {
      console.warn('Error fetching check-ins:', error);
      return [];
    }
  }

  /**
   * Fetch user badges
   */
  private async fetchBadges(userId: string): Promise<any[]> {
    try {
      return await ApiService.get(`/badges/user/${userId}/earned`);
    } catch (error) {
      console.warn('Error fetching badges:', error);
      return [];
    }
  }

  /**
   * Fetch user encouragements
   */
  private async fetchEncouragements(userId: string): Promise<any[]> {
    try {
      // Fetch both sent and received encouragements
      const [received, sent] = await Promise.all([
        ApiService.get(`/encouragements/user/${userId}`).catch(() => []),
        ApiService.get(`/encouragements/from-user/${userId}`).catch(() => [])
      ]);
      
      // Ensure we have arrays before spreading
      const receivedArray = Array.isArray(received) ? received : [];
      const sentArray = Array.isArray(sent) ? sent : [];
      
      // Combine and deduplicate
      const combined = [...receivedArray, ...sentArray];
      const unique = combined.filter((item, index, self) => 
        index === self.findIndex(t => t.id === item.id)
      );
      
      return unique;
    } catch (error) {
      console.warn('Error fetching encouragements:', error);
      return [];
    }
  }

  /**
   * Export data as CSV using backend endpoint
   */
  async exportCSV(userId: string, options: ExportOptions, data: ExportData): Promise<void> {
    try {
      // Convert frontend options to backend format
      const exportOptions = this.convertToBackendOptions(options);
      
      // Make request to backend export endpoint
      const response = await fetch(`${getApiUrl()}/export/csv`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.getAuthToken()}`
        },
        body: JSON.stringify(exportOptions)
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'Export failed' }));
        throw new Error(errorData.message || 'Failed to export CSV');
      }

      // Get the CSV data as blob
      const blob = await response.blob();
      
      // Extract filename from response headers or use default
      const contentDisposition = response.headers.get('Content-Disposition');
      const filename = this.extractFilename(contentDisposition) || `habitchain-export-${new Date().toISOString().split('T')[0]}.csv`;

      // Download the file
      this.downloadBlob(blob, filename);
    } catch (error) {
      console.error('Error exporting CSV:', error);
      throw new Error('Failed to export CSV file');
    }
  }

  /**
   * Export data as PDF using backend endpoint
   */
  async exportPDF(userId: string, options: ExportOptions, data: ExportData): Promise<void> {
    try {
      // Convert frontend options to backend format
      const exportOptions = this.convertToBackendOptions(options);
      
      // Make request to backend export endpoint
      const response = await fetch(`${getApiUrl()}/export/pdf`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.getAuthToken()}`
        },
        body: JSON.stringify(exportOptions)
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'Export failed' }));
        throw new Error(errorData.message || 'Failed to export PDF');
      }

      // Get the PDF data as blob
      const blob = await response.blob();
      
      // Extract filename from response headers or use default
      const contentDisposition = response.headers.get('Content-Disposition');
      const filename = this.extractFilename(contentDisposition) || `habitchain-export-${new Date().toISOString().split('T')[0]}.pdf`;

      // Download the file
      this.downloadBlob(blob, filename);
    } catch (error) {
      console.error('Error exporting PDF:', error);
      // If PDF export fails, we can fall back to CSV
      console.warn('PDF export failed, falling back to CSV');
      await this.exportCSV(userId, options, data);
    }
  }

  /**
   * Get export preview from backend
   */
  async getExportPreview(userId: string, options: ExportOptions): Promise<any> {
    try {
      const exportOptions = this.convertToBackendOptions(options);
      
      const response = await ApiService.post('/export/preview', exportOptions);
      return response;
    } catch (error) {
      console.error('Error getting export preview:', error);
      // Return fallback preview based on current data
      const data = await this.fetchUserData(userId);
      return {
        habitsCount: data.habits.length,
        checkInsCount: data.checkIns.length,
        badgesCount: data.badges.length,
        encouragementsCount: data.encouragements.length,
        estimatedFileSize: 'Unknown',
        statistics: this.calculateBasicStats(data)
      };
    }
  }

  /**
   * Get available export formats from backend
   */
  async getExportFormats(): Promise<any> {
    try {
      return await ApiService.get('/export/formats');
    } catch (error) {
      console.warn('Error fetching export formats from backend, using defaults');
      return {
        formats: [
          {
            format: 'CSV',
            description: 'Comma-separated values format for spreadsheet applications',
            mimeType: 'text/csv',
            extension: '.csv',
            features: [
              'Raw data export',
              'Compatible with Excel, Google Sheets',
              'Easy data manipulation',
              'Lightweight file size'
            ]
          },
          {
            format: 'PDF',
            description: 'Formatted document for sharing and printing',
            mimeType: 'application/pdf',
            extension: '.pdf',
            features: [
              'Professional formatting',
              'Charts and visualizations',
              'Print-ready layout',
              'Universal compatibility'
            ]
          }
        ]
      };
    }
  }

  /**
   * Convert frontend export options to backend format
   */
  private convertToBackendOptions(options: ExportOptions): any {
    return {
      format: options.format.toUpperCase(),
      dateRange: options.dateRange,
      startDate: options.startDate || null,
      endDate: options.endDate || null,
      includeHabits: options.includeHabits,
      includeCheckIns: options.includeCheckIns,
      includeBadges: options.includeBadges,
      includeEncouragements: options.includeEncouragements,
      includeStats: options.includeStats
    };
  }

  /**
   * Get authentication token from localStorage or context
   */
  private getAuthToken(): string {
    // This should be retrieved from your auth context or localStorage
    return localStorage.getItem('accessToken') || '';
  }

  /**
   * Extract filename from Content-Disposition header
   */
  private extractFilename(contentDisposition: string | null): string | null {
    if (!contentDisposition) return null;
    
    const filenameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
    if (filenameMatch && filenameMatch[1]) {
      return filenameMatch[1].replace(/['"]/g, '');
    }
    return null;
  }

  /**
   * Download blob as file
   */
  private downloadBlob(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }

  /**
   * Calculate basic statistics from data (fallback)
   */
  private calculateBasicStats(data: ExportData): any {
    const activeHabits = data.habits.filter(h => h.isActive || h.IsActive);
    const maxStreak = Math.max(...data.habits.map(h => h.longestStreak || h.LongestStreak || 0), 0);
    const totalCurrentStreak = data.habits.reduce((sum, h) => sum + (h.currentStreak || h.CurrentStreak || 0), 0);

    // Calculate 30-day completion rate
    const thirtyDaysAgo = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000);
    const recentCheckIns = data.checkIns.filter(ci => 
      new Date(ci.completedAt || ci.CompletedAt) >= thirtyDaysAgo
    );
    const expectedCheckIns = activeHabits.length * 30;
    const completionRate = expectedCheckIns > 0 ? 
      (recentCheckIns.length / expectedCheckIns * 100) : 0;

    return {
      totalHabits: data.habits.length,
      activeHabits: activeHabits.length,
      totalCheckIns: data.checkIns.length,
      totalBadges: data.badges.length,
      totalEncouragements: data.encouragements.length,
      longestStreak: maxStreak,
      totalCurrentStreaks: totalCurrentStreak,
      completionRate30Days: Math.round(completionRate * 100) / 100,
      daysTracking: this.calculateDaysTracking(data.habits)
    };
  }

  /**
   * Calculate days tracking from habits
   */
  private calculateDaysTracking(habits: any[]): number {
    if (habits.length === 0) return 0;
    
    const createdDates = habits
      .map(h => new Date(h.createdAt || h.CreatedAt))
      .filter(date => !isNaN(date.getTime()));
    
    if (createdDates.length === 0) return 0;
    
    const earliestDate = new Date(Math.min(...createdDates.map(d => d.getTime())));
    const daysDiff = Math.floor((Date.now() - earliestDate.getTime()) / (1000 * 60 * 60 * 24));
    
    return Math.max(0, daysDiff);
  }
}

export default new ExportService(); 