$(function () {
    var user = api.getUser();
    if (user) { location.href = authHelper.roleToDashboard(user.role); return; }

    var isHost = /[?&]role=host\b/.test(location.search);
    if (isHost) {
        $('#auth-eyebrow').text('Izloži svoju nekretninu');
        $('#auth-title').text('Postani domaćin');
        document.title = 'Postani domaćin — RezervisiMe';
        $('#auth-alert')
            .text('Naloge domaćina kreira administrator preko admin panela. Ovde možeš da se registruješ samo kao gost.')
            .removeClass('is-hidden');
        $('#auth-submit').prop('disabled', true);
    }

    var $form = $('#register-form');
    var $alert = $('#auth-alert');
    var $submit = $('#auth-submit');
    var submitLabel = $submit.text();

    function showAlert(msg) { $alert.text(msg).removeClass('is-hidden'); }
    function hideAlert() { $alert.addClass('is-hidden'); }

    function toApiDate(iso) {
        var p = iso.split('-');
        return p[2] + '/' + p[1] + '/' + p[0];
    }

    $form.on('submit', function (e) {
        e.preventDefault();
        hideAlert();

        var dob = $form.find('[name=dateOfBirth]').val();
        var data = {
            firstName: $.trim($form.find('[name=firstName]').val()),
            lastName: $.trim($form.find('[name=lastName]').val()),
            username: $.trim($form.find('[name=username]').val()),
            email: $.trim($form.find('[name=email]').val()),
            password: $form.find('[name=password]').val(),
            gender: $form.find('[name=gender]').val()
        };

        if (!data.firstName || !data.lastName || !data.username || !data.email) {
            showAlert('Popuni sva polja.');
            return;
        }
        if (!dob) {
            showAlert('Unesi datum rođenja.');
            return;
        }
        if (new Date(dob) > new Date()) {
            showAlert('Datum rođenja ne može biti u budućnosti.');
            return;
        }
        if (!data.password || data.password.length < 6) {
            showAlert('Lozinka mora imati bar 6 karaktera.');
            return;
        }
        data.dateOfBirth = toApiDate(dob);

        $submit.prop('disabled', true).text('Registrovanje…');

        var call = isHost ? api.users.createHost(data) : api.auth.register(data);

        call.done(function () {
            api.auth.login({ username: data.username, password: data.password })
                .done(function (res) {
                    api.setSession(res.token, res.user);
                    location.href = authHelper.roleToDashboard(res.user.role);
                })
                .fail(function () {
                    location.href = 'login.html';
                });
        }).fail(function (xhr) {
            showAlert((xhr.responseJSON && xhr.responseJSON.error) || 'Registracija nije uspela. Pokušaj ponovo.');
            $submit.prop('disabled', false).text(submitLabel);
        });
    });
});
