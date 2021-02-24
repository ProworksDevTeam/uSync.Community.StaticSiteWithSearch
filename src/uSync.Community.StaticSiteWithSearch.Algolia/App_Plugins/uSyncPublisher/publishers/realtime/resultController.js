(function () {
    'use strict';

    function resultController($scope) {

        $scope.vm.complete = isComplete($scope.vm.results?.Results);

        if ($scope.model != null && $scope.vm != null && $scope.vm.selectedServer != null) {
            var server = $scope.vm.selectedServer;
            $scope.model.title = server.Name + ' Complete';
            $scope.model.subtitle = server.Description + ': [' + server.Url + ']';
        }

        function isComplete(results) {
            if (results != undefined && results !== null) {

                for (let i = 0; i < results.length; i++) {
                    if (results[i].Change === 'WillChange') {
                        return false;
                    }
                };
            }
            return true;
        }
    }

    angular.module('umbraco')
        .controller('uSyncPublisherResultController', resultController);
})();