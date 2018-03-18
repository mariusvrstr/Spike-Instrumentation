
namespace Spike.Instrumentation.Monitoring
{
    using System;
    using System.Globalization;
    using System.Linq;

    public class LoopQueue
    {
        public int?[] Queue { get; set; }

        private int QueueLength { get; }

        private int Position { get; set; }

        public LoopQueue(IntervalType intervalType, int iterationTimeInMinutes)
        {
            QueueLength = GetRequiredQueueLengthFromIntervalType(intervalType, iterationTimeInMinutes);
            Queue = new int?[QueueLength];
            Position = 0;
        }

        private int GetRequiredQueueLengthFromIntervalType(IntervalType intervalType, int iterationTimeInMinutes)
        {
            var intervalinMinutes = float.Parse(((int)intervalType).ToString(), NumberStyles.Integer);
            var rawQueuSize = intervalinMinutes / iterationTimeInMinutes;

            return (int)Math.Round(rawQueuSize, MidpointRounding.AwayFromZero);
        }
        private long GetTotalQueuSum()
        {
            var totalOfItemsInQueue = Queue
                .Where(e => e != null)
                .Select(e => e.Value)
                .Sum();

            return totalOfItemsInQueue;
        }

        public long IntervalTic(ref int counter)
        {
            if (counter == 0)
            {
                Queue[Position] = null;
            }
            else
            {
                Queue[Position] = counter;
            }
            
            counter = 0;
            if (Position == QueueLength - 1) Position = 0;
            else Position++;

            return GetTotalQueuSum();
        }
    }
}
