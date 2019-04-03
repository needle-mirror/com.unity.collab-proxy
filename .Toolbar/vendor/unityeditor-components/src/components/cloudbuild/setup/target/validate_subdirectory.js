angular.module('ut.cloudbuild.setup').directive('validatesubdirectory', function() {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {
            function validate(val) {
                ngModel.$setValidity('subdirectoryNewline', true);
                ngModel.$setValidity('subdirectoryCharacters', true);
                ngModel.$setValidity('subdirectoryParent', true);
                ngModel.$setValidity('subdirectorySource', true);

                var subdir = element.val();
                if (!subdir || subdir.length === 0) {
                    // will be picked up by the 'required' message if necessary
                }
                else if (/[\r\n\f]/.test(subdir)) {  // must not contain a new line
                    ngModel.$setValidity('subdirectoryNewline', false);
                }
                // Per: https://www.gnu.org/software/bash/manual/html_node/Double-Quotes.html
                // Do not allow the following characters: $ ` \ ! " * @ #
                else if (/[$`\\!"*@#]/.test(subdir)) {
                    ngModel.$setValidity('subdirectoryCharacters', false);
                }
                else if (/(?:^|\/)\.\.\//.test(subdir)) {  // must not contain a ../ or /../
                    ngModel.$setValidity('subdirectoryParent', false);
                }
                else if (/(?:^|\/)\.\//.test(subdir)) {  // must not contain a ./ or /./
                    ngModel.$setValidity('subdirectorySource', false);
                }
            }

            // validate anytime value changes
            validate(ngModel.$viewValue);
            scope.$watch( function() {
                return ngModel.$viewValue;
            }, validate);
        }
    };
});