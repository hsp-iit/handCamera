import asyncio
import websockets
import cv2
import numpy as np
import json
import time  # Added for FPS calculation

# WebSocket server configuration
WEBSOCKET_HOST = "0.0.0.0"  # Listen on all interfaces
WEBSOCKET_PORT = 9999

async def handle_client(websocket):
    frame_count = 0
    start_time = time.time()

    try:
        async for message in websocket:
            # Check if the message is binary (image) or text (JSON)
            if isinstance(message, bytes):
                # Handle image data
                np_arr = np.frombuffer(message, np.uint8)
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
            else:
                # Handle JSON data
                human_data = json.loads(message)
                print(human_data)
    except websockets.exceptions.ConnectionClosed as e:
        print(f"Connection closed: {e}")
    finally:
        cv2.destroyAllWindows()

async def main():
    async with websockets.serve(handle_client, WEBSOCKET_HOST, WEBSOCKET_PORT):
        print(f"WebSocket server started on ws://{WEBSOCKET_HOST}:{WEBSOCKET_PORT}")
        await asyncio.Future()  # Run forever

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("Server stopped.")
        cv2.destroyAllWindows()