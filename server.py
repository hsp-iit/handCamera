import socket
import cv2
import numpy as np

# Set up the UDP socket
UDP_IP = "0.0.0.0"  # Listen on all interfaces
UDP_PORT = 9999
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))

try:
    while True:
        # Receive the data
        data, addr = sock.recvfrom(65536)  # Buffer size large enough for compressed images

        # Print the number of bytes received
        # print(f"Received {len(data)} bytes from {addr}")
        if len(data) < 10000:
            print(data.decode('utf-8'))

        # Convert byte data to numpy array
        np_arr = np.frombuffer(data, np.uint8)

        # Decode the image from the compressed format
        img = cv2.imdecode(np_arr, cv2.IMREAD_COLOR)

        if img is not None:
            cv2.imshow("Received Image", img)
            cv2.waitKey(1)
except KeyboardInterrupt:
    cv2.destroyAllWindows()
    sock.close()