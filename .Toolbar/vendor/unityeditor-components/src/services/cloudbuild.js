angular.module('ut.services.cloudbuild', ['ngResource', 'ut.unityeditor-components.cloud-config'])
.factory('cloudBuildServiceCredentials', ["$injector", "$document", function cloudBuildServiceCredentials($injector, $document) {
    // CSRF protection for JSON payloads
    function getCookie(name) {
        var value = "; " + $document.prop('cookie');
        var parts = value.split("; " + name + "=");
        if (parts.length == 2) {
            return parts.pop().split(";").shift();
        }
        else {
            return null;
        }
    }

    return {
        'request': function (config) {
            var cloudBuildService = $injector.get('cloudBuildService'); // prevent dependency loop
            if (cloudBuildService && cloudBuildService.isServiceRequest(config.url)) {
                var token = getCookie('connect_token') || getCookie('access_token');
                if(token) {
                    config.headers = config.headers || {};
                    config.headers.Authorization = 'Bearer ' + token;
                }
            }
            return config;
        }
    };
}])
.factory('cloudBuildServiceMaintenance', ["$injector", "$rootScope", "$q", function cloudBuildServiceInterceptor($injector, $rootScope, $q) {
    var triggerServiceDown = function() {
        $rootScope.$broadcast('uw.service.down', {cloudBuildService: true});
    }
    var batchRegexp = /\/batch\?/;
    return {
        response: function(response) {
            if (batchRegexp.exec(response.config.url) && response.data) {
                var cloudBuildService = $injector.get('cloudBuildService'); // prevent dependency loop
                if (cloudBuildService && cloudBuildService.isServiceRequest(response.config.url)) {
                    Object.keys(response.data).forEach(function (key) {
                        if (response.data[key].statusCode === 503) {
                            triggerServiceDown();
                        }
                    });
                }
            }
            return response;
        },
        responseError: function (response) {
            if (response.status === 503) {
                var cloudBuildService = $injector.get('cloudBuildService'); // prevent dependency loop
                if (cloudBuildService && cloudBuildService.isServiceRequest(response.config.url)) {
                    triggerServiceDown();
                }
            }
            return $q.reject(response);
        }
    };
}])
.factory('cloudBuildService', ["$resource", "$location", "$q", "$http", "coreConfigService", function cloudBuildServiceProvider($resource, $location, $q, $http, coreConfigService) {
    var serviceUrl = '';

    // api resources
    var api = {};
    var ready = $q.defer();
    var factory = this;
    coreConfigService.getConfigUrl ('build_api_url').then(function(url) {
        buildService.serviceUrl(url);
        var baseApiUrl = serviceUrl + '/api/v1/';
        api.baseUrl = baseApiUrl;
        api.userSelfUrl = baseApiUrl + 'users/me';
        api.projectsUrl = baseApiUrl + 'projects';
        api.orgsUrl = baseApiUrl + 'orgs';
        api.versionsUrl = baseApiUrl + 'versions';
        api.oauthUrl = baseApiUrl + 'oauth';
        ready.resolve(true);
    });

    // track current user
    var cachedUser = {};
    function queryAndCacheCurrentUser() {
        var deferred = $q.defer();
        var userSelf = $resource(api.userSelfUrl+ "?include=orgs,lastAcceptedEulaVersion,permissions", {});
        userSelf.get().$promise
            .then(function(user) {
                return checkUserPermissions(user);
            })
            .then(function(user) {
                cachedUser = user;
                deferred.resolve(user);
            }, function(error) {
                deferred.reject(error);
            });
        return deferred.promise;
    }

    function checkUserPermissions(user) {
        var deferred = $q.defer();
        if(!user) {
            deferred.reject(authError('User not found.'));
        }
        else {
            deferred.resolve(user);
        }
        return deferred.promise;
    }

    function authError(message) {
        var err = new Error(message);
        err.authError = true;
        return err;
    }

    function getBuilds(path, queryOptions, cb) {
        var query = '';

        if(queryOptions.hasOwnProperty('status') && queryOptions.status) {
            query = query + '&buildStatus=' + queryOptions.status;
        }
        if(queryOptions.hasOwnProperty('platform') && queryOptions.platform) {
            query = query + '&platform=' + queryOptions.platform;
        }
        if(queryOptions.hasOwnProperty('showDeleted') && queryOptions.showDeleted) {
            query = query + '&showDeleted=' + queryOptions.showDeleted;
        }
        if(queryOptions.hasOwnProperty('onlyFavorites') && queryOptions.onlyFavorites) {
            query = query + '&onlyFavorites=' + queryOptions.onlyFavorites;
        }

        var canceler = $q.defer();
        $http.get(path + query, {cache: false,timeout:canceler.promise}).
        then(function(res) {
            if(!res.data){
                cb('Server returned an error');
            }
            else{
                var total = res.headers('content-range');
                total = total.split("/");

                var resp = {total_count: total[1], data:res.data};
                cb(null,resp);
            }
        },function(res) {
            cb(res);
        });

        return canceler;
    }

    // external interface
    var buildService = {
        ready: ready.promise,
        currentUser: {
            get: function (shouldQuery) {
                var deferred = $q.defer();
                if (_.isEmpty(cachedUser)) {
                    if(shouldQuery) {
                        return queryAndCacheCurrentUser();
                    }
                    else {
                        deferred.reject(authError('User not found.'));
                    }
                }
                else {
                    deferred.resolve(cachedUser);
                }
                return deferred.promise;
            },
            clear: function() {
                cachedUser = {};
            }
        },

        users: {},
        projects: {},
        orgs: {}
    };

    buildService.serviceUrl = function(value) {
        serviceUrl = value;
    };

    buildService.getServiceUrl = function() {
        return serviceUrl;
    };

    buildService.isServiceRequest = function(url) {
        return serviceUrl && _.startsWith(url, serviceUrl);
    };

    buildService.needsProjectSetup = function(project) {
      return !project || _.isEmpty(project.settings.scm) || _.isEmpty(project.settings.scm.type);
    };

    // list users for a specific project
    buildService.projects.listUsers = function(projectid) {
        return $resource(api.projectsUrl + '/' + projectid + '/users').query().$promise;
    };

    // list projects for a specific user
    buildService.users.listProjects = function() {
        return $resource(api.projectsUrl+'?include=settings,serviceFlags').query().$promise;
    };

    // list projects for a specific org
    buildService.orgs.listProjects = function(orgid) {
        return $resource(api.orgsUrl + '/' + orgid + '/projects').query().$promise;
    };

    // get an orgs billing user
    buildService.orgs.getBillingUser = function(orgid) {
        return $resource(api.orgsUrl + '/' + orgid + '/billinguser').get().$promise;
    };

    // get an orgs billing user
    buildService.orgs.getBillingPlan = function(orgid) {
        return $resource(api.orgsUrl + '/' + orgid + '/billingplan').get().$promise;
    };

    buildService.orgs.getBuildTargets = function(orgid) {
        return $resource(api.orgsUrl + '/' + orgid + '/buildtargets/?include=settings,credentials').query().$promise;
    };

    buildService.orgs.getBuilds = function(orgid, limit, page, queryOptions, cb) {
        var path = api.orgsUrl + '/' + orgid + '/builds?per_page='+limit+'&page='+page;
        getBuilds(path, queryOptions, cb);
    };

    // Cancel Build
    buildService.orgs.cancelBuilds = function(orgid) {
        return $resource(api.orgsUrl + '/' + orgid + '/builds').delete().$promise;
    };

    buildService.projects.details = function(orgid, projectid) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid+'?include=serviceFlags').get().$promise;
    };

    buildService.projects.guid = function(projectidGuid) {
        return $resource(api.projectsUrl + '/' + projectidGuid).get().$promise;
    };

    buildService.projects.recordPageView = function(projectidGuid, data) {
        return $resource(api.projectsUrl + '/' + projectidGuid + '/events', {}, {}).save({type: 'page.view', data: data}).$promise;
    };

    buildService.projects.create = function(orgid, name) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects').save({name: name}).$promise;
    };

    buildService.projects.edit = function(orgid, projectid, updates) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid, {}, {'update': {method: 'PUT'}}).update(updates).$promise;
    };

    buildService.projects.reset = function(orgid, projectid) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid, {}).delete().$promise;
    };

    buildService.projects.disable = function(orgid, projectid) {
        var updates = { 'disabled': true };
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid, {}, {'update': {method: 'PUT'}}).update(updates).$promise;
    };

    buildService.projects.enable = function(orgid, projectid) {
        var updates = { 'disabled': false };
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid, {}, {'update': {method: 'PUT'}}).update(updates).$promise;
    };

    buildService.projects.listCredentials = function(orgid, projectid, platform) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid + '/credentials/signing/' + platform).query().$promise;
    };

    buildService.projects.uploadCredentials = function(orgid, projectid, platform, options) {
        var saveParams = generateMultipartSaveParameter();
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid + '/credentials/signing/' + platform, {}, saveParams).save(options).$promise;
    };

    function generateMultipartSaveParameter() {
        return {
            save: {
                method: 'POST',
                transformRequest: function(data) {
                    var fd = new FormData();
                    angular.forEach(data, function(value, key) {
                        fd.append(key, value);
                    });
                    return fd;
                },
                headers: {'Content-Type':undefined, enctype:'multipart/form-data'}
            }
        };
    }

    buildService.projects.scm = {};
    buildService.projects.scm.checkUrl = function(orgid, projectid, params) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid + '/scm/checkurl').save(params).$promise;
    };

    buildService.projects.scm.gitListBranches = function(orgid, projectid) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid + '/scm/git/branches').query().$promise;
    };

    buildService.projects.scm.svnListFolders = function(orgid, projectid, subfolder) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid + '/scm/svn/folders', {}, {'post': {method: 'POST', isArray:true}}).post({subfolder:subfolder}).$promise;
    };

    buildService.projects.scm.p4ListClients = function(orgid, projectid) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid + '/scm/p4/clients').query().$promise;
    };

    buildService.projects.hooks = {};
    buildService.projects.hooks.list = function(orgid, projectid) {
        return $resource(api.orgsUrl + '/' + orgid + '/projects/' + projectid + '/hooks/').query().$promise;
    };
    buildService.projects.hooks.create = function(orgid, projectid, options) {
        return $resource(api.orgsUrl + '/' + orgid + '/projects/' + projectid + '/hooks/').save(options).$promise;
    };
    buildService.projects.hooks.get = function(orgid, projectid, hookid) {
        return $resource(api.orgsUrl + '/' + orgid + '/projects/' + projectid + '/hooks/' + hookid).get().$promise;
    };
    buildService.projects.hooks.update = function(orgid, projectid, hookid, options) {
        return $resource(api.orgsUrl + '/' + orgid + '/projects/' + projectid + '/hooks/' + hookid, {}, {'update':{method:'PUT'}}).update(options).$promise;
    };
    buildService.projects.hooks.delete = function(orgid, projectid, hookid) {
        return $resource(api.orgsUrl + '/' + orgid + '/projects/' + projectid + '/hooks/' + hookid).delete().$promise;
    };
    buildService.projects.hooks.ping = function(orgid, projectid, hookid) {
        return $resource(api.orgsUrl + '/' + orgid + '/projects/' + projectid + '/hooks/' + hookid + '/ping').save({}).$promise;
    };

    ///// GET BUILDS WITH HEADERS
    buildService.projects.getBuilds = function(orgid, projectid, buildtargetid, limit, page, queryOptions, cb) {
        var path = api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/buildtargets/'+ buildtargetid +'/builds?per_page='+limit+'&page='+page;
        getBuilds(path, queryOptions, cb);
    };

    buildService.projects.getAuditlog = function(orgid, projectid, limit, page) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/auditlog/?per_page=' + limit + '&page=' + page, {}, {
            query: {
                method: 'GET',
                transformResponse: function(data, headers) {
                    var response = {};

                    response.data = data;
                    if (typeof data === 'string') {
                        response.data = angular.fromJson(data);
                    }

                    var total = headers('content-range');
                    total = total.split("/");
                    response.total_count = total[1];
                    return response;
                }
            }
        }).query().$promise;
    };

    // Delete all artifacts associated with a build target (_all is allowed)
    buildService.projects.deleteBuildArtifactsForTarget = function(orgid, projectid, buildtargetid) {
        if(!buildtargetid) {
            buildtargetid = '_all';
        }
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid  +'/buildtargets/' + buildtargetid + '/builds/artifacts').delete().$promise;
    };

    // Delete all artifacts associated with a batch of builds
    buildService.projects.batchDeleteBuildArtifacts = function(orgid, projectid, builds) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid + '/artifacts/delete', {}, {'update': {method: 'POST'}}).update({builds:builds}).$promise;
    };

    // Update build Details
    buildService.projects.updateBuild = function(orgid, projectid, buildtargetid, buildnumber, updates) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/buildtargets/' + buildtargetid + '/builds/' + buildnumber, {}, {'update': {method: 'PUT'}}).update(updates).$promise;
    };

    // Build Details
    buildService.projects.getBuild = function(orgid, projectid, buildtargetid, buildnumber) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/buildtargets/' + buildtargetid + '/builds/' + buildnumber).get().$promise;
    };

    // Build Details including test results
    buildService.projects.getBuildWithTestData = function(orgid, projectid, buildtargetid, buildnumber) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/buildtargets/' + buildtargetid + '/builds/' + buildnumber + '?include=testResults').get().$promise;
    };

    // Cancel Build
    buildService.projects.cancelBuild = function(orgid, projectid, buildtargetid, buildnumber) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/buildtargets/'+ buildtargetid +'/builds/' +buildnumber).delete().$promise;
    };

    // Start Builds
    buildService.projects.startBuilds = function(orgid, projectid, buildtargetid, clean, cb) {
        if(!buildtargetid) {
            buildtargetid = '_all';
        }

        if(!clean){
            clean = false;
        }

        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/buildtargets/'+ buildtargetid +'/builds', {}, {'post': {method: 'POST', isArray:true}}).post({"clean": clean}).$promise;
    };

    // Build Targets
    buildService.projects.getBuildTargets = function(orgid, projectid) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/buildtargets/?include=settings,credentials').query().$promise;
    };

    // Build target with last success
    buildService.projects.getBuildTargetsAndLastSuccess = function(orgid, projectid) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/buildtargets/?include_last_success=true').query().$promise;
    };

    // Create Target
    buildService.projects.createBuildTarget = function(orgid, projectid, newTarget) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/buildtargets/').save(newTarget).$promise;
    };

    // Update Target
    buildService.projects.updateBuildTarget = function(orgid, projectid, buildtargetid, updates) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/buildtargets/'+ buildtargetid, {}, {'update':{method:'PUT'}}).update(updates).$promise;
    };

    // Delete Target
    buildService.projects.deleteBuildTarget = function(orgid, projectid, buildtargetid) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/buildtargets/'+ buildtargetid).delete().$promise;
    };

    // Build Log
    buildService.projects.getBuildLog = function(orgid, projectid, buildtargetid, buildnumber, compact, offset, cb) {
        if (!compact) {
            compact = 'false';
        }

        if (!offset) {
            offset = 0;
        }

        $http.get(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/buildtargets/'+ buildtargetid +'/builds/' +buildnumber+'/log?compact='+compact+'&offsetlines='+offset+'&withHtml=true&linenumbers=true').
        then(function(response) {
            if (response.status !== 200) {
                cb('Server returned an error');
            }
            else {
                if(!response.data){
                    response.data = "No logs for this build";
                }
                cb(null,response.data);
            }
        },function(response) {
            cb('Server returned an error');
        });
    };

    buildService.projects.getRawBuildLog = function(orgid, projectid, buildtargetid, buildnumber, cb) {
        $http.get(api.baseUrl + 'admin/orgs' + '/' + orgid +'/projects/'+ projectid +'/buildtargets/'+ buildtargetid +'/builds/' +buildnumber+'/rawlog').
        then(function(response) {
            if (response.status !== 200) {
                cb('Server returned an error');
            }
            else {
                if(!response.data){
                    response.data = "No logs for this build";
                }
                cb(null,response.data);
            }
        },function(response) {
            cb('Server returned an error');
        });
    };

    buildService.projects.getBuildAuditlog = function(orgid, projectid, buildtargetid, buildnumber, limit, page) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/buildtargets/'+ buildtargetid +'/builds/' +buildnumber+'/auditlog/?per_page=' + limit + '&page=' + page, {}, {
            query: {
                method: 'GET',
                transformResponse: function(data, headers) {
                    var response = {};

                    response.data = data;
                    if (typeof data === 'string') {
                        response.data = angular.fromJson(data);
                    }

                    var total = headers('content-range');
                    total = total.split("/");
                    response.total_count = total[1];
                    return response;
                }
            }
        }).query().$promise;
    };

    // SSH Key
    buildService.projects.sshkey = function(orgid, projectid) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/sshkey').get().$promise;
    };

    // Project Collaborators
    buildService.projects.collabs = function(orgid, projectid) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/users').query().$promise;
    };

    //Project Add Collaborators
    buildService.projects.addCollab = function(orgid, projectid, options, cb) {
        $http.post(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/users', options)
            .success(function(data, status, headers, config){
                if(data){
                    cb(null, data);
                }
                else{
                    cb('error');
                }
            });
    };

    //Remove Collaborators
    buildService.projects.removeCollab = function(orgid, projectid, email, cb) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/users/' + email).delete().$promise;
    };

    // Project Stats
    buildService.projects.stats = function(orgid, projectid) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/stats').get().$promise;
    };

    //Project Plan
    buildService.projects.plan = function(orgid, projectid) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/billingplan').get().$promise;
    };

    // Get an projects owning user
    buildService.projects.getOwningUser = function(projectid) {
        return $resource(api.projectsUrl + '/' + projectid + '/owninguser').get().$promise;
    };

    // Create share ID
    buildService.projects.getShareID = function(orgid, projectid, buildtargetid, buildnumber) {
        return $resource(api.orgsUrl + '/' + orgid +'/projects/'+ projectid +'/buildtargets/'+ buildtargetid +'/builds/' +buildnumber + '/share').get().$promise;
    };

    // Delete share ID
    buildService.projects.deleteShareID = function(orgid, projectid, buildtargetid, buildnumber) {
        return $resource(api.orgsUrl+'/'+orgid+'/projects/'+projectid+'/buildtargets/'+buildtargetid+'/builds/'+buildnumber+'/share').delete().$promise;
    };

    buildService.projects.createShare = function(orgid, projectid, buildtargetid, buildnumber) {
        return $resource(api.orgsUrl+'/'+orgid+'/projects/'+projectid+'/buildtargets/'+buildtargetid+'/builds/'+buildnumber+'/share').save({}).$promise;
    };

    // Transfer Org
    buildService.projects.transferProjectToOrg = function(projectid, orgid) {
        return $resource(api.projectsUrl + '/' + projectid + '/transfer', {}, {'put': { method:'PUT' }}).put({'orgid': orgid}).$promise;
    };

    buildService.users.edit = function(options) {
        console.dir(options);
        return $resource(api.userSelfUrl, {}, {'put': { method:'PUT' }}).put(options).$promise;
    };

    // Regenerate api key
    buildService.users.apikey = function() {
        // have to pass in empty object to get correct content type
        var options = {};
        return $resource(api.userSelfUrl + '/apikey', {}, {post:{method: 'POST' }}).post(options).$promise;
    };

    buildService.users.devices = {};
    buildService.users.devices.list = function() {
        return $resource(api.userSelfUrl + '/devices').query().$promise;
    };
    buildService.users.devices.create = function(options, cb) {
        return $resource(api.userSelfUrl  + '/devices').save(options).$promise;
    };

    buildService.users.recordPageView = function(data) {
        return $resource(api.userSelfUrl + '/events', {}, {}).save({type: 'page.view', data: data}).$promise;
    };

    buildService.getUrl = function(url, array) {
        if(array) {
            return $resource(serviceUrl + url).query().$promise;
        }
        else {
            return $resource(serviceUrl + url).get().$promise;
        }
    };

    buildService.versions = {};
    buildService.versions.get = function(key) {
        return $resource(api.versionsUrl + '/' + key).query().$promise;
    };

    buildService.oauth = {};
    buildService.oauth.get_authorize_url = function(oauthType, queryString) {
        return api.oauthUrl + '/' + oauthType + '/authorize' + queryString;
    };
    buildService.oauth.access_token = function(oauthType, options) {
        return $resource(api.oauthUrl + '/' + oauthType + '/access_token').save(options).$promise;
    };

    return buildService;
}]);
