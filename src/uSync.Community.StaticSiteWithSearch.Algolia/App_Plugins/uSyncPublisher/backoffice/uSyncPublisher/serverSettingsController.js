(function () {
    'use strict';

    function serverSettingsController($scope, $routeParams, $timeout, $http,
        $rootScope, navigationService, notificationsService,
        uSyncPublishService, uSyncPublishDialogManager,
        uSyncPublishServerManager) {

        var vm = this;
        vm.loading = true;
        vm.checked = false;
        vm.showClose = false;
        vm.showAdvanced = true;

        vm.networkMode = Umbraco.Sys.ServerVariables.uSyncPublisher.networkMode;

        vm.buttonState = 'init';
        vm.checkStatus = 'init';
        vm.checkStatusButton = 'Check access';

        vm.status = { Enabled: false };
        vm.server = {
            Id: '',
            SendSettings: { groups: 'admin,editor' },
            Icon: 'icon-server',
            AllowedServers: []
        };


        vm.page = {
            title: '[Server name]',
            description: '[Server description]'
        };

        vm.save = save;
        vm.close = close;
        vm.checkServer = checkServer;
        vm.deploy = deploy;

        vm.remoteSetup = remoteSetup;
        
        $timeout(function () {
            if (!vm.showClose) {
                navigationService.syncTree({ tree: "uSyncPublisher", path: vm.alias });
            }
        });
     
        Init();

        function Init() {

            getSettings();

            var serverAlias = $routeParams.id;
            if ($scope.model != null) {
                serverAlias = $scope.model.serverAlias;
                vm.showClose = true;
            }

            if (vm.alias !== serverAlias) {
                vm.alias = serverAlias;
                if (vm.alias !== '-1') {
                    loadServer();
                    return;
                }
            }
        }

        ////////////////

        function getSettings() {
            uSyncPublishService.getSettings()
                .then(function (result) {
                    vm.settings = result.data;

                    if (!vm.settings.HasAppId) {
                        uSyncPublishServerManager.createLocalApiKeys();
                    }
                });
        }


        function loadServer() {
            uSyncPublishService.getServer(vm.alias)
                .then(function (result) {
                    vm.server = result.data;

                    if (vm.server) {
                        vm.networkMode = vm.server.NetworkMode;
                    }

                    initPicker();
                    checkServer(false);

                    vm.loading = false;
                }, function (error) {
                    notificationsService.error('Error', error.data.ExceptionMessage);
                });
        }

        function initPicker() {

            if (vm.server.AllowedServers === undefined || vm.server.AllowedServers === null || vm.server.AllowedServers.length === 0) {
                vm.server.AllowedServers = [];
            }

            vm.allowedPicker = {
                value: vm.server.AllowedServers,
                view: Umbraco.Sys.ServerVariables.uSyncPublisher.pluginPath + 'serverPicker/picker.html',
                validation: {
                    mandatory: true
                },
                config: {
                    multiPicker: false
                }
            };

            vm.userGroupPicker = {
                value: vm.server.SendSettings,
                view: Umbraco.Sys.ServerVariables.uSyncPublisher.pluginPath + 'pickers/userGroupPicker.html',
                validation: {
                    mandatory: false
                },
                config: {}
            };

        }

        function save()
        {
            vm.saved = false;
            vm.buttonState = 'busy';
            uSyncPublishService.saveServer(vm.server)
                .then(function (result) {
                    vm.buttonState = 'success';
                    notificationsService.success('Saved', vm.server.Alias + ' server settings have been updated');
                    navigationService.syncTree({ tree: 'uSyncPublisher', path: ["-1", vm.server.Alias], forceReload: true });
                    vm.saved = true;
                    vm.checked = false;
                    // event so sub setting views can act if they need to.
                    $rootScope.$broadcast('usync-publish-serverSave');
                    checkServer(false);

                }, function (error) {
                    vm.buttonState = 'error';
                    notificationsService.error('error', error.data.ExceptionMessage);
                });
        }

        function close() {
            if ($scope.model.close) {
                $scope.model.close();
            }
        }

        function checkServer(showSuccessBar) {

            vm.checked = true;
            vm.checkStatus = 'busy';
            vm.status = {};

            uSyncPublishServerManager.checkServer(vm.server.Alias, showSuccessBar,
                function (status) {
                    vm.status = status;
                    if (status != null) {
                        vm.checkStatus = 'success';
                        vm.checkStatusButton = status;
                        vm.saved = false;

                        $timeout(() => {
                            vm.checked = false;
                        }, 2000);
                    }
                    else {
                        vm.checkStatus = 'error';
                    }
                });
        }

        function deploy() {

            var emptyGuid = '00000000-0000-0000-0000-000000000000';
            var items = [
                {
                    uid: 'umb://document-type/' + emptyGuid, name: 'ContentType'
                },
                {
                    udi: 'umb://data-type/' + emptyGuid, name: 'DataType'
                },
                {
                    udi: 'umb://media-type/' + emptyGuid, name: 'MediaType'
                }];

            var options = {
                entity: {
                    id: '-1',
                    metaData: {
                        application: 'settings'
                    },
                    items: items,
                    server: vm.server
                }
            };

            uSyncPublishDialogManager.openSyncDialog('Deploy Settings', 'publisher', options, function () { }, 'SettingsPush', '');
        }

        function remoteSetup(server) {
            uSyncPublishServerManager.remoteSetup(server, function (success) {
                checkServer(false);
                vm.checked = true;
            });
        }
    }

    angular.module('umbraco')
        .controller('uSyncPublisherServerSettingsController', serverSettingsController);
})();