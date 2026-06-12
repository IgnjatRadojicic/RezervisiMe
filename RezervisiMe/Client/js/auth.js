(function (global) {

    function roleToDashboard(role) {
        if (role === 'Gost')          return 'guest.html';
        if (role === 'Domacin')       return 'host.html';
        if (role === 'Administrator') return 'admin.html';
        return 'index.html';
    }

    function renderHeaderAuth() {
        var $actions = $('#header-actions');
        if (!$actions.length) return;

        var user = api.getUser();
        if (!user) return;

        var dashboard = roleToDashboard(user.role);
        var initial = (user.firstName || user.username || '?').charAt(0).toUpperCase();

        $actions.html(
            '<div class="locale">RSD <span class="locale__flag">🇷🇸</span></div>' +
            '<a href="' + dashboard + '" class="user-pill">' +
                '<span class="user-pill__avatar">' + initial + '</span>' +
                '<span class="user-pill__name">' + escapeHtml(user.firstName || user.username) + '</span>' +
            '</a>' +
            '<button type="button" id="logout-btn" class="btn btn--outline">Odjavi se</button>'
        );

        $('#logout-btn').on('click', function () {
            api.auth.logout().always(function () {
                api.clearSession();
                location.href = 'index.html';
            });
        });
    }

    function requireRole(roles) {
        var user = api.getUser();
        if (!user) {
            location.href = 'login.html';
            return false;
        }
        if (roles && roles.length && roles.indexOf(user.role) === -1) {
            location.href = roleToDashboard(user.role);
            return false;
        }
        return true;
    }

    function escapeHtml(s) {
        return $('<div>').text(s == null ? '' : s).html();
    }

    global.authHelper = {
        renderHeaderAuth: renderHeaderAuth,
        requireRole: requireRole,
        escapeHtml: escapeHtml
    };

    $(renderHeaderAuth);
})(window);
