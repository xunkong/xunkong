namespace Xunkong.Core;

public static class RegionHelper
{

    public static RegionType UidToRegionType(int uid)
    {
        return uid.ToString().FirstOrDefault() switch
        {
            '1' => RegionType.cn_gf01,
            '2' => RegionType.cn_gf01,
            '3' => RegionType.cn_gf01,
            '4' => RegionType.cn_gf01,
            '5' => RegionType.cn_qd01,
            '6' => RegionType.os_usa,
            '7' => RegionType.os_euro,
            '8' => RegionType.os_asia,
            '9' => RegionType.os_cht,
            _ => RegionType.None,
        };
    }

}
