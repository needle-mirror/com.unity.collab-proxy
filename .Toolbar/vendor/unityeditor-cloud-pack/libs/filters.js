angular.module('ngUnity.filters', [])

.filter('formatCode', function() {
  return function(input) {
		return "FMT:"+input;
  };
});