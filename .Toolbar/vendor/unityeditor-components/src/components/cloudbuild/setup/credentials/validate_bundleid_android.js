angular.module('ut.cloudbuild.setup').directive('validatebundleidandroid', function() {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {
            function validate(val) {
                ngModel.$setValidity('bundleIdPeriod', true);
                ngModel.$setValidity('bundleIdDoublePeriod', true);
                ngModel.$setValidity('bundleIdCharacters', true);
                ngModel.$setValidity('bundleIdStartPeriod', true);
                ngModel.$setValidity('bundleIdEndPeriod', true);
                ngModel.$setValidity('bundleIdStartUnderscore', true);
                ngModel.$setValidity('bundleIdDash', true);
                ngModel.$setValidity('bundleIdPeriodNonLetter', true);
                ngModel.$setValidity('bundleIdBeginNonLetter', true);

                var bundleId = element.val();
                if(!bundleId || bundleId.length === 0) {
                    // will be picked up by the 'required' message
                }
                else if(!bundleId.match(/\./)) {  // must contain a period
                    ngModel.$setValidity('bundleIdPeriod', false);
                }
                else if(bundleId.match(/\.\./)) { // no double period
                    ngModel.$setValidity('bundleIdDoublePeriod', false);
                }
                else if(bundleId.indexOf('.') === 0) { // can't start with a period
                    ngModel.$setValidity('bundleIdStartPeriod', false);
                }
                else if(bundleId.substring(bundleId.length-1, bundleId.length) === '.') { // can't end with a period
                    ngModel.$setValidity('bundleIdEndPeriod', false);
                }
                else if(bundleId.indexOf('_') === 0) { // can't start with an underscore
                    ngModel.$setValidity('bundleIdStartUnderscore', false);
                }
                else if(bundleId.indexOf('-') !== -1) { // no dash
                    ngModel.$setValidity('bundleIdDash', false);
                }
                else if(bundleId.match(/[^A-Za-z0-9_.]/)) { // can only contain alpha numeric, period, and underscore
                    ngModel.$setValidity('bundleIdCharacters', false);
                }
                else if(bundleId.match(/^[^A-Za-z]/)) {  // first character must be a letter
                    ngModel.$setValidity('bundleIdBeginNonLetter', false);
                }
                else if(bundleId.match(/\.[^A-Za-z]/)) {  // first character after a period must be a letter
                    ngModel.$setValidity('bundleIdPeriodNonLetter', false);
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