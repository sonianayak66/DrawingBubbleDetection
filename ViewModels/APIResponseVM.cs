namespace MPCRS.ViewModels
{
    public class APIResponseVM
    {
        public string url { get; set; }
        public string type { get; set; }
        public string Content { get; set; }

        public bool FormatedType { get; set; } = true;
    }

    public class HttpUsrResponse
    {
        public string Get { get; set; }
        public string Post { get; set; }
        public string Put { get; set; }
        public string Patch { get; set; }
        public string Delete { get; set; }

    }
}
