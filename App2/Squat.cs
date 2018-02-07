﻿using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;

namespace App2
{
    class Squat
    {
        private KinectSensor sensor;
        Skeleton[] skeletons = new Skeleton[3];

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
            StartSkeletonStream(sensor);
        }

        internal void StartSkeletonStream(KinectSensor sensor1)
        {
            this.sensor = sensor1;
            sensor.SkeletonStream.Enable();
            sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
            sensor.Start();
        }

        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }
            if (skeletons.Length != 0)
            {
                foreach (Skeleton skeleton in skeletons)
                {
                    if (skeleton != null)
                    {
                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            if (CheckStartingPos(skeleton))
                            {
                                TrackSquat(skeleton);
                            }
                        }
                    }
                }
            }
        }

        internal Boolean CheckStartingPos(Skeleton skeleton)
        {
            Joint head = skeleton.Joints[JointType.Head];
            Joint neck = skeleton.Joints[JointType.ShoulderCenter];
            Joint rightShoulder = skeleton.Joints[JointType.ShoulderRight];
            Joint leftShoulder = skeleton.Joints[JointType.ShoulderLeft];
            Joint rightElbow = skeleton.Joints[JointType.ElbowRight];
            Joint leftElbow = skeleton.Joints[JointType.ElbowLeft];
            Joint rightHand = skeleton.Joints[JointType.WristRight];
            Joint leftHand = skeleton.Joints[JointType.WristLeft];
            Joint spine = skeleton.Joints[JointType.Spine];
            Joint waist = skeleton.Joints[JointType.HipCenter];
            Joint leftHip = skeleton.Joints[JointType.HipLeft];
            Joint rightHip = skeleton.Joints[JointType.HipRight];
            Joint leftKnee = skeleton.Joints[JointType.KneeLeft];
            Joint rightKnee = skeleton.Joints[JointType.KneeRight];
            Joint leftAnkle = skeleton.Joints[JointType.AnkleLeft];
            Joint rightAnkle = skeleton.Joints[JointType.AnkleRight];

            if ((Math.Abs((head.Position.X + neck.Position.X + spine.Position.X + waist.Position.X) / 4 - Math.Abs(waist.Position.X)) < 0.1f) &&
                    (Math.Abs((head.Position.Z + neck.Position.Z + spine.Position.Z + waist.Position.Z) / 4 - Math.Abs(waist.Position.Z)) < 0.1f) &&
                    (Math.Abs((leftShoulder.Position.Y + leftHand.Position.Y) / 2 - Math.Abs(leftShoulder.Position.Y)) < 0.1f) &&
                    (Math.Abs((rightShoulder.Position.Y + rightHand.Position.Y) / 2 - Math.Abs(rightShoulder.Position.X)) < 0.1f) &&
                    (Math.Abs((rightHip.Position.X + rightKnee.Position.X + rightAnkle.Position.X) / 3 - Math.Abs(rightHip.Position.X)) < 0.2f) &&
                    (Math.Abs((leftHip.Position.X + leftKnee.Position.X + leftAnkle.Position.X) / 3 - Math.Abs(leftHip.Position.X)) < 0.2f))
            {
                return true;
            }
            else
            {
                return false; 
            }

        }

        internal static void TrackSquat(Skeleton skeleton)
        {
            if (skeleton.Joints[JointType.AnkleLeft].Position.Y > (skeleton.Joints[JointType.FootLeft].Position.Y + 0.05f))
            {
                Debug.WriteLine("Left heel coming off floor");
            }

            if (skeleton.Joints[JointType.AnkleRight].Position.Y > (skeleton.Joints[JointType.FootRight].Position.Y + 0.05f));
            {
                Debug.WriteLine("Right heel coming off floor");
            }

            if ((skeleton.Joints[JointType.Head].Position.Z < (skeleton.Joints[JointType.KneeLeft].Position.Z + 0.1f)) || 
                (skeleton.Joints[JointType.Head].Position.Z < (skeleton.Joints[JointType.KneeRight].Position.Z + 0.1f))) {
                Debug.WriteLine("Head coming too far forward");
            }

            if (skeleton.Joints[JointType.FootLeft].Position.X - Math.Abs(skeleton.Joints[JointType.HipCenter].Position.X) - 
                skeleton.Joints[JointType.FootRight].Position.X - Math.Abs(skeleton.Joints[JointType.HipCenter].Position.X) > 0.1f)
            {
                Debug.WriteLine("Hips not in line");
            }

            if (skeleton.Joints[JointType.KneeLeft].Position.X < skeleton.Joints[JointType.AnkleLeft].Position.X - 0.05f)
            {
                Debug.WriteLine("Left knee out of line");
            }

            if (skeleton.Joints[JointType.KneeRight].Position.X > skeleton.Joints[JointType.AnkleRight].Position.X - 0.05f)
            {
                Debug.WriteLine("Right knee out of line");
            }

           //If depth isn't achieved

        }

    }
}