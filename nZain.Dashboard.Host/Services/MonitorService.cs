using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace nZain.Dashboard.Services
{
    public class MonitorService
    {
        public enum MonitorStatus
        {
            Off = 0,
            Fading = 1,
            On = 2,
        }

        private static readonly NLog.ILogger Logger = NLog.LogManager.GetCurrentClassLogger();

        private MonitorStatus _status = MonitorStatus.On;

        public MonitorService()
        {
        }

        public MonitorStatus Status
        {
            get => this._status;
            set
            {
#if DEBUG
                if (this._status == value)
                {
                    return;
                }
#else
                bool oldValue = (this._status != MonitorStatus.Off);
                bool newValue = (value != MonitorStatus.Off);
                if (oldValue == newValue)
                {
                    return;
                }
#endif
                this.SetDisplayPower(value);
            }
        }

        public bool IsMonitorOn => this.Status != MonitorStatus.Off;

        private void SetDisplayPower(MonitorStatus value)
        {
            string args = null;
            switch (value)
            {
                case MonitorStatus.Off:
                    args = "display_power 0";
                    Logger.Info(args);
                    break;
                case MonitorStatus.On:
                    args = "display_power 1";
                    Logger.Info(args);
                    break;
                default:
                    Logger.Info("display_power 0.75");
                    this._status = value;
                    return;
            }

            try
            {
                Process.Start("vcgencmd", args);
                this._status = value;
            }
            catch (Exception e)
            {
                Logger.Error(e, "vcgencmd failed");
            }
        }
    }
}