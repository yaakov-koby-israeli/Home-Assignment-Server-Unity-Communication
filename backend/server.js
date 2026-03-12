const WebSocket = require("ws");
const {
  createHandshakeCommand,
  handleClientMessage,
  createAckCommand,
  createTimeoutCommand,
  createGoodbyeCommand,
} = require("./messageHandler");

const ClientTimer = require("./timerManager");

const PORT = 8080;
const clientServerConnection = new WebSocket.Server({ port: PORT });

clientServerConnection.on("connection", (ws) => {
  console.log(`[NETWORK] New client connected!`);

  // 1. Handshake: Immediately upon connection, generate and send a command via the logic layer
  const handshakeCommand = createHandshakeCommand();
  ws.send(handshakeCommand);
  console.log(`[NETWORK] Handshake command sent.`);

  // Initialize a timer manager for this client connection
  const clientTimer = new ClientTimer(ws);
  clientTimer.start();

  // 2. Listen for client responses
  ws.on("message", (message) => {
    // Pass raw data to the logic layer - the server is decoupled from message content
    const parsedMessage = handleClientMessage(message);

    // if client pressed a key confirming handshake, we can send an ACK command back
    if (parsedMessage && parsedMessage.type === "HANDSHAKE_RESPONSE") {
      clientTimer.clear(); // Clear the warning timeout since we got a response
      const ackCommand = createAckCommand();
      console.log("[NETWORK] Timeout cancelled, client is active.");
      ws.send(ackCommand);
    } else if (parsedMessage && parsedMessage.type === "INVALID_KEY_PRESSED") {
      clientTimer.reset();
    }
  });

  ws.on("close", () => {
    console.log(`[NETWORK] Client disconnected.`);
    ws.send(createGoodbyeCommand());

    // Clear any pending timeouts when client disconnects
    clientTimer.clear();

    ws.close(); // Ensure the connection is closed
  });

  ws.on("error", (error) => {
    console.error(`[NETWORK] WebSocket error: ${error.message}`);
    // Clear any pending timeouts when client disconnects
    clientTimer.clear();
    ws.close(); // Ensure the connection is closed on error
  });
});

console.log(
  `[NETWORK] Server is running and listening on ws://localhost:${PORT}`,
);
