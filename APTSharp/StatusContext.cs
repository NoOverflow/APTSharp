using System;
using System.Collections.Generic;
using System.Text;

namespace APTSharp
{
    public enum Status
    {
        SOUND_PARSING,
        SOUND_CLEANING,
        SYNCING,
        READING_RAW_WEDGES,
        GETTING_NORMAL_COEFFS,
        COLOR_CORRECTING,
        COMPUTING_BBT,
        COMPUTING_TEMP,
        CREATING_FALSE_COLORS,
        CREATING_THERMAL_COLORS,
        DONE
    }

    public class StatusContext
    {
        public APTData ReturnData { get; set; }
        public Status CurrentStatus { get; set; }

        public event OnStatusChangeDelegate OnStatusChange;

        public delegate void OnStatusChangeDelegate(object sender, Status newStatus);

        public void UpdateCurrentState(Status status)
        {
            CurrentStatus = status;
            OnStatusChange?.Invoke(this, status);
        }
    }
}
