// ---- Popup dialogs: forms are loaded into a Bootstrap modal and submitted with AJAX ----
$(function () {
    const modalEl = document.getElementById('appModal');
    if (!modalEl) return;
    const modal = new bootstrap.Modal(modalEl);
    const $content = $(modalEl).find('.modal-content');

    function show(html) {
        $content.html(html);
        $.validator.unobtrusive.parse($content);
    }

    // A dialog action succeeded: real-time pages refresh themselves through SignalR,
    // every other page simply reloads.
    function done() {
        modal.hide();
        if (!document.getElementById('newsList')) location.reload();
    }
    window.appRefreshDone = done;

    $(document).on('click', '[data-modal-url]', function (e) {
        e.preventDefault();
        $.get($(this).data('modal-url'))
            .done(function (html) { show(html); modal.show(); })
            .fail(function () { alert('Unable to load the requested item.'); });
    });

    $(modalEl).on('submit', 'form[data-ajax-form]', function (e) {
        e.preventDefault();
        const $form = $(this);
        if ($form.valid && !$form.valid()) return;
        $.post($form.attr('action'), $form.serialize())
            .done(function (res) {
                if (res && res.success) {
                    done();
                } else {
                    show(res);
                }
            })
            .fail(function () { alert('The request failed. Please try again.'); });
    });
});

// ---- Real-time news (SignalR) ----
(function () {
    const list = document.getElementById('newsList');
    if (!list || !window.signalR) return;

    function toast(text) {
        const host = document.getElementById('toastHost');
        const el = document.createElement('div');
        el.className = 'toast align-items-center text-bg-dark border-0';
        el.setAttribute('role', 'alert');
        el.innerHTML = '<div class="d-flex"><div class="toast-body"></div>' +
            '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div>';
        el.querySelector('.toast-body').textContent = text;
        host.appendChild(el);
        const t = new bootstrap.Toast(el, { delay: 4000 });
        el.addEventListener('hidden.bs.toast', () => el.remove());
        t.show();
    }

    // Re-request the current page (filters included) and swap only the list.
    async function refresh() {
        try {
            const res = await fetch(location.href, { headers: { 'X-Requested-With': 'fetch' }, credentials: 'same-origin' });
            const doc = new DOMParser().parseFromString(await res.text(), 'text/html');
            const fresh = doc.getElementById('newsList');
            if (fresh) list.innerHTML = fresh.innerHTML;
        } catch (e) { /* keep the current list on network errors */ }
    }

    const verbs = { created: 'created', updated: 'updated', deleted: 'deleted' };
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/newsHub')
        .withAutomaticReconnect()
        .build();

    connection.on('NewsChanged', function (action, id, title, by) {
        toast('News "' + title + '" was ' + (verbs[action] || action) + (by ? ' by ' + by : '') + '.');
        refresh();
    });

    connection.onreconnected(refresh);
    connection.start().catch(function (err) { console.error('SignalR connection failed', err); });
})();
