require('dotenv').config();
const WebSocket = require('ws');

/**
 * Starts a WebSocket server for Testing with Unity.
 * @param {number} port - The port to listen on (default: from .env or 8080).
 * @returns {WebSocket.Server} - The server instance.
 */
function startServer(port = process.env.PORT || 8080) {
    const wss = new WebSocket.Server({ port }, () => {
        console.log(`QA Server started on port ${port}. Waiting for Unity handshake...`);
    });

    // Event: Triggered when Unity connects
    wss.on('connection', (ws) => {
        console.log("Connection detected. Awaiting 'Unity_Ready'...");

        /**
         * Helper function to wrap a command in a JSON structure and send it.
         * @param {string} actionName - The specific command to be executed by Unity.
         */
        const sendCommand = (actionName) => {
            const command = {
                // Generate a random ID for tracking/logging purposes
                id: "CMD_" + Math.floor(Math.random() * 1000),
                action: actionName
            };
            
            // Send the serialized JSON command to the client
            ws.send(JSON.stringify(command));
            console.log(`[Sent to Unity] ${actionName}`);
        };

        /**
         * Event: Message received from the client.
         * Logic: The server waits for a specific 'Unity_Ready' signal before
         * triggering the automated sequence of test commands.
         */
        ws.on('message', (message) => {
            const msgStr = message.toString();
            console.log(`[Message from Unity] ${msgStr}`);

            if (msgStr === "Unity_Ready") {
                console.log("Handshake successful! Sending commands...");

                setTimeout(() => sendCommand("System Standby"), 1000);
                setTimeout(() => sendCommand("Test Mode"), 4000);
                setTimeout(() => sendCommand("Ad Delivered"), 7000);
                setTimeout(() => sendCommand("High Latency"), 10000);
                setTimeout(() => sendCommand("Timeout Error"), 13000);
            }
        });
    });

    return wss;
}

if (require.main === module) { 
    startServer(); 
}

module.exports = { startServer };