using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace XRayConnector
{
    public class WorkflowConfig
    {
        private const uint DefaultPollingIntervalSeconds = 180;
        private const uint DefaultMaximumReplayHistorySeconds = 900;

        private readonly uint _pollingIntervalSeconds;
        public uint PollingIntervalSeconds { get => _pollingIntervalSeconds; }

        private readonly uint _maximumReplayHistorySeconds;
        public uint MaximumReplayHistorySeconds { get => _maximumReplayHistorySeconds; }

        private readonly bool _autoStart;
        public bool AutoStart { get => _autoStart; }



        public WorkflowConfig(ILogger<WorkflowConfig> log)
        {
            if (!UInt32.TryParse(Environment.GetEnvironmentVariable("PollingIntervalSeconds"), out _pollingIntervalSeconds))
            {
                if (UInt32.TryParse(Environment.GetEnvironmentVariable("PollingIntervalMinutes"), out uint pollingIntervalMinutes)) // for backwards compatibility with older versions
                {
                    _pollingIntervalSeconds = pollingIntervalMinutes * 60;
                }
                else
                {
                    _pollingIntervalSeconds = DefaultPollingIntervalSeconds;
                    log.LogWarning($"Config(PollingIntervalSeconds) not set, using default value ({_pollingIntervalSeconds})");
                }
            }

            if (!UInt32.TryParse(Environment.GetEnvironmentVariable("MaximumReplayHistorySeconds"), out _maximumReplayHistorySeconds))
            {
                _maximumReplayHistorySeconds = DefaultMaximumReplayHistorySeconds; 
                log.LogInformation($"Config(MaximumReplayHistorySeconds) not set, using default value ({_maximumReplayHistorySeconds})");
            }

            _autoStart = bool.TryParse(Environment.GetEnvironmentVariable("AutoStart"), out bool result) && result;
        }


    }
}
