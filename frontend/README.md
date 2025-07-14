# HabitChain Frontend

This is the React frontend for the HabitChain application, a habit tracking and building platform.

## Features

- **Authentication**: Login and registration forms with validation
- **Protected Routes**: Route protection based on authentication status
- **Responsive Design**: Mobile-friendly UI with modern styling
- **JWT Integration**: Secure token-based authentication with refresh tokens

## Getting Started

### Prerequisites

- Node.js (v16 or higher)
- npm or yarn
- Backend API running (see backend README)

### Installation

1. Install dependencies:
```bash
npm install
```

2. Start the development server:
```bash
npm start
```

The application will open at [http://localhost:3000](http://localhost:3000).

## Authentication Forms

### Login Form
- **Email**: Required, must be a valid email format
- **Password**: Required, minimum 6 characters
- **Features**: 
  - Real-time validation
  - Error handling
  - Loading states
  - Automatic redirect to dashboard on success

### Registration Form
- **First Name**: Required, maximum 100 characters
- **Last Name**: Required, maximum 100 characters
- **Email**: Required, must be a valid email format
- **Username**: Required, 3-50 characters, alphanumeric and underscores only
- **Password**: Required, minimum 6 characters, maximum 100 characters
- **Confirm Password**: Required, must match password
- **Features**:
  - Real-time validation
  - Password confirmation
  - Error handling
  - Loading states
  - Automatic redirect to dashboard on success

## Testing the Forms

### Backend Setup
1. Ensure the backend is running on `https://habitchain.onrender.com`
2. The database should be seeded with test data

### Test Scenarios

#### Registration Testing
1. Navigate to `/register`
2. Fill out the form with valid data
3. Submit and verify successful registration
4. Test validation by submitting invalid data

#### Login Testing
1. Navigate to `/login`
2. Use credentials from a registered user
3. Submit and verify successful login
4. Test validation with invalid credentials

### API Endpoints Used

- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Token refresh
- `POST /api/auth/revoke` - Token revocation
- `GET /api/auth/me` - Get current user info

## Project Structure

```
src/
├── components/
│   ├── Login.tsx          # Login form component
│   ├── Register.tsx       # Registration form component
│   ├── Navbar.tsx         # Navigation component
│   ├── ProtectedRoute.tsx # Route protection component
│   ├── PublicRoute.tsx    # Public route component
│   └── AuthForms.css      # Authentication form styles
├── contexts/
│   └── AuthContext.tsx    # Authentication context and API
├── App.tsx                # Main application component
└── index.tsx              # Application entry point
```

## Styling

The authentication forms use a modern, responsive design with:
- Gradient backgrounds
- Card-based layout
- Smooth animations
- Error states
- Loading indicators
- Mobile-first responsive design

## Development

### Available Scripts

- `npm start` - Start development server
- `npm build` - Build for production
- `npm test` - Run tests
- `npm eject` - Eject from Create React App

### Environment Variables

Create a `.env` file in the root directory:

```env
REACT_APP_API_URL=https://habitchain.onrender.com/api
```

## Troubleshooting

### CORS Issues
If you encounter CORS errors, ensure the backend CORS policy allows requests from `http://localhost:3000`.

### API Connection Issues
- Verify the backend is running on the correct port
- Check the `REACT_APP_API_URL` environment variable
- Ensure the backend database is properly seeded

### Form Validation Issues
- Check browser console for JavaScript errors
- Verify all required fields are filled
- Ensure password confirmation matches

## Contributing

1. Follow the existing code style
2. Add proper error handling
3. Include responsive design considerations
4. Test on multiple devices and browsers
