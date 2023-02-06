using System.Collections.Generic;
using Game.Scripts.Behaviours;

namespace Game.Scripts.Helpers
{
    public class PlayerData
    {
        #region Singleton

        private static readonly PlayerData _instance = new PlayerData();

        public static PlayerData Instance
        {
            get => _instance;
        }

        #endregion

        // public ObservableDict<string, CollectableInformationList> SavedStackData
        // {
        //     get { return PlayerStats.Get<ObservableDict<string, CollectableInformationList>>(StatKeys.SavedStackData); }
        //
        //     set { PlayerStats.Set(StatKeys.SavedStackData, value); }
        // }
    }
}
