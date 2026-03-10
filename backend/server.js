const WebSocket = require("ws");
const {
  createHandshakeCommand,
  handleClientMessage,
} = require("./messageHandler");

const PORT = 8080;
const clientServerConnection = new WebSocket.Server({ port: PORT });

clientServerConnection.on("connection", (ws) => {
  console.log(`[NETWORK] New client connected!`);

  // 1. Handshake: Immediately upon connection, generate and send a command via the logic layer
  const handshakeCommand = createHandshakeCommand();
  ws.send(handshakeCommand);
  console.log(`[NETWORK] Handshake command sent.`);

  // 2. Listen for client responses
  ws.on("message", (message) => {
    // Pass raw data to the logic layer - the server is decoupled from message content
    handleClientMessage(message);
  });

  ws.on("close", () => {
    console.log(`[NETWORK] Client disconnected.`);
  });

  ws.on("error", (error) => {
    console.error(`[NETWORK] WebSocket error: ${error.message}`);
  });
});

console.log(
  `[NETWORK] Server is running and listening on ws://localhost:${PORT}`,
);
