# 🎮 Unity & Node.js WebSocket Integration

![Node.js](https://img.shields.io/badge/Node.js-43853D?style=for-the-badge&logo=node.js&logoColor=white)
![Unity](https://img.shields.io/badge/Unity-100000?style=for-the-badge&logo=unity&logoColor=white)
![Jest](https://img.shields.io/badge/Jest-C21325?style=for-the-badge&logo=jest&logoColor=white)

> A bidirectional WebSocket communication system between a **Node.js Server** and a **Unity 3D Client** using WebSockets.
---

## 📽️ Live Demonstration

*(Replace this placeholder with your actual recorded GIF to show the server-client handshake and crystal transitions!)*

---

## 🏗️ Architecture Overview

The system follows a decoupled architecture where the server acts as the "Test Controller" and the Unity client acts as the "System Under Test" (SUT).

```mermaid
graph LR
    subgraph "Server (Node.js)"
    A[Command Dispatcher] --> B((WebSocket Server))
    end
    
    B <--> |JSON Commands| C((Unity Client))
    
    subgraph "Unity (Client)"
    C --> D[Network Manager]
    D --> E[Visual Feedback - Crystals]
    E --> F[On-Screen Logs]
    end

```

---

## 🛠️ Design Decisions

1. **Protocol (WebSockets over TCP):** Chosen for its low-latency, full-duplex nature. Unlike HTTP, WebSockets allow the server to "push" test commands to Unity without the client constantly polling.
2. **Directory Structure:** The project is strictly separated into `/Server` and `/Unity_Client` to ensure clean dependency management and to mimic real-world microservice environments.
3. **On-Screen UI Logger:** Instead of relying solely on the Unity Console (which is hidden in builds), I built an overlay UI logger. This makes the tool immediately useful for manual QA testers running the compiled game.
4. **Defensive Coding:** Every incoming message in Unity is wrapped in a `try-catch` block with JSON validation. If the server sends malformed data, the client logs a "Parse Error" instead of crashing a critical requirement for stable QA tools.
5. **Main Thread Dispatcher:** Since WebSocket events run on background threads, I implemented a `UnityMainThreadDispatcher` to ensure UI and visual updates (crystals) occur safely on Unity's main thread.

---

## 🚀 Installation & Setup

### 1. Server Setup (Node.js)

The server logic and dependencies are isolated within the /Server directory. Open your terminal in the root project folder and run:

```bash
# Navigate to the server directory
cd Server

# Install dependencies (ws, dotenv, jest, etc.)
npm install

# Run automated tests and generate HTML report
npm test

# Start the QA Server
npm start

```

### 2. Unity Client Setup

* **Unity Version:** Developed and tested on **Unity 2018.4.36f1** (LTS).
* **Option A (Quick Run):** Launch the pre-compiled executable at `Unity_Client/Build/Unity_Client.exe`.
* **Option B (Development):** 
    1. Open the `Unity_Client` folder as a project in Unity.
    2. Load `Assets/Scenes/SampleScene.unity`.
    3. Press **Play**.
    4. Use the **"Run Sequence"** button in the bottom-right corner to re-trigger the test flow.

---

## 🧪 Command Reference Table

| Command (`action`) | Unity Visual Response | Expected Client Reply |
| --- | --- | --- |
| `System Standby` | 🔵 **Blue Crystal** Activated | `Action Executed: System Standby` |
| `Test Mode` | 🟣 **Purple Crystal** Activated | `Action Executed: Test Mode` |
| `Ad Delivered` | 🟢 **Green Crystal** Activated | `Action Executed: Ad Delivered` |
| `High Latency` | 🟡 **Yellow Crystal** Activated | `Action Executed: High Latency` |
| `Timeout Error` | 🔴 **Red Crystal** Activated | `Action Executed: Timeout Error` |

---

## 📊 Automated Reporting

This project uses `jest-html-reporter`. After running `npm test`, a professional HTML report is generated at:

`Server/test-report.html`

It provides a visual breakdown of test durations and pass/fail status for the communication protocol.

---

## ⚠️ Known Issues 

* **Auto-Reconnect:** Currently, if the server restarts, the Unity client does not automatically attempt to reconnect.
* **Security:** The current WebSocket implementation is unencrypted (`ws://`). For production-level field testing, `WSS` (Secure WebSockets) should be implemented.
* **Command Acknowledgement:** While Unity logs the execution, adding a unique `Command_ID` feedback loop to the server would enhance tracking of asynchronous tasks.

---

## 👤 Author

**Inbar Kehimker**