/*
 * MinIO .NET Library for Amazon S3 Compatible Cloud Storage, (C) 2020 MinIO, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Minio.DataModel;

using System;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Minio.Examples.Cases
{
    class SelectObjectContent
    {
        // Get object in a bucket
        public async static Task Run(MinioClient minio,
                                     string bucketName = "my-bucket-name",
                                     string objectName = "my-object-name",
                                     string fileName = "my-file-name")
        {
            string newObjectName = "new" + objectName;
            try
            {
                Console.WriteLine("Running example for API: SelectObjectContentAsync");

                StringBuilder csvString = new StringBuilder();
                csvString.AppendLine("Employee,Manager,Group");
                csvString.AppendLine("Employee4,Employee2,500");
                csvString.AppendLine("Employee3,Employee1,500");
                csvString.AppendLine("Employee1,,1000");
                csvString.AppendLine("Employee5,Employee1,500");
                csvString.AppendLine("Employee2,Employee1,800");
                var csvBytes = Encoding.UTF8.GetBytes(csvString.ToString());
                using (var stream = new MemoryStream(csvBytes))
                {
                    PutObjectArgs putObjectArgs = new PutObjectArgs()
                                                            .WithBucket(bucketName)
                                                            .WithObject(newObjectName)
                                                            .WithStreamData(stream)
                                                            .WithObjectSize(stream.Length);
                    await minio.PutObjectAsync(putObjectArgs);
                }
                QueryExpressionType queryType = QueryExpressionType.SQL;
                string queryExpr = "select count(*) from s3object";
                SelectObjectInputSerialization inputSerialization = new SelectObjectInputSerialization()
                {
                    CompressionType = SelectCompressionType.NONE,
                    CSV = new CSVInputOptions()
                    {
                        FileHeaderInfo = CSVFileHeaderInfo.None,
                        RecordDelimiter = "\n",
                        FieldDelimiter = ",",
                    }
                };
                SelectObjectOutputSerialization outputSerialization = new SelectObjectOutputSerialization()
                {
                    CSV = new CSVOutputOptions()
                    {
                        RecordDelimiter = "\n",
                        FieldDelimiter = ",",
                    }
                };
                SelectObjectContentArgs args = new SelectObjectContentArgs()
                                                            .WithBucket(bucketName)
                                                            .WithObject(newObjectName)
                                                            .WithExpressionType(queryType)
                                                            .WithQueryExpression(queryExpr)
                                                            .WithInputSerialization(inputSerialization)
                                                            .WithOutputSerialization(outputSerialization);
                SelectResponseStream resp = await minio.SelectObjectContentAsync(args);
                resp.Payload.CopyTo(Console.OpenStandardOutput());
                Console.WriteLine("Bytes scanned:" + resp.Stats.BytesScanned);
                Console.WriteLine("Bytes returned:" + resp.Stats.BytesReturned);
                Console.WriteLine("Bytes processed:" + resp.Stats.BytesProcessed);
                if (resp.Progress != null)
                {
                    Console.WriteLine("Progress :" + resp.Progress.BytesProcessed);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Bucket]  Exception: {e}");
            }
        }
    }
}
