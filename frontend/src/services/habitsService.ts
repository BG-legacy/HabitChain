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
  static async getHabits(): Promise<Habit[]> {
    return ApiService.get<Habit[]>('/habits');
  }

  // Get a specific habit by ID
  static async getHabit(id: string): Promise<Habit> {
    return ApiService.get<Habit>(`/habits/${id}`);
  }

  // Create a new habit
  static async createHabit(habit: CreateHabitRequest): Promise<Habit> {
    return ApiService.post<Habit>('/habits', habit);
  }

  // Update an existing habit
  static async updateHabit(id: string, updates: UpdateHabitRequest): Promise<Habit> {
    return ApiService.put<Habit>(`/habits/${id}`, updates);
  }

  // Delete a habit
  static async deleteHabit(id: string): Promise<{ message: string }> {
    return ApiService.delete<{ message: string }>(`/habits/${id}`);
  }

  // Get habit entries for a specific habit
  static async getHabitEntries(habitId: string, startDate?: string, endDate?: string): Promise<HabitEntry[]> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    
    return ApiService.get<HabitEntry[]>(`/habits/${habitId}/entries`, params);
  }

  // Create a new habit entry (check-in)
  static async createHabitEntry(entry: CreateHabitEntryRequest): Promise<HabitEntry> {
    return ApiService.post<HabitEntry>('/habit-entries', entry);
  }

  // Update a habit entry
  static async updateHabitEntry(id: string, updates: Partial<CreateHabitEntryRequest>): Promise<HabitEntry> {
    return ApiService.put<HabitEntry>(`/habit-entries/${id}`, updates);
  }

  // Delete a habit entry
  static async deleteHabitEntry(id: string): Promise<{ message: string }> {
    return ApiService.delete<{ message: string }>(`/habit-entries/${id}`);
  }

  // Get habit statistics
  static async getHabitStats(habitId: string, period?: string): Promise<any> {
    const params: any = {};
    if (period) params.period = period;
    
    return ApiService.get<any>(`/habits/${habitId}/stats`, params);
  }

  // Get user's overall habit statistics
  static async getUserStats(): Promise<any> {
    return ApiService.get<any>('/habits/stats');
  }
}

export default HabitsService; 