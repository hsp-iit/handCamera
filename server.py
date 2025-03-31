import socket
import cv2
import numpy as np
import json
import time  # Added for FPS calculation

# Set up the UDP socket
UDP_IP = "0.0.0.0"  # Listen on all interfaces
UDP_PORT = 9999
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))

try:
    frame_count = 0
    start_time = time.time()
    
    while True:
        data, addr = sock.recvfrom(65536)  # Buffer size large enough for compressed images

        if len(data) < 10000:
            data = data[1:]
            human_data = json.loads(data.decode('utf-8'))
            print(human_data)
        else:
            np_arr = np.frombuffer(data, np.uint8)
            img = cv2.imdecode(np_arr, cv2.IMREAD_COLOR)
            if img is not None:
                frame_count += 1
                elapsed_time = time.time() - start_time
                if elapsed_time > 1.0:  # Update FPS every second
                    fps = frame_count / elapsed_time
                    print(f"FPS: {fps:.2f}")
                    frame_count = 0
                    start_time = time.time()
                cv2.imshow("Received Image", img)
                cv2.waitKey(1)
except KeyboardInterrupt:
    cv2.destroyAllWindows()
    sock.close()