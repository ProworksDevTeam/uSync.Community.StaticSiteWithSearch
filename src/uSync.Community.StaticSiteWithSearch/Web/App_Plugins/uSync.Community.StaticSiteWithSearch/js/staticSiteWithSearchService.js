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

        function getTotalRecords(siteId) {
            return $http.get(serviceRoot + 'GetTotalRecords?siteId=' + encodeURIComponent(siteId));
        }

        function rebuildIndex(siteId) {
            return $http.get(serviceRoot + 'RebuildIndex?siteId=' + encodeURIComponent(siteId));
        }

        function getRebuildStatus(rebuildId) {
            return $http.get(serviceRoot + 'GetRebuildStatus?rebuildId=' + encodeURIComponent(rebuildId));
        }

        function searchIndex(siteId, term, page) {
            return $http.get(serviceRoot + 'SearchIndex?siteId=' + encodeURIComponent(siteId) + '&term=' + encodeURIComponent(term) + '&page=' + encodeURIComponent(page));
        }
    }

    angular.module('umbraco.services')
        .factory('uSync.Community.StaticSiteWithSearch.ExternalSiteSearchService', externalSiteSearchService);

})();