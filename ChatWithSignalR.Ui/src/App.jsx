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
import { Routes, Route, Navigate, useLocation, Link } from "react-router-dom";
import LogoutButton from "./components/LogoutButton";
import FriendsList from "./components/FriendsList";

function App() {
  const location = useLocation();
  const apiServer = import.meta.env.VITE_CHAT_API;

  const [connection, setConnection] = useState();
  const [messages, setMessages] = useState([]);
  const [jwt, setJwt] = useState(() => localStorage.getItem("jwt") || "");
  const [isLoggedIn, setIsLoggedIn] = useState(() => !!localStorage.getItem("jwt"));

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

      newConnection.on("ReceiveFriendRequest", (friendName) => {
        console.log("Received friend request from: ", friendName);
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

  return (
    <>
      <header>
        {isLoggedIn && (
          <nav>
            <div className="d-flex justify-content-center align-items-center my-3 position-relative">
              <div>
                <Link to="/" className="btn btn-primary mx-2">
                  <i className="bi bi-chat-dots me-2"></i>
                  Chats
                </Link>
                <Link to="/friends" className="btn btn-primary mx-2">
                  <i className="bi bi-people me-2"></i>
                  Friends
                </Link>
              </div>
              <div className="position-absolute end-0 me-4">
                <LogoutButton
                  setIsLoggedIn={setIsLoggedIn}
                  setConnection={setConnection}
                  setJwt={setJwt}
                />
              </div>
            </div>
          </nav>
        )}
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
                    <FriendsList apiServer={apiServer} jwt={jwt} />
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
