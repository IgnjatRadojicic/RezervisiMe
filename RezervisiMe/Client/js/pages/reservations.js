$(function () {
    if (!authHelper.requireRole(['Gost', 'Administrator'])) return;

    var user = api.getUser();
    var isAdmin = user.role === 'Administrator';
    var status = '';
    var current = [];
    var reviewTarget = null;
    var esc = authHelper.escapeHtml;

    var STATUS_LABELS = {
        KREIRANA: 'Kreirana',
        ODOBRENA: 'Odobrena',
        OTKAZANA: 'Otkazana',
        ZAVRSENA: 'Završena'
    };

    renderHead();
    bind();
    load();

    function renderHead() {
        var cols = '<th>Smeštaj</th>';
        if (isAdmin) cols += '<th>Gost</th>';
        cols += '<th>Prijava</th><th>Odjava</th><th>Gostiju</th><th>Ukupno</th><th>Status</th><th>Akcije</th>';
        $('#table-head').html(cols);
    }

    function bind() {
        $('#status-tabs').on('click', '.tab', function () {
            $('#status-tabs .tab').removeClass('is-active');
            $(this).addClass('is-active');
            status = $(this).data('status');
            load();
        });

        $('#table-body').on('click', '.js-cancel', function () {
            var r = findById($(this).data('id'));
            if (!r) return;
            if (!confirm('Otkazati rezervaciju za „' + (r.accommodationName || '') + '“?')) return;
            api.reservations.cancel(r.id).done(load).fail(showErr);
        });

        $('#table-body').on('click', '.js-approve', function () {
            api.reservations.approve($(this).data('id')).done(load).fail(showErr);
        });

        $('#table-body').on('click', '.js-reject', function () {
            api.reservations.reject($(this).data('id')).done(load).fail(showErr);
        });

        $('#table-body').on('click', '.js-review', function () {
            reviewTarget = findById($(this).data('id'));
            if (!reviewTarget) return;
            $('#review-form')[0].reset();
            $('#review-alert').addClass('is-hidden').removeClass('page-alert--ok');
            $('#review-modal').removeClass('is-hidden');
        });

        $('[data-close=review-modal]').on('click', function () {
            $('#review-modal').addClass('is-hidden');
        });
        $('#review-modal').on('click', function (e) {
            if (e.target === this) $(this).addClass('is-hidden');
        });

        $('#review-form').on('submit', function (e) {
            e.preventDefault();
            submitReview();
        });
    }

    function findById(id) {
        for (var i = 0; i < current.length; i++)
            if (current[i].id === id) return current[i];
        return null;
    }

    function showErr(xhr) {
        var msg = (xhr.responseJSON && (xhr.responseJSON.error || xhr.responseJSON.message))
            || 'Došlo je do greške. Pokušaj ponovo.';
        var $a = $('#page-alert').text(msg).removeClass('is-hidden');
        setTimeout(function () { $a.addClass('is-hidden'); }, 6000);
    }

    function load() {
        var call = isAdmin
            ? api.reservations.all(status ? { status: status } : {})
            : api.reservations.mine(status || null);

        call.done(function (items) {
            current = items || [];
            render();
        }).fail(function () {
            $('#table-body').empty();
            $('#empty').text('Ne možemo da učitamo rezervacije.').removeClass('is-hidden');
        });
    }

    function countText(n) {
        var m10 = n % 10, m100 = n % 100;
        if (m10 === 1 && m100 !== 11) return n + ' rezervacija';
        if (m10 >= 2 && m10 <= 4 && !(m100 >= 12 && m100 <= 14)) return n + ' rezervacije';
        return n + ' rezervacija';
    }

    function actionsFor(r) {
        var html = '';
        if (isAdmin) {
            if (r.status === 'KREIRANA') {
                html += '<button type="button" class="btn btn--blue btn--sm js-approve" data-id="' + r.id + '">Odobri</button>';
                html += '<button type="button" class="btn btn--red-line btn--sm js-reject" data-id="' + r.id + '">Odbij</button>';
            }
            if (r.status === 'ODOBRENA') {
                html += '<button type="button" class="btn btn--red-line btn--sm js-cancel" data-id="' + r.id + '">Otkaži</button>';
            }
        } else {
            if (r.status === 'KREIRANA' || r.status === 'ODOBRENA') {
                html += '<button type="button" class="btn btn--red-line btn--sm js-cancel" data-id="' + r.id + '">Otkaži</button>';
            }
            if (r.status === 'ZAVRSENA') {
                html += '<button type="button" class="btn btn--line btn--sm js-review" data-id="' + r.id + '">Oceni</button>';
            }
        }
        return html || '<span style="color: var(--rs-muted);">—</span>';
    }

    function render() {
        var $b = $('#table-body');
        $b.empty();
        $('#results-sub').text(countText(current.length));

        if (!current.length) {
            $('#empty').text('Nema rezervacija za izabrani filter.').removeClass('is-hidden');
            $('.table-wrap').addClass('is-hidden');
            return;
        }

        $('#empty').addClass('is-hidden');
        $('.table-wrap').removeClass('is-hidden');

        current.forEach(function (r) {
            var row = '<tr>' +
                '<td><a href="accommodation.html?id=' + r.accommodationId + '" style="color: var(--rs-blue); font-weight: 600;">' +
                esc(r.accommodationName || '—') + '</a></td>';
            if (isAdmin) row += '<td>' + esc(r.guestUsername || '—') + '</td>';
            row +=
                '<td>' + util.apiToDisplay(r.checkIn) + '</td>' +
                '<td>' + util.apiToDisplay(r.checkOut) + '</td>' +
                '<td>' + r.numberOfGuests + '</td>' +
                '<td><strong>' + util.formatPrice(r.totalPrice) + ' RSD</strong></td>' +
                '<td><span class="badge badge--' + r.status.toLowerCase() + '">' + (STATUS_LABELS[r.status] || r.status) + '</span></td>' +
                '<td><div class="table__actions">' + actionsFor(r) + '</div></td>' +
                '</tr>';
            $b.append(row);
        });
    }

    function submitReview() {
        var title = $.trim($('#review-title').val());
        var content = $.trim($('#review-content').val());
        var rating = parseInt($('#review-rating').val(), 10);

        var alert = function (msg, ok) {
            $('#review-alert').text(msg)
                .toggleClass('page-alert--ok', !!ok)
                .removeClass('is-hidden');
        };

        if (!title || !content) {
            alert('Popuni naslov i sadržaj recenzije.');
            return;
        }

        $('#review-submit').prop('disabled', true);

        api.reviews.create({
            accommodationId: reviewTarget.accommodationId,
            title: title,
            content: content,
            rating: rating
        }).done(function () {
            alert('Recenzija je poslata i čeka odobrenje administratora.', true);
            setTimeout(function () { $('#review-modal').addClass('is-hidden'); }, 1400);
        }).fail(function (xhr) {
            alert((xhr.responseJSON && (xhr.responseJSON.error || xhr.responseJSON.message)) || 'Slanje nije uspelo.');
        }).always(function () {
            $('#review-submit').prop('disabled', false);
        });
    }
});
