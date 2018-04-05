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
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace App2
{
    class Squat
    {
        private KinectSensor sensor;
        Skeleton[] skeletons = new Skeleton[3];
        private static bool startingPosFound;
        private static bool depthAchieved;
        private static float skeletonHeight = 0.0f;
        private static Dictionary<String, String> jointErrorDict = new Dictionary<String, String>();

        internal Dictionary<String, String> getSquatJointDict()
        {
            return jointErrorDict;
        }

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
                    (Math.Abs((rightShoulder.Position.Y + rightHand.Position.Y) / 2 - Math.Abs(rightShoulder.Position.Y)) <( 0.1 * skeletonHeight)) &&
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

        internal static void TrackSquat(Skeleton skeleton)
        {
            jointErrorDict.Clear();
            //Average Knee Y Position
            float kneesY = (skeleton.Joints[JointType.KneeLeft].Position.Y + skeleton.Joints[JointType.KneeRight].Position.Y) / 2;
            float kneesZ = (skeleton.Joints[JointType.KneeLeft].Position.Z + skeleton.Joints[JointType.KneeRight].Position.Z) / 2;

            if (skeleton.Joints[JointType.AnkleLeft].Position.Y > (skeleton.Joints[JointType.FootLeft].Position.Y + (0.02 * skeletonHeight)))
            {
                if (jointErrorDict.ContainsKey("AnkleLeft"))
                {
                    jointErrorDict.Remove("AnkleLeft");
                    jointErrorDict.Add("AnkleLeft", "Left heel coming off floor.");
                }
                else
                {
                    jointErrorDict.Add("AnkleLeft", "Left heel coming off floor.");
                }
            }

            if (skeleton.Joints[JointType.AnkleRight].Position.Y > (skeleton.Joints[JointType.FootRight].Position.Y + (0.02 * skeletonHeight)));
            {
                if (jointErrorDict.ContainsKey("AnkleRight"))
                {
                    jointErrorDict.Remove("AnkleRight");
                    jointErrorDict.Add("AnkleRight", "Right heel coming off floor.");
                }
                else
                {
                    jointErrorDict.Add("AnkleRight", "Right heel coming off floor.");
                }
            }

            if (skeleton.Joints[JointType.Head].Position.Z < kneesZ)
            {
                if (jointErrorDict.ContainsKey("Head"))
                {
                    jointErrorDict.Remove("Head");
                    jointErrorDict.Add("Head", "Head coming too far forward.");
                }
                else
                {
                    jointErrorDict.Add("Head", "Head coming too far forward");
                }
            }

            if (skeleton.Joints[JointType.FootLeft].Position.X - Math.Abs(skeleton.Joints[JointType.HipCenter].Position.X) - 
                skeleton.Joints[JointType.FootRight].Position.X - Math.Abs(skeleton.Joints[JointType.HipCenter].Position.X) > (0.05 * skeletonHeight))
            {
                if (jointErrorDict.ContainsKey("HipCentre"))
                {
                    jointErrorDict.Remove("HipCentre");
                    jointErrorDict.Add("HipCentre", "Hips off-centre");
                }
                else
                {
                    jointErrorDict.Add("HipCentre", "Hips off-centre");
                }
            }

            if (skeleton.Joints[JointType.KneeLeft].Position.X < skeleton.Joints[JointType.AnkleLeft].Position.X - (0.02 * skeletonHeight))
            {
                if (jointErrorDict.ContainsKey("KneeLeft"))
                {
                    jointErrorDict.Remove("KneeLeft");
                    jointErrorDict.Add("KneeLeft", "Left knee off line");
                }
                else
                {
                    jointErrorDict.Add("KneeLeft", "Left knee off line");
                }
            }

            if (skeleton.Joints[JointType.KneeRight].Position.X > skeleton.Joints[JointType.AnkleRight].Position.X - (0.02 * skeletonHeight))
            {
                if (jointErrorDict.ContainsKey("KneeRight"))
                {
                    jointErrorDict.Remove("KneeRight");
                    jointErrorDict.Add("KneeRight", "Right knee off line");
                }
                else
                {
                    jointErrorDict.Add("KneeRight", "Right knee off line");
                }
            }

            if (skeleton.Joints[JointType.HipCenter].Position.Y <= kneesY)
            {
                depthAchieved = true;
            }
            else if (CheckStartingPos(skeleton) && depthAchieved == false)
            {
                if (jointErrorDict.ContainsKey("HipCentre"))
                {
                    jointErrorDict.Remove("HipCentre");
                    jointErrorDict.Add("HipCentre", "Didn't achieve parallel depth");
                }
                else
                {
                    jointErrorDict.Add("HipCentre", "Didn't achieve parallel depth");
                }
            }
        }
    }
}