(function () {
    'use strict';

    function dialogManager($rootScope, $timeout, editorService, navigationService) {

        return {
            openPublisherDialog: openPublisherDialog,
            openPublisherPullDialog: openPublisherPullDialog,
            openPublisherMediaPush: openPublisherMediaPush,
            openPublisherMediaPull: openPublisherMediaPull,

            openSettingsPush: openSettingsPush,

            openSyncDialog: openSyncDialog,

            openServerDialog: openServerDialog,
            openNewServerDialog: openNewServerDialog
        };

        function openPublisherDialog(options, cb) {
            openSyncDialog('Publish Content', 'publisher', options, cb, "Push");
        }

        function openPublisherPullDialog(options, cb) {
            openSyncDialog('Pull Content', 'publisher', options, cb, "Pull");
        }

        function openPublisherMediaPush(options, cb) {
            openSyncDialog('Publish Media', 'publisher', options, cb, "Push");
        }

        function openPublisherMediaPull(options, cb) {
            openSyncDialog('Pull Media', 'publisher', options, cb, "Pull");
        }

        function openSettingsPush(options, cb) {

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

            var deployOptions = {
                entity: {
                    id: '-1',
                    metaData: {
                        application: 'settings'
                    },
                    items: items,
                    serverAlias: options.entity.id
                }
            };

            openSyncDialog('Deploy Settings', 'publisher', deployOptions, cb, "SettingsPush", "");
        }

        function openSyncDialog(dialogTitle, dialogView, options, cb, mode, size = 'small') {
            editorService.open({
                entity: options.entity,
                mode: mode,
                title: dialogTitle,
                size: size,
                view: Umbraco.Sys.ServerVariables.uSyncPublisher.pluginPath + 'dialogs/' + dialogView + '.html',
                submit: function (done) {
                    editorService.close();
                    navigationService.hideNavigation();
                    if (cb !== undefined) {
                        cb(true);
                    }
                },
                close: function () {
                    editorService.close();
                    navigationService.hideNavigation();
                    if (cb !== undefined) {
                        cb(false);
                    }
                }
            });

            // wrap in a timeout, get rid of the 'bounce' 
            $timeout(function () {
                navigationService.hideDialog();
            });
        }


        function openNewServerDialog(entity, cb) {
            editorService.open({
                view: Umbraco.Sys.ServerVariables.uSyncPublisher.pluginPath + 'dialogs/addServer.html',
                title: 'Add Server',
                size: 'small',
                submit: function (model) {
                    editorService.close();
                    navigationService.hideNavigation();
                    openServerDialog(model.Alias);
                    if (cb !== undefined) {
                        cb(true);
                    }
                },
                close: function () {
                    editorService.close();
                    navigationService.hideNavigation();
                    if (cb !== undefined) {
                        cb(false);
                    }
                }
            });
        }

        function openServerDialog(alias, cb) {
            editorService.open({
                serverAlias: alias,
                title: 'Server View',
                view: Umbraco.Sys.ServerVariables.uSyncPublisher.pluginPath + 'backoffice/uSyncPublisher/server.html',
                submit: function (done) {
                    $rootScope.$broadcast('usync-publisher-settings-reload');
                    navigationService.hideNavigation();
                    editorService.close();
                    if (cb !== undefined) {
                        cb(true);
                    }
                },
                close: function () {
                    $rootScope.$broadcast('usync-publisher-settings-reload');
                    navigationService.hideNavigation();
                    editorService.close();
                    if (cb !== undefined) {
                        cb(false);
                    }
                }
            });
        }

    }

    angular.module('umbraco')
        .factory('uSyncPublishDialogManager', dialogManager);
})();