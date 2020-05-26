(function () {
    'use strict';

    function configController($scope,
        uSyncPublishService, uSyncActionManager) {

        // publisher vm
        var pvm = this;


        // config from the parent. 
        pvm.mode = $scope.vm.mode;
        pvm.isMedia = $scope.vm.isMedia;
        // options stored on the parent.
        pvm.stepArgs = $scope.vm.stepArgs;
        pvm.flags = $scope.vm.flags;

        // used in the UI, not the logic.
        pvm.server = $scope.vm.selectedServer;

        if ($scope.model != null) {
            $scope.model.title = pvm.server.Name + ': Settings';
            $scope.model.subtitle = uSyncActionManager.getDescription(pvm.mode, pvm.isMedia, pvm.server.Name);
        }

        pvm.contentName = pvm.isMedia ? $scope.vm.content.name : $scope.vm.content.variants[0].name;

        pvm.largeTree = false;
        if (!pvm.isMedia) {
            uSyncPublishService.getDecendentCount($scope.vm.content.id).then(function (result) {
                pvm.largeTree = result.data > 1000;
            });
        }
        

        InitOptions();

        ///////

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

        }

        $scope.$on('sync-server-selected', function (event, args) {
            pvm.server = args.server;
        });
    }

    angular.module('umbraco')
        .controller('uSyncPublisherConfigController', configController);

})();