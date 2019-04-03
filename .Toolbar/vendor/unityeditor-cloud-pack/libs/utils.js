// utils
utils= {
	/** @ngInject*/
	servicePromises: ["$q", "unityProjectService", "unityConnectService", "unityCloudPanelService", "unityClipboardService", function ($q, unityProjectService, unityConnectService, unityCloudPanelService, unityClipboardService) {
		return $q.all([
			unityProjectService.promise,
			unityConnectService.promise,
			unityCloudPanelService.promise,
			unityClipboardService.promise
		]).then(function() {
			// Leave here to allow debug breaks.
		});
	}],

	/**
	 *
	 * @returns {*} State
	 */
	makeDefaultState: function (stateName, objectPanelUrl, extraViews, extraResolves){
		var stateViews= {
		    header:{
		        template: '<uc-panel-header></uc-panel-header>'
		    },
			status: {
				controller: 'StatusCtrl',
				templateUrl: 'libs/status/status.tpl.html'
			}
		};

		var resolves = {
			/** @ngInject*/
			servicePromise: utils.servicePromises
		};

		$.extend(stateViews, extraViews);
		$.extend(resolves, extraResolves);

		return {
			name: stateName,
			url: objectPanelUrl,
			abstract: true,
			resolve: resolves,
			views: stateViews
		};
	},

	makeSampleLoader: function ($q,$http, sampleUrl){
		
		return function (){
		  var deferred = $q.defer();
			var config = {
				cache: false
			};
		  $http.get (sampleUrl+"samples.json", config)
		    .success(function(samples, status, headers) {
		        var promises= [];
		        for (var i=0; i<samples.length;i++){
		          promises.push($http.get (sampleUrl+samples[i].ref, config));
		        }
		        $q.all(promises)
		          .then(function(result){
		              for (var i=0; i<result.length;i++){
		                samples[i].code= result[i].data;
		              }
		              deferred.resolve(samples);
		          },function(err){
		              deferred.reject(err.status);
		          });
		    }).
		    error(function(data, status, headers, config) {
		        return deferred.reject(status);
		    });

		  return deferred.promise;
		};
	}
};
