#pragma warning disable CS8602 // 解引用可能出现空引用。

using System.Text.Json;
using Xunkong.GenshinData.Achievement;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Material;
using Xunkong.GenshinData.Text;
using Xunkong.GenshinData.Weapon;

namespace Xunkong.ApiServer.Services;

public class XunkongDbContext : DbContext
{
    public DbSet<WishlogItem> WishlogItems { get; set; }

    public DbSet<WishlogAuthkeyItem> WishlogAuthkeys { get; set; }

    public DbSet<BaseRecordModel> AllRecords { get; set; }

    public DbSet<WishlogRecordModel> WishlogRecords { get; set; }

    public DbSet<CharacterInfo> CharacterInfos { get; set; }

    public DbSet<WeaponInfo> WeaponInfos { get; set; }

    public DbSet<WeaponSkillModel> WeaponSkills { get; set; }

    public DbSet<WishEventInfo> WishEventInfos { get; set; }

    public DbSet<WallpaperInfo> WallpaperInfos { get; set; }

    public DbSet<TextMapItem> TextMapItems { get; set; }




    private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public XunkongDbContext(DbContextOptions<XunkongDbContext> options) : base(options)
    {
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<WishlogItem>(e =>
        {
            e.ToTable("wishlog_items");
            e.HasKey(x => new { x.Uid, x.Id });
            e.Ignore(x => x.Count).Ignore(x => x.ItemId).Ignore(x => x._TimeString);
        });
        modelBuilder.Entity<WishlogAuthkeyItem>().ToTable("wishlog_authkeys");

        modelBuilder.Entity<WishEventInfo>(e =>
        {
            e.ToTable("info_wishevent");
            e.Ignore(x => x.StartTime).Ignore(x => x.EndTime).Ignore(x => x.QueryType);
            e.Property(x => x._StartTimeString).HasColumnName("StartTime");
            e.Property(x => x._EndTimeString).HasColumnName("EndTime");
            e.Property(x => x.Rank5UpItems).HasConversion(list => string.Join(",", list), s => s.Split(",", StringSplitOptions.None).ToList());
            e.Property(x => x.Rank4UpItems).HasConversion(list => string.Join(",", list), s => s.Split(",", StringSplitOptions.None).ToList());
        });

        modelBuilder.Entity<WallpaperInfo>().Property(x => x.Tags).HasConversion(v => v.ToString(), s => s.Split(';', StringSplitOptions.None).ToList());
        modelBuilder.Entity<WallpaperInfo>().ToTable("wallpapers");

        modelBuilder.Entity<AchievementGoal>().ToTable("Info_Achievement_Goal");
        modelBuilder.Entity<AchievementGoal>().Property(x => x.RewardNameCard).HasConversion(obj => JsonSerializer.Serialize(obj, JsonOptions), str => JsonSerializer.Deserialize<NameCard>(str, JsonOptions));

        modelBuilder.Entity<AchievementItem>().ToTable("Info_Achievement_Item");
        modelBuilder.Entity<AchievementItem>().Property(x => x.TriggerConfig).HasConversion(obj => JsonSerializer.Serialize(obj, JsonOptions), str => JsonSerializer.Deserialize<TriggerConfig>(str, JsonOptions));

        modelBuilder.Entity<CharacterInfo>().ToTable("info_character_v1");
        modelBuilder.Entity<CharacterInfo>().Property(x => x.Talents).HasConversion(obj => JsonSerializer.Serialize(obj, JsonOptions), str => JsonSerializer.Deserialize<List<CharacterTalent>>(str, JsonOptions));
        modelBuilder.Entity<CharacterInfo>().Property(x => x.Constellations).HasConversion(obj => JsonSerializer.Serialize(obj, JsonOptions), str => JsonSerializer.Deserialize<List<CharacterConstellation>>(str, JsonOptions));
        modelBuilder.Entity<CharacterInfo>().Property(x => x.Promotions).HasConversion(obj => JsonSerializer.Serialize(obj, JsonOptions), str => JsonSerializer.Deserialize<List<CharacterPromotion>>(str, JsonOptions));
        modelBuilder.Entity<CharacterInfo>().Property(x => x.NameCard).HasConversion(obj => JsonSerializer.Serialize(obj, JsonOptions), str => JsonSerializer.Deserialize<NameCard>(str, JsonOptions));
        modelBuilder.Entity<CharacterInfo>().Property(x => x.Food).HasConversion(obj => JsonSerializer.Serialize(obj, JsonOptions), str => JsonSerializer.Deserialize<Food>(str, JsonOptions));
        modelBuilder.Entity<CharacterInfo>().Property(x => x.Stories).HasConversion(obj => JsonSerializer.Serialize(obj, JsonOptions), str => JsonSerializer.Deserialize<List<CharacterStory>>(str, JsonOptions));
        modelBuilder.Entity<CharacterInfo>().Property(x => x.Voices).HasConversion(obj => JsonSerializer.Serialize(obj, JsonOptions), str => JsonSerializer.Deserialize<List<CharacterVoice>>(str, JsonOptions));
        modelBuilder.Entity<CharacterInfo>().Property(x => x.Outfits).HasConversion(obj => JsonSerializer.Serialize(obj, JsonOptions), str => JsonSerializer.Deserialize<List<CharacterOutfit>>(str, JsonOptions));

        modelBuilder.Entity<WeaponInfo>().ToTable("info_weapon_v1");
        modelBuilder.Entity<WeaponInfo>().Property(x => x.Properties).HasConversion(obj => JsonSerializer.Serialize(obj, JsonOptions), str => JsonSerializer.Deserialize<List<WeaponProperty>>(str, JsonOptions));
        modelBuilder.Entity<WeaponInfo>().Property(x => x.Skills).HasConversion(obj => JsonSerializer.Serialize(obj, JsonOptions), str => JsonSerializer.Deserialize<List<WeaponSkill>>(str, JsonOptions));
        modelBuilder.Entity<WeaponInfo>().Property(x => x.Promotions).HasConversion(obj => JsonSerializer.Serialize(obj, JsonOptions), str => JsonSerializer.Deserialize<List<WeaponPromotion>>(str, JsonOptions));

        modelBuilder.Entity<TextMapItem>().ToTable("info_textmap");
        modelBuilder.Entity<TextMapItem>().HasKey(x => x.ItemId);

    }



}
