namespace Xunkong.GenshinData.Character;

public class CharacterStory
{
    public int FetterId { get; set; }

    public int CharacterId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }
}



public class CharacterVoice
{
    public int FetterId { get; set; }

    public int CharacterId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public string? Chinese { get; set; }

    public string? English { get; set; }

    public string? Japanese { get; set; }

    public string? Korean { get; set; }

}