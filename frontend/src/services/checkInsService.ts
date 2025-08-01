import { ApiService } from './api';

// Types
export interface CheckIn {
  id: string;
  habitId: string;
  userId: string;
  completedAt: string;
  notes?: string;
  streakDay: number;
  isManualEntry: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCheckInRequest {
  userId?: string;
  habitId: string;
  completedAt: string;
  notes?: string;
  streakDay: number;
  isManualEntry: boolean;
}

export interface UpdateCheckInRequest {
  notes?: string;
  streakDay?: number;
  isManualEntry?: boolean;
}

export interface CheckInStats {
  totalCheckIns: number;
  currentStreak: number;
  longestStreak: number;
  checkInsByDay: Record<string, number>;
  checkInsByMonth: Record<string, number>;
}

// Check-ins Service
export class CheckInsService {
  // Get all check-ins for the current user
  static async getCheckIns(filters?: {
    habitId?: string;
    startDate?: string;
    endDate?: string;
  }): Promise<CheckIn[]> {
    return ApiService.get<CheckIn[]>('/check-ins', filters);
  }

  // Get a specific check-in by ID
  static async getCheckIn(id: string): Promise<CheckIn> {
    return ApiService.get<CheckIn>(`/check-ins/${id}`);
  }

  // Create a new check-in
  static async createCheckIn(checkIn: CreateCheckInRequest): Promise<CheckIn> {
    // Ensure habitId is properly formatted as GUID
    const formattedCheckIn = {
      ...checkIn,
      habitId: checkIn.habitId // The backend will handle the conversion
    };
    
    console.log('CheckInsService - formatted check-in data:', formattedCheckIn);
    
    return ApiService.post<CheckIn>('/check-ins', formattedCheckIn);
  }

  // Update an existing check-in
  static async updateCheckIn(id: string, updates: UpdateCheckInRequest): Promise<CheckIn> {
    return ApiService.put<CheckIn>(`/check-ins/${id}`, updates);
  }

  // Delete a check-in
  static async deleteCheckIn(id: string): Promise<{ message: string }> {
    return ApiService.delete<{ message: string }>(`/check-ins/${id}`);
  }

  // Get check-in statistics
  static async getCheckInStats(period?: string): Promise<CheckInStats> {
    const params: any = {};
    if (period) params.period = period;
    
    return ApiService.get<CheckInStats>('/check-ins/stats', params);
  }

  // Get check-ins for a specific habit
  static async getHabitCheckIns(habitId: string, startDate?: string, endDate?: string): Promise<CheckIn[]> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    
    return ApiService.get<CheckIn[]>(`/habits/${habitId}/check-ins`, params);
  }

  // Bulk create check-ins
  static async bulkCreateCheckIns(checkIns: CreateCheckInRequest[]): Promise<CheckIn[]> {
    return ApiService.post<CheckIn[]>('/check-ins/bulk', { checkIns });
  }

  // Get check-in calendar data
  static async getCheckInCalendar(year: number, month: number): Promise<Record<string, number>> {
    return ApiService.get<Record<string, number>>('/check-ins/calendar', { year, month });
  }
}

export default CheckInsService; 