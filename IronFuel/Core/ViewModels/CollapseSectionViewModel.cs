namespace IronFuel.Web.Core.ViewModels
{
    public class CollapseSectionViewModel
    {
        /// <summary>
        /// Unique HTML id used for the collapse target and ARIA relationships.
        /// </summary>
        public string TargetId { get; set; } = string.Empty;

        /// <summary>
        /// Visible heading label shown in the toggle row.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// HTML content to render inside the collapsed body.
        /// Pass null or empty to suppress the entire section.
        /// </summary>
        public string? Content { get; set; }
    }
}
