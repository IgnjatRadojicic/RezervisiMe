(function (global) {
    var MONTHS = ['Januar', 'Februar', 'Mart', 'April', 'Maj', 'Jun', 'Jul', 'Avgust', 'Septembar', 'Oktobar', 'Novembar', 'Decembar'];
    var MONTHS_SHORT = ['jan', 'feb', 'mar', 'apr', 'maj', 'jun', 'jul', 'avg', 'sep', 'okt', 'nov', 'dec'];
    var WEEKDAYS = ['Po', 'Ut', 'Sr', 'Če', 'Pe', 'Su', 'Ne'];
    var DAY_SHORT = ['ned', 'pon', 'uto', 'sre', 'čet', 'pet', 'sub'];

    function pad(n) { return n < 10 ? '0' + n : '' + n; }

    function toIso(d) {
        return d.getFullYear() + '-' + pad(d.getMonth() + 1) + '-' + pad(d.getDate());
    }

    function fromIso(s) {
        if (!s) return null;
        var p = s.split('-');
        if (p.length !== 3) return null;
        return new Date(+p[0], +p[1] - 1, +p[2]);
    }

    function label(d) {
        return DAY_SHORT[d.getDay()] + ', ' + d.getDate() + '. ' + MONTHS_SHORT[d.getMonth()];
    }

    function attach(opts) {
        var $field = $(opts.field);
        var $display = $(opts.display);
        var $inputIn = $(opts.checkIn);
        var $inputOut = $(opts.checkOut);
        var $anchor = $(opts.anchor);
        var onChange = opts.onChange || function () { };

        var now = new Date();
        var today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
        var start = fromIso($inputIn.val());
        var end = fromIso($inputOut.val());
        var base = start || today;
        var view = new Date(base.getFullYear(), base.getMonth(), 1);
        var hover = null;
        var open = false;

        $anchor.addClass('rp-anchor');
        var $panel = $('<div class="rp is-hidden"></div>').appendTo($anchor);

        function renderMonth(year, month) {
            var first = new Date(year, month, 1);
            var lead = (first.getDay() + 6) % 7;
            var daysIn = new Date(year, month + 1, 0).getDate();
            var html = '<div class="rp__month"><div class="rp__month-name">' + MONTHS[month] + ' ' + year + '</div><div class="rp__grid">';
            for (var w = 0; w < 7; w++) html += '<span class="rp__wd">' + WEEKDAYS[w] + '</span>';
            for (var i = 0; i < lead; i++) html += '<span class="rp__day is-empty"></span>';
            var rangeEnd = end || hover;
            for (var d = 1; d <= daysIn; d++) {
                var date = new Date(year, month, d);
                var cls = 'rp__day';
                if (date < today) cls += ' is-disabled';
                if (start && date.getTime() === start.getTime()) cls += ' is-edge is-start';
                if (end && date.getTime() === end.getTime()) cls += ' is-edge is-end';
                if (start && rangeEnd && date > start && date < rangeEnd) cls += ' is-between';
                html += '<button type="button" class="' + cls + '" data-date="' + toIso(date) + '">' + d + '</button>';
            }
            html += '</div></div>';
            return html;
        }

        function render() {
            var y2 = view.getMonth() === 11 ? view.getFullYear() + 1 : view.getFullYear();
            var m2 = (view.getMonth() + 1) % 12;
            var prevDisabled = view.getFullYear() === today.getFullYear() && view.getMonth() === today.getMonth();
            $panel.html(
                '<button type="button" class="rp__nav rp__nav--prev"' + (prevDisabled ? ' disabled' : '') + '>‹</button>' +
                '<button type="button" class="rp__nav rp__nav--next">›</button>' +
                renderMonth(view.getFullYear(), view.getMonth()) +
                renderMonth(y2, m2)
            );
        }

        function updateDisplay() {
            if (start && end) $display.val(label(start) + ' — ' + label(end));
            else if (start) $display.val(label(start) + ' — ?');
            else $display.val('');
        }

        function sync() {
            $inputIn.val(start ? toIso(start) : '');
            $inputOut.val(end ? toIso(end) : '');
            updateDisplay();
            onChange(start ? toIso(start) : null, end ? toIso(end) : null);
        }

        function position() {
            var fr = $field[0].getBoundingClientRect();
            var ar = $anchor[0].getBoundingClientRect();
            var left = fr.left - ar.left;
            var max = $anchor.outerWidth() - $panel.outerWidth() - 8;
            if (max < 0) max = 0;
            if (left > max) left = max;
            if (left < 0) left = 0;
            $panel.css({ top: fr.bottom - ar.top + 10, left: left });
        }

        function show() {
            if (open) return;
            open = true;
            render();
            $panel.removeClass('is-hidden');
            position();
        }

        function hide() {
            open = false;
            hover = null;
            $panel.addClass('is-hidden');
        }

        $field.on('click', function (e) {
            e.stopPropagation();
            show();
        });

        $panel.on('click', '.rp__nav--prev', function () {
            view = new Date(view.getFullYear(), view.getMonth() - 1, 1);
            render();
        });

        $panel.on('click', '.rp__nav--next', function () {
            view = new Date(view.getFullYear(), view.getMonth() + 1, 1);
            render();
        });

        $panel.on('click', '.rp__day', function () {
            var $d = $(this);
            if ($d.hasClass('is-disabled') || $d.hasClass('is-empty')) return;
            var date = fromIso($d.attr('data-date'));
            if (!start || (start && end)) { start = date; end = null; }
            else if (date.getTime() <= start.getTime()) { start = date; end = null; }
            else { end = date; }
            sync();
            render();
            if (start && end) setTimeout(hide, 160);
        });

        function paintRange() {
            var rangeEnd = end || hover;
            $panel.find('.rp__day').not('.is-empty').each(function () {
                var $d = $(this);
                var t = fromIso($d.attr('data-date')).getTime();
                $d.toggleClass('is-between',
                    !!(start && rangeEnd && t > start.getTime() && t < rangeEnd.getTime()));
            });
        }

        $panel.on('mouseenter', '.rp__day', function () {
            var $d = $(this);
            if ($d.hasClass('is-disabled') || $d.hasClass('is-empty')) return;
            if (start && !end) {
                hover = fromIso($d.attr('data-date'));
                paintRange();
            }
        });

        $panel.on('click mousedown', function (e) { e.stopPropagation(); });

        $(document).on('click', function () { if (open) hide(); });
        $(window).on('resize', function () { if (open) position(); });

        updateDisplay();

        return {
            get: function () {
                return {
                    checkIn: start ? toIso(start) : null,
                    checkOut: end ? toIso(end) : null
                };
            },
            set: function (ci, co) {
                start = fromIso(ci);
                end = fromIso(co);
                if (start) view = new Date(start.getFullYear(), start.getMonth(), 1);
                sync();
                if (open) render();
            }
        };
    }

    global.rangePicker = { attach: attach };
})(window);
