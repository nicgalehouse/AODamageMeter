using System;

namespace AODamageMeter.UI.ViewModels.BossModules
{
    public class AddTracker
    {
        public string Name { get; }
        public bool IsActive { get; private set; }
        public DateTime? DetectedTimestamp { get; private set; }

        public AddTracker(string name)
            => Name = name;

        public void Activate(DateTime timestamp)
        {
            IsActive = true;
            DetectedTimestamp = timestamp;
        }

        public void Deactivate()
        {
            IsActive = false;
            DetectedTimestamp = null;
        }
    }
}
