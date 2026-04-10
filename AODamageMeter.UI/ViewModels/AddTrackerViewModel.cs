using System;

namespace AODamageMeter.UI.ViewModels
{
    public class AddTrackerViewModel : ViewModelBase
    {
        public string Name { get; }
        public bool IsActive { get; private set; }
        public DateTime? DetectedTimestamp { get; private set; }

        public AddTrackerViewModel(string name)
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
