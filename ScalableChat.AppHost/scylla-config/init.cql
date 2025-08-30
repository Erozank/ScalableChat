CREATE KEYSPACE IF NOT EXISTS scalable_chat
WITH REPLICATION = { 'class' : 'SimpleStrategy', 'replication_factor' : 1 };

USE scalable_chat;

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

CREATE TABLE IF NOT EXISTS user_friends (
    user_id UUID,
    friend_id UUID,
    since TIMESTAMP,
    PRIMARY KEY (user_id, friend_id)
);

CREATE TABLE IF NOT EXISTS direct_chats (
    user_a UUID,
    user_b UUID,
    chat_id UUID,
    PRIMARY KEY (user_a, user_b)
);

CREATE TABLE IF NOT EXISTS direct_chats_by_chat_id (
    chat_id UUID PRIMARY KEY,
    user_a UUID,
    user_b UUID
);

CREATE TABLE user_chats (
    user_id UUID,
    chat_id UUID,
    last_message_at TIMESTAMP,
    PRIMARY KEY (user_id, chat_id)
);

CREATE TABLE IF NOT EXISTS messages (
    chat_id UUID,
    message_id TIMEUUID,
    created_at TIMESTAMP,
    sender_id UUID,
    content TEXT,
    PRIMARY KEY (chat_id, message_id)
) WITH CLUSTERING ORDER BY (message_id ASC);