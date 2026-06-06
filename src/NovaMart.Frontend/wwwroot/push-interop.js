const VAPID_PUBLIC_KEY = 'BDkEYk83ZkiBjBIUzOSfi-mccntxv7aLLNNB25PacJnVF1e21lU8jyG3vvhki72VcQc_SUYLuDhXPDo_qabAr-g';

function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    const rawData = atob(base64);
    return Uint8Array.from([...rawData].map(c => c.charCodeAt(0)));
}

// Escuta mensagens do service worker (nova notificação recebida em background)
if ('serviceWorker' in navigator) {
    navigator.serviceWorker.addEventListener('message', event => {
        if (event.data?.type !== 'NOVA_NOTIFICACAO') return;

        const lista = JSON.parse(localStorage.getItem('nm_notificacoes') || '[]');
        lista.unshift(event.data.notificacao);
        if (lista.length > 30) lista.length = 30;
        localStorage.setItem('nm_notificacoes', JSON.stringify(lista));

        // Avisa o componente Blazor para atualizar o badge
        if (window._nmNotifDotNet) {
            window._nmNotifDotNet.invokeMethodAsync('OnNovaNotificacaoRecebida').catch(() => {});
        }
    });
}

window.PushInterop = {
    isSupported: () => 'serviceWorker' in navigator && 'PushManager' in window,

    getPermissionState: () => Notification.permission,

    async subscribe(vapidPublicKey) {
        const reg = await navigator.serviceWorker.ready;
        const sub = await reg.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: urlBase64ToUint8Array(vapidPublicKey),
        });
        const json = sub.toJSON();
        return { endpoint: json.endpoint, p256dh: json.keys.p256dh, auth: json.keys.auth };
    },

    async unsubscribe() {
        const reg = await navigator.serviceWorker.ready;
        const sub = await reg.pushManager.getSubscription();
        if (sub) await sub.unsubscribe();
    },

    async getCurrentSubscription() {
        const reg = await navigator.serviceWorker.ready;
        const sub = await reg.pushManager.getSubscription();
        if (!sub) return null;
        const json = sub.toJSON();
        return { endpoint: json.endpoint, p256dh: json.keys.p256dh, auth: json.keys.auth };
    },

    // ── Histórico de notificações (localStorage) ──────────────────────────────

    getNotificacoes() {
        return JSON.parse(localStorage.getItem('nm_notificacoes') || '[]');
    },

    marqueTodasLidas() {
        const lista = JSON.parse(localStorage.getItem('nm_notificacoes') || '[]');
        lista.forEach(n => n.lida = true);
        localStorage.setItem('nm_notificacoes', JSON.stringify(lista));
    },

    contarNaoLidas() {
        const lista = JSON.parse(localStorage.getItem('nm_notificacoes') || '[]');
        return lista.filter(n => !n.lida).length;
    },

    // ── Modal de permissão (mostrada uma vez por browser) ──────────────────────

    jaPerguntouPermissao() {
        return localStorage.getItem('nm_push_asked') === 'true';
    },

    marcarPermissaoPergunada() {
        localStorage.setItem('nm_push_asked', 'true');
    },

    // ── Callback Blazor para atualização em tempo real ────────────────────────

    registrarCallbackNotificacao(dotNetRef) {
        window._nmNotifDotNet = dotNetRef;
    },

    removerCallbackNotificacao() {
        window._nmNotifDotNet = null;
    },
};
