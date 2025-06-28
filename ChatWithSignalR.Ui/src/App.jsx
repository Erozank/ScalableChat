import { Col, Container, Row } from 'react-bootstrap'
import './App.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import WaitingRoom from './components/waitingroom'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useState, useEffect } from 'react'
import ChatRoom from './components/ChatRoom'
import Register from './components/Register'
import Login from './components/Login'
import { Routes, Route } from 'react-router-dom'
import LogoutButton from './components/LogoutButton'

function App() {
  const apiServer = import.meta.env.VITE_CHAT_API

  const [connection, setConnection] = useState()
  const [messages, setMessages] = useState([])
  const [isLoggedIn, setIsLoggedIn] = useState(false)
  const [jwt, setJwt] = useState(() => localStorage.getItem('jwt') || '')

  useEffect(() => {
    if (jwt) {
      setIsLoggedIn(true)
    } else {
      setIsLoggedIn(false)
    }
  }, [jwt])

  useEffect(() => {
    if (isLoggedIn && !connection) {
      console.log("sending jwt: ", jwt)
      const newConnection = new HubConnectionBuilder()
        .withUrl(`${apiServer}/chat`, {
          accessTokenFactory: () => jwt // Use the JWT token for authentication
        })
        .withAutomaticReconnect()
        .build();

      newConnection.on('JoinSpecificChatRoom', (username, message) => {
        setMessages(messages => [...messages, { username, message }]);
        console.log("msg: ", message);
      });

      newConnection.on('ReceiveMessage', (username, message) => {
        setMessages(messages => [...messages, { username, message }]);
      });

      newConnection.start()
        .then(() => {
          setConnection(newConnection);
          console.log('SignalR Connected');
        })
        .catch(err => console.log('SignalR Connection Error: ', err));
    }
  }, [isLoggedIn, connection, apiServer, jwt]);

  const joinChatRoom = async (username, chatroom) => {
    try {
      if (connection) {
        console.log("Joining chat room: ", chatroom);
        await connection.invoke('JoinSpecificChatRoom', { username, chatroom });
      }
    } catch (error) {
      console.log(error);
    }
  }

  const sendMessage = async (message) => {
    try {
      console.log("Sending message: ", message);
      await connection.invoke('SendMessage', message)
    } catch (error) {
      console.log(error)
    }
  }

  const sendFriendRequest = async (friendName) => {
    if (!connection) return;
    try {
      console.log("Sending friend request to: ", friendName);
      await connection.invoke('SendFriendRequest', friendName);
    } catch (error) {
      console.log(error);
    }
  };

  return (
    <>
      <div>
        {isLoggedIn && (
          <div className="d-flex justify-content-end p-3">
            <LogoutButton setIsLoggedIn={setIsLoggedIn} setConnection={setConnection} setJwt={setJwt} />
          </div>
        )}
        <main>
          <Container>
            <Row className='px-5 my-5'>
              <Col sm='12'>
                <h1 className='font-weight-light'>Welcome to the ChatApp </h1>
              </Col>
            </Row>
            <Routes>
              <Route path="/" element={
                !isLoggedIn 
                  ? <Login setIsLoggedIn={setIsLoggedIn} setJwt={setJwt} />
                  : (!connection 
                      ? <WaitingRoom joinChatRoom={joinChatRoom} sendFriendRequest={sendFriendRequest} />
                      : <ChatRoom messages={messages} sendMessage={sendMessage} sendFriendRequest={sendFriendRequest} />
                    )
              } />
              <Route path="/register" element={<Register setIsLoggedIn={setIsLoggedIn} setJwt={setJwt} />} />
            </Routes>
          </Container>
        </main>
      </div>
    </>
  )
}

export default App
