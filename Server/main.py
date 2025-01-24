import threading
from restapi.app import app
from socketMultiplayer.server import start_socket_server

if __name__ == "__main__":
    # Start the socket server in a separate thread
    socket_thread = threading.Thread(target=start_socket_server, daemon=True)
    socket_thread.start()

    # Start the REST API server
    import uvicorn
    print("Starting REST API server on http://127.0.0.1:8000")
    uvicorn.run(app, host="127.0.0.1", port=8000)
