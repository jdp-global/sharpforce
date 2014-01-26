﻿using System.Collections.Generic;

namespace SalesforceSharp.Responses
{
    public class DescribeGlobalResponse : SalesforceResponse
    {
        public string Encoding { get; set; }
        public List<DescribeResponse> SObjects { get; set; }
        public int MaxBatchSize { get; set; }
    }
}