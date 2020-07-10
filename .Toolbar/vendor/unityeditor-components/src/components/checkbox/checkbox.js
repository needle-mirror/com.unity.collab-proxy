
utCheckboxDirective.$inject = ["$timeout"];angular
	.module('ut.checkbox', ['ut.core'])
	.directive('utCheckbox', utCheckboxDirective);

/**
 * @ngdoc directive
 * @name ut.checkbox.directive:utCheckbox
 * @restrict E
 * 
 * @description
 * `<ut-checkbox>` is a checkbox directive that can hooks up a checkbox to a model. Based on
 * the state of the checkbox, the model's value will be set to true or false.
 *
 * @param {expression} [utCheckboxLabel] The label for the checkbox
 * @param {Object} [utCheckboxModel] The model whose value will be set
 */
function utCheckboxDirective($timeout) {
	return {
		replace: true,
		templateUrl: "../unityeditor-components/src/components/checkbox/checkbox.tpl.html",
		scope: {
			utCheckboxLabel: '@',
			utCheckboxModel: '='
		}
	};
}
