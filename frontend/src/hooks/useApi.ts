import { useState, useCallback } from 'react';
import { toast } from 'react-hot-toast';

interface UseApiState<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
}

interface UseApiReturn<T> extends UseApiState<T> {
  execute: (...args: any[]) => Promise<T | null>;
  reset: () => void;
  setData: (data: T | null) => void;
}

export function useApi<T = any>(
  apiFunction: (...args: any[]) => Promise<T>,
  options?: {
    showSuccessToast?: boolean;
    successMessage?: string;
    showErrorToast?: boolean;
    onSuccess?: (data: T) => void;
    onError?: (error: string) => void;
  }
): UseApiReturn<T> {
  const [state, setState] = useState<UseApiState<T>>({
    data: null,
    loading: false,
    error: null,
  });

  const execute = useCallback(
    async (...args: any[]): Promise<T | null> => {
      setState(prev => ({ ...prev, loading: true, error: null }));

      try {
        const result = await apiFunction(...args);
        
        setState(prev => ({ ...prev, data: result, loading: false }));

        // Show success toast if enabled
        if (options?.showSuccessToast && options?.successMessage) {
          toast.success(options.successMessage);
        }

        // Call onSuccess callback if provided
        if (options?.onSuccess) {
          options.onSuccess(result);
        }

        return result;
      } catch (error: any) {
        const errorMessage = error.response?.data?.message || error.message || 'An error occurred';
        
        setState(prev => ({ ...prev, error: errorMessage, loading: false }));

        // Show error toast if enabled
        if (options?.showErrorToast !== false) {
          toast.error(errorMessage);
        }

        // Call onError callback if provided
        if (options?.onError) {
          options.onError(errorMessage);
        }

        return null;
      }
    },
    [apiFunction, options]
  );

  const reset = useCallback(() => {
    setState({ data: null, loading: false, error: null });
  }, []);

  const setData = useCallback((data: T | null) => {
    setState(prev => ({ ...prev, data }));
  }, []);

  return {
    ...state,
    execute,
    reset,
    setData,
  };
}

// Specialized hooks for common operations
export function useCreate<T, R = any>(
  createFunction: (data: T) => Promise<R>,
  options?: {
    showSuccessToast?: boolean;
    successMessage?: string;
    onSuccess?: (data: R) => void;
  }
) {
  return useApi(createFunction, {
    showSuccessToast: true,
    successMessage: options?.successMessage || 'Created successfully!',
    onSuccess: options?.onSuccess,
    ...options,
  });
}

export function useUpdate<T, R = any>(
  updateFunction: (id: string, data: T) => Promise<R>,
  options?: {
    showSuccessToast?: boolean;
    successMessage?: string;
    onSuccess?: (data: R) => void;
  }
) {
  return useApi(updateFunction, {
    showSuccessToast: true,
    successMessage: options?.successMessage || 'Updated successfully!',
    onSuccess: options?.onSuccess,
    ...options,
  });
}

export function useDelete<R = any>(
  deleteFunction: (id: string) => Promise<R>,
  options?: {
    showSuccessToast?: boolean;
    successMessage?: string;
    onSuccess?: (data: R) => void;
  }
) {
  return useApi(deleteFunction, {
    showSuccessToast: true,
    successMessage: options?.successMessage || 'Deleted successfully!',
    onSuccess: options?.onSuccess,
    ...options,
  });
}

export function useFetch<T>(
  fetchFunction: (...args: any[]) => Promise<T>,
  options?: {
    onSuccess?: (data: T) => void;
    onError?: (error: string) => void;
  }
) {
  return useApi(fetchFunction, {
    showErrorToast: true,
    onSuccess: options?.onSuccess,
    onError: options?.onError,
    ...options,
  });
} 