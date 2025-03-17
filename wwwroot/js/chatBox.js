document.addEventListener("DOMContentLoaded", function () {
    console.log("DOM fully loaded.");

    const chatBubble = document.getElementById("chatBubble");
    const chatContainer = document.getElementById("chatContainer");
    const closeChat = document.getElementById("closeChat");
    
    const nameInputSection = document.getElementById("nameInputSection");
    if (nameInputSection) {
        console.log("Hiding name input section.");
        nameInputSection.style.display = "none";
    }
    
    const messageInputSection = document.getElementById("messageInputSection");
    const messageInput = document.getElementById("messageInput");
    const sendButton = document.getElementById("sendButton");
    const messagesList = document.getElementById("messagesList");

    chatBubble.addEventListener("click", function () {
        console.log("Chat bubble clicked.");
        chatContainer.style.display = "flex";
        chatBubble.style.display = "none";
    });

    closeChat.addEventListener("click", function () {
        console.log("Chat close clicked.");
        chatContainer.style.display = "none";
        chatBubble.style.display = "flex";
    });

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/livechat")
        .build();

    console.log("Starting connection...");
    connection.start()
        .then(() => {
            console.log("SignalR connection started. Connection ID:", connection.connectionId);
            connection.invoke("LoadPreviousMessages")
                .then(() => console.log("LoadPreviousMessages invoked successfully."))
                .catch(err => console.error("Error invoking LoadPreviousMessages:", err.toString()));
        })
        .catch(err => console.error("Error starting connection:", err.toString()));

    sendButton.addEventListener("click", function () {
        const message = messageInput.value.trim();
        if (message === "") {
            console.log("Empty message; not sending.");
            return;
        }
        console.log("Sending message:", message);
        const chatMessageDto = {
            message: message,
            sentAt: new Date().toISOString()
        };

        connection.invoke("SendMessage", chatMessageDto)
            .then(() => console.log("Message sent successfully."))
            .catch(err => console.error("Error sending message:", err.toString()));

        messageInput.value = "";
    });

    connection.on("ReceiveMessage", function (chatMessageDto) {
        console.log("ReceiveMessage event received:", chatMessageDto);
        addMessageToList(chatMessageDto);
    });

    connection.on("LoadMessages", function (messages) {
        console.log("LoadMessages received:", messages);
        messagesList.innerHTML = "";
        messages.forEach(msg => addMessageToList(msg));
    });

    function addMessageToList(msg) {
        console.log("Adding message:", msg);
        const item = document.createElement("li");
        const time = new Date(msg.sentAt).toLocaleTimeString();
        item.textContent = `[${time}] ${msg.userName}: ${msg.message}`;
        messagesList.appendChild(item);
        messagesList.scrollTop = messagesList.scrollHeight; // auto-scroll
    }
});
