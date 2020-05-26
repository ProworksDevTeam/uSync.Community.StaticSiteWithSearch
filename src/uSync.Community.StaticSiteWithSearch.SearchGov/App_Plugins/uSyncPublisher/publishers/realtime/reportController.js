(function () {
    'use strict';

    function reportController($scope) {

        // simple check to see if there are 
        // any changes that need to be made.

        var results = $scope.vm.results.Results;
        var pending = false; 


        if ($scope.model != null) {
            var server = $scope.vm.selectedServer;
            $scope.model.title = server.Name + ': Report';
            $scope.model.subtitle = server.Description + ' [' + server.Url + ']';
        }


        for (let i = 0; i < results.length; i++) {

            if (results[i].Change !== 'NoChange') {
                pending = true;
                break;
            }
        }
        $scope.vm.complete = !pending;
    }

    angular.module('umbraco')
        .controller('uSyncPublisherReportController', reportController);
})();