namespace AODamageMeter.UI.ViewModels
{
    public interface IBossModuleViewModel
    {
        void OnFightEventAdded(FightEvent fightEvent);
        void UpdateView();
        bool IsPaused { get; set; }
        void Reset();
    }
}
