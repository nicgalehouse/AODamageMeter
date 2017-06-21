using AODamageMeter.UI.Properties;
using AODamageMeter.UI.ViewModels.Rows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AODamageMeter.UI.ViewModels
{
    public class FightViewModel : ViewModelBase
    {
        private readonly ObservableCollection<MainRowBase> _viewingModeRows = new ObservableCollection<MainRowBase>();
        private readonly Dictionary<FightCharacter, MainRowBase> _damageDoneRowMap = new Dictionary<FightCharacter, MainRowBase>();
        private readonly ObservableCollection<MainRowBase> _damageDoneRows = new ObservableCollection<MainRowBase>();
        private readonly Dictionary<FightCharacter, Dictionary<DamageInfo, MainRowBase>> _damageDoneInfoRowMapMap = new Dictionary<FightCharacter, Dictionary<DamageInfo, MainRowBase>>();
        private readonly Dictionary<FightCharacter, ObservableCollection<MainRowBase>> _damageDoneInfoRowsMap = new Dictionary<FightCharacter, ObservableCollection<MainRowBase>>();
        private readonly Dictionary<FightCharacter, MainRowBase> _damageTakenRowMap = new Dictionary<FightCharacter, MainRowBase>();
        private readonly ObservableCollection<MainRowBase> _damageTakenRows = new ObservableCollection<MainRowBase>();
        private readonly Dictionary<FightCharacter, Dictionary<DamageInfo, MainRowBase>> _damageTakenInfoRowMapMap = new Dictionary<FightCharacter, Dictionary<DamageInfo, MainRowBase>>();
        private readonly Dictionary<FightCharacter, ObservableCollection<MainRowBase>> _damageTakenInfoRowsMap = new Dictionary<FightCharacter, ObservableCollection<MainRowBase>>();
        private readonly Dictionary<HealingInfo, MainRowBase> _ownersHealingDoneRowMap = new Dictionary<HealingInfo, MainRowBase>();
        private readonly ObservableCollection<MainRowBase> _ownersHealingDoneRows = new ObservableCollection<MainRowBase>();
        private readonly Dictionary<HealingInfo, MainRowBase> _ownersHealingTakenRowMap = new Dictionary<HealingInfo, MainRowBase>();
        private readonly ObservableCollection<MainRowBase> _ownersHealingTakenRows = new ObservableCollection<MainRowBase>();
        private readonly Dictionary<CastInfo, MainRowBase> _ownersCastsRowMap = new Dictionary<CastInfo, MainRowBase>();
        private readonly ObservableCollection<MainRowBase> _ownersCastsRows = new ObservableCollection<MainRowBase>();

        public FightViewModel(Fight fight)
            => Fight = fight;

        public Fight Fight { get; }
        public DamageMeter DamageMeter => Fight.DamageMeter;
        public Character Owner => Fight.DamageMeter.Owner;

        private FightCharacter _fightOwner;
        public FightCharacter FightOwner
        {
            get
            {
                if (_fightOwner != null)
                    return _fightOwner;

                lock (Fight)
                {
                    Fight.TryGetFightOwner(out _fightOwner);
                    return _fightOwner;
                }
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedViewingModeRows()
        {
            lock (Fight)
            {
                if (_viewingModeRows.Count == 0)
                {
                    foreach (var viewingModeRow in ViewingModeMainRowBase.GetRows(this))
                    {
                        _viewingModeRows.Add(viewingModeRow);
                    }
                }

                foreach (var viewingModeRow in _viewingModeRows)
                {
                    viewingModeRow.Update();
                }

                return _viewingModeRows;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedDamageDoneRows()
        {
            lock (Fight)
            {
                int displayIndex = 1;
                foreach (var fightCharacter in Fight.FightCharacters
                    .Where(c => !c.IsFightPet)
                    .OrderByDescending(c => c.TotalDamageDonePlusPets)
                    .ThenBy(c => c.UncoloredName))
                {
                    if (!Settings.Default.ShowTopLevelNPCRows && fightCharacter.IsNPC
                        || !Settings.Default.ShowTopLevelZeroDamageRows && fightCharacter.TotalDamageDonePlusPets == 0)
                    {
                        if (_damageDoneRowMap.TryGetValue(fightCharacter, out MainRowBase damageDoneRow))
                        {
                            _damageDoneRowMap.Remove(fightCharacter);
                            _damageDoneRows.Remove(damageDoneRow);
                        }
                    }
                    else
                    {
                        if (!_damageDoneRowMap.TryGetValue(fightCharacter, out MainRowBase damageDoneRow))
                        {
                            _damageDoneRowMap.Add(fightCharacter, damageDoneRow = new DamageDoneMainRow(this, fightCharacter));
                            _damageDoneRows.Add(damageDoneRow);
                        }
                        damageDoneRow.Update(displayIndex++);
                    }
                }

                return _damageDoneRows;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedDamageDoneInfoRows(Character character)
        {
            lock (Fight)
            {
                return Fight.TryGetFightCharacter(character, out FightCharacter fightCharacter)
                    ? GetUpdatedDamageDoneInfoRows(fightCharacter) : null;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedDamageDoneInfoRows(FightCharacter fightCharacter)
        {
            lock (Fight)
            {
                Dictionary<DamageInfo, MainRowBase> damageDoneInfoRowMap;
                ObservableCollection<MainRowBase> damageDoneInfoRows;
                if (_damageDoneInfoRowMapMap.TryGetValue(fightCharacter, out damageDoneInfoRowMap))
                {
                    damageDoneInfoRows = _damageDoneInfoRowsMap[fightCharacter];
                }
                else
                {
                    damageDoneInfoRowMap = _damageDoneInfoRowMapMap[fightCharacter] = new Dictionary<DamageInfo, MainRowBase>();
                    damageDoneInfoRows = _damageDoneInfoRowsMap[fightCharacter] = new ObservableCollection<MainRowBase>();
                }

                int displayIndex = 1;
                foreach (var damageDoneInfo in fightCharacter.DamageDoneInfos
                    .OrderByDescending(i => i.TotalDamagePlusPets)
                    .ThenBy(i => i.Target.UncoloredName))
                {
                    if (!damageDoneInfoRowMap.TryGetValue(damageDoneInfo, out MainRowBase damageDoneInfoRow))
                    {
                        damageDoneInfoRowMap.Add(damageDoneInfo, damageDoneInfoRow = new DamageDoneInfoMainRow(this, damageDoneInfo));
                        damageDoneInfoRows.Add(damageDoneInfoRow);
                    }
                    damageDoneInfoRow.Update(displayIndex++);
                }

                return damageDoneInfoRows;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedDamageTakenRows()
        {
            lock (Fight)
            {
                int displayIndex = 1;
                foreach (var fightCharacter in Fight.FightCharacters
                    .OrderByDescending(c => c.TotalDamageTaken)
                    .ThenBy(c => c.UncoloredName))
                {
                    if (!Settings.Default.ShowTopLevelNPCRows && fightCharacter.IsNPC
                        || !Settings.Default.ShowTopLevelZeroDamageRows && fightCharacter.TotalDamageTaken == 0)
                    {
                        if (_damageTakenRowMap.TryGetValue(fightCharacter, out MainRowBase damageTakenRow))
                        {
                            _damageTakenRowMap.Remove(fightCharacter);
                            _damageTakenRows.Remove(damageTakenRow);
                        }
                    }
                    else
                    {
                        if (!_damageTakenRowMap.TryGetValue(fightCharacter, out MainRowBase damageTakenRow))
                        {
                            _damageTakenRowMap.Add(fightCharacter, damageTakenRow = new DamageTakenMainRow(this, fightCharacter));
                            _damageTakenRows.Add(damageTakenRow);
                        }
                        damageTakenRow.Update(displayIndex++);
                    }
                }

                return _damageTakenRows;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedDamageTakenInfoRows(Character character)
        {
            lock (Fight)
            {
                return Fight.TryGetFightCharacter(character, out FightCharacter fightCharacter)
                    ? GetUpdatedDamageTakenInfoRows(fightCharacter) : null;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedDamageTakenInfoRows(FightCharacter fightCharacter)
        {
            lock (Fight)
            {
                Dictionary<DamageInfo, MainRowBase> damageTakenInfoRowMap;
                ObservableCollection<MainRowBase> damageTakenInfoRows;
                if (_damageTakenInfoRowMapMap.TryGetValue(fightCharacter, out damageTakenInfoRowMap))
                {
                    damageTakenInfoRows = _damageTakenInfoRowsMap[fightCharacter];
                }
                else
                {
                    damageTakenInfoRowMap = _damageTakenInfoRowMapMap[fightCharacter] = new Dictionary<DamageInfo, MainRowBase>();
                    damageTakenInfoRows = _damageTakenInfoRowsMap[fightCharacter] = new ObservableCollection<MainRowBase>();
                }

                int displayIndex = 1;
                foreach (var damageTakenInfo in fightCharacter.DamageTakenInfos
                    .Where(i => !i.Source.IsFightPet)
                    .OrderByDescending(i => i.TotalDamagePlusPets)
                    .ThenBy(i => i.Source.UncoloredName))
                {
                    if (!damageTakenInfoRowMap.TryGetValue(damageTakenInfo, out MainRowBase damageTakenInfoRow))
                    {
                        damageTakenInfoRowMap.Add(damageTakenInfo, damageTakenInfoRow = new DamageTakenInfoMainRow(this, damageTakenInfo));
                        damageTakenInfoRows.Add(damageTakenInfoRow);
                    }
                    damageTakenInfoRow.Update(displayIndex++);
                }

                return damageTakenInfoRows;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedOwnersHealingDoneRows()
        {
            lock (Fight)
            {
                if (FightOwner == null)
                    return _ownersHealingDoneRows;

                int displayIndex = 1;
                foreach (var healingDoneInfo in FightOwner.HealingDoneInfos
                    .OrderByDescending(i => i.PotentialHealingPlusPets)
                    .ThenBy(i => i.Target.UncoloredName))
                {
                    if (!_ownersHealingDoneRowMap.TryGetValue(healingDoneInfo, out MainRowBase ownersHealingDoneRow))
                    {
                        _ownersHealingDoneRowMap.Add(healingDoneInfo, ownersHealingDoneRow = new OwnersHealingDoneMainRow(this, healingDoneInfo));
                        _ownersHealingDoneRows.Add(ownersHealingDoneRow);
                    }
                    ownersHealingDoneRow.Update(displayIndex++);
                }

                return _ownersHealingDoneRows;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedOwnersHealingTakenRows()
        {
            lock (Fight)
            {
                if (FightOwner == null)
                    return _ownersHealingTakenRows;

                int displayIndex = 1;
                foreach (var healingTakenInfo in FightOwner.HealingTakenInfos
                    .Where(i => !i.Source.IsFightPet)
                    .OrderByDescending(i => i.PotentialHealingPlusPets)
                    .ThenBy(i => i.Target.UncoloredName))
                {
                    if (!_ownersHealingTakenRowMap.TryGetValue(healingTakenInfo, out MainRowBase ownersHealingTakenRow))
                    {
                        _ownersHealingTakenRowMap.Add(healingTakenInfo, ownersHealingTakenRow = new OwnersHealingTakenMainRow(this, healingTakenInfo));
                        _ownersHealingTakenRows.Add(ownersHealingTakenRow);
                    }
                    ownersHealingTakenRow.Update(displayIndex++);
                }

                return _ownersHealingTakenRows;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedOwnersCastsRows()
        {
            lock (Fight)
            {
                if (FightOwner == null)
                    return _ownersCastsRows;

                int displayIndex = 1;
                foreach (var castInfo in FightOwner.CastInfos
                    .OrderByDescending(i => i.CastSuccesses)
                    .ThenBy(i => i.NanoProgram))
                {
                    if (!_ownersCastsRowMap.TryGetValue(castInfo, out MainRowBase ownersCastsRow))
                    {
                        _ownersCastsRowMap.Add(castInfo, ownersCastsRow = new OwnersCastsMainRow(this, castInfo));
                        _ownersCastsRows.Add(ownersCastsRow);
                    }
                    ownersCastsRow.Update(displayIndex++);
                }

                return _ownersCastsRows;
            }
        }
    }
}
