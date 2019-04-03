module.exports = function ( karma ) {
  karma.set({
    /**
     * From where to look for files, starting with the location of this file.
     */
    basePath: '../',

    /**
     * This is the list of file patterns to load into the browser during testing.
     */
    files: [
      'vendor/jquery/dist/jquery.js',
      'vendor/bootstrap/dist/js/bootstrap.js',
      'vendor/angular/angular.js',
      'vendor/angular-bootstrap/ui-bootstrap-tpls.js',
      'vendor/angular-ui-router/release/angular-ui-router.js',
      'vendor/angular-resource/angular-resource.js',
      'vendor/angular-animate/angular-animate.js',
      'vendor/prism/prism.js',
      'vendor/prism/components/prism-clike.js',
      'vendor/prism/components/prism-csharp.js',
      'vendor/prism/components/prism-java.js',
      'vendor/prism/components/prism-c.js',
      'vendor/prism/components/prism-objectivec.js',
      'vendor/lodash/lodash.js',
      'vendor/spin.js/spin.js',
      'vendor/ladda-bootstrap/dist/ladda.js',
      'vendor/angular-spinner/angular-spinner.js',
      'vendor/angular-loading-bar/build/loading-bar.js',
      'vendor/angular-cookies/angular-cookies.js',
      'vendor/ngstorage/ngStorage.js',
      'vendor/unityeditor-components/src/components/alert/alertDirective.js',
      'vendor/unityeditor-components/src/components/alert/alertService.js',
      'vendor/unityeditor-components/src/components/button/button.js',
      'vendor/unityeditor-components/src/components/checkbox/checkbox.js',
      'vendor/unityeditor-components/src/components/cloudbuild.js',
      'vendor/unityeditor-components/src/components/cloudbuild/constants.js',
      'vendor/unityeditor-components/src/components/cloudbuild/history.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/credentials/credentials_android.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/credentials/credentials_base.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/credentials/credentials_ios.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/credentials/validate_bundleid_android.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/credentials/validate_bundleid_ios.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/project/basic.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/scm/scmDirective.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/scm/scmService.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/scm/scm_base.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/scm/step01.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/scm/step02.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/target/advanced.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/target/assetbundles.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/target/basic.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/target/platform_select.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/target/scm/svn.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/target/target_edit.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/target/tests.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/target/unityversion.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/target/validate_subdirectory.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/util/file_model.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/util/setup_constants.js',
      'vendor/unityeditor-components/src/components/cloudbuild/setup/util/utils.js',
      'vendor/unityeditor-components/src/components/progress/progress.js',
      'vendor/unityeditor-components/src/components/switch/switch.js',
      'vendor/unityeditor-components/src/components/wizard-progress/wizard-progress.js',
      'vendor/unityeditor-components/src/core/core.js',
      'vendor/unityeditor-components/src/main.js',
      'vendor/unityeditor-components/src/services/cloudbuild.js',
      'vendor/unityeditor-components/src/services/cloudbuildstatus.js',
      'vendor/unityeditor-components/src/services/config.js',
      'vendor/unityeditor-components/src/services/messagelog.js',
      'vendor/unityeditor-components/src/services/notifications.js',
      'vendor/unityeditor-components/src/services/pubnub.js',
      'vendor/unityeditor-components/src/services/routes.js',
      'vendor/unityeditor-components/src/services/socket.js',
      'vendor/unityeditor-components/src/shared/mocks/mockCloudBuildService.js',
      'vendor/unityeditor-cloud-pack/libs/cloudbuild/cloudbuild.js',
      'vendor/unityeditor-cloud-pack/libs/cloudbuild/cloudbuildui.js',
      'vendor/unityeditor-cloud-pack/libs/cloudcollab/cloudcollab.js',
      'vendor/unityeditor-cloud-pack/libs/cloudcollab/errors.js',
      'vendor/unityeditor-cloud-pack/libs/cloudcore/cloudcore.js',
      'vendor/unityeditor-cloud-pack/libs/cloudcore/errors.js',
      'vendor/unityeditor-cloud-pack/libs/cloudcredentials/cloudcredentials.js',
      'vendor/unityeditor-cloud-pack/libs/cloudcredentials/errorhandler.js',
      'vendor/unityeditor-cloud-pack/libs/cloudcredentials/headerauth.js',
      'vendor/unityeditor-cloud-pack/libs/common/components/icons.js',
      'vendor/unityeditor-cloud-pack/libs/common/index.js',
      'vendor/unityeditor-cloud-pack/libs/directives.js',
      'vendor/unityeditor-cloud-pack/libs/editorcollab/editorcollab.js',
      'vendor/unityeditor-cloud-pack/libs/editorcollab/editorcollabREST.js',
      'vendor/unityeditor-cloud-pack/libs/editorcollab/editorcollabnative.js',
      'vendor/unityeditor-cloud-pack/libs/errors/http.js',
      'vendor/unityeditor-cloud-pack/libs/errors/messagegroup.js',
      'vendor/unityeditor-cloud-pack/libs/errors/ui.js',
      'vendor/unityeditor-cloud-pack/libs/filters.js',
      'vendor/unityeditor-cloud-pack/libs/general/general.js',
      'vendor/unityeditor-cloud-pack/libs/google-analytics.js',
      'vendor/unityeditor-cloud-pack/libs/services/clipboardService.js',
      'vendor/unityeditor-cloud-pack/libs/services/cloudPanelService.js',
      'vendor/unityeditor-cloud-pack/libs/services/config.js',
      'vendor/unityeditor-cloud-pack/libs/services/connectService.js',
      'vendor/unityeditor-cloud-pack/libs/services/projectService.js',
      'vendor/unityeditor-cloud-pack/libs/services/projectUnityVersion.js',
      'vendor/unityeditor-cloud-pack/libs/services/unity.js',
      'vendor/unityeditor-cloud-pack/libs/services/unityService.js',
      'vendor/unityeditor-cloud-pack/libs/status/status.js',
      'vendor/unityeditor-cloud-pack/libs/tooltips/tooltips.js',
      'vendor/unityeditor-cloud-pack/libs/utClasses.js',
      'vendor/unityeditor-cloud-pack/libs/utils.js',
      'vendor/angulartics/dist/angulartics.min.js',
      'vendor/angulartics/dist/angulartics-ga.min.js',
      'vendor/airbrake-js-client/dist/client.js',
      'vendor/moment/min/moment.min.js',
      'vendor/angular-timeago/dist/angular-timeago.js',
      'vendor/pubnub/dist/web/pubnub.js',
      'vendor/angular-sanitize/angular-sanitize.min.js',
      'vendor/angular-aria/angular-aria.min.js',
      'vendor/angular-material/angular-material.js',
      'vendor/angular-svg-round-progressbar/build/roundProgress.min.js',
      'build/templates-app.js',
      'build/templates-common.js',
      'vendor/angular-mocks/angular-mocks.js',
      'vendor/unityeditor-cloud-pack/libs/testing/mocks.spec.js',
      
      'src/**/*.js'
    ],
    exclude: [
      'src/assets/**/*.js'
    ],
    frameworks: [ 'jasmine' ],

    plugins: [
      'karma-jasmine',
      'karma-phantomjs-launcher',
      'karma-coverage',
      'karma-spec-reporter'
    ],

    preprocessors: {
      // source files, that you wanna generate coverage for
      // do not include tests or libraries
      // (these files will be instrumented by Istanbul)
      'src/**/!(*spec).js': ['coverage']
    },

    /**
     * How to report, by default.
     */
    reporters: ['spec', 'coverage'],

    specReporter: {
      maxLogLines: 5,
      suppressErrorSummary: true,
      suppressFailed: false,
      suppressPassed: false,
      suppressSkipped: false,
      showSpecTiming: true
    },

    coverageReporter: {
      reporters: [
        {
          type : 'html',
          dir : 'coverage/',
          subdir: 'report'
        },
        {
          type: "lcovonly",
          dir: "coverage/",
          subdir: 'lcov'
        }
      ]
    },

    /**
     * On which port should the browser connect, on which port is the test runner
     * operating, and what is the URL path for the browser to use.
     */
    port: 9018,
    runnerPort: 9100,
    urlRoot: '/',

    /**
     * Disable file watching by default.
     */
    autoWatch: false,

    /**
     * The list of browsers to launch to test on. This includes only "Firefox" by
     * default, but other browser names include:
     * Chrome, ChromeCanary, Firefox, Opera, Safari, PhantomJS
     *
     * Note that you can also use the executable name of the browser, like "chromium"
     * or "firefox", but that these vary based on your operating system.
     *
     * You may also leave this blank and manually navigate your browser to
     * http://localhost:9018/ when you're running tests. The window/tab can be left
     * open and the tests will automatically occur there during the build. This has
     * the aesthetic advantage of not launching a browser every time you save.
     */
    browsers: [
        "PhantomJS"
    ]
  });
};
