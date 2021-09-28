using Microsoft.Azure.Cosmos.Table;
using System;

namespace FunloadMetadata.Models
{
    public class MFDTableColumnEntity : TableEntity
    {
        public Int64 ColumnLength { get; set; }
        public string ColumnName { get; set; }
        public string DWEasyDataType { get; set; }
        public string DWSchemaName { get; set; }
        public Int64 DecimalPlaces { get; set; }
        public string FileSource { get; set; }
        public bool HasHistoryGroup { get; set; }
        public bool IsMemoryOptimized { get; set; }
        public string JavaDataType { get; set; }
        public bool Occurence { get; set; }
        public string RecordType { get; set; }
        public string RectypeReoccurYN { get; set; }
        public Int64 SequenceNumber { get; set; }
        public string SqlType { get; set; }
        public string StageColumnName { get; set; }
        public bool Occurs { get; set; }
        public string StageReoccurYN { get; set; }
        public string TableName { get; set; }
        public string TranslationRule { get; set; }
        public string HistoryTableName { get; set; }
    }
    public class MFDTableColumnDTO
    {
        public string DWSchemaName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string SqlType { get; set; }
        public string RecordType { get; set; }
        public string HistoryTableName { get; set; }
        public bool Occurs { get; set; }
        public string Alias { get; set; }

    }
}
