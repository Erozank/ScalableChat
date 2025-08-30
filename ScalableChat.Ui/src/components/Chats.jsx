import React, { useState, useEffect, useCallback } from 'react';
import { Row, Col } from 'react-bootstrap';
import ChatList from './ChatList';
import Chat from './Chat';
import axios from 'axios';
import '../styles/chat.css';

const Chats = ({ apiServer, jwt, connection, nickname, userId }) => {
  const [chats, setChats] = useState([]);
  const [selectedChat, setSelectedChat] = useState(null);



  // Function to handle selecting an existing chat
  const handleSelectChat = useCallback((chat) => {
    console.log('Selected chat:', chat);
    setSelectedChat(chat);
    
  // Mark as read
    setChats(prevChats =>
      prevChats.map(c =>
        c.id === chat.id ? { ...c, unreadCount: 0 } : c
      )
    );
  }, []);

  // Function to handle new messages
  const handleNewMessage = useCallback((chatId, message) => {
    setChats(prevChats =>
      prevChats.map(chat => {
        if (chat.id === chatId) {
          const updatedMessages = [...chat.messages];
          const messageExists = updatedMessages.some(m => m.id === message.id);
          
          if (!messageExists) {
            updatedMessages.push(message);
            updatedMessages.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
          }

          return {
            ...chat,
            messages: updatedMessages,
            unreadCount: selectedChat?.id === chatId ? 0 : chat.unreadCount + 1
          };
        }
        return chat;
      })
    );
  }, [selectedChat]);

  // Load chats
  useEffect(() => {
    const fetchChats = async () => {
      try {
        const response = await axios.get(`${apiServer}/chats`, {
          headers: { Authorization: `Bearer ${jwt}` },
        });
        
        const chatsData = response.data || [];
        const formattedChats = chatsData.map(chat => ({
          chatId: chat.chatId,
          name: chat.friend.nickname || 'No name',
          friendId: chat.friend.userId,
          messages: (chat.messages || []).sort(
            (a, b) => new Date(a.createdAt) - new Date(b.createdAt)
          ),
          unreadCount: 0
        }));
        
        setChats(formattedChats);
      } catch (error) {
        console.error('Error fetching chats:', error);
      }
    };

    if (jwt && apiServer) {
      fetchChats();
    }
  }, [apiServer, jwt]);

  // Listen to global SignalR events for all chats
  useEffect(() => {
    if (!connection) return;

    const handleReceiveMessage = (message) => {
      handleNewMessage(message.chatId, message);
    };

    connection.on("ReceiveMessage", handleReceiveMessage);

    return () => {
      connection.off("ReceiveMessage", handleReceiveMessage);
    };
  }, [connection, handleNewMessage]);

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
          onNewMessage={handleNewMessage}
          nickname={nickname}
          userId={userId}
        />
      </Col>
    </Row>
  );
};

export default Chats;
