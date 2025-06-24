# HabitChain Animation System

This document outlines the comprehensive animation system implemented in HabitChain using Framer Motion and custom CSS animations.

## 🎯 Overview

The animation system provides smooth, engaging, and accessible animations throughout the application while maintaining performance and user experience best practices.

## 📦 Dependencies

- **Framer Motion**: Main animation library
- **React Router DOM**: For page transitions
- **Custom CSS**: Additional animation effects

## 🏗️ Architecture

### Core Components

#### 1. AnimatedComponents.tsx
Reusable animated components with consistent behavior:

- `AnimatedPage`: Page wrapper with fade-in animations
- `AnimatedCard`: Cards with hover effects and scale animations
- `AnimatedButton`: Interactive buttons with tap feedback
- `AnimatedLink`: Navigation links with hover effects
- `AnimatedIcon`: Icons with rotation and scale animations
- `AnimatedModal`: Modal dialogs with spring animations
- `AnimatedNavbar`: Navigation bar with slide-in effects
- `AnimatedProgress`: Progress bars with smooth transitions
- `AnimatedCounter`: Number counters with scale animations
- `AnimatedNotification`: Toast notifications with spring effects

#### 2. Animation Variants
Predefined animation variants for consistency:

```typescript
// Fade in from bottom
export const fadeInUp: Variants = {
  hidden: { opacity: 0, y: 20 },
  visible: { 
    opacity: 1, 
    y: 0,
    transition: { duration: 0.6, ease: "easeOut" }
  }
};

// Stagger container for lists
export const staggerContainer: Variants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: {
      staggerChildren: 0.1,
      delayChildren: 0.2
    }
  }
};
```

#### 3. Custom Hooks
Animation hooks for common patterns:

- `useScrollAnimation()`: Scroll-triggered animations
- `useStaggerAnimation()`: Staggered list animations
- `useHoverAnimation()`: Hover effects
- `useCardAnimation()`: Card-specific animations

## 🎨 Animation Types

### 1. Page Transitions
- Smooth fade-in/out between pages
- Slide transitions for navigation
- Loading states with spinners

### 2. Component Animations
- **Cards**: Scale and shadow effects on hover
- **Buttons**: Scale feedback on tap
- **Icons**: Rotation and scale animations
- **Lists**: Staggered entrance animations

### 3. Interactive Elements
- **Hover Effects**: Subtle scale and shadow changes
- **Focus States**: Glow effects for accessibility
- **Loading States**: Spinning animations and shimmer effects

### 4. Background Animations
- Floating shapes on landing page
- Gradient animations
- Parallax effects

## 🚀 Usage Examples

### Basic Component Animation
```tsx
import { AnimatedCard } from './components/AnimatedComponents';

<AnimatedCard className="my-card" delay={0.2}>
  <h3>Animated Content</h3>
  <p>This card will animate in with a delay</p>
</AnimatedCard>
```

### Page with Animations
```tsx
import { PageTransition } from './components/AnimatedComponents';

<PageTransition>
  <div className="main-container">
    <h1>My Page</h1>
    <p>This page will animate in smoothly</p>
  </div>
</PageTransition>
```

### Custom Animation Hook
```tsx
import { useScrollAnimation } from './hooks/useAnimations';

const MyComponent = () => {
  const { ref, isInView, variants } = useScrollAnimation();
  
  return (
    <motion.div
      ref={ref}
      variants={variants}
      initial="hidden"
      animate={isInView ? "visible" : "hidden"}
    >
      Content that animates when scrolled into view
    </motion.div>
  );
};
```

### Staggered List Animation
```tsx
import { AnimatedList, AnimatedListItem } from './components/AnimatedComponents';

<AnimatedList className="my-list">
  {items.map((item, index) => (
    <AnimatedListItem key={item.id} index={index}>
      {item.content}
    </AnimatedListItem>
  ))}
</AnimatedList>
```

## 🎭 CSS Animations

### Utility Classes
- `.animate-float`: Floating animation
- `.animate-pulse`: Pulsing effect
- `.animate-bounce`: Bouncing animation
- `.animate-shimmer`: Shimmer loading effect

### Hover Effects
- `.hover-lift`: Lift effect on hover
- `.hover-scale`: Scale effect on hover
- `.hover-glow`: Glow effect on hover

### Loading States
- `.loading-spinner`: Spinning animation
- `.skeleton`: Skeleton loading effect

## ♿ Accessibility

### Reduced Motion Support
The system respects user preferences for reduced motion:

```css
@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}
```

### Focus Management
- Clear focus indicators
- Keyboard navigation support
- Screen reader compatibility

## 📱 Performance Considerations

### Optimization Techniques
1. **Transform-based animations**: Using `transform` instead of layout properties
2. **Hardware acceleration**: Leveraging GPU for smooth animations
3. **Reduced repaints**: Minimizing layout thrashing
4. **Efficient easing**: Using optimized easing functions

### Best Practices
- Keep animations under 300ms for UI feedback
- Use `will-change` sparingly
- Implement proper cleanup for animation listeners
- Test on lower-end devices

## 🧪 Testing Animations

### Manual Testing
1. Test on different devices and screen sizes
2. Verify reduced motion preferences
3. Check performance on slower devices
4. Validate keyboard navigation

### Automated Testing
```tsx
// Example test for animation component
test('AnimatedCard renders with animation', () => {
  render(<AnimatedCard>Test Content</AnimatedCard>);
  expect(screen.getByText('Test Content')).toBeInTheDocument();
});
```

## 🔧 Customization

### Adding New Animations
1. Define variants in `AnimatedComponents.tsx`
2. Create new animated components as needed
3. Add CSS animations in `animations.css`
4. Update documentation

### Modifying Existing Animations
1. Adjust timing and easing in variants
2. Update CSS keyframes
3. Test across different devices
4. Ensure accessibility compliance

## 📚 Resources

- [Framer Motion Documentation](https://www.framer.com/motion/)
- [Web Animation API](https://developer.mozilla.org/en-US/docs/Web/API/Web_Animations_API)
- [CSS Animation Best Practices](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_Animations/Using_CSS_animations)

## 🤝 Contributing

When adding new animations:
1. Follow the existing patterns
2. Test on multiple devices
3. Consider accessibility implications
4. Update this documentation
5. Ensure performance doesn't degrade

---

**Note**: This animation system is designed to enhance user experience without compromising performance or accessibility. Always test animations on various devices and respect user preferences for reduced motion. 