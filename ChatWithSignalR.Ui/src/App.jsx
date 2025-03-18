import { Col, Container, Row } from 'react-bootstrap'
import './App.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import WaitingRoom from './components/waitingroom'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useState } from 'react'

function App() {
  const apiServer = import.meta.env.VITE_CHAT_API

  const[connection, setConnection] = useState()

  const joinChatRoom = async (username, chatroom) => {
    try{
      // initialice a conection
      const connection = new HubConnectionBuilder()
        .withUrl(`${apiServer}/chat`)
        .configureLogging(LogLevel.Information)
        .build()

      // set up handler
      connection.on('JoinSpecificChatRoom', (username, message) => {
        console.log("msg: ", message) 
      })

      // start the connection
      await connection.start()
      await connection.invoke('JoinSpecificChatRoom', {username, chatroom})
      
      setConnection(connection)

    } catch (error) {
      console.log(error)
    }
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
            <WaitingRoom joinChatRoom={joinChatRoom} />
          </Container>
        </main>
      </div>
    </>
  )
}

export default App
