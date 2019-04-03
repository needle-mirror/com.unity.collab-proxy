angular.module('ut.services.notification')
  .service('socketNotifications', ["routes", function(routes) {
    this.channels = {
      'CHANGES_TO_PUBLISH_CHANNEL': routes.COLLAB_LOCAL_CHANGES_ROUTE,
      'COLLAB_INFO_CHANNEL': routes.COLLAB_INFO_ROUTE,
      'COLLAB_REVISIONS_UPDATE_CHANNEL': routes.COLLAB_REVISIONS_UPDATE_ROUTE,
      'COLLAB_ERROR_CHANNEL': routes.COLLAB_ERROR_ROUTE,
      'CLOUD_CONNECT_INFO_CHANNEL': routes.CLOUD_CONNECT_INFO_ROUTE,
    };

    var subscriptions = {};

    var dispatch = function(message) {
      var info = JSON.parse(message.data);
      if(!subscriptions.hasOwnProperty(info.channel)) {
        return;
      }
      subscriptions[info.channel].forEach(function(messageCallback){
        messageCallback(info.data);
      });
    };

    this.subscribe = function(channel, messageCallback) {
      if(!subscriptions.hasOwnProperty(channel)) {
        subscriptions[channel] = [];
      }
      subscriptions[channel].push(messageCallback);
    };

    // Initialize
    this.init = function(deps) {
      var collabSocket, connectSocket;
      collabSocket = new WebSocket(deps.rest_url.replace('http','ws') + routes.COLLAB_WEBSOCKET_ROUTE);
      connectSocket = new WebSocket(deps.rest_url.replace('http','ws') + routes.CONNECT_WEBSOCKET_ROUTE);
      collabSocket.onmessage = connectSocket.onmessage = dispatch;
    };
  }]);
