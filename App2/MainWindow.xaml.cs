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

namespace App2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private KinectSensor sensor;
        private String noKinectReady = "No Kinect connected.";

        public MainWindow()
        {
            InitializeComponent();
        }

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

        private void Squat(object sender, RoutedEventArgs e)
        {
            if (null == sensor)
            {
               this.statusBarText.Text = noKinectReady;
            }
            else
            {
                this.statusBarText.Text = "Squat mode acitivated";
                Squat squatMode = new App2.Squat();
                squatMode.StartSquatMode(sensor);
                this.activityText.Text = "Please enter starting position.";
                this.DemoImage.Source = squatMode.ShowSquatImage();
            }
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
    }
}
