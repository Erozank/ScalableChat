import React, { useState, useEffect, useCallback } from 'react';
import { Row, Col } from 'react-bootstrap';
import ChatList from './ChatList';
import Chat from './Chat';
import '../styles/chat.css';

const Chats = ({ connection, nickname, userId, chats, setChats }) => {
  const [selectedChat, setSelectedChat] = useState(null);

  // Handle selection of newly created chat
  useEffect(() => {
    if (window.selectChat) {
      setSelectedChat(window.selectChat);
      window.selectChat = null;
    }
  }, [chats]);

  // Function to handle selecting an existing chat
  const handleSelectChat = useCallback((chat) => {
    console.log('Selected chat:', chat);
    setSelectedChat(chat);
    // Mark as read
    setChats(prevChats =>
      prevChats.map(c =>
        c.chatId === chat.chatId ? { ...c, unreadCount: 0 } : c
      )
    );
  }, [setChats]);



  // No need for global SignalR events here as they are now handled in App.jsx

  return (
    <Row className="g-3">
      <Col md={4}>
        <ChatList 
          chats={chats} 
          selectedChatId={selectedChat?.chatId} 
          onSelectChat={handleSelectChat} 
        />
      </Col>
      <Col md={8}>
        <Chat
          selectedChat={selectedChat}
          connection={connection}
          nickname={nickname}
          userId={userId}
        />
      </Col>
    </Row>
  );
};

export default Chats;
