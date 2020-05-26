(function () {
    'use static';

    function dashboardSettingsController($scope,
        notificationsService, uSyncPublishService) {

        var vm = this;
        vm.loading = true;

        vm.server = {};
        vm.settings = {};

        vm.saveState = 'init';

        vm.save = save;
        vm.reloadSettings = reloadSettings;

        vm.copyText = copyText;

        vm.groups = [];

        vm.userGroupPicker = {};

        getSettings();

        /////////////////
        function save() {
            saveSettings();
        }

        $scope.$on('usync-publisher-settings-save', function () {
            saveSettings();
        });

        $scope.$on('usync-publisher-settings-reload', function () {
            reloadSettings();
        })


        /////////////////
        function getSettings() {
            uSyncPublishService.getSettings()
                .then(function (result) {
                    vm.loading = false;
                    vm.settings = result.data;

                    initPicker();
                });
        }

        function reloadSettings() {
            vm.loading = true;
            vm.saveState = 'busy';
            uSyncPublishService.reloadSettings()
                .then(function (result) {
                    vm.saveState = 'success';
                    vm.loading = false;
                    vm.settings = result.data;

                    initPicker();

                    notificationsService.add({
                        type: 'info',
                        headline: 'Reload',
                        message: 'Settings reloaded from disk'
                    });
                });
        }

        function saveSettings() {

            vm.saveState = 'busy';

            uSyncPublishService.saveSettings(vm.settings)
                .then(function (result) {
                    vm.saveState = 'success';
                    notificationsService.success('Save', 'uSync publisher settings saved');
                }, function (error) {
                    vm.saveState = 'error';
                    notificationsService.error('Error', error.data.ExceptionMessage);
                });
        }

        function copyText() {
            var copyItem = document.getElementById("serverUrl");
            copyItem.select();
            copyItem.setSelectionRange(0, 99999); /*For mobile devices*/
            document.execCommand("copy");
            notificationsService.success('Copied', 'Server url copied to clipboard');
        }

        function initPicker() {
            vm.userGroupPicker = {
                value: vm.settings.SendSettings,
                view: Umbraco.Sys.ServerVariables.uSyncPublisher.pluginPath + 'pickers/userGroupPicker.html',
                validation: {
                    mandatory: false
                },
                config: {}
            };
        }

    }

    angular.module('umbraco')
        .controller('uSyncPublisherSettingsController', dashboardSettingsController);
})();