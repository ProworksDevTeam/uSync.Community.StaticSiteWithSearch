﻿(function () {
    'use strict';

    function settingsController($scope,
        uSyncPublishService) {

        // publisher vm
        var pvm = this;

        // config from the parent. 
        pvm.mode = $scope.vm.mode;
        pvm.isMedia = $scope.vm.isMedia;
        // options stored on the parent.
        pvm.stepArgs = $scope.vm.stepArgs;
        pvm.flags = {
            includeChildren: { toggle: true, value: true },
            includeMedia: { toggle: false, value: false },
            includeLinked: { toggle: false, value: false },
            includeAncestors: { toggle: false, value: false },
            includeDependencies: { toggle: false, value: false },
            includeFiles: { toggle: false, value: true },
            includeMediaFiles: { toggle: false, value: false },
            deleteMissing: { toggle: false, value: false }
        };

        pvm.flagsValue = uSyncPublishService.getFlags(pvm.flags);

        pvm.items = {
            docTypes: { toggle: true, value: true, typeName: 'ContentType' },
            dataTypes: { toggle: true, value: true, typeName: 'DataType' },
            mediaTypes: { toggle: true, value: false, typeName: 'MediaType' },
            domains: { toggle: true, value: false, typeName: 'Domain' },
            memberTypes: { toggle: true, value: false, typeName: 'MemberType' },
            dictionary: { toggle: true, value: false, typeName: 'Dictionary' },
            macro: { toggle: true, value: false, typeName: 'Macro' },
            template: { toggle: true, value: false, typeName: 'Template' },
            files: { toggle: true, value: false, typeName: null },
            languages: { toggle: true, value: false, typeName: 'Language' },
            protect: { toggle: true, value: false, typeName: 'Protect' }
        };

        // used in the UI, not the logic.
        pvm.server = $scope.vm.selectedServer;

        var contentName = pvm.isMedia ? $scope.vm.content.name : $scope.vm.content.variants[0].name;

        InitOptions();

        ///////

        function InitOptions() {

            pvm.stepArgs.options = {
                items: [{
                    id: $scope.vm.content.id,
                    name: contentName,
                    udi: $scope.vm.content.udi,
                    flags: uSyncPublishService.getFlags(pvm.flags)
                }],
                removeOrphans: pvm.flags.deleteMissing.value,
                includeFileHash: pvm.flags.includeFiles.value
            };

            // when the flags change.
            $scope.$watch('pvm.items', function (newVal, oldVal) {

                if (newVal !== undefined) {

                    pvm.stepArgs.options.items = [];

                    angular.forEach(newVal, function (value, key) {

                        if (key === 'files') {
                            pvm.stepArgs.options.includeFileHash = value.value;
                        }
                        else if (value.value === true && value.typeName !== null) {
                            pvm.stepArgs.options.items.push({ id: '-1', udi: '', name: value.typeName, flags: pvm.flagsValue });
                        }
                    });

                    // console.log(pvm.stepArgs.options);
                    if (newVal.template.value === true && newVal.files.value !== true) {
                        newVal.files.value = true;
                    }
                }
            }, true);
        }
    }

    angular.module('umbraco')
        .controller('uSyncPublisherSettingsPushController', settingsController);

})();