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
        private readonly AutoResetEvent _isStopping = new AutoResetEvent(false);

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

        private void GetDictionary (SkeletonPos skel)
        {
            TimeSpan waitInterval = TimeSpan.FromMilliseconds(50);
            List<String> keys = new List<String>();
            bool isEmpty;

            for (; !_isStopping.WaitOne(waitInterval);)
            {
                using (var dictionaryEnum = dict.GetEnumerator())
                {
                    isEmpty = !dictionaryEnum.MoveNext();
                }

                if (isEmpty)
                {
                    foreach (KeyValuePair<String, ColorImagePoint> entry in skel.getJointPointDict())
                    {
                        dict.Add(entry.Key, entry.Value);
                        keys.Add(entry.Key);
                    }
                }

                else
                {
                    foreach (var key in keys)
                    {
                        foreach (KeyValuePair<String, ColorImagePoint> realEntry in skel.getJointPointDict())
                        {
                            if (key == realEntry.Key)
                            {
                                dict[key] = realEntry.Value;
                            }
                        }
                    }
                }
                foreach (KeyValuePair<String, ColorImagePoint> item in dict)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        DrawDots(Brushes.Gold, item.Key, item.Value);
                    });
                }
            }
        }

        private void Squat(object sender, RoutedEventArgs e)
        {
            Squat squatMode = new Squat();

            SkeletonPos skeletonPos = new SkeletonPos();
            skeletonPos.StartSkeletonStream(sensor);
            //Boolean to control whether starting position was found
            bool startPosFound = false;

            if (null == sensor)
            {
                this.statusBarText.Text = noKinectReady;
                return;
            }

            //Update dictionary - ONE THREAD
            Thread getDict = new Thread(() => GetDictionary(skeletonPos));
            getDict.Start();

            //Draw on screen - One thread? 

            //foreach (KeyValuePair<String, ColorImagePoint> item in dict)
            //{
            //    DrawDots(Brushes.Gold, item.Value);
            //}
            
            
            ////Check start pos. If found draw green dots.
            //while (startPosFound)
            //{
            //    this.animatedText.Opacity = 1.0;
            //    this.animatedText.Text = "Go!";
            //    foreach (KeyValuePair<Joint, ColorImagePoint> item in dict)
            //    {
            //        DrawDots(Brushes.Green, item.Value);
            //    }
            //}

            //Check squat - another thread
            squatMode.StartSquatMode(sensor);

        
            //Change of UI
            this.statusBarText.Text = "Squat mode activated";
            this.activityText.Text = "Please enter starting position.";
            this.DemoImage.Source = squatMode.ShowSquatImage();
            SquatButton.Background = Brushes.Gray;
    
        }
        

        private void Deadlift(object sender, RoutedEventArgs e)
        {
            if (null == sensor)
            {
                this.statusBarText.Text = noKinectReady;
            }
            else
            {
                this.statusBarText.Text = "Deadlift mode activated";
            }
        }

        private void OverheadPress(object sender, RoutedEventArgs e)
        {
            if (null == sensor)
            {
                this.statusBarText.Text = noKinectReady;
            }
            else
            {
                this.statusBarText.Text = "Overhead press mode activated";
            }

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
