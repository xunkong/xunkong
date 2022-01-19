namespace Xunkong.Core.Artifact
{
    /// <summary>
    /// 圣遗物信息
    /// </summary>
    public class ArtifactItem
    {
        public int Id { get; set; }

        public bool IsLock { get; set; }

        public string SetName { get; set; }

        public string ArtifactName { get; set; }

        public ArtifactType Type { get; set; }

        public int Rarity { get; set; }

        public int Level { get; set; }

        public ArtifactTag MainTag { get; set; }

        public List<ArtifactTag> SubTags { get; set; }

        public string EquippingCharacter { get; set; }
    }
}
