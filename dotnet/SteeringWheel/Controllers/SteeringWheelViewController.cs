﻿using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using CoreFoundation;
using CoreGraphics;
using CoreMotion;
using LCTP.Core.Client;
using Lego.Client;
using Lego.Core;
using Maths;
using UIKit;

namespace SteeringWheel.Controllers
{
    public class SteeringWheelViewController : UIViewController
    {
        private readonly LegoCarClient _client;
        private readonly CMMotionManager _motionManager = MotionManager.Instance;

        private readonly UISlider _throttleSlider;
        private readonly UIButton _headlightsButton;
        private readonly UIButton _leftBlinkerButton;
        private readonly UIButton _rightBlinkerButton;
        private readonly UIButton _disconnectButton;
        private readonly UILabel _speedometer;
        private readonly InterlockedAsyncTimer _timer = new InterlockedAsyncTimer(15);
        private readonly UIImpactFeedbackGenerator _impactFeedback = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Heavy);

        public SteeringWheelViewController(string host, int port) : base(null, null)
        {
            View.BackgroundColor = UIColor.White;   
            
            _throttleSlider = new UISlider
            {
                MinValue = -255,
                MaxValue = 255,
            }
            .WithParent(View)
            .Rotate(Math.PI * 3 / 2)
            .WithTouchUpInside(Throttle_Cancel)
            .WithTouchUpOutside(Throttle_Cancel)
            ;

            _headlightsButton = new UIButton()
            .WithTitle("💡")
            .WithParent(View)
            .WithConstraints(v => new[] {
                v.TopAnchor.ConstraintEqualTo(View.TopAnchor, 100),
                v.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor)
            })
            .WithTouchUpInside(HeadlightsButton_TouchUpInside);

            _leftBlinkerButton = new UIButton()
            .WithTitle("⇦")
            .WithTitleColor(UIColor.Green)
            .WithParent(View)
            .WithConstraints(v => new[] {
                v.TopAnchor.ConstraintEqualTo(View.TopAnchor, 100),
                v.TrailingAnchor.ConstraintEqualTo(_headlightsButton.LeadingAnchor, -50),
            })
            .WithTouchUpInside(LeftBlinkerButton_TouchUpInside);

            _rightBlinkerButton = new UIButton()
            .WithTitle("⇨")
            .WithTitleColor(UIColor.Green)
            .WithParent(View)
            .WithConstraints(v => new[] {
                v.TopAnchor.ConstraintEqualTo(View.TopAnchor, 100),
                v.LeadingAnchor.ConstraintEqualTo(_headlightsButton.TrailingAnchor, 50),
            })
            .WithTouchUpInside(RightBlinkerButton_TouchUpInside);
            
            _disconnectButton = new UIButton()
            .WithTitle("Disconnect")
            .WithTitleColor(UIColor.Blue)
            .WithParent(View)
            .WithConstraints(v => new[] {
                v.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                v.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
            })
            .WithTouchUpInside(DisconnectButton_TouchUpInside);
            
            _speedometer = new UILabel()
                .With(l =>
                {
                    l.BackgroundColor = UIColor.SystemPinkColor;
                    l.Text = "0";
                    l.TextAlignment = UITextAlignment.Center;
                    l.Font = UIFont.PreferredTitle1.WithSize(50f);
                    l.TextColor = UIColor.Orange;
                })
                .WithParent(View)
                .WithConstraints(v => new[]
                {
                    v.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                    v.BottomAnchor.ConstraintEqualTo(View.BottomAnchor, 100)
                });

            // Set throttle positions
            var frame = View.GetFrame();
            const int width = 50;
            const int height = 200;
            _throttleSlider.Frame = new CGRect(frame.Width - 100, frame.Height / 2 - height / 2, width, height);
            
            var client = new LctpClient(UIDevice.CurrentDevice.Name, host, port);
            _client = new LegoCarClient(client);
        }

        public async Task ConnectAsync()
        {
            await _client.ConnectAsync();
            _motionManager.DeviceMotionUpdateInterval = 0.1;
            _motionManager.StartDeviceMotionUpdates();
            _timer.Elapsed = UpdateAsync;
            _timer.Start();
        }

        private async Task UpdateAsync()
        {
            var sw = new Stopwatch();
            sw.Start();
            DispatchQueue.MainQueue.DispatchSync(() =>
            {
                _client.SetMotorSpeed((int)_throttleSlider.Value);
                _client.SetSteer(GetSteerAngle());
                
            });
            try
            {
                await _client.UpdateAsync();
                var state = _client.GetState();
                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    _speedometer.Text = $"{state.Speed.X}";
                    if (state.Speed.Y < -100 || state.Speed.Y > 100)
                    {
                        _impactFeedback.ImpactOccurred();
                    }
                });
            }
            catch (Exception exception)
            {
                var inner = exception.GetBaseException();
                switch (inner)
                {
                    case null:
                        Console.WriteLine(exception);
                        await DisconnectAndDismissAsync();
                        break;
                    case SocketException socketException:
                        Console.WriteLine($"SocketException: {socketException.SocketErrorCode}");
                        Console.WriteLine(exception);
                        await DisconnectAndDismissAsync();
                        break;
                    default:
                        Console.WriteLine(exception);
                        await DisconnectAndDismissAsync();
                        break;
                }
            }
            finally
            {
                sw.Stop();
                Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms");
            }
        }

        private int GetSteerAngle()
        {
            var attitude = _motionManager.DeviceMotion?.Attitude;
            var angle = 90 - attitude?.Pitch.ToDeg() ?? 0;
            //Console.WriteLine($"Angle: {angle}");
            return angle;
        }

        private void HeadlightsButton_TouchUpInside(object sender, EventArgs e)
        {
            _client.HeadlightSwitch.Toggle();
        }

        private void RightBlinkerButton_TouchUpInside(object sender, EventArgs e)
        {
            _client.RightBlinkerSwitch.Toggle();
        }

        private void LeftBlinkerButton_TouchUpInside(object sender, EventArgs e)
        {
            _client.LeftBlinkerSwitch.Toggle();
        }

        private void Throttle_Cancel(object sender, EventArgs e)
        {
            _throttleSlider.Value = 0;
        }

        private async void DisconnectButton_TouchUpInside(object sender, EventArgs e)
        {
            await DisconnectAndDismissAsync();
        }
        
        private async Task DisconnectAndDismissAsync()
        {
            await DisconnectAsync();
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                DismissViewController(true, () => { });    
            });
        }

        private async Task DisconnectAsync()
        {
            _timer.Stop();
            _timer.Elapsed = null;
            await _client.DisconnectAsync();
            _motionManager.StopDeviceMotionUpdates();
        }
    }
}

