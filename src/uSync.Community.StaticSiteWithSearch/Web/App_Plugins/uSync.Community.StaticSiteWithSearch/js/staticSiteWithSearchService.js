/**
 * @ngdoc
 * @name uSync.Community.StaticSiteWithSearch.ExternalSiteSearchService
 * @requires $http
 * 
 * @description provides the link to the external site search api methods required for the dashboard to function
 */

(function () {
    'use strict';

    function externalSiteSearchService($http) {

        var serviceRoot = Umbraco.Sys.ServerVariables.uSync.Community.StaticSiteWithSearch.ExternalSiteSearch.serviceRoot;

        var service = {
            getKnownSites: getKnownSites,
            getTotalRecords: getTotalRecords,
            rebuildIndex: rebuildIndex,
            getRebuildStatus: getRebuildStatus,
            searchIndex: searchIndex
        };

        return service;

        /////////////////////

        function getKnownSites() {
            return $http.get(serviceRoot + 'GetKnownSites');
        }

        function getTotalRecords(uniqueName) {
            return $http.get(serviceRoot + 'GetTotalRecords?uniqueName=' + encodeURIComponent(uniqueName));
        }

        function rebuildIndex(uniqueName) {
            return $http.get(serviceRoot + 'RebuildIndex?uniqueName=' + encodeURIComponent(uniqueName));
        }

        function getRebuildStatus(rebuildId) {
            return $http.get(serviceRoot + 'GetRebuildStatus?rebuildId=' + encodeURIComponent(rebuildId));
        }

        function searchIndex(uniqueName, term, page) {
            return $http.get(serviceRoot + 'SearchIndex?uniqueName=' + encodeURIComponent(uniqueName) + '&term=' + encodeURIComponent(term) + '&page=' + encodeURIComponent(page));
        }
    }

    angular.module('umbraco.services')
        .factory('uSync.Community.StaticSiteWithSearch.ExternalSiteSearchService', externalSiteSearchService);

})();