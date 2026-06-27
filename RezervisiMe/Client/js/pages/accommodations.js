$(function () {
    var TYPES = [
        { value: 'Hotel', label: 'Hotel' },
        { value: 'Apartman', label: 'Apartman' },
        { value: 'Hostel', label: 'Hostel' },
        { value: 'Vila', label: 'Vila' },
        { value: 'Soba', label: 'Soba' },
        { value: 'Kuca', label: 'Kuća' },
        { value: 'Motel', label: 'Motel' }
    ];

    var MINE = util.qs('mine') === '1';
    var user = api.getUser();
    if (MINE && (!user || user.role !== 'Domacin')) {
        location.href = 'accommodations.html';
        return;
    }

    var esc = authHelper.escapeHtml;
    var all = [];
    var editing = null;

    init();

    function init() {
        $('#city-input').val(util.qs('city') || '');
        $('#guests-input').val(util.qs('guests') || '');
        $('#check-in').val(util.qs('checkIn') || '');
        $('#check-out').val(util.qs('checkOut') || '');

        rangePicker.attach({
            field: '#dates-field',
            display: '#dates-display',
            checkIn: '#check-in',
            checkOut: '#check-out',
            anchor: '#search-band',
            onChange: function () { apply(); }
        });

        renderTypeOptions();

        if (MINE) {
            $('.listing').addClass('listing--mine');
            $('#filters-panel, #search-band').addClass('is-hidden');
            $('#results-title').text('Moji smeštaji');
            $('#add-acc-btn, #avail-filter').removeClass('is-hidden');
            document.title = 'Moji smeštaji — RezervisiMe';
        } else if (user && user.role === 'Domacin') {
            $('#add-acc-btn').removeClass('is-hidden');
        }

        bind();
        load();

        if (MINE && util.qs('new') === '1') openModal(null);
    }

    function bind() {
        $('#search-form').on('submit', function (e) {
            e.preventDefault();
            var params = {};
            $(this).serializeArray().forEach(function (kv) {
                if (kv.value) params[kv.name] = kv.value;
            });
            var q = $.param(params);
            history.replaceState(null, '', 'accommodations.html' + (q ? '?' + q : ''));
            load();
        });

        $('#sort-select, #avail-filter').on('change', load);

        var budgetTimer = null;
        $('#price-min, #price-max, #name-filter').on('input', function () {
            clearTimeout(budgetTimer);
            budgetTimer = setTimeout(load, 450);
        });

        $('#guests-input').on('change', apply);
        $(document).on('change', '#type-filters input, input[name=minRating]', apply);

        $('#add-acc-btn').on('click', function () { openModal(null); });
        $('[data-close=acc-modal]').on('click', closeModal);
        $('#acc-modal').on('click', function (e) { if (e.target === this) closeModal(); });
        $('#acc-form').on('submit', function (e) {
            e.preventDefault();
            save();
        });

        $('#results').on('click', '.js-edit', function () {
            var item = findById($(this).data('id'));
            if (item) openModal(item);
        });

        $('#results').on('click', '.js-delete', function () {
            var item = findById($(this).data('id'));
            if (!item) return;
            if (!confirm('Obrisati smeštaj „' + item.name + '“?')) return;
            api.accommodations.remove(item.id)
                .done(load)
                .fail(function (xhr) { pageAlert(errMsg(xhr)); });
        });
    }

    function findById(id) {
        for (var i = 0; i < all.length; i++)
            if (all[i].id === id) return all[i];
        return null;
    }

    function load() {
        var sort = ($('#sort-select').val() || 'postedAt.desc').split('.');
        var q = { sortBy: sort[0], sortDir: sort[1] };

        var done = function (items) {
            all = items || [];
            renderTypeCounts();
            apply();
        };
        var fail = function () {
            $('#results').html('<div class="empty-state">Trenutno ne možemo da učitamo smeštaje.</div>');
        };

        if (MINE) {
            var avail = $('#avail-filter').val();
            if (avail) q.isAvailable = avail;
            api.accommodations.mine(q).done(done).fail(fail);
            return;
        }

        var city = $.trim($('#city-input').val());
        if (city) q.city = city;
        var name = $.trim($('#name-filter').val());
        if (name) q.name = name;
        var min = parseFloat($('#price-min').val());
        var max = parseFloat($('#price-max').val());
        if (min > 0) q.priceMin = min;
        if (max > 0) q.priceMax = max;

        api.accommodations.list(q).done(done).fail(fail);
    }

    function apply() {
        var types = $('#type-filters input:checked').map(function () { return $(this).val(); }).get();
        var minRating = parseFloat($('input[name=minRating]:checked').val()) || 0;
        var guests = parseInt($('#guests-input').val(), 10) || 0;

        var items = all.filter(function (a) {
            if (types.length && types.indexOf(a.type) === -1) return false;
            if (minRating && (a.averageRating == null || a.averageRating < minRating)) return false;
            if (guests && a.maxGuests < guests) return false;
            return true;
        });

        renderResults(items);

        if (MINE) {
            $('#results-sub').text(countText(items.length));
        } else {
            var city = $.trim($('#city-input').val());
            var title = city
                ? city.charAt(0).toUpperCase() + city.slice(1) + ': ' + countText(items.length)
                : countText(items.length);
            $('#results-title').text(title);
        }
    }

    function renderResults(items) {
        var $r = $('#results');
        $r.empty();

        if (!items.length) {
            $r.html('<div class="empty-state">' +
                (MINE ? 'Još nemaš dodatih smeštaja.' : 'Nema rezultata za izabrane filtere.') +
                '</div>');
            return;
        }

        var ci = $('#check-in').val();
        var co = $('#check-out').val();
        var guests = parseInt($('#guests-input').val(), 10) || 0;
        var nights = util.nightsBetween(ci, co);

        items.forEach(function (a, i) {
            $r.append(MINE ? mineCard(a, i) : resultCard(a, i, nights, guests, ci, co));
        });
    }

    function detailHref(a, ci, co, guests) {
        var href = 'accommodation.html?id=' + a.id;
        if (ci && co) href += '&checkIn=' + ci + '&checkOut=' + co;
        if (guests) href += '&guests=' + guests;
        return href;
    }

    function imgStyle(a) {
        return a.imagePath
            ? ' style="background-image:url(\'/Content/uploads/' + encodeURIComponent(a.imagePath) + '\')"'
            : '';
    }

    function resultCard(a, i, nights, guests, ci, co) {
        var href = detailHref(a, ci, co, guests);

        var priceBlock;
        if (nights > 0) {
            priceBlock =
                '<div>' +
                '<div class="result__price-note">za ' + nights + ' ' + nightsWord(nights) +
                (guests ? ', ' + guests + ' ' + guestsWord(guests) : '') + '</div>' +
                '<div class="result__price">' + util.formatPrice(a.pricePerNight * nights) + ' RSD</div>' +
                '<div class="result__actions"><a class="btn btn--blue btn--sm" href="' + href + '">Pogledaj ponudu</a></div>' +
                '</div>';
        } else {
            priceBlock =
                '<div>' +
                '<div class="result__price-note">po noći</div>' +
                '<div class="result__price">' + util.formatPrice(a.pricePerNight) + ' RSD</div>' +
                '<div class="result__actions"><a class="btn btn--blue btn--sm" href="' + href + '">Pogledaj ponudu</a></div>' +
                '</div>';
        }

        return $(
            '<article class="result" style="animation-delay:' + (i * 50) + 'ms">' +
            '<a class="result__img" href="' + href + '"' + imgStyle(a) + '></a>' +
            '<div class="result__body">' +
            '<a class="result__title" href="' + href + '">' + esc(a.name) + '</a>' +
            '<div class="result__loc">' + esc(a.city) + ' · ' + esc(a.address) + '</div>' +
            '<div><span class="badge badge--type">' + typeLabel(a.type) + '</span>' +
            (a.isAvailable === false ? ' <span class="badge badge--off">Nedostupan</span>' : '') + '</div>' +
            '<p class="result__desc">' + esc(a.description || '') + '</p>' +
            '<div class="result__meta">Do ' + a.maxGuests + ' gostiju · Domaćin: ' + esc(a.hostUsername || '—') + '</div>' +
            '</div>' +
            '<div class="result__side">' + scoreHtml(a) + priceBlock + '</div>' +
            '</article>'
        );
    }

    function mineCard(a, i) {
        var href = detailHref(a, null, null, 0);
        return $(
            '<article class="result" style="animation-delay:' + (i * 50) + 'ms">' +
            '<a class="result__img" href="' + href + '"' + imgStyle(a) + '></a>' +
            '<div class="result__body">' +
            '<a class="result__title" href="' + href + '">' + esc(a.name) + '</a>' +
            '<div class="result__loc">' + esc(a.city) + ' · ' + esc(a.address) + '</div>' +
            '<div><span class="badge badge--type">' + typeLabel(a.type) + '</span> ' +
            (a.isAvailable
                ? '<span class="badge badge--odobrena">Dostupan</span>'
                : '<span class="badge badge--off">Nedostupan</span>') + '</div>' +
            '<p class="result__desc">' + esc(a.description || '') + '</p>' +
            '<div class="result__meta">Do ' + a.maxGuests + ' gostiju · ' +
            util.formatPrice(a.pricePerNight) + ' RSD / noć</div>' +
            '</div>' +
            '<div class="result__side">' +
            scoreHtml(a) +
            (a.isAvailable
                ? '<div class="result__actions">' +
                  '<button type="button" class="btn btn--line btn--sm js-edit" data-id="' + a.id + '">Izmeni</button>' +
                  '<button type="button" class="btn btn--red-line btn--sm js-delete" data-id="' + a.id + '">Obriši</button>' +
                  '</div>'
                : '<div class="result__price-note">Nedostupan objekat se ne može menjati ni brisati.</div>') +
            '</div>' +
            '</article>'
        );
    }

    function scoreHtml(a) {
        if (a.averageRating == null) {
            return '<div class="result__score"><div class="result__score-words"><small>Nema ocena</small></div></div>';
        }
        return '<div class="result__score">' +
            '<div class="result__score-words"><strong>' + scoreWord(a.averageRating) + '</strong>' +
            '<small>' + a.approvedReviewsCount + ' ' + reviewsWord(a.approvedReviewsCount) + '</small></div>' +
            '<span class="score-badge">' + a.averageRating.toFixed(1) + '</span>' +
            '</div>';
    }

    function scoreWord(r) {
        if (r >= 4.5) return 'Izuzetan';
        if (r >= 4) return 'Odličan';
        if (r >= 3.5) return 'Vrlo dobar';
        if (r >= 3) return 'Dobar';
        return 'Ocena';
    }

    function reviewsWord(n) {
        var m10 = n % 10, m100 = n % 100;
        if (m10 === 1 && m100 !== 11) return 'recenzija';
        if (m10 >= 2 && m10 <= 4 && !(m100 >= 12 && m100 <= 14)) return 'recenzije';
        return 'recenzija';
    }

    function nightsWord(n) {
        return n % 10 === 1 && n % 100 !== 11 ? 'noć' : 'noći';
    }

    function guestsWord(n) {
        return n % 10 === 1 && n % 100 !== 11 ? 'gost' : (n % 10 >= 2 && n % 10 <= 4 && !(n % 100 >= 12 && n % 100 <= 14) ? 'gosta' : 'gostiju');
    }

    function countText(n) {
        var m10 = n % 10, m100 = n % 100;
        if (m10 === 1 && m100 !== 11) return n + ' smeštaj pronađen';
        if (m10 >= 2 && m10 <= 4 && !(m100 >= 12 && m100 <= 14)) return n + ' smeštaja pronađena';
        return n + ' smeštaja pronađeno';
    }

    function typeLabel(t) {
        for (var i = 0; i < TYPES.length; i++)
            if (TYPES[i].value === t) return TYPES[i].label;
        return t;
    }

    function renderTypeOptions() {
        var checks = '';
        var opts = '';
        TYPES.forEach(function (t) {
            checks += '<label class="filters__check">' +
                '<input type="checkbox" value="' + t.value + '">' +
                '<span>' + t.label + '</span>' +
                '<span class="filters__count" data-type="' + t.value + '"></span>' +
                '</label>';
            opts += '<option value="' + t.value + '">' + t.label + '</option>';
        });
        $('#type-filters').html(checks);
        $('#acc-type').html(opts);
    }

    function renderTypeCounts() {
        var counts = {};
        all.forEach(function (a) { counts[a.type] = (counts[a.type] || 0) + 1; });
        $('.filters__count').each(function () {
            var c = counts[$(this).data('type')] || 0;
            $(this).text(c || '');
        });
    }

    function openModal(item) {
        editing = item;
        $('#acc-form')[0].reset();
        $('#acc-alert').addClass('is-hidden');
        $('#acc-modal-title').text(item ? 'Izmeni smeštaj' : 'Dodaj smeštaj');
        $('#acc-available-wrap').toggleClass('is-hidden', !item);
        if (item) {
            $('#acc-name').val(item.name);
            $('#acc-type').val(item.type);
            $('#acc-desc').val(item.description);
            $('#acc-city').val(item.city);
            $('#acc-address').val(item.address);
            $('#acc-price').val(item.pricePerNight);
            $('#acc-guests').val(item.maxGuests);
            $('#acc-available').prop('checked', item.isAvailable);
        }
        $('#acc-modal').removeClass('is-hidden');
    }

    function closeModal() {
        $('#acc-modal').addClass('is-hidden');
        editing = null;
    }

    function modalAlert(msg) {
        $('#acc-alert').text(msg).removeClass('is-hidden');
    }

    function pageAlert(msg) {
        var $a = $('#page-alert').text(msg).removeClass('is-hidden');
        setTimeout(function () { $a.addClass('is-hidden'); }, 5000);
    }

    function errMsg(xhr) {
        return (xhr.responseJSON && (xhr.responseJSON.error || xhr.responseJSON.message))
            || 'Došlo je do greške. Pokušaj ponovo.';
    }

    function save() {
        var data = {
            name: $.trim($('#acc-name').val()),
            type: $('#acc-type').val(),
            description: $.trim($('#acc-desc').val()),
            city: $.trim($('#acc-city').val()),
            address: $.trim($('#acc-address').val()),
            pricePerNight: parseFloat($('#acc-price').val()),
            maxGuests: parseInt($('#acc-guests').val(), 10)
        };

        if (!data.name || !data.city || !data.address) {
            modalAlert('Popuni naziv, grad i adresu.');
            return;
        }
        if (!(data.pricePerNight > 0)) {
            modalAlert('Cena po noći mora biti veća od 0.');
            return;
        }
        if (!(data.maxGuests > 0)) {
            modalAlert('Maksimalan broj gostiju mora biti veći od 0.');
            return;
        }

        var file = ($('#acc-image')[0].files || [])[0];
        if (!editing && !file) {
            modalAlert('Slika je obavezna pri kreiranju objekta.');
            return;
        }

        var $save = $('#acc-save').prop('disabled', true);

        var proceed = function (imagePath) {
            data.imagePath = imagePath || (editing ? editing.imagePath : null);
            var call;
            if (editing) {
                data.isAvailable = $('#acc-available').is(':checked');
                call = api.accommodations.update(editing.id, data);
            } else {
                call = api.accommodations.create(data);
            }
            call.done(function () {
                closeModal();
                load();
            }).fail(function (xhr) {
                modalAlert(errMsg(xhr));
            }).always(function () {
                $save.prop('disabled', false);
            });
        };

        if (file) {
            api.accommodations.uploadImage(file)
                .done(function (name) { proceed(name); })
                .fail(function (xhr) {
                    modalAlert(errMsg(xhr));
                    $save.prop('disabled', false);
                });
        } else {
            proceed(null);
        }
    }
});
