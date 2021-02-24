(function () {
    'use strict';

    function configController($scope, dateHelper, userService,
        uSyncPublishService, uSyncActionManager) {

        // publisher vm
        var pvm = this;

        // config from the parent. 
        pvm.mode = $scope.vm.mode;
        pvm.isMedia = $scope.vm.isMedia;
        // options stored on the parent.
        pvm.stepArgs = $scope.vm.stepArgs;
        pvm.flags = $scope.vm.flags;
        pvm.action = $scope.vm.currentAction();

        // used in the UI, not the logic.
        pvm.server = $scope.vm.selectedServer;

        pvm.showtoggle = pvm.server.HideAdvanced;
        pvm.showAdvanced = !pvm.showtoggle;

        if ($scope.model != null) {
            $scope.model.title = pvm.server.Name + ': Settings';
            $scope.model.subtitle = uSyncActionManager.getDescription(pvm.mode, pvm.isMedia, pvm.server.Name);
        }

        pvm.contentName = pvm.isMedia ? $scope.vm.content.name : $scope.vm.content.variants[0].name;
       
        calculateTreeSize();

        InitOptions();

        ///////
        function calculateTreeSize() {
            pvm.largeTree = false;
            if (!pvm.isMedia && !$scope.vm.isRoot) {
                uSyncPublishService.getDecendentCount($scope.vm.content.id).then(function (result) {
                    pvm.largeTree = result.data > 1000;
                });
            }
        }

        function InitOptions() {


            pvm.stepArgs.options = {
                items: [{
                    id: $scope.vm.content.id,
                    name: pvm.contentName,
                    udi: $scope.vm.content.udi,
                    flags: uSyncPublishService.getFlags(pvm.flags)
                }],
                removeOrphans: pvm.flags.deleteMissing.value,
                includeFileHash: pvm.flags.includeFiles.value
            };

            // when the flags change.
            $scope.$watch('pvm.flags', function (newVal, oldVal) {
                if (newVal !== undefined) {
                    pvm.stepArgs.options.items[0].flags = uSyncPublishService.getFlags(pvm.flags);
                    pvm.stepArgs.options.deleteMissing = pvm.flags.deleteMissing.value;
                    pvm.stepArgs.options.includeFileHash = pvm.flags.includeFiles.value;
                }
            }, true);

            pvm.showtoggle = shouldShowAdvanced();
            pvm.showAdvanced = !pvm.showtoggle;
        }

        function shouldShowAdvanced() {

            if (!pvm.server.HideAdvanced) return false; // TODO: remove ?

            if (pvm.action.ActionOptions['canSchedule'] === true) return true;

            for (var flag in pvm.flags) {
                if (flag != 'includeChildren' && flag !== 'deleteMissing') {
                    if (pvm.flags[flag].toggle == true) {
                        return true;
                    }
                }
            }

            return false;
        }


        $scope.$on('sync-server-selected', function (event, args) {
            pvm.server = args.server;
        });


        //// schedules publishing stuff
        // pvm.datePickerConfig = {};

        pvm.stepArgs.options.attributes = {};
        pvm.stepArgs.options.attributes.releaseDate = null;

        pvm.currentUser = null;
        pvm.releaseDateFormatted = null;

        pvm.datePickerSetup = datePickerSetup;
        pvm.datePickerChange = datePickerChange;
        pvm.datePickerShow = datePickerShow;
        pvm.datePickerClose = datePickerClose;
        pvm.clearPublishDate = clearPublishDate;

        pvm.flatPickr = null;

        function datePickerSetup(instance) {
            pvm.flatPickr = instance;
        }

        function datePickerChange(date) {
            if (!date) { return; }
            var serverTime = dateHelper.convertToServerStringTime(moment(date), Umbraco.Sys.ServerVariables.application.serverTimeOffset);
            pvm.releaseDate = serverTime;
            pvm.releaseDateFormatted = dateHelper.getLocalDate(pvm.releaseDate, pvm.currentUser.locale, "MMM Do YYYY, HH:mm");

            pvm.stepArgs.options.attributes.releaseDate = serverTime;
        }

        function datePickerShow() {

        }

        function datePickerClose() {

        }

        function clearPublishDate() {

            pvm.stepArgs.options.attributes["releaseDate"] = null;

            pvm.releaseDate = null;

            // we don't have a publish date anymore so we can clear the min date for unpublish
            var now = new Date();
            var nowFormatted = moment(now).format("YYYY-MM-DD HH:mm");
            pvm.flatPickr.set("minDate", nowFormatted);
        }

        // get current backoffice user and format dates
        userService.getCurrentUser().then(function (currentUser) {

            pvm.currentUser = currentUser;

            var now = new Date();
            var nowFormatted = moment(now).format("YYYY-MM-DD HH:mm");

            pvm.datePickerConfig = {
                enableTime: true,
                dateFormat: "Y-m-d H:i",
                time_24hr: true,
                minDate: nowFormatted,
                defaultDate: nowFormatted
            };       
        });
    }

    angular.module('umbraco')
        .controller('uSyncPublisherConfigController', configController);

})();