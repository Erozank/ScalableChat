import { Col, Container, Row } from 'react-bootstrap'
import './App.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import WaitingRoom from './components/waitingroom'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useState } from 'react'
import ChatRoom from './components/ChatRoom'
import Register from './components/Register'
import Login from './components/Login'
import { Routes, Route, useNavigate } from 'react-router-dom'

function App() {
  const apiServer = import.meta.env.VITE_CHAT_API
  const navigate = useNavigate()

  const[connection, setConnection] = useState()
  const[messages, setMessages] = useState([])
  const [isLoggedIn, setIsLoggedIn] = useState(false)

  const joinChatRoom = async (username, chatroom) => {
    try{
      // initialice a conection
      const connection = new HubConnectionBuilder()
        .withUrl(`${apiServer}/chat`)
        .configureLogging(LogLevel.Information)
        .build()

      // set up handler
      connection.on('JoinSpecificChatRoom', (username, message) => {
        setMessages(messages => [...messages, {username, message}])
        console.log("msg: ", message) 
      })

      connection.on('ReceiveMessage', (username, message) => {
        setMessages(messages => [...messages, {username, message}])
      })

      // start the connection
      await connection.start()
      await connection.invoke('JoinSpecificChatRoom', {username, chatroom})
      
      setConnection(connection)

    } catch (error) {
      console.log(error)
    }
  }

  const sendMessage = async (message) => {
    try {
      await connection.invoke('SendMessage', message)
    } catch (error) {
      console.log(error)
    }
  }

  const handleRegistrationComplete = () => {
    navigate('/')
  }

  return (
    <>
      <div>
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
                  ? <Login setIsLoggedIn={setIsLoggedIn} />
                  : (!connection 
                      ? <WaitingRoom joinChatRoom={joinChatRoom} />
                      : <ChatRoom messages={messages} sendMessage={sendMessage}/>
                    )
              } />
              <Route path="/register" element={<Register onRegistrationComplete={handleRegistrationComplete} />} />
            </Routes>
          </Container>
        </main>
      </div>
    </>
  )
}

export default App
