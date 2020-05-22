function ExternalSiteSearchConfig(item) {
	var self = this;
	item = item || {};

	self.siteId = item.siteId || null;
	self.url = item.url || null;
	self.folderPath = item.folderPath || null;
	self.applicationId = item.applicationId || null;
	self.indexName = item.indexName || null;
	self.hasSearchApiKey = item.hasSearchApiKey || false;
	self.hasUpdateApiKey = item.hasUpdateApiKey || false;
	self.hasIndexDataFile = item.hasIndexDataFile || false;

	self.totalRecords = 0;
	self.isHealthy = false;
	self.healthStatus = null;
	self.isProcessing = false;
	self.processingAttempts = 0;
}

function ExternalSiteSearchResult(site, term, item) {
	var self = this;
	item = item || {};

	self.site = site || null;
	self.term = term || null;
	self.results = item.results || [];
	self.pageNumber = item.pageNumber || 1;
	self.totalPages = item.totalPages || 1;
}