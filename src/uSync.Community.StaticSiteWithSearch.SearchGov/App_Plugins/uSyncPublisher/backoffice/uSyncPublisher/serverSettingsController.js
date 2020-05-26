(function () {
    'use strict';

    function serverSettingsController($scope, $routeParams, $timeout, $http,
        $rootScope, navigationService, notificationsService,
        uSyncPublishService) {

        var vm = this;
        vm.loading = true;
        vm.picker = false;

        vm.networkMode = Umbraco.Sys.ServerVariables.uSyncPublisher.networkMode;

        vm.applyTemplate = applyTemplate;
        vm.templates = [];

        vm.buttonState = 'init';
        vm.checkStatus = 'init';
        vm.checkStatusButton = 'Check access';

        vm.status = {};
        vm.server = {
            Id: '',
            SendSettings: { groups: 'admin,editor' },
            Icon: 'icon-server',
            AllowedServers: []
        };

        vm.checked = false;

        vm.page = {
            title: '[Server name]',
            description: '[Server description]'
        };

        vm.save = saveServer;
        vm.checkServer = checkServer;

        $timeout(function () {
            navigationService.syncTree({ tree: "uSyncPublisher", path: vm.alias });
        });

        $rootScope.$on('$routeUpdate', function (event, next) {
            if ($routeParams.id == -1) {
                Init();
            }
        }); 

        Init();


        function Init() {
            if (vm.alias !== $routeParams.id) {
                vm.alias = $routeParams.id;

                if (vm.alias !== '-1') {
                    loadServer();
                    return;
                }
            }
            // else 
            loadTemplates();
        }

        ////////////////

        function loadServer() {
            uSyncPublishService.getServer(vm.alias)
                .then(function (result) {
                    vm.server = result.data;

                    if (vm.server) {
                        vm.networkMode = vm.server.NetworkMode;
                    }

                    initPicker();
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

        function saveServer() {

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
                    checkServer();

                }, function (error) {
                    vm.buttonState = 'error';
                    notificationsService.error('error', error.data.ExceptionMessage);
                });
        }

        function checkServer() {
            vm.checked = true;
            vm.checkStatus = 'busy';
            vm.status = {};
            uSyncPublishService.checkServer(vm.server.Alias)
                .then(function (result) {
                    vm.checkStatus = 'success';
                    vm.checkStatusButton = result.data.Status;
                    vm.status = result.data;
                    vm.saved = false;

                    $timeout(() => {
                        vm.status = {};
                        vm.checked = false;
                    }, 2000);

                }, function (error) {
                    notificationsService.error('error', error.data.ExceptionMessage);
                });
        }

        function applyTemplate(template) {

            vm.server = {
                Icon: template.icon,
                Enabled: true,
                PullEnabled: true,
                Url: '[https://my.server.url/umbraco]',
                SendSettings: template.flags,
                Publisher: template.publisher,
                PublisherConfig: template.publisherConfig
            };
            vm.picker = false;
            initPicker();
        }


        function loadTemplates() {
            // templates (when you pick create )
            $http.get(Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath + '/uSyncPublisher/backoffice/uSyncPublisher/templates.json?v=860')
                .then(function (result) {
                    vm.templates = result.data;
                    vm.picker = true;
                    vm.loading = false;
                }, function (error) {
                    vm.loading = false;
                });

        }
    }

    angular.module('umbraco')
        .controller('uSyncPublisherServerSettingsController', serverSettingsController);
})();