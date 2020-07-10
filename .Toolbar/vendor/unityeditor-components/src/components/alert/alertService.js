angular.module('ut.alert')
.factory('utAlert', ["$q", "$timeout", function ($q, $timeout){

	var service = {
		alertText: '',
		alertType: '',
		alertCode: 0,
		showMoreInfo: false,
		linkHandler: null,
		linkPreText: "For more information, please click",
		linkText: "here",
		linkSuffixText: ".",
		linkUrl: '',
		clearHandler: null,
		id: 0
	};

	service.setOptions = function (options) {
		this.id++;

		if (options != null) {
			this.showMoreInfo = options.showMoreInfo;
			if (options.linkHandler != null) {
				this.linkHandler = options.linkHandler;
				if (options.linkPreText != null && options.linkText != null && options.linkSuffixText != null) {
						this.linkPreText = options.linkPreText;
						this.linkText = options.linkText;
						this.linkSuffixText = options.linkSuffixText;
				}
			}
			if (options.linkUrl != null) {
				this.linkUrl = options.linkUrl;
			}
			if (options.clearHandler != null && (typeof options.clearHandler == 'function')) {
				this.clearHandler = options.clearHandler;
			}
			if (options.id) {
				this.id = options.id;
			}
		}
	}

	service.setCritical = function (text, code, options) {
		return this.set({text: text, code: code, options: options, type: 'critical'});
	};

	service.setError = function (text,code, options){
		return this.set({text: text, code: code, options: options, type: 'danger'});
	};

	service.setWarning = function (text, code, options){
		return this.set({text: text, code: code, options: options, type: 'warning'});
	};

	service.setInfo = function (text, code, options){
		return this.set({text: text, code: code, options: options, type: 'info'});
	};

	service.setSuccess = function (text, code, options){
		return this.set({text: text, code: code, options: options, type: 'success'});
	};

	/**
	 * Generic service set function
	 * @param {Object} params
	 * @param {string} params.text
	 * @param {number} params.code
	 * @param {Object} params.options
	 */
	service.set = function(params) {
		this.alertText = params.text;
		this.alertCode = params.code;
		this.alertType = params.type;
		this.setOptions(params.options);
		this.closeable = _.isBoolean(params.closeable) ? params.closeable : true;

		if (params.autoClear) {
			this.autoClear(params.autoClear);
		}

		return this;
	};

	service.clearAlert = function (){
		this.alertText = '';
		this.alertType = '';
		this.alertCode = 0;
		this.showMoreInfo = false;
		this.closeable = true;
		if (this.clearHandler)
			this.clearHandler();
	};

	service.clearAlertById = function(id) {
		if (this.id === id) {
			this.clearAlert();
			return true;
		}
	};

	service.setConfig = function(options){
		this.linkHandler = options.linkHandler;
		this.linkUrl = options.linkUrl;
	}

	/**
	 * Automatically clear the alert after a delay
	 * Param format is one of:
	 * 		true
	 * 		{delay, callback}
	 * 		delay, callback
	 *
	 * @param delay Optional
	 * @param callback Optional
	 */
	service.autoClear = function(delay, callback) {
		// Standardize arguments
		if (_.isBoolean(delay)) {
			if (delay === false) {
				return;
			}

			delay = undefined;
		} else if (_.isObject(delay)) {
			callback = delay.callback;
			delay = delay.delay;
		}

		// Setup auto-clear alert

		var autoClearId = this.id;			// Keep id of current alert
		callback = callback || this.clearHandler ? this.clearHandler.bind(this) : this.clearAlert.bind(this);
		delay = delay || 4000;

		this.automaticAlert = $timeout(function () {
			// Only clear if it's the same alert
			if (this.id === autoClearId) {
				callback(this);
			}
		}.bind(this), delay);

		return this.automaticAlert;			// Return timeout to allow cancelling
	};

	service.cancelAutoClear = function() {
		$timeout.cancel(this.automaticAlert);
		this.automaticAlert = undefined;
	};

	return service;
}]);
