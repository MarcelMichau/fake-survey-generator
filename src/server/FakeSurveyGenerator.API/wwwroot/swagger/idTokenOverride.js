// Swagger UI does not support custom response_type parameters. Azure Active Directory requires an 'id_token' value to
// be passed instead of 'token' (See https://github.com/swagger-api/swagger-ui/issues/1974).
window.swaggerUiAuth = window.swaggerUiAuth || {};
window.swaggerUiAuth.tokenName = 'id_token';

if (!window.isOpenReplaced) {
    window.open = function(open) {
        return function(url) {
            url = url.replace('response_type=token', 'response_type=id_token%20token');
            let nonce = 'N' + Math.random() + '' + Date.now();
            url = url + '&nonce=' + nonce;
            console.log(url);
            return open.call(window, url);
        };
    }(window.open);
    window.isOpenReplaced = true;
}