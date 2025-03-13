
namespace pdpditxx
{
    internal class SplitIndex
    {
        public int Counter { get; set; }

        public string FileName { get; set; }

        public int FirstPage { get; set; }

        public int LastPage { get; set; }

        public int PageRange
        {
            get
            {
                return this.LastPage - this.FirstPage;
            }
        }
    }
}
