angular.module('uc.editor.components.icons', [])
.directive('ucIcon', ["$compile", function ($compile) {
	return {
		restrict: 'EAC',
		templateUrl: function (element, attrs) {
			var icon = attrs.ucIcon;

			if (!icon) {
				console.warn('Could not retrieve svg icon');
				return '';
			}

			var root = '../../vendor/unityeditor-cloud-pack/assets/icons/svg/';
			var path = root + icon + '.svg';
			return path;
		}
	};
}]);
