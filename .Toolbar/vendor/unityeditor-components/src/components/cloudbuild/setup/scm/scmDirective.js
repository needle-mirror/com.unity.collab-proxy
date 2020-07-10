angular.module('ut.cloudbuild.setup.scm', [])
    .directive('validatescmurl', ["$timeout", "$parse", function($timeout, $parse) {
        return {
            restrict: 'A',
            require: 'ngModel',
            link: function (scope, element, attrs, ngModel) {
                function validate(val) {
                    ngModel.$setValidity('scmValidProtocol', true);

                    var repoType = $parse(attrs.validatescmurl)(scope);
                    if (repoType === "hg" && val) {
                        // Mercurial only supports username/password auth over http/https
                        var matches = val.match("^(.*)://");
                        if (matches && matches.length > 1) {
                            if (matches[1] !== "http" && matches[1] !== "https") {
                                ngModel.$setValidity('scmValidProtocol', false);
                            }
                        }

                    }
                }

                validate(ngModel.$viewValue);

                // validate anytime value changes
                scope.$watch(function () {
                    return ngModel.$viewValue;
                }, validate);

                // validate when repo type changes
                scope.$watch(function () {
                    return $parse(attrs.validatescmurl)(scope);
                }, function() {
                    validate(ngModel.$viewValue);
                });
            }
        };
    }]);