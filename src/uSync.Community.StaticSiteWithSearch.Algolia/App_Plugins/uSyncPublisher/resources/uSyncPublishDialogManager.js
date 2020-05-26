(function () {
    'use strict';

    function dialogManager($timeout, editorService, navigationService) {

        return {
            openPublisherDialog: openPublisherDialog,
            openPublisherPullDialog: openPublisherPullDialog,
            openPublisherMediaPush: openPublisherMediaPush,
            openPublisherMediaPull: openPublisherMediaPull,

            openSyncDialog : openSyncDialog
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

        function openSyncDialog(dialogTitle, dialogView, options, cb, mode) {
            editorService.open({
                entity: options.entity,
                mode: mode,
                title: dialogTitle,
                size: 'small',
                view: Umbraco.Sys.ServerVariables.uSyncPublisher.pluginPath + 'dialogs/' + dialogView + '.html',
                submit: function (done) {
                    editorService.close();
                    if (cb !== undefined) {
                        cb(true);
                    }
                },
                close: function () {
                    editorService.close();
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
    }

    angular.module('umbraco')
        .factory('uSyncPublishDialogManager', dialogManager);
})();