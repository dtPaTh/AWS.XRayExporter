using Amazon.XRay;
using Amazon.XRay.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmazonSDKWrapper
{ 
    public class XRayClientSimulator : IXRayClient
    {
        public enum BatchSegmentMode
        {
            Never,
            Always,
            Random
        }

        private readonly ushort _traceCountPerRequest;
        private readonly byte _pageSize;

        private readonly BatchSegmentMode _batchSegementsMode;
        private bool _batchSegments;

        public XRayClientSimulator(ushort traceCountPerSummariesRequest = 10, byte pageSize = 2, BatchSegmentMode batchSegmentsMode = BatchSegmentMode.Never)
        {
            _traceCountPerRequest = traceCountPerSummariesRequest;
            _pageSize = pageSize;
            _batchSegementsMode = batchSegmentsMode;

            if (_batchSegementsMode == BatchSegmentMode.Never)
                _batchSegments = false;
            else if (_batchSegementsMode == BatchSegmentMode.Always)
                _batchSegments = true;

        }

        public Task<GetTraceSummariesResponse> GetTraceSummariesAsync(GetTraceSummariesRequest request)
        {
            if (_batchSegementsMode == BatchSegmentMode.Random)
                _batchSegments = new Random().NextDouble() < 0.25;

            var summaries = new List<TraceSummary>();
            for (int i = 0; i < _pageSize; i++) //only create samples with page size number of summaries, even if more are requested, as we only return the page size number of summaries
            {
                summaries.Add(new TraceSummary
                {
                    Id = $"1-{BitConverter.ToString(Guid.NewGuid().ToByteArray()).Replace("-", string.Empty).ToLower().Insert(8, "-")}",
                    Duration = 1.0,
                    ResponseTime = 1.0,
                    HasError = false,
                    HasFault = false,
                    HasThrottle = false,
                    IsPartial = false
                });
            }

            string nextToken = null;
            if (request.NextToken != null)
            {
                var tokenParts = request.NextToken.Split('_');
                int batchNumber = int.Parse(tokenParts[0]) + 1;
                if (batchNumber < _traceCountPerRequest / _pageSize) 
                    nextToken = $"{batchNumber}_{tokenParts[1]}";
                else
                    nextToken = null; 
            }
            else if (_traceCountPerRequest > _pageSize)
            {
                nextToken = $"1_{Guid.NewGuid()}";
            }

            var response = new GetTraceSummariesResponse
            {
                TraceSummaries = summaries,
                NextToken = nextToken
            };

            return Task.FromResult(response);
        }

        private List<Segment> GetSampleSegments(string traceid, bool batched = false, int batchNumber = 0)
        {
            var segments = new List<Segment>();

            if (!batched || (batched && batchNumber == 0))
                segments.AddRange(
                    new Segment
                    {
                        Id = "1fb07842d944e714",
                        Document = $"{{\"id\":\"1fb07842d944e714\",\"name\":\"random-name\",\"start_time\":1.499473411677E9,\"end_time\":1.499473414572E9,\"parent_id\":\"0c544c1b1bbff948\",\"http\":{{\"response\":{{\"status\":200}}}},\"aws\":{{\"request_id\":\"ac086670-6373-11e7-a174-f31b3397f190\"}},\"trace_id\":\"{traceid}\",\"origin\":\"AWS::Lambda\",\"resource_arn\":\"arn:aws:lambda:us-west-2:123456789012:function:random-name\"}}"
                    },
                    new Segment
                    {
                        Id = "194fcc8747581230",
                        Document = $"{{\"id\":\"194fcc8747581230\",\"name\":\"Scorekeep\",\"start_time\":1.499473411562E9,\"end_time\":1.499473414794E9,\"http\":{{\"request\":{{\"url\":\"http://scorekeep.elasticbeanstalk.com/api/user\",\"method\":\"POST\",\"user_agent\":\"Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36\",\"client_ip\":\"205.251.233.183\"}},\"response\":{{\"status\":200}}}},\"aws\":{{\"elastic_beanstalk\":{{\"version_label\":\"app-abb9-170708_002045\",\"deployment_id\":406,\"environment_name\":\"scorekeep-dev\"}},\"ec2\":{{\"availability_zone\":\"us-west-2c\",\"instance_id\":\"i-0cd9e448944061b4a\"}},\"xray\":{{\"sdk_version\":\"1.1.2\",\"sdk\":\"X-Ray for Java\"}}}},\"service\":{{}},\"trace_id\":\"{traceid}\",\"user\":\"5M388M1E\",\"origin\":\"AWS::ElasticBeanstalk::Environment\",\"subsegments\":[{{\"id\":\"0c544c1b1bbff948\",\"name\":\"Lambda\",\"start_time\":1.499473411629E9,\"end_time\":1.499473414572E9,\"http\":{{\"response\":{{\"status\":200,\"content_length\":14}}}},\"aws\":{{\"log_type\":\"None\",\"status_code\":200,\"function_name\":\"random-name\",\"invocation_type\":\"RequestResponse\",\"operation\":\"Invoke\",\"request_id\":\"ac086670-6373-11e7-a174-f31b3397f190\",\"resource_names\":[\"random-name\"]}},\"namespace\":\"aws\"}},{{\"id\":\"071684f2e555e571\",\"name\":\"## UserModel.saveUser\",\"start_time\":1.499473414581E9,\"end_time\":1.499473414769E9,\"metadata\":{{\"debug\":{{\"test\":\"Metadata string from UserModel.saveUser\"}}}},\"subsegments\":[{{\"id\":\"4cd3f10b76c624b4\",\"name\":\"DynamoDB\",\"start_time\":1.49947341469E9,\"end_time\":1.499473414769E9,\"http\":{{\"response\":{{\"status\":200,\"content_length\":57}}}},\"aws\":{{\"table_name\":\"scorekeep-user\",\"operation\":\"UpdateItem\",\"request_id\":\"MFQ8CGJ3JTDDVVVASUAAJGQ6NJ82F738BOB4KQNSO5AEMVJF66Q9\",\"resource_names\":[\"scorekeep-user\"]}},\"namespace\":\"aws\"}}]}}]}}"
                    },
                    new Segment
                    {
                        Id = "00f91aa01f4984fd",
                        Document = $"{{\"id\":\"00f91aa01f4984fd\",\"name\":\"random-name\",\"start_time\":1.49947341283E9,\"end_time\":1.49947341457E9,\"parent_id\":\"1fb07842d944e714\",\"aws\":{{\"function_arn\":\"arn:aws:lambda:us-west-2:123456789012:function:random-name\",\"resource_names\":[\"random-name\"],\"account_id\":\"123456789012\"}},\"trace_id\":\"{traceid}\",\"origin\":\"AWS::Lambda::Function\",\"subsegments\":[{{\"id\":\"e6d2fe619f827804\",\"name\":\"annotations\",\"start_time\":1.499473413012E9,\"end_time\":1.499473413069E9,\"annotations\":{{\"UserID\":\"5M388M1E\",\"Name\":\"Ola\"}}}},{{\"id\":\"b29b548af4d54a0f\",\"name\":\"SNS\",\"start_time\":1.499473413112E9,\"end_time\":1.499473414071E9,\"http\":{{\"response\":{{\"status\":200}}}},\"aws\":{{\"operation\":\"Publish\",\"region\":\"us-west-2\",\"request_id\":\"a2137970-f6fc-5029-83e8-28aadeb99198\",\"retries\":0,\"topic_arn\":\"arn:aws:sns:us-west-2:123456789012:awseb-e-ruag3jyweb-stack-NotificationTopic-6B829NT9V5O9\"}},\"namespace\":\"aws\"}},{{\"id\":\"2279c0030c955e52\",\"name\":\"Initialization\",\"start_time\":1.499473412064E9,\"end_time\":1.499473412819E9,\"aws\":{{\"function_arn\":\"arn:aws:lambda:us-west-2:123456789012:function:random-name\"}}}}]}}"
                    });

            if (!batched || (batched && batchNumber == 1))
                segments.AddRange(
                    new Segment
                        {
                            Id = "17ba309b32c7fbaf",
                            Document = $"{{\"id\":\"17ba309b32c7fbaf\",\"name\":\"DynamoDB\",\"start_time\":1.49947341469E9,\"end_time\":1.499473414769E9,\"parent_id\":\"4cd3f10b76c624b4\",\"inferred\":true,\"http\":{{\"response\":{{\"status\":200,\"content_length\":57}}}},\"aws\":{{\"table_name\":\"scorekeep-user\",\"operation\":\"UpdateItem\",\"request_id\":\"MFQ8CGJ3JTDDVVVASUAAJGQ6NJ82F738BOB4KQNSO5AEMVJF66Q9\",\"resource_names\":[\"scorekeep-user\"]}},\"trace_id\":\"{traceid}\",\"origin\":\"AWS::DynamoDB::Table\"}}"
                        },
                        new Segment
                        {
                            Id = "1ee3c4a523f89ca5",
                            Document = $"{{\"id\":\"1ee3c4a523f89ca5\",\"name\":\"SNS\",\"start_time\":1.499473413112E9,\"end_time\":1.499473414071E9,\"parent_id\":\"b29b548af4d54a0f\",\"inferred\":true,\"http\":{{\"response\":{{\"status\":200}}}},\"aws\":{{\"operation\":\"Publish\",\"region\":\"us-west-2\",\"request_id\":\"a2137970-f6fc-5029-83e8-28aadeb99198\",\"retries\":0,\"topic_arn\":\"arn:aws:sns:us-west-2:123456789012:awseb-e-ruag3jyweb-stack-NotificationTopic-6B829NT9V5O9\"}},\"trace_id\":\"{traceid}\",\"origin\":\"AWS::SNS\"}}"
                        });
            
            return segments;
        }
        public Task<BatchGetTracesResponse> BatchGetTracesAsync(BatchGetTracesRequest request)
        {
            var traces = new List<Trace>();
            foreach (var traceid in request.TraceIds)
            {
                traces.Add(
                    new Trace
                    {
                        Id = traceid,
                        Segments = GetSampleSegments(traceid, _batchSegments, String.IsNullOrEmpty(request.NextToken)?0:1)
                    }
                );
            }

            var response = new BatchGetTracesResponse
            {
                Traces = traces,
                NextToken = _batchSegments && String.IsNullOrEmpty(request.NextToken) ? "1": null
            };

            return Task.FromResult(response);
        }

        public Task<PutTraceSegmentsResponse> PutTraceSegmentsAsync(PutTraceSegmentsRequest request)
        {
            return Task.FromResult(new PutTraceSegmentsResponse());
        }
    }
}