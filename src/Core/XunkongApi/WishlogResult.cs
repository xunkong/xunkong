using Xunkong.Core.Wish;

namespace Xunkong.Core.XunkongApi
{
    public record WishlogResult(int Uid, int CurrentCount, int GetCount, int PutCount, int DeleteCount, List<WishlogItem>? List);

}
