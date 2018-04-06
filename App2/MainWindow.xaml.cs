using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;
using System.Timers;

namespace App2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private KinectSensor sensor;
        private String noKinectReady = "No Kinect connected.";
        Dictionary<String, ColorImagePoint> dict = new Dictionary<string, ColorImagePoint>();
        Dictionary<String, String> squatDict = new Dictionary<string, string>();
        private readonly AutoResetEvent _isStopping = new AutoResetEvent(false);
        bool startPostFound = false;

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
                        dict.Add(entry.Key, entry.Value);
                        keys.Add(entry.Key);
                    }
                    foreach (KeyValuePair<String, String> entry in exercise.GetDictionary().ToList())
                    {
                        squatDict.Add(entry.Key, entry.Value);
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
            if (!startPostFound)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (s, ea) => Thread.Sleep(TimeSpan.FromSeconds(2));
                backgroundWorker.RunWorkerCompleted += (s, ea) =>
                {
                    this.animatedText.Text = "";
                };

                this.animatedText.Text = text;
                backgroundWorker.RunWorkerAsync();
                startPostFound = true;
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
                    Canvas.SetLeft(ellipse, point.X + 50);
                    Canvas.SetTop(ellipse, point.Y - ellipse.Width / 2);
                }
            }
        }
    }
}
