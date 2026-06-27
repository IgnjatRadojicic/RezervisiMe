(function (global) {

    function isoToApi(iso) {
        if (!iso) return null;
        var p = iso.split('-');
        return p[2] + '/' + p[1] + '/' + p[0];
    }

    function apiToIso(api) {
        if (!api) return null;
        var p = api.split('/');
        return p[2] + '-' + p[1] + '-' + p[0];
    }

    function apiToDisplay(api) {
        if (!api) return '';
        var p = api.split('/');
        return p[0] + '.' + p[1] + '.' + p[2] + '.';
    }

    function formatPrice(n) {
        if (n == null) return '';
        return Math.round(n).toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    }

    function qs(name) {
        var m = new RegExp('[?&]' + name + '=([^&]*)').exec(location.search);
        return m ? decodeURIComponent(m[1].replace(/\+/g, ' ')) : null;
    }

    function nightsBetween(isoIn, isoOut) {
        if (!isoIn || !isoOut) return 0;
        var a = new Date(isoIn);
        var b = new Date(isoOut);
        var n = Math.round((b - a) / 86400000);
        return n > 0 ? n : 0;
    }

    function stars(r) {
        var full = Math.round(r || 0);
        var s = '';
        for (var i = 1; i <= 5; i++) s += i <= full ? '★' : '☆';
        return s;
    }

    global.util = {
        isoToApi: isoToApi,
        apiToIso: apiToIso,
        apiToDisplay: apiToDisplay,
        formatPrice: formatPrice,
        qs: qs,
        nightsBetween: nightsBetween,
        stars: stars
    };
})(window);
