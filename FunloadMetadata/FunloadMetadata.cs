using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections;
using System.Collections.Generic;
using FunloadMetadata.Models;


namespace FunloadTranslate
{
    public class FunloadMetadata
    {
        private int _maxMetadataVersion = -1;
        public int MaxMetadataVersion
        {
            get
            {
                if(_maxMetadataVersion == -1)
                {
                    _maxMetadataVersion = GetAzureMaxMetadataVersion();
                }

                return _maxMetadataVersion;
            }
        }
        private int GetAzureMaxMetadataVersion()
        {
            List<int> versions = new List<int>();

            CloudTable dvVersions = GetTable("dvVersions");
            List<VersionEntity> versionEntities =  PartitionScan<VersionEntity>(dvVersions, "YRC-M204");
            foreach(VersionEntity entity in versionEntities)
            {
                versions.Add(int.Parse(entity.RowKey));
            }
            versions.Sort();
            versions.Reverse();
            return versions.Count > 0 ? versions[0] : 0;
        }
        public M204FileDTO GetAzureFileData(int _version, string _m204File)
        {
            M204FileDTO result = null;
            CloudTable dvSourceTable = GetTable("dvSource");

            try
            {
                M204FileEntity entity = (M204FileEntity)SingleEntityQuery<M204FileEntity>(dvSourceTable, $"YRC-M204-{_version}", _m204File);
                result = new M204FileDTO()
                {
                    M204File = entity.RowKey,
                    RecTypeColumn = entity.rectype,
                    IsActive = entity.isActive
                };
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
            return result;
        }
        public List<MFDTableDTO> GetAzureMFDTables(int _version, string _m204File)
        {
            string startRowKey = $"{_version}-{_m204File}-";

            CloudTable dvDWSourceTable = GetTable("dvDWSource");
            List<MFDTableEntity> dwEntities = PartitionRangeScan<MFDTableEntity>(dvDWSourceTable, "YRC-M204", startRowKey);

            List<MFDTableDTO> mfdTables = new List<MFDTableDTO>();

            foreach (MFDTableEntity entity in dwEntities)
            {
                if(entity.RowKey.StartsWith(startRowKey))
                {
                    mfdTables.Add(new MFDTableDTO()
                    {
                        M204File = _m204File,
                        TableName = entity.TableName,
                        ReoccurTableName = (entity.DW_Rectype_Reoccur == 1 ? $"{entity.TableName}_REOCCUR" : ""),
                        Rectype = (entity.RectypeValue == "No Rectype Value" ? "No Rectype" : entity.RectypeValue)
                    });
                }
            }
            return mfdTables;
        }
        public List<MFDTableColumnDTO> GetAzureMFDColumns(int _version, string _table, string _rectype)
        {
            string startRectypeValue = (_rectype == "No Rectype" ? "" : $"_{_rectype}");
            string endRectypeValue = (_rectype == "No Rectype" ? "." : $"_{_rectype}.");
            string startReoccurValue = (_rectype == "No Rectype" ? "_REOCCUR" : $"_{_rectype}_REOCCUR");
            string endReoccurValue = (_rectype == "No Rectype" ? "_REOCCUR." : $"_{_rectype}_REOCCUR.");
            string startRowKey = $"{_version}-{_table}-{_table}{startRectypeValue}";
            string endRowKey = $"{_version}-{_table}-{_table}{endRectypeValue}";

            CloudTable dvDWEntitiesTable = GetTable("dvDWEntity");
            List<MFDTableColumnEntity> dwEntities = PartitionRangeQuery<MFDTableColumnEntity>(dvDWEntitiesTable, "YRC-M204", startRowKey, endRowKey);

            startRowKey = $"{_version}-{_table}-{_table}{startReoccurValue}";
            endRowKey = $"{_version}-{_table}-{_table}{endReoccurValue}";
            List<MFDTableColumnEntity> reoccurEntities = PartitionRangeQuery<MFDTableColumnEntity>(dvDWEntitiesTable, "YRC-M204", startRowKey, endRowKey);
            if (reoccurEntities != null)
            {
                dwEntities.AddRange(reoccurEntities);
            }

            List<MFDTableColumnDTO> mfdColumns = new List<MFDTableColumnDTO>();

            foreach (MFDTableColumnEntity entity in dwEntities)
            {
                mfdColumns.Add(new MFDTableColumnDTO()
                {
                    DWSchemaName = entity.DWSchemaName,
                    TableName = entity.TableName,
                    ColumnName = entity.ColumnName,
                    SqlType = entity.SqlType,
                    RecordType = entity.RecordType,
                    HistoryTableName = entity.HistoryTableName,
                    Occurs = (entity.TableName.Contains("REOCCUR") ? true : false),
                    Alias = (entity.TableName.Contains("REOCCUR") ? "reoccur" : "base")
                });
            }
            return mfdColumns;
        }
        public static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }
        //  <CreateTable>
        public static CloudTable GetTable(string tableName)
        {
            string storageConnectionString = AppSettings.LoadAppSettings().StorageConnectionString;

            // Retrieve storage account information from connection string.
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            CloudTable table = tableClient.GetTableReference(tableName);

            return table;
        }
        //  </GetTable>

        //  </createStorageAccount>
        /// <summary>
        /// Demonstrate a partition scan whereby we are searching for all the entities within a partition. Note this is not as efficient 
        /// as a range scan - but definitely more efficient than a full table scan. The async APIs require that the user handle the segment 
        /// size and return the next segment using continuation tokens.
        /// </summary>
        /// <param name="table">Sample table name</param>
        /// <param name="partitionKey">The partition within which to search</param>
        /// <returns>A Task object</returns>
        private static List<T> PartitionScan<T>(CloudTable table, string partitionKey) where T : TableEntity, new()
        {
            List<T> entities = new List<T>();
            try
            {
                TableQuery<T> partitionScanQuery =
                    new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

                TableContinuationToken token = null;

                // Read entities from each query segment.
                do
                {
                    TableQuerySegment<T> segment = table.ExecuteQuerySegmented(partitionScanQuery, token);
                    token = segment.ContinuationToken;
                    entities.AddRange(segment.Results as List<T>);
                }
                while (token != null);
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            return entities;
        }
        private static List<T> PartitionRangeScan<T>(CloudTable table, string partitionKey, string startRowKey) where T : TableEntity, new()
        {
            List<T> entities = new List<T>();
            try
            {
                // Create the range query using the fluid API 
                TableQuery<T> rangeQuery = new TableQuery<T>().Where(
                    TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                            TableOperators.And,
                                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, startRowKey)));

                // Request 50 results at a time from the server. 
                TableContinuationToken token = null;
                rangeQuery.TakeCount = 50;
                do
                {
                    // Execute the query, passing in the continuation token.
                    // The first time this method is called, the continuation token is null. If there are more results, the call
                    // populates the continuation token for use in the next call.
                    TableQuerySegment<T> segment = table.ExecuteQuerySegmented<T>(rangeQuery, token);

                    // Save the continuation token for the next call to ExecuteQuerySegmentedAsync
                    token = segment.ContinuationToken;
                    entities.AddRange(segment.Results as List<T>);
                }
                while (token != null);
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            return entities;
        }
        /// <summary>
        /// Demonstrate a partition range query that searches within a partition for a set of entities that are within a 
        /// specific range. The async APIs require that the user handle the segment size and return the next segment 
        /// using continuation tokens. 
        /// </summary>
        /// <param name="table">Sample table name</param>
        /// <param name="partitionKey">The partition within which to search</param>
        /// <param name="startRowKey">The lowest bound of the row key range within which to search</param>
        /// <param name="endRowKey">The highest bound of the row key range within which to search</param>
        /// <returns>A Task object</returns>
        private static List<T> PartitionRangeQuery<T>(CloudTable table, string partitionKey, string startRowKey, string endRowKey) where T : TableEntity, new()
        {
            List<T> entities = new List<T>();
            try
            {
                // Create the range query using the fluid API 
                TableQuery<T> rangeQuery = new TableQuery<T>().Where(
                    TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                            TableOperators.And,
                            TableQuery.CombineFilters(
                                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, startRowKey),
                                TableOperators.And,
                                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, endRowKey))));

                // Request 50 results at a time from the server. 
                TableContinuationToken token = null;
                rangeQuery.TakeCount = 50;
                do
                {
                    // Execute the query, passing in the continuation token.
                    // The first time this method is called, the continuation token is null. If there are more results, the call
                    // populates the continuation token for use in the next call.
                    TableQuerySegment<T> segment = table.ExecuteQuerySegmented<T>(rangeQuery, token);

                    // Save the continuation token for the next call to ExecuteQuerySegmentedAsync
                    token = segment.ContinuationToken;
                    entities.AddRange(segment.Results as List<T>);
                }
                while (token != null);
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            return entities;
        }
        /// <summary>
        /// Demonstrate a partition range query that searches within a partition for a set of entities that are within a 
        /// specific range. The async APIs require that the user handle the segment size and return the next segment 
        /// using continuation tokens. 
        /// </summary>
        /// <param name="table">Sample table name</param>
        /// <param name="partitionKey">The partition within which to search</param>
        /// <param name="startRowKey">The lowest bound of the row key range within which to search</param>
        /// <param name="endRowKey">The highest bound of the row key range within which to search</param>
        /// <returns>A Task object</returns>
        private static TableEntity SingleEntityQuery<T>(CloudTable table, string partitionKey, string rowKey) where T : TableEntity, new()
        {
            TableResult result = new TableResult();
            try
            {
                TableOperation operation = TableOperation.Retrieve<T>(partitionKey, rowKey);
                result = table.Execute(operation);
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            return (T)(dynamic)result.Result;
        }
    }
}
