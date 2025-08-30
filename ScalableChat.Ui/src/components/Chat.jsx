import React, { useState, useEffect, useRef } from 'react';
import { Card, Form, Button } from 'react-bootstrap';

const Chat = ({ selectedChat, connection, onNewMessage, nickname, userId }) => {
  const [messages, setMessages] = useState([]);
  const [newMessage, setNewMessage] = useState('');
  const messagesEndRef = useRef(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  // Sync local messages state with selectedChat.messages when chat changes
  useEffect(() => {
    if (selectedChat?.messages) {
      setMessages(selectedChat.messages);
    } else {
      setMessages([]);
    }
  }, [selectedChat?.chatId, selectedChat?.messages]);


  // Listen for new messages via SignalR
  useEffect(() => {
    if (!connection) return;

    const handleReceiveMessage = (message) => {
      if (message.chatId === selectedChat?.chatId) {
        setMessages(prev => {
          const exists = prev.some(m => m.id === message.id);
          if (exists) return prev;
          return [...prev, message].sort(
            (a, b) => new Date(a.createdAt) - new Date(b.createdAt)
          );
        });
        
        // Notify the parent component that there is a new message
        if (onNewMessage) {
          onNewMessage(selectedChat?.chatId, message);
        }
      }
    };

    connection.on("ReceiveMessage", handleReceiveMessage);

    return () => {
      connection.off("ReceiveMessage", handleReceiveMessage);
    };
  }, [connection, selectedChat?.chatId, onNewMessage]);

  const sendMessage = async (e) => {
    e.preventDefault();
    if (!newMessage.trim() || !connection) return;

    try {
      console.log("Sending message:", newMessage);
      await connection.invoke("SendMessage", selectedChat?.chatId, selectedChat?.friendId, newMessage.trim());
      setNewMessage('');
    } catch (error) {
      console.error('Error sending message:', error);
    }
  };

  const formatTimestamp = (timestamp) => {
    const date = new Date(timestamp);
    return date.toLocaleString();
  };

  if (!selectedChat?.chatId) {
    return (
      <Card className="h-100">
        <Card.Body className="d-flex align-items-center justify-content-center">
          <p className="text-muted">Select a chat to start chatting</p>
        </Card.Body>
      </Card>
    );
  }

  return (
    <Card className="h-100">
      <Card.Header>
        <h6 className="mb-0">{selectedChat.name}</h6>
      </Card.Header>
      <Card.Body className="d-flex flex-column chat-container">
        <div className="flex-grow-1 overflow-auto mb-3 chat-messages">
          <div>
            {messages.map((message) => {
              const isOwn = message.senderId === userId;
              return (
                <div
                  key={message.id}
                  className={`message-item ${isOwn ? "message-right" : "message-left"}`}
                >
                  <div className="d-flex justify-content-between align-items-start">
                    <div className="flex-grow-1">
                      <div className="message-sender">
                        {isOwn ? nickname : selectedChat?.name}
                      </div>
                      <div className="message-content">{message.content}</div>
                    </div>
                    <div className="message-timestamp">
                      {formatTimestamp(message.createdAt)}
                    </div>
                  </div>
                </div>
              );
            })}
            <div ref={messagesEndRef} />
          </div>
        </div>
        <div className="chat-input-container">
          <Form onSubmit={sendMessage}>
            <div className="d-flex gap-2">
              <Form.Control
                type="text"
                placeholder="Type a message..."
                value={newMessage}
                onChange={(e) => setNewMessage(e.target.value)}
              />
              <Button type="submit" disabled={!newMessage.trim()}>
                Send
              </Button>
            </div>
          </Form>
        </div>
      </Card.Body>
    </Card>
  );
};

export default Chat;
