
namespace pdpditxx
{
    internal class ServerConfig
    {
        public class Root
        {
            public string Author { get; set; }
            public string Date { get; set; }
            public string Description { get; set; }
            public string Notes { get; set; }
            public Directories Directories { get; set; }
        }

        public class Directories
        {
            public string LogDir { get; set; }
            public string OutDir { get; set; }
            public string WorkDir { get; set; }
        }
    }
}
