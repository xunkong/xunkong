using Microsoft.UI.Xaml;

namespace Xunkong.Desktop.Models;

public class GrowthScheduleItem
{

    public int Id { get; set; }

    public int ItemId { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// 0: 角色，1: 武器
    /// </summary>
    public int Type { get; set; }

    public int Rarity { get; set; }

    public string Icon { get; set; }

    public int Order { get; set; }


    public List<GrowthScheduleCostItem>? LevelCostItems { get; set; }


    public List<GrowthScheduleCostItem>? TalentCostItems { get; set; }


    public List<GrowthScheduleCostItem> GetUnFinishedMaterialItems()
    {
        var items = new List<GrowthScheduleCostItem>();
        if (LevelCostItems?.Any() ?? false)
        {
            items.AddRange(LevelCostItems.Where(x => !x.IsFinish));
        }
        if (TalentCostItems?.Any() ?? false)
        {
            items.AddRange(TalentCostItems.Where(x => !x.IsFinish));
        }
        return items.Where(x => x.Id != 202).GroupBy(x => x.Id)
                    .Select(x => new GrowthScheduleCostItem { Id = x.Key, Name = x.FirstOrDefault()?.Name!, Icon = x.FirstOrDefault()?.Icon!, CostCount = x.Sum(x => x.CostCount) })
                    .OrderBy(x => x.Id).ToList();
    }


    private Visibility? hasTodayMaterial;

    public Visibility HasTodayMaterial()
    {
        if (hasTodayMaterial == null)
        {
            hasTodayMaterial = (LevelCostItems?.Any(x => !x.IsFinish && x.IsToday), TalentCostItems?.Any(x => !x.IsFinish && x.IsToday)) switch
            {
                (true, _) => Visibility.Visible,
                (_, true) => Visibility.Visible,
                _ => Visibility.Collapsed,
            };
        }
        return hasTodayMaterial.Value;
    }

}


[INotifyPropertyChanged]
public partial class GrowthScheduleCostItem
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Icon { get; set; }

    public int CurrentCount { get; set; }

    public int CostCount { get; set; }

    public bool IsToday { get; set; }

    [ObservableProperty]
    private bool isFinish;

}