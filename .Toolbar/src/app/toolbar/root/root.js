angular.module('ngPanel.toolbar', [
	'ngPanel.toolbar.conflicts',
	'ngPanel.toolbar.progress',
	'ngPanel.toolbar.publish',
	'ngPanel.toolbar.update',
	'ngPanel.toolbar.uptodate',
	'yaru22.angular-timeago',
	'uc.editor.components'
])
.config(["$stateProvider", function config($stateProvider) {
	$stateProvider.state('project', {
		views: {
			'root': {
				controller: 'RootCtrl',
				templateUrl: 'toolbar/root/root.tpl.html'
			}
		},
		url: '/',
		resolve: {
			standardServices: utils.servicePromises,
			services: ["editorcollab", function(editorcollab) {
				return editorcollab.IsReady();
			}]
		}
	});
}])

.controller('RootCtrl', ["$scope", "editorcollab", function RootCtrl($scope, editorcollab) {
	$('[data-toggle="tooltip"]').tooltip({
		animation: false,
		delay: {
			show: 0,
			hide: 0
		},
		placement: 'top'
	});
}])

.filter('filename', function () {
	return function (input) {

		if (!input || (input.length === 0)) {
			return '';
		}

		var resa = input.split('/');

		return resa[resa.length - 1];
	};
})
.filter('stripExtension', function () {
	return function (input) {
		if (input === undefined){
			return;
		}
		var re = /[.]([^\/.])*$/;
		return input.replace(re, '');
	};
});
