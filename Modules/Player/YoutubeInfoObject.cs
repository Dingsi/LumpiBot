using System;
using System.Collections.Generic;
using System.Text;

namespace LumpiBot.Modules.Player
{
    public class YTPageInfo
    {
        public int totalResults { get; set; }
        public int resultsPerPage { get; set; }
    }

    public class YTRegionRestriction
    {
        public List<string> allowed { get; set; }
    }

    public class YTContentDetails
    {
        public string duration { get; set; }
        public string dimension { get; set; }
        public string definition { get; set; }
        public string caption { get; set; }
        public bool licensedContent { get; set; }
        public YTRegionRestriction regionRestriction { get; set; }
        public string projection { get; set; }
    }

    public class YTItem
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public YTContentDetails contentDetails { get; set; }
    }

    public class YTRootObject
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public YTPageInfo pageInfo { get; set; }
        public List<YTItem> items { get; set; }
    }
}
