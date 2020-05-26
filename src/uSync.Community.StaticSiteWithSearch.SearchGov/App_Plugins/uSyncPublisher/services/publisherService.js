(function () {
    'use strict';

    function publisherService($http) {

        var publisherService = Umbraco.Sys.ServerVariables.uSyncPublisher.publisherService;

        return {
            getActions: getActions,
            performAction: performAction,
            performStep: performStep,
            clean: clean
        };

        ////////

        function getActions(mode, server) {
            return $http.get(publisherService + "GetActions/?server=" + server + "&mode=" + mode);
        }

        function performAction(id, mode, args) {
            return $http.post(publisherService + "PerformAction/" + id + "?mode=" + mode, args);
        }

        function performStep(id, mode, args) {
            return $http.post(publisherService + "PerformStep/" + id + "?mode=" + mode, args);
        }

        function clean(id, server) {
            return $http.delete(publisherService + "Clean/" + id + "?server=" + server);
        }
    }

    angular.module('umbraco')
        .factory('uSyncPublisherService', publisherService);
})();