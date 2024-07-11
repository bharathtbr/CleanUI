window.startWebSocket = (dotNetHelper) => {
    try {
        const socket = new WebSocket("wss://" + window.location.host + "/ws");
        const apiSocket = new WebSocket("wss://" + window.location.host + "/api/ws");

        socket.onmessage = function (event) {
            console.log("Message received from server: ", event.data);
            dotNetHelper.invokeMethodAsync('ReceiveMessage', event.data);
        };

        socket.onopen = function (event) {
            console.log("WebSocket connection opened");
            socket.send("Hello from Blazor!");
        };

        socket.onclose = function (event) {
            console.log("WebSocket closed: " + event.reason);
        };

        socket.onerror = function (event) {
            console.error("WebSocket error: ", event);
        };

        apiSocket.onmessage = function (event) {
            console.log("Message received from server: ", event.data);
            dotNetHelper.invokeMethodAsync('ReceiveMessage', event.data);
        };

        apiSocket.onopen = function (event) {
            console.log("WebSocket connection opened");
            socket.send("Hello from Blazor!");
        };

        apiSocket.onclose = function (event) {
            console.log("WebSocket closed: " + event.reason);
        };

        apiSocket.onerror = function (event) {
            console.error("WebSocket error: ", event);
        };

        return {
            sendMessage: (message) => {
                socket.send(message);
                apiSocket.send(message);
            }
        };
    } catch (error) {
        console.error("Failed to start WebSocket: ", error);
        throw error;
    }
};
