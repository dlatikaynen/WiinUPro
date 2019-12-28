﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NintrollerLib;
using Shared;

namespace WiinUPro
{
    /// <summary>
    /// Interaction logic for GameCubeControl.xaml
    /// </summary>
    public partial class GameCubeControl : BaseControl, INintyControl
    {
        public event Delegates.BoolArrDel OnChangeLEDs;
        public event Delegates.JoystickDel OnJoyCalibrated;
        public event Delegates.TriggerDel OnTriggerCalibrated;

        protected Windows.JoyCalibrationWindow _openJoyWindow = null;
        protected Windows.TriggerCalibrationWindow _openTrigWindow = null;
        protected string _calibrationTarget = "";
        protected GameCubeAdapter _lastState;
        protected GameCubePort _activePort = GameCubePort.PORT1;

        protected enum GameCubePort
        {
            PORT1 = 1,
            PORT2 = 2,
            PORT3 = 3,
            PORT4 = 4
        }

        public GameCubeControl()
        {
            _inputPrefix = "1_";
            InitializeComponent();
        }
        
        public void ApplyInput(INintrollerState state)
        {
            // Maybe one day I will remember what this was for or just remove it
        }

        public void UpdateVisual(INintrollerState state)
        {
            if (state is GameCubeAdapter gcn)
            {
                _lastState = gcn;
                GameCubeController activePort;

                bool connecited = GetActivePort(out activePort);

                A.Opacity = activePort.A ? 1 : 0;
                B.Opacity = activePort.B ? 1 : 0;
                X.Opacity = activePort.X ? 1 : 0;
                Y.Opacity = activePort.Y ? 1 : 0;
                Z.Opacity = activePort.Z ? 1 : 0;
                START.Opacity = activePort.Start ? 1 : 0;

                dpadUp.Opacity = activePort.Up ? 1 : 0;
                dpadDown.Opacity = activePort.Down ? 1 : 0;
                dpadLeft.Opacity = activePort.Left ? 1 : 0;
                dpadRight.Opacity = activePort.Right ? 1 : 0;

                L.Opacity = activePort.L.value > 0 ? 1 : 0;
                R.Opacity = activePort.R.value > 0 ? 1 : 0;

                var lOpacityMask = L.OpacityMask as LinearGradientBrush;
                if (lOpacityMask != null && lOpacityMask.GradientStops.Count == 2)
                {
                    double offset = 1 - System.Math.Min(1, activePort.L.value);
                    lOpacityMask.GradientStops[0].Offset = offset;
                    lOpacityMask.GradientStops[1].Offset = offset;
                }

                var rOpacityMask = R.OpacityMask as LinearGradientBrush;
                if (rOpacityMask != null && rOpacityMask.GradientStops.Count == 2)
                {
                    double offset = 1 - System.Math.Min(1, activePort.R.value);
                    rOpacityMask.GradientStops[0].Offset = offset;
                    rOpacityMask.GradientStops[1].Offset = offset;
                }

                joystick.Margin = new Thickness(190 + 100 * activePort.joystick.X, 272 - 100 * activePort.joystick.Y, 0, 0);
                cStick.Margin = new Thickness(887 + 100 * activePort.cStick.X, 618 - 100 * activePort.cStick.Y, 0, 0);

                connectionStatus.Content = connecited ? "Connected" : "Disconnected";

                if (_openJoyWindow != null)
                {
                    if (_calibrationTarget == "joy") _openJoyWindow.Update(activePort.joystick);
                    else if (_calibrationTarget == "cStk") _openJoyWindow.Update(activePort.cStick);
                }
                else if (_openTrigWindow != null)
                {
                    if (_calibrationTarget == "L")  _openTrigWindow.Update(activePort.L);
                    else if (_calibrationTarget == "R") _openTrigWindow.Update(activePort.R);
                }
            }
        }

        public void ChangeLEDs(bool one, bool two, bool three, bool four)
        {
            // Doesn't use LEDs
        }

        private bool GetActivePort(out GameCubeController controller)
        {
            switch (_activePort)
            {
                default:
                case GameCubePort.PORT1:
                    controller = _lastState.port1;
                    return _lastState.port1Connected;
                case GameCubePort.PORT2:
                    controller = _lastState.port2;
                    return _lastState.port2Connected;
                case GameCubePort.PORT3:
                    controller = _lastState.port3;
                    return _lastState.port3Connected;
                case GameCubePort.PORT4:
                    controller = _lastState.port4;
                    return _lastState.port4Connected;
            }
        }

        private void Calibrate_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void CalibrateTrigger_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Apply Port target
            _calibrationTarget = (sender as FrameworkElement).Tag.ToString();
            
            GameCubeController controller;
            GetActivePort(out controller);

            var nonCalibrated = new NintrollerLib.Trigger();
            var curCalibrated = new NintrollerLib.Trigger();

            if (_calibrationTarget == App.CAL_GCN_RTRIGGER)
            {
                nonCalibrated = Calibrations.None.GameCubeControllerRaw.R;
                curCalibrated = controller.R;
            }
            else if (_calibrationTarget == App.CAL_GCN_LTRIGGER)
            {
                nonCalibrated = Calibrations.None.GameCubeControllerRaw.L;
                curCalibrated = controller.L;
            }

            Windows.TriggerCalibrationWindow trigCal = new Windows.TriggerCalibrationWindow(nonCalibrated, curCalibrated);
            _openTrigWindow = trigCal;
            trigCal.ShowDialog();

            if (trigCal.Apply)
            {
                OnTriggerCalibrated?.Invoke(trigCal.Calibration, _calibrationTarget, trigCal.FileName);
            }

            _openJoyWindow = null;
            trigCal = null;
        }

        private void portSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (portSelection.SelectedIndex)
            {
                default:
                case 0:
                    _activePort = GameCubePort.PORT1;
                    break;
                case 1:
                    _activePort = GameCubePort.PORT2;
                    break;
                case 2:
                    _activePort = GameCubePort.PORT3;
                    break;
                case 3:
                    _activePort = GameCubePort.PORT4;
                    break;
            }

            _inputPrefix = ((int)_activePort).ToString() + "_";
        }
    }
}
