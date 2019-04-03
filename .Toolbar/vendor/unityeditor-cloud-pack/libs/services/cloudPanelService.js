angular.module('ngUnity.cloudPanelService', [
	'ngUnity.unityService',
	'ngUnity.connectService',
	'ut.components',
	'ngUnity.general',
	'ngUnity.cloudcore'
])
.factory('unityCloudPanelService', ["$q", "$http", "$interval", "$rootScope", "$timeout", "unityConnectService", "utAlert", "unityService", "Roles", "cloudcoreUi", "StreamEvent", function ($q, $http, $interval, $rootScope, $timeout, unityConnectService, utAlert, unityService, Roles, cloudcoreUi, StreamEvent){
	var serviceReady = $q.defer();
	var events = {
		enabled: new StreamEvent('panelInfo.enabled')
	};

	var k_sampleUrl= "assets/samples/";
	var service = {

		initialize: function (){

			//register showWebView event
			document.addEventListener("showWebView", function(event) {
		  		$rootScope.$emit('showWebView', event.detail.visible);
			});

			var _caller= this;

 			var pending_getServiceEnabled= false;

			var tickInterval = 100;

			var IsAdsServiceEnabled = function(){
				var adsObject = unityService.getUnityObject('unity/project/cloud/ads');
				// wait for unityService to get c# object stub

				var deferred = $q.defer();
				adsObject.ready (function (){
					$q.all([
						adsObject.GetIOSGameId(),
						adsObject.GetAndroidGameId()
					]).then( function (results){
						deferred.resolve(results[0] || results[1] ? true : false);
					});
				});

				return deferred.promise;
			};

			var _getServiceEnabled = function (){
				if (pending_getServiceEnabled || !_caller.IsServiceEnabled){
					return $q.when();
				}

				pending_getServiceEnabled= true;
				var innerPromise;
				if (_caller.panelInfo.serviceFlag != "ads"){
					innerPromise = _caller.IsServiceEnabled();
				} else {
					innerPromise = IsAdsServiceEnabled();
				}
				return innerPromise
					.success(function (value) {
						_caller.enabled = value;
						_caller.panelInfo.enabled= value;

						events.enabled.update(value);
					})
					.final(function (){
						pending_getServiceEnabled= false;
					});
			};

			var _getPanelSamples = function (){
				var samplePromise = $q.defer();

				if (!_caller.panelInfo.hasSamples){
					samplePromise.resolve();
				}else{
					var loader= utils.makeSampleLoader($q,$http,k_sampleUrl);
					var innerPromise = loader();

					innerPromise.success(function (samples){
							_caller.codeSamples= samples;
							samplePromise.resolve();
						})
						.error(function (status){
							samplePromise.reject(status);
						});
				}

				return samplePromise.promise;
			};

			var _getServiceInfo = function (){
				var config = {
					ignoreLoadingBar: true,
					cache: false,
					no_auth: true
				};

				return $http.get("assets/service.json", config).then(
					function success(options) {
						_caller.panelInfo = options.data;

						return options.data;
					},
					function error(data, status, headers, config) {
						_caller.panelInfo = {};
						return $q.reject();
					}
				);
			};

			var _extendEnableService = function (){

				var baseEnableService = _caller.EnableService;
				_caller.passthroughEnableService = baseEnableService;

				var perms = unityConnectService.projectInfo.permissions;
 				perms.ready().then(function(){
					// hide the enable switch entirely from project users not in the org
					if (perms.project) {
					  if(perms.project.value === Roles.user.value && !perms.organizationAccess) {
							_caller.panelInfo.hideEnableSwitch = true;
						}
					}
				});
				_caller.EnableService = function (flag){
					var _caller= this;
					var enableServicePromise = $q.defer();
					var perms = unityConnectService.projectInfo.permissions;

					if (_caller.panelInfo.limitEnableToOrgOwner &&
						((perms.project.value != Roles.owner.value) ||
						 (!perms.organizationAccess))){
						utAlert.setError("Cannot " + (flag?"enable":"disable") + " service unless you have owner privilege");
						_caller.panelInfo.preEnable= false;
						_caller.panelInfo.enabled= false;
						enableServicePromise.reject();
						return enableServicePromise.promise;
					}

					cloudcoreUi.UpdateServiceFlag(_caller.panelInfo.serviceFlag, flag)
						.success(function(flags) {
							// should we be able to chain promise and do something like
							// return baseEnableService.call(_caller,flag);  instead
							// of these nested callback?  doesn't seem to work...
							// NOTE: .success and .error seems to never be called by the editor. So this acts as a 'fire and forget' more then a promise.
							baseEnableService.call(_caller,flag)
							.success(function(){
								enableServicePromise.resolve();
							})
							.error(function(){
								enableServicePromise.reject();
							});
						})
						.error(function(status) {
							if (status == 403) {
								utAlert.setError("You need owner privilege to enable or disable "+_caller.panelInfo.title);
							} else {
								utAlert.setError("Cannot " + (flag?"enable":"disable") + " " + _caller.panelInfo.title);
							}
							enableServicePromise.reject();
						});

				    return enableServicePromise.promise;
				};
			};

			var _getEditorObject = function (){
				var editorObjectPromise = $q.defer();

				if (_caller.panelInfo.unityObject !== undefined){
					var editorObject= unityService.getUnityObject(_caller.panelInfo.unityObject);
					// wait for unityService to get c# object stub
					editorObject.ready (function (){
						$.extend(_caller,editorObject);
						_extendEnableService();

					    _caller.enabled= false;
					    if (_caller.panelInfo.hideEnableSwitch){
					    	_caller.panelInfo.enabled= true;
					    	_caller.enabled= true;
					    }

						_getServiceEnabled().final(function() {
							// Keep watching for service status change
							$interval(_getServiceEnabled, tickInterval);

							editorObjectPromise.resolve();
						});
					});
				}else{
					editorObjectPromise.resolve();
				}

				return editorObjectPromise.promise;
			};



			_getServiceInfo().then(function() {
				return _getEditorObject();
			}).then(function() {
				return _getPanelSamples();
			}).then(
				function success() {
					serviceReady.resolve();
				},
				function error() {
					serviceReady.reject();
				}
			);

			return serviceReady.promise;
		},

		IsReady: function() {
			return serviceReady.promise;
		},

		canEnableService: function (){
			var perms = unityConnectService.projectInfo.permissions;
			return (!this.panelInfo.limitEnableToOrgOwner || (perms.project === undefined) ||
						((perms.project.value == Roles.owner.value) &&
						 (perms.organizationAccess)));
		},

		getEnvLinkValue: function (linkGroupValue){
			var env= unityConnectService.configuration;

			var link= linkGroupValue;

			if (typeof link == 'object'){
				if(linkGroupValue.hasOwnProperty(env)) {
					link = linkGroupValue[env];
				} else if(linkGroupValue.hasOwnProperty('production')) {
					link = linkGroupValue['production'];
				} else {
					return "";
				}
			}

			if (!_.isString(link)) {
				return link;
			}

			link = link.replace(/%%ENV%%/g, env);
			link = link.replace(/%%UPID%%/g, unityConnectService.projectInfo.projectGUID);
			link = link.replace(/%%ACCESS_TOKEN%%/g, unityConnectService.userInfo.accessToken);
			link = link.replace(/%%ORGID%%/g, unityConnectService.projectInfo.organizationId);

			return link;
		},

		getEnvLink: function (linkGroup){
			// using data found in service.json
			if (this.panelInfo && this.panelInfo.hasOwnProperty(linkGroup)) {
				var linkGrp = this.panelInfo[linkGroup];
	  		return this.getEnvLinkValue(linkGrp);
			}
			return "";
		},

		getEnvLinks: function() {

			var keys = _.keys(this.panelInfo);
			return _.transform(this.panelInfo, function(result, value, key) {
				result[key] = service.getEnvLinkValue(value);
			});
		},

		getContextHelpUrlById: function (id) {
			if (_.has(this.panelInfo, 'contextHelpLinks')) {
				var helpLink = _.find(this.panelInfo.contextHelpLinks, _.matchesProperty('id', id));
				if (_.has(helpLink, 'url')) {
					return helpLink.url;
				}
			}
			return "";
		}
	};

	service.promise= service.initialize();
	return service;
}]);
