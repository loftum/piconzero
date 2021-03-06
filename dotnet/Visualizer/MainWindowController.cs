﻿using System;
using Foundation;
using AppKit;
using CoreGraphics;
using Visualizer.ViewControllers;

namespace Visualizer
{
    [Register("MainWindowController")]
    public class MainWindowController : NSWindowController
    {
        private readonly ConnectViewController _connectViewController = new ConnectViewController();
        private readonly VisualizerViewController _visualizerViewcontroller = new VisualizerViewController();
        private readonly MetalTestViewController _metalTestViewController = new MetalTestViewController();

        public MainWindowController(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public MainWindowController(NSCoder coder) : base(coder)
        {
        }

        public MainWindowController() : base("MainWindow")
        {
            var contentRect = new CGRect(0, 0, 1200, 1200);
            base.Window = new MainWindow(contentRect, NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable, NSBackingStore.Buffered, false);

            //Window.ContentView = _metalTestViewController.View;
            Window.ContentView = _connectViewController.View;

            _connectViewController.OnConnect += OnConnect;
            _visualizerViewcontroller.OnDisconnect += OnDisconnect;
        }

        private void OnDisconnect(object sender, EventArgs e)
        {
            Window.ContentView = _connectViewController.View;
        }

        private async void OnConnect(object sender, ConnectEventArgs e)
        {
            await _visualizerViewcontroller.ConnectAsync(e.Host, e.Port);
            Window.ContentView = _visualizerViewcontroller.View;
        }

        public new MainWindow Window => (MainWindow)base.Window;
    }
}
