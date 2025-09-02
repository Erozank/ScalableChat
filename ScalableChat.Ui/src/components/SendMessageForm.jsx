import React, { useState } from "react";
import { Button, Form, InputGroup } from "react-bootstrap";

const SendMessageForm = ({ sendMessage }) => {
    const [message, setMessage] = useState("");

    const handleSubmit = (e) => {
        e.preventDefault();
        if (!message.trim()) return;
        sendMessage(message.trim());
        setMessage("");
    };

    return (
        <Form onSubmit={handleSubmit} autoComplete="off">
            <InputGroup className="mb-3">
                <InputGroup.Text>Chat</InputGroup.Text>
                <Form.Control
                    onChange={(e) => setMessage(e.target.value)}
                    value={message}
                    type="text"
                    placeholder="Type your message here..."
                    aria-label="Type your message here"
                />
                <Button variant="primary" type="submit" disabled={!message.trim()}>
                    Send
                </Button>
            </InputGroup>
        </Form>
    );
};

export default SendMessageForm;