﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using SmartTaskbar.Core.AutoMode;
using SmartTaskbar.Core.UserConfig;
using static SmartTaskbar.Core.InvokeMethods;
using Timer = System.Timers.Timer;

namespace SmartTaskbar
{
    internal class TaskbarController : ApplicationContext
    {
        private static int _count;
        private readonly IAutoMode _autoMode;
        private readonly Container _container = new Container();
        private readonly Timer _timer;
        private readonly IconTray _tray;

        public TaskbarController()
        {
            // Initialize the native method.
            Initialization();

            // Load AutoMode as fast as possible.
            switch (Settings.ModeType)
            {
                case AutoModeType.Disabled:
#if DEBUG
                    _autoMode = new DumbMode();
#endif
                    break;
                case AutoModeType.ForegroundMode:
                    _autoMode = new ForegroundMode();
                    break;
                case AutoModeType.ClassicAutoMode:
                case AutoModeType.ClassicAdaptiveMode:
                case AutoModeType.WhitelistMode:
                case AutoModeType.BlacklistMode:
                    _autoMode = new AutoMode();
                    break;
                default:
#if DEBUG
                    throw new ArgumentOutOfRangeException();
#else
                    Settings.ModeType = AutoModeType.Disabled;
#endif
            }

            // timer is running on UI thread.
            _timer = new Timer(375);
            // if AutoMode is Enabled, then run it every 375 milliseconds.
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
            Application.ApplicationExit += (s, e) =>
            {
                _timer.Dispose();
                _container.Dispose();
            };
            // Start Settings Tray
            _tray = new IconTray(_container);
        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            ++_count;
            // Run AutoMode If Enable
            if (Settings.InAutoMode) _autoMode.Run();

            if (Settings.InTransparentMode) SetTransparent();

            // Reset all each 10+ second.
            if (_count % 27 != 0) return;

            _autoMode.Ready();
            UpdateCache();
            _count = 0;
        }
    }
}