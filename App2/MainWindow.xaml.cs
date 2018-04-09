using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Threading;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows.Forms;
using Coding4Fun.Kinect.Wpf;
using System.Windows.Media.Imaging;

namespace App2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor sensor;
        private String noKinectReady = "No Kinect connected.";
        ConcurrentDictionary<String, ColorImagePoint> dict = new ConcurrentDictionary<string, ColorImagePoint>();
        ConcurrentDictionary<String, String> squatDict = new ConcurrentDictionary<string, string>();
        private readonly AutoResetEvent _isStopping = new AutoResetEvent(false);
        bool startPosFound = false;

        private const float ClickHoldingRectThreshold = 0.05f;
        private Rect _clickHoldingLastRect;
        private readonly Stopwatch _clickHoldingTimer = new Stopwatch();

        private const float SkeletonMaxX = 0.60f;
        private const float SkeletonMaxY = 0.40f;

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                ColourStream colourStream = new ColourStream();
                Image.Source = colourStream.StartColourStream(sensor);
                Button0.Background = Brushes.Gray;
                this.sensor.DepthStream.Enable();
                this.sensor.SkeletonStream.Enable();

                sensor.AllFramesReady += SensorAllFramesReady;
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = noKinectReady;
            }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
            Environment.Exit(Environment.ExitCode);
        }

        void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            SensorSkeletonFrameReady(e);
        }

        void SensorSkeletonFrameReady(AllFramesReadyEventArgs e)
        {
            if (!startPosFound)
            {
                using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
                {
                    if (skeletonFrameData == null)
                    {
                        return;
                    }

                    var allSkeletons = new Skeleton[skeletonFrameData.SkeletonArrayLength];

                    skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                    foreach (Skeleton sd in allSkeletons)
                    {
                        // the first found/tracked skeleton moves the mouse cursor
                        if (sd.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            // make sure both hands are tracked
                            if (sd.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
                            {
                                BitmapImage image = new BitmapImage(new Uri("/Images/hand.png", UriKind.Relative));
                                CursorImage.Source = image;
                                var wristRight = sd.Joints[JointType.WristRight];
                                var scaledRightHand = wristRight.ScaleTo((int)(SystemParameters.PrimaryScreenWidth), (int)(SystemParameters.PrimaryScreenHeight), SkeletonMaxX, SkeletonMaxY);

                                var cursorX = (int)(scaledRightHand.Position.X) + 10;
                                var cursorY = (int)(scaledRightHand.Position.Y) + 10;

                                Canvas.SetLeft(CursorImage, cursorX);
                                Canvas.SetTop(CursorImage, cursorY);

                                var leftClick = CheckForClickHold(scaledRightHand);
                                NativeMethods.SendMouseInput(cursorX, cursorY, (int)(SystemParameters.PrimaryScreenWidth), (int)(SystemParameters.PrimaryScreenHeight), leftClick);
                            }
                        }
                    }
                }
            }
        }

        private bool CheckForClickHold(Joint hand)
        {
            // This does one handed click when you hover for the allotted time.  It gives a false positive when you hover accidentally.
            var x = hand.Position.X;
            var y = hand.Position.Y;

            var screenwidth = (int)SystemParameters.PrimaryScreenWidth;
            var screenheight = (int)SystemParameters.PrimaryScreenHeight;
            var clickwidth = (int)(screenwidth * ClickHoldingRectThreshold);
            var clickheight = (int)(screenheight * ClickHoldingRectThreshold);

            var newClickHold = new Rect(x - clickwidth, y - clickheight, clickwidth * 2, clickheight * 2);

            if (_clickHoldingLastRect != Rect.Empty)
            {
                if (newClickHold.IntersectsWith(_clickHoldingLastRect))
                {
                    if ((int)_clickHoldingTimer.ElapsedMilliseconds > 2000)
                    {
                        _clickHoldingTimer.Stop();
                        _clickHoldingLastRect = Rect.Empty;
                        return true;
                    }

                    if (!_clickHoldingTimer.IsRunning)
                    {
                        _clickHoldingTimer.Reset();
                        _clickHoldingTimer.Start();
                    }
                    return false;
                }

                _clickHoldingTimer.Stop();
                _clickHoldingLastRect = newClickHold;
                return false;
            }

            _clickHoldingLastRect = newClickHold;
            if (!_clickHoldingTimer.IsRunning)
            {
                _clickHoldingTimer.Reset();
                _clickHoldingTimer.Start();
            }
            return false;
        }

        private void ColourStreamClick(object sender, RoutedEventArgs e)
        {
            if (null == this.sensor)
            {
                this.statusBarText.Text = noKinectReady;
            }
            else
            {
                ColourStream colourStream = new ColourStream();
                Image.Source = colourStream.StartColourStream(sensor);
                Button0.Background = Brushes.Gray;
                Button1.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
                Button2.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
            }
        }

        private void DepthStreamClick(object sender, RoutedEventArgs e)
        {
            if (null == this.sensor)
            {
                this.statusBarText.Text = noKinectReady;
            }
            else
            {
                DepthStream depthStream = new DepthStream();
                Image.Source = depthStream.StartDepthStream(sensor);
                Button1.Background = Brushes.Gray;
                Button0.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
                Button2.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
            }
        }

        private void SkeletonStreamClick(object sender, RoutedEventArgs e)
        {
            if (null == sensor)
            {
                this.statusBarText.Text = noKinectReady;
            }
            else
            {
                SkeletonStream skeletonStream = new SkeletonStream();
                Image.Source = skeletonStream.StartSkeletonStream(sensor);
                Button2.Background = Brushes.Gray;
                Button0.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
                Button1.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
            }
        }

        internal void GetDictionary(SkeletonPos skel, Exercise exercise)
        {
            TimeSpan waitInterval = TimeSpan.FromMilliseconds(50);
            List<String> keys = new List<String>();
            bool isEmpty;

            for (; !_isStopping.WaitOne(waitInterval);)
            {
                squatDict.Clear();
                using (var dictionaryEnum = dict.GetEnumerator())
                {
                    isEmpty = !dictionaryEnum.MoveNext();
                }

                if (isEmpty)
                {
                    foreach (KeyValuePair<String, ColorImagePoint> entry in skel.getJointPointDict().ToList())
                    {
                        dict.AddOrUpdate(entry.Key, entry.Value, (key, oldValue) => entry.Value);
                        keys.Add(entry.Key);
                    }
                    foreach (KeyValuePair<String, String> entry in exercise.GetDictionary().ToList())
                    {
                        squatDict.AddOrUpdate(entry.Key, entry.Value, (key, oldValue) => entry.Value);
                    }
                }

                else
                {
                    foreach (var key in keys)
                    {
                        foreach (KeyValuePair<String, ColorImagePoint> realEntry in skel.getJointPointDict().ToList())
                        {
                            if (key == realEntry.Key)
                            {
                                dict[key] = realEntry.Value;
                            }
                        }
                        foreach (KeyValuePair<String, String> realEntry in exercise.GetDictionary().ToList())
                        {
                            if (key == realEntry.Key)
                            {
                                squatDict[key] = realEntry.Value;
                            }
                        }
                    }
                }
                foreach (KeyValuePair<String, ColorImagePoint> item in dict)
                    if (!exercise.CheckStartPosFound())
                    {
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                DrawDots(Brushes.Gold, item.Key, item.Value);
                            });
                        }
                    }
                    else if (exercise.CheckStartPosFound())
                    {
                        {
                            if (squatDict.ContainsKey(item.Key))
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    DrawDots(Brushes.Red, item.Key, item.Value);
                                });
                            } else
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    DisplayTextOnce("Starting Position Found - Squat!");
                                    DrawDots(Brushes.SpringGreen, item.Key, item.Value);
                                });
                            }
                        }
                    }
            }
        }

        private void DisplayText(String text)
        {
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, ea) => Thread.Sleep(TimeSpan.FromSeconds(2));
            backgroundWorker.RunWorkerCompleted += (s, ea) =>
            {
                this.animatedText.Text = "";
            };

            this.animatedText.Text = text;
            backgroundWorker.RunWorkerAsync();
        }

        private void DisplayTextOnce(String text)
        {
            if (!startPosFound)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (s, ea) => Thread.Sleep(TimeSpan.FromSeconds(2));
                backgroundWorker.RunWorkerCompleted += (s, ea) =>
                {
                    this.animatedText.Text = "";
                };

                this.animatedText.Text = text;
                backgroundWorker.RunWorkerAsync();
                startPosFound = true;
            }
        }

        private void Squat(object sender, RoutedEventArgs e)
        {
            Squat squatMode = new Squat();
            SkeletonPos skeletonPos = new SkeletonPos();
            skeletonPos.StartSkeletonStream(sensor);

            DisplayText("Enter Starting Position");

            if (null == sensor)
            {
                this.statusBarText.Text = noKinectReady;
                return;
            }

            //Update dictionary - ONE THREAD
            Thread getDict = new Thread(() => GetDictionary(skeletonPos, squatMode));
            getDict.Start();

            if (squatMode.CheckStartPosFound())
            {
                DisplayText("Starting Position Found - Squat!");
                
            }
            squatMode.StartExercise(sensor);

            //Change of UI
            this.statusBarText.Text = "Squat mode activated";
            this.activityText.Text = "Please enter starting position.";
            this.DemoImage.Source = squatMode.ShowImage();
            SquatButton.Background = Brushes.Gray;
            DeadliftButton.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
            OHPButton.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
        }

        private void OverheadPress(object sender, RoutedEventArgs e)
        {
            OverheadPress overheadPress = new OverheadPress();
            SkeletonPos skeletonPos = new SkeletonPos();
            skeletonPos.StartSkeletonStream(sensor);

            DisplayText("Enter Starting Position");

            if (null == sensor)
            {
                this.statusBarText.Text = noKinectReady;
                return;
            }

            Thread getDict = new Thread(() => GetDictionary(skeletonPos, overheadPress));
            getDict.Start();

            if (overheadPress.CheckStartPosFound()) {
                DisplayText("Starting Position Found - Press!");
            }

            overheadPress.StartExercise(sensor);

            this.statusBarText.Text = "Overhead press mode activated";
            this.activityText.Text = "Please enter starting position";
            this.DemoImage.Source = overheadPress.ShowImage();
            OHPButton.Background = Brushes.Gray; Button0.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
            SquatButton.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
            DeadliftButton.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));

        }

        private void Deadlift(object sender, RoutedEventArgs e)
        {
            Deadlift deadlift = new Deadlift();
            SkeletonPos skeletonPos = new SkeletonPos();
            skeletonPos.StartSkeletonStream(sensor);

            DisplayText("Enter Starting Position");

            if (null == sensor)
            {
                this.statusBarText.Text = noKinectReady;
                return;
            }

            Thread getDict = new Thread(() => GetDictionary(skeletonPos, deadlift));
            getDict.Start();

            if (deadlift.CheckStartPosFound())
            {
                DisplayText("Starting Position Found - Lift!");
            }

            deadlift.StartExercise(sensor);

            this.statusBarText.Text = "Deadlift mode activated";
            this.activityText.Text = "Please enter starting position.";
            this.DemoImage.Source = deadlift.ShowImage();
            DeadliftButton.Background = Brushes.Gray;
            SquatButton.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
            OHPButton.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
        }

        private void DrawDots(Brush colour, String joint, ColorImagePoint point)
        {
            foreach (Object obj in Canvas.Children)
            {
                var ellipse = obj as Ellipse;
                if (ellipse != null && ellipse.Name.Contains(joint))
                {
                    ellipse.Fill = colour;
                    ellipse.Stroke = Brushes.Black;
                    Canvas.SetLeft(ellipse, (point.X + 50) * 2);
                    Canvas.SetTop(ellipse, point.Y * 2);
                }
            }
        }
    }
}
