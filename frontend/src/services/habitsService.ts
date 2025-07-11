import { ApiService } from './api';

// Types
export interface Habit {
  id: string;
  name: string;
  description?: string;
  frequency: HabitFrequency;
  targetCount: number;
  currentStreak: number;
  longestStreak: number;
  totalCompletions: number;
  completionRate: number; // Overall completion rate as percentage
  weeklyCompletionRate: number; // Last 7 days completion rate
  monthlyCompletionRate: number; // Last 30 days completion rate
  totalPossibleCompletions: number; // Total possible completions based on frequency
  totalActualCompletions: number; // Total actual completions
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  category?: string;
  reminderTime?: string;
  color?: string;
  icon?: string;
}

export interface CreateHabitRequest {
  name: string;
  description?: string;
  frequency: HabitFrequency;
  targetCount: number;
  category?: string;
  reminderTime?: string;
  color?: string;
  icon?: string;
}

export interface UpdateHabitRequest {
  name?: string;
  description?: string;
  frequency?: HabitFrequency;
  targetCount?: number;
  category?: string;
  reminderTime?: string;
  color?: string;
  icon?: string;
  isActive?: boolean;
}

export interface HabitEntry {
  id: string;
  habitId: string;
  completedAt: string;
  notes?: string;
  mood?: number;
}

export interface CompletionRate {
  habitId: string;
  habitName: string;
  overallCompletionRate: number;
  weeklyCompletionRate: number;
  monthlyCompletionRate: number;
  totalPossibleCompletions: number;
  totalActualCompletions: number;
  currentStreak: number;
  longestStreak: number;
  lastCompletedAt?: string;
  isActive: boolean;
}

export interface UserCompletionRate {
  userId: string;
  overallCompletionRate: number;
  weeklyCompletionRate: number;
  monthlyCompletionRate: number;
  totalHabits: number;
  activeHabits: number;
  completedHabitsToday: number;
  habitCompletionRates: CompletionRate[];
}

export interface CreateHabitEntryRequest {
  habitId: string;
  notes?: string;
  mood?: number;
}

export enum HabitFrequency {
  Daily = 'Daily',
  Weekly = 'Weekly',
  Monthly = 'Monthly',
  Custom = 'Custom'
}

// Habits Service
export class HabitsService {
  // Get all habits for the current user
  static async getHabits(userId: string): Promise<Habit[]> {
    return ApiService.get<Habit[]>(`/Habits/user/${userId}`);
  }

  // Get a specific habit by ID
  static async getHabit(id: string): Promise<Habit> {
    return ApiService.get<Habit>(`/Habits/${id}`);
  }

  // Get active habits for the current user
  static async getActiveHabits(userId: string): Promise<Habit[]> {
    return ApiService.get<Habit[]>(`/Habits/user/${userId}/active`);
  }

  // Create a new habit
  static async createHabit(habit: CreateHabitRequest): Promise<Habit> {
    return ApiService.post<Habit>('/Habits', habit);
  }

  // Update an existing habit
  static async updateHabit(id: string, updates: UpdateHabitRequest): Promise<Habit> {
    return ApiService.put<Habit>(`/Habits/${id}`, updates);
  }

  // Delete a habit
  static async deleteHabit(id: string): Promise<{ message: string }> {
    return ApiService.delete<{ message: string }>(`/Habits/${id}`);
  }

  // Get habit entries for a specific habit
  static async getHabitEntries(habitId: string, startDate?: string, endDate?: string): Promise<HabitEntry[]> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    
    return ApiService.get<HabitEntry[]>(`/Habits/${habitId}/entries`, params);
  }

  // Create a new habit entry (check-in)
  static async createHabitEntry(entry: CreateHabitEntryRequest): Promise<HabitEntry> {
    return ApiService.post<HabitEntry>('/HabitEntries', entry);
  }

  // Update a habit entry
  static async updateHabitEntry(id: string, updates: Partial<CreateHabitEntryRequest>): Promise<HabitEntry> {
    return ApiService.put<HabitEntry>(`/HabitEntries/${id}`, updates);
  }

  // Delete a habit entry
  static async deleteHabitEntry(id: string): Promise<{ message: string }> {
    return ApiService.delete<{ message: string }>(`/HabitEntries/${id}`);
  }

  // Get habit statistics
  static async getHabitStats(habitId: string, period?: string): Promise<any> {
    const params: any = {};
    if (period) params.period = period;
    
    return ApiService.get<any>(`/Habits/${habitId}/stats`, params);
  }

  // Get user's overall habit statistics
  static async getUserStats(): Promise<any> {
    return ApiService.get<any>('/Habits/stats');
  }

  // Get user's completion rate statistics
  static async getUserCompletionRates(userId: string): Promise<UserCompletionRate> {
    return ApiService.get<UserCompletionRate>(`/Habits/user/${userId}/completion-rates`);
  }

  // Get completion rate for a specific habit
  static async getHabitCompletionRate(habitId: string): Promise<CompletionRate> {
    return ApiService.get<CompletionRate>(`/Habits/${habitId}/completion-rate`);
  }

  // Complete a habit (simple completion tracking using HabitEntry)
  static async completeHabit(habitId: string, notes?: string): Promise<HabitEntry> {
    return ApiService.post<HabitEntry>(`/Habits/${habitId}/complete`, { notes });
  }

  // Create a detailed check-in (using CheckIn for detailed logging)
  static async createCheckIn(habitId: string, notes?: string, completedAt?: string): Promise<any> {
    return ApiService.post<any>('/check-ins', { 
      habitId, 
      notes, 
      completedAt: completedAt || new Date().toISOString() 
    });
  }

  // Check if a habit has been completed today
  static async isHabitCompletedToday(habitId: string): Promise<boolean> {
    // Use local date and format it consistently
    const today = new Date();
    // Format as YYYY-MM-DD in local timezone
    const localDateString = today.toLocaleDateString('en-CA'); // 'en-CA' gives YYYY-MM-DD format
    
    try {
      return ApiService.get<boolean>(`/HabitEntries/habit/${habitId}/check-date?date=${localDateString}`);
    } catch (error) {
      console.error('Error checking habit completion status:', error);
      return false;
    }
  }
}

export default HabitsService; 