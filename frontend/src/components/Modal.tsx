import React, { useEffect, useRef } from 'react';
import './Modal.css';

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

const Modal: React.FC<ModalProps> = ({
  isOpen,
  onClose,
  title,
  children,
  size = 'medium',
  showCloseButton = true,
  closeOnOverlayClick = true,
  closeOnEscape = true,
  className = '',
  overlayClassName = ''
}) => {
  const modalRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && closeOnEscape) {
        onClose();
      }
    };

    const handleClickOutside = (event: MouseEvent) => {
      if (
        modalRef.current &&
        !modalRef.current.contains(event.target as Node) &&
        closeOnOverlayClick
      ) {
        onClose();
      }
    };

    if (isOpen) {
      document.addEventListener('keydown', handleEscape);
      document.addEventListener('mousedown', handleClickOutside);
      document.body.style.overflow = 'hidden';
    }

    return () => {
      document.removeEventListener('keydown', handleEscape);
      document.removeEventListener('mousedown', handleClickOutside);
      document.body.style.overflow = 'unset';
    };
  }, [isOpen, onClose, closeOnEscape, closeOnOverlayClick]);

  if (!isOpen) {
    return null;
  }

  return (
    <div className={`modal-overlay ${overlayClassName}`}>
      <div 
        ref={modalRef}
        className={`modal ${size} ${className}`}
      >
        {(title || showCloseButton) && (
          <div className="modal-header">
            {title && (
              <h2 className="modal-title">{title}</h2>
            )}
            {showCloseButton && (
              <button 
                className="modal-close-btn"
                onClick={onClose}
                aria-label="Close modal"
              >
                ✕
              </button>
            )}
          </div>
        )}
        
        <div className="modal-content">
          {children}
        </div>
      </div>
    </div>
  );
};

// Modal Footer Component for consistent action buttons
interface ModalFooterProps {
  children: React.ReactNode;
  className?: string;
}

export const ModalFooter: React.FC<ModalFooterProps> = ({ 
  children, 
  className = '' 
}) => {
  return (
    <div className={`modal-footer ${className}`}>
      {children}
    </div>
  );
};

// Modal Action Button Component
interface ModalActionButtonProps {
  onClick: () => void;
  variant?: 'primary' | 'secondary' | 'success' | 'danger' | 'warning';
  disabled?: boolean;
  children: React.ReactNode;
  className?: string;
}

export const ModalActionButton: React.FC<ModalActionButtonProps> = ({
  onClick,
  variant = 'primary',
  disabled = false,
  children,
  className = ''
}) => {
  return (
    <button
      className={`modal-action-btn ${variant} ${className}`}
      onClick={onClick}
      disabled={disabled}
    >
      {children}
    </button>
  );
};

export default Modal; 