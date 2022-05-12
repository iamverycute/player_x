using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace player_x
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        VideoCapture _capture;
        bool canPlay = true;
        double frameCount;
        double fps;
        int pauseTime;
        bool isPlaying = true;
        bool autoSlider = true;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _capture = new VideoCapture();
            VideoControl.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(VideoControl_MouseLeftButtonUp), true);
            VideoControl.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(VideoControl_MouseLeftButtonDown), true);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            canPlay = false;
        }

        double percent = 0;
        private void VideoControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            percent = e.NewValue;
        }

        private void VideoControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            double calcRate = percent / 100 * frameCount;
            isPlaying = false;
            _capture.Set(VideoCaptureProperties.PosFrames, calcRate);
            isPlaying = true;
            autoSlider = true;
        }

        private void VideoControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            autoSlider = false;
        }

        private string filePath;

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false,
                Multiselect = false
            };
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                needSwitch = true;
                filePath = dialog.FileName;
                Thread.Sleep(1000);
                Play();
            }
        }

        bool needSwitch = false;

        public void Play()
        {
            needSwitch = false;
            _capture.Open(filePath);
            frameCount = _capture.Get(VideoCaptureProperties.FrameCount);
            fps = _capture.Get(VideoCaptureProperties.Fps);
            pauseTime = (int)Math.Round(800 / fps);
            new Thread(async () =>
           {
               while (canPlay)
               {
                   if (needSwitch) break;
                   if (isPlaying)
                   {
                       using (var _frame = new Mat())
                       {
                           if (_capture.Read(_frame) && !_frame.Empty())
                           {
                               await VideoView.Dispatcher.InvokeAsync(() => VideoView.Source = BitmapSourceConverter.ToBitmapSource(_frame));
                           }
                           double curFrames = _capture.Get(VideoCaptureProperties.PosFrames);
                           if (autoSlider)
                           {
                               await VideoControl.Dispatcher.InvokeAsync(() => VideoControl.Value = curFrames / frameCount * 100);
                           }
                       }
                       Thread.Sleep(pauseTime);
                   }
                   else
                   {
                       Thread.Sleep(1000);
                   }
               }
           }).Start();
        }
    }
}
