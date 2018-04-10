using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace App2
{
    internal class Deadlift : Exercise
    {
        private KinectSensor sensor;
        Skeleton[] skeletons = new Skeleton[3];
        private static bool startingPosFound;
        private static float skeletonHeight = 0.0f;
        private static float standingHipHeight = 0.0f;
        private static ConcurrentDictionary<String, String> jointErrorDict = new ConcurrentDictionary<String, String>();

        public ConcurrentDictionary<string, string> GetDictionary()
        {
            return jointErrorDict;
        }

        public ImageSource ShowImage()
        {
            BitmapImage image = new BitmapImage(new Uri("/Images/DeadliftStart.png", UriKind.Relative));
            System.Windows.Controls.Image myImage = new System.Windows.Controls.Image();
            myImage.Source = image;
            return myImage.Source;
        }

        public float SetSkeletonHeight(Skeleton skeleton)
        {
            float footAverageY = (skeleton.Joints[JointType.FootRight].Position.Y + skeleton.Joints[JointType.FootLeft].Position.Y) / 2;
            standingHipHeight = skeleton.Joints[JointType.HipCenter].Position.Y - footAverageY + 0.1f;
            return skeleton.Joints[JointType.Head].Position.Y - footAverageY + 0.1f;
        }

        public void StartExercise(KinectSensor sensor)
        {
            this.sensor = sensor;
            StartSkeletonStream(sensor);
        }

        public void StartSkeletonStream(KinectSensor sensor)
        {
            this.sensor = sensor;
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
                                skeletonHeight = SetSkeletonHeight(skeleton);
                            }
                            if (CheckStartingPos(skeleton))
                            {
                                TrackDeadlift(skeleton);
                            }
                        }
                    }
                }
            }
        }

        public bool CheckStartPosFound()
        {
            if (startingPosFound)
            {
                return true;
            }
            else return false;
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
            Joint rightFoot = skeleton.Joints[JointType.FootRight];
            Joint leftFoot = skeleton.Joints[JointType.FootLeft];

            float handsY = ((rightHand.Position.Y + leftHand.Position.Y) / 2) - (0.1f * skeletonHeight);
            float kneesY = (rightKnee.Position.Y + leftKnee.Position.Y) / 2;
            float kneesZ = (rightKnee.Position.Z + leftKnee.Position.Z) / 2;

            if (handsY < kneesY && waist.Position.Z > kneesZ - 0.1 * skeletonHeight)
            {
                startingPosFound = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static void TrackDeadlift(Skeleton skeleton)
        {
            jointErrorDict.Clear();

            if (skeleton.Joints[JointType.Head].Position.Z > skeleton.Joints[JointType.HipCenter].Position.Z)
            {
                //Debug this when someone is home
                jointErrorDict.AddOrUpdate("Head", "Leaning too far back at the end.", (key, oldValue) => "Leaning too far back at the end.");
            }

            float kneesZ = (skeleton.Joints[JointType.KneeLeft].Position.Z + skeleton.Joints[JointType.KneeRight].Position.Z) / 2;

            if (skeleton.Joints[JointType.ShoulderCenter].Position.Z < kneesZ)
            {
                //Debug this when someone is home
                jointErrorDict.AddOrUpdate("ShoulderLeft", "Leaning too far forward.", (key, oldValue) => "Leaning too far forward.");
                jointErrorDict.AddOrUpdate("ShoulderRight", "Leaning too far forward.", (key, oldValue) => "Leaning too far forward.");
            }

            if (!(skeleton.Joints[JointType.HipRight].Position.X >= (skeleton.Joints[JointType.ShoulderCenter].Position.X + 0.02 * skeletonHeight)))
            {
                jointErrorDict.AddOrUpdate("HipRight", "Right hip out of line", (key, oldValue) => "Right hip out of line");
            }

            if (!(skeleton.Joints[JointType.HipLeft].Position.X <= skeleton.Joints[JointType.ShoulderCenter].Position.X - 0.02 * skeletonHeight))
            {
                jointErrorDict.AddOrUpdate("HipLeft", "Left hip coming out of line", (key, oldValue) => "Left hip out of line");
            }

            //Insert things for back rounding
        }
    }
}