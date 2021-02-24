(function () {
    'use strict';

    function dashboardController($timeout, $rootScope, navigationService) {

        var vm = this;

        vm.page = {
            title: 'uSync Publisher',
            description: 'Send and Pull content from other Umbraco installs',
            navigation: [
                {
                    'name': 'Publisher',
                    'alias': 'publisher',
                    'icon': 'icon-truck',
                    'view': Umbraco.Sys.ServerVariables.uSyncPublisher.pluginPath + 'dashboard/default.html',
                    'active': true
                },
                {
                    'name': 'Advanced',
                    'alias': 'settings',
                    'icon': 'icon-settings',
                    'view': Umbraco.Sys.ServerVariables.uSyncPublisher.pluginPath + 'dashboard/settings.html',
                }
            ]
        };

        $timeout(function () {
            navigationService.syncTree({ tree: 'uSyncPublisher', path: '-1' });
        });

        vm.save = saveSettings;

        function saveSettings() {
            $rootScope.$broadcast('usync-publisher-settings-save');
        }
    }

    angular.module('umbraco')
        .controller('uSyncPublisherDashboardController', dashboardController);

})();