using System.Text.Json.Serialization;


namespace TestProject.DataModel
{
    internal class RequestBody
    {
        [JsonPropertyName("query")]
        public string Query { get; } = "query SearchReportWoodDeal($size: Int!, " +
            "$number: Int!, $filter: Filter, $orders: [Order!]) {\n  " +
            "searchReportWoodDeal(filter: $filter, pageable: {number: $number, " +
            "size: $size}, orders: $orders) {\n    content {\n      sellerName\n      " +
            "sellerInn\n      buyerName\n      buyerInn\n      woodVolumeBuyer\n      " +
            "woodVolumeSeller\n      dealDate\n      dealNumber\n      __typename\n    " +
            "}\n    __typename\n  }\n}\n";

        [JsonPropertyName("variables")]
        public RequestVariables Variables { get; set; } = new RequestVariables();

        [JsonPropertyName("operationName")]
        public string OperationName { get; } = "SearchReportWoodDeal";
    }
}