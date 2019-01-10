using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using NLog;
using nZain.Dashboard.Host;

namespace nZain.Dashboard.Services
{
    public class PirSensorService : IDisposable
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly MonitorService _monitorService;
        
        private Thread _pollingThread = null;

        public PirSensorService(DashboardConfig cfg, MonitorService monitorService)
        {
            this._monitorService = monitorService;
// TODO ruhephasen aus cfg
            this.PirSensorPin = cfg.PirSensorPin;
            this.MonitorFadeoutTimeMs = (int)(cfg.MonitorFadeoutTimeMinutes * 60 * 1000);

            if (TimeSpan.TryParse(cfg.WakeupTime, CultureInfo.InvariantCulture, out TimeSpan wakeupTime))
            {
                this.WakeupTime = wakeupTime;
            }
            else
            {
                this.WakeupTime = new TimeSpan(06, 00, 00);
            }
            if (TimeSpan.TryParse(cfg.BedTime, CultureInfo.InvariantCulture, out TimeSpan bedTime))
            {
                this.BedTime = bedTime;
            }
            else
            {
                this.BedTime = new TimeSpan(22, 00, 00);
            }
        }

#if LED
        public int LedPin { get; } = 18;
#endif

        public int PirSensorPin { get; }

        public int MonitorFadeoutTimeMs { get; }

        public TimeSpan WakeupTime { get; }

        public TimeSpan BedTime { get; }

        public bool IsMonitorOn => this._monitorService.Status != MonitorService.MonitorStatus.Off;

        public bool IsRunning => this._pollingThread != null;

        public void Start()
        {
            this.Stop(); // may be useful to restart a broken service?
            this._pollingThread = new Thread(this.Run)
            {
                Name = "PIR-polling-thread",
                IsBackground = true
            };
            Logger.Info($"Starting thread '{this._pollingThread.Name}'...");
            this._pollingThread.Start();
        }

        public void Stop()
        {
            var t = this._pollingThread;
            if (t != null)
            {
                Logger.Info($"Stopping thread '{t.Name}'...");
                this._pollingThread = null;
                t.Join();
            }
        }

        public void Dispose()
        {
            this.Stop();
        }

        private void Run(object obj)
        {
            try
            {
                var controller = new GpioController(PinNumberingScheme.Logical);
                using (controller)
                {
#if LED
                    controller.OpenPin(LedPin, PinMode.Output); //noop, this is the default
                    Console.WriteLine($"Pin-{LedPin} enabled for PinMode.{controller.GetPinMode(LedPin)}");
#endif
                    controller.OpenPin(PirSensorPin, PinMode.Input); // write access /sys/class/gpio/gpio23/direction
                    Console.WriteLine($"Pin-{PirSensorPin} enabled for PinMode.{controller.GetPinMode(PirSensorPin)}");
                    
                    // blocking infinite call
                    this.RunGpioLoop(controller);

                    // end of using block: dispose controller
                    Console.WriteLine("cleanup...");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "GPIO Controller failed");
            }
            this._pollingThread = null;
        }

        private void RunGpioLoop(GpioController controller)
        {
#if LED
            controller.Write(LedPin, PinValue.Low);
            bool led = false;
#endif

            PinValue lastPinValue = PinValue.Low;
            Stopwatch sw = new Stopwatch();

            while (this._pollingThread != null)
            {
                var timeOfDay = DateTimeOffset.Now.TimeOfDay;
                if (this.WakeupTime > timeOfDay || timeOfDay > this.BedTime)
                {
                    Thread.Sleep(60000);
                    continue;
                }

                PinValue activity = controller.Read(PirSensorPin);
                if (activity == PinValue.High || activity != lastPinValue)
                {
                    // a) Activity => Monitor on
                    sw.Restart();
                    this._monitorService.Status = MonitorService.MonitorStatus.On;
#if LED
                    controller.Write(LedPin, PinValue.High);
                    led = true;
#endif
                }
                else if (this.IsMonitorOn) // but PIR sensor inactive
                {
                    var remaining = MonitorFadeoutTimeMs - sw.ElapsedMilliseconds;
                    if (remaining > 0)
                    {
                        // b) no activity for a while => Monitor off soon (blink LED)
                        this._monitorService.Status = MonitorService.MonitorStatus.Fading;
#if LED
                        controller.Write(LedPin, led ? PinValue.Low : PinValue.High);
                        led = !led;
#endif
                    }
                    else
                    {
                        // c) Monitor off
                        this._monitorService.Status = MonitorService.MonitorStatus.Off;
                        sw.Stop();
#if LED
                        controller.Write(LedPin, PinValue.Low);
                        led = false;
#endif
                    }
                }
                // else: idle

                Thread.Sleep(1000);
            }
        }
    }
}