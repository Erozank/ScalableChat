CREATE KEYSPACE IF NOT EXISTS chat_with_signalr
WITH REPLICATION = { 'class' : 'SimpleStrategy', 'replication_factor' : 1 };

USE chat_with_signalr;

CREATE TABLE IF NOT EXISTS users (
user_id uuid,
nickname text,
email text,
password_hash text,
PRIMARY KEY (user_id, email));

CREATE INDEX IF NOT EXISTS ON users(email);

CREATE INDEX IF NOT EXISTS ON users(nickname);

CREATE TABLE IF NOT EXISTS user_contacts (
    user_id UUID,
    contact_user_id UUID,
    added_at TIMESTAMP,
    PRIMARY KEY (user_id, contact_user_id)
) WITH CLUSTERING ORDER BY (contact_user_id ASC);

CREATE TABLE IF NOT EXISTS chats (
    chat_id UUID PRIMARY KEY,
    is_group BOOLEAN,
    title TEXT,
    creator_id UUID,
    created_at TIMESTAMP,
    last_message_at TIMESTAMP
);

CREATE TABLE IF NOT EXISTS chat_users (
    user_id UUID,        
    chat_id UUID,        
    joined_at TIMESTAMP,
    left_at TIMESTAMP,   
    PRIMARY KEY (user_id, chat_id)
) WITH CLUSTERING ORDER BY (chat_id ASC);

CREATE TABLE IF NOT EXISTS mensajes (
    chat_id uuid,
    message_id int,
    createt_at timestamp,
    sender_id uuid,
    content text,
    PRIMARY KEY (chat_id, message_id)
) WITH CLUSTERING ORDER BY (message_id ASC);