using System.Diagnostics;

namespace AzureDeprecation.Runtimes.AzureFunctions
{
    public struct ValueStopwatch
    {
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
        private static readonly double TimestampToMilliseconds = TimeSpan.TicksPerSecond / (Stopwatch.Frequency * (double)TimeSpan.TicksPerMillisecond);

        private readonly long _startTimestamp;

        public bool IsActive => _startTimestamp != 0;

        private ValueStopwatch(long startTimestamp)
        {
            _startTimestamp = startTimestamp;
        }

        public static ValueStopwatch StartNew() => new ValueStopwatch(Stopwatch.GetTimestamp());

        public TimeSpan GetElapsedTime()
        {
            if (_startTimestamp == 0) throw new InvalidOperationException("An uninitialized, or 'default', ValueStopwatch cannot be used to get elapsed time.");

            var end = Stopwatch.GetTimestamp();
            var timestampDelta = end - _startTimestamp;
            var ticks = (long)(TimestampToTicks * timestampDelta);
            return new TimeSpan(ticks);
        }

        public double GetElapsedTotalMilliseconds()
        {
            if (_startTimestamp == 0) throw new InvalidOperationException("An uninitialized, or 'default', ValueStopwatch cannot be used to get elapsed time.");

            var end = Stopwatch.GetTimestamp();
            var timestampDelta = end - _startTimestamp;
            return Math.Round(TimestampToMilliseconds * timestampDelta, 4);
        }
    }
}