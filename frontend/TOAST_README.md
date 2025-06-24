# Toast Notifications with react-hot-toast

This document explains how to use the toast notification system in the HabitChain application.

## Overview

The application uses `react-hot-toast` for displaying toast notifications. A custom hook `useToast` has been created to provide a consistent and easy-to-use interface for showing different types of notifications.

## Setup

The toast system is already configured in `App.tsx` with the following features:

- **Position**: Top-right corner
- **Duration**: 4 seconds (4000ms)
- **Styling**: Dark theme with glass-morphism design
- **Icons**: Custom colored icons for different toast types

## Usage

### Basic Usage

Import the `useToast` hook in your component:

```tsx
import { useToast } from '../hooks/useToast';

const MyComponent = () => {
  const { showSuccess, showError, showLoading, showCustom, dismiss } = useToast();
  
  // Use the toast functions...
};
```

### Available Toast Types

#### 1. Success Toast
```tsx
showSuccess('Operation completed successfully!');
```

#### 2. Error Toast
```tsx
showError('Something went wrong. Please try again.');
```

#### 3. Loading Toast
```tsx
const loadingToast = showLoading('Processing your request...');

// Later, dismiss the loading toast
dismiss(loadingToast);

// Or show a success/error toast
showSuccess('Request completed!');
```

#### 4. Custom Toast
```tsx
showCustom('Custom message', {
  icon: '🎉',
  duration: 6000,
  style: {
    background: '#363636',
    color: '#fff',
  },
});
```

#### 5. Promise Toast
```tsx
import { toast } from 'react-hot-toast';

const promise = someAsyncOperation();

toast.promise(
  promise,
  {
    loading: 'Processing...',
    success: 'Operation completed successfully!',
    error: 'Operation failed. Please try again.',
  }
);
```

### Dismissing Toasts

#### Dismiss Specific Toast
```tsx
const toastId = showLoading('Loading...');
// Later...
dismiss(toastId);
```

#### Dismiss All Toasts
```tsx
dismiss(); // Dismisses all active toasts
```

## Real-World Examples

### Form Submission
```tsx
const handleSubmit = async (e: React.FormEvent) => {
  e.preventDefault();
  
  const loadingToast = showLoading('Submitting form...');
  
  try {
    await submitForm(formData);
    dismiss(loadingToast);
    showSuccess('Form submitted successfully!');
  } catch (error) {
    dismiss(loadingToast);
    showError('Failed to submit form. Please try again.');
  }
};
```

### API Call with Promise
```tsx
const handleApiCall = () => {
  const promise = fetch('/api/data')
    .then(response => response.json())
    .then(data => {
      // Process data
      return 'Data loaded successfully!';
    });

  toast.promise(
    promise,
    {
      loading: 'Loading data...',
      success: (message) => message,
      error: 'Failed to load data',
    }
  );
};
```

### Validation Error
```tsx
const validateAndSubmit = () => {
  if (!isValid) {
    showError('Please fill in all required fields.');
    return;
  }
  
  // Proceed with submission...
};
```

## Toast Configuration

The toast configuration is set in `App.tsx`:

```tsx
<Toaster
  position="top-right"
  toastOptions={{
    duration: 4000,
    style: {
      background: '#363636',
      color: '#fff',
      borderRadius: '12px',
      padding: '16px',
      fontSize: '14px',
      fontWeight: '500',
      boxShadow: '0 8px 32px rgba(0, 0, 0, 0.12)',
      border: '1px solid rgba(255, 255, 255, 0.1)',
    },
    success: {
      iconTheme: {
        primary: '#10b981',
        secondary: '#fff',
      },
    },
    error: {
      iconTheme: {
        primary: '#ef4444',
        secondary: '#fff',
      },
    },
    loading: {
      iconTheme: {
        primary: '#3b82f6',
        secondary: '#fff',
      },
    },
  }}
/>
```

## Best Practices

1. **Use appropriate toast types**: Use success for positive outcomes, error for failures, and loading for async operations.

2. **Keep messages concise**: Toast messages should be brief and clear.

3. **Dismiss loading toasts**: Always dismiss loading toasts before showing success/error toasts.

4. **Use promises for async operations**: The `toast.promise` method is perfect for API calls and other async operations.

5. **Don't overuse toasts**: Use toasts for important feedback, not for every minor action.

6. **Consider accessibility**: Toast messages should be meaningful for screen readers.

## Customization

You can customize the toast appearance by modifying the `toastOptions` in `App.tsx` or by passing custom options to individual toast calls.

### Custom Styling
```tsx
showCustom('Message', {
  style: {
    background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
    color: '#fff',
    borderRadius: '20px',
  },
});
```

### Custom Duration
```tsx
showSuccess('Quick message', { duration: 2000 });
```

## Testing

To test the toast system, you can use the `ToastExamples` component which demonstrates all available toast types.

## Troubleshooting

- **Toasts not appearing**: Make sure the `Toaster` component is rendered in your app.
- **Loading toasts not dismissing**: Ensure you're calling `dismiss()` with the correct toast ID.
- **Styling issues**: Check that the toast configuration in `App.tsx` is correct.

## Additional Resources

- [react-hot-toast Documentation](https://react-hot-toast.com/)
- [react-hot-toast GitHub](https://github.com/timolins/react-hot-toast) 