$(function () {
    var user = api.getUser();
    if (user) { location.href = authHelper.roleToDashboard(user.role); return; }

    var $form = $('#login-form');
    var $alert = $('#auth-alert');
    var $submit = $('#auth-submit');

    function showAlert(msg) { $alert.text(msg).removeClass('is-hidden'); }
    function hideAlert() { $alert.addClass('is-hidden'); }

    $form.on('submit', function (e) {
        e.preventDefault();
        hideAlert();

        var data = {
            username: $.trim($form.find('[name=username]').val()),
            password: $form.find('[name=password]').val()
        };

        if (!data.username || !data.password) {
            showAlert('Unesi korisničko ime i lozinku.');
            return;
        }

        $submit.prop('disabled', true).text('Prijavljivanje…');

        api.auth.login(data)
            .done(function (res) {
                api.setSession(res.token, res.user);
                location.href = authHelper.roleToDashboard(res.user.role);
            })
            .fail(function (xhr) {
                showAlert((xhr.responseJSON && xhr.responseJSON.error) || 'Prijava nije uspela. Pokušaj ponovo.');
                $submit.prop('disabled', false).text('Prijavi se');
            });
    });
});
