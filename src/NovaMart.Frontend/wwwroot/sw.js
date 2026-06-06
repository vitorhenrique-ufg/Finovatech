self.addEventListener('install', e => e.waitUntil(self.skipWaiting()));
self.addEventListener('activate', e => e.waitUntil(self.clients.claim()));

self.addEventListener('push', event => {
    if (!event.data) return;

    let data;
    try { data = event.data.json(); }
    catch { data = { titulo: 'NovaMart', corpo: event.data.text(), url: '/' }; }

    const notificacao = {
        titulo: data.titulo || 'NovaMart',
        corpo:  data.corpo  || '',
        url:    data.url    || '/',
        tempo:  new Date().toISOString(),
        lida:   false,
    };

    const options = {
        body:     notificacao.corpo,
        icon:     '/icon-192.png',
        badge:    '/icon-192.png',
        vibrate:  [200, 100, 200],
        tag:      'novamart-pagamento',
        renotify: true,
        data:     { url: notificacao.url },
    };

    event.waitUntil(Promise.all([
        self.registration.showNotification(notificacao.titulo, options),

        // Envia para todas as abas abertas para salvar no localStorage e atualizar badge
        self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then(clients =>
            clients.forEach(client =>
                client.postMessage({ type: 'NOVA_NOTIFICACAO', notificacao })
            )
        ),
    ]));
});

self.addEventListener('notificationclick', event => {
    event.notification.close();
    const url = event.notification.data?.url || '/';
    event.waitUntil(
        self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then(clients => {
            for (const client of clients) {
                if ('focus' in client) return client.focus();
            }
            return self.clients.openWindow(url);
        })
    );
});
