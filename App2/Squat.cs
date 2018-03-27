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
using System.IO;


namespace App2
{
    class Squat
    {
        private KinectSensor sensor;
        Skeleton[] skeletons = new Skeleton[3];
        public static bool startingPosFound;
        private static bool depthAchieved;
        private static float skeletonHeight = 0.0f;

        public ImageSource ShowSquatImage()
        {
            BitmapImage image = new BitmapImage(new Uri("/Images/SquatStart.png", UriKind.Relative));
            System.Windows.Controls.Image myImage = new System.Windows.Controls.Image();
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
                            if (skeletonHeight == 0.0f)
                            {
                                skeletonHeight = setSkeletonHeight(skeleton);
                            }
                            if (CheckStartingPos(skeleton))
                            {
                                TrackSquat(skeleton);
                            }
                        }
                    }
                }
            }
        }

        internal static float setSkeletonHeight(Skeleton skeleton)
        {
            float footAverageY = (skeleton.Joints[JointType.FootRight].Position.Y + skeleton.Joints[JointType.FootLeft].Position.Y) / 2;
            return skeleton.Joints[JointType.Head].Position.Y - footAverageY;
        }

        internal static bool CheckStartingPos(Skeleton skeleton)
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

            if ((Math.Abs((head.Position.X + neck.Position.X + spine.Position.X + waist.Position.X) / 4 - Math.Abs(waist.Position.X)) < (0.1 * skeletonHeight)) &&
                    (Math.Abs((head.Position.Z + neck.Position.Z + spine.Position.Z + waist.Position.Z) / 4 - Math.Abs(waist.Position.Z)) < (0.1 * skeletonHeight)) &&
                    (Math.Abs((leftShoulder.Position.Y + leftHand.Position.Y) / 2 - Math.Abs(leftShoulder.Position.Y)) < (0.1 * skeletonHeight)) &&
                    (Math.Abs((rightShoulder.Position.Y + rightHand.Position.Y) / 2 - Math.Abs(rightShoulder.Position.X)) <( 0.1 * skeletonHeight)) &&
                    (Math.Abs((rightHip.Position.X + rightKnee.Position.X + rightAnkle.Position.X) / 3 - Math.Abs(rightHip.Position.X)) < (0.2 * skeletonHeight)) &&
                    (Math.Abs((leftHip.Position.X + leftKnee.Position.X + leftAnkle.Position.X) / 3 - Math.Abs(leftHip.Position.X)) < (0.2 * skeletonHeight)))
            {
                Debug.WriteLine("Starting Position Found");
                startingPosFound = true;
                return true;
            }
            else
            {
                return false; 
            }

        }

 //      private static void WriteStartPosToCSV(Skeleton skeleton)
 //     {
 //         String filepath = @"StartPos.csv";
 //         String delimiter = ",";
//
//          String[] headings = new string[41];
//          String[] values = new string[41];
//          StringBuilder sb = new StringBuilder();
//          foreach (Joint joint in skeleton.Joints)
//          {
//              headings[iterator] = joint.JointType.ToString() + " X: ";
//              headings[iterator + 1] = joint.JointType.ToString() + " Y: ";
//
//              values[iterator] = joint.Position.X.ToString();
//              values[iterator + 1] = joint.Position.Y.ToString();
//              iterator++;
//          }
//         String s = null;
//          for (int i = 0; i < headings.Length; i++)
//          {
//              s += String.Format("{0,-40} {1,-40:N0}\n", headings[i], values[i]);
//          }
//          File.WriteAllText(filepath, s);
//      }

        internal static void TrackSquat(Skeleton skeleton)
        {
            //Average Knee Y Position
            float kneesY = (skeleton.Joints[JointType.KneeLeft].Position.Y + skeleton.Joints[JointType.KneeRight].Position.Y) / 2;
            float kneesZ = (skeleton.Joints[JointType.KneeLeft].Position.Z + skeleton.Joints[JointType.KneeRight].Position.Z) / 2;

            if (skeleton.Joints[JointType.AnkleLeft].Position.Y > (skeleton.Joints[JointType.FootLeft].Position.Y + (0.02 * skeletonHeight)))
            {
                Debug.WriteLine("Left heel coming off floor");
            }

            if (skeleton.Joints[JointType.AnkleRight].Position.Y > (skeleton.Joints[JointType.FootRight].Position.Y + (0.02 * skeletonHeight)));
            {
                Debug.WriteLine("Right heel coming off floor");
            }

            if (skeleton.Joints[JointType.Head].Position.Z < kneesZ)
            {
                Debug.WriteLine("Head coming too far forward");
            }

            if (skeleton.Joints[JointType.FootLeft].Position.X - Math.Abs(skeleton.Joints[JointType.HipCenter].Position.X) - 
                skeleton.Joints[JointType.FootRight].Position.X - Math.Abs(skeleton.Joints[JointType.HipCenter].Position.X) > (0.05 * skeletonHeight))
            {
                Debug.WriteLine("Hips not in line");
            }

            if (skeleton.Joints[JointType.KneeLeft].Position.X < skeleton.Joints[JointType.AnkleLeft].Position.X - (0.02 * skeletonHeight))
            {
                Debug.WriteLine("Left knee out of line");
            }

            if (skeleton.Joints[JointType.KneeRight].Position.X > skeleton.Joints[JointType.AnkleRight].Position.X - (0.02 * skeletonHeight))
            {
                Debug.WriteLine("Right knee out of line");
            }

            if (skeleton.Joints[JointType.HipCenter].Position.Y <= kneesY)
            {
                depthAchieved = true;
            }
            else if (CheckStartingPos(skeleton) && depthAchieved == false)
            {
                Debug.WriteLine("Didn't reach parallel");
            }
        }
    }
}