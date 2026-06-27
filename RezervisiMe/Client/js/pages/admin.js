$(function () {
    if (!authHelper.requireRole(['Administrator'])) return;

    var esc = authHelper.escapeHtml;

    var TYPES = [
        { value: 'Hotel', label: 'Hotel' },
        { value: 'Apartman', label: 'Apartman' },
        { value: 'Hostel', label: 'Hostel' },
        { value: 'Vila', label: 'Vila' },
        { value: 'Soba', label: 'Soba' },
        { value: 'Kuca', label: 'Kuća' },
        { value: 'Motel', label: 'Motel' }
    ];

    var GENDER_LABELS = { Muski: 'Muški', Zenski: 'Ženski', Drugo: 'Drugo' };
    var ROLE_LABELS = { Gost: 'Gost', Domacin: 'Domaćin', Administrator: 'Administrator' };
    var RES_LABELS = { KREIRANA: 'Kreirana', ODOBRENA: 'Odobrena', OTKAZANA: 'Otkazana', ZAVRSENA: 'Završena' };
    var REV_LABELS = { KREIRANA: 'Na čekanju', ODOBRENA: 'Odobrena', ODBIJENA: 'Odbijena' };

    var users = [];
    var accs = [];
    var accNames = {};
    var reservations = [];
    var reviews = [];
    var editingUser = null;
    var editingAcc = null;
    var loaded = { users: false, accs: false, reservations: false, reviews: false };

    init();

    function init() {
        var opts = '';
        TYPES.forEach(function (t) { opts += '<option value="' + t.value + '">' + t.label + '</option>'; });
        $('#acc-type').html(opts);

        bind();
        loadUsers();
    }

    function bind() {
        $('#admin-tabs').on('click', '.tab', function () {
            $('#admin-tabs .tab').removeClass('is-active');
            $(this).addClass('is-active');
            var section = $(this).data('section');
            $('.admin-section').addClass('is-hidden');
            $('#section-' + section).removeClass('is-hidden');
            if (section === 'users' && !loaded.users) loadUsers();
            if (section === 'accs' && !loaded.accs) loadAccs();
            if (section === 'reservations' && !loaded.reservations) loadReservations();
            if (section === 'reviews' && !loaded.reviews) loadReviews();
        });

        $(document).on('click', '[data-close]', function () {
            $('#' + $(this).data('close')).addClass('is-hidden');
        });
        $('.modal-backdrop').on('click', function (e) {
            if (e.target === this) $(this).addClass('is-hidden');
        });

        $('#users-filter').on('submit', function (e) {
            e.preventDefault();
            loadUsers();
        });
        $('#uf-sort').on('change', loadUsers);
        $('#af-avail, #af-sort').on('change', loadAccs);
        $('#rf-status').on('change', loadReservations);
        $('#vf-status').on('change', loadReviews);

        $('#add-host-btn').on('click', function () {
            $('#host-form')[0].reset();
            $('#host-alert').addClass('is-hidden');
            $('#host-modal').removeClass('is-hidden');
        });

        $('#users-body').on('click', '.js-edit-user', function () {
            var u = byId(users, $(this).data('id'));
            if (u) openUserModal(u);
        });

        $('#users-body').on('click', '.js-delete-user', function () {
            var u = byId(users, $(this).data('id'));
            if (!u) return;
            if (!confirm('Obrisati korisnika „' + u.userName + '“?')) return;
            api.users.remove(u.id).done(loadUsers).fail(pageErr);
        });

        $('#user-form').on('submit', function (e) {
            e.preventDefault();
            saveUser();
        });

        $('#host-form').on('submit', function (e) {
            e.preventDefault();
            saveHost();
        });

        $('#accs-body').on('click', '.js-edit-acc', function () {
            var a = byId(accs, $(this).data('id'));
            if (a) openAccModal(a);
        });

        $('#accs-body').on('click', '.js-delete-acc', function () {
            var a = byId(accs, $(this).data('id'));
            if (!a) return;
            if (!confirm('Obrisati smeštaj „' + a.name + '“?')) return;
            api.accommodations.remove(a.id).done(loadAccs).fail(pageErr);
        });

        $('#acc-form').on('submit', function (e) {
            e.preventDefault();
            saveAcc();
        });

        $('#reservations-body').on('click', '.js-approve-res', function () {
            api.reservations.approve($(this).data('id')).done(loadReservations).fail(pageErr);
        });
        $('#reservations-body').on('click', '.js-reject-res', function () {
            api.reservations.reject($(this).data('id')).done(loadReservations).fail(pageErr);
        });
        $('#reservations-body').on('click', '.js-cancel-res', function () {
            if (!confirm('Otkazati ovu rezervaciju?')) return;
            api.reservations.cancel($(this).data('id')).done(loadReservations).fail(pageErr);
        });

        $('#reviews-body').on('click', '.js-approve-rev', function () {
            api.reviews.approve($(this).data('id')).done(loadReviews).fail(pageErr);
        });
        $('#reviews-body').on('click', '.js-reject-rev', function () {
            api.reviews.reject($(this).data('id')).done(loadReviews).fail(pageErr);
        });
        $('#reviews-body').on('click', '.js-delete-rev', function () {
            if (!confirm('Obrisati ovu recenziju?')) return;
            api.reviews.remove($(this).data('id')).done(loadReviews).fail(pageErr);
        });
    }

    function byId(list, id) {
        for (var i = 0; i < list.length; i++)
            if (list[i].id === id) return list[i];
        return null;
    }

    function errMsg(xhr) {
        return (xhr.responseJSON && (xhr.responseJSON.error || xhr.responseJSON.message))
            || 'Došlo je do greške. Pokušaj ponovo.';
    }

    function pageErr(xhr) {
        var $a = $('#page-alert').text(errMsg(xhr)).removeClass('is-hidden');
        setTimeout(function () { $a.addClass('is-hidden'); }, 6000);
    }

    function emptyState(section, items, msg) {
        var $e = $('#' + section + '-empty');
        var $w = $('#section-' + section + ' .table-wrap');
        if (!items.length) {
            $e.text(msg).removeClass('is-hidden');
            $w.addClass('is-hidden');
        } else {
            $e.addClass('is-hidden');
            $w.removeClass('is-hidden');
        }
    }

    function loadUsers() {
        var sort = ($('#uf-sort').val() || 'name.asc').split('.');
        var q = { sortBy: sort[0], sortDir: sort[1] };
        var first = $.trim($('#uf-first').val());
        var last = $.trim($('#uf-last').val());
        var role = $('#uf-role').val();
        var from = $('#uf-dob-from').val();
        var to = $('#uf-dob-to').val();
        if (first) q.firstName = first;
        if (last) q.lastName = last;
        if (role) q.role = role;
        if (from) q.dobFrom = util.isoToApi(from);
        if (to) q.dobTo = util.isoToApi(to);

        api.users.search(q).done(function (items) {
            loaded.users = true;
            users = items || [];
            renderUsers();
        }).fail(pageErr);
    }

    function renderUsers() {
        var $b = $('#users-body').empty();
        emptyState('users', users, 'Nema korisnika za izabrane filtere.');
        users.forEach(function (u) {
            $b.append(
                '<tr>' +
                '<td><strong>' + esc(u.userName) + '</strong></td>' +
                '<td>' + esc(u.firstName) + '</td>' +
                '<td>' + esc(u.lastName) + '</td>' +
                '<td>' + esc(u.email) + '</td>' +
                '<td>' + util.apiToDisplay(u.dateOfBirth) + '</td>' +
                '<td>' + (GENDER_LABELS[u.gender] || u.gender) + '</td>' +
                '<td><span class="badge badge--type">' + (ROLE_LABELS[u.role] || u.role) + '</span></td>' +
                '<td><div class="table__actions">' +
                '<button type="button" class="btn btn--line btn--sm js-edit-user" data-id="' + u.id + '">Izmeni</button>' +
                '<button type="button" class="btn btn--red-line btn--sm js-delete-user" data-id="' + u.id + '">Obriši</button>' +
                '</div></td>' +
                '</tr>'
            );
        });
    }

    function openUserModal(u) {
        editingUser = u;
        $('#user-alert').addClass('is-hidden');
        $('#u-first').val(u.firstName);
        $('#u-last').val(u.lastName);
        $('#u-email').val(u.email);
        $('#u-dob').val(util.apiToIso(u.dateOfBirth));
        $('#u-gender').val(u.gender);
        $('#u-password').val('');
        $('#user-modal').removeClass('is-hidden');
    }

    function saveUser() {
        var alertBox = function (msg) { $('#user-alert').text(msg).removeClass('is-hidden'); };
        var dob = $('#u-dob').val();
        var data = {
            firstName: $.trim($('#u-first').val()),
            lastName: $.trim($('#u-last').val()),
            email: $.trim($('#u-email').val()),
            gender: $('#u-gender').val(),
            newPassword: $('#u-password').val() || null
        };
        if (!data.firstName || !data.lastName || !data.email) {
            alertBox('Popuni ime, prezime i email.');
            return;
        }
        if (!dob) {
            alertBox('Unesi datum rođenja.');
            return;
        }
        data.dateOfBirth = util.isoToApi(dob);

        $('#user-save').prop('disabled', true);
        api.users.update(editingUser.id, data)
            .done(function () {
                $('#user-modal').addClass('is-hidden');
                loadUsers();
            })
            .fail(function (xhr) { alertBox(errMsg(xhr)); })
            .always(function () { $('#user-save').prop('disabled', false); });
    }

    function saveHost() {
        var alertBox = function (msg) { $('#host-alert').text(msg).removeClass('is-hidden'); };
        var dob = $('#h-dob').val();
        var data = {
            firstName: $.trim($('#h-first').val()),
            lastName: $.trim($('#h-last').val()),
            username: $.trim($('#h-username').val()),
            email: $.trim($('#h-email').val()),
            gender: $('#h-gender').val(),
            password: $('#h-password').val()
        };
        if (!data.firstName || !data.lastName || !data.username || !data.email) {
            alertBox('Popuni sva polja.');
            return;
        }
        if (!dob) {
            alertBox('Unesi datum rođenja.');
            return;
        }
        if (!data.password || data.password.length < 6) {
            alertBox('Lozinka mora imati bar 6 karaktera.');
            return;
        }
        data.dateOfBirth = util.isoToApi(dob);

        $('#host-save').prop('disabled', true);
        api.users.createHost(data)
            .done(function () {
                $('#host-modal').addClass('is-hidden');
                loadUsers();
            })
            .fail(function (xhr) { alertBox(errMsg(xhr)); })
            .always(function () { $('#host-save').prop('disabled', false); });
    }

    function loadAccs() {
        var sort = ($('#af-sort').val() || 'name.asc').split('.');
        var q = { sortBy: sort[0], sortDir: sort[1] };
        var avail = $('#af-avail').val();
        if (avail) q.isAvailable = avail;
        api.accommodations.list(q).done(function (items) {
            loaded.accs = true;
            accs = items || [];
            accs.forEach(function (a) { accNames[a.id] = a.name; });
            renderAccs();
        }).fail(pageErr);
    }

    function typeLabel(t) {
        for (var i = 0; i < TYPES.length; i++)
            if (TYPES[i].value === t) return TYPES[i].label;
        return t;
    }

    function renderAccs() {
        var $b = $('#accs-body').empty();
        emptyState('accs', accs, 'Nema smeštaja.');
        accs.forEach(function (a) {
            $b.append(
                '<tr>' +
                '<td><a href="accommodation.html?id=' + a.id + '" style="color: var(--rs-blue); font-weight: 600;">' + esc(a.name) + '</a></td>' +
                '<td>' + esc(a.city) + '</td>' +
                '<td>' + typeLabel(a.type) + '</td>' +
                '<td>' + util.formatPrice(a.pricePerNight) + ' RSD</td>' +
                '<td>' + a.maxGuests + '</td>' +
                '<td>' + (a.averageRating != null ? a.averageRating.toFixed(1) + ' ★' : '—') + '</td>' +
                '<td>' + esc(a.hostUsername || '—') + '</td>' +
                '<td>' + (a.isAvailable
                    ? '<span class="badge badge--odobrena">Dostupan</span>'
                    : '<span class="badge badge--off">Nedostupan</span>') + '</td>' +
                '<td><div class="table__actions">' +
                '<button type="button" class="btn btn--line btn--sm js-edit-acc" data-id="' + a.id + '">Izmeni</button>' +
                '<button type="button" class="btn btn--red-line btn--sm js-delete-acc" data-id="' + a.id + '">Obriši</button>' +
                '</div></td>' +
                '</tr>'
            );
        });
    }

    function openAccModal(a) {
        editingAcc = a;
        $('#acc-form')[0].reset();
        $('#acc-alert').addClass('is-hidden');
        $('#acc-name').val(a.name);
        $('#acc-type').val(a.type);
        $('#acc-desc').val(a.description);
        $('#acc-city').val(a.city);
        $('#acc-address').val(a.address);
        $('#acc-price').val(a.pricePerNight);
        $('#acc-guests').val(a.maxGuests);
        $('#acc-available').prop('checked', a.isAvailable);
        $('#acc-modal').removeClass('is-hidden');
    }

    function saveAcc() {
        var alertBox = function (msg) { $('#acc-alert').text(msg).removeClass('is-hidden'); };
        var data = {
            name: $.trim($('#acc-name').val()),
            type: $('#acc-type').val(),
            description: $.trim($('#acc-desc').val()),
            city: $.trim($('#acc-city').val()),
            address: $.trim($('#acc-address').val()),
            pricePerNight: parseFloat($('#acc-price').val()),
            maxGuests: parseInt($('#acc-guests').val(), 10),
            isAvailable: $('#acc-available').is(':checked')
        };
        if (!data.name || !data.city || !data.address) {
            alertBox('Popuni naziv, grad i adresu.');
            return;
        }
        if (!(data.pricePerNight > 0) || !(data.maxGuests > 0)) {
            alertBox('Cena i broj gostiju moraju biti veći od 0.');
            return;
        }

        var $save = $('#acc-save').prop('disabled', true);
        var file = ($('#acc-image')[0].files || [])[0];

        var proceed = function (imagePath) {
            data.imagePath = imagePath || editingAcc.imagePath;
            api.accommodations.update(editingAcc.id, data)
                .done(function () {
                    $('#acc-modal').addClass('is-hidden');
                    loadAccs();
                })
                .fail(function (xhr) { alertBox(errMsg(xhr)); })
                .always(function () { $save.prop('disabled', false); });
        };

        if (file) {
            api.accommodations.uploadImage(file)
                .done(function (name) { proceed(name); })
                .fail(function (xhr) {
                    alertBox(errMsg(xhr));
                    $save.prop('disabled', false);
                });
        } else {
            proceed(null);
        }
    }

    function loadReservations() {
        var status = $('#rf-status').val();
        api.reservations.all(status ? { status: status } : {}).done(function (items) {
            loaded.reservations = true;
            reservations = items || [];
            renderReservations();
        }).fail(pageErr);
    }

    function renderReservations() {
        var $b = $('#reservations-body').empty();
        emptyState('reservations', reservations, 'Nema rezervacija za izabrani filter.');
        reservations.forEach(function (r) {
            var actions = '';
            if (r.status === 'KREIRANA') {
                actions +=
                    '<button type="button" class="btn btn--blue btn--sm js-approve-res" data-id="' + r.id + '">Odobri</button>' +
                    '<button type="button" class="btn btn--red-line btn--sm js-reject-res" data-id="' + r.id + '">Odbij</button>';
            }
            if (r.status === 'ODOBRENA') {
                actions += '<button type="button" class="btn btn--red-line btn--sm js-cancel-res" data-id="' + r.id + '">Otkaži</button>';
            }
            $b.append(
                '<tr>' +
                '<td><a href="accommodation.html?id=' + r.accommodationId + '" style="color: var(--rs-blue); font-weight: 600;">' +
                esc(r.accommodationName || '—') + '</a></td>' +
                '<td>' + esc(r.guestUsername || '—') + '</td>' +
                '<td>' + util.apiToDisplay(r.checkIn) + '</td>' +
                '<td>' + util.apiToDisplay(r.checkOut) + '</td>' +
                '<td>' + r.numberOfGuests + '</td>' +
                '<td><strong>' + util.formatPrice(r.totalPrice) + ' RSD</strong></td>' +
                '<td><span class="badge badge--' + r.status.toLowerCase() + '">' + (RES_LABELS[r.status] || r.status) + '</span></td>' +
                '<td><div class="table__actions">' + (actions || '<span style="color: var(--rs-muted);">—</span>') + '</div></td>' +
                '</tr>'
            );
        });
    }

    function loadReviews() {
        var status = $('#vf-status').val();
        var go = function () {
            api.reviews.all(status ? { status: status } : {}).done(function (items) {
                loaded.reviews = true;
                reviews = items || [];
                renderReviews();
            }).fail(pageErr);
        };
        if (!loaded.accs) {
            api.accommodations.list({}).done(function (items) {
                (items || []).forEach(function (a) { accNames[a.id] = a.name; });
                go();
            }).fail(go);
        } else {
            go();
        }
    }

    function renderReviews() {
        var $b = $('#reviews-body').empty();
        emptyState('reviews', reviews, 'Nema recenzija za izabrani filter.');
        reviews.forEach(function (r) {
            var actions = '';
            if (r.status !== 'ODOBRENA')
                actions += '<button type="button" class="btn btn--blue btn--sm js-approve-rev" data-id="' + r.id + '">Odobri</button>';
            if (r.status !== 'ODBIJENA')
                actions += '<button type="button" class="btn btn--red-line btn--sm js-reject-rev" data-id="' + r.id + '">Odbij</button>';
            actions += '<button type="button" class="btn btn--red-line btn--sm js-delete-rev" data-id="' + r.id + '">Obriši</button>';

            $b.append(
                '<tr>' +
                '<td>' + esc(r.accommodationName || accNames[r.accommodationId] || '—') + '</td>' +
                '<td>' + esc(r.reviewerUserName || '—') + '</td>' +
                '<td><span class="stars">' + util.stars(r.rating) + '</span></td>' +
                '<td><strong>' + esc(r.title) + '</strong></td>' +
                '<td><span class="cell-clip" title="' + esc(r.content) + '">' + esc(r.content) + '</span></td>' +
                '<td><span class="badge badge--' + r.status.toLowerCase() + '">' + (REV_LABELS[r.status] || r.status) + '</span></td>' +
                '<td><div class="table__actions">' + actions + '</div></td>' +
                '</tr>'
            );
        });
    }
});
