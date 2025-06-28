CREATE KEYSPACE IF NOT EXISTS chat_with_signalr
WITH REPLICATION = { 'class' : 'SimpleStrategy', 'replication_factor' : 1 };

USE chat_with_signalr;

CREATE TABLE IF NOT EXISTS users (
user_id uuid,
nickname text,
email text,
password_hash text,
PRIMARY KEY (user_id));

CREATE INDEX IF NOT EXISTS ON users(email);

CREATE INDEX IF NOT EXISTS ON users(nickname);

CREATE TABLE IF NOT EXISTS friend_requests (
    from_user_id UUID,
    to_user_id UUID,
    status int, -- 0 = pending, 1 = accepted, 2 = rejected
    requested_at TIMESTAMP,
    responded_at TIMESTAMP,
    PRIMARY KEY (from_user_id, to_user_id)
);

CREATE TABLE IF NOT EXISTS direct_chats (
    user_a UUID,
    user_b UUID,
    chat_id UUID,
    PRIMARY KEY (user_a, user_b)
);

CREATE TABLE IF NOT EXISTS messages (
    chat_id uuid,
    message_id timeuuid,
    created_at timestamp,
    sender_id uuid,
    content text,
    PRIMARY KEY (chat_id, message_id)
) WITH CLUSTERING ORDER BY (message_id ASC);