const {
  createHandshakeCommand,
  handleClientMessage,
} = require("./messageHandler");

describe("Message Handler Logic", () => {
  // Test 1: Verify the handshake command structure
  test("createHandshakeCommand should return a valid JSON string with COMMAND type", () => {
    const commandStr = createHandshakeCommand();
    const commandObj = JSON.parse(commandStr);

    expect(commandObj).toBeDefined();
    expect(commandObj.type).toBe("COMMAND");
    expect(commandObj.action).toBe("SHOW_WELCOME_MESSAGE");
    expect(commandObj.payload).toBeDefined();
  });

  // Test 2: Verify it handles correct client responses properly
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

  // Test 3: Verify error handling (Exception handling requirement)
  test("handleClientMessage should handle invalid JSON gracefully and return null", () => {
    const invalidJson = "This is definitely not a JSON object";

    // The console.error will fire during the test, which is expected
    const result = handleClientMessage(invalidJson);

    expect(result).toBeNull(); // Asserts that our try-catch works
  });
});
