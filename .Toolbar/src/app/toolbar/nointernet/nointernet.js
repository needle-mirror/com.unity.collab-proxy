angular.module('ngPanel.nointernet', [
	'ui.router'
])
.config(["$stateProvider", function config($stateProvider) {
	$stateProvider.state('nointernet', {
		url: '/no-internet',
		views: {
			'root': {
				templateUrl: 'toolbar/nointernet/nointernet.tpl.html'
			}
		}
	});
}]);
