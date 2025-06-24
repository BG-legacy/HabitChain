# Reusable Components Documentation

This document provides comprehensive documentation for the reusable components in the HabitChain frontend application.

## Table of Contents

1. [HabitCard](#habitcard)
2. [GroupList](#grouplist)
3. [BadgeCard](#badgecard)
4. [Modal](#modal)
5. [CalendarHeatmap](#calendarheatmap)

## HabitCard

A reusable card component for displaying habit information with stats and actions.

### Props

```typescript
interface HabitCardProps {
  habit: {
    id: string;
    name: string;
    description: string;
    frequency: string;
    targetDays: number;
    currentStreak: number;
    longestStreak: number;
    totalCheckIns: number;
    createdAt: string;
    isActive: boolean;
  };
  onToggleActive?: (id: string) => void;
  onDelete?: (id: string) => void;
  onEdit?: (id: string) => void;
  showActions?: boolean;
  className?: string;
}
```

### Usage Example

```tsx
import { HabitCard } from '../components';

const habit = {
  id: '1',
  name: 'Morning Exercise',
  description: '30 minutes of cardio or strength training',
  frequency: 'Daily',
  targetDays: 1,
  currentStreak: 7,
  longestStreak: 15,
  totalCheckIns: 45,
  createdAt: '2024-01-01T00:00:00Z',
  isActive: true
};

const MyComponent = () => {
  const handleToggleActive = (id: string) => {
    console.log('Toggle habit:', id);
  };

  const handleDelete = (id: string) => {
    console.log('Delete habit:', id);
  };

  return (
    <HabitCard
      habit={habit}
      onToggleActive={handleToggleActive}
      onDelete={handleDelete}
      showActions={true}
    />
  );
};
```

### Features

- Displays habit statistics (current streak, longest streak, total check-ins)
- Color-coded streak indicators
- Action buttons for edit, toggle, and delete
- Responsive design
- Hover effects and animations

## GroupList

A reusable list component for displaying groups with member statistics and actions.

### Props

```typescript
interface GroupListProps {
  groups: Group[];
  onJoinGroup?: (groupId: string) => void;
  onLeaveGroup?: (groupId: string) => void;
  onViewGroup?: (groupId: string) => void;
  showActions?: boolean;
  className?: string;
  emptyMessage?: string;
  loading?: boolean;
}
```

### Usage Example

```tsx
import { GroupList } from '../components';

const groups = [
  {
    id: '1',
    name: 'Morning Warriors',
    description: 'Early risers who exercise before 8 AM',
    memberCount: 15,
    maxMembers: 20,
    isPublic: true,
    createdAt: '2024-01-01T00:00:00Z',
    isMember: true,
    chainStatus: {
      totalMembers: 15,
      activeMembers: 12,
      currentStreak: 5,
      longestStreak: 12
    }
  }
];

const MyComponent = () => {
  const handleJoinGroup = (groupId: string) => {
    console.log('Join group:', groupId);
  };

  const handleLeaveGroup = (groupId: string) => {
    console.log('Leave group:', groupId);
  };

  return (
    <GroupList
      groups={groups}
      onJoinGroup={handleJoinGroup}
      onLeaveGroup={handleLeaveGroup}
      showActions={true}
    />
  );
};
```

### Features

- Displays group statistics and member counts
- Visual streak indicators
- Privacy badges (public/private)
- Member status indicators
- Loading and empty states
- Responsive design

## BadgeCard

A reusable card component for displaying badges with progress tracking and rarity levels.

### Props

```typescript
interface BadgeCardProps {
  badge: Badge;
  onClick?: (badge: Badge) => void;
  showProgress?: boolean;
  showEarnedDate?: boolean;
  className?: string;
  size?: 'small' | 'medium' | 'large';
}
```

### Usage Example

```tsx
import { BadgeCard } from '../components';

const badge = {
  id: '1',
  name: 'First Steps',
  description: 'Complete your first habit check-in',
  icon: '👣',
  category: 'milestone',
  isEarned: true,
  earnedDate: '2024-01-01T10:00:00Z',
  rarity: 'common'
};

const MyComponent = () => {
  const handleBadgeClick = (badge: Badge) => {
    console.log('Badge clicked:', badge);
  };

  return (
    <BadgeCard
      badge={badge}
      onClick={handleBadgeClick}
      showProgress={true}
      showEarnedDate={true}
      size="medium"
    />
  );
};
```

### Features

- Rarity-based color coding
- Progress tracking for unearned badges
- Category indicators
- Multiple size options
- Click handlers for interactions
- Visual feedback for earned vs locked badges

## Modal

A flexible modal component with various configuration options and built-in accessibility features.

### Props

```typescript
interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  children: React.ReactNode;
  size?: 'small' | 'medium' | 'large' | 'full';
  showCloseButton?: boolean;
  closeOnOverlayClick?: boolean;
  closeOnEscape?: boolean;
  className?: string;
  overlayClassName?: string;
}
```

### Usage Example

```tsx
import { Modal, ModalFooter, ModalActionButton } from '../components';

const MyComponent = () => {
  const [isModalOpen, setIsModalOpen] = useState(false);

  const handleClose = () => {
    setIsModalOpen(false);
  };

  const handleSave = () => {
    console.log('Save action');
    handleClose();
  };

  return (
    <>
      <button onClick={() => setIsModalOpen(true)}>
        Open Modal
      </button>

      <Modal
        isOpen={isModalOpen}
        onClose={handleClose}
        title="Example Modal"
        size="medium"
      >
        <p>This is the modal content.</p>
        
        <ModalFooter>
          <ModalActionButton variant="secondary" onClick={handleClose}>
            Cancel
          </ModalActionButton>
          <ModalActionButton variant="primary" onClick={handleSave}>
            Save
          </ModalActionButton>
        </ModalFooter>
      </Modal>
    </>
  );
};
```

### Features

- Multiple size options (small, medium, large, full)
- Keyboard navigation support (Escape to close)
- Click outside to close
- Built-in footer component for actions
- Pre-styled action buttons
- Accessibility features
- Smooth animations

## CalendarHeatmap

A GitHub-style contribution calendar for visualizing activity over time.

### Props

```typescript
interface CalendarHeatmapProps {
  data: CalendarData[];
  startDate?: string;
  endDate?: string;
  title?: string;
  subtitle?: string;
  showLegend?: boolean;
  showTooltip?: boolean;
  className?: string;
  onDayClick?: (date: string, count: number) => void;
  colorScheme?: 'default' | 'green' | 'blue' | 'purple' | 'orange';
}
```

### Usage Example

```tsx
import { CalendarHeatmap } from '../components';

const calendarData = [
  { date: '2024-01-01', count: 3, level: 2 },
  { date: '2024-01-02', count: 5, level: 3 },
  { date: '2024-01-03', count: 0, level: 0 },
  // ... more data
];

const MyComponent = () => {
  const handleDayClick = (date: string, count: number) => {
    console.log(`Clicked ${date} with ${count} activities`);
  };

  return (
    <CalendarHeatmap
      data={calendarData}
      title="Activity Calendar"
      subtitle="Your habit tracking activity over the past year"
      showLegend={true}
      showTooltip={true}
      onDayClick={handleDayClick}
      colorScheme="green"
    />
  );
};
```

### Features

- GitHub-style contribution visualization
- Multiple color schemes
- Interactive tooltips
- Click handlers for day interactions
- Responsive design
- Automatic date range calculation
- Month and weekday labels

## Importing Components

You can import components individually or use the index file:

```tsx
// Individual imports
import { HabitCard } from '../components/HabitCard';
import { GroupList } from '../components/GroupList';

// Or use the index file
import { 
  HabitCard, 
  GroupList, 
  BadgeCard, 
  Modal, 
  CalendarHeatmap 
} from '../components';
```

## Styling

All components follow the existing design system with:
- Consistent color palette
- Responsive breakpoints
- Hover effects and animations
- Accessibility considerations
- Dark mode support (where applicable)

## Best Practices

1. **Props Validation**: Always provide proper TypeScript interfaces for your data
2. **Error Handling**: Implement proper error states and loading indicators
3. **Accessibility**: Use semantic HTML and ARIA attributes where needed
4. **Performance**: Use React.memo for components that receive stable props
5. **Testing**: Write unit tests for component behavior and user interactions

## Contributing

When adding new reusable components:

1. Follow the existing naming conventions
2. Include TypeScript interfaces
3. Add comprehensive CSS with responsive design
4. Include usage examples in this documentation
5. Update the index.ts file
6. Add appropriate tests 