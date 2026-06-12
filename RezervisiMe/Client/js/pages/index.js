$(function () {
    initMap();
    loadRecommended();
    initSearchSubmit();
});

var CITY_ZOOM = 9;

var CITIES = {
    'Beograd': [44.7866, 20.4489],
    'Novi Sad': [45.2671, 19.8335],
    'Niš': [43.3209, 21.8954],
    'Kragujevac': [44.0142, 20.9239],
    'Subotica': [46.1005, 19.6675],
    'Pančevo': [44.8704, 20.6402],
    'Čačak': [43.8914, 20.3497],
    'Novi Pazar': [43.1370, 20.5121],
    'Kraljevo': [43.7235, 20.6873],
    'Zrenjanin': [45.3781, 20.3893],
    'Sombor': [45.7741, 19.1124],
    'Užice': [43.8556, 19.8425],
    'Šabac': [44.7522, 19.6920],
    'Smederevo': [44.6620, 20.9295],
    'Leskovac': [42.9981, 21.9461],
    'Vranje': [42.5512, 21.8993],
    'Valjevo': [44.2670, 19.8896],
    'Kruševac': [43.5806, 21.3262],
    'Vrbas': [45.5715, 19.6406],
    'Pirot': [43.1530, 22.5882],
    'Zaječar': [43.9046, 22.2716],
    'Sremska Mitrovica': [44.9696, 19.6125],
    'Jagodina': [43.9777, 21.2614],
    'Zlatibor': [43.7283, 19.6967],
    'Kopaonik': [43.2840, 20.8205],
    'Tara': [43.9116, 19.4233],
    'Vrnjačka Banja': [43.6225, 20.9020],
    'Sokobanja': [43.6428, 21.8709],
    'Palić': [46.1006, 19.7642],
    'Đerdap': [44.6700, 22.0500],
    'Negotin': [44.2275, 22.5311],
    'Bor': [44.0747, 22.0950]
};

function stripDiacritics(s) {
    return s.toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .trim();
}

function findCity(input) {
    if (!input || !input.trim()) return null;
    var needle = stripDiacritics(input);
    var prefix = null, contains = null;
    for (var name in CITIES) {
        var normalized = stripDiacritics(name);
        if (normalized.indexOf(needle) === 0) { prefix = { name: name, coords: CITIES[name] }; break; }
        if (!contains && normalized.indexOf(needle) >= 0) contains = { name: name, coords: CITIES[name] };
    }
    return prefix || contains;
}

function injectSerbiaShape() {
    $('body').prepend(
        '<svg width="0" height="0" style="position:absolute" aria-hidden="true">' +
        '<defs><clipPath id="serbia-clip" clipPathUnits="objectBoundingBox">' +
        '<path d="' + SERBIA_SHAPE.clipD + '"/>' +
        '</clipPath></defs></svg>'
    );
    $('.hero__map').append(
        '<svg class="map-outline" viewBox="' + SERBIA_SHAPE.viewBox + '" aria-hidden="true">' +
        '<path d="' + SERBIA_SHAPE.outlineD + '" fill="none" stroke="#fff" ' +
        'stroke-width="2.5" stroke-linejoin="round" vector-effect="non-scaling-stroke"/>' +
        '</svg>'
    );
}

function initMap() {
    injectSerbiaShape();

    var serbiaBounds = L.latLngBounds(SERBIA_SHAPE.bounds);

    var map = L.map('serbia-map', {
        zoomControl: false,
        attributionControl: false,
        zoomSnap: 0,
        dragging: false,
        scrollWheelZoom: false,
        doubleClickZoom: false,
        boxZoom: false,
        touchZoom: false,
        keyboard: false,
        tap: false
    });

    map.fitBounds(serbiaBounds, { animate: false });

    L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}.png', {
        maxZoom: 11,
        minZoom: 6,
        subdomains: 'abcd'
    }).addTo(map);

    var pinIcon = L.divIcon({
        className: 'rs-pin-wrap',
        html: '<div class="rs-pin"><div class="rs-pin__shape"></div></div>',
        iconSize: [28, 28],
        iconAnchor: [14, 28]
    });

    var currentPin = null;
    var debounceTimer = null;
    var $hint = $('#map-hint');

    function showCity(coords) {
        if (currentPin) map.removeLayer(currentPin);
        currentPin = L.marker(coords, { icon: pinIcon }).addTo(map);
        map.flyTo(coords, CITY_ZOOM, { duration: 0.9 });
        $hint.addClass('is-hidden');
    }

    function reset() {
        if (currentPin) { map.removeLayer(currentPin); currentPin = null; }
        map.flyToBounds(serbiaBounds, { duration: 0.9 });
        $hint.removeClass('is-hidden');
    }

    $('#city-input').on('input', function () {
        var val = $(this).val();
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(function () {
            var match = findCity(val);
            if (match) showCity(match.coords);
            else reset();
        }, 220);
    });
}

function loadRecommended() {
    api.accommodations.list({
        sortBy: 'postedAt',
        sortDir: 'desc'
    }).done(function (items) {
        renderCards((items || []).slice(0, 8));
    }).fail(function () {
        $('#recommended-grid').html(
            '<div class="cards-grid__state">Trenutno ne možemo da učitamo preporuke.</div>'
        );
    });
}

function renderCards(items) {
    var $grid = $('#recommended-grid');
    $grid.empty();

    if (!items.length) {
        $grid.html('<div class="cards-grid__state">Još nema dodatih smeštaja.</div>');
        return;
    }

    var esc = authHelper.escapeHtml;

    items.forEach(function (a, i) {
        var img = a.imagePath ? '/Content/uploads/' + a.imagePath : '';
        var rating, ratingClass;
        if (a.averageRating !== null && a.averageRating !== undefined) {
            rating = a.averageRating.toFixed(1) + ' ★';
            ratingClass = '';
        } else {
            rating = 'Nema ocena';
            ratingClass = 'card__rating--empty';
        }

        var card = $(
            '<a class="card" href="accommodation.html?id=' + a.id + '" ' +
            'style="animation-delay: ' + (i * 60) + 'ms">' +
            '<div class="card__image"' +
            (img ? ' style="background-image: url(\'' + img + '\')"' : '') +
            '></div>' +
            '<div class="card__body">' +
            '<div class="card__title">' + esc(a.name) + '</div>' +
            '<div class="card__meta">' + esc(a.city) + ' · ' + esc(a.type) + '</div>' +
            '<div class="card__footer">' +
            '<div>' +
            '<span class="card__price">' + formatPrice(a.pricePerNight) + ' RSD</span>' +
            '<span class="card__price-unit"> / noć</span>' +
            '</div>' +
            '<div class="card__rating ' + ratingClass + '">' + rating + '</div>' +
            '</div>' +
            '</div>' +
            '</a>'
        );
        $grid.append(card);
    });
}

function formatPrice(n) {
    if (n == null) return '';
    return Math.round(n).toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
}

function initSearchSubmit() {
    $('#search-form').on('submit', function (e) {
        e.preventDefault();
        var params = {};
        $(this).serializeArray().forEach(function (kv) {
            if (kv.value) params[kv.name] = kv.value;
        });
        var qs = $.param(params);
        location.href = 'search.html' + (qs ? '?' + qs : '');
    });
}