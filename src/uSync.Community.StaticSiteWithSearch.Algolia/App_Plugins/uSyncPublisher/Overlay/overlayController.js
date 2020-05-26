(function () {
    'use strict';

    function overlayController($scope, $q, notificationsService,
        uSyncHub, uSyncPublishService, uSyncPublisherService,
        uSyncActionManager) {

        var vm = this;
        vm.servers = [];
        vm.loading = true;
        vm.pickServer = true;
        vm.working = false;

        vm.onSelected = onSelected;
        vm.mode = 'Push';
        vm.servers = $scope.model.servers;
        vm.content = $scope.model.entity;
        vm.complete = false;
        vm.contentIsDirty = false;

        $scope.model.moveToNext = moveToNext;
        $scope.model.isComplete = isComplete;

        vm.servers = [{ Name: 'loading', Icon: 'icon-timer' }]

        vm.actionNo = -1;
        vm.message = {
            Steps: [],
            Message: 'working'
        };

        vm.processId = '00000000-0000-0000-0000-000000000000';

        vm.stepArgs = {
            stepAlias: '',
            target: '',
            options: '',
            clientId: ''
        };

        vm.servers = [];
        vm.flags = uSyncActionManager.initFlags();

        vm.calcPercentage = calcPercentage;
        vm.complete = false;

        $scope.$watch('vm.complete', function (newVal, oldVal) {
            if (newVal === true) {
                $scope.model.submitButtonLabel = 'Done';
                $scope.model.hideSubmitButton = true;
            }
        });

        ////////////

        function init() {
            $scope.model.disableSubmitButton = true;

            uSyncPublishService.getServers(vm.mode)
                .then(function (result) {
                    vm.servers = result.data;
                    checkServers(vm.servers);

                    if (vm.servers.length === 1) {
                        onSelected(vm.servers[0]);
                        moveToNext();
                    }
                    vm.loading = false;
                });

            initHub();

            if (isDirty()) {
                vm.contentIsDirty = true;
            }
        }

        function isDirty() {
            for (let i = 0; i < vm.content.variants.length; i++) {
                if (vm.content.variants[i].isDirty === true) {
                    return true;
                }
            }
            return false;
        }

        /// server checks.
        function checkServers(servers) {
            servers.forEach(function (server) {
                uSyncPublishService.checkServer(server.Alias)
                    .then(function (result) {
                        server.status = result.data;
                    });
            });
        }

        function onSelected(server) {
            $scope.model.server = server;
            vm.selectedServer = server;

            $scope.model.disableSubmitButton = false;
            vm.stepArgs.target = server.Alias;
        }

        ///// actions

        function loadActions() {
            vm.loadedActions = false;

            uSyncPublisherService.getActions(vm.mode, vm.selectedServer.Alias)
                .then(function (result) {
                    vm.actions = result.data;
                    vm.actionNo = 0;
                    vm.loadedActions = true;
                    setupAction(vm.actionNo);
                    vm.pickServer = false;
                }, function (error) {
                    notificationsService.error('Error', error.data.ExceptionMessage);
                    vm.actionButton.state = 'error';
                    vm.working = false;
                });
        }

        function setupAction(actionNo, init) {

            var current = vm.actions[actionNo];

            $scope.model.title = uSyncActionManager.getDialogTitle(current);

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

        function showActionView(action) {
            vm.working = false;
            vm.view = {
                show: true,
                path: action.View
            };

            $scope.model.submitButtonLabel = action.ButtonText;
        }

        function performStep(action) {
            vm.working = true;
            $scope.model.submitButtonState = 'busy';
            vm.stepArgs.clientId = getClientId();

            var currentStep = action.Steps[action.currentStepNo];
            vm.stepArgs.stepName = currentStep.Name;

            uSyncPublisherService.performStep(vm.processId, vm.mode, vm.stepArgs)
                .then(function (result) {
                    vm.results = result.data;

                    if (vm.processId === '00000000-0000-0000-0000-000000000000') {
                        vm.processId = vm.results.Id;
                    }

                    if (vm.results.Success) {
                        vm.complete = vm.results.Complete;

                        if (!vm.complete) {
                            vm.stepArgs.options = vm.results.Options;
                            nextStep();
                        }
                        else {
                            vm.working = false;
                        }
                    }
                }, function (error) {
                    vm.hasError = true;
                    vm.error = error.data;
                    $scope.model.submitButtonState = 'error';
                    vm.working = false;
                    vm.valid = false;
                });
        }

        function performAction() {

            vm.working = true;
            $scope.model.submitButtonState = 'busy';
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
                        $scope.model.submitButtonState = 'success';
                    }
                    else {
                        notificationsService.error('Failed', 'There was a problem running this step');
                        $scope.model.submitButtonState = 'error';
                        vm.working = false;
                    }
                }, function (error) {
                    notificationsService.error('Error', error.data.ExceptionMessage);
                    $scope.model.submitButtonState = 'error';
                    vm.working = false;
                });
        }

        function nextAction() {
            if (vm.actionNo < vm.actions.length) {
                vm.actionNo++;

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
            $scope.model.submitButtonState = 'success';

            nextAction();
        }

        // called in by overlay 
        function moveToNext() {

            if (vm.actionNo == -1) {
                uSyncActionManager.prepToggles(vm.selectedServer, vm.flags, false);
                loadActions();
            }
            else {
                nextAction();
            }
        }

        function isComplete() {
            return vm.complete;
        }

        function calcPercentage(status) {
            if (status !== undefined) {
                return (100 * status.Count) / status.Total;
            }
            return 1;
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


        init();
    };

    angular.module('umbraco')
        .controller('uSyncPublishOverlayController', overlayController);
})();