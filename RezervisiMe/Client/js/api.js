(function (global) {
    var BASE = '/api';
    var TOKEN_KEY = 'rm_token';
    var USER_KEY  = 'rm_user';

    function getToken() { return localStorage.getItem(TOKEN_KEY); }
    function getUser() {
        var raw = localStorage.getItem(USER_KEY);
        try { return raw ? JSON.parse(raw) : null; }
        catch (e) { return null; }
    }
    function setSession(token, user) {
        localStorage.setItem(TOKEN_KEY, token);
        localStorage.setItem(USER_KEY, JSON.stringify(user));
    }
    function clearSession() {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
    }

    function request(method, path, body, isMultipart) {
        var opts = {
            url: BASE + path,
            method: method,
            headers: {},
            dataType: 'json'
        };
        var token = getToken();
        if (token) opts.headers['Authorization'] = 'Bearer ' + token;

        if (body !== undefined && body !== null) {
            if (isMultipart) {
                opts.data = body;
                opts.processData = false;
                opts.contentType = false;
            } else {
                opts.contentType = 'application/json';
                opts.data = JSON.stringify(body);
            }
        }

        return $.ajax(opts).fail(function (xhr) {
            if (xhr.status === 401) {
                clearSession();
                var p = location.pathname;
                if (!/(?:login|register|index|accommodation)\.html$/.test(p) && p !== '/') {
                    location.href = 'login.html';
                }
            }
        });
    }

    global.api = {
        getToken: getToken,
        getUser: getUser,
        setSession: setSession,
        clearSession: clearSession,

        auth: {
            register: function (d) { return request('POST', '/auth/register', d); },
            login:    function (d) { return request('POST', '/auth/login', d); },
            logout:   function ()  { return request('POST', '/auth/logout'); },
            me:       function ()  { return request('GET',  '/auth/me'); },
            updateMe: function (d) { return request('PUT',  '/auth/me', d); }
        },
        users: {
            search:     function (q)        { return request('GET',  '/users?' + $.param(q || {})); },
            getById:    function (id)       { return request('GET',  '/users/' + id); },
            createHost: function (d)        { return request('POST', '/users/host', d); },
            update:     function (id, d)    { return request('PUT',  '/users/' + id, d); },
            remove:     function (id)       { return request('DELETE', '/users/' + id); }
        },
        accommodations: {
            list:        function (q)       { return request('GET',  '/accommodations?' + $.param(q || {})); },
            get:         function (id)      { return request('GET',  '/accommodations/' + id); },
            mine:        function (q)       { return request('GET',  '/accommodations/mine?' + $.param(q || {})); },
            create:      function (d)       { return request('POST', '/accommodations', d); },
            update:      function (id, d)   { return request('PUT',  '/accommodations/' + id, d); },
            remove:      function (id)      { return request('DELETE', '/accommodations/' + id); },
            uploadImage: function (file) {
                var fd = new FormData();
                fd.append('file', file);
                return request('POST', '/accommodations/upload-image', fd, true);
            }
        },
        reservations: {
            mine:    function (status)       { return request('GET',  '/reservations/mine' + (status ? '?status=' + status : '')); },
            all:     function (q)            { return request('GET',  '/reservations?' + $.param(q || {})); },
            create:  function (d)            { return request('POST', '/reservations', d); },
            cancel:  function (id)           { return request('POST', '/reservations/' + id + '/cancel'); },
            approve: function (id)           { return request('POST', '/reservations/' + id + '/approve'); },
            reject:  function (id)           { return request('POST', '/reservations/' + id + '/reject'); }
        },
        reviews: {
            forAccommodation: function (accId)   { return request('GET',  '/reviews/for-accommodation/' + accId); },
            all:              function (q)       { return request('GET',  '/reviews?' + $.param(q || {})); },
            mine:             function ()        { return request('GET',  '/reviews/mine'); },
            create:           function (d)       { return request('POST', '/reviews', d); },
            update:           function (id, d)   { return request('PUT',  '/reviews/' + id, d); },
            remove:           function (id)      { return request('DELETE', '/reviews/' + id); },
            approve:          function (id)      { return request('POST', '/reviews/' + id + '/approve'); },
            reject:           function (id)      { return request('POST', '/reviews/' + id + '/reject'); }
        }
    };
})(window);
