document.addEventListener("DOMContentLoaded", function () {
    // Elements
    const chatBubble = document.getElementById("chatBubble");
    const chatContainer = document.getElementById("chatContainer");
    const closeChat = document.getElementById("closeChat");

    const nameInputSection = document.getElementById("nameInputSection");
    const userNameInput = document.getElementById("userNameInput");
    const saveNameButton = document.getElementById("saveNameButton");

    const messageInputSection = document.getElementById("messageInputSection");
    const messageInput = document.getElementById("messageInput");
    const sendButton = document.getElementById("sendButton");

    const messagesList = document.getElementById("messagesList");

    // We'll store the user's name in a variable
    let userName = null;

    // 1. Check localStorage for a stored username
    const storedName = localStorage.getItem("chatUserName");
    if (storedName && storedName.trim() !== "") {
        // If found, set userName and show the message section
        userName = storedName.trim();
        nameInputSection.style.display = "none";
        messageInputSection.style.display = "flex";
    } else {
        // Otherwise, show name input section, hide message section
        nameInputSection.style.display = "flex";
        messageInputSection.style.display = "none";
    }

    // 2. Chat bubble opens the chat container
    chatBubble.addEventListener("click", function () {
        chatContainer.style.display = "flex";
        chatBubble.style.display = "none";
    });

    // 3. Close chat
    closeChat.addEventListener("click", function () {
        chatContainer.style.display = "none";
        chatBubble.style.display = "flex";
    });

    // 4. Save Name button
    saveNameButton.addEventListener("click", function () {
        const inputVal = userNameInput.value.trim();
        if (inputVal !== "") {
            userName = inputVal;
            localStorage.setItem("chatUserName", userName);

            nameInputSection.style.display = "none";
            messageInputSection.style.display = "flex";
        }
    });

    // ----- SignalR Connection -----
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/livechat")
        .build();

    connection.start()
        .then(() => {
            // Load previous messages after connecting
            connection.invoke("LoadPreviousMessages")
                .catch(err => console.error(err.toString()));
        })
        .catch(err => console.error(err.toString()));

    // ----- Send a Message -----
    sendButton.addEventListener("click", function () {
        // Ensure we have a user name
        if (!userName) {
            alert("Please set your name first.");
            return;
        }

        const message = messageInput.value.trim();
        if (message === "") return;

        // Build DTO
        const chatMessageDto = {
            userName: userName,
            message: message,
            sentAt: new Date().toISOString()
        };

        connection.invoke("SendMessage", chatMessageDto)
            .catch(err => console.error(err.toString()));

        // Clear the message box
        messageInput.value = "";
    });

    // ----- Receive Messages -----
    connection.on("ReceiveMessage", function (chatMessageDto) {
        addMessageToList(chatMessageDto);
    });

    connection.on("LoadMessages", function (messages) {
        messagesList.innerHTML = "";
        messages.forEach(msg => addMessageToList(msg));
    });

    // Helper function to append messages to the list
    function addMessageToList(msg) {
        const item = document.createElement("li");
        const time = new Date(msg.sentAt).toLocaleTimeString();
        item.textContent = `[${time}] ${msg.userName}: ${msg.message}`;
        messagesList.appendChild(item);
        messagesList.scrollTop = messagesList.scrollHeight; // auto-scroll
    }
});