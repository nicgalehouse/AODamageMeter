namespace AODamageMeter.UI.ViewModels
{
    public interface IBossModuleViewModel
    {
        void OnFightEventAdded(FightEvent fightEvent);
        void UpdateView();
        void Reset();
    }
}
