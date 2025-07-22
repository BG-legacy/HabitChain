import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { useAuth } from './AuthContext';
import { ApiService } from '../services/api';

interface Habit {
  id: string;
  name: string;
  description: string;
  frequency: string;
  currentStreak: number;
  longestStreak: number;
  nextCheckIn: string;
  isActive: boolean;
  totalCheckIns: number;
  lastCompletedAt?: string;
}

interface DashboardStats {
  totalHabits: number;
  activeStreaks: number;
  totalCheckIns: number;
  completionRate: number;
}

interface DashboardContextType {
  habits: Habit[];
  stats: DashboardStats;
  loading: boolean;
  refreshDashboard: () => Promise<void>;
  updateHabitStreak: (habitId: string, newStreak: number) => void;
  incrementCheckIns: () => void;
  updateHabitData: (habitId: string, updates: Partial<Habit>) => void;
  markHabitCompleted: (habitId: string, completedAt: string, newStreak: number) => void;
  updateHabitCompletion: (habitId: string, completedAt: string, newStreak: number) => void;
}

const DashboardContext = createContext<DashboardContextType | undefined>(undefined);

export const useDashboard = () => {
  const context = useContext(DashboardContext);
  if (context === undefined) {
    throw new Error('useDashboard must be used within a DashboardProvider');
  }
  return context;
};

interface DashboardProviderProps {
  children: ReactNode;
}

export const DashboardProvider: React.FC<DashboardProviderProps> = ({ children }) => {
  const { user } = useAuth();
  const [habits, setHabits] = useState<Habit[]>([]);
  const [stats, setStats] = useState<DashboardStats>({
    totalHabits: 0,
    activeStreaks: 0,
    totalCheckIns: 0,
    completionRate: 0
  });
  const [loading, setLoading] = useState(true);

  // Convert backend enum values to frontend string values
  const convertFrequencyToString = (frequency: number): string => {
    const frequencyMap: { [key: number]: string } = {
      1: 'Daily',
      2: 'Weekly', 
      3: 'Monthly',
      4: 'Custom'
    };
    return frequencyMap[frequency] || 'Daily';
  };

  const calculateStats = (habitsData: Habit[]) => {
    const activeHabits = habitsData.filter(h => h.isActive);
    const totalCheckIns = habitsData.reduce((sum, h) => sum + (h.totalCheckIns || 0), 0);
    
    // Calculate active streaks - habits with current streak > 0
    const activeStreaks = habitsData.reduce((sum, h) => sum + (h.currentStreak > 0 ? 1 : 0), 0);
    
    // Calculate completion rate based on habits completed today
    const today = new Date().toDateString();
    const recentlyCompletedHabits = habitsData.filter(h => 
              h.lastCompletedAt && new Date(h.lastCompletedAt).toDateString() === today
    ).length;
    
    // Calculate completion rate as percentage of active habits completed today
    const completionRate = activeHabits.length > 0 ? 
      (recentlyCompletedHabits / activeHabits.length) * 100 : 0;
    
    return {
      totalHabits: habitsData.length,
      activeStreaks: activeStreaks,
      totalCheckIns: totalCheckIns,
      completionRate: Math.round(completionRate)
    };
  };

  const fetchDashboardData = async () => {
    try {
      if (!user?.id) {
        setHabits([]);
        setStats({
          totalHabits: 0,
          activeStreaks: 0,
          totalCheckIns: 0,
          completionRate: 0
        });
        return;
      }

      const data = await ApiService.get<any[]>(`/habits/user/${user.id}`);
      
      // Convert frequency enum values to strings for frontend
      const convertedHabits = data.map(habit => ({
        ...habit,
        frequency: convertFrequencyToString(habit.frequency)
      }));
      
      setHabits(convertedHabits);
      setStats(calculateStats(convertedHabits));
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
      setHabits([]);
      setStats({
        totalHabits: 0,
        activeStreaks: 0,
        totalCheckIns: 0,
        completionRate: 0
      });
    } finally {
      setLoading(false);
    }
  };

  const refreshDashboard = async () => {
    setLoading(true);
    await fetchDashboardData();
  };

  const updateHabitStreak = (habitId: string, newStreak: number) => {
    setHabits(prevHabits => {
      const updatedHabits = prevHabits.map(habit => 
        habit.id === habitId 
          ? { 
              ...habit, 
              currentStreak: newStreak,
              longestStreak: Math.max(habit.longestStreak, newStreak),
              totalCheckIns: (habit.totalCheckIns || 0) + 1
            }
          : habit
      );
      
      // Recalculate stats after updating streak
      setStats(calculateStats(updatedHabits));
      return updatedHabits;
    });
  };

  const incrementCheckIns = () => {
    setStats(prevStats => ({
      ...prevStats,
      totalCheckIns: prevStats.totalCheckIns + 1
    }));
  };

  const updateHabitData = (habitId: string, updates: Partial<Habit>) => {
    setHabits(prevHabits => {
      const updatedHabits = prevHabits.map(habit => 
        habit.id === habitId 
          ? { ...habit, ...updates }
          : habit
      );
      
      // Recalculate stats after updating habit data
      setStats(calculateStats(updatedHabits));
      return updatedHabits;
    });
  };

  const markHabitCompleted = (habitId: string, completedAt: string, newStreak: number) => {
    setHabits(prevHabits => {
      const updatedHabits = prevHabits.map(habit => 
        habit.id === habitId 
          ? { 
              ...habit, 
              currentStreak: newStreak,
              longestStreak: Math.max(habit.longestStreak, newStreak),
              totalCheckIns: (habit.totalCheckIns || 0) + 1,
              lastCompletedAt: completedAt
            }
          : habit
      );
      
      // Recalculate stats after marking habit as completed
      setStats(calculateStats(updatedHabits));
      return updatedHabits;
    });
  };

  const updateHabitCompletion = (habitId: string, completedAt: string, newStreak: number) => {
    setHabits(prevHabits => {
      const updatedHabits = prevHabits.map(habit => 
        habit.id === habitId 
          ? { 
              ...habit, 
              currentStreak: newStreak,
              longestStreak: Math.max(habit.longestStreak, newStreak),
              totalCheckIns: (habit.totalCheckIns || 0) + 1,
              lastCompletedAt: completedAt
            }
          : habit
      );
      
      // Immediately recalculate stats for real-time updates
      const newStats = calculateStats(updatedHabits);
      setStats(newStats);
      
      return updatedHabits;
    });
  };

  useEffect(() => {
    fetchDashboardData();
  }, [user?.id]);

  const value: DashboardContextType = {
    habits,
    stats,
    loading,
    refreshDashboard,
    updateHabitStreak,
    incrementCheckIns,
    updateHabitData,
    markHabitCompleted,
    updateHabitCompletion
  };

  return (
    <DashboardContext.Provider value={value}>
      {children}
    </DashboardContext.Provider>
  );
}; 