import React, { useState, useEffect } from "react";
import { Col, Container, Row } from "react-bootstrap";
import { Routes, Route, Navigate, useLocation, useNavigate } from "react-router-dom";
import { HubConnectionBuilder } from "@microsoft/signalr";
import Register from "./components/Register";
import Login from "./components/Login";
import LogoutButton from "./components/LogoutButton";
import DarkModeToggle from "./components/DarkModeToggle";
import HeaderNav from "./components/HeaderNav";
import Friends from "./components/Friends";
import Chats from "./components/Chats";
import { useInitialChatData } from "./utils/useInitialChatData";
import { parseJwt } from "./utils/parseJwt";
import "./App.css";
import "bootstrap/dist/css/bootstrap.min.css";
import "bootstrap-icons/font/bootstrap-icons.css";

// Main App component
const App = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const apiServer = import.meta.env.VITE_CHAT_API;

  // Global states
  const [connection, setConnection] = useState();
  const [jwt, setJwt] = useState(() => localStorage.getItem("jwt") || "");
  const [isLoggedIn, setIsLoggedIn] = useState(() => !!localStorage.getItem("jwt"));
  const [darkMode, setDarkMode] = useState(false);
  const [nickname, setNickname] = useState("");
  const [userId, setUserId] = useState("");

  // Synchronizes JWT and login state when route changes
  useEffect(() => {
    const token = localStorage.getItem("jwt");
    setJwt(token || "");
    setIsLoggedIn(!!token);
  }, [location]);

  // Decodes JWT to get nickname and userId
  useEffect(() => {
    if (jwt) {
      const payload = parseJwt(jwt);
      setNickname(payload?.nickname || "");
      setUserId(payload?.sub || "");
    } else {
      setNickname("");
      setUserId("");
    }
  }, [jwt]);

  // Initializes SignalR connection if user is logged in
  useEffect(() => {
    if (isLoggedIn && !connection) {
      const newConnection = new HubConnectionBuilder()
        .withUrl(`${apiServer}/chat`, {
          accessTokenFactory: () => jwt,
        })
        .withAutomaticReconnect()
        .build();

      newConnection
        .start()
        .then(() => {
          setConnection(newConnection);
          console.log("SignalR Connected");
        })
        .catch((err) => console.log("SignalR Connection Error: ", err));
    }
  }, [isLoggedIn, connection, apiServer, jwt]);

  const sendFriendRequest = async (friendName) => {
    if (!connection) return;
    try {
      console.log("Sending friend request to: ", friendName);
      await connection.invoke("SendFriendRequest", friendName);
    } catch (error) {
      console.log(error);
    }
  };

  const acceptFriendRequest = async (friendId) => {
    if (!connection) return;
    try {
      console.log("Accepting friend request from: ", friendId);
      await connection.invoke("AcceptFriendRequest", friendId);
    } catch (error) {
      console.log(error);
    }
  };

  const handleStartChatFromFriends = async (friend) => {
    // Send a request to start a chat with the selected friend
    if (!isLoggedIn) return;

    try {
      const response = await fetch(`${apiServer}/chat?friendId=${friend.userId}`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${jwt}`
        }
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();
      console.log('Create chat response:', data);
      const chatId = data.chatId;
      
      // Create new chat with required structure
      const newChat = {
        chatId: chatId,
        friendId: friend.userId,
        messages: [],
        name: friend.nickname,
        unreadCount: 0
      };

      // Add the new chat to the chats list
      setChats(prevChats => [...prevChats, newChat]);
      
      console.log('Chat started with ID:', chatId);

      navigate('/');
      
      // Store the chat to select it after navigation
      window.selectChat = newChat;
      
      setTimeout(() => {
        if (window.handleStartChat) {
          window.handleStartChat({ ...friend, chatId });
        }
      }, 100);
    } catch (error) {
      console.error('Error starting chat:', error);
    }
  };


  // Global chat/friend state, loaded only once on login
  const {
    friends, setFriends,
    friendRequests, setFriendRequests,
    chats, setChats
  } = useInitialChatData(isLoggedIn, apiServer, jwt);

  // Global event handlers for friend requests
  useEffect(() => {
    if (!connection) return;

    const handleReceiveFriendRequest = (friendData) => {
      setFriendRequests((prev) => {
        const exists = prev.some((req) => req.userId === friendData.userId);
        if (exists) return prev;
        return [
          ...prev,
          { nickname: friendData.nickname, userId: friendData.userId }
        ];
      });
      console.log("Received friend request from: ", friendData.nickname);
    };

    const handleFriendRequestAccepted = (request) => {
      console.log("Friend request accepted:", request);
      setFriendRequests((prev) => prev.filter((r) => r.userId !== request.userId));
      setFriends((prev) => {
        const exists = prev.some((f) => f.userId === request.userId);
        if (exists) return prev;
        return [...prev, request];
      });
    };

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

            // Only increment unreadCount if we're not in the chat view or the chat isn't selected
            const isInChatView = location.pathname === '/';
            const isChatSelected = window.selectChat?.chatId === message.chatId;
            const shouldIncrementUnread = !isInChatView || !isChatSelected;

            return {
              ...chat,
              messages,
              unreadCount: shouldIncrementUnread ? (chat.unreadCount || 0) + 1 : chat.unreadCount
            };
          }
          return chat;
        });
      });
    };

    connection.on("ReceiveFriendRequest", handleReceiveFriendRequest);
    connection.on("FriendRequestAccepted", handleFriendRequestAccepted);
    connection.on("ReceiveMessage", handleReceiveMessage);

    return () => {
      connection.off("ReceiveFriendRequest", handleReceiveFriendRequest);
      connection.off("FriendRequestAccepted", handleFriendRequestAccepted);
      connection.off("ReceiveMessage", handleReceiveMessage);
    };
  }, [connection, setFriendRequests, setFriends, setChats, location.pathname]);

  useEffect(() => {
    document.body.className = darkMode ? "dark-mode" : "";
  }, [darkMode]);

  return (
    <>
      <header>
        <div className="d-flex justify-content-between align-items-center px-4 py-2">
          {isLoggedIn && (
            <HeaderNav 
              pendingRequestsCount={friendRequests.length} 
              unreadMessagesCount={chats.reduce((total, chat) => total + (chat.unreadCount || 0), 0)}
            />
          )}
          <DarkModeToggle darkMode={darkMode} setDarkMode={setDarkMode} />
          {isLoggedIn && (
            <div className="ms-3">
              <LogoutButton
                setIsLoggedIn={setIsLoggedIn}
                setConnection={setConnection}
                setJwt={setJwt}
              />
            </div>
          )}
        </div>
      </header>
      <div>
        <main>
          <Container>
            <Row className="px-5 my-5">
              <Col sm="12">
                <h1 className="font-weight-light">ChatApp</h1>
                {isLoggedIn && (
                  <h4 className="mt-2">Welcome {nickname}</h4>
                )}
              </Col>
            </Row>
            <Routes>
              <Route
                path="/"
                element={
                  !isLoggedIn ? (
                    <Login setIsLoggedIn={setIsLoggedIn} setJwt={setJwt} />
                  ) : (
                    <Chats
                      apiServer={apiServer}
                      jwt={jwt}
                      connection={connection}
                      nickname={nickname}
                      userId={userId}
                      chats={chats}
                      setChats={setChats}
                      onChatCreated={(chat) => window.selectChat = chat}
                    />
                  )
                }
              />
              <Route
                path="/register"
                element={
                  <Register setIsLoggedIn={setIsLoggedIn} setJwt={setJwt} />
                }
              />
              <Route
                path="/friends"
                element={
                  !isLoggedIn ? (
                    <Navigate to="/" />
                  ) : (
                    <Friends
                      connection={connection}
                      sendFriendRequest={sendFriendRequest}
                      acceptFriendRequest={acceptFriendRequest}
                      onStartChat={handleStartChatFromFriends}
                      friends={friends}
                      setFriends={setFriends}
                      friendRequests={friendRequests}
                      setFriendRequests={setFriendRequests}
                    />
                  )
                }
              />
            </Routes>
          </Container>
        </main>
      </div>
    </>
  );
}

export default App;
