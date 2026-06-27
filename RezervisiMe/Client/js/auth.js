(function (global) {

    var ICONS = {
        user: '<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>',
        calendar: '<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>',
        home: '<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>',
        shield: '<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>',
        exit: '<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>'
    };

    function roleToDashboard(role) {
        if (role === 'Gost')          return 'accommodations.html';
        if (role === 'Domacin')       return 'accommodations.html?mine=1';
        if (role === 'Administrator') return 'admin.html';
        return 'index.html';
    }

    function roleLabel(role) {
        if (role === 'Domacin') return 'Domaćin';
        return role || '';
    }

    function escapeHtml(s) {
        return $('<div>').text(s == null ? '' : s).html();
    }

    function fullName(user) {
        var name = ((user.firstName || '') + ' ' + (user.lastName || '')).trim();
        return name || user.userName || user.username || '';
    }

    function renderHeaderAuth() {
        var $actions = $('#header-actions');
        if (!$actions.length) return;

        var user = api.getUser();
        if (!user) return;

        var initial = (user.firstName || user.userName || user.username || '?').charAt(0).toUpperCase();

        var items = '<a class="user-menu__item" href="profile.html">' + ICONS.user + '<span>Moj nalog</span></a>';
        if (user.role === 'Gost' || user.role === 'Administrator')
            items += '<a class="user-menu__item" href="reservations.html">' + ICONS.calendar + '<span>Rezervacije</span></a>';
        if (user.role === 'Domacin')
            items += '<a class="user-menu__item" href="accommodations.html?mine=1">' + ICONS.home + '<span>Moji smeštaji</span></a>';
        if (user.role === 'Administrator')
            items += '<a class="user-menu__item" href="admin.html">' + ICONS.shield + '<span>Admin panel</span></a>';

        $actions.html(
            '<div class="locale">RSD <span class="locale__flag">🇷🇸</span></div>' +
            (user.role === 'Domacin'
                ? '<a href="accommodations.html?mine=1&new=1" class="btn btn--ghost">Dodaj smeštaj</a>'
                : '') +
            '<div class="user-menu" id="user-menu">' +
                '<button type="button" class="user-menu__trigger" id="user-menu-trigger">' +
                    '<span class="user-menu__avatar">' + escapeHtml(initial) + '</span>' +
                    '<span class="user-menu__info">' +
                        '<strong>' + escapeHtml(fullName(user)) + '</strong>' +
                        '<small>' + escapeHtml(roleLabel(user.role)) + '</small>' +
                    '</span>' +
                '</button>' +
                '<div class="user-menu__panel is-hidden" id="user-menu-panel">' +
                    items +
                    (items ? '<div class="user-menu__sep"></div>' : '') +
                    '<button type="button" id="logout-btn" class="user-menu__item">' + ICONS.exit + '<span>Odjavi se</span></button>' +
                '</div>' +
            '</div>'
        );

        $('#user-menu-trigger').on('click', function (e) {
            e.stopPropagation();
            $('#user-menu-panel').toggleClass('is-hidden');
        });

        $(document).on('click', function (e) {
            if (!$(e.target).closest('#user-menu').length)
                $('#user-menu-panel').addClass('is-hidden');
        });

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

    global.authHelper = {
        renderHeaderAuth: renderHeaderAuth,
        requireRole: requireRole,
        escapeHtml: escapeHtml,
        roleToDashboard: roleToDashboard,
        roleLabel: roleLabel
    };

    $(renderHeaderAuth);
})(window);
