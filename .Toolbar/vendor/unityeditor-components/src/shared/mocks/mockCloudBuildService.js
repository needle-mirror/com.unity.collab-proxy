angular.module('ngUnity.mockCloudBuildService', [])
    .factory('mockCloudBuildService', ["$q", function ($q){

        // for faking promises with static values resolution
        var rejectTo = jasmine.createSpy('auto reject', function (value){
            var deferred = $q.defer();
            deferred.reject(value);
            return deferred.promise;
        }).and.callThrough();

        // for faking promises with static values rejection
        var resolveTo = jasmine.createSpy('auto resolve', function (value){
            var deferred = $q.defer();
            deferred.resolve(value);
            return deferred.promise;
        }).and.callThrough();

        // use these to fake methods that return promises in your mocks
        // they return zero-arg function spies that return promises that return value
        var returnsResolved = function(value) {
            return jasmine.createSpy('return resolved', function() {
                return resolveTo(value);
            }).and.callThrough();
        };

        var returnsRejected = function(value) {
            return jasmine.createSpy('return rejected', function() {
                return rejectTo(value);
            }).and.callThrough();
        };


        return {

            returnsResolved : returnsResolved,
            returnsRejected : returnsRejected,
            resolveTo: resolveTo,
            rejectTo: rejectTo,

            projects: {
                sshkey: returnsResolved({
                    publickey: "test"
                }),
                scm: {
                    checkUrl: {}
                }
            }
        };
    }]);
