const {
  // ref to the real functions we want to test from messageHandler.js we want to test
  createHandshakeCommand,
  handleClientMessage,
} = require("./messageHandler");

describe("Message Handler Logic", () => {
  // test 1: ensure the handshake command is correctly formatted and contains expected fields (data validation)
  test("createHandshakeCommand should return a valid JSON string with COMMAND type", () => {
    const commandStr = createHandshakeCommand();
    const commandObj = JSON.parse(commandStr);

    expect(commandObj).toBeDefined();
    expect(commandObj.type).toBe("COMMAND");
    expect(commandObj.action).toBe("SHOW_WELCOME_MESSAGE");
    expect(commandObj.payload).toBeDefined();
  });

  // test 2: ensure that handleClientMessage correctly parses a valid client message and identifies its type
  test("handleClientMessage should parse valid HANDSHAKE_RESPONSE", () => {
    const mockClientMessage = JSON.stringify({
      type: "HANDSHAKE_RESPONSE",
      payload: "Client acknowledged",
    });

    const result = handleClientMessage(mockClientMessage);

    expect(result).not.toBeNull();
    expect(result.type).toBe("HANDSHAKE_RESPONSE");
    expect(result.payload).toBe("Client acknowledged");
  });

  // test 3: ensure server identifies client invalid input
  test("handleClientMessage should parse INVALID_KEY_PRESSED", () => {
    const mockInvalidKeyMessage = JSON.stringify({
      type: "INVALID_KEY_PRESSED",
      payload: "User pressed the wrong key",
    });

    const result = handleClientMessage(mockInvalidKeyMessage);

    expect(result).not.toBeNull();
    expect(result.type).toBe("INVALID_KEY_PRESSED");
  });

  // test : ensure that handleClientMessage handles invalid JSON input without crashing and returns null
  test("handleClientMessage should handle invalid JSON gracefully and return null", () => {
    const invalidJson = "This is definitely not a JSON object";

    // we expect this to not throw an error and instead return null
    const result = handleClientMessage(invalidJson);

    expect(result).toBeNull();
  });
});
