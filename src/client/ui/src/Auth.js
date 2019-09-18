import React, { useEffect, useState } from 'react';

const AUTH_BASE_URL = `${window.location.protocol}//${window.location.hostname}/identity`;

const login = () => {
	resetAuthorizationData();

	let authorizationUrl = AUTH_BASE_URL + '/connect/authorize';
	let client_id = 'fake-survey-generator-ui';
	let redirect_uri = window.location.origin + '/';
	let response_type = 'id_token token';
	let scope = 'openid profile fake-survey-generator-api';
	let nonce = 'N' + Math.random() + '' + Date.now();
	let state = Date.now() + '' + Math.random();

	localStorage.setItem('authStateControl', state);
	localStorage.setItem('authNonce', nonce);

	let url =
		authorizationUrl +
		'?' +
		'response_type=' +
		encodeURI(response_type) +
		'&' +
		'client_id=' +
		encodeURI(client_id) +
		'&' +
		'redirect_uri=' +
		encodeURI(redirect_uri) +
		'&' +
		'scope=' +
		encodeURI(scope) +
		'&' +
		'nonce=' +
		encodeURI(nonce) +
		'&' +
		'state=' +
		encodeURI(state);

	window.location.href = url;
};

const authCallback = () => {
	resetAuthorizationData();

	let hash = window.location.hash.substr(1);

	let result = hash.split('&').reduce(function(result, item) {
		let parts = item.split('=');
		result[parts[0]] = parts[1];
		return result;
	}, {});

	console.log(result);

	let token = '';
	let id_token = '';
	let authResponseIsValid = false;

	if (!result.error) {
		if (result.state !== localStorage.getItem('authStateControl')) {
			console.log('authCallback incorrect state');
		} else {
			token = result.access_token;
			id_token = result.id_token;

			let dataIdToken = getDataFromToken(id_token);
			console.log(dataIdToken);

			// validate nonce
			if (dataIdToken.nonce !== localStorage.getItem('authNonce')) {
				console.log('authCallback incorrect nonce');
			} else {
				localStorage.setItem('authNonce', '');
				localStorage.setItem('authStateControl', '');

				authResponseIsValid = true;
				console.log(
					'authCallback state and nonce validated, returning access token'
				);
			}
		}
	}

	if (authResponseIsValid) {
		setAuthorizationData(token, id_token);
	}

	return authResponseIsValid;
};

const setAuthorizationData = (token, id_token) => {
	if (localStorage.getItem('authorizationData') !== '') {
		localStorage.setItem('authorizationData', '');
	}

	localStorage.setItem('authorizationData', token);
	localStorage.setItem('authorizationDataIdToken', id_token);
	localStorage.setItem('IsAuthorized', true);

	window.location.href = window.location.origin;
};

const resetAuthorizationData = () => {
	localStorage.setItem('authorizationData', '');
	localStorage.setItem('authorizationDataIdToken', '');
	localStorage.setItem('IsAuthorized', false);
};

const getDataFromToken = token => {
	let data = {};
	if (typeof token !== 'undefined') {
		let encoded = token.split('.')[1];
		data = JSON.parse(urlBase64Decode(encoded));
	}

	return data;
};

const urlBase64Decode = str => {
	let output = str.replace('-', '+').replace('_', '/');
	switch (output.length % 4) {
		case 0:
			break;
		case 2:
			output += '==';
			break;
		case 3:
			output += '=';
			break;
		default:
			throw new Error('Illegal base64url string!');
	}

	return window.atob(output);
};

const logout = () => {
	let authorizationUrl = AUTH_BASE_URL + '/connect/endsession';
	let id_token_hint = localStorage.getItem('authorizationDataIdToken');
	let post_logout_redirect_uri = window.location.origin + '/';

	let url =
		authorizationUrl +
		'?' +
		'id_token_hint=' +
		encodeURI(id_token_hint) +
		'&' +
		'post_logout_redirect_uri=' +
		encodeURI(post_logout_redirect_uri);

	resetAuthorizationData();
	window.location.href = url;
};

const Auth = () => {
	const hasBeenAuthorized = localStorage.getItem('IsAuthorized') === 'true';

	const [isAuthorized, setIsAuthorized] = useState(hasBeenAuthorized);

	useEffect(() => {
		if (window.location.hash) {
			setIsAuthorized(authCallback());
		}
	}, []);

	return (
		<button
			style={{
				margin: '1em',
				background: 'rgb(28, 184, 65)',
				color: 'white'
			}}
			className="pure-button"
			type="button"
			onClick={isAuthorized ? logout : login}
		>
			{isAuthorized ? <span>Logout</span> : <span>Login</span>}
		</button>
	);
};

export default Auth;
