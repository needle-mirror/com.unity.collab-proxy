
utClickOnceDirective.$inject = ["$compile"];angular
    .module('ut.button', [ 'ut.core' ])
    .directive('utButton', utButtonDirective)
	.directive('ngClickOnce', utClickOnceDirective);

/**
 * @ngdoc directive
 * @name ut.button.directive:utButton
 * @restrict EA
 *
 * @description
 * `<ut-button>` is a button directive.
 *
 * @param {expression} [ngDisabled] Enable or Disable based on the given expression
 * @param $compile
 */
function utButtonDirective() {
    return {
        restrict: 'EA',
        replace: true,
        transclude: true,
        template: getTemplate,
        link: link
	};

    function isAnchor(attr) {
        return angular.isDefined(attr.href) || angular.isDefined(attr.ngHref) || angular.isDefined(attr.ngLink) || angular.isDefined(attr.uiSref);
    }

    function link(scope, element, attr) {
	}

    function getTemplate(element, attr) {
		return isAnchor(attr) ? '<a class="ut-button" ng-transclude></a>' : '<button class="ut-button" ng-transclude></button>';
    }
}

/**
 * @ngdoc directive
 * @name ut.button.directive:utClickOnce
 * @restrict A
 * @description
 * Use ng-click-once when you want an action to complete before it can be launched again.
 *
 * Use with an expression returning a promise such as: ng-click-once="myFunc()" where myFunc returns x.promise.
 */
function utClickOnceDirective($compile) {
	return {
		restrict: 'A',
		link: link
	};

	function isPromise(value) {
		return _.isObject(value) && _.isFunction(value.then);
	}

	function link(scope, element, attrs) {
		// Need element id, since the __clickOnce method will override others in same scope.
		scope.__clickOnce = function (id) {
			var elem = $(element).parent().find("[click-once-id='" + id + "']");

			if (elem.attr('click-locked')) {
				return;
			}

			var clickExpr = elem.attr('click-once-expr');

			var result = scope.$eval(clickExpr);

			// If result is a promise
			if (isPromise(result)) {
				elem.attr('click-locked', true);

				result.final(function () {
					elem.removeAttr('click-locked');
				});
			}

			return result;
		};


		//
		//	Setup click-once
		//			This setup is very involved since we are not isolating the scope. This means we need to use the html element
		//			to pass information around.
		//
		var id = Math.floor(Math.random() * 100000);		// Keep ids number sorta-nice looking
		var clickExpr = attrs.ngClickOnce;
		var wrappedExpr = '__clickOnce(' + id + ')';

		attrs.$set('clickOnceExpr', clickExpr);
		attrs.$set('clickOnceId', id)
		attrs.$set('ngTransclude');							// Needed for compile
		attrs.$set('ngClickOnce');							// Clear to prevent loops
		attrs.$set('ngClick', wrappedExpr);
		$compile(element)(scope);
	}
}
