# Authentication & Route Protection

This document describes the comprehensive authentication and route protection system implemented in the HabitChain frontend.

## 🛡️ Route Protection

### ProtectedRoute Component

The `ProtectedRoute` component ensures that only authenticated users can access specific routes.

**Features:**
- ✅ **Authentication Check**: Verifies user is logged in
- ✅ **Token Expiration Check**: Automatically detects expired tokens
- ✅ **Loading States**: Shows spinner while checking authentication
- ✅ **Account Status Check**: Verifies user account is active
- ✅ **Redirect Handling**: Saves attempted location for post-login redirect

**Usage:**
```tsx
<Route 
  path="/dashboard" 
  element={
    <ProtectedRoute>
      <Dashboard />
    </ProtectedRoute>
  } 
/>
```

**Props:**
- `children`: React components to render if authenticated
- `redirectTo`: Where to redirect if not authenticated (default: `/login`)
- `requireActiveUser`: Whether to check if user account is active (default: `true`)

### PublicRoute Component

The `PublicRoute` component prevents authenticated users from accessing public routes (like login/register).

**Features:**
- ✅ **Reverse Protection**: Redirects authenticated users away from public routes
- ✅ **Smart Redirects**: Returns users to their originally intended destination
- ✅ **Token Expiration Handling**: Respects token expiration status

**Usage:**
```tsx
<Route 
  path="/login" 
  element={
    <PublicRoute>
      <Login />
    </PublicRoute>
  } 
/>
```

## 🔐 Authentication Context

### AuthContext Features

The `AuthContext` provides comprehensive authentication state management:

**State Management:**
- ✅ **User Information**: Current user data
- ✅ **Token Management**: Access and refresh tokens
- ✅ **Authentication Status**: Real-time authentication state
- ✅ **Token Expiration**: Automatic expiration detection
- ✅ **Loading States**: Loading indicators for async operations

**Authentication Methods:**
- ✅ **Login**: User authentication with credentials
- ✅ **Register**: New user registration
- ✅ **Logout**: Secure logout with token revocation
- ✅ **Token Refresh**: Automatic token refresh
- ✅ **Password Change**: Secure password updates

### Token Expiration Handling

**Automatic Detection:**
- ✅ **JWT Parsing**: Decodes and validates token expiration
- ✅ **Periodic Checks**: Checks token expiration every minute
- ✅ **Request Interception**: Validates tokens before API requests
- ✅ **Automatic Refresh**: Attempts token refresh on expiration

**User Experience:**
- ✅ **Session Warnings**: Shows expiration warnings 5 minutes before expiry
- ✅ **Graceful Degradation**: Handles failed refreshes gracefully
- ✅ **Automatic Redirects**: Redirects to login on complete expiration

## ⏰ Session Expiration

### SessionExpiration Component

Provides proactive session management with user-friendly warnings.

**Features:**
- ✅ **Early Warning**: Shows warning 5 minutes before expiration
- ✅ **Time Display**: Shows remaining time in minutes
- ✅ **Action Options**: Extend session or logout immediately
- ✅ **Modal Interface**: Non-intrusive modal overlay
- ✅ **Responsive Design**: Works on all device sizes

**Configuration:**
```tsx
<SessionExpiration warningMinutes={5} />
```

**User Actions:**
- **Extend Session**: Automatically refreshes tokens
- **Logout Now**: Immediately logs out user

## 🔄 Token Refresh Strategy

### Automatic Refresh

**When it happens:**
- ✅ **API Request Interception**: On 401 responses
- ✅ **Periodic Checks**: Every minute during active sessions
- ✅ **Route Navigation**: When accessing protected routes
- ✅ **Component Mount**: When authentication context initializes

**Refresh Process:**
1. **Detect Expiration**: Check if current token is expired
2. **Attempt Refresh**: Use refresh token to get new access token
3. **Update State**: Store new tokens and user data
4. **Retry Request**: Retry original failed request
5. **Fallback**: Redirect to login if refresh fails

### Manual Refresh

**Available Methods:**
```tsx
const { refreshAuth } = useAuth();

// Manually refresh tokens
await refreshAuth();
```

## 🚪 Logout Handling

### Secure Logout Process

**Steps:**
1. **Token Revocation**: Revokes refresh token on server
2. **Local Cleanup**: Removes tokens from localStorage
3. **State Reset**: Clears authentication state
4. **Redirect**: Redirects to login page
5. **Page Reload**: Ensures complete state cleanup

**Implementation:**
```tsx
const { logout } = useAuth();

// Secure logout
await logout();
```

## 🎨 UI/UX Features

### Loading States

**Route Protection Loading:**
- ✅ **Spinner Animation**: Smooth loading spinner
- ✅ **Status Message**: "Checking authentication..."
- ✅ **Consistent Design**: Matches app theme

**Authentication Loading:**
- ✅ **Button States**: Disabled buttons during operations
- ✅ **Loading Text**: Dynamic button text
- ✅ **Form Disabled**: Prevents multiple submissions

### Error Handling

**Form Validation:**
- ✅ **Real-time Validation**: Validates as user types
- ✅ **Error Display**: Clear error messages
- ✅ **Field Highlighting**: Visual error indicators

**API Error Handling:**
- ✅ **Network Errors**: Handles connection issues
- ✅ **Server Errors**: Displays server error messages
- ✅ **Token Errors**: Handles authentication failures

### Responsive Design

**Mobile Support:**
- ✅ **Touch-friendly**: Large touch targets
- ✅ **Responsive Layout**: Adapts to screen size
- ✅ **Keyboard Navigation**: Full keyboard support

**Accessibility:**
- ✅ **Focus Management**: Proper focus indicators
- ✅ **Screen Reader**: ARIA labels and descriptions
- ✅ **High Contrast**: High contrast mode support
- ✅ **Reduced Motion**: Respects motion preferences

## 🔧 Configuration

### Environment Variables

**API Configuration:**
```env
REACT_APP_API_URL=https://habitchain.onrender.com/api
```

### JWT Settings

**Token Configuration:**
- **Access Token**: 60 minutes (configurable)
- **Refresh Token**: 7 days
- **Warning Time**: 5 minutes before expiration

## 🧪 Testing

### Test Scenarios

**Authentication Flow:**
1. **Registration**: Create new account
2. **Login**: Authenticate with credentials
3. **Token Refresh**: Verify automatic refresh
4. **Logout**: Test secure logout
5. **Session Expiration**: Test expiration handling

**Route Protection:**
1. **Protected Routes**: Verify access control
2. **Public Routes**: Verify redirect behavior
3. **Token Expiration**: Test expired token handling
4. **Account Status**: Test inactive account handling

### Error Scenarios

**Network Issues:**
- ✅ **Offline Handling**: Graceful offline behavior
- ✅ **Timeout Handling**: Request timeout management
- ✅ **Retry Logic**: Automatic retry on failures

**Token Issues:**
- ✅ **Expired Tokens**: Automatic refresh attempts
- ✅ **Invalid Tokens**: Proper error handling
- ✅ **Missing Tokens**: Redirect to login

## 🚀 Performance

### Optimization Features

**Efficient Token Checking:**
- ✅ **Debounced Checks**: Prevents excessive API calls
- ✅ **Cached Validation**: Reduces redundant checks
- ✅ **Lazy Loading**: Loads auth state on demand

**Memory Management:**
- ✅ **Cleanup**: Proper cleanup on unmount
- ✅ **State Optimization**: Minimal state updates
- ✅ **Event Cleanup**: Removes event listeners

## 🔒 Security Features

### Token Security

**Storage:**
- ✅ **localStorage**: Secure token storage
- ✅ **Automatic Cleanup**: Removes tokens on logout
- ✅ **Expiration Handling**: Respects token expiration

**Transmission:**
- ✅ **HTTPS Only**: Secure API communication
- ✅ **Authorization Headers**: Proper token transmission
- ✅ **CORS Protection**: Cross-origin request handling

### User Security

**Account Protection:**
- ✅ **Active Status Check**: Verifies account is active
- ✅ **Session Management**: Proper session handling
- ✅ **Secure Logout**: Complete session termination

## 📱 Mobile Considerations

### Mobile-Specific Features

**Touch Interface:**
- ✅ **Large Buttons**: Touch-friendly button sizes
- ✅ **Gesture Support**: Swipe and tap gestures
- ✅ **Virtual Keyboard**: Keyboard-aware layouts

**Performance:**
- ✅ **Optimized Loading**: Fast loading on mobile
- ✅ **Battery Efficient**: Minimal background processing
- ✅ **Network Aware**: Handles poor connections

## 🔄 Integration

### Backend Integration

**API Endpoints:**
- ✅ **Authentication**: `/api/auth/login`, `/api/auth/register`
- ✅ **Token Management**: `/api/auth/refresh`, `/api/auth/revoke`
- ✅ **User Management**: `/api/auth/me`, `/api/auth/change-password`

**Error Handling:**
- ✅ **Consistent Responses**: Standardized error format
- ✅ **Status Codes**: Proper HTTP status codes
- ✅ **Error Messages**: User-friendly error messages

### State Management

**Context Integration:**
- ✅ **React Context**: Centralized state management
- ✅ **Hook-based**: Easy-to-use custom hooks
- ✅ **TypeScript**: Full type safety

## 🎯 Best Practices

### Code Organization

**Component Structure:**
- ✅ **Separation of Concerns**: Clear component responsibilities
- ✅ **Reusable Components**: Modular, reusable components
- ✅ **Consistent Patterns**: Standardized implementation patterns

**Error Handling:**
- ✅ **Graceful Degradation**: Handles errors gracefully
- ✅ **User Feedback**: Clear error messages
- ✅ **Recovery Options**: Provides recovery mechanisms

### User Experience

**Accessibility:**
- ✅ **WCAG Compliance**: Follows accessibility guidelines
- ✅ **Keyboard Navigation**: Full keyboard support
- ✅ **Screen Reader**: Screen reader compatibility

**Performance:**
- ✅ **Fast Loading**: Optimized loading times
- ✅ **Smooth Interactions**: Responsive user interactions
- ✅ **Efficient Updates**: Minimal re-renders

This authentication system provides a robust, secure, and user-friendly experience for the HabitChain application. 