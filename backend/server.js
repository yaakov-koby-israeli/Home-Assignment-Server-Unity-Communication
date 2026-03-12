const WebSocket = require("ws");
const {
  createHandshakeCommand,
  handleClientMessage,
  createAckCommand,
  createTimeoutCommand,
  createGoodbyeCommand,
} = require("./messageHandler");

const PORT = 8080;
const clientServerConnection = new WebSocket.Server({ port: PORT });

clientServerConnection.on("connection", (ws) => {
  console.log(`[NETWORK] New client connected!`);

  // 1. Handshake: Immediately upon connection, generate and send a command via the logic layer
  const handshakeCommand = createHandshakeCommand();
  ws.send(handshakeCommand);
  console.log(`[NETWORK] Handshake command sent.`);

  // Set up a timeout to detect inactivity (e.g., no response from client after 10 seconds)
  let disconnectTimeout;
  let warningTimeout = setTimeout(() => {
    console.log(`[NETWORK] Client inactive for 10 seconds. Sending warning...`);
    ws.send(createTimeoutCommand());

    disconnectTimeout = setTimeout(() => {
      console.log(`[NETWORK] Disconnecting inactive client.`);
      if (ws.readyState === WebSocket.OPEN) {
        ws.send(createGoodbyeCommand());
        ws.close();
      }
    }, 5000);
  }, 10000);

  // 2. Listen for client responses
  ws.on("message", (message) => {
    // Pass raw data to the logic layer - the server is decoupled from message content
    const parsedMessage = handleClientMessage(message);

    // if we get msg from client confirming handshake, we can send an ACK command back
    if (parsedMessage && parsedMessage.type === "HANDSHAKE_RESPONSE") {
      clearTimeout(warningTimeout); // Clear the warning timeout since we got a response
      if (disconnectTimeout) {
        clearTimeout(disconnectTimeout);
      }
      const ackCommand = createAckCommand();
      console.log("[NETWORK] Timeout cancelled, client is active.");
      ws.send(ackCommand);
    }
  });

  ws.on("close", () => {
    console.log(`[NETWORK] Client disconnected.`);
    ws.send(createGoodbyeCommand());

    // Clear any pending timeouts when client disconnects
    clearTimeout(warningTimeout);
    if (disconnectTimeout) {
      clearTimeout(disconnectTimeout);
    }

    ws.close(); // Ensure the connection is closed
  });

  ws.on("error", (error) => {
    console.error(`[NETWORK] WebSocket error: ${error.message}`);
    // Clear any pending timeouts when client disconnects
    clearTimeout(warningTimeout);
    if (disconnectTimeout) {
      clearTimeout(disconnectTimeout);
    }
  });
});

console.log(
  `[NETWORK] Server is running and listening on ws://localhost:${PORT}`,
);
