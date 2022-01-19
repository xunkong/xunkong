namespace Xunkong.Core.Metadata
{
    [Table("Info_Character_Constellation")]
    public class ConstellationInfo
    {

        public int Id { get; set; }

        public int CharacterInfoId { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        public string? Effect { get; set; }

        public string? Icon { get; set; }

        public int Position { get; set; }

    }
}
