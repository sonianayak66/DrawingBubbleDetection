using MPCRS.Models;

namespace MPCRS.ViewModels
{
    // ── Parent record — one per part+revision ──────────────────
    public class DrawingBubbleInspectionVM
    {
        public int Inspection_Dbkey { get; set; }
        public int Engine_Part_Dbkey { get; set; }
        public string Draw_part_no { get; set; }
        public string Revision { get; set; }
        public string Original_File_Name { get; set; }
        public string Original_File_Path { get; set; }
        public string Annotated_File_Name { get; set; }
        public string Annotated_File_Path { get; set; }
        public int Total_Bubbles { get; set; }
        public int Needs_Review_Count { get; set; }
        public string Status { get; set; }
        public DateTime Processed_On { get; set; }
        public string Processed_By_Name { get; set; }
        public bool Is_Active { get; set; }
        public string Description { get; set; }
        public string Detection_Method { get; set; }
    }

    // ── Individual bubble row ───────────────────────────────────
    public class DrawingBubbleInspectionItem
    {
        public int Item_Dbkey { get; set; }
        public int Inspection_Dbkey { get; set; }
        public string Bubble_Number { get; set; }
        public string Dimension { get; set; }
        public string Tolerance { get; set; }
        public decimal Confidence { get; set; }
        public bool Needs_Review { get; set; }
        public bool Is_Manually_Added { get; set; }
        public bool Is_Corrected { get; set; }
        public int X_Coordinate { get; set; }
        public int Y_Coordinate { get; set; }
        public int Radius { get; set; }
        public bool Is_Active { get; set; }
    }

    // ── Used to pass save request from controller to service ───
    public class DrawingBubbleInspectionSaveRequest
    {
        public int Engine_Part_Dbkey { get; set; }
        public string Draw_part_no { get; set; }
        public string Revision { get; set; }
        public string Original_File_Name { get; set; }
        public string Original_File_Path { get; set; }
        public string Annotated_File_Name { get; set; }
        public string Annotated_File_Path { get; set; }
        public int Total_Bubbles { get; set; }
        public int Needs_Review_Count { get; set; }
        public int Processed_By { get; set; }
        public string Detection_Method { get; set; }
        public List<DrawingBubbleInspectionItem> Items { get; set; } = new();
    }

    // ── Python API response models ─────────────────────────────
    public class BubbleDetectionApiResponse
    {
        public int bubble_count { get; set; }
        public List<BubbleDetectionResult> bubbles { get; set; } = new();
        public string annotated_image_base64 { get; set; }
    }

    public class BubbleDetectionResult
    {
        public string bubble_number { get; set; }
        public string dimension { get; set; }
        public string zone { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int radius { get; set; }
        public decimal confidence { get; set; }
        public bool needs_review { get; set; }
    }

    // Response shape for POST /api/auto-annotate. The Python service
    // generates balloons + leader lines for every dimension it finds;
    // each balloon carries its placement (cx/cy/radius) and the
    // dimension text it was drawn for. The controller flattens this
    // into the same DrawingBubbleInspectionItem shape used by the
    // detect path so Results / Save / History work uniformly.
    public class AutoAnnotateApiResponse
    {
        public int balloon_count { get; set; }
        public List<AutoAnnotateBalloon> balloons { get; set; } = new();
        public string annotated_image_base64 { get; set; }
    }

    public class AutoAnnotateBalloon
    {
        public int number { get; set; }
        public AutoAnnotateBalloonShape balloon { get; set; } = new();
        public string dimension_text { get; set; }
    }

    public class AutoAnnotateBalloonShape
    {
        public int cx { get; set; }
        public int cy { get; set; }
        public int radius { get; set; }
    }

    // ── Passed to the Results view ─────────────────────────────
    public class DrawingBubbleResultViewModel
    {
        public int Engine_Part_Dbkey { get; set; }
        public string Draw_part_no { get; set; }
        public string Revision { get; set; }
        public string Original_File_Name { get; set; }
        public string AnnotatedImageUrl { get; set; }
        public List<DrawingBubbleInspectionItem> Bubbles { get; set; } = new();

        // For multi-page PDFs — each page has its own image + bubbles
        public List<DrawingBubblePageResult> Pages { get; set; } = new();
        public bool IsMultiPage => Pages.Count > 1;
        public bool IsReadOnly { get; set; } = false;
        public string Original_File_Path { get; set; }

        public string Detection_Method { get; set; }
        public string Detection_Method_DisplayName { get; set; }
    }

    public class DrawingBubblePageResult
    {
        public int PageNumber { get; set; }
        public string AnnotatedImageUrl { get; set; }
        public List<DrawingBubbleInspectionItem> Bubbles { get; set; } = new();
        public string OriginalImageUrl { get; set; }
    }

    // ── Used by the Upload view (part dropdown) ────────────────
    public class PartDropdownItem
    {
        public int Engine_Part_Dbkey { get; set; }
        public string Draw_part_no { get; set; }
        public string Description { get; set; }
        public string Revision { get; set; }
        public string DisplayText => $"{Draw_part_no} — {Description}";
    }

    public class DrawingBubbleIndexVM
    {
        public List<PartDropdownItem> Parts { get; set; } = new();
        public List<DetectionMethodItem> DetectionMethods { get; set; } = new();
        public string DefaultMethod { get; set; }
    }

    public class DetectionMethodItem
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
    }

    public class DrawingBubbleHistoryVM
    {
        public List<PartDropdownItem> Parts { get; set; } = new();
        public List<DrawingBubbleInspectionVM> Records { get; set; } = new();
        public int SelectedEnginePartDbkey { get; set; }
    }
}
