(function () {
    'use strict';

    function reportController($scope) {

        // simple check to see if there are 
        // any changes that need to be made.

        var results = $scope.vm.results.Results;

        if ($scope.model != null && $scope.vm != null && $scope.vm.selectedServer != null) {
            var server = $scope.vm.selectedServer;
            $scope.model.title = server.Name + ': Report';
            $scope.model.subtitle = server.Description + ' [' + server.Url + ']';
        }

        $scope.vm.complete = !hasPending(results);
        $scope.vm.postActions = hasPostActions(results);

        function hasPending(results) {
            for (let i = 0; i < results.length; i++) {
                if (results[i].Change !== 'NoChange') {
                    return true;
                }
            }
            return false;
        }

        function hasPostActions(results) {
            for (let i = 0; i < results.length; i++) {
                if (results[i].ItemType.startsWith('Umbraco.Core.Models.IFile') && results[i].RequiresPostProcessing) {
                    return true;
                }
            }

            return false;
        }
    }

    angular.module('umbraco')
        .controller('uSyncPublisherReportController', reportController);
})();