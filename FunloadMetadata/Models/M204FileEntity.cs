using Microsoft.Azure.Cosmos.Table;

namespace FunloadMetadata.Models
{
    public class M204FileEntity : TableEntity
    {
        public bool isActive { get; set; }
        public string rectype { get; set; }
    }
    public class M204FileDTO
    {
        public string M204File { get; set; }
        public bool IsActive { get; set; }
        public string RecTypeColumn { get; set; }
    }
}
