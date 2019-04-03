angular
    .module('ut.wizard-progress', ['ut.core'])
    .directive('utWizardProgress', function() {
        return {
            restrict: 'E',
            templateUrl: "../unityeditor-components/src/components/wizard-progress/wizard-progress.tpl.html",
            scope: {
                utStep: '@',
                utTotal: '@'
            },
            link: function(scope, element, attr) {
                var i;
                scope.utSteps = [];
                for (i = 0; i < scope.utTotal; i++) {
                    scope.utSteps.push(i + 1);
                }
            }
        };
    });