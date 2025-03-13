using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace pdpditxx
{
    internal class ErrorMessage
    {
        public string TimeStamp { get; set; }
        public string FileName { get; set; }
        public string ActiveStep { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Module { get; set; }

        public static void ErrorListToJson(List<ErrorMessage> messageList, string jsonOutputFile)
        {
            using (StreamWriter jsonOut = File.CreateText(jsonOutputFile))
            {
                jsonOut.Write(JsonConvert.SerializeObject(messageList));
            }
        }
    }
}
