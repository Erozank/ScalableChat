import React, { useState, useEffect, useCallback } from 'react';
import { Row, Col } from 'react-bootstrap';
import ChatList from './ChatList';
import Chat from './Chat';
import '../styles/chat.css';

const Chats = ({ connection, nickname, userId, chats, setChats }) => {
  const [selectedChat, setSelectedChat] = useState(null);
  // Handler to delete chat locally
  const handleDeleteChat = useCallback((chatId) => {
    setChats(prevChats => prevChats.filter(c => c.chatId !== chatId));
    if (selectedChat && selectedChat.chatId === chatId) {
      setSelectedChat(null);
    }
  }, [setChats, selectedChat]);

  // Listen for 'ChatDeleted' event from SignalR
  useEffect(() => {
    if (!connection) return;
    const handleChatDeleted = (chatId) => {
      handleDeleteChat(chatId);
    };
    connection.on("ChatDeleted", handleChatDeleted);
    return () => {
      connection.off("ChatDeleted", handleChatDeleted);
    };
  }, [connection, handleDeleteChat]);

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

  return (
    <Row className="g-3">
      <Col md={4}>
        <ChatList 
          chats={chats} 
          selectedChatId={selectedChat?.chatId} 
          onSelectChat={handleSelectChat} 
          onDeleteChat={handleDeleteChat}
          connection={connection}
        />
      </Col>
      <Col md={8}>
        <Chat
          selectedChat={selectedChat}
          connection={connection}
          nickname={nickname}
          userId={userId}
          setChats={setChats}
        />
      </Col>
    </Row>
  );
};

export default Chats;
