(function () {
    'use strict';

    function publishActionManager($q, uSyncPublisherService) {

        return {
            // flags
            initFlags: initFlags,
            prepToggles: prepToggles,

            // actions
            hasStepActions: hasStepActions,

            // ui things. 
            getDialogTitle: getDialogTitle,
            getActionMessage: getActionMessage,
            getDescription: getDescription,

            makeUdi: makeUdi,
            emptyGuid: '00000000-0000-0000-0000-000000000000'
        };

        ////

        function makeUdi(type, guid) {
            var key = guid !== undefined ? guid : '00000000-0000-0000-0000-000000000000';
            return 'umb://' + type + '/' + key;
        }

        //////
       

        function getActionMessage(action) {
            return { title: action.Name, Steps: action.Steps };
        }

        function getDialogTitle(action) {
            return action.Name;
        }

        function getDescription(mode, isMedia, serverName) {
            var modeName = mode === 'SettingsPush' ? 'Push' : mode;
            var direction = mode === 'Pull' ? ' from ' : ' to ';
            var items = mode === 'SettingsPush' ? 'Settings' : isMedia ? 'media' : 'content';
            return modeName + ' ' + items + direction + serverName;
        }

        function hasStepActions(action) {
            for (let n = 0; n < action.Steps.length; n++) {
                if (action.Steps[n].IsAction == true) {
                    return true;
                }
            }

            return false;
        }

        ///// flags
        function initFlags() {
            return {
                includeChildren: { toggle: true, value: true },
                includeMedia: { toggle: true, value: true },
                includeLinked: { toggle: true, value: false },
                includeAncestors: { toggle: false, value: false },
                includeDependencies: { toggle: false, value: false },
                includeFiles: { toggle: false, value: false },
                includeMediaFiles: { toggle: false, value: false },
                deleteMissing: { toggle: true, value: false },
                includeConfig: { toggle: false, value: true }
            };
        }

        function prepToggles(server, flags, isMedia) {
            var op = server.SendSettings;
            if (op !== undefined) {

                flags.includeAncestors = setToggle(op.IncludeAncestors);
                flags.includeChildren = setToggle(op.IncludeChildren);
                flags.includeDependencies = setToggle(op.IncludeDependencies);
                flags.includeFiles = setToggle(op.IncludeFiles);
                flags.includeLinked = setToggle(op.IncludeLinked);
                flags.includeMedia = setToggle(op.IncludeMedia);
                flags.deleteMissing = setToggle(op.DeleteMissing);
                flags.includeConfig = setToggle(op.IncludeConfig);

                // override the settings for media 
                if (isMedia) {
                    flags.includeMedia = { toggle: false, value: true };
                    flags.includeAncestors = { toggle: false, value: true };
                    flags.includeFiles = { toggle: false, value: false };
                    flags.includeLinked = { toggle: false, value: false };
                }
            }

            return flags;
        }


        function setToggle(value) {
            if (value !== undefined && value.startsWith('user')) {
                return { toggle: true, value: value.endsWith('yes') };
            }
            else {
                return { toggle: false, value: value === 'yes' };
            }
        }

    }

    angular.module('umbraco')
        .factory('uSyncActionManager', publishActionManager);

})();