// Клієнт чату на SignalR
(function () {
    const data = window.chatData;
    const list = document.getElementById('chatMessages');
    const form = document.getElementById('chatForm');
    const textInput = document.getElementById('messageText');
    const recipientSelect = document.getElementById('recipientId');
    const orderInput = document.getElementById('orderNumber');
    const fileInput = document.getElementById('file');
    const sendButton = document.getElementById('sendButton');
    const statusBadge = document.getElementById('chatStatus');
    const errorBox = document.getElementById('chatError');

    // ---------- Відображення повідомлення ----------
    function addBadge(parent, text, cls) {
        const b = document.createElement('span');
        b.className = 'badge ms-1 ' + cls;
        b.textContent = text;
        parent.appendChild(b);
    }

    function renderMessage(m) {
        document.getElementById('chatEmpty')?.remove();

        const mine = m.senderId === data.currentUserId;
        const row = document.createElement('div');
        row.className = 'chat-row' + (mine ? ' chat-row-mine' : '');

        const bubble = document.createElement('div');
        bubble.className = 'chat-bubble' + (m.isPrivate ? ' chat-private' : '');

        const head = document.createElement('div');
        head.className = 'chat-head';
        const name = document.createElement('strong');
        name.textContent = mine ? 'Ви' : m.senderName;
        head.appendChild(name);
        if (m.senderIsAdmin) addBadge(head, 'адміністратор', 'text-bg-danger');
        if (m.isPrivate) addBadge(head, mine ? 'особисто → ' + m.recipientName : 'особисто', 'text-bg-info');
        if (m.orderNumber) addBadge(head, 'Замовлення №' + m.orderNumber, 'text-bg-warning');
        bubble.appendChild(head);

        if (m.text) {
            const text = document.createElement('div');
            text.className = 'chat-text';
            text.textContent = m.text;          // textContent — захист від вставки HTML/скриптів
            bubble.appendChild(text);
        }

        if (m.attachmentUrl) {
            const link = document.createElement('a');
            link.href = m.attachmentUrl;
            link.target = '_blank';
            link.rel = 'noopener';
            if (m.isImage) {
                const img = document.createElement('img');
                img.src = m.attachmentUrl;
                img.alt = m.attachmentName || '';
                img.className = 'chat-image';
                link.appendChild(img);
            } else {
                link.className = 'chat-file';
                link.innerHTML = '<i class="bi bi-paperclip"></i> ';
                link.appendChild(document.createTextNode(m.attachmentName || 'файл'));
            }
            bubble.appendChild(link);
        }

        const time = document.createElement('div');
        time.className = 'chat-time';
        time.textContent = m.sentAt;
        bubble.appendChild(time);

        row.appendChild(bubble);
        list.appendChild(row);
    }

    function scrollDown() {
        list.scrollTop = list.scrollHeight;
    }

    // Історія з сервера
    if (data.messages.length === 0) {
        list.innerHTML = '<p id="chatEmpty" class="text-muted text-center my-4">Повідомлень ще немає. Напишіть першим!</p>';
    }
    data.messages.forEach(renderMessage);
    scrollDown();

    // ---------- Підключення до хаба ----------
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/chatHub')
        .withAutomaticReconnect()
        .build();

    // Сервер викликає цей метод, коли надходить нове повідомлення
    connection.on('ReceiveMessage', m => {
        renderMessage(m);
        scrollDown();
    });

    // Адміністратор очистив історію — очищаємо список в усіх відкритих вікнах
    connection.on('HistoryCleared', () => {
        list.innerHTML = '<p id="chatEmpty" class="text-muted text-center my-4">Історію чату очищено адміністратором.</p>';
    });

    function setStatus(text, cls, connected) {
        statusBadge.textContent = text;
        statusBadge.className = 'badge ' + cls;
        sendButton.disabled = !connected;
    }

    connection.onreconnecting(() => setStatus('Перепідключення…', 'text-bg-warning', false));
    connection.onreconnected(() => setStatus('Онлайн', 'text-bg-success', true));
    connection.onclose(() => setStatus('Відключено', 'text-bg-secondary', false));

    connection.start()
        .then(() => setStatus('Онлайн', 'text-bg-success', true))
        .catch(err => {
            setStatus('Помилка підключення', 'text-bg-danger', false);
            console.error(err);
        });

    // ---------- Надсилання ----------
    async function uploadFile() {
        const resp = await fetch(form.action, { method: 'POST', body: new FormData(form) });
        const result = await resp.json().catch(() => ({}));
        if (!resp.ok) throw new Error(result.error || 'Не вдалося завантажити файл');
        return result;
    }

    form.addEventListener('submit', async e => {
        e.preventDefault();
        errorBox.textContent = '';
        sendButton.disabled = true;

        try {
            let file = null;
            if (fileInput.files.length) {
                file = await uploadFile();      // 1) файл — звичайним HTTP-запитом
            }

            await connection.invoke('SendMessage', {   // 2) повідомлення — через SignalR
                text: textInput.value,
                recipientId: recipientSelect.value || null,
                orderNumber: orderInput.value || null,
                attachmentUrl: file ? file.url : null,
                attachmentName: file ? file.name : null
            });

            textInput.value = '';
            fileInput.value = '';
            textInput.focus();
        } catch (err) {
            // Помилки з хаба (HubException) показуємо користувачу
            errorBox.textContent = (err.message || String(err)).split('HubException: ').pop();
        } finally {
            sendButton.disabled = connection.state !== signalR.HubConnectionState.Connected;
        }
    });

    // Enter — надіслати, Shift+Enter — новий рядок
    textInput.addEventListener('keydown', e => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            form.requestSubmit();
        }
    });
})();