﻿using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace App2
{
    class OverheadPress : Exercise
    {
        private KinectSensor sensor;
        Skeleton[] skeletons = new Skeleton[3];
        private static bool startingPosFound;
        private static float skeletonHeight = 0.0f;
        private static Dictionary<String, String> jointErrorDict = new Dictionary<String, String>();
        private static float previousLeftKneeZ = 0.0f;
        private static float previousRightKneeZ = 0.0f;
        private static float previousRightKneeY = 0.0f;
        private static float previousLeftKneeY = 0.0f;

        public Dictionary<String, String> GetDictionary()
        {
            return jointErrorDict;
        }

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

        public void StartSkeletonStream(KinectSensor sensor1)
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

        public float SetSkeletonHeight(Skeleton skeleton)
        {
            float footAverageY = (skeleton.Joints[JointType.FootRight].Position.Y + skeleton.Joints[JointType.FootLeft].Position.Y) / 2;
            return skeleton.Joints[JointType.Head].Position.Y - footAverageY + 0.1f;
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
            if (previousLeftKneeY > currentLeftKneeY + 0.1 * skeletonHeight || previousLeftKneeY < currentLeftKneeY - 0.1 * skeletonHeight || 
                previousLeftKneeZ > currentLeftKneeZ + 0.1 * skeletonHeight || previousLeftKneeZ < currentLeftKneeZ - 0.1 * skeletonHeight)
            {
                if (jointErrorDict.ContainsKey("KneeLeft"))
                {
                    jointErrorDict.Remove("KneeLeft");
                    jointErrorDict.Add("KneeLeft", "Knees should not move during overhead press.");
                }
                else
                {
                    jointErrorDict.Add("KneeLeft", "Knees should not move during overhead press.");
                }
            }

            if (previousRightKneeY > currentRightKneeY + 0.05 * skeletonHeight || previousRightKneeY < currentRightKneeY - 0.05 * skeletonHeight || 
                previousRightKneeZ > currentRightKneeZ + 0.05 * skeletonHeight || previousRightKneeZ < currentRightKneeZ - 0.05 * skeletonHeight)
            {
                if (jointErrorDict.ContainsKey("KneeRight"))
                {
                    jointErrorDict.Remove("KneeRight");
                    jointErrorDict.Add("KneeRight", "Knees should not move during overhead press.");
                }
                else
                {
                    jointErrorDict.Add("KneeRight", "Knees should not move during overhead press.");
                }
            }

            //Update previous knee position store
            previousLeftKneeY = currentLeftKneeY;
            previousLeftKneeZ = currentLeftKneeZ;
            previousRightKneeY = currentRightKneeY;
            previousRightKneeZ = currentRightKneeZ;

            //Elbows Correction
            if (skeleton.Joints[JointType.ElbowLeft].Position.Z < (skeleton.Joints[JointType.ShoulderLeft].Position.Z - 0.1 * skeletonHeight) ||
                skeleton.Joints[JointType.ElbowLeft].Position.X > (skeleton.Joints[JointType.HandLeft].Position.X + 0.1 * skeletonHeight))
            {
                if (jointErrorDict.ContainsKey("ElbowLeft"))
                {
                    jointErrorDict.Remove("ElbowLeft");
                    jointErrorDict.Add("ElbowLeft", "Elbows should not flare too far forward or out.");
                }
                else
                {
                    jointErrorDict.Add("ElbowLeft", "Elbows should not flare too far forward or out.");
                }
            }

            if (skeleton.Joints[JointType.ElbowRight].Position.Z < (skeleton.Joints[JointType.ShoulderRight].Position.Z - 0.1 * skeletonHeight) ||
                skeleton.Joints[JointType.ElbowRight].Position.X > (skeleton.Joints[JointType.HandRight].Position.X + 0.1 * skeletonHeight))
            {
                if (jointErrorDict.ContainsKey("ElbowRight"))
                {
                    jointErrorDict.Remove("ElbowRight");
                    jointErrorDict.Add("ElbowRight", "Elbows should not flare too far forward or out.");
                }
                else
                {
                    jointErrorDict.Add("ElbowRight", "Elbows should not flare too far forward or out.");
                }
            }

            //Hand Correction
            if (skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.HandLeft].Position.Y)
            {
                if (jointErrorDict.ContainsKey("HandRight"))
                {
                    jointErrorDict.Remove("HandRight");
                    jointErrorDict.Add("HandRight", "Right hand is slower rising than left hand.");
                }
                else
                {
                    jointErrorDict.Add("HandRight", "Right hand is slower rising than left hand.");
                }
            }

            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HandLeft].Position.Y)
            {
                if (jointErrorDict.ContainsKey("HandLeft"))
                {
                    jointErrorDict.Remove("HandLeft");
                    jointErrorDict.Add("HandLeft", "Left hand is slower rising than right hand");
                }
                else
                {
                    jointErrorDict.Add("HandLeft", "Left hand is slower rising than right hand.");
                }
            }

            float handLineZ = (skeleton.Joints[JointType.HandRight].Position.Z + skeleton.Joints[JointType.HandLeft].Position.Z) / 2;
            float shoulderLineZ = (skeleton.Joints[JointType.ShoulderRight].Position.Z + skeleton.Joints[JointType.ShoulderLeft].Position.Z) / 2;

            if (handLineZ > shoulderLineZ - (0.05 * skeletonHeight))
            {
                if (jointErrorDict.ContainsKey("HandLeft"))
                {
                    jointErrorDict.Remove("HandLeft");
                    jointErrorDict.Add("HandLeft", "Bringing the bar too far back at the end.");
                }
                else
                {
                    jointErrorDict.Add("HandLeft", "Bringing the bar too far back at the end.");
                }
                if (jointErrorDict.ContainsKey("HandRight"))
                {
                    jointErrorDict.Remove("HandRight");
                    jointErrorDict.Add("HandRight", "Bringing the bar too far back at the end.");
                }
                else
                {
                    jointErrorDict.Add("HandRight", "Bringing the bar too far back at the end.");
                }
            }

            if (handLineZ < shoulderLineZ - (0.05 * skeletonHeight))
            {
                if (jointErrorDict.ContainsKey("HandLeft"))
                {
                    jointErrorDict.Remove("HandLeft");
                    jointErrorDict.Add("HandLeft", "Bringing the bar too far forward at the end.");
                }
                else
                {
                    jointErrorDict.Add("HandLeft", "Bringing the bar too far forward at the end.");
                }
                if (jointErrorDict.ContainsKey("HandRight"))
                {
                    jointErrorDict.Remove("HandRight");
                    jointErrorDict.Add("HandRight", "Bringing the bar too far forward at the end.");
                }
                else
                {
                    jointErrorDict.Add("HandRight", "Bringing the bar too far forward at the end.");
                }
            }
        }
    }
}