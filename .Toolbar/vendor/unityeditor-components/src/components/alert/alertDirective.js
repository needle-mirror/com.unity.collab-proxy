angular.module('ut.alert', ['ngSanitize'])
.directive('utAlert', ["utAlert", "$timeout", function(utAlert, $timeout) {
	/**
	 * If you get an $sce error, remember to make sure you have included 'ngSanitize' module.
	 */
	return {
		restrict: 'EAC',
		templateUrl: '../unityeditor-components/src/components/alert/alert.tpl.html',
		controller: ["$scope", "$element", function($scope, $element) {
			$scope.$watch(function () {
				return utAlert;
			}, function (newVal) {
				$scope.alertText = newVal.alertText;
				$scope.alertType = newVal.alertType;
				$scope.linkPreText = newVal.linkPreText;
				$scope.linkText = newVal.linkText;
				$scope.linkSuffixText = newVal.linkSuffixText;
				$scope.closeable = newVal.closeable;

				$element.removeClass('uncloseable');
				if (!$scope.closeable) {
					$element.addClass('uncloseable');
				}
			}, true);

			$scope.resetAlert = function () {
				if (utAlert.closeable) {
					utAlert.clearAlert();
				}
			};

			$scope.getShowMoreinfo = function () {
				return utAlert.showMoreInfo && utAlert.linkHandler !== null && utAlert.linkUrl !== null;
			};

			$scope.linkClickHandler = function () {
				utAlert.linkHandler(utAlert.linkUrl);
			};
		}]
	};
}]);
