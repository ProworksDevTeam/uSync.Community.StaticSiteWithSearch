(function () {
    'use strict';

    function langController($scope, uSyncActionManager) {
        var pvm = this;

        pvm.loading = true;
        pvm.all = true;

        if ($scope.vm.content.variants.length == 1) {
            $scope.vm.performAction();
        }
        else {

            pvm.mode = $scope.vm.mode;
            pvm.isMedia = $scope.vm.isMedia;
            pvm.stepArgs = $scope.vm.stepArgs;
            pvm.flags = $scope.vm.flags;
            pvm.server = $scope.vm.selectedServer;

            if ($scope.model != null) {
                $scope.model.title = 'Select languages';
                $scope.model.subtitle = "Select Languages to publish to " + pvm.server.Name;
            }

            pvm.variants = [];
            $scope.vm.content.variants.forEach(function (variant) {
                variant._checked = true;
                pvm.variants.push(variant);
            });

            pvm.loading = false;
        }

        $scope.$watch('pvm.variants', function (newValue) {
            if (newValue !== undefined) {
                pvm.stepArgs.options.Cultures = [];
                $scope.vm.valid = false;

                for (let i = 0; i < newValue.length; i++) {
                    if (newValue[i]._checked === true) {
                        pvm.stepArgs.options.Cultures.push(newValue[i].language.culture);
                        $scope.vm.valid = true;
                    }
                }

                if (pvm.stepArgs.options.Cultures.length == newValue.length) {
                    // all selected
                    pvm.stepArgs.options.Cultures = []
                    pvm.all = true;
                }
                else {
                    pvm.all = false;
                }

            }
        }, true);
    }

    angular.module('umbraco')
        .controller('uSyncPublisherLanguageController', langController);

})();