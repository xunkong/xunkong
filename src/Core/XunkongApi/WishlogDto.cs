using Xunkong.Core.Wish;

namespace Xunkong.Core.XunkongApi
{
    public class WishlogDto
    {

        public int Uid { get; set; }


        public string? Url { get; set; }


        public long LastId { get; set; }


        public IEnumerable<WishlogItem>? List { get; set; }

    }
}
