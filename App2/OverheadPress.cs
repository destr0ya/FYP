﻿using Microsoft.Kinect;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace App2
{
    // Class for the overhead press exercise which implements the Exercise interface. 
    class OverheadPress : Exercise
    {
        private KinectSensor sensor;
        Skeleton[] skeletons = new Skeleton[3];
        private static bool startingPosFound;
        private static float skeletonHeight = 0.0f;
        private static ConcurrentDictionary<string, List<string>> jointErrorDict = new ConcurrentDictionary<string, List<string>>();
        private static float previousLeftKneeZ = 0.0f;
        private static float previousRightKneeZ = 0.0f;
        private static float previousRightKneeY = 0.0f;
        private static float previousLeftKneeY = 0.0f;

        public ConcurrentDictionary<string, List<string>> GetDictionary()
        {
            return jointErrorDict;
        }

        //Displays image of starting position for user to enter.
        public ImageSource ShowImage()
        {
            BitmapImage image = new BitmapImage(new Uri("/Images/SquatStart.png", UriKind.Relative));
            System.Windows.Controls.Image myImage = new System.Windows.Controls.Image();
            myImage.Source = image;
            return myImage.Source;
        }

        public void StartExercise(KinectSensor sensor)
        {
            this.sensor = sensor;
            StartSkeletonStream(sensor);
        }

        //Calls SensorSkeletonFrameReady with every refresh.
        public void StartSkeletonStream(KinectSensor sensor1)
        {
            this.sensor = sensor1;
            sensor.SkeletonStream.Enable();
            sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
            sensor.Start();
        }

        //Sets the height of the user if it hasn't been set already and begins tracking if the starting
        //position is found. 
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
                                TrackPress(skeleton);
                            }
                        }
                    }
                }
            }
        }

        //Sets skeleton height. 
        public float SetSkeletonHeight(Skeleton skeleton)
        {
            float footAverageY = (skeleton.Joints[JointType.FootRight].Position.Y + skeleton.Joints[JointType.FootLeft].Position.Y) / 2;
            return skeleton.Joints[JointType.Head].Position.Y - footAverageY + 0.1f;
        }

        //Public CheckStartPosFound() for use in MainWindow.xaml.cs for displaying text, 
        //changing dots colour etc. 
        public bool CheckStartPosFound()
        {
            if (startingPosFound)
            {
                return true;
            }
            else return false;
        }

        //Checks for the starting position. 
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
                    (Math.Abs((rightShoulder.Position.Y + rightHand.Position.Y) / 2 - Math.Abs(rightShoulder.Position.Y)) < (0.1 * skeletonHeight)) &&
                    (Math.Abs((rightHip.Position.X + rightKnee.Position.X + rightAnkle.Position.X) / 3 - Math.Abs(rightHip.Position.X)) < (0.2 * skeletonHeight)) &&
                    (Math.Abs((leftHip.Position.X + leftKnee.Position.X + leftAnkle.Position.X) / 3 - Math.Abs(leftHip.Position.X)) < (0.2 * skeletonHeight)))
            {
                startingPosFound = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        //Includes rules that the user needs to follow in order to perform the exercise correctly. 
        //If a joint position is found to be incorrect, it adds the joint to the jointErrorDict with a list of strings. 
        //The list of strings contains the problem and also a solution. 
        internal static void TrackPress(Skeleton skeleton)
        {
            jointErrorDict.Clear();

            //Knees Correction
            if (previousLeftKneeZ == 0.0f || previousRightKneeZ == 0.0f || previousRightKneeY == 0.0f || previousLeftKneeY == 0.0f)
            {
                previousLeftKneeZ = skeleton.Joints[JointType.KneeLeft].Position.X;
                previousLeftKneeY = skeleton.Joints[JointType.KneeLeft].Position.Y;
                previousRightKneeZ = skeleton.Joints[JointType.KneeRight].Position.X;
                previousRightKneeY = skeleton.Joints[JointType.KneeRight].Position.Y;
            }
            float currentLeftKneeY = skeleton.Joints[JointType.KneeLeft].Position.Y;
            float currentLeftKneeZ = skeleton.Joints[JointType.KneeLeft].Position.Z;
            float currentRightKneeY = skeleton.Joints[JointType.KneeRight].Position.Y;
            float currentRightKneeZ = skeleton.Joints[JointType.KneeRight].Position.Z;

            //Check if knees have changed position
            if (previousLeftKneeY > currentLeftKneeY + 0.005 * skeletonHeight || previousLeftKneeY < currentLeftKneeY - 0.005 * skeletonHeight || 
                previousLeftKneeZ > currentLeftKneeZ + 0.005 * skeletonHeight || previousLeftKneeZ < currentLeftKneeZ - 0.005 * skeletonHeight)
            {
                List<string> list = new List<string>
                {
                    "Knees should not move during overhead press.",
                    "This is an exercise for the shoulders, triceps and back. If you cannot lift the weight without using your knees, consider lightening it."
                };
                jointErrorDict.AddOrUpdate("KneeLeft", list, (key, oldValue) => list);
            }

            if (previousRightKneeY > currentRightKneeY + 0.005 * skeletonHeight || previousRightKneeY < currentRightKneeY - 0.005 * skeletonHeight || 
                previousRightKneeZ > currentRightKneeZ + 0.005 * skeletonHeight || previousRightKneeZ < currentRightKneeZ - 0.005 * skeletonHeight)
            {
                List<string> list = new List<string>
                {
                    "Knees should not move during overhead press.",
                    "This is an exercise for the shoulders, triceps and back. If you cannot lift the weight without using your knees, consider lightening it."
                };
                jointErrorDict.AddOrUpdate("KneeRight", list, (key, oldValue) => list);
            }

            //Update previous knee position store
            previousLeftKneeY = currentLeftKneeY;
            previousLeftKneeZ = currentLeftKneeZ;
            previousRightKneeY = currentRightKneeY;
            previousRightKneeZ = currentRightKneeZ;

            //Elbows Correction
            if (skeleton.Joints[JointType.ElbowLeft].Position.Z < (skeleton.Joints[JointType.ShoulderLeft].Position.Z - 0.1 * skeletonHeight) ||
                skeleton.Joints[JointType.ElbowLeft].Position.X < (skeleton.Joints[JointType.HandLeft].Position.X - 0.05 * skeletonHeight))
            {
                List<string> list = new List<string>
                {
                    "Elbows should not flare too far forward or out.",
                    "Keep your elbows close to your side and pointed down. This is safer and will help you lift more weight."
                };
                jointErrorDict.AddOrUpdate("ElbowLeft", list, (key, oldValue) => list);
            }

            if (skeleton.Joints[JointType.ElbowRight].Position.Z < (skeleton.Joints[JointType.ShoulderRight].Position.Z - 0.1 * skeletonHeight) ||
                skeleton.Joints[JointType.ElbowRight].Position.X > (skeleton.Joints[JointType.HandRight].Position.X + 0.05 * skeletonHeight))
            {
                List<string> list = new List<string>
                {
                    "Elbows should not flare too far forward or out.",
                    "Keep your elbows close to your side and pointed down. This is safer and will help you lift more weight."
                };
                jointErrorDict.AddOrUpdate("ElbowRight", list, (key, oldValue) => list);
            }

            //Hand Correction
            if (skeleton.Joints[JointType.HandRight].Position.Y < (skeleton.Joints[JointType.HandLeft].Position.Y - 0.05 * skeletonHeight))
            {
                List<string> list = new List<string>
                {
                    "Right hand is rising slower than the left hand",
                    "This is a sign of an imbalance in the right shoulder. Consider doing isolation exercises for that shoulder until it is as strong as the other. For example, single arm dumbbell press: https://www.youtube.com/watch?v=NVnyDQqmhPo"
                };
                jointErrorDict.AddOrUpdate("HandRight", list, (key, oldValue) => list);
            }

            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HandLeft].Position.Y + 0.05 * skeletonHeight)
            {
                List<string> list = new List<string>
                {
                    "Left hand is rising slower than the right hand",
                    "This is a sign of an imbalance in the left shoulder. Consider doing isolation exercises for that shoulder until it is as strong as the other. For example, single arm dumbbell press: https://www.youtube.com/watch?v=NVnyDQqmhPo"
                };
                jointErrorDict.AddOrUpdate("HandLeft", list, (key, oldValue) => list);
            }

            float handLineY = (skeleton.Joints[JointType.HandRight].Position.Y + skeleton.Joints[JointType.HandLeft].Position.Y) / 2;
            float handLineZ = (skeleton.Joints[JointType.HandRight].Position.Z + skeleton.Joints[JointType.HandLeft].Position.Z) / 2;
            float shoulderLineZ = (skeleton.Joints[JointType.ShoulderRight].Position.Z + skeleton.Joints[JointType.ShoulderLeft].Position.Z) / 2;

            if ((handLineY > skeleton.Joints[JointType.Head].Position.Y) && (handLineZ > shoulderLineZ + (0.1 * skeletonHeight)))
            {
                List<string> list = new List<string>
                {
                    "Bringing the bar too far back at the end.",
                    "This puts unnecessary strain on the shoulders and will cause you to lose balance. Try to keep it directly over your head."
                };
                jointErrorDict.AddOrUpdate("HandLeft", list, (key, oldValue) => list);
                jointErrorDict.AddOrUpdate("HandRight", list, (key, oldValue) => list);
            }

            if ((handLineY > skeleton.Joints[JointType.Head].Position.Y) && (handLineZ < shoulderLineZ - (0.1 * skeletonHeight)))
            {
                List<string> list = new List<string>
                {
                    "Bringing the bar too far forward at the end.",
                    "This puts unnecessary strain on the shoulders and will cause you to lose balance. Try to keep it directly over your head."
                };
                jointErrorDict.AddOrUpdate("HandLeft", list, (key, oldValue) => list);
                jointErrorDict.AddOrUpdate("HandRight", list, (key, oldValue) => list);
            }
        }
    }
}
