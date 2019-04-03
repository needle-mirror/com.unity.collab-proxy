window.ga_debug = {trace: true};
(function (i, s, o, g, r, a, m) {
	i['GoogleAnalyticsObject'] = r;
	i[r] = i[r] || function () {
		(i[r].q = i[r].q || []).push(arguments)
	}, i[r].l = 1 * new Date();
	a = s.createElement(o),
	m = s.getElementsByTagName(o)[0];
	a.async = 1;
	a.src = g;
	m.parentNode.insertBefore(a, m)
})(window, document, 'script', 'https://www.google-analytics.com/analytics.js', 'ga');

var UA_KEY = (typeof GA_UA_KEY !== 'undefined') ? GA_UA_KEY :  'UA-16068464-29',
	GA_LOCAL_STORAGE_KEY = 'ga:clientId';

if (window.localStorage) {
  ga('create', UA_KEY, {
    'storage': 'none',
    'clientId': localStorage.getItem(GA_LOCAL_STORAGE_KEY)
  });
  ga(function(tracker) {
    localStorage.setItem(GA_LOCAL_STORAGE_KEY, tracker.get('clientId'));
  });
}

ga('set', 'checkProtocolTask', function(){ /* nothing */ });
ga('send', 'pageview');