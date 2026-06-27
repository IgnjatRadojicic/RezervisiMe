$(function () {
    var TYPE_LABELS = {
        Hotel: 'Hotel', Apartman: 'Apartman', Hostel: 'Hostel',
        Vila: 'Vila', Soba: 'Soba', Kuca: 'Kuća', Motel: 'Motel'
    };

    var id = util.qs('id');
    if (!id) {
        location.href = 'accommodations.html';
        return;
    }

    var user = api.getUser();
    var esc = authHelper.escapeHtml;
    var acc = null;

    api.accommodations.get(id)
        .done(function (a) {
            acc = a;
            render();
            loadReviews();
        })
        .fail(function () {
            $('#detail-state').text('Smeštaj nije pronađen.');
        });

    function typeLabel(t) {
        return TYPE_LABELS[t] || t;
    }

    function errMsg(xhr) {
        return (xhr.responseJSON && (xhr.responseJSON.error || xhr.responseJSON.message))
            || 'Došlo je do greške. Pokušaj ponovo.';
    }

    function render() {
        document.title = acc.name + ' — RezervisiMe';
        $('#detail-state').addClass('is-hidden');
        $('#detail').removeClass('is-hidden');

        $('#acc-name').text(acc.name);
        $('#acc-type').text(typeLabel(acc.type));
        if (acc.isAvailable === false) $('#acc-unavailable').removeClass('is-hidden');
        $('#acc-loc').text(acc.address + ', ' + acc.city);
        if (acc.imagePath) {
            $('#acc-image').css('background-image',
                'url(/Content/uploads/' + encodeURIComponent(acc.imagePath) + ')');
        }
        $('#acc-facts').html(
            '<span>Do ' + acc.maxGuests + ' gostiju</span>' +
            '<span>·</span>' +
            '<span>' + typeLabel(acc.type) + '</span>' +
            '<span>·</span>' +
            '<span>Domaćin: ' + esc(acc.hostUsername || '—') + '</span>'
        );
        $('#acc-desc').text(acc.description || 'Nema opisa.');
        $('#acc-price').text(util.formatPrice(acc.pricePerNight));

        if (acc.averageRating != null) {
            $('#acc-score').html(
                '<span class="score-badge">' + acc.averageRating.toFixed(1) + '</span>' +
                '<span>' + acc.approvedReviewsCount + ' rec.</span>'
            );
        } else {
            $('#acc-score').html('<span>Nema ocena</span>');
        }

        initBooking();
    }

    function showNote(html, ok) {
        $('#book-form').addClass('is-hidden');
        $('#book-note').html(html).removeClass('is-hidden')
            .toggleClass('book-card__note--ok', !!ok);
    }

    function initBooking() {
        if (!user) {
            showNote('<a href="login.html">Prijavi se</a> ili <a href="register.html">napravi nalog</a> da bi rezervisao ovaj smeštaj.');
            return;
        }
        if (user.role !== 'Gost') {
            showNote('Samo gosti mogu da prave rezervacije.');
            return;
        }
        if (acc.isAvailable === false) {
            showNote('Ovaj smeštaj trenutno nije dostupan za rezervacije.');
            return;
        }

        $('#book-check-in').val(util.qs('checkIn') || '');
        $('#book-check-out').val(util.qs('checkOut') || '');
        var qsGuests = parseInt(util.qs('guests'), 10);
        if (qsGuests > 0) $('#book-guests').val(Math.min(qsGuests, acc.maxGuests));
        $('#book-guests').attr('max', acc.maxGuests);

        rangePicker.attach({
            field: '#book-dates-field',
            display: '#book-dates-display',
            checkIn: '#book-check-in',
            checkOut: '#book-check-out',
            anchor: '#detail',
            onChange: updateSummary
        });

        $('#book-guests').on('input', updateSummary);
        updateSummary();

        $('#book-form').on('submit', function (e) {
            e.preventDefault();
            submitBooking();
        });
    }

    function updateSummary() {
        var nights = util.nightsBetween($('#book-check-in').val(), $('#book-check-out').val());
        var $s = $('#book-summary');
        if (!nights) {
            $s.addClass('is-hidden');
            return;
        }
        var total = nights * acc.pricePerNight;
        $s.html(
            nights + ' ' + (nights % 10 === 1 && nights % 100 !== 11 ? 'noć' : 'noći') +
            ' × ' + util.formatPrice(acc.pricePerNight) + ' RSD = <strong>' +
            util.formatPrice(total) + ' RSD</strong>'
        ).removeClass('is-hidden');
    }

    function bookAlert(msg) {
        $('#book-alert').text(msg).removeClass('is-hidden');
    }

    function submitBooking() {
        $('#book-alert').addClass('is-hidden');
        var ci = $('#book-check-in').val();
        var co = $('#book-check-out').val();
        var guests = parseInt($('#book-guests').val(), 10);

        if (!ci || !co) {
            bookAlert('Izaberi datume prijave i odjave.');
            return;
        }
        if (!(guests > 0)) {
            bookAlert('Unesi broj gostiju.');
            return;
        }
        if (guests > acc.maxGuests) {
            bookAlert('Maksimalan broj gostiju je ' + acc.maxGuests + '.');
            return;
        }

        $('#book-submit').prop('disabled', true).text('Slanje…');

        api.reservations.create({
            accommodationId: acc.id,
            checkIn: util.isoToApi(ci),
            checkOut: util.isoToApi(co),
            numberOfGuests: guests
        }).done(function () {
            showNote('Rezervacija je kreirana i čeka odobrenje. <a href="reservations.html">Pogledaj svoje rezervacije</a>.', true);
        }).fail(function (xhr) {
            bookAlert(errMsg(xhr));
            $('#book-submit').prop('disabled', false).text('Rezerviši');
        });
    }

    function loadReviews() {
        api.reviews.forAccommodation(acc.id)
            .done(function (list) {
                renderReviews(list || []);
            })
            .fail(function () {
                $('#reviews-list').html('<div class="empty-state">Ne možemo da učitamo recenzije.</div>');
            });

        if (user && user.role === 'Gost') {
            $('#review-form').removeClass('is-hidden');
            $('#review-form').on('submit', function (e) {
                e.preventDefault();
                submitReview();
            });
        }
    }

    function renderReviews(list) {
        $('#rev-count').text('(' + list.length + ')');
        var $l = $('#reviews-list');
        $l.empty();

        if (!list.length) {
            $l.html('<div class="empty-state">Još nema recenzija za ovaj smeštaj.</div>');
            return;
        }

        list.forEach(function (r) {
            var initial = (r.reviewerUserName || '?').charAt(0).toUpperCase();
            var badge = r.status && r.status !== 'ODOBRENA'
                ? ' <span class="badge badge--' + r.status.toLowerCase() + '">' + r.status + '</span>'
                : '';
            $l.append(
                '<article class="review">' +
                '<div class="review__head">' +
                '<span class="review__avatar">' + esc(initial) + '</span>' +
                '<div class="review__user"><strong>' + esc(r.reviewerUserName || 'Nepoznat') + badge + '</strong>' +
                '<span class="stars">' + util.stars(r.rating) + '</span></div>' +
                '</div>' +
                '<h4 class="review__title">' + esc(r.title) + '</h4>' +
                '<p class="review__content">' + esc(r.content) + '</p>' +
                (r.imagePath
                    ? '<img class="review__image" src="/Content/uploads/' + encodeURIComponent(r.imagePath) + '" alt="">'
                    : '') +
                '</article>'
            );
        });
    }

    function reviewAlert(msg, ok) {
        $('#review-alert').text(msg)
            .toggleClass('page-alert--ok', !!ok)
            .removeClass('is-hidden');
    }

    function submitReview() {
        $('#review-alert').addClass('is-hidden');
        var title = $.trim($('#review-title').val());
        var content = $.trim($('#review-content').val());
        var rating = parseInt($('#review-rating').val(), 10);

        if (!title || !content) {
            reviewAlert('Popuni naslov i sadržaj recenzije.');
            return;
        }

        $('#review-submit').prop('disabled', true);

        api.reviews.create({
            accommodationId: acc.id,
            title: title,
            content: content,
            rating: rating
        }).done(function () {
            $('#review-form')[0].reset();
            reviewAlert('Recenzija je poslata i biće vidljiva kada je administrator odobri.', true);
        }).fail(function (xhr) {
            reviewAlert(errMsg(xhr));
        }).always(function () {
            $('#review-submit').prop('disabled', false);
        });
    }
});
