angular
	.module('ut.progress', ['ut.core'])
	.directive('utProgress', utProgressDirective);

/**
 * @ngdoc directive
 * @name ut.progress.directive:utProgress
 * @restrict E
 * 
 * @description
 * `<ut-progress>` is a directive shows a progress indicator with optional text
 *
 * Spinner from: http://loading.io/loader/?use=eyJzaXplIjoxMCwic3BlZWQiOjEsImNiayI6IiNmZmZmZmYiLCJjMSI6IiMwMGIyZmYiLCJjMiI6IjEwIiwiYzMiOiIxMiIsImM0IjoiMzAiLCJjNSI6IjUiLCJjNiI6IjM1IiwidHlwZSI6ImRlZmF1bHQifQ==
 *
 * @param {expression} [utProgressLabel] The label for the progress indicator
 */
function utProgressDirective() {
	return {
		templateUrl: "../unityeditor-components/src/components/progress/progress.tpl.html",
		scope: {
			utProgressLabel: '@'
		}
	};
}
