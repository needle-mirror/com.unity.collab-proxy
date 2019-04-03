angular.module('ngPanel.loggedout', [
	'ui.router',
	'ngUnity.connectService'
])
.config(["$stateProvider", function config($stateProvider) {
	$stateProvider.state('loggedout', {
		url: '/loggedout',
		views: {
			'root': {
				templateUrl: 'toolbar/loggedout/loggedout.tpl.html',
				controller: 'LoggedOutCtrl'
			}
		}
	});
}])
/**
 * Displays a message to the user that they are logged out and presents a button to log in.
 * @param {Object} '$scope'
 * @param {angular factory} 'unityConnectService' Provides dynamically created C# bindings.
 * @param {angular factory} 'unityProjectService' Access methods defined in EditorProjectAccess.cs.
 */
.controller('LoggedOutCtrl', ["$scope", "unityConnectService", "unityProjectService", function ($scope, unityConnectService, unityProjectService) {

		/**
		* Opens the Login Window and closes this toolbar window.
		*/
		$scope.login = function () {
			unityConnectService.ready(function() {
				unityConnectService.ShowLogin();

				if (unityProjectService.CloseToolbarWindowImmediately) {
					unityProjectService.CloseToolbarWindowImmediately();
				}
			});
		};

}]);