angular.module('ngUnity.cloudcollab')
.factory('errorHandlerInterface', ["$q", "utAlert", "defaultHttpErrors", "MessageGroup", "toArray", "httpErrorConfigs", function ($q, utAlert, defaultHttpErrors, MessageGroup, toArray, httpErrorConfigs) {
	/**
	 * Retrieves a message from a message group and a status
	 *
	 * @param status
	 * @param overwrites Object Message Group.
	 * @param defaults MessageGroup list of defaults messages if none are found in overwrites
	 * @returns {*}
	 */
	function httpErrorMessages(status, overwrites, defaults) {
		overwrites = overwrites || new MessageGroup;

		var errorConfig = overwrites.getErrorConfig(status) || defaults.getErrorConfig(status);

		errorConfig.handler = errorConfig.handler || angular.noop;
		errorConfig.handler = toArray(errorConfig.handler);

		// Add global handlers
		var all = overwrites.all || defaults.all;
		if (all) {
			errorConfig.handler.push(all.handler);
		}

		return errorConfig;
	}


	return function (origin, promise) {
		// Default MessageGroup to handle errors
		var defaults = httpErrorConfigs[origin.method] || defaultHttpErrors;

		return {
			/**
			 * Error handler interface to be added to promises
			 *
			 * @param messages Object Message group format {400: 'my error message', ...}
			 * @returns {promise}
			 */
			handleErrors: function (messages) {
				return promise.error(function (error) {
					var status;
					if (error && error.status !== undefined) {
						status = error.status || error;
					}

					var errorConfig = httpErrorMessages(status, messages, defaults);

					if (status && errorConfig) {
						if (errorConfig.message) {
							utAlert.setError(errorConfig.message);
						}

						errorConfig.handler.forEach(function (handler) {
							handler(error, origin);
						});
					}

					return $q.reject(error);			// Keep error chain going as we haven't recovered.
				});
			}
		};
	};
}]);
