angular.module('ngUnity.errors')

.factory('MessageGroup', ["ucStrings", function (ucStrings) {
	function mergeErrorConfig(current, other) {
		var currentClone = _.cloneDeep(current);

		return _.extend(currentClone, other);
	}

	function setKeyLast(object, key) {
		// Place key at the end of the object keys
		var value = object[key];
		delete object[key];
		object[key] = value;
	}

	/**
	 *
	 *
	 * @param config Format {404: 'error', '500-599': 'Server unavailable'}
	 *                            First matched key will be used as a result in case of multiple possible matches (ex: {400:, '300-500'})
	 */
	function MessageGroup(config) {
		this.set(config);
	}

	/**
	 * Normalizes an error config or string to {message:, handler:}
	 * @private
	 */
	MessageGroup.prototype._normalize = function (errorConfig) {
		// Normalize config
		if (_.isString(errorConfig)) {
			errorConfig = {message: errorConfig};
		}

		return errorConfig;
	};

	/**
	 * Overwrite config
	 * @param config
	 * @returns {MessageGroup}
	 */
	MessageGroup.prototype.set = function (config) {
		config = config || {};
		this.config = _.mapValues(config, this._normalize.bind(this));

		return this;
	};

	/**
	 * Extend config
	 * @param config
	 * @param overwrite Whether the new config value should overwrite or extend (default) existing values.
	 * @returns {MessageGroup}
	 */
	MessageGroup.prototype.extend = function (config, overwrite) {
		// Normalize so that extending messages works correctly (ie: 'new message' won't overwrite handler of {message:, handler:}
		var group = new MessageGroup(config);
		var mergeStrategy = overwrite ? undefined : mergeErrorConfig;

		var currentConfig = this.config;
		_.extend(currentConfig, group.config, mergeStrategy);

		// Sort keys by putting ranges last
		_.keys(currentConfig).forEach(function(key) {
			// Place ranges at the end of the object, in the same order as they were found by deleting them and re-adding them.
			// In javascript, object key order is guaranteed to be in order of creation.
			if (_.includes(key, '-')) {
				setKeyLast(currentConfig, key);
			}
		});

		// Unknown always at the very end
		setKeyLast(currentConfig, 'unknown');

		return this;
	};

	/**
	 * Convenience method to clone and extend this group
	 */
	MessageGroup.prototype.extendClone = function (config, overwrite) {
		var clone = this.clone();
		return clone.extend.apply(clone, arguments);
	};

	/**
	 * Deep clone of message group
	 * @returns {MessageGroup}
	 */
	MessageGroup.prototype.clone = function () {
		var config = _.cloneDeep(this.config);

		return new MessageGroup(config);
	};

	/**
	 * Get error message for given status from error code message group
	 *
	 * @param status
	 */
	MessageGroup.prototype.getErrorConfig = function (status) {
		var errorConfig = _.find(this.config, function (errorConfig, range) {
			if (ucStrings.isInRange(range, status)) {
				return errorConfig;
			}
		});

		errorConfig = errorConfig || this.config.unknown;

		return _.cloneDeep(errorConfig);
	}

	return MessageGroup;
}]);
