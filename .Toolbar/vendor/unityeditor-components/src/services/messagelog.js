angular
  .module('ut.unityeditor-components.message-log', [])
  .factory('messageLog', function () {

      // Regex for extracting important context that should be prepended to the normal message
      var PRE_REGEX = /(shader error)/i;
      // Regex for extracting just the useful/repeated parts of error log messages
      var CONTENT_REGEX = /(?:warning ([\w]+: *[\w\W]*$))|(?:[\w]+: *`([\w\W]*$))|(?:'[\w\/]+': ([\w\W]+) at line [\d]+)/;
      // If the message meets this regex then it is just repeating what we already know and shouldn't be included in top-level reports
      var REJECT_REGEX = /(?:Error building [\w]+: [\d]+ errors)/;
      // Variables for current usage, these could be altered or removed with future research
      var ERRORS = [
        0, // Errors
        1, // Assert failures in Unity
        4 // Exceptions
      ];
      var WARNINGS = [2];
      var contentField = 'content';
      var typeField = 'type';

      // Setters for (currently theoretical) reusability
      function setContentField(field) {
          contentField = field;
      }

      function setTypeField(field) {
          typeField = field;
      }

      function setTypeSelector(fn) {
          selectByType = fn;
      }

      // This method could be overridden by anything which fulfils the same contract
      function selectByType(messages, type) {
          if (type === 'warning') {
              type = WARNINGS;
          } else {
              type = ERRORS;
          }
          return _.filter(messages, function (msg) {
            return type.indexOf(msg[typeField]) > -1;
          });
      }

      function getMessage(message) {
          var preMatch = message[contentField].match(PRE_REGEX);
          // Strip off irrelevant details to get something that's likely to be repeated
          var content = getMessageContent(message);
          if (content) {
              var preMatchContent = preMatch ? preMatch[1] + ': ' : '';
              return preMatchContent + content;
          }
          // Can't identify a repeatable message, just push out the entire message
          return message[contentField];
      }

      function getMessageContent(message) {
          var match = message[contentField].match(CONTENT_REGEX);
          if (match) {
              for (var i = 1; i < match.length; i++) {
                  if (match[i]) {
                      return match[i];
                  }
              }
          }
      }

      function rejectMessage(message) {
          return message[0].match(REJECT_REGEX);
      }

      function sortByValue(value) {
          return value[1];
      }

      function getSortedCount(messages, messageType) {
          var separated = selectByType(messages, messageType);
          // This is the part that's least likely to be reusable as written, you could use a clever selectByType method
          // or you could just allow users to override this method rather than the selectByType method (though that might
          // defeat the purpose of making this service reusable at all).
          return _.chain(separated)
            .countBy(getMessage)
            .pairs()
            .reject(rejectMessage)
            .sortBy(sortByValue)
            .value()
            .reverse();
      }

      function sortMessages(messages) {
          var errors = getSortedCount(messages, 'error');
          var errorObjs = [];
          errors.forEach(function (error) {
              errorObjs.push({
                  message: error[0],
                  count: error[1],
                  type: 'error'
              });
          });
          var warnings = getSortedCount(messages, 'warning');
          var warningObjs = [];
          warnings.forEach(function (warning) {
              warningObjs.push({
                  message: warning[0],
                  count: warning[1],
                  type: 'warning'
              });
          });
          return {
              errors: errorObjs,
              warnings: warningObjs
          };
      }

      return {
          sortMessages: sortMessages,
          setContentField: setContentField,
          setTypeField: setTypeField,
          setTypeSelector: setTypeSelector
      };
  });
