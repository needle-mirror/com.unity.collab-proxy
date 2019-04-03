angular.module('ut.cloudbuild.setup').directive('svnpath', ["$q", "$timeout", "cloudBuildService", function($q, $timeout, cloudBuildService) {
    return {
        require: 'ngModel',
        link: function(scope, elm, attrs, ctrl) {

            ctrl.$asyncValidators.svnpath = function(modelValue, viewValue) {

                var def = $q.defer();
                cloudBuildService.projects.scm.svnListFolders(scope.project.orgid, scope.project.projectid, viewValue)
                    .then(function(results) {
                        def.resolve();
                    })
                    .catch(function(error) {
                        def.reject();
                    });
                return def.promise;
            };
        }
    };
}]);