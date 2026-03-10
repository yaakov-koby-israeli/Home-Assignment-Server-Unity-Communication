// backend/messageHandler.js

/**
 * Creates the handshake command to send to the client.
 * We use JSON to establish a clear communication contract.
 */
const createHandshakeCommand = () => {
  return JSON.stringify({
    type: "COMMAND",
    action: "SHOW_WELCOME_MESSAGE",
    payload: "We are glad you are here! Please press any key to confirm.",
  });
};

/**
 * Receives a raw message from the client, parses it, and decides the next action.
 */
const handleClientMessage = (rawMessage) => {
  try {
    // Convert the incoming network buffer/string into a JS object
    const parsedMessage = JSON.parse(rawMessage);

    // Simple switch case to handle different message types (can evolve into a Command Pattern)
    switch (parsedMessage.type) {
      case "HANDSHAKE_RESPONSE":
        console.log(
          `[LOGIC] Client confirmed handshake with message: ${parsedMessage.payload}`,
        );
        break;
      default:
        console.warn(
          `[LOGIC] Received unknown message type: ${parsedMessage.type}`,
        );
    }

    return parsedMessage; // Returned for easy testing with Jest
  } catch (error) {
    console.error("[LOGIC] Failed to parse client message:", error.message);
    return null;
  }
};

// Export functions so the server (and Jest tests) can use them
module.exports = { createHandshakeCommand, handleClientMessage };
