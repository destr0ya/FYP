using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace App2
{
    //Deadlift implementation - similar to squat and overhead press.
    internal class Deadlift : Exercise
    {
        private KinectSensor sensor;
        Skeleton[] skeletons = new Skeleton[3];
        private static bool startingPosFound;
        private static float skeletonHeight = 0.0f;
        private static float standingHipHeight = 0.0f;
        private static ConcurrentDictionary<string, List<string>> jointErrorDict = new ConcurrentDictionary<string, List<string>>();

        public ConcurrentDictionary<string, List<string>> GetDictionary()
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

        //Includes rules that the user needs to follow in order to perform the exercise correctly. 
        //If a joint position is found to be incorrect, it adds the joint to the jointErrorDict with a list of strings. 
        //The list of strings contains the problem and also a solution. 
        internal static void TrackDeadlift(Skeleton skeleton)
        {
            jointErrorDict.Clear();

            float kneesZ = (skeleton.Joints[JointType.KneeLeft].Position.Z + skeleton.Joints[JointType.KneeRight].Position.Z) / 2;
            float hipsZ = (skeleton.Joints[JointType.HipLeft].Position.Z + skeleton.Joints[JointType.HipRight].Position.Z) / 2;

            if (skeleton.Joints[JointType.Head].Position.Z < (hipsZ - 0.2 * skeletonHeight))
            {
                List<string> list = new List<string>
                {
                    "Head coming too far forward",
                    "Your weight is most likely too fair forward. This puts too much strain on the upper back and reduces power. Try keeping the weight closer to your body."
                };
                jointErrorDict.AddOrUpdate("Head", list, (key, oldValue) => list);
            }

            Debug.WriteLine("ShoulderCentre: " + skeleton.Joints[JointType.ShoulderCenter].Position.Z + "\r\n Knees Z: " + kneesZ);
            if (skeleton.Joints[JointType.ShoulderCenter].Position.Z < (kneesZ - 0.2 * skeletonHeight))
            {
                List<string> list = new List<string>
                {
                    "Leaning too far back",
                    "This may be leading to hyperextension and squeezing of the spinal discs. Avoid by finishing without exaggerating a backwards motion."
                };
                jointErrorDict.AddOrUpdate("ShoulderLeft", list, (key, oldValue) => list);
                jointErrorDict.AddOrUpdate("ShoulderRight", list, (key, oldValue) => list);
            }

            if (!(skeleton.Joints[JointType.HipRight].Position.X >= (skeleton.Joints[JointType.ShoulderCenter].Position.X + 0.02 * skeletonHeight)))
            {
                List<string> list = new List<string>
                {
                    "Right hip out of line.",
                    "This could be a sign of posterior weakness or anterior tighetness. See: https://breakingmuscle.com/fitness/squats-and-hip-dysfunction-2-common-problems-and-how-to-fix-them"
                };
                jointErrorDict.AddOrUpdate("HipRight", list, (key, oldValue) => list);
            }

            if (!(skeleton.Joints[JointType.HipLeft].Position.X <= skeleton.Joints[JointType.ShoulderCenter].Position.X - 0.02 * skeletonHeight))
            {
                List<string> list = new List<string>
                {
                    "Left hip out of line.",
                    "This could be a sign of posterior weakness or anterior tighetness. See: https://breakingmuscle.com/fitness/squats-and-hip-dysfunction-2-common-problems-and-how-to-fix-them"
                };
                jointErrorDict.AddOrUpdate("HipLeft", list, (key, oldValue) => list);
            }

            //Vector calculation for back rounding - check if ShoulderCentre, Spine and HipCentre are colinear
            float xVector0 = (skeleton.Joints[JointType.HipCenter].Position.X - skeleton.Joints[JointType.ShoulderCenter].Position.X);
            float yVector0 = (skeleton.Joints[JointType.HipCenter].Position.Y - skeleton.Joints[JointType.ShoulderCenter].Position.Y);
            float zVector0 = (skeleton.Joints[JointType.HipCenter].Position.Z - skeleton.Joints[JointType.ShoulderCenter].Position.Z);

            float xVector1 = (skeleton.Joints[JointType.Spine].Position.X - skeleton.Joints[JointType.ShoulderCenter].Position.X);
            float yVector1 = (skeleton.Joints[JointType.Spine].Position.Y - skeleton.Joints[JointType.ShoulderCenter].Position.Y);
            float zVector1 = (skeleton.Joints[JointType.Spine].Position.Z - skeleton.Joints[JointType.ShoulderCenter].Position.Z);

            if (Math.Abs(xVector0 % xVector1) > 0.007 || Math.Abs(yVector0 % yVector1) > 0.07 || Math.Abs(zVector0 % zVector1) > 0.05)
            {
                List<string> list = new List<string>
                {
                    "Back is rounded.",
                    "This is the most crucial thing to be aware of when deadlifting. Here is a video to help fix back rounding: https://www.youtube.com/watch?v=ta6NAgDzqgw"
                };
                jointErrorDict.AddOrUpdate("Spine", list, (key, oldValue) => list);
            }

        }
    }
}