# Test Plan: Server-Unity Communication System

## 1. Overview

This document outlines the testing strategy, scenarios, and edge cases for the WebSocket-based communication between the Node.js QA Server and the Unity Client.

## 2. Test Scenarios

### A. Functional Testing (The Happy Path)

* **Successful Handshake**: Verify that the Unity client sends `Unity_Ready` upon connection and the server acknowledges it.
* **Command Sequence**: Verify that all commands (`System Standby`, `Test Mode`, `Ad Delivered`, etc.) are received and executed in the correct order.
* **Visual Feedback**: Confirm that each command triggers the correct crystal activation in Unity (e.g., `Ad Delivered` activates `crystalGreen`).
* **Feedback Loop**: Ensure the server receives and logs the `Action Executed` acknowledgment from Unity for every command.
* **Manual Retest**: Verify that the "Run Sequence" button successfully triggers a fresh handshake and restarts the test sequence without reconnecting.

### B. Connectivity & Edge Cases

* **Late Connection**: Unity starts before the server.
* *Expected*: Unity logs a "Connection Error" and remains stable.


* **Handshake Interruption**: Client disconnects immediately after sending `Unity_Ready`.
* *Handling*: Server logs the connection close and clears internal states.


* **Network Drop**: Simulate a network failure during the 13-second sequence.
* *Handling*: Unity's `ReceiveLoop` catches the exception and logs a "Receive Error".


* **Server Crash**: Unity remains responsive and handles the socket closure gracefully via `OnDestroy`.

### C. Data Integrity & Validation

* **Malformed JSON**: Server sends a non-JSON string.
* *Handling*: Unity logs a "Parse Error" using `try-catch`.


* **Unknown Action**: Server sends an unregistered action (e.g., `"action": "Explode"`).
* *Handling*: Unity reverts to the `default` state (Blue Crystal).


* **Rapid handshakes**: User clicks "Run Sequence" multiple times rapidly.
* *Handling*: Server triggers the sequence based on the latest valid handshake.



### D. UI/UX & Visibility (The "Look & Feel")

* **Log Persistence**: Verify that the `LogManager` maintains a history of up to 50 entries.
* **Auto-Scrolling**: Ensure the `ScrollRect` automatically scrolls to the newest log entry at the bottom.
* **Manual Glimpse**: Confirm the user can manually scroll up to review previous command timestamps during a live run.
* **Console Cleanup**: Verify the "Clear Log" button resets the UI and clears the scroll view.

## 3. Automation Strategy

* **Unit/Integration Tests**: Handled via **Jest** with a simulated client to verify the ping-pong sequence.
* **Reporting**: Automated HTML reports generated via `jest-html-reporter`.

## 4. Manual Verification Tools

* **Interactive Console**: Real-time timestamps and colored tags in Unity for instant debugging.
* **Configurable URL**: `NetworkManager` allows for real-time URI adjustments in the Unity Inspector.