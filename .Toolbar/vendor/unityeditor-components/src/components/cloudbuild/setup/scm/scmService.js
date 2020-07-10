angular.module('ut.cloudbuild.setup.scm').service('scmUtils', ["$q", "cloudBuildService", function($q, cloudBuildService) {
    var data = {
        // possible SCMs
        'repoTypes': [
            { 'name': 'GIT',      'value': 'git' },
            { 'name': 'SVN',      'value': 'svn' },
            { 'name': 'Perforce', 'value': 'p4'  },
            { 'name': 'Mercurial', 'value': 'hg' },
            { 'name': 'Collab', 'value': 'collab' }
        ],
        // hosts we support
        'repoHosts': [
            { 'name': 'GitHub' },
            { 'name': 'BitBucket' },
            { 'name': 'Beanstalk' },
            { 'name': 'Assembla' },
            { 'name': 'Other'}
        ]
    };

    var self = {
        data: function() {
            return data;
        },
        detectScmType: function (dataUrl) {
            if(!dataUrl) {
                return null;
            }

            dataUrl = dataUrl.toLowerCase();
            if ( dataUrl.indexOf('git@') !== -1 || dataUrl.indexOf('.git') !== -1 || dataUrl.indexOf('git://') !== -1 || dataUrl.indexOf('github.com') !== -1 ) {
                return 'git';
            }
            else if ( dataUrl.indexOf('svn://') !== -1 || dataUrl.indexOf('/svn') !== -1 ) {
                return 'svn';
            }
            else if ( dataUrl.indexOf('p4.') !== -1 || dataUrl.indexOf('perforce.') !== -1 || dataUrl.indexOf(':1666') !== -1 ) {
                return 'p4';
            }
            else {
                return null;
            }
        },
        detectScmHost: function (dataUrl) {
            dataUrl = dataUrl || '';
            dataUrl = dataUrl.toLowerCase();
            if (dataUrl.indexOf('github') !== -1 ) {
                return 'GitHub';
            }
            else if ( dataUrl.indexOf('bitbucket') !== -1 ) {
                return 'BitBucket';
            }
            else if ( dataUrl.indexOf('beanstalk') !== -1 ) {
                return 'Beanstalk';
            }
            else if ( dataUrl.indexOf('assembla') !== -1 ) {
                return 'Assembla';
            }
            else {
                return 'Other';
            }
        },
        getScmTypeNameFromType: function (scmType) {
            var types = data.repoTypes;
            var retType = null;
            types.forEach(function(type) {
                if(type.value === scmType) {
                    retType = type.name;
                }
            });
            return retType || types[0].name;
        },
        getScmBranches: function(scope) {
            if (scope.project.settings.scm.type === 'git') {
                return cloudBuildService.projects.scm.gitListBranches(scope.project.orgid, scope.project.projectid)
                    .then(function (results) {
                        if (!results || results.length === 0) {
                            scope.showError('This appears to be an empty Git repo without any branches. Add project files to this repo to proceed.');
                        }
                        else {
                            scope.branches = results;
                            if (scope.branches.length === 1 && _.isEmpty(scope.target.settings.scm.branch)) {
                                scope.target.settings.scm.branch = scope.branches[0];
                            }

                        }
                    })
                    .catch(function (error) {
                        scope.showError(error.message);
                    });
            }
            else if(scope.project.settings.scm.type === 'p4') {
                return cloudBuildService.projects.scm.p4ListClients(scope.project.orgid, scope.project.projectid)
                    .then(function(results) {
                        if(!results || results.length === 0) {
                            scope.showError('This appears to be a p4 repo without any clients. Add at least one client to this repo to proceed.');
                        }
                        else {
                            scope.clients = results;
                            if (scope.clients.length === 1 && _.isEmpty(scope.target.settings.scm.client)) {
                                scope.target.settings.scm.client = scope.clients[0];
                            }
                        }
                    })
                    .catch(function(error) {
                        scope.showError(error.message);
                    });
            }
            return $q.when();
        }
    };
    return self;
}]);
