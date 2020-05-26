(function () {
    'use strict';

    function publisherDialogController($scope, $q,
        contentResource, mediaResource, notificationsService,
        uSyncActionManager,
        uSyncPublishService, uSyncPublisherService, uSyncHub) {

        var vm = this;

        vm.loading = true;
        vm.loadedActions = false;
        vm.complete = false;
        vm.working = false;
        vm.valid = false;

        vm.hasError = false;
        vm.error = "";

        vm.mode = $scope.model.mode;

        vm.entity = $scope.model.entity;
        vm.isMedia = vm.entity.metaData.application === 'media';
        vm.isSettings = vm.entity.metaData.application === 'settings';

        vm.view = {
            show: false,
            path: ''
        };

        vm.actionButton = {
            state: 'init',
            name: 'Send'
        };

        vm.dialog = {
            title: 'Select a Server',
            desc: vm.mode + ' ' + (vm.isMedia ? 'media' : 'content')
        };

        vm.message = {
            Steps: [],
            Message: 'working'
        };

        vm.processId = '00000000-0000-0000-0000-000000000000'; // this will be the guid.

        vm.stepArgs = {
            stepAlias: '',
            target: '',
            options: {},
            clientId: ''
        };

        vm.actionNo = 0;
        vm.pickServer = true;
        if (vm.entity.server !== undefined) {
            vm.pickServer = false;
            vm.selectedServer = vm.entity.server;
        }

        vm.content = {};

        /// function 
        vm.close = close;
        vm.performAction = performAction;
        vm.setValidState = setValidState;
        vm.calcPercentage = calcPercentage;

        // server and toggle prep.
        vm.servers = [];

        // 
        vm.flags = uSyncActionManager.initFlags();

        /// init
        InitController();

        function InitController() {

            var promises = [];

            if (vm.entity.id * 1 === -1) {
                vm.content = {
                    id: vm.entity.id,
                    name: vm.isMedia ? 'Media' : 'Content',
                    variants: [{ name: 'Content' }]
                };
            }
            else {
                if (!vm.isMedia) {
                    promises.push(contentResource.getById(vm.entity.id)
                        .then(function (content) {
                            vm.content = content;
                        }));
                }
                else {
                    promises.push(mediaResource.getById(vm.entity.id)
                        .then(function (content) {
                            vm.content = content;
                        }));
                }
            }

            if (vm.pickServer) {
                promises.push(uSyncPublishService.getServers(vm.mode)
                    .then(function (result) {
                        vm.servers = result.data;
                        checkServers(vm.servers);
                    }));
            }

            initHub();

            $q.all(promises).then(function () {
                if (!vm.pickServer) {
                    initDirectServer();
                }

                vm.loading = false;
            });

        }

        function loadActions() {
            vm.loadedActions = false;
            uSyncPublisherService.getActions(vm.mode, vm.selectedServer.Alias)
                .then(function (result) {
                    vm.actions = result.data;
                    vm.actionNo = 0;
                    vm.loadedActions = true;
                    setupAction(vm.actionNo, true);
                }, function (error) {
                        notificationsService.error('Error',
                            error.data.ExceptionMessage !== undefined ? error.data.ExceptionMessage : error.data.exceptionMessage);
                    vm.actionButton.state = 'error';
                    vm.working = false;
                });
        }


        ///////////
        function setupAction(actionNo, init) {

            var current = vm.actions[actionNo];

            // dialog title
            vm.dialog.title = uSyncActionManager.getDialogTitle(current);

            if (init) {
                // init the message display.
                vm.message = uSyncActionManager.getActionMessage(current);
                vm.stepArgs.stepAlias = current.Alias;
                setStep(0);
            }

            if (current.View !== null && current.View.length > 0) {
                // user UI step.
                showActionView(current);
            }
            else {
                // automatic step.
                vm.view = { show: false };

                if (uSyncActionManager.hasStepActions(current)) {
                    performStep(current);
                }
                else {
                    performAction();
                }
            }
        }

        function setStep(stepIndex) {
            vm.stepArgs.options.StepIndex = stepIndex;
        }

        function performStep(action) {
            var currentStep = action.Steps[action.currentStepNo];
            vm.working = true;
            vm.actionButton.state = 'busy';
            vm.stepArgs.clientId = getClientId();
            vm.stepArgs.stepName = currentStep.Name;

            uSyncPublisherService.performStep(vm.processId, vm.mode, vm.stepArgs)
                .then(function (result) {
                    vm.results = result.data;

                    if (vm.results.Success) {
                        vm.complete = vm.results.Complete;

                        if (!vm.complete) {
                            vm.stepArgs.options = vm.results.Options;
                            nextStep();
                        }
                        else {
                            vm.working = false;
                        }
                        vm.actionButton.state = 'success';
                    }
                    else {
                        vm.hasError = true;
                        vm.error = {
                            ExceptionMessage: 'Step failed [' + vm.results.Message + ']',
                            StackTrace: vm.results.Details
                        };
                        vm.actionButton.state = 'error';
                        vm.working = false;
                        vm.valid = false; 
                    }
                }, function (error) {
                    vm.hasError = true;
                    vm.error = error.data;
                    vm.actionButton.state = 'error';
                    vm.working = false;
                    vm.valid = false;
                });
        }

        function showActionView(action) {
            vm.working = false;
            vm.view = {
                show: true,
                path: action.View
            };
            vm.actionButton.name = action.ButtonText;
        }

        function nextAction() {
            vm.pickServer = false;
            if (vm.actionNo < vm.actions.length) {
                vm.actionNo++;

                // set to the first step.
                vm.actions[vm.actionNo].currentStepNo = 0;
                setupAction(vm.actionNo, true);
            }
        }

        function nextStep() {
            vm.pickServer = false;
            var action = vm.actions[vm.actionNo];

            if (action.currentStepNo < action.Steps.length - 1) {
                for (let n = action.currentStepNo + 1; n < action.Steps.length; n++) {
                    if (action.Steps[n].IsAction) {
                        vm.actions[vm.actionNo].currentStepNo = n;
                        return setupAction(vm.actionNo, false);
                    }
                }
            }

            // end of steps - next action
            nextAction();
        }

        /////////

        function close() {
            if (vm.processId !== '00000000-0000-0000-0000-000000000000') {
                clean(vm.processId, vm.selectedServer.Alias);
            }

            if ($scope.model.close) {
                $scope.model.close();
            }
        }

        function clean(id, server) {
            uSyncPublisherService.clean(id, server)
                .then(function () {
                    // console.log('cleaned up our mess');
                });
        }

        // do the thing, 
        // do we need to get config etc from the included view?
        function performAction() {

            vm.working = true;
            vm.actionButton.state = 'busy';
            vm.stepArgs.clientId = getClientId();

            uSyncPublisherService.performAction(vm.processId, vm.mode, vm.stepArgs)
                .then(function (result) {
                    vm.results = result.data;

                    if (vm.results.Success) {

                        if (vm.processId === '00000000-0000-0000-0000-000000000000') {
                            vm.processId = vm.results.Id;
                        }

                        // is this the end.
                        vm.complete = vm.results.Complete;

                        // next step 
                        if (!vm.complete) {
                            nextAction();
                        }
                        else {
                            vm.working = false;
                        }
                        vm.actionButton.state = 'success';
                    }
                    else {
                        notificationsService.error('Failed', 'There was a problem running this step');
                        vm.actionButton.state = 'error';
                        vm.working = false;
                    }
                }, function (error) {
                    vm.hasError = true;
                    vm.error = error.data;
                    vm.actionButton.state = 'error';
                    vm.working = false;
                    vm.valid = false;
                });
        }

        function setValidState(valid) {
            vm.valid = valid;
        }

        function calcPercentage(status) {
            if (status !== undefined) {
                return (100 * status.Count) / status.Total;
            }
            return 1;
        }

        ////// server picker 
        vm.onSelected = onSelected;

        ////////

        function checkServers(servers) {
            servers.forEach(function (server) {
                uSyncPublishService.checkServer(server.Alias)
                    .then(function (result) {
                        server.status = result.data;
                    });
            });
        }

        function onSelected(server) {
            vm.selectedServer = server;

            vm.flags = uSyncActionManager.prepToggles(server, vm.flags, vm.isMedia);
            // prepToggles(server);
            setValidState(true);
            vm.stepArgs.target = server.Alias;

            vm.dialog.desc = uSyncActionManager.getDescription(vm.mode, vm.isMedia, vm.selectedServer.Name);


            loadActions();

            $scope.$broadcast('sync-server-selected', { server: server, flags: vm.flags });
        }

        function initDirectServer() {
            vm.stepArgs.options = {
                items: vm.entity.items
            };
            onSelected(vm.selectedServer);
        }


        ////// SignalR things 
        function initHub() {
            uSyncHub.initHub(function (hub) {
                vm.hub = hub;

                vm.hub.on('update', function (update) {
                    vm.update = update;
                    vm.update.blocks = update.Message.split('||');
                });

                vm.hub.on('add', function (status) {
                    vm.status = status;
                });

                vm.hub.on('publisher', function (message) {
                    vm.message = message;
                });

                vm.hub.start();
            });
        }

        function getClientId() {
            if ($.connection !== undefined && $.connection.hub !== undefined) {
                return $.connection.hub.id;
            }
            return "";
        }


    }

    angular.module('umbraco')
        .controller('uSyncPublisherDialogController', publisherDialogController);

})();