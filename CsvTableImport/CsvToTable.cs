using DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CsvTableImport
{
    public static class CsvToTable
    {
        [FunctionName("ReadCsvAndInsertTableRows")]
        public static void Run([BlobTrigger("unzipped/data/{name}", Connection = "")]Stream myBlob, string name, [Table("obdData")] CloudTable outputTable, TraceWriter log)
        {
            log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes.");            
            var dt = DataTable.New.Read(new StreamReader(myBlob));
            log.Info($"C# Read Csv from stream.");
            try
            {
                foreach (Row row in dt.Rows)
                {
                    outputTable.Execute(TableOperation.Insert(new DynamicTableEntity("Infiniti_G37xS", Guid.NewGuid().ToString(), "*", ConvertRow(row))));
                    log.Info($"C# Saved DataTable row to table");
                }

                log.Info($"C# Saved Csv to table");
            }
            catch (Exception ex)
            {
                log.Error("Exception caught: " + ex.Message, ex);
            }
        }

        private static IDictionary<string, EntityProperty> ConvertRow(Row row)
        {
            Dictionary<string, EntityProperty> entityRow = new Dictionary<string, EntityProperty>();
            int i = 0;
            foreach (string header in row.ColumnNames)
            {
                entityRow.Add(header.ToAzureKeyString().Replace("[IADV]", "").Replace(" ", "") + i, new EntityProperty(row.GetValueOrEmpty(header)));
                i++;
            }

            return entityRow;
        }

        public static string ToAzureKeyString(this string str)
        {
            var sb = new StringBuilder();
            foreach (var c in str
                .Where(c => (char.IsLetter(c))
                            && !char.IsControl(c)))
                sb.Append(c);
            return sb.ToString();
        }
    }
}
