
const MessageContainer = ({ messages }) => {
    if (!messages || !messages.length) {
        return <div className="text-muted">No messages yet.</div>;
    }
    return (
        <div className="message-container">
            <table className="table table-striped table-bordered">
                <tbody>
                    {messages.map((message, index) => (
                        <tr key={index}>
                            <td>
                                <strong>{message.username}:</strong> {message.message}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default MessageContainer;