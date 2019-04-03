// Polyfill for bind because PhantomJS use an old webkit version
if (!Function.prototype.bind) {
	Function.prototype.bind = function (oThis) {
		if (typeof this !== 'function') {
			// closest thing possible to the ECMAScript 5
			// internal IsCallable function
			throw new TypeError('Function.prototype.bind - what is trying to be bound is not callable');
		}

		var aArgs = Array.prototype.slice.call(arguments, 1),
		fToBind = this,
		fNOP = function () {
		},
		fBound = function () {
			return fToBind.apply(this instanceof fNOP
			? this
			: oThis,
			aArgs.concat(Array.prototype.slice.call(arguments)));
		};

		fNOP.prototype = this.prototype;
		fBound.prototype = new fNOP();

		return fBound;
	};
}

angular.module('ngUnity.unityService', ['ngUnity.unity'])

.config(["$provide", function($provide) {
	$provide.decorator('$q', ["$delegate", function($delegate) {
		// decorating method
		function decoratePromise(promise) {
			var then = promise.then.bind(promise);

			// Overwrite promise.then. Note that $q's custom methods (.catch and .finally) are implemented by using .then themselves, so they're covered too.
			promise.then = function (thenFn, errFn, notifyFn) {
				var result = then(thenFn, errFn, notifyFn);
				return decoratePromise(result);
			};

			promise.success = promise.then.bind(promise);
			promise.error = promise.then.bind(promise, null);
			promise.final = promise.finally.bind(promise);

			return promise;
		}

		var defer = $delegate.defer,
		when = $delegate.when,
		reject = $delegate.reject,
		all = $delegate.all;

		$delegate.defer = function () {
			var deferred = defer();
			decoratePromise(deferred.promise);
			return deferred;
		};

		$delegate.when = function () {
			return decoratePromise(when.apply(when, arguments));
		};

		$delegate.reject = function () {
			return decoratePromise(reject.apply(reject, arguments));
		};

		$delegate.all = function () {
			return decoratePromise(all.apply(all, arguments));
		};

		return $delegate;
	}]);
}])

.factory('unityService', ["$q", "unityObj", "$timeout", function ($q, unityObj, $timeout){

	var unityService= {
		makeStubFunction: function (obj,key){
			return function (){
				var funcDeferred = $q.defer();
				var args= [].slice.apply(arguments);
				args.push(function(err,res){
					if (err){
						funcDeferred.reject(err);
						return;
					}
					funcDeferred.resolve(res);
				});
				obj[key].apply(obj,args);
				return funcDeferred.promise;
			};
		},
		getUnityObject: function (objectPath){

			var ngStub = {
				readyCalled: false
			};

			var deferred = $q.defer();
			unityObj.getUnityObject(objectPath, function(err,obj){
				if (err){
					deferred.reject(err);
					return;
				}
				for (var key in obj){
					if (obj.hasOwnProperty(key)){
						if (typeof obj[key] == "function"){
							ngStub[key+"Sync"] = obj[key];
							ngStub[key]= unityService.makeStubFunction (obj, key);
						}else{
							ngStub[key]= obj[key];
						}
					}
				}

				deferred.resolve(ngStub);
			});

			ngStub.promise= deferred.promise;

			ngStub.ready= function (a_func){
				// For some currently unknown reasons, the object is sometimes not yet ready, and becomes ready a few milliseconds later.
				// When this happens, any usage of the unityObject's method will throw since none exists, which results in blank screens
				// as app initialization isn't functioning.
				// This attempts to weakly work around this issue by re-trying the ready call just a little later.
				function callWithRetry(err) {
					try {
						a_func(err);
					} catch (error) {
						console.error('Error retrying unity object initialization', error);		// Make sure error doesn't go unreported...

						$timeout(function() {
							a_func(err);
						}, 1000);
					}
				}

				if (ngStub.readyCalled){
					callWithRetry();
					return;
				}
				ngStub.promise.
					success(function (promiseResult){
						ngStub.readyCalled= false;
						callWithRetry(null);
					}).
					error(function(err) {
						callWithRetry(err);
					});
			};

			return ngStub;
		}
	};

	return unityService;
}]);
