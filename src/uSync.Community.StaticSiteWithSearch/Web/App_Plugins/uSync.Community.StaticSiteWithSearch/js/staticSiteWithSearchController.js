/**
 * @ngdoc
 * @name uSync.Community.StaticSiteWithSearch.ExternalSiteSearchController
 * @requires $scope
 * @requires $timeout
 * @requires uSync.Community.StaticSiteWithSearch.ExternalSiteSearchService
 *
 * @description provides the link to the external site search api methods required for the dashboard to function
 */

(function () {
    'use strict';

    function externalSiteSearchController($scope, $timeout, localizationService, editorService, externalSiteSearchService) {
        var vm = this;

        vm.loading = true;
        vm.sites = [];
        vm.selectedSite = null;
        vm.searchText = '';
        vm.searchResults = null;

        vm.showIndexInfo = function (site) {
            vm.selectedSite = site;
        };

        vm.showList = function () {
            vm.selectedSite = null;
            vm.searchText = '';
            vm.searchResults = null;
        };

        vm.search = function () {
            if (!vm.selectedSite) return;

            doSearch(vm.selectedSite, vm.searchText, 1);
        };

        vm.fieldCount = function (result) {
            var fields = 0;

            for (var key in result) {
                if (Object.prototype.hasOwnProperty.call(result, key)) fields++;
            }

            return fields;
        };

        vm.showSearchResultDialog = function (result) {
            localizationService.localize("examineManagement_fieldValues").then(function (value) {
                editorService.open({
                    title: value,
                    searchResultValues: result,
                    size: "medium",
                    view: "views/dashboard/settings/examinemanagementresults.html",
                    close: function () {
                        editorService.close();
                    }
                });
            });
        };

        vm.nextSearchResultPage = function () {
            if (!vm.searchResults || vm.searchResults.pageNumber >= vm.searchResults.totalPages) return;
            doSearch(vm.searchResults.site, vm.searchResults.term, vm.searchResults.pageNumber + 1);
        };

        vm.prevSearchResultPage = function () {
            if (!vm.searchResults || vm.searchResults.pageNumber <= 1) return;
            doSearch(vm.searchResults.site, vm.searchResults.term, vm.searchResults.pageNumber + 1);
        };

        vm.goToPageSearchResultPage = function (page) {
            if (!vm.searchResults || page < 1 || page > vm.searchResults.totalPages) return;
            doSearch(vm.searchResults.site, vm.searchResults.term, page);
        };

        vm.rebuildIndex = function () {
            var site = vm.selectedSite;
            if (!site || !site.siteId || !site.hasUpdateApiKey || !site.hasIndexDataFile) return;

            site.isProcessing = true;
            externalSiteSearchService.rebuildIndex(site.siteId).then(({ data }) => { handleRebuildResult(site, data); }).catch(function () { site.isProcessing = false; });
        };

        function doSearch(site, term, page) {
            site.isProcessing = true;

            externalSiteSearchService.searchIndex(site.siteId, term, page).then(({ data }) => {
                if (data) {
                    vm.searchResults = new ExternalSiteSearchResult(site, term, data);
                }
            }).finally(() => { site.isProcessing = false; });
        }

        function handleRebuildResult(site, rebuildId, resultData) {
            if (!resultData || !(resultData.success || resultData.error)) {
                return $timeout(function () {
                    externalSiteSearchService.getRebuildStatus(rebuildId).then(({ data }) => { handleRebuildResult(site, rebuildId, data); }).catch(function () { site.isProcessing = false; });
                }, 1000);
            }

            updateTotalRecords(site, resultData);
        }

        function updateTotalRecords(site, resultData) {
            site.isProcessing = false;

            if (resultData && resultData.success) {
                site.totalRecords = resultData.totalRecords || 0;
                site.isHealthy = true;
                site.healthStatus = "Healthy";
            } else {
                site.isHealthy = false;
                site.healthStatus = (resultData || {}).error || 'An unknown problem has occurred';
            }
        }

        function getTotalRecords(site) {
            site.isProcessing = true;

            externalSiteSearchService.getTotalRecords(site.siteId).then(({ data }) => {
                updateTotalRecords(site, data);
            }).finally(() => site.isProcessing = false);
        }

        function init() {
            externalSiteSearchService.getKnownSites().then(({ data }) => {
                if (data) {
                    vm.sites.splice(0, vm.sites.length);

                    for (var i = 0; i < data.length; i++) {
                        var model = new ExternalSiteSearchConfig(data[i]);
                        vm.sites.push(model);
                        getTotalRecords(vm.sites[i]);
                    }
                }
            }).finally(() => { vm.loading = false; });
        }

        init();
    }

    angular.module('umbraco')
        .controller('uSync.Community.StaticSiteWithSearch.ExternalSiteSearchController', ['$scope', '$timeout', 'localizationService', 'editorService', 'uSync.Community.StaticSiteWithSearch.ExternalSiteSearchService', externalSiteSearchController]);

})();