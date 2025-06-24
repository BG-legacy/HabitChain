import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import './Encouragements.css';

interface Encouragement {
  id: string;
  senderId: string;
  senderName: string;
  receiverId: string;
  receiverName: string;
  message: string;
  type: 'motivation' | 'celebration' | 'support' | 'reminder';
  isRead: boolean;
  createdAt: string;
}

interface User {
  id: string;
  name: string;
  avatar?: string;
  isOnline: boolean;
}

const Encouragements: React.FC = () => {
  const { user } = useAuth();
  const [encouragements, setEncouragements] = useState<Encouragement[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [showSendForm, setShowSendForm] = useState(false);
  const [selectedUser, setSelectedUser] = useState<string>('');
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState<'motivation' | 'celebration' | 'support' | 'reminder'>('motivation');
  const [activeTab, setActiveTab] = useState<'received' | 'sent'>('received');

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      // TODO: Replace with actual API calls
      // const encouragementsResponse = await fetch('/api/encouragements');
      // const usersResponse = await fetch('/api/users');
      
      // Initialize with empty data
      setEncouragements([]);
      setUsers([]);
    } catch (error) {
      console.error('Error fetching data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSendEncouragement = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!selectedUser || !message.trim()) {
      alert('Please select a user and enter a message');
      return;
    }
    
    try {
      // TODO: Replace with actual API call
      // await fetch('/api/encouragements', {
      //   method: 'POST',
      //   headers: { 'Content-Type': 'application/json' },
      //   body: JSON.stringify({
      //     receiverId: selectedUser,
      //     message,
      //     type: messageType
      //   })
      // });
      
      const selectedUserName = users.find(u => u.id === selectedUser)?.name || 'Unknown';
      
      const newEncouragement: Encouragement = {
        id: Date.now().toString(),
        senderId: user?.id || '',
        senderName: user?.firstName || 'User',
        receiverId: selectedUser,
        receiverName: selectedUserName,
        message,
        type: messageType,
        isRead: false,
        createdAt: new Date().toISOString()
      };
      
      setEncouragements(prev => [newEncouragement, ...prev]);
      setShowSendForm(false);
      setSelectedUser('');
      setMessage('');
      setMessageType('motivation');
    } catch (error) {
      console.error('Error sending encouragement:', error);
    }
  };

  const markAsRead = async (encouragementId: string) => {
    try {
      // TODO: Replace with actual API call
      // await fetch(`/api/encouragements/${encouragementId}/read`, { method: 'PATCH' });
      
      setEncouragements(prev => prev.map(enc => 
        enc.id === encouragementId ? { ...enc, isRead: true } : enc
      ));
    } catch (error) {
      console.error('Error marking as read:', error);
    }
  };

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'motivation': return '💪';
      case 'celebration': return '🎉';
      case 'support': return '🤗';
      case 'reminder': return '⏰';
      default: return '💝';
    }
  };

  const getTypeLabel = (type: string) => {
    switch (type) {
      case 'motivation': return 'Motivation';
      case 'celebration': return 'Celebration';
      case 'support': return 'Support';
      case 'reminder': return 'Reminder';
      default: return 'Encouragement';
    }
  };

  const receivedEncouragements = encouragements.filter(enc => enc.receiverId === user?.id);
  const sentEncouragements = encouragements.filter(enc => enc.senderId === user?.id);
  const unreadCount = receivedEncouragements.filter(enc => !enc.isRead).length;

  if (loading) {
    return (
      <div className="encouragements-loading">
        <div className="spinner"></div>
        <p>Loading encouragements...</p>
      </div>
    );
  }

  return (
    <div className="encouragements">
      <div className="encouragements-header">
        <h1>Encouragements</h1>
        <button 
          className="btn-primary"
          onClick={() => setShowSendForm(true)}
        >
          💝 Send Encouragement
        </button>
      </div>

      {/* Send Form */}
      {showSendForm && (
        <div className="send-form-overlay">
          <div className="send-form-container">
            <div className="send-form-header">
              <h2>Send Encouragement</h2>
              <button 
                className="close-btn"
                onClick={() => setShowSendForm(false)}
              >
                ✕
              </button>
            </div>
            
            <form onSubmit={handleSendEncouragement} className="send-form">
              <div className="form-group">
                <label htmlFor="receiver">Send to</label>
                <select
                  id="receiver"
                  value={selectedUser}
                  onChange={(e) => setSelectedUser(e.target.value)}
                  required
                >
                  <option value="">Select a user...</option>
                  {users.map(user => (
                    <option key={user.id} value={user.id}>
                      {user.name} {user.isOnline ? '🟢' : '⚫'}
                    </option>
                  ))}
                </select>
              </div>
              
              <div className="form-group">
                <label htmlFor="type">Message Type</label>
                <select
                  id="type"
                  value={messageType}
                  onChange={(e) => setMessageType(e.target.value as any)}
                >
                  <option value="motivation">💪 Motivation</option>
                  <option value="celebration">🎉 Celebration</option>
                  <option value="support">🤗 Support</option>
                  <option value="reminder">⏰ Reminder</option>
                </select>
              </div>
              
              <div className="form-group">
                <label htmlFor="message">Message</label>
                <textarea
                  id="message"
                  value={message}
                  onChange={(e) => setMessage(e.target.value)}
                  placeholder="Write your encouraging message..."
                  rows={4}
                  required
                />
              </div>
              
              <div className="form-actions">
                <button type="button" onClick={() => setShowSendForm(false)} className="btn-secondary">
                  Cancel
                </button>
                <button type="submit" className="btn-primary">
                  Send Message
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Tabs */}
      <div className="encouragements-tabs">
        <button 
          className={`tab-btn ${activeTab === 'received' ? 'active' : ''}`}
          onClick={() => setActiveTab('received')}
        >
          📥 Received ({unreadCount > 0 ? `${unreadCount} new` : '0'})
        </button>
        <button 
          className={`tab-btn ${activeTab === 'sent' ? 'active' : ''}`}
          onClick={() => setActiveTab('sent')}
        >
          📤 Sent ({sentEncouragements.length})
        </button>
      </div>

      {/* Messages List */}
      <div className="messages-container">
        {activeTab === 'received' ? (
          receivedEncouragements.length === 0 ? (
            <div className="empty-state">
              <div className="empty-icon">💝</div>
              <h3>No encouragements yet</h3>
              <p>When others send you encouraging messages, they'll appear here!</p>
            </div>
          ) : (
            <div className="messages-list">
              {receivedEncouragements.map(encouragement => (
                <div 
                  key={encouragement.id} 
                  className={`message-card ${!encouragement.isRead ? 'unread' : ''}`}
                  onClick={() => !encouragement.isRead && markAsRead(encouragement.id)}
                >
                  <div className="message-header">
                    <div className="sender-info">
                      <h3>{encouragement.senderName}</h3>
                      <span className="message-type">
                        {getTypeIcon(encouragement.type)} {getTypeLabel(encouragement.type)}
                      </span>
                    </div>
                    <div className="message-meta">
                      <span className="message-time">
                        {new Date(encouragement.createdAt).toLocaleDateString()}
                      </span>
                      {!encouragement.isRead && <div className="unread-indicator"></div>}
                    </div>
                  </div>
                  
                  <p className="message-content">{encouragement.message}</p>
                </div>
              ))}
            </div>
          )
        ) : (
          sentEncouragements.length === 0 ? (
            <div className="empty-state">
              <div className="empty-icon">📤</div>
              <h3>No sent messages yet</h3>
              <p>Send your first encouragement to someone!</p>
            </div>
          ) : (
            <div className="messages-list">
              {sentEncouragements.map(encouragement => (
                <div key={encouragement.id} className="message-card sent">
                  <div className="message-header">
                    <div className="sender-info">
                      <h3>To: {encouragement.receiverName}</h3>
                      <span className="message-type">
                        {getTypeIcon(encouragement.type)} {getTypeLabel(encouragement.type)}
                      </span>
                    </div>
                    <div className="message-meta">
                      <span className="message-time">
                        {new Date(encouragement.createdAt).toLocaleDateString()}
                      </span>
                      <span className="read-status">
                        {encouragement.isRead ? '✅ Read' : '📬 Sent'}
                      </span>
                    </div>
                  </div>
                  
                  <p className="message-content">{encouragement.message}</p>
                </div>
              ))}
            </div>
          )
        )}
      </div>

      {/* Quick Stats */}
      <div className="encouragements-stats">
        <div className="stat-card">
          <div className="stat-icon">💝</div>
          <div className="stat-content">
            <h3>{receivedEncouragements.length}</h3>
            <p>Received</p>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">📤</div>
          <div className="stat-content">
            <h3>{sentEncouragements.length}</h3>
            <p>Sent</p>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">👥</div>
          <div className="stat-content">
            <h3>{users.length}</h3>
            <p>Active Users</p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Encouragements; 