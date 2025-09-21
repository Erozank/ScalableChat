import React from 'react';
import { Card, ListGroup, Badge } from 'react-bootstrap';

const ChatList = ({ chats, selectedChatId, onSelectChat }) => {
  if (!chats || !chats.length) {
    return (
      <Card>
        <Card.Body>
          <p className="text-muted mb-0">No active chats</p>
        </Card.Body>
      </Card>
    );
  }

  const formatLastMessageTime = (timestamp) => {
    if (!timestamp) return '';
    
    const date = new Date(timestamp);
    const now = new Date();
    const diffTime = Math.abs(now - date);
    const diffHours = Math.ceil(diffTime / (1000 * 60 * 60));
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffHours < 24) {
      return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    } else if (diffDays === 1) {
      return 'Ayer';
    } else if (diffDays < 7) {
      return date.toLocaleDateString([], { weekday: 'short' });
    } else {
      return date.toLocaleDateString([], { day: 'numeric', month: 'short' });
    }
  };

  const getLastMessagePreview = (messages) => {
    if (!messages || !messages.length) return 'No messages';
    
    const lastMessage = messages[messages.length - 1];
    const maxLength = 40;
    
    if (lastMessage.content.length > maxLength) {
      return `${lastMessage.content.substring(0, maxLength)}...`;
    }
    
    return lastMessage.content;
  };

  // Sort chats by most recent message
  const sortedChats = [...chats].sort((a, b) => {
    const aLastMessage = a.messages && a.messages.length > 0 
      ? new Date(a.messages[a.messages.length - 1].timestamp) 
      : new Date(0);
    const bLastMessage = b.messages && b.messages.length > 0 
      ? new Date(b.messages[b.messages.length - 1].timestamp) 
      : new Date(0);
    
    return bLastMessage - aLastMessage;
  });

  return (
    <Card>
      <Card.Header>
        <h6 className="mb-0">Chats</h6>
      </Card.Header>
      <Card.Body className="p-0">
        <ListGroup variant="flush">
          {sortedChats.map((chat) => {
            const isSelected = chat.chatId === selectedChatId;
            const lastMessage = chat.messages && chat.messages.length > 0 
              ? chat.messages[chat.messages.length - 1] 
              : null;
            
            return (
              <ListGroup.Item
                key={chat.chatId}
                action
                active={isSelected}
                onClick={() => onSelectChat(chat)}
                className={`chat-list-item d-flex justify-content-between align-items-start ${isSelected ? 'active' : ''}`}
              >
                <div className="w-100">
                  <div className="d-flex justify-content-between align-items-center mb-1">
                    <h6 className="mb-0">{chat.name}</h6>
                    <div className="d-flex align-items-center gap-2">
                      {chat.unreadCount > 0 && (
                        <Badge bg="primary" pill>
                          {chat.unreadCount}
                        </Badge>
                      )}
                      {lastMessage && (
                        <small className="text-muted">
                          {formatLastMessageTime(lastMessage.timestamp)}
                        </small>
                      )}
                    </div>
                  </div>
                  <p className="mb-0 text-muted small">
                    {getLastMessagePreview(chat.messages)}
                  </p>
                </div>
              </ListGroup.Item>
            );
          })}
        </ListGroup>
      </Card.Body>
    </Card>
  );
};

export default ChatList;
