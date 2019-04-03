angular.module('ngPanel.noproject', [
	'ui.router',
	'ngUnity.cloudPanelService'
])
.config(["$stateProvider", function config($stateProvider) {
	$stateProvider.state('noproject', {
		url: '/no-project',
		views: {
			'root': {
				templateUrl: 'toolbar/noproject/noproject.tpl.html',
				controller: 'NoProjectCtrl'
			}
		}
	});
}])
/**
 * Displays the UI in the toolbar window when Collaborate is turned off.
 * @param {Object} '$scope'
 * @param {angular factory} 'unityCloudPanelService' Accesses data in service.json. Defined in pack 'libs'.
 */
.controller('NoProjectCtrl', ["$scope", "unityCloudPanelService", function ($scope, unityCloudPanelService) {

		$scope.userMessage = "";
		$scope.userMessageInnerHTML = "";

		// Display text found in the service.json supplied by the panel service.
		unityCloudPanelService.IsReady().then(function() {

			$scope.userMessage = "";
			$scope.userMessageInnerHTML = "";

			if (unityCloudPanelService.panelInfo.longDescriptionHTML) {
				$scope.userMessageInnerHTML = unityCloudPanelService.panelInfo.longDescriptionHTML;
			} else if (unityCloudPanelService.panelInfo.longDescription) {
				$scope.userMessage = unityCloudPanelService.panelInfo.longDescription;
			}
		});
}]);