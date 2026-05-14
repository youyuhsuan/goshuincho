namespace backend.DTOs.Responses
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public int? NextPage => CurrentPage < TotalPages ? CurrentPage + 1 : null;
        public int? PreviousPage => CurrentPage > 1 ? CurrentPage - 1 : null;
    }
}
