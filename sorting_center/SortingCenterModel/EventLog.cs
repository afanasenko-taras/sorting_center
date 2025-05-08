using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    public class EventLog
    {
        public TimeSpan startTime;
        public TimeSpan endTime;
        public string botId;
        public string command;
        public string Start;
        public string End;
        public int boxType;
        public string channel;
        public string TrID;

        public EventLog(TimeSpan startTime, TimeSpan endTime, string botId, string command, string Start, string End, int boxType, string channel, string TrID)
        {
            this.startTime = startTime;
            this.endTime = endTime;
            this.botId = botId;
            this.command = command;
            this.Start = Start;
            this.End = End;
            this.boxType = boxType;
            this.channel = channel;
            this.TrID = TrID;
        }
    }
}
