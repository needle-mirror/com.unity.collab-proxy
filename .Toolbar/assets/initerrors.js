// -----------------------------------------------------------------------------------------------------------------------------------------------------
//		To be as safe as possible, this must be the very first script loaded, before anything else has a chance to report an error.
//		and ideally even inlined in the html file.
//
//		Placed in 'assets' in order to avoid being bundled.
//		Bundling creates a somewhat large file, which on slow connections means there is no feedback to the user while
//		it is loading. Having this script outside of the main bundle (and ideally inlined in index.html) means there is always feedback
//		visible to the user.
// -----------------------------------------------------------------------------------------------------------------------------------------------------
(function () {
	var displayLoadAfter = 500;				// When to display 'Loading...' message -- 1sec
	var displayErrorAfter = 30000;			// When to display cannot load message -- Should be around 30sec to be sure it's not just a slow connection

	function create(htmlStr) {
		var frag = document.createDocumentFragment(),
		temp = document.createElement('div');
		temp.innerHTML = htmlStr;
		while (temp.firstChild) {
			frag.appendChild(temp.firstChild);
		}
		return frag;
	}

	var html = '';
	html += '<delayed-message class="hidden">';
	html += '	<div class="content">';
	html += '		<div class="message">';
	html += '					<span class="text">Loading...</span>';
	html += '					<button class="hidden reload" onclick="location.reload()" style="float:right">Reload</button>';
	html += '		</div>';
	html += '		<div class="hidden console"></div>';
	html += '	</div>';
	html += '</delayed-message>';

	var styleStr = "";
	styleStr += "		.hidden {";
	styleStr += "			display: none;";
	styleStr += "		}";
	styleStr += "";
	styleStr += "		delayed-message {";
	styleStr += "			position: absolute;";
	styleStr += "			top: 0;";
	styleStr += "			left: 0;";
	styleStr += "			width: 100%;";
	styleStr += "			height: 100%;";
	styleStr += "			max-height: 100%;";
	styleStr += "			overflow-y: auto;";
	styleStr += "            text-align: center;";
	styleStr += "";
	styleStr += "			display: flex;";
	styleStr += "			align-items: center;";
	styleStr += "		}";
	styleStr += "";
	styleStr += "		delayed-message .content {";
	styleStr += "			width: 100%;";
	styleStr += "			max-height: 100%;";
	styleStr += "            padding: 16px;";
	styleStr += "		}";
	styleStr += "";
	styleStr += "		delayed-message .message {";
	styleStr += "			display: flex;";
	styleStr += "			align-items: center;";
	styleStr += "			justify-content: center;";
	styleStr += "			padding: 6px;";
	styleStr += "		}";
	styleStr += "";
	styleStr += "		delayed-message button {";
	styleStr += "			border: 1px solid #009aff;";
	styleStr += "		}";
	styleStr += "";
	styleStr += "		delayed-message .console {";
	styleStr += "			border: 1px solid #3f3f3f;";
	styleStr += "			color: orangered;";
	styleStr += "			overflow: auto;";
	styleStr += "			width: 100%;";
	styleStr += "			max-height: 500px;";
	styleStr += "			white-space: pre;";
	styleStr += "			text-align: left;";
	styleStr += "			font-size: 14px;";
	styleStr += "		}";

	var style = document.createElement('style');
	style.type = 'text/css';
	if (style.styleSheet) {
		style.styleSheet.cssText = styleStr;
	} else {
		style.appendChild(document.createTextNode(styleStr));
	}

	document.head.appendChild(style);

	var errorMessage = '';							// Optional error message

	function catchErrors(message, source, lineno, colno, error) {
		errorMessage += message + '\n';

		if (previousOnError) {
			previousOnError.apply(window, arguments)
		}
	}

	var previousOnError = window.onerror;
	window.onerror = catchErrors;

	function isWorking() {
		var isWorking = false;

		if (window.angular) {
			var appElement = window.angular.element(document.querySelector('[ng-app]'))
			var injector = appElement.injector();
			if (injector) {
				isWorking = injector.has('$http');
			}
		}

		return isWorking;
	}

	var showErrorTimeout;
	var showLoadingTimeOut;

	function clearInitErrorMessage() {
		var elems = document.querySelectorAll("delayed-message");
		if (elems.length)
			elems[0].remove();

		clearTimeout(showLoadingTimeOut);
		clearTimeout(showErrorTimeout);
	}
	window._clearInitErrorMessage = clearInitErrorMessage;			// Make available for any app to clear once init is known to be good.

	function getFirstElement(object, query) {
		var elems = object.querySelectorAll(query);
		if (elems.length)
			return elems[0];
	}

	function CheckUnknownError() {
		showErrorTimeout = setTimeout(function () {
			if (!isWorking()) {
				var elem = getFirstElement(document, "delayed-message");
				if (elem) {
					var text = getFirstElement(elem, '.text');
					var reload = getFirstElement(elem, '.reload');
					if (text) text.textContent = 'An unknown error has occured.';
					if (reload) reload.classList.remove('hidden');
				}

				if (errorMessage) {
					var consoleEl = getFirstElement(elem, '.console');
					if (consoleEl) {
						consoleEl.textContent = 'Error:\n\n' + errorMessage;
						consoleEl.classList.remove('hidden');
					}
				}
			} else {
				clearInitErrorMessage();
			}
		}, displayErrorAfter)
	}

	function StartLoadingIndicator() {
		if (!isWorking()) {
			var elem = getFirstElement(document, "delayed-message");
			if (elem)
				elem.classList.remove('hidden');

			CheckUnknownError();
		} else {
			if (window.onerror === catchErrors) {
				window.onerror = previousOnError;
			}
		}
	}

	document.addEventListener("DOMContentLoaded", function (event) {
		var fragment = create(html);
		document.body.insertBefore(fragment, document.body.childNodes[0]);

		showLoadingTimeOut = setTimeout(StartLoadingIndicator, displayLoadAfter);
	});
})();
