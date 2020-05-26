function ExternalSiteSearchConfig(item) {
	var self = this;
	Object.assign(self, item || {});

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