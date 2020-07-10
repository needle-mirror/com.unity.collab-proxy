angular.module('ngUnity.general', [])
/**
 * Promise Utilities
 */
.factory('ucPromise', function () {
	/**
	 * Decorate a single method
	 *
	 * @param promise
	 * @param methodName
	 * @param decorator
	 */
	function decorateMethod(promise, methodName, decorator) {
		var method = promise[methodName];
		promise[methodName] = function () {
			var promise = method.apply(promise, arguments);
			decorator(promise);

			return promise;
		};
	}

	/**
	 * Decorate a promise's success/error with an interface
	 *
	 * @param extensionInterfaceFactory
	 * @param promise
	 * @returns {*}
	 */
	function decoratePromise(extensionInterfaceFactory, promise) {
		_.extend(promise, extensionInterfaceFactory(promise));

		var decorator = decoratePromise.bind(this, extensionInterfaceFactory);

		decorateMethod(promise, 'success', decorator);
		decorateMethod(promise, 'error', decorator);

		return promise;
	}

	return {
		decorateMethod: decorateMethod,
		decoratePromise: decoratePromise
	};
})

/**
 * String utilities
 */
.factory('ucStrings', function () {
	/**
	 * Takes a string with format '200-400,404,403' and return true if given number is found in this group
	 */
	function isInRange(rangeString, number) {
		var groups = rangeString.split(',');

		return _.some(groups, function (group) {
			var range = group.split('-').map(function (s) {
				return parseInt(s);
			});

			if (range.length === 1) {
				return range[0] === number;
			} else {
				return _.inRange(number, range[0], range[1]);
			}
		});
	}

	return {
		isInRange: isInRange
	};
})

/**
 * Make sure object is an array (will create an array of one element if it's not..)
 */
.factory('toArray', function() {
	return function toArray(object) {
		if (!_.isArray(object)) {
			if (_.isUndefined(object)) {
				object = [];
			} else {
				object = [object];
			}
		}

		return object;
	};
})
/**
 * Create an event from data updated regularly possibly with/without change  (stream/interval)
 */
.factory('StreamEvent', ["$rootScope", function ($rootScope) {
	function StreamEvent(eventName) {
		this._eventName = eventName;
		this._previous = Infinity;
	}

	StreamEvent.prototype.update = function (value) {
		if (value !== this._previous) {
			$rootScope.$broadcast(this._eventName, value);

			this._previous = value;
		}
	};

	return StreamEvent;
}]);
