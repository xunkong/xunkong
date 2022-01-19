using System.ComponentModel;

namespace Xunkong.Core.XunkongApi
{
    public enum ReturnCode
    {

        [Description("Ok")]
        Ok,

        [Description("Server internal exception")]
        InternalException = -1,

        [Description("Request arguement can not be parsed")]
        InvalidModelException = -2,

        [Description("Format of wishlog url is error")]
        UrlFormatError = 101,

        [Description("Hoyolab exception")]
        HoyolabException = 102,

        [Description("Wishlog url does not match the uid")]
        UrlNotMatchUid = 103,

        [Description("No wishlog item in request body")]
        NoWishlogItem = 104,

        [Description("Wishlog of uid is not found")]
        UidNotFound = 105,
    }
}
