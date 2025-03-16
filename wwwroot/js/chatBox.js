document.addEventListener("DOMContentLoaded", function () {
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

    let userName = null;

    const storedName = localStorage.getItem("chatUserName");
    if (storedName && storedName.trim() !== "") {
        userName = storedName.trim();
        nameInputSection.style.display = "none";
        messageInputSection.style.display = "flex";
    } else {
        nameInputSection.style.display = "flex";
        messageInputSection.style.display = "none";
    }

    chatBubble.addEventListener("click", function () {
        chatContainer.style.display = "flex";
        chatBubble.style.display = "none";
    });

    closeChat.addEventListener("click", function () {
        chatContainer.style.display = "none";
        chatBubble.style.display = "flex";
    });

    saveNameButton.addEventListener("click", function () {
        const inputVal = userNameInput.value.trim();
        if (inputVal !== "") {
            userName = inputVal;
            localStorage.setItem("chatUserName", userName);

            nameInputSection.style.display = "none";
            messageInputSection.style.display = "flex";
        }
    });

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/livechat")
        .build();

    connection.start()
        .then(() => {
            connection.invoke("LoadPreviousMessages")
                .catch(err => console.error(err.toString()));
        })
        .catch(err => console.error(err.toString()));

    sendButton.addEventListener("click", function () {
        if (!userName) {
            alert("Please set your name first.");
            return;
        }

        const message = messageInput.value.trim();
        if (message === "") return;

        const chatMessageDto = {
            userName: userName,
            message: message,
            sentAt: new Date().toISOString()
        };

        connection.invoke("SendMessage", chatMessageDto)
            .catch(err => console.error(err.toString()));

        messageInput.value = "";
    });

    connection.on("ReceiveMessage", function (chatMessageDto) {
        addMessageToList(chatMessageDto);
    });

    connection.on("LoadMessages", function (messages) {
        messagesList.innerHTML = "";
        messages.forEach(msg => addMessageToList(msg));
    });

    function addMessageToList(msg) {
        const item = document.createElement("li");
        const time = new Date(msg.sentAt).toLocaleTimeString();
        item.textContent = `[${time}] ${msg.userName}: ${msg.message}`;
        messagesList.appendChild(item);
        messagesList.scrollTop = messagesList.scrollHeight; // auto-scroll
    }
});