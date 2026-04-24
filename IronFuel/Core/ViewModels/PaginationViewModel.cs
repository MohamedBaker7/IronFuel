

namespace IronFuel.Web.Core.ViewModels
{
    public class PaginationViewModel
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;

        public int Start
        {
            get
            {
                var start = 1;

                var maxPages = (int)ReportsConfigutations.maxPagesNumber;

                if (TotalPages > maxPages)
                    start = PageNumber - (maxPages - 1) < 1 ? 1 : PageNumber - (maxPages - 1);

                return start;
            }
        }
        public int End
        {
            get
            {
                var end = TotalPages;

                var maxPages = (int)ReportsConfigutations.maxPagesNumber;

                if (TotalPages > maxPages)
                    end = Start + maxPages > TotalPages ? TotalPages : Start + maxPages;

                return end;
            }
        }
    }
}
