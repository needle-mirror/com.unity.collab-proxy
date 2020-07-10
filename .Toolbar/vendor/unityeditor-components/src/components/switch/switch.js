
utSwitchDirective.$inject = ["$timeout"];angular
	.module('ut.switch', ['ut.core'])
	.directive('utSwitch', utSwitchDirective);

/**
 *
 */
function utSwitchDirective($timeout) {
	return {
		templateUrl: "../unityeditor-components/src/components/switch/switch.tpl.html",

		/**
		 * @param ngModel Value of switch. Note that when using a toggle function, this will be the value AFTER the toggle has been completed.
		 * @param toggle Optional toggle function that takes the form function(currentValue, callback), where callback should be called with (error, newvalue)
		 */
		scope: {
			value: '=ngModel',
			toggle: '=?'
		},
		link: function (scope, element, attrs, ngModel) {
			function toggle(value, callback) {
				callback(null, !value);
			}

			scope.isDisabled = function () {
				return scope.stealthDisabled || scope.inProgress || $(element).attr('disabled');
			};

			scope.toggle = scope.toggle || toggle;

			scope.toggleValue = function () {
				// Cannot accept changes while action is in progress
				if (scope.isDisabled()) {
					return;
				}

				scope.checked = !scope.checked;
				scope.stealthDisabled = true;			// Component is disabled until action is completed, but not visually

				// Delayed
				var loadingDelay = 300;				// How long to wait until we show the component as loading (spinner)
				var id = $timeout(function () {
					scope.inProgress = true;
				}, loadingDelay);

				scope.toggle(scope.value, function (error, value) {
					// Process the callback result in a timeout to make sure we are in an $apply
					$timeout(function () {
						scope.value = value;
						scope.checked = value;

						scope.inProgress = false;
						scope.stealthDisabled = false;
						$timeout.cancel(id);
					});
				});
			};


			scope.$watch('value', function (value, previous) {
				scope.checked = value;			// Make sure checked is always up to date with outside changes to enabled.
			});
		}
	};
}
