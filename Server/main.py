# Required modules
import sqlite3
import os
import json
import socket
from contextlib import closing
from validator import validateUser  # External validator function/module
import select
import bcrypt

# Database configuration
DB_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "../1vs1Football.db")

# Global state
users_connected = 0  # Count of currently connected users
player1_already_assign = False  # Indicates if player1 is already assigned
game_sockets = []  # List of sockets for all connected game players
player_sessions = {"player1": None, "player2": None}  # Track which socket is assigned to which player
buffers = {}  # Socket input buffers (for line-based protocol)
users = {}  # Tracks user emails by client address (port used as key)


### --- USER & AUTHENTICATION LOGIC --- ###

def check_username(username):
    """Check if a username exists in the database."""
    with closing(sqlite3.connect(DB_FILE)) as conn:
        with conn:
            cursor = conn.cursor()
            cursor.execute("SELECT username FROM users WHERE username=?", (username,))
            return cursor.fetchone() is not None

def check_email(email):
    """Check if an email exists in the database."""
    with closing(sqlite3.connect(DB_FILE)) as conn:
        with conn:
            cursor = conn.cursor()
            cursor.execute("SELECT email FROM users WHERE email=?", (email,))
            return cursor.fetchone() is not None

def user_is_already_connected(email):
    """Ensure a user isn't already connected using the same email."""
    return email not in users.values()

def check_login(password, email, addr):
    """Verify user credentials and prevent duplicate login."""
    if not user_is_already_connected(email):
        return False
    with closing(sqlite3.connect(DB_FILE)) as conn:
        with conn:
            cursor = conn.cursor()
            cursor.execute("SELECT password FROM users WHERE email=?", (email,))
            result = cursor.fetchone()
            if result and bcrypt.checkpw(password.encode(), result[0]):
                users[addr] = email
                return True
    return False

def create_user(user, addr):
    """Create a new user in the database after validation."""
    if validateUser(user):
        with closing(sqlite3.connect(DB_FILE)) as conn:
            with conn:
                hashed_password = bcrypt.hashpw(user.password.encode(), bcrypt.gensalt())
                user.password = hashed_password
                data = [(None, user.username, user.password, user.email, user.firstname, user.lastname)]
                cursor = conn.cursor()
                cursor.executemany("INSERT INTO users VALUES(?,?,?,?,?,?,1)", data)
                users[addr] = user.email
                return True
    return False


### --- GAME LOGIC --- ###

def check_multiplayer_users(data, socket):
    """Handles multiplayer connection logic: joining or leaving."""
    global users_connected, game_sockets, player_sessions, player1_already_assign
    try:
        if data == "connected":
            if users_connected < 2:
                users_connected += 1
                game_sockets.append(socket)
                if player_sessions["player1"] is None:
                    player_sessions["player1"] = socket
                elif player_sessions["player2"] is None:
                    player_sessions["player2"] = socket
                return True
            return False
        elif data == "disconnected":
            users_connected -= 1
            if users_connected == 0:
                player1_already_assign = False
            game_sockets.remove(socket)
            if player_sessions["player1"] == socket:
                player_sessions["player1"] = None
            elif player_sessions["player2"] == socket:
                player_sessions["player2"] = None
            return True
    except Exception as e:
        print(f"Error occurred: {e}")

def check_opponent_connected():
    """Returns True if both players are connected."""
    return users_connected == 2

def assign_player1(socket):
    """Assigns player1 if not already assigned."""
    global player1_already_assign
    if not player1_already_assign:
        player_sessions["player1"] = socket
        player1_already_assign = True
        return True
    return False

def game_actions(from_socket, action_data):
    """Relays player action to the opponent's socket."""
    try:
        opponent_socket = next(
            (sock for role, sock in player_sessions.items() if sock != from_socket and sock is not None), None
        )
        if opponent_socket:
            message = json.dumps({
                "action": "opponent_action",
                "data": action_data
            }) + "\n"
            opponent_socket.sendall(message.encode('utf-8'))
            return True
        print("Opponent not connected.")
        return True
    except Exception as e:
        print(f"Error relaying action: {e}")
        return False


### --- SKIN SYSTEM --- ###

def get_player_skin(skinId, addr):
    """Check if the given skinId matches the current user's skin."""
    email = users[addr]
    with closing(sqlite3.connect(DB_FILE)) as conn:
        with conn:
            cursor = conn.cursor()
            cursor.execute("SELECT skin_id FROM users WHERE email = ?", (email,))
            result = cursor.fetchone()
            return result and result[0] == int(skinId)

def set_player_skin(skinId, addr):
    """Updates the current user's selected skin."""
    try:
        email = users[addr]
        with closing(sqlite3.connect(DB_FILE)) as conn:
            cursor = conn.cursor()
            cursor.execute("UPDATE users SET skin_id = ? WHERE email = ?", (skinId, email))
            conn.commit()
            return cursor.rowcount > 0
    except sqlite3.OperationalError as e:
        print("SQLite error:", e)
        return False


### --- REQUEST HANDLER --- ###

def handle_request(request, socket, addr):
    """Route incoming client requests based on 'action' field."""
    try:
        action = request.get("action")
        data = request.get("data", {})

        if isinstance(data, str):
            data = json.loads(data)

        match action:
            case 'check_multiplayer_users':
                return check_multiplayer_users(data, socket)
            case 'check_opponent_connected':
                return check_opponent_connected()
            case 'player_action':
                return game_actions(socket, data)
            case 'check_player_one':
                return assign_player1(socket)
            case 'check_username':
                return check_username(data['username'])
            case 'check_email':
                return check_email(data['email'])
            case 'user_is_already_connected':
                return user_is_already_connected(data['email'])
            case 'check_login':
                return check_login(data['password'], data['email'], addr)
            case 'create_user':
                user = User(
                    username=data['username'],
                    password=data['password'],
                    email=data['email'],
                    firstname=data['firstname'],
                    lastname=data['lastname']
                )
                return create_user(user, addr)
            case 'get_player_skin':
                return get_player_skin(data["skinId"], addr)
            case 'set_player_skin':
                return set_player_skin(data["skinId"], addr)
            case _:
                return False
    except Exception as e:
        print(f"Request handling error: {e}")
        return False


### --- SOCKET SERVER SETUP --- ###

def start_server(host="0.0.0.0", port=5000):
    """Starts the main TCP server loop."""
    global player1_already_assign
    try:
        server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server_socket.bind((host, port))
        server_socket.listen()
        print(f"Server is running on {host}:{port}")
    except socket.error as e:
        print(f"Failed to set up server: {e}")
        return

    client_sockets = []

    try:
        while True:
            rlist, _, _ = select.select([server_socket] + client_sockets, [], [])
            for current_socket in rlist:
                if current_socket is server_socket:
                    conn, addr = server_socket.accept()
                    print(f"Connected by {addr}")
                    client_sockets.append(conn)
                    users[addr[1]] = None
                    buffers[conn] = ""
                else:
                    try:
                        data = current_socket.recv(1024)
                        if not data:
                            print("Client disconnected")
                            client_sockets.remove(current_socket)
                            current_socket.close()
                            buffers.pop(current_socket, None)
                            continue

                        buffers[current_socket] += data.decode('utf-8') #Add the data into buffer and make a queue
                        while '\n' in buffers[current_socket]:
                            line, buffers[current_socket] = buffers[current_socket].split('\n', 1)
                            if not line.strip():
                                continue
                            request = json.loads(line)
                            response = handle_request(request, current_socket, addr[1])
                            print("Parsed request:", request)
                            if request["action"] != 'player_action':
                                current_socket.sendall((json.dumps(response) + "\n").encode('utf-8'))

                    except Exception as e:
                        print(f"Error handling client: {e}")
                        if current_socket in client_sockets:
                            client_sockets.remove(current_socket)
                        current_socket.close()
                        buffers.pop(current_socket, None)
    finally:
        server_socket.close()
        for sock in client_sockets:
            try:
                sock.close()
            except socket.error as e:
                print(f"Error closing socket: {e}")


### --- DATA CLASS --- ###

class User:
    def __init__(self, username, password, email, firstname, lastname):
        self.username = username
        self.password = password
        self.email = email
        self.firstname = firstname
        self.lastname = lastname

    def __repr__(self):
        return f"User(username={self.username}, email={self.email}, firstname={self.firstname}, lastname={self.lastname})"


### --- ENTRY POINT --- ###

if __name__ == "__main__":
    start_server()
