angular.module('ngUnity.tooltips', [])
.run(function () {
	var body = $('body');
	if (!body.find('#tooltips').length) {
		// Configure tooltips -- Make sure they can appear on body correctly
		body.append('<div id="tooltips"><div class="uni-tooltip"></div></div>');
	}
})
.directive('ucTooltip', function () {
	// Until a better solution comes along, this removes the tooltip when it's related element is hidden/removed.
	function removeTooltip() {
		$('#tooltips .tooltip').remove();
	}

	return {
		link: function (scope, element, attrs) {
			element.attr('title', element.attr('title') || attrs.ucTooltip);
			element.addClass('uni-tooltip');
			var delay = attrs['ucTooltipDelay'] || 1000;
			delay = parseInt(delay);

			$('#tooltips').addClass('uni-tooltip');

			var el = $(element).tooltip({
				animation: false,
				delay: {
					show: delay,
					hide: 0
				},
				placement: 'top',
				container: '#tooltips'
			});

			$(element).on('remove', removeTooltip);
			scope.$on('$destroy', removeTooltip);
		}
	};
});
