﻿using Ec2Manager.Models.DataManagement;

namespace Ec2Manager.Interfaces
{
    public interface ISearchService
    {
        SearchResult GetSearchResult(string searchType, string query, int page, int pageSize, string sortOrder);
    }
}
