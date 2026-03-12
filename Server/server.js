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

        const expectedSequence = [
            "System Standby",
            "Test Mode",
            "Ad Delivered",
            "High Latency",
            "Timeout Error"
        ];
        
        let currentStep = 0;

        /**
         * Helper function to wrap a command in a JSON structure and send it.
         * @param {string} actionName - The specific command to be executed by Unity.
         */
        const sendCommand = (actionName) => {
            const command = {
                id: "CMD_" + Math.floor(Math.random() * 1000),
                action: actionName
            };
            
            ws.send(JSON.stringify(command));
            console.log(`[Sent to Unity] ${actionName}`);
        };

        /**
         * Helper function to process the sequence based on responses.
         */
        const sendNextCommand = () => {
            if (currentStep < expectedSequence.length) {
                setTimeout(() => {
                    if (ws.readyState === WebSocket.OPEN) {
                        sendCommand(expectedSequence[currentStep]);
                    }
                }, 1000); 
            } else {
                console.log("✅ All test commands executed successfully.");
            }
        };

        /**
         * Event: Message received from the client.
         * Logic: The server waits for 'Unity_Ready' to begin.
         * Then, it waits for an acknowledgment before sending the next command in the sequence.
         */
        ws.on('message', (message) => {
            const msgStr = message.toString();
            console.log(`[Message from Unity] ${msgStr}`);

            if (msgStr === "Unity_Ready") {
                console.log("Handshake successful! Starting command sequence...");
                currentStep = 0;
                sendNextCommand();
            } 
            else if (msgStr.startsWith("Action Executed:")) {
                console.log(`Unity confirmed execution. Advancing to next step.`);
                currentStep++;
                sendNextCommand();
            }
        });
    });

    return wss;
}

if (require.main === module) { 
    startServer(); 
}

module.exports = { startServer };