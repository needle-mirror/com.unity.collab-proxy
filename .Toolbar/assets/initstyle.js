(function() {
	window.unityGlobal = {
		/**
		 * Parse user agent to find out what theme is currently set on unity...
		 */
		getTheme: function () {
			// useragent format: "Mozilla/5.0 (MacIntel; ) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2062.94 Safari/537.36 Unity/5.6.0a1 (unity3d.com;dark)"
			var regExp = /\(([^)]+)\)/ig;
			var matches;
			var theme;

			// Go through every match (all parenthesis groups)
			while (matches = regExp.exec(navigator.userAgent)) {
				// If we're at the right parenthesis...
				if (matches.length && matches[0].indexOf('unity3d.com') !== -1) {
					// Split in tokens (using ';' as separator)
					var content = matches[0]
					content = content.replace('(', '');
					content = content.replace(')', '');
					var tokens = content.split(';');
					if (tokens.length >= 2) {
						theme = tokens[1];
					}
				}
			}

			return theme || 'dark';
		},

		getStyleSheetPath: function () {
			var elem = document.querySelector(".main-theme");
			var path;
			if (elem)
				path = elem.getAttribute('data-path');

			return path;
		},

		getFullStyleSheetPath: function () {
			var theme = this.getTheme();
			var path = this.getStyleSheetPath();

			if (path)
				path = path + theme + '.css';

			return path;
		},

		setDefaultStyle: function () {
			var stylesheet = this.getFullStyleSheetPath()

			var elem = document.querySelector(".main-theme");
			if (elem && stylesheet) {
				elem.href = stylesheet;
			} else {
				console.warn('Main theme stylesheet not found.');
			}
		}
	};

	// This needs to be set really, really early when page is shown, since otherwise there will be a brief "flash" (white or black)
	// when reloading pages.
	window.unityGlobal.setDefaultStyle();
}());
