using MDT.Models;

namespace MDT.ViewModels
{
    public class GameSettingsVM
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int DrawTypeId { get; set; }
        public string GameName { get; set; }
        public bool AutoPlay { get; set; }
        public int MaximumPlays { get; set; }
        public int Priority { get; set; }

        public GameSettingsVM()
        {
        }
        public GameSettingsVM(UserDrawTypeOption pg)
        {
            UserId = pg.UserId;
            UserName = pg.User.UserName;
            DrawTypeId = pg.DrawTypeId;
            GameName = pg.DrawType.DrawTypeName;
            AutoPlay = pg.PlayAll;
            MaximumPlays = pg.MaxPlay;
            Priority = pg.Priority;
        }
    }
}