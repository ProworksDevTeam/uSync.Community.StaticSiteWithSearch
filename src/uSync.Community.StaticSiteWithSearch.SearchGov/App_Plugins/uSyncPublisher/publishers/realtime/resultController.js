(function () {
    'use strict';

    function resultController($scope) {
        $scope.vm.complete = true;

        if ($scope.model != null) {
            var server = $scope.vm.selectedServer;
            $scope.model.title = server.Name + ' Complete';
            $scope.model.subtitle = server.Description + ': [' + server.Url + ']';
        }
    }

    angular.module('umbraco')
        .controller('uSyncPublisherResultController', resultController);
})();