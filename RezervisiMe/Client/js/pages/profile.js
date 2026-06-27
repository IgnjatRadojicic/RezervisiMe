$(function () {
    if (!authHelper.requireRole(null)) return;

    var user = api.getUser();
    var esc = authHelper.escapeHtml;
    var myReviews = [];
    var accNames = {};
    var editingReview = null;

    loadProfile();
    if (user.role === 'Gost') initMyReviews();

    function errMsg(xhr) {
        return (xhr.responseJSON && (xhr.responseJSON.error || xhr.responseJSON.message))
            || 'Došlo je do greške. Pokušaj ponovo.';
    }

    function profileAlert(msg, ok) {
        $('#profile-alert').text(msg)
            .toggleClass('page-alert--ok', !!ok)
            .removeClass('is-hidden');
    }

    function fillForm(u) {
        $('#p-username').val(u.userName);
        $('#p-role').val(authHelper.roleLabel(u.role));
        $('#p-first').val(u.firstName);
        $('#p-last').val(u.lastName);
        $('#p-email').val(u.email);
        $('#p-dob').val(util.apiToIso(u.dateOfBirth));
        $('#p-gender').val(u.gender);
        $('#p-password').val('');
        $('#profile-sub').text(u.userName + ' · ' + authHelper.roleLabel(u.role));
    }

    function loadProfile() {
        api.auth.me()
            .done(fillForm)
            .fail(function (xhr) { profileAlert(errMsg(xhr)); });

        $('#profile-form').on('submit', function (e) {
            e.preventDefault();
            saveProfile();
        });
    }

    function saveProfile() {
        $('#profile-alert').addClass('is-hidden');
        var dob = $('#p-dob').val();
        var data = {
            firstName: $.trim($('#p-first').val()),
            lastName: $.trim($('#p-last').val()),
            email: $.trim($('#p-email').val()),
            gender: $('#p-gender').val(),
            newPassword: $('#p-password').val() || null
        };

        if (!data.firstName || !data.lastName || !data.email) {
            profileAlert('Popuni ime, prezime i email.');
            return;
        }
        if (!dob) {
            profileAlert('Unesi datum rođenja.');
            return;
        }
        if (new Date(dob) > new Date()) {
            profileAlert('Datum rođenja ne može biti u budućnosti.');
            return;
        }
        if (data.newPassword && data.newPassword.length < 6) {
            profileAlert('Nova lozinka mora imati bar 6 karaktera.');
            return;
        }
        data.dateOfBirth = util.isoToApi(dob);

        $('#profile-save').prop('disabled', true);
        api.auth.updateMe(data)
            .done(function (updated) {
                api.setSession(api.getToken(), updated);
                fillForm(updated);
                authHelper.renderHeaderAuth();
                profileAlert('Izmene su sačuvane.', true);
            })
            .fail(function (xhr) { profileAlert(errMsg(xhr)); })
            .always(function () { $('#profile-save').prop('disabled', false); });
    }

    function initMyReviews() {
        $('#my-reviews').removeClass('is-hidden');

        $('#myrev-list').on('click', '.js-edit-rev', function () {
            var r = findReview($(this).data('id'));
            if (!r) return;
            editingReview = r;
            $('#rev-alert').addClass('is-hidden');
            $('#rev-rating').val(String(r.rating));
            $('#rev-title').val(r.title);
            $('#rev-content').val(r.content);
            $('#rev-modal').removeClass('is-hidden');
        });

        $('#myrev-list').on('click', '.js-delete-rev', function () {
            var r = findReview($(this).data('id'));
            if (!r) return;
            if (!confirm('Obrisati recenziju „' + r.title + '“?')) return;
            api.reviews.remove(r.id)
                .done(loadMyReviews)
                .fail(function (xhr) {
                    var $a = $('#myrev-alert').text(errMsg(xhr)).removeClass('is-hidden');
                    setTimeout(function () { $a.addClass('is-hidden'); }, 5000);
                });
        });

        $('[data-close=rev-modal]').on('click', function () {
            $('#rev-modal').addClass('is-hidden');
        });
        $('#rev-modal').on('click', function (e) {
            if (e.target === this) $(this).addClass('is-hidden');
        });

        $('#rev-form').on('submit', function (e) {
            e.preventDefault();
            saveReview();
        });

        api.accommodations.list({}).always(function (items) {
            ($.isArray(items) ? items : []).forEach(function (a) { accNames[a.id] = a.name; });
            loadMyReviews();
        });
    }

    function findReview(id) {
        for (var i = 0; i < myReviews.length; i++)
            if (myReviews[i].id === id) return myReviews[i];
        return null;
    }

    function loadMyReviews() {
        api.reviews.mine()
            .done(function (items) {
                myReviews = items || [];
                renderMyReviews();
            })
            .fail(function () {
                $('#myrev-list').html('<div class="empty-state">Ne možemo da učitamo recenzije.</div>');
            });
    }

    var REV_LABELS = { KREIRANA: 'Na čekanju', ODOBRENA: 'Odobrena', ODBIJENA: 'Odbijena' };

    function renderMyReviews() {
        var $l = $('#myrev-list').empty();
        $('#myrev-count').text('(' + myReviews.length + ')');

        if (!myReviews.length) {
            $l.html('<div class="empty-state">Još nemaš nijednu recenziju. Recenziju možeš da ostaviš za smeštaj u kom si boravio.</div>');
            return;
        }

        myReviews.forEach(function (r) {
            var accName = r.accommodationName || accNames[r.accommodationId] || 'Pogledaj smeštaj';
            $l.append(
                '<article class="myrev">' +
                '<div class="myrev__head">' +
                '<a class="myrev__acc" href="accommodation.html?id=' + r.accommodationId + '">' + esc(accName) + '</a>' +
                '<span class="badge badge--' + r.status.toLowerCase() + '">' + (REV_LABELS[r.status] || r.status) + '</span>' +
                '</div>' +
                '<span class="stars">' + util.stars(r.rating) + '</span>' +
                '<h4 class="myrev__title">' + esc(r.title) + '</h4>' +
                '<p class="myrev__content">' + esc(r.content) + '</p>' +
                '<div class="myrev__foot">' +
                '<button type="button" class="btn btn--line btn--sm js-edit-rev" data-id="' + r.id + '">Izmeni</button>' +
                '<button type="button" class="btn btn--red-line btn--sm js-delete-rev" data-id="' + r.id + '">Obriši</button>' +
                '</div>' +
                '</article>'
            );
        });
    }

    function saveReview() {
        var alertBox = function (msg) { $('#rev-alert').text(msg).removeClass('is-hidden'); };
        var title = $.trim($('#rev-title').val());
        var content = $.trim($('#rev-content').val());
        var rating = parseInt($('#rev-rating').val(), 10);

        if (!title || !content) {
            alertBox('Popuni naslov i sadržaj.');
            return;
        }

        $('#rev-save').prop('disabled', true);
        api.reviews.update(editingReview.id, { title: title, content: content, rating: rating })
            .done(function () {
                $('#rev-modal').addClass('is-hidden');
                loadMyReviews();
            })
            .fail(function (xhr) { alertBox(errMsg(xhr)); })
            .always(function () { $('#rev-save').prop('disabled', false); });
    }
});
