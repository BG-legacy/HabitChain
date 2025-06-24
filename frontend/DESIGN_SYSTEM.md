# HabitChain Design System

## Overview

HabitChain uses a modern glassmorphism design system with a black and silver color palette. The design emphasizes minimalism, clean layouts, and smooth interactions while maintaining excellent accessibility and user experience.

## Color Palette

### Primary Colors
- **Black**: `#0a0a0a` - Main background color
- **Dark Gray**: `#1a1a1a` - Secondary background
- **Charcoal**: `#2d2d2d` - Tertiary background
- **Silver**: `#c0c0c0` - Primary text color
- **Light Silver**: `#e8e8e8` - Secondary text color
- **White**: `#ffffff` - Headings and highlights

### Accent Colors
- **Primary Accent**: `#4a90e2` - Primary actions and links
- **Accent Hover**: `#357abd` - Hover states for primary accent

### Status Colors
- **Success**: `#2ecc71` - Positive actions and states
- **Warning**: `#f1c40f` - Caution and warnings
- **Error**: `#e74c3c` - Errors and destructive actions
- **Info**: `#4a90e2` - Informational content

## Glassmorphism Properties

### Core Glass Effects
```css
--glass-bg: rgba(255, 255, 255, 0.1);
--glass-border: rgba(255, 255, 255, 0.2);
--glass-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
--glass-blur: blur(10px);
```

### Glass Card Class
The `.glass-card` class provides a complete glassmorphism effect:
- Semi-transparent background
- Backdrop blur effect
- Subtle border
- Soft shadow
- Hover animations

## Typography

### Font Family
- **Primary**: Inter (Google Fonts)
- **Fallback**: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', sans-serif

### Font Sizes
```css
--font-size-xs: 0.75rem;    /* 12px */
--font-size-sm: 0.875rem;   /* 14px */
--font-size-base: 1rem;     /* 16px */
--font-size-lg: 1.125rem;   /* 18px */
--font-size-xl: 1.25rem;    /* 20px */
--font-size-2xl: 1.5rem;    /* 24px */
--font-size-3xl: 1.875rem;  /* 30px */
--font-size-4xl: 2.25rem;   /* 36px */
```

### Font Weights
- **Light**: 300
- **Regular**: 400
- **Medium**: 500
- **Semi-bold**: 600
- **Bold**: 700
- **Extra Bold**: 800

## Spacing System

### Spacing Scale
```css
--spacing-xs: 0.25rem;   /* 4px */
--spacing-sm: 0.5rem;    /* 8px */
--spacing-md: 1rem;      /* 16px */
--spacing-lg: 1.5rem;    /* 24px */
--spacing-xl: 2rem;      /* 32px */
--spacing-2xl: 3rem;     /* 48px */
```

### Utility Classes
- `.p-sm`, `.p-md`, `.p-lg` - Padding
- `.mt-sm`, `.mt-md`, `.mt-lg` - Margin top
- `.mb-sm`, `.mb-md`, `.mb-lg` - Margin bottom
- `.gap-sm`, `.gap-md`, `.gap-lg` - Gap

## Border Radius

### Radius Scale
```css
--radius-sm: 0.375rem;   /* 6px */
--radius-md: 0.5rem;     /* 8px */
--radius-lg: 0.75rem;    /* 12px */
--radius-xl: 1rem;       /* 16px */
--radius-2xl: 1.5rem;    /* 24px */
```

### Utility Classes
- `.rounded-sm`, `.rounded-md`, `.rounded-lg`, `.rounded-xl`

## Transitions

### Transition Durations
```css
--transition-fast: 0.15s ease;
--transition-normal: 0.3s ease;
--transition-slow: 0.5s ease;
```

## Components

### Buttons

#### Primary Button
```css
.btn.btn-primary
```
- Gradient background (accent colors)
- White text
- Hover animations
- Shadow effects

#### Secondary Button
```css
.btn.btn-secondary
```
- Glass background
- Silver text
- Subtle hover effects

### Cards

#### Glass Card
```css
.glass-card
```
- Glassmorphism effect
- Hover animations
- Consistent spacing
- Border radius

### Form Elements

#### Input Fields
```css
.form-input
```
- Glass background
- Focus states
- Error states
- Placeholder styling

### Status Indicators

#### Status Badges
```css
.status-badge.status-success
.status-badge.status-warning
.status-badge.status-error
.status-badge.status-info
```

### Progress Indicators

#### Progress Bar
```css
.progress-bar
.progress-fill
```
- Animated progress
- Shimmer effect
- Glass background

## Layout Components

### Page Structure
```css
.main-container
.page-header
.page-title
.page-subtitle
```

### Grid System
```css
.grid
.grid-cols-1
.grid-cols-2
.grid-cols-3
.grid-cols-4
```

### Section Headers
```css
.section-header
.section-title
.section-actions
```

## States

### Loading States
```css
.loading-container
.loading-spinner
.loading-text
```

### Empty States
```css
.empty-state
.empty-icon
.empty-title
.empty-description
```

### Error States
```css
.error-container
.error-icon
.error-title
.error-message
```

## Animations

### Keyframe Animations
- `fadeIn` - Fade in from bottom
- `slideIn` - Slide in from left
- `pulse` - Pulsing effect
- `spin` - Loading spinner
- `shimmer` - Progress bar effect

### Animation Classes
- `.fade-in`
- `.slide-in`
- `.pulse`

## Responsive Design

### Breakpoints
- **Mobile**: < 640px
- **Tablet**: 640px - 1024px
- **Desktop**: > 1024px

### Responsive Utilities
- Grid columns adjust automatically
- Spacing scales down on mobile
- Typography adjusts for screen size

## Accessibility

### Focus States
- High contrast focus indicators
- Keyboard navigation support
- Screen reader compatibility

### High Contrast Mode
- Enhanced contrast for accessibility
- Maintained glassmorphism effects
- Clear visual hierarchy

### Reduced Motion
- Respects user preferences
- Disables animations when requested
- Maintains functionality

## Usage Examples

### Creating a Glass Card
```html
<div class="glass-card p-lg">
  <h3>Card Title</h3>
  <p>Card content goes here</p>
</div>
```

### Creating a Button
```html
<button class="btn btn-primary">
  Primary Action
</button>
```

### Creating a Form
```html
<form class="auth-form">
  <div class="form-group">
    <label class="form-label">Email</label>
    <input type="email" class="form-input" placeholder="Enter your email">
  </div>
</form>
```

### Creating a Grid Layout
```html
<div class="grid grid-cols-3 gap-lg">
  <div class="glass-card">Item 1</div>
  <div class="glass-card">Item 2</div>
  <div class="glass-card">Item 3</div>
</div>
```

## Best Practices

### Do's
- Use glassmorphism effects sparingly
- Maintain consistent spacing
- Follow the color palette
- Use semantic HTML
- Test accessibility features

### Don'ts
- Overuse glass effects
- Mix different design systems
- Ignore responsive design
- Skip accessibility considerations
- Use hardcoded colors

## Browser Support

### Modern Browsers
- Chrome 88+
- Firefox 87+
- Safari 14+
- Edge 88+

### Fallbacks
- Backdrop-filter fallbacks for older browsers
- Graceful degradation for unsupported features
- Progressive enhancement approach

## Performance Considerations

### Optimizations
- CSS custom properties for theming
- Efficient animations using transform
- Minimal reflows and repaints
- Optimized font loading

### Loading Strategy
- Preconnect to Google Fonts
- Critical CSS inlined
- Non-critical CSS loaded asynchronously
- Optimized asset delivery

## Future Enhancements

### Planned Features
- Dark/light theme toggle
- Custom color schemes
- Advanced animation library
- Component library documentation
- Design token export

### Roadmap
- CSS-in-JS integration
- Design system automation
- Component testing framework
- Accessibility audit tools
- Performance monitoring 