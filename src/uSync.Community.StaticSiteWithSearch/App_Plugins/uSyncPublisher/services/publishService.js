(function () {
    'use strict';

    function publishService($http) {

        var publishService = Umbraco.Sys.ServerVariables.uSyncPublisher.publishService;

        var dependencyFlags = {
            none: 0,
            includeChildren: 2,
            includeAncestors: 4,
            includeDependencies: 8,
            includeFiles: 16,
            includeMedia: 32,
            includeLinked: 64,
            includeMediaFiles: 128,
            includeConfig: 256
        };

        return {
          
            // Server checks 
            getServers: getServers,
            getServer: getServer,
            getAllServers: getAllServers,
            checkServer: checkServer,
            checkServerUrl: checkServerUrl,

            // servers 
            saveServer: saveServer,
            deleteServer: deleteServer,

            setupServer: setupServer,

            // settings
            getSettings: getSettings,
            saveSettings: saveSettings,
            reloadSettings: reloadSettings,
            getVersion: getVersion,
            createKeys: createKeys,

            setServerOrder: setServerOrder,

            // until
            getDecendentCount: getDecendentCount,

            // flags
            getFlags: getFlags,

            getTemplates: getTemplates,

        };

        function getFlags(options) {
            var flags = 0;
            if (options.includeChildren.value) { flags |= dependencyFlags.includeChildren; }
            if (options.includeAncestors.value) { flags |= dependencyFlags.includeAncestors; }
            if (options.includeDependencies.value) { flags |= dependencyFlags.includeDependencies; }
            if (options.includeFiles.value) { flags |= dependencyFlags.includeFiles; }
            if (options.includeMedia.value) { flags |= dependencyFlags.includeMedia; }
            if (options.includeLinked.value) { flags |= dependencyFlags.includeLinked; }
            if (options.includeMediaFiles) { flags |= dependencyFlags.includeMediaFiles; }
            if (options.includeConfig?.value) { flags |= dependencyFlags.includeConfig; }

            return flags;
        }

        ///////////////////
        /// server checks

        function getServers(action) {
            return $http.get(publishService + 'GetServers/?action=' + action);
        }

        function getServer(alias) {
            return $http.get(publishService + 'GetServer/?alias=' + alias);
        }

        function checkServer(alias) {
            return $http.get(publishService + 'CheckServer/?server=' + alias);
        }

        function checkServerUrl(url) {
            return $http.get(publishService + 'CheckServerUrl/?url=' + url);
        }

        function getAllServers() {
            return $http.get(publishService + 'GetAllServers/?enabledOnly=' + false);
        }

        function setupServer(alias, url, user, pass) {
            return $http.post(publishService + 'setupServer', {
                server: alias,
                url: url,
                username: user,
                password: pass
            });
        }

        ///////////////////
        // settings get/set

        function getSettings() {
            return $http.get(publishService + 'GetSettings');
        }

        function saveSettings(settings) {
            return $http.post(publishService + 'SaveSettings', settings);
        }

        function reloadSettings() {
            return $http.get(publishService + 'ReloadSettings');
        }

        function getVersion() {
            return $http.get(publishService + 'GetVersion');
        }

        function createKeys() {
            return $http.put(publishService + 'CreateKeys');
        }


        function saveServer(server) {
            return $http.post(publishService + 'SaveServer', server);
        }

        function deleteServer(alias) {
            return $http.delete(publishService + 'DeleteServer/?server=' + alias);
        }

        function setServerOrder(order) {
            return $http.post(publishService + 'SetServerOrder', order);
        }

        function getDecendentCount(id) {
            return $http.get(publishService + 'DecendentCount/' + id);
        }

        function getTemplates() {
            return $http.get(publishService + 'GetTemplates');
        }

    }

    angular.module('umbraco')
        .factory('uSyncPublishService', publishService);
})();