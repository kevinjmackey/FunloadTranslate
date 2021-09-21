using Microsoft.Azure.Cosmos.Table;
using System;


namespace FunloadMetadata.Models
{
    class MFDTableEntity : TableEntity
    {
        public string DWSchemaName { get; set; }
        public Int64 DW_Rectype_Reoccur { get; set; }
        public string IsHistoryTable { get; set; }
        public string RecordType { get; set; }
        public string RectypeValue { get; set; }
        public string TableName { get; set; }
    }
    public class MFDTableDTO
    {
        public string M204File { get; set; }
        public string TableName { get; set; }
        public string ReoccurTableName { get; set; }
        public string Rectype { get; set; }
        public string FromCaluse { get; set; }
        public string JoinCaluse { get; set; }

    }
}
