using Xunkong.GenshinData.Material;

namespace Xunkong.GenshinData;

public class PromotionCostItem
{

    public int Id { get; set; }

    public int Count { get; set; }

    public MaterialItem Item { get; set; }

    public static int GetPromoteLevel(int level, bool isPromote) => (level, isPromote) switch
    {
        ( < 20, _) => 0,
        (20, false) => 0,
        (20, true) => 1,
        ( < 40, _) => 1,
        (40, false) => 1,
        (40, true) => 2,
        ( < 50, _) => 2,
        (50, false) => 2,
        (50, true) => 3,
        ( < 60, _) => 3,
        (60, false) => 3,
        (60, true) => 4,
        ( < 70, _) => 4,
        (70, false) => 4,
        (70, true) => 5,
        ( < 80, _) => 5,
        (80, false) => 5,
        (80, true) => 6,
        ( <= 90, _) => 6,
        _ => 0,
    };

}


public class PromotionAddProperty
{

    public string PropType { get; set; }

    public double Value { get; set; }
}