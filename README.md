# HabitChain 🔗
<img width="2224" height="1222" alt="Screenshot 2025-07-22 at 7 43 21 PM" src="https://github.com/user-attachments/assets/0cc77f9b-269c-418f-aae4-0c2594da4f18" />


A modern, full-stack habit tracking application built with .NET 9 and React. HabitChain helps users build and maintain positive habits through gamification, AI-powered recommendations, and social encouragement features.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19.1.0-blue.svg)](https://reactjs.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-12+-blue.svg)](https://www.postgresql.org/)

## 🌟 Features

### Core Functionality
- **🎯 Habit Creation & Management** - Create, edit, and organize habits with custom colors and icons
- **📊 Daily Check-ins** - Track habit completion with notes and streak tracking
- **🔥 Streak Tracking** - Monitor consecutive completion streaks and personal bests
- **📅 Calendar View** - Visual heat map showing habit completion patterns
- **🏆 Badge System** - Earn achievements for streaks, consistency, and milestones
- **📈 Progress Analytics** - Detailed statistics and progress visualizations

### Advanced Features
- **🤖 AI-Powered Recommendations** - Get personalized habit suggestions using OpenAI
- **📋 Data Export** - Export habit data in CSV
- **🔔 Session Management** - Smart session expiration warnings
- **📱 Responsive Design** - Modern glassmorphism UI optimized for all devices

### Technical Features
- **🔐 JWT Authentication** - Secure token-based authentication with refresh tokens
- **🛡️ Input Validation** - Comprehensive server-side and client-side validation
- **🎨 Modern UI/UX** - Glassmorphism design with smooth animations
- **⚡ Real-time Updates** - Instant UI updates with optimistic rendering
- **🧪 Comprehensive Testing** - Unit tests with high code coverage

## 🏗️ Architecture

### Backend (.NET 9)
```
HabitChain.Backend/
├── HabitChain.Domain/          # Core entities, enums, and interfaces
├── HabitChain.Application/     # Business logic, DTOs, and services
├── HabitChain.Infrastructure/  # Data access, repositories, and external services
├── HabitChain.WebAPI/         # REST API controllers and configuration
└── HabitChain.Tests/          # Unit and integration tests
```

**Clean Architecture Principles:**
- **Domain Layer**: Core business entities and rules
- **Application Layer**: Use cases and business logic
- **Infrastructure Layer**: Data persistence and external services
- **Presentation Layer**: REST API endpoints

### Frontend (React 19)
```
frontend/src/
├── components/          # React components
├── contexts/           # React context providers
├── hooks/              # Custom React hooks
├── services/           # API service layers
├── styles/             # Global styles and animations
└── config/             # Configuration files
```

## 🚀 Quick Start

### Prerequisites
- **Backend**: .NET 9 SDK, PostgreSQL 12+, Entity Framework Core Tools
- **Frontend**: Node.js 16+, npm or yarn
- **AI Features**: OpenAI API key (optional)

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/HabitChain.git
cd HabitChain
```

### 2. Backend Setup

#### Environment Configuration
```bash
cd backend
cp env.template .env
```

Edit `.env` with your configuration:
```env
# Database
DB_HOST=localhost
DB_NAME=HabitChainDb
DB_USERNAME=postgres
DB_PASSWORD=your_password
DEV_DB_NAME=HabitChainDb_Dev

# JWT Authentication
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
JWT_ISSUER=HabitChainAPI
JWT_AUDIENCE=HabitChainClient
JWT_EXPIRE_MINUTES=60

# OpenAI (Optional)
OPENAI_API_KEY=sk-your-openai-key-here
OPENAI_MODEL=gpt-4o-mini
```

#### Database Setup
```bash
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create and seed the database
dotnet ef database update --project HabitChain.Infrastructure --startup-project HabitChain.WebAPI

# Or use the development script
./dev-db.sh
```

#### Run the Backend
```bash
dotnet run --project HabitChain.WebAPI
```

Backend will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

### 3. Frontend Setup

```bash
cd frontend
npm install

# Create environment file
echo "REACT_APP_API_URL=https://localhost:5001/api" > .env

# Start development server
npm start
```

Frontend will be available at `http://localhost:3000`

### 4. Test the Application

#### Using Seeded Data
The application comes with pre-seeded test data:

**Test Users:**
- Email: `john.doe@example.com`, Password: `TestPass123!`
- Email: `jane.smith@example.com`, Password: `TestPass123!`
- Email: `bob.johnson@example.com`, Password: `TestPass123!`

#### API Testing
Use the provided HTTP files:
```bash
# Test authentication endpoints
backend/HabitChain.WebAPI/auth-test.http
```

## 📚 API Documentation

### Authentication Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | User login |
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/revoke` | Revoke refresh token |
| GET | `/api/auth/me` | Get current user info |

### Habit Management
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/habits/user/{userId}` | Get user's habits |
| GET | `/api/habits/{id}` | Get habit by ID |
| POST | `/api/habits` | Create new habit |
| PUT | `/api/habits/{id}` | Update habit |
| DELETE | `/api/habits/{id}` | Delete habit |

### Check-ins & Progress
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/checkins/user/{userId}` | Get user's check-ins |
| POST | `/api/checkins` | Create check-in |
| GET | `/api/checkins/habit/{habitId}/stats` | Get habit statistics |

### Additional Endpoints
- **Badges**: `/api/badges/*` - Badge management and achievement tracking
- **Encouragements**: `/api/encouragements/*` - Social features
- **AI Recommendations**: `/api/ai-recommendations/*` - AI-powered suggestions

For complete API documentation, visit `/swagger` when running the backend.

## 🎨 Design System

HabitChain uses a modern **glassmorphism** design system with:

### Color Palette
- **Primary**: Black (`#0a0a0a`) and Silver (`#c0c0c0`)
- **Accent**: Blue (`#4a90e2`) for interactive elements
- **Status**: Success (`#2ecc71`), Warning (`#f1c40f`), Error (`#e74c3c`)

### Key Features
- **Glass Cards**: Semi-transparent backgrounds with backdrop blur
- **Smooth Animations**: Framer Motion powered transitions
- **Responsive Design**: Mobile-first approach
- **Accessibility**: WCAG 2.1 compliant

## 🧪 Testing

### Backend Testing
```bash
cd backend
dotnet test
```

**Test Coverage:**
- **Services**: 90%+ coverage for business logic
- **Controllers**: API endpoint testing
- **Repositories**: Data access layer testing
- **Integration**: End-to-end API testing

### Frontend Testing
```bash
cd frontend
npm test
```

**Testing Stack:**
- **Jest**: Unit testing framework
- **React Testing Library**: Component testing
- **User Event**: User interaction testing

## 🔧 Development

### Backend Development
```bash
# Watch mode for development
dotnet watch run --project HabitChain.WebAPI

# Create new migration
dotnet ef migrations add MigrationName --project HabitChain.Infrastructure --startup-project HabitChain.WebAPI

# Update database
dotnet ef database update --project HabitChain.Infrastructure --startup-project HabitChain.WebAPI
```

### Frontend Development
```bash
# Development server with hot reload
npm start

# Build for production
npm run build

# Run tests in watch mode
npm test -- --watch
```

### Code Quality Tools
- **Backend**: Built-in analyzers, StyleCop
- **Frontend**: ESLint, Prettier (configured)
- **Git Hooks**: Pre-commit validation

## 📱 Features in Detail

### Habit Management
- **Smart Categorization**: Color-coded and icon-based organization
- **Flexible Frequencies**: Daily, weekly, monthly, or custom schedules
- **Streak Tracking**: Current and longest streak monitoring
- **Progress Visualization**: Charts and statistics

### Gamification
- **Achievement System**: 25+ unique badges
- **Streak Challenges**: Consecutive completion rewards
- **Progress Milestones**: Celebration of key achievements
- **Social Recognition**: Share achievements with friends

### AI Integration
- **Personalized Suggestions**: Habits tailored to your goals
- **Smart Reminders**: AI-powered reminder optimization
- **Progress Analysis**: Insights into habit patterns
- **Adaptive Recommendations**: Evolving suggestions based on success

### Data & Privacy
- **Export Options**: JSON, CSV, PDF formats
- **Data Ownership**: Complete control over your data
- **Privacy First**: No tracking, no ads
- **Secure Storage**: Encrypted sensitive information

## 🚀 Deployment

### Backend Deployment
1. **Docker**: Use provided Dockerfile
2. **Cloud**: Deploy to Azure, AWS, or DigitalOcean
3. **Database**: PostgreSQL on cloud provider
4. **Environment**: Set production environment variables

### Frontend Deployment
1. **Static Hosting**: Netlify, Vercel, or GitHub Pages
2. **CDN**: CloudFlare for global distribution
3. **Environment**: Set production API URL

### Production Considerations
- **SSL/TLS**: Required for production
- **Rate Limiting**: Implemented for API endpoints
- **Logging**: Structured logging with Serilog
- **Monitoring**: Application Insights integration ready

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Workflow
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Code Standards
- **Backend**: Follow C# conventions and Clean Architecture
- **Frontend**: Use TypeScript, follow React best practices
- **Testing**: Maintain test coverage above 80%
- **Documentation**: Update README for new features

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Design Inspiration**: Modern glassmorphism trends
- **Icons**: React Icons library
- **Animations**: Framer Motion
- **AI Integration**: OpenAI GPT models
- **Database**: PostgreSQL community

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/HabitChain/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/HabitChain/discussions)
- **Email**: support@habitchain.app

---

**Made with ❤️ by the HabitChain Team**

*Building better habits, one day at a time.*
