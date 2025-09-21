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



  // Listen to global SignalR events for all chats
  useEffect(() => {
    if (!connection) return;

    const handleReceiveMessage = (message) => {
      console.log('Received message:', message);
      
      setChats(prevChats => {
        // Check if chat exists
        const existingChat = prevChats.find(chat => chat.chatId === message.chatId);
        
        if (!existingChat) {
          // Create new chat if it doesn't exist
          const newChat = {
            chatId: message.chatId,
            name: message.senderNickname || 'No name',
            friendId: message.senderId,
            messages: [message],
            unreadCount: 1
          };
          return [...prevChats, newChat];
        }

        // Update existing chat
        return prevChats.map(chat => {
          if (chat.chatId === message.chatId) {
            const messages = [...(chat.messages || [])];
            const messageExists = messages.some(m => m.id === message.id);
            
            if (!messageExists) {
              messages.push(message);
              messages.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
            }

            return {
              ...chat,
              messages,
              unreadCount: selectedChat?.chatId === message.chatId ? 0 : (chat.unreadCount || 0) + 1
            };
          }
          return chat;
        });
      });
    };

    connection.on("ReceiveMessage", handleReceiveMessage);

    return () => {
      connection.off("ReceiveMessage", handleReceiveMessage);
    };
  }, [connection, selectedChat, setChats]);

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
