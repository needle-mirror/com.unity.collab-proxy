var entryPoint = function (functionName, param) {
    if (window.angular &&
        angular.element &&
        angular.element(document.getElementById('collab-toolbar')).injector() &&
        angular.element(document.getElementById('collab-toolbar')).injector().get('$rootScope')) {
        // function call
        angular.element(document.getElementById('collab-toolbar')).injector().get('$rootScope')[functionName](param);
    }
};
