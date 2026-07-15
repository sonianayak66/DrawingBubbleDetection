using System.Collections.Generic;

namespace MPCRS.Services
{
    public class BubbleDetectionOptions
    {
        public string DefaultMethod { get; set; } = "";
        public Dictionary<string, BubbleDetectionMethodOptions> Methods { get; set; } = new();
    }

    public class BubbleDetectionMethodOptions
    {
        public string DisplayName { get; set; } = "";
        public string Folder { get; set; } = "";
        public bool UseVenv { get; set; } = true;
        public string Module { get; set; } = "";
        public int Port { get; set; }
        public string BaseUrl { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string HealthPath { get; set; } = "/api/health";
        public string DetectPath { get; set; } = "/api/detect";
        public string LogsPath { get; set; } = "/api/logs";

        // "detect"        — POST returns { bubbles[], annotated_image_base64 }
        // "auto-annotate" — POST returns { balloons[], annotated_image_base64 }
        // The two responses are mapped onto the same DrawingBubbleInspectionItem
        // list so the Results / Save / History pages don't need to know which
        // mode produced the data.
        public string Mode { get; set; } = "detect";
    }
}
