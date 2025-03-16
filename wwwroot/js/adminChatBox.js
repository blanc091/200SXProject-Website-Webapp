document.addEventListener("DOMContentLoaded", function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/livechat")
        .build();

    connection.start()
        .then(() => {
            connection.invoke("JoinAdminGroup")
                .catch(err => console.error(err.toString()));
        })
        .catch(err => console.error(err.toString()));

    const pendingChatsContainer = document.getElementById("pendingChatsContainer");
    const chatSessionsContainer = document.getElementById("chatSessionsContainer");

    const sessions = {};

    fetch('/Admin/GetPendingChatSessions')
        .then(response => response.json())
        .then(pendingSessions => {
            pendingSessions.forEach(session => {
                sessions[session.sessionId] = session;
            });
            updatePendingChats();
        })
        .catch(error => console.error('Error fetching pending sessions:', error));

    connection.on("NewChatSession", function (sessionId) {
        if (!sessions[sessionId]) {
            sessions[sessionId] = { sessionId: sessionId, userName: "Unknown", isAnswered: false };
        }
        updatePendingChats();
    });

    connection.on("UpdateChatSession", function (sessionId, userName) {
        if (sessions[sessionId]) {
            sessions[sessionId].userName = userName;
            updatePendingChats();
        }
    });

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

    function openChatSession(sessionId) {
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

        connection.invoke("JoinChatSession", sessionId)
            .catch(err => console.error(err.toString()));

        document.getElementById(`sessionSend_${sessionId}`).addEventListener("click", function () {
            const messageInput = document.getElementById(`sessionInput_${sessionId}`);
            const message = messageInput.value.trim();
            if (message !== "") {
                connection.invoke("AdminSendMessageToSession", sessionId, message)
                    .catch(err => console.error(err.toString()));
                messageInput.value = "";
            }
        });

        connection.on("ReceiveMessage", function (chatMessageDto) {
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

    document.getElementById("togglePendingChats").addEventListener("click", function () {
        if (pendingChatsContainer.style.display === "none" || !pendingChatsContainer.style.display) {
            pendingChatsContainer.style.display = "block";
        } else {
            pendingChatsContainer.style.display = "none";
        }
    });
});
