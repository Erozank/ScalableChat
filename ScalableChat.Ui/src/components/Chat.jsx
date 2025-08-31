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

    const handleUpdateMessageId = (oldId, newId) => {
      setMessages(prev => {
        console.log('Messages before updating id:', prev);
        const updatedMessages = prev.map(m => m.id === oldId ? { ...m, id: newId } : m);
        console.log('Messages after updating id:', updatedMessages);
        return updatedMessages;
      });
    }

    connection.on("ReceiveMessage", handleReceiveMessage);
    connection.on("UpdateMessageId", handleUpdateMessageId);

    return () => {
      connection.off("ReceiveMessage", handleReceiveMessage);
      connection.off("UpdateMessageId", handleUpdateMessageId);
    };
  }, [connection, selectedChat?.chatId, onNewMessage]);

  const sendMessage = async (e) => {
    e.preventDefault();
    if (!newMessage.trim() || !connection) return;

    // Create local message
    const localMessage = {
      id: Math.floor(Math.random() * 1e12).toString(),
      createdAt: new Date().toISOString(),
      senderId: userId,
      chatId: selectedChat.chatId,
      content: newMessage.trim()
    };

    // Add local message to the messages list
    setMessages(prev => [...prev, localMessage]);

    try {
      await connection.invoke("SendMessage", selectedChat?.chatId, selectedChat?.friendId, localMessage.content, localMessage.id);
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
                  <div className="d-flex flex-column align-items-start">
                    <div className="flex-grow-1 w-100">
                      <div className="message-timestamp mb-1 text-muted" style={{ fontSize: '0.8em' }}>
                        {formatTimestamp(message.createdAt)}
                      </div>
                      <div className="message-sender">
                        {isOwn ? nickname : selectedChat?.name}
                      </div>
                      <div className="message-content">{message.content}</div>
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
