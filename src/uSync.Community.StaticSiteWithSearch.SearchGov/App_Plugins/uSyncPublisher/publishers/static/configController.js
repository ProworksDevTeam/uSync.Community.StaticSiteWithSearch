(function () {
    'use strict';

    function configController($scope, uSyncPublishService) {

        var pvm = this;

        /// passed to us when the server is first picked
        pvm.mode = $scope.vm.mode;
        pvm.isMedia = $scope.vm.isMedia;
        pvm.stepArgs = $scope.vm.stepArgs;
        pvm.flags = $scope.vm.flags;
        pvm.server = $scope.vm.selectedServer;

        if ($scope.model != null) {
            $scope.model.title = pvm.server.Name + ' Settings';
            $scope.model.subtitle = pvm.server.Description + ' [' + pvm.server.Url + ']';
        }

        pvm.contentName = pvm.isMedia ? $scope.vm.content.name : $scope.vm.content.variants[0].name;

        initOptions();

        function initOptions() {
            pvm.stepArgs.options = {
                items: [{
                    id: $scope.vm.content.id,
                    name: pvm.contentName,
                    udi: $scope.vm.content.udi,
                    flags: uSyncPublishService.getFlags(pvm.flags)
                }],
                removeOrphans: true,
                includeFileHash: pvm.flags.includeFiles.value
            };

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
        .controller('uSyncStaticPublishConfigController', configController)
})();