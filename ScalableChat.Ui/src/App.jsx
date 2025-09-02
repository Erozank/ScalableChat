import { Col, Container, Row } from "react-bootstrap";
import "./App.css";
import "bootstrap/dist/css/bootstrap.min.css";
import "bootstrap-icons/font/bootstrap-icons.css";
import { HubConnectionBuilder } from "@microsoft/signalr";
import { useState, useEffect } from "react";
import { useInitialChatData } from "./utils/useInitialChatData";
import { parseJwt } from "./utils/parseJwt";
import Register from "./components/Register";
import Login from "./components/Login";
import { Routes, Route, Navigate, useLocation, useNavigate } from "react-router-dom";
import LogoutButton from "./components/LogoutButton";
import DarkModeToggle from "./components/DarkModeToggle";
import HeaderNav from "./components/HeaderNav";
import Friends from "./components/Friends";
import Chats from "./components/Chats";


function App() {
  const location = useLocation();
  const navigate = useNavigate();
  const apiServer = import.meta.env.VITE_CHAT_API;

  const [connection, setConnection] = useState();
  const [jwt, setJwt] = useState(() => localStorage.getItem("jwt") || "");
  const [isLoggedIn, setIsLoggedIn] = useState(() => !!localStorage.getItem("jwt"));
  const [darkMode, setDarkMode] = useState(false);
  const [nickname, setNickname] = useState("");
  const [userId, setUserId] = useState("");

  useEffect(() => {
    const token = localStorage.getItem("jwt");
    setJwt(token || "");
    setIsLoggedIn(!!token);
  }, [location]);

  useEffect(() => {
    if (jwt) {
      const payload = parseJwt(jwt);
      setNickname(payload?.nickname || "");
      setUserId(payload?.sub || "");
    } else {
      setNickname("");
    }
  }, [jwt]);

  useEffect(() => {
    console.log("isLoggedIn: ", isLoggedIn);
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
    // send a request to start a chat with the selected friend
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
      
      console.log('Chat started with ID:', chatId);

      navigate('/');
      
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

  useEffect(() => {
    document.body.className = darkMode ? "dark-mode" : "";
  }, [darkMode]);

  return (
    <>
      <header>
        <div className="d-flex justify-content-between align-items-center px-4 py-2">
          {isLoggedIn && <HeaderNav />}
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
