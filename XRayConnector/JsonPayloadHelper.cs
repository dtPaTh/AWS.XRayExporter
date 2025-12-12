using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace XRayConnector
{

    public class JsonPayloadHelper
    {
        private readonly bool _CompressionEnabled;
        public bool CompressionEnabled => _CompressionEnabled;
        public JsonPayloadHelper(bool enableCompression = true)
        {
            _CompressionEnabled = enableCompression;
        }

        public string Serialize(string jsonData)
        {
            if (_CompressionEnabled)
            {
                var compressedBytes = Compress(jsonData);
                return Convert.ToBase64String(compressedBytes);
            }
            else
            {
                return jsonData;
            }
        }

        public string Deserialize(string data)
        {
            if (_CompressionEnabled)
            {
                var bytes = Convert.FromBase64String(data);
                return Decompress(bytes);
            }
            else
            {
                return data;
            }
        }

        private byte[] Compress(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
                throw new ArgumentNullException(nameof(jsonData));

            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);

            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    gzipStream.Write(jsonBytes, 0, jsonBytes.Length);
                }
                return outputStream.ToArray();
            }
        }

        private string Decompress(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0)
                throw new ArgumentNullException(nameof(compressedData));

            using (var inputStream = new MemoryStream(compressedData))
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                gzipStream.CopyTo(outputStream);
                return Encoding.UTF8.GetString(outputStream.ToArray());
            }
        }
    }
}