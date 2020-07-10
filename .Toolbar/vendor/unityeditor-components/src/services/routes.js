angular.module('ut.routes', [])
  .service('routes', function() {
    // Generic Service Routes
    this.COLLAB_WEBSOCKET_ROUTE = '/unity/services/collab/notifications',
    this.CONNECT_WEBSOCKET_ROUTE = '/unity/services/connect/notifications',
    // Collab Service Routes
    this.COLLAB_INFO_ROUTE = '/unity/service/collab/info',
    this.COLLAB_LOCAL_CHANGES_ROUTE = '/unity/service/collab/localChanges',
    this.COLLAB_REVISIONS_UPDATE_ROUTE = '/unity/service/collab/revisions/update',
    this.COLLAB_ERROR_ROUTE = '/unity/service/collab/error',
    // Connect Service Routes
    this.CLOUD_CONNECT_INFO_ROUTE = '/unity/service/cloud/connectInfo'
  });
