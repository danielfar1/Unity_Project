import socket

HOST = "127.0.0.1"
PORT = 9000

def start_socket_server():
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server_socket.bind((HOST, PORT))
    server_socket.listen(5)
    print(f"Socket server is running on {HOST}:{PORT}")

    while True:
        client_socket, client_address = server_socket.accept()
        print(f"Connection established with {client_address}")
        handle_client(client_socket)

def handle_client(client_socket):
    try:
        while True:
            data = client_socket.recv(1024).decode("utf-8")
            if not data:
                break
            print(f"Received: {data}")

            # Echo the message back to the client
            response = f"Echo: {data}"
            client_socket.send(response.encode("utf-8"))
    except Exception as e:
        print(f"Socket error: {e}")
    finally:
        client_socket.close()
