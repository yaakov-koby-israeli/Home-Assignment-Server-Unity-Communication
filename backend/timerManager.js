const WebSocket = require("ws");
const {
  createTimeoutCommand,
  createGoodbyeCommand,
} = require("./messageHandler");

class ClientTimer {
  constructor(ws, warningTime = 10000, disconnectTime = 5000) {
    this.ws = ws; //current client connection
    this.warningTime = warningTime;
    this.disconnectTime = disconnectTime;
    this.warningTimeout = null;
    this.disconnectTimeout = null;
  }

  // start the timers when the client connects or after a successful interaction
  start() {
    this.clear(); // first clear any existing timers to avoid duplicates

    this.warningTimeout = setTimeout(() => {
      console.log(
        `[NETWORK] Client inactive for ${this.warningTime / 1000} seconds. Sending warning...`,
      );
      if (this.ws.readyState === WebSocket.OPEN) {
        this.ws.send(createTimeoutCommand());
      }

      this.disconnectTimeout = setTimeout(() => {
        console.log(`[NETWORK] Disconnecting inactive client.`);
        if (this.ws.readyState === WebSocket.OPEN) {
          this.ws.send(createGoodbyeCommand());
          this.ws.close();
        }
      }, this.disconnectTime);
    }, this.warningTime);
  }

  // reset the timers (e.g., when we get a valid response from the client)
  reset() {
    console.log(`[NETWORK] Resetting inactivity timers.`);
    this.start();
  }

  // clear all timers (e.g., when client disconnects or on error)
  clear() {
    if (this.warningTimeout) clearTimeout(this.warningTimeout);
    if (this.disconnectTimeout) clearTimeout(this.disconnectTimeout);
  }
}

module.exports = ClientTimer;
