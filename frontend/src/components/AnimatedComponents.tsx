import React from 'react';
import { motion, AnimatePresence, Variants } from 'framer-motion';
import { Link } from 'react-router-dom';

// Animation variants for consistent animations
export const fadeInUp: Variants = {
  hidden: { opacity: 0, y: 20 },
  visible: { 
    opacity: 1, 
    y: 0,
    transition: { duration: 0.6, ease: "easeOut" }
  }
};

export const fadeIn: Variants = {
  hidden: { opacity: 0 },
  visible: { 
    opacity: 1,
    transition: { duration: 0.5, ease: "easeOut" }
  }
};

export const slideInFromLeft: Variants = {
  hidden: { opacity: 0, x: -50 },
  visible: { 
    opacity: 1, 
    x: 0,
    transition: { duration: 0.6, ease: "easeOut" }
  }
};

export const slideInFromRight: Variants = {
  hidden: { opacity: 0, x: 50 },
  visible: { 
    opacity: 1, 
    x: 0,
    transition: { duration: 0.6, ease: "easeOut" }
  }
};

export const scaleIn: Variants = {
  hidden: { opacity: 0, scale: 0.8 },
  visible: { 
    opacity: 1, 
    scale: 1,
    transition: { duration: 0.5, ease: "easeOut" }
  }
};

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

export const cardHover: Variants = {
  rest: { 
    scale: 1,
    boxShadow: "0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)"
  },
  hover: { 
    scale: 1.02,
    boxShadow: "0 10px 25px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)",
    transition: { duration: 0.2, ease: "easeOut" }
  }
};

export const buttonTap: Variants = {
  rest: { scale: 1 },
  tap: { scale: 0.95 }
};

// Reusable animated components
export const AnimatedPage: React.FC<{ children: React.ReactNode; className?: string }> = ({ 
  children, 
  className = "" 
}) => (
  <motion.div
    initial="hidden"
    animate="visible"
    exit="hidden"
    variants={fadeInUp}
    className={className}
  >
    {children}
  </motion.div>
);

export const AnimatedCard: React.FC<{ 
  children: React.ReactNode; 
  className?: string;
  delay?: number;
}> = ({ children, className = "", delay = 0 }) => (
  <motion.div
    initial="hidden"
    animate="visible"
    variants={scaleIn}
    whileHover="hover"
    transition={{ delay }}
    className={className}
  >
    {children}
  </motion.div>
);

export const AnimatedButton: React.FC<{ 
  children: React.ReactNode; 
  className?: string;
  onClick?: () => void;
  disabled?: boolean;
}> = ({ children, className = "", onClick, disabled = false }) => (
  <motion.button
    whileHover={{ scale: 1.05 }}
    whileTap="tap"
    variants={buttonTap}
    onClick={onClick}
    disabled={disabled}
    className={className}
    style={{ cursor: disabled ? 'not-allowed' : 'pointer' }}
  >
    {children}
  </motion.button>
);

export const AnimatedLink: React.FC<{ 
  children: React.ReactNode; 
  to: string;
  className?: string;
  onClick?: () => void;
}> = ({ children, to, className = "", onClick }) => (
  <motion.div
    whileHover={{ scale: 1.05 }}
    whileTap={{ scale: 0.95 }}
  >
    <Link to={to} className={className} onClick={onClick}>
      {children}
    </Link>
  </motion.div>
);

export const AnimatedList: React.FC<{ 
  children: React.ReactNode; 
  className?: string;
}> = ({ children, className = "" }) => (
  <motion.div
    initial="hidden"
    animate="visible"
    variants={staggerContainer}
    className={className}
  >
    {children}
  </motion.div>
);

export const AnimatedListItem: React.FC<{ 
  children: React.ReactNode; 
  className?: string;
  index?: number;
}> = ({ children, className = "", index = 0 }) => (
  <motion.div
    variants={fadeInUp}
    custom={index}
    className={className}
  >
    {children}
  </motion.div>
);

export const AnimatedModal: React.FC<{ 
  children: React.ReactNode; 
  isOpen: boolean;
  onClose: () => void;
  className?: string;
}> = ({ children, isOpen, onClose, className = "" }) => (
  <AnimatePresence>
    {isOpen && (
      <>
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          onClick={onClose}
          className="fixed inset-0 bg-black bg-opacity-50 z-40"
        />
        <motion.div
          initial={{ opacity: 0, scale: 0.8, y: 50 }}
          animate={{ opacity: 1, scale: 1, y: 0 }}
          exit={{ opacity: 0, scale: 0.8, y: 50 }}
          transition={{ type: "spring", damping: 25, stiffness: 300 }}
          className={`fixed top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 z-50 ${className}`}
        >
          {children}
        </motion.div>
      </>
    )}
  </AnimatePresence>
);

export const AnimatedNavbar: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <motion.nav
    initial={{ y: -100, opacity: 0 }}
    animate={{ y: 0, opacity: 1 }}
    transition={{ duration: 0.6, ease: "easeOut" }}
  >
    {children}
  </motion.nav>
);

export const AnimatedSidebar: React.FC<{ 
  children: React.ReactNode; 
  isOpen: boolean;
  className?: string;
}> = ({ children, isOpen, className = "" }) => (
  <motion.div
    initial={{ x: -300 }}
    animate={{ x: isOpen ? 0 : -300 }}
    transition={{ type: "spring", damping: 25, stiffness: 300 }}
    className={className}
  >
    {children}
  </motion.div>
);

export const AnimatedProgress: React.FC<{ 
  progress: number; 
  className?: string;
}> = ({ progress, className = "" }) => (
  <motion.div
    initial={{ width: 0 }}
    animate={{ width: `${progress}%` }}
    transition={{ duration: 1, ease: "easeOut" }}
    className={className}
  />
);

export const AnimatedCounter: React.FC<{ 
  value: number; 
  className?: string;
}> = ({ value, className = "" }) => (
  <motion.span
    key={value}
    initial={{ scale: 1.2, opacity: 0 }}
    animate={{ scale: 1, opacity: 1 }}
    transition={{ duration: 0.3, ease: "easeOut" }}
    className={className}
  >
    {value}
  </motion.span>
);

export const AnimatedIcon: React.FC<{ 
  children: React.ReactNode; 
  className?: string;
}> = ({ children, className = "" }) => (
  <motion.div
    whileHover={{ rotate: 360, scale: 1.1 }}
    whileTap={{ scale: 0.9 }}
    transition={{ duration: 0.3, ease: "easeOut" }}
    className={className}
  >
    {children}
  </motion.div>
);

export const AnimatedNotification: React.FC<{ 
  children: React.ReactNode; 
  isVisible: boolean;
  className?: string;
}> = ({ children, isVisible, className = "" }) => (
  <AnimatePresence>
    {isVisible && (
      <motion.div
        initial={{ opacity: 0, y: -50, scale: 0.8 }}
        animate={{ opacity: 1, y: 0, scale: 1 }}
        exit={{ opacity: 0, y: -50, scale: 0.8 }}
        transition={{ type: "spring", damping: 25, stiffness: 300 }}
        className={className}
      >
        {children}
      </motion.div>
    )}
  </AnimatePresence>
);

// Page transition wrapper
export const PageTransition: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <motion.div
    initial={{ opacity: 0, x: 20 }}
    animate={{ opacity: 1, x: 0 }}
    exit={{ opacity: 0, x: -20 }}
    transition={{ duration: 0.3, ease: "easeInOut" }}
  >
    {children}
  </motion.div>
); 