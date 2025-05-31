import sqlite3
import os
import json
import socket
from contextlib import closing
from validator import validateUser
import select
import bcrypt

DB_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "../1vs1Football.db")
users_connected = 0
opponent1 = False
opponent2 = False
player1_already_assign = False
game_sockets = []
player_sessions = {"player1": None, "player2": None}
buffers = {}  # Each socket will get its own buffer
users = {}


def check_username(username):
    with closing(sqlite3.connect(DB_FILE)) as conn:
        with conn:
            cursor = conn.cursor()
            cursor.execute(f'select username from users where username="{username}"')
            return cursor.fetchone() is not None


def game_actions(from_socket, action_data):
    try:
        opponent_socket = None
        for role, sock in player_sessions.items():
            if sock != from_socket and sock is not None:
                opponent_socket = sock
                break
        if opponent_socket:
            message = json.dumps({
                "action": "opponent_action",
                "data": action_data
            }) +"\n"
            opponent_socket.sendall(message.encode('utf-8'))
            return True
        else:
            print("Opponent not connected.")
            return True
    except Exception as e:
        print(f"Error relaying action: {e}")
        return False


def check_email(email):
    with closing(sqlite3.connect(DB_FILE)) as conn:
        with conn:
            cursor = conn.cursor()
            cursor.execute(f'select email from users where email="{email}"')
            return cursor.fetchone() is not None


def check_login(password, email,adrr):
    key =   True
    key = user_is_already_connected(email)
    if key:
        with closing(sqlite3.connect(DB_FILE)) as conn:
            with conn:
                cursor = conn.cursor()
                print(password)
                cursor.execute("SELECT password FROM users WHERE email = ?", (email,))
                result = cursor.fetchone()
                if result:
                    if bcrypt.checkpw(password.encode(),result[0] ) :
                        users[adrr] = email
                        return True
                    else:
                        return False
                else:
                    return False
    else:
        return False

def user_is_already_connected(email):
    for socketkey,user in users.items():
            if user == email:
                return False
    return True

def check_multiplayer_users(data, socket):
    global users_connected, game_sockets, player_sessions,player1_already_assign 
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
            else:
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
    return users_connected == 2


def create_user(user,adrr):
    if validateUser(user):
        with closing(sqlite3.connect(DB_FILE)) as conn:
            with conn:
                user.password = bcrypt.hashpw(user.password.encode(), bcrypt.gensalt())
                data = [(None, user.username, user.password, user.email, user.firstname, user.lastname)]    
                cursor = conn.cursor()
                cursor.executemany("INSERT INTO users VALUES(?,?,?,?,?,?,null)", data)
                users[adrr] = user.email
                return True 

def assign_player1(socket):
    global player1_already_assign

    if player1_already_assign == False:
        player_sessions["player1"] = socket
        player1_already_assign = True
        return True
    else:
        return False

def handle_request(request, socket ,adrr):
    try:
        action = request['action']
        if action == 'check_multiplayer_users':
            return check_multiplayer_users(request['data'], socket)
        elif action == 'check_opponent_connected':
            return check_opponent_connected()
        elif action == 'player_action':
            return game_actions(socket, request['data'])
        elif action == "check_player_one":
            return assign_player1(socket)

        data = request.get('data', {})
        print(action)
        if isinstance(data, str):
            data = json.loads(data)

        if action == 'check_username':
            return check_username(data['username'])
        elif action == 'check_email':
            return check_email(data['email'])
        elif action == 'user_is_already_connected':
            return user_is_already_connected(data['email'])
        elif action == 'check_login':
            return check_login(data['password'], data['email'],adrr)
        elif action == 'create_user':
            user = User(
                username=data['username'],
                password=data['password'],
                email=data['email'],
                firstname=data['firstname'],
                lastname=data['lastname']
            )
            return create_user(user,adrr)
        else:
            return False
    except Exception as e:
        print(f"Request handling error: {e}")
        return False



def start_server(host="0.0.0.0", port=5000):
    global player1_already_assign
    try:
        server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server_socket.bind((host, port)) #building the server
        server_socket.listen() #lisening for users
        player1_already_assign = False
        print(f"Server is running on {host}:{port}")
    except socket.error as e:
        #server error
        print(f"Failed to set up server: {e}")
        return

    client_sockets = []

    try:
        while True:
            rlist, _, _ = select.select([server_socket] + client_sockets, [], [])

            for current_socket in rlist:
                if current_socket is server_socket:
                    conn, addr = server_socket.accept() #accept clients
                    print(f"Connected by {addr}")
                    client_sockets.append(conn)     #saving the conn off every client
                    users[addr[1]] = None
                    buffers[conn] = "" 
                else:
                    try:
                        data = current_socket.recv(1024) # receiving the message
                        if not data:            #without data the client disconnect
                            print("Client disconnected")
                            client_sockets.remove(current_socket)
                            users.pop(addr[1],None)
                            buffers.pop(current_socket, None)
                            current_socket.close()
                            continue
                        buffers[current_socket] += data.decode('utf-8') #add to buffer
                        while '\n' in buffers[current_socket]:
                            line, buffers[current_socket] = buffers[current_socket].split('\n', 1)
                            if not line.strip():
                                continue
                            try:
                                request = json.loads(line)
                                response = handle_request(request, current_socket,addr[1])
                                print("Parsed request game:", request)
                                if request["action"] != 'player_action':
                                    current_socket.sendall((json.dumps(response) + "\n").encode('utf-8'))
                            except json.JSONDecodeError as e:
                                print("JSON decode error:", e)
                    except Exception as e:
                        print(f"Error handling client: {e}")
                        if current_socket in client_sockets:
                            client_sockets.remove(current_socket)
                        buffers.pop(current_socket, None)
                        current_socket.close()
    finally:
        server_socket.close()
        for sock in client_sockets:
            try:
                sock.close()
            except socket.error as e:
                print(f"Error closing socket: {e}")


class User:
    def __init__(self, username, password, email, firstname, lastname):
        self.username = username
        self.password = password
        self.email = email
        self.firstname = firstname
        self.lastname = lastname

    def __repr__(self):
        return f"User(username={self.username}, email={self.email}, firstname={self.firstname}, lastname={self.lastname})"


if __name__ == "__main__":
    start_server()
