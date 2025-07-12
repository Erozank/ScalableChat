
const MessageContainer = ({messages}) => {
    return (
        <div>
            {messages.map((message, index) => {
                return (
                    <table striped bordered key={index}>
                        <tr key={index}>
                            {message.username}: {message.message}
                        </tr>
                    </table>
                )
            })}
        </div>
    )

 }

export default MessageContainer