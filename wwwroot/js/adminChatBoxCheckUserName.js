connection.on("UpdateChatSession", function (sessionId, userName) {
    if (sessions[sessionId]) {
        sessions[sessionId].userName = userName;
        updatePendingChats();
    }
});