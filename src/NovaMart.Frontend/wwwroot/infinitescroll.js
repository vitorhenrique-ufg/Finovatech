// IntersectionObserver-based infinite scroll for NovaMart catalog
window.infiniteScroll = {
    observer: null,

    initialize: function (sentinelId, dotnetRef, methodName) {
        const sentinel = document.getElementById(sentinelId);
        if (!sentinel) return;

        if (this.observer) {
            this.observer.disconnect();
        }

        this.observer = new IntersectionObserver(
            (entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        dotnetRef.invokeMethodAsync(methodName);
                    }
                });
            },
            { rootMargin: '200px', threshold: 0.1 }
        );

        this.observer.observe(sentinel);
    },

    dispose: function () {
        if (this.observer) {
            this.observer.disconnect();
            this.observer = null;
        }
    }
};
