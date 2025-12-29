using Amazon.XRay;
using Amazon.XRay.Model;
using System.Threading.Tasks;

namespace AmazonSDKWrapper
{ 
    public class XRayClient : IXRayClient
    {
        private readonly IAmazonXRay _xrayClient;

        public XRayClient(IAmazonXRay xrayClient)
        {
            _xrayClient = xrayClient;
        }

        public Task<GetTraceSummariesResponse> GetTraceSummariesAsync(GetTraceSummariesRequest request)
        {
            return _xrayClient.GetTraceSummariesAsync(request);
        }

        public Task<BatchGetTracesResponse> BatchGetTracesAsync(BatchGetTracesRequest request)
        {
            return _xrayClient.BatchGetTracesAsync(request);
        }

        public Task<PutTraceSegmentsResponse> PutTraceSegmentsAsync(PutTraceSegmentsRequest request)
        {
            return _xrayClient.PutTraceSegmentsAsync(request);
        }
    }
}