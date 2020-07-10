angular.module('ut.services.notification')
.service('pubnubNotifications', ["$q", "$http", "$interval", function($q, $http, $interval) {
  var self = this;
  var channelInfo, projectId, fullUrl;
  var PUBNUB_PROBE_INTERVAL_SECONDS = 30;

  this.pubnub = null;
  this.PUBNUB_DISCONNECTED = 0;
  this.PUBNUB_CONNECTING = 1;
  this.PUBNUB_CONNECTED = 2;
  this.pubnubStatus = this.PUBNUB_DISCONNECTED;

  this.channels = {
    'USER_CHANNEL': 'user',
    'COMMIT_CHANNEL': 'commits',
    'SOFTLOCK_CHANNEL': 'softlocks'
  }

  this.pubnubErrorCallBack = function (error) {
    self.pubnubStatus = self.PUBNUB_DISCONNECTED;
    if (self.pubnub) {
      self.pubnub.unsubscribe({channel: channelInfo.channels[self.channel]});
      delete(self.pubnub);
    }
  };

  // Call the service to obtain keys and use them to initialize pubnub
  this.probePubnub = function() {
    if (!self.channel || !projectId || !fullUrl) {
      return;
    }

    if (self.pubnubStatus != self.PUBNUB_DISCONNECTED) {
      return;
    }

    self.pubnubStatus = self.PUBNUB_CONNECTING;
    self.refreshChannelAccess().then(function success(data) {
      // Use the data to set up pubnub
      channelInfo = data;

      self.pubnub = PUBNUB.init({
          subscribe_key: channelInfo.subscribe_key,
          auth_key: channelInfo.access_key,
          ssl: true
      });
    
      self.pubnub.subscribe({
        channel: channelInfo.channels[self.channel],
        // We define the connect callback to ensure that the application
        // hasn't missed any notifications in the period in which the application
        // attempts to connect to pubnub.
        connect: self.messageCallback,
        message: self.messageCallback,
        error: self.pubnubErrorCallBack
      });
      self.pubnubStatus = self.PUBNUB_CONNECTED;
    }, function failure(err) {
      self.pubnubStatus = self.PUBNUB_DISCONNECTED;
    });
    return;
  };

  this.refreshChannelAccess = function () {
    var deferred = $q.defer();
    $http.post(fullUrl, {project_id: projectId})
      .success(function(response) {
        deferred.resolve(response);
      })
      .error(function(status) {
        deferred.reject(status);
      });
    return deferred.promise;
  };

  // Initialize pubnub and then pass to subscribeInternal
  this.subscribe = function(channel, messageCallback) {
    // Cancel this too
    self.channel = channel;
    self.messageCallback = messageCallback;
    self.probePubnub();
  };

  this.init = function(deps) {
    projectId = deps.project_id;
    fullUrl = deps.full_url;
    $interval(self.probePubnub, PUBNUB_PROBE_INTERVAL_SECONDS * 1000);
  };
}]);
