using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;

namespace App2
{
    class Squat
    {
        private KinectSensor sensor;
        Skeleton[] skeletonData;
        //private BitmapImage SquatImage;

        public ImageSource ShowSquatImage()
        {
            BitmapImage image = new BitmapImage(new Uri("/Images/SquatStart.png", UriKind.Relative));
            Image myImage = new Image();
            myImage.Source = image;
            return myImage.Source;
        }

        internal void StartSquatMode(KinectSensor sensor)
        {
            this.sensor = sensor;
            sensor.SkeletonStream.Enable(); // Enable skeletal tracking

            skeletonData = new Skeleton[sensor.SkeletonStream.FrameSkeletonArrayLength]; // Allocate ST data

            //sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady); // Get Ready for Skeleton Ready Events

            sensor.Start();

            //Enter starting position

            //When starting position entered, begin movement. 

            //When end position reached, stop recording. 

            //Save as one gesture and analyse. 

            //Average over 10 reps? 
        }

    }
}