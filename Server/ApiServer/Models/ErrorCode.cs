using System.ComponentModel;

namespace Xunkong.ApiServer.Models;

public enum ErrorCode
{

    [Description("Ok")]
    Ok = 0,

    [Description("Server internal exception")]
    InternalException = -1,

    [Description("Invalid arguement")]
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



    [Description("Version is null")]
    VersionIsNull = 201,

    [Description("No content for specific version")]
    NoContentForVersion = 202,

}
