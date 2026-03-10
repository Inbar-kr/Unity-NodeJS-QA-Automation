const WebSocket = require('ws');
const { startServer } = require('./server'); 

describe('Server Communication Tests', () => {
    let wss;
    const port = 8081;

    // Start a fresh instance of the WebSocket server before running the tests
    beforeAll((done) => {
        wss = startServer(port);
        setTimeout(done, 500);
    });

    /**
     * Teardown: Close the WebSocket server after all tests are finished
     * to prevent memory leaks or open handles.
     */
    afterAll((done) => {
        if (wss) wss.close(() => done());
        else done();
    });

    // Test the complete end-to-end communication flow between server and a simulated client
    it('should complete the full ping-pong command sequence', (done) => {
        const client = new WebSocket(`ws://localhost:${port}`);
        
        const expectedSequence = [
            "System Standby",
            "Test Mode",
            "Ad Delivered",
            "High Latency",
            "Timeout Error"
        ];
        let currentStep = 0;

        /**
         * Event: Connection Open
         * Trigger the sequence by notifying the server that the client is ready.
         */
        client.on('open', () => {
            client.send("Unity_Ready"); 
        });

        /**
         * Event: Message Received
         * Logic: 
         * 1. Parse the incoming command.
         * 2. Verify it matches the expected step in the sequence.
         * 3. Send an acknowledgment back to the server to trigger the next command.
         */
        client.on('message', (data) => {
            const command = JSON.parse(data);
            
            expect(command.action).toBe(expectedSequence[currentStep]);
            
            client.send(`Action Executed: ${command.action}`);
            
            currentStep++;

            if (currentStep === expectedSequence.length) {
                client.close();
                done();
            }
        });

        client.on('error', (err) => done(err));
        
    }, 15000);
});