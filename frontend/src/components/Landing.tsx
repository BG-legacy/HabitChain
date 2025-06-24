import React from 'react';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { 
  AnimatedButton, 
  AnimatedCard, 
  AnimatedIcon,
  fadeInUp,
  staggerContainer,
  slideInFromLeft,
  slideInFromRight
} from './AnimatedComponents';
import './Landing.css';

const Landing: React.FC = () => {
  const navigate = useNavigate();

  const handleGetStarted = () => {
    navigate('/login');
  };

  return (
    <motion.div 
      className="landing-container"
      initial="hidden"
      animate="visible"
      variants={fadeInUp}
    >
      {/* Background Effects */}
      <div className="landing-background">
        <motion.div 
          className="floating-shapes"
          animate={{
            rotate: 360
          }}
          transition={{
            duration: 60,
            repeat: Infinity,
            ease: "linear"
          }}
        >
          <motion.div 
            className="shape shape-1"
            animate={{
              y: [0, -20, 0],
              scale: [1, 1.1, 1]
            }}
            transition={{
              duration: 4,
              repeat: Infinity,
              ease: "easeInOut"
            }}
          ></motion.div>
          <motion.div 
            className="shape shape-2"
            animate={{
              y: [0, 20, 0],
              scale: [1, 0.9, 1]
            }}
            transition={{
              duration: 5,
              repeat: Infinity,
              ease: "easeInOut",
              delay: 1
            }}
          ></motion.div>
          <motion.div 
            className="shape shape-3"
            animate={{
              y: [0, -15, 0],
              scale: [1, 1.05, 1]
            }}
            transition={{
              duration: 6,
              repeat: Infinity,
              ease: "easeInOut",
              delay: 2
            }}
          ></motion.div>
          <motion.div 
            className="shape shape-4"
            animate={{
              y: [0, 25, 0],
              scale: [1, 0.95, 1]
            }}
            transition={{
              duration: 7,
              repeat: Infinity,
              ease: "easeInOut",
              delay: 3
            }}
          ></motion.div>
        </motion.div>
      </div>

      {/* Main Content */}
      <div className="landing-content">
        <motion.div 
          className="hero-section"
          variants={staggerContainer}
          initial="hidden"
          animate="visible"
        >
          <motion.div 
            className="hero-icon"
            variants={fadeInUp}
            whileHover={{ 
              scale: 1.2,
              rotate: 360,
              transition: { duration: 0.6 }
            }}
          >
            🔗
          </motion.div>
          <motion.h1 
            className="hero-title"
            variants={slideInFromLeft}
          >
            HabitChain
          </motion.h1>
          <motion.p 
            className="hero-subtitle"
            variants={slideInFromRight}
          >
            Build lasting habits, track your progress, and achieve your goals with our modern habit tracking platform
          </motion.p>
          
          <motion.div 
            className="hero-features"
            variants={staggerContainer}
          >
            <motion.div 
              className="feature-item"
              variants={fadeInUp}
              whileHover={{ scale: 1.05 }}
            >
              <AnimatedIcon className="feature-icon">📊</AnimatedIcon>
              <span>Track Progress</span>
            </motion.div>
            <motion.div 
              className="feature-item"
              variants={fadeInUp}
              whileHover={{ scale: 1.05 }}
            >
              <AnimatedIcon className="feature-icon">🏆</AnimatedIcon>
              <span>Earn Badges</span>
            </motion.div>
            <motion.div 
              className="feature-item"
              variants={fadeInUp}
              whileHover={{ scale: 1.05 }}
            >
              <AnimatedIcon className="feature-icon">📅</AnimatedIcon>
              <span>Stay Consistent</span>
            </motion.div>
          </motion.div>

          <AnimatedButton 
            className="btn btn-primary hero-cta"
            onClick={handleGetStarted}
          >
            Get Started
            <motion.span 
              className="cta-arrow"
              animate={{ x: [0, 5, 0] }}
              transition={{ duration: 1.5, repeat: Infinity }}
            >
              →
            </motion.span>
          </AnimatedButton>
        </motion.div>

        <motion.div 
          className="features-section"
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
          variants={staggerContainer}
        >
          <motion.div 
            className="features-grid"
            variants={staggerContainer}
          >
            <AnimatedCard className="glass-card feature-card" delay={0.1}>
              <AnimatedIcon className="feature-card-icon">🎯</AnimatedIcon>
              <h3>Smart Goal Setting</h3>
              <p>Set achievable goals with our intelligent habit planning system that adapts to your lifestyle and preferences.</p>
            </AnimatedCard>
            
            <AnimatedCard className="glass-card feature-card" delay={0.2}>
              <AnimatedIcon className="feature-card-icon">📈</AnimatedIcon>
              <h3>Progress Analytics</h3>
              <p>Visualize your habit journey with detailed analytics, streaks, and insights to keep you motivated.</p>
            </AnimatedCard>
            
            <AnimatedCard className="glass-card feature-card" delay={0.3}>
              <AnimatedIcon className="feature-card-icon">🤝</AnimatedIcon>
              <h3>Community Support</h3>
              <p>Connect with like-minded individuals and receive encouragement from your community to stay motivated.</p>
            </AnimatedCard>
            
            <AnimatedCard className="glass-card feature-card" delay={0.4}>
              <AnimatedIcon className="feature-card-icon">🔔</AnimatedIcon>
              <h3>Smart Reminders</h3>
              <p>Never miss a habit with intelligent notifications that adapt to your schedule and preferences.</p>
            </AnimatedCard>
          </motion.div>
        </motion.div>

        <motion.div 
          className="cta-section"
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
          variants={fadeInUp}
        >
          <AnimatedCard className="glass-card cta-card">
            <h2>Ready to Transform Your Habits?</h2>
            <p>Join thousands of users who have already built lasting habits with HabitChain</p>
            <div className="cta-buttons">
              <AnimatedButton 
                className="btn btn-primary"
                onClick={handleGetStarted}
              >
                Start Your Journey
              </AnimatedButton>
              <AnimatedButton 
                className="btn btn-secondary"
                onClick={() => navigate('/register')}
              >
                Create Account
              </AnimatedButton>
            </div>
          </AnimatedCard>
        </motion.div>
      </div>
    </motion.div>
  );
};

export default Landing; 