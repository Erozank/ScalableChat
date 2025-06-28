import { useState } from "react";
import { Button, Col, Form, Row } from "react-bootstrap";
import AddFriend from "./AddFriend";

const WaitingRoom = ({ joinChatRoom, sendFriendRequest }) => {
  const [username, setUsername] = useState();
  const [chatroom, setChatroom] = useState();

  return (
    <>
      <Form
        onSubmit={(e) => {
          e.preventDefault();
          joinChatRoom(username, chatroom);
        }}
      >
        <Row className="px-5 py-5">
          <Col sm="12">
            <Form.Group>
              <Form.Control
                placeholder="Username"
                onChange={(e) => setUsername(e.target.value)}
              />
              <Form.Control
                placeholder="ChatRoom"
                onChange={(e) => setChatroom(e.target.value)}
              />
            </Form.Group>
          </Col>
          <Col sm="12">
            <Button variant="success" type="submit">
              Join Chat
            </Button>
          </Col>
        </Row>
      </Form>
      <div className="mt-4">
        <h5>Añadir amigo</h5>
        <AddFriend onAddFriend={sendFriendRequest} />
      </div>
    </>
  );
};

export default WaitingRoom;
