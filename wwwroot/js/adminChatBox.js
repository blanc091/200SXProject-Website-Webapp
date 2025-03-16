document.addEventListener("DOMContentLoaded", function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/livechat")
        .build();

    // Join the admin group
    connection.start()
        .then(() => {
            connection.invoke("JoinAdminGroup")
                .catch(err => console.error(err.toString()));
        })
        .catch(err => console.error(err.toString()));

    // Container elements
    const pendingChatsContainer = document.getElementById("pendingChatsContainer");
    const chatSessionsContainer = document.getElementById("chatSessionsContainer");

    // Keep track of sessions locally
    const sessions = {};

    // Listen for new chat sessions
    connection.on("NewChatSession", function (sessionId) {
        sessions[sessionId] = { sessionId: sessionId, userName: "Unknown" };
        updatePendingChats();
    });

    // Optionally update sessions when user sets their name
    connection.on("UpdateChatSession", function (sessionId, userName) {
        if (sessions[sessionId]) {
            sessions[sessionId].userName = userName;
            updatePendingChats();
        }
    });

    // Helper function to update pending chat list UI
    function updatePendingChats() {
        pendingChatsContainer.innerHTML = "";
        for (let sessionId in sessions) {
            let session = sessions[sessionId];
            let btn = document.createElement("button");
            btn.textContent = `Chat with ${session.userName} (${sessionId.substr(0, 5)})`;
            btn.addEventListener("click", function () {
                openChatSession(sessionId);
            });
            pendingChatsContainer.appendChild(btn);
        }
    }

    // When an admin chooses to open a chat session, load a partial view
    function openChatSession(sessionId) {
        // Create a container for the session chat
        const sessionDiv = document.createElement("div");
        sessionDiv.classList.add("adminChatSession");
        sessionDiv.innerHTML = `
            <h4>Chat Session: ${sessionId.substr(0, 5)}</h4>
            <div class="sessionMessages" id="sessionMessages_${sessionId}"></div>
            <input type="text" placeholder="Your reply" id="sessionInput_${sessionId}" />
            <button id="sessionSend_${sessionId}">Send</button>
            <hr/>
        `;
        chatSessionsContainer.appendChild(sessionDiv);

        // Have the admin join the specific session group
        connection.invoke("JoinChatSession", sessionId)
            .catch(err => console.error(err.toString()));

        // Attach send message logic for this session
        document.getElementById(`sessionSend_${sessionId}`).addEventListener("click", function () {
            const messageInput = document.getElementById(`sessionInput_${sessionId}`);
            const message = messageInput.value.trim();
            if (message !== "") {
                connection.invoke("AdminSendMessageToSession", sessionId, message)
                    .catch(err => console.error(err.toString()));
                messageInput.value = "";
            }
        });

        // Listen for messages for this session
        connection.on("ReceiveMessage", function (chatMessageDto) {
            // Only display if the message is for this session.
            // (If multiple sessions are open, you'll need to route messages appropriately.)
            // For simplicity, assume admin only gets messages for sessions they've joined.
            const sessionMessages = document.getElementById(`sessionMessages_${sessionId}`);
            if (sessionMessages) {
                const msg = document.createElement("div");
                const time = new Date(chatMessageDto.sentAt).toLocaleTimeString();
                msg.textContent = `[${time}] ${chatMessageDto.userName}: ${chatMessageDto.message}`;
                sessionMessages.appendChild(msg);
                sessionMessages.scrollTop = sessionMessages.scrollHeight;
            }
        });
    }

    // Toggle pending chats display
    document.getElementById("togglePendingChats").addEventListener("click", function () {
        if (pendingChatsContainer.style.display === "none" || !pendingChatsContainer.style.display) {
            pendingChatsContainer.style.display = "block";
        } else {
            pendingChatsContainer.style.display = "none";
        }
    });
});