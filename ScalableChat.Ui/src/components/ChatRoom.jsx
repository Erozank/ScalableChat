import { Row, Col } from "react-bootstrap";
import MessageContainer from "./MessageContainer";
import SendMessageForm from "./SendMessageForm";
import AddFriend from './AddFriend'

const ChatRoom = ({messages, sendMessage, sendFriendRequest}) =>
    <div>
        <Row className='px-5 my-5'>
            <Col sm={10}>
                <h2 className='font-weight-light'>Chat Room</h2>
            </Col>
            <Col>

            </Col>
        </Row>
        <Row className='px-5 my-5'>
            <Col sm={12}>
                <MessageContainer messages={messages} />
            </Col>
            <Col sm={12}>
                <SendMessageForm sendMessage={sendMessage} />
            </Col>
        </Row>
        <div className="mt-4">
            <h5>Add friend</h5>
            <AddFriend onAddFriend={sendFriendRequest} />
        </div>
    </div>

export default ChatRoom