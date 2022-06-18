namespace WebVezdekod.DTO;

public class ProfileDTO
{
    public List<string>? know_list { get; set; }
    public List<string>? want_know_list { get; set; }
    public bool want_mentor { get; set; }
    public bool search_mentor { get; set; }
    public string about { get; set; }
    public int? time_start { get; set; }
    public int time_finish { get; set; }
}