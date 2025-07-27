import { Col, Container, Row } from "react-bootstrap";
import "./App.css";
import "bootstrap/dist/css/bootstrap.min.css";
import "bootstrap-icons/font/bootstrap-icons.css";
import WaitingRoom from "./components/waitingroom";
import { HubConnectionBuilder } from "@microsoft/signalr";
import { useState, useEffect } from "react";
import ChatRoom from "./components/ChatRoom";
import Register from "./components/Register";
import Login from "./components/Login";
import { Routes, Route, Navigate, useLocation } from "react-router-dom";
import LogoutButton from "./components/LogoutButton";
import DarkModeToggle from "./components/DarkModeToggle";
import HeaderNav from "./components/HeaderNav";
import Friends from "./components/Friends";

function App() {
  const location = useLocation();
  const apiServer = import.meta.env.VITE_CHAT_API;

  const [connection, setConnection] = useState();
  const [messages, setMessages] = useState([]);
  const [jwt, setJwt] = useState(() => localStorage.getItem("jwt") || "");
  const [isLoggedIn, setIsLoggedIn] = useState(() => !!localStorage.getItem("jwt"));
  const [darkMode, setDarkMode] = useState(false);
  const [friendRequests, setFriendRequests] = useState([]);

  useEffect(() => {
    const token = localStorage.getItem("jwt");
    setJwt(token || "");
    setIsLoggedIn(!!token);
  }, [location]);

  useEffect(() => {
    console.log("isLoggedIn: ", isLoggedIn);
    if (isLoggedIn && !connection) {
      const newConnection = new HubConnectionBuilder()
        .withUrl(`${apiServer}/chat`, {
          accessTokenFactory: () => jwt,
        })
        .withAutomaticReconnect()
        .build();

      newConnection.on("JoinSpecificChatRoom", (username, message) => {
        setMessages((messages) => [...messages, { username, message }]);
        console.log("msg: ", message);
      });

      newConnection.on("ReceiveMessage", (username, message) => {
        setMessages((messages) => [...messages, { username, message }]);
      });

      newConnection.on("ReceiveFriendRequest", (friendData) => {
        setFriendRequests((prev) => {
          const exists = prev.some((req) => req.userId === friendData.userId);
          if (exists) return prev;
          return [
            ...prev,
            { nickname: friendData.nickname, userId: friendData.userId }
          ];
        });
        console.log("Received friend request from: ", friendData.nickname);
      });

      newConnection
        .start()
        .then(() => {
          setConnection(newConnection);
          console.log("SignalR Connected");
        })
        .catch((err) => console.log("SignalR Connection Error: ", err));
    }
  }, [isLoggedIn, connection, apiServer, jwt]);

  const joinChatRoom = async (username, chatroom) => {
    try {
      if (connection) {
        console.log("Joining chat room: ", chatroom);
        await connection.invoke("JoinSpecificChatRoom", { username, chatroom });
      }
    } catch (error) {
      console.log(error);
    }
  };

  const sendMessage = async (message) => {
    try {
      console.log("Sending message: ", message);
      await connection.invoke("SendMessage", message);
    } catch (error) {
      console.log(error);
    }
  };

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
                <h1 className="font-weight-light">Welcome to the ChatApp </h1>
              </Col>
            </Row>
            <Routes>
              <Route
                path="/"
                element={
                  !isLoggedIn ? (
                    <Login setIsLoggedIn={setIsLoggedIn} setJwt={setJwt} />
                  ) : !connection ? (
                    <WaitingRoom
                      joinChatRoom={joinChatRoom}
                      sendFriendRequest={sendFriendRequest}
                    />
                  ) : (
                    <ChatRoom
                      messages={messages}
                      sendMessage={sendMessage}
                      sendFriendRequest={sendFriendRequest}
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
                      apiServer={apiServer} 
                      jwt={jwt} 
                      sendFriendRequest={sendFriendRequest} 
                      acceptFriendRequest={acceptFriendRequest} 
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
