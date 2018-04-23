using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Collections.Concurrent;

namespace App2
{
    //Class for squat exercise which implements Exercise interface.
    class Squat : Exercise
    {
        private KinectSensor sensor;
        Skeleton[] skeletons = new Skeleton[3];
        private static bool startingPosFound;
        private static bool depthAchieved;
        private static float skeletonHeight = 0.0f;
        private static ConcurrentDictionary<string, List<string>> jointErrorDict = new ConcurrentDictionary<string, List<string>>();

        public ConcurrentDictionary<string, List<string>> GetDictionary()
        {
            return jointErrorDict;
        }

        //Displays starting position image for the user to follow.
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

        //Sets user height and if the starting position has been found, it tracks for the squat exercise.
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
                                TrackSquat(skeleton);
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
                    (Math.Abs((rightShoulder.Position.Y + rightHand.Position.Y) / 2 - Math.Abs(rightShoulder.Position.Y)) <( 0.1 * skeletonHeight)) &&
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
        internal static void TrackSquat(Skeleton skeleton)
        {
            jointErrorDict.Clear();
            //Average Knee Y Position
            float kneesY = (skeleton.Joints[JointType.KneeLeft].Position.Y + skeleton.Joints[JointType.KneeRight].Position.Y) / 2;
            float kneesZ = (skeleton.Joints[JointType.KneeLeft].Position.Z + skeleton.Joints[JointType.KneeRight].Position.Z) / 2;

            if (skeleton.Joints[JointType.AnkleLeft].Position.Y > (skeleton.Joints[JointType.FootLeft].Position.Y + (0.05 * skeletonHeight)))
            {
                List<string> list = new List<string>
                {
                    "Left ankle coming off floor",
                    "This could be a sign of tight calves, poor ankle mobility or core instability. See: https://woman.thenest.com/proper-squat-technique-heels-coming-off-floor-20267.html"
                };
                jointErrorDict.AddOrUpdate("AnkleLeft", list, (key, oldValue) => list);
            }

            if (skeleton.Joints[JointType.AnkleRight].Position.Y > (skeleton.Joints[JointType.FootRight].Position.Y + (0.05 * skeletonHeight)))
            {
                List<string> list = new List<string>
                {
                    "Right ankle coming off floor",
                    "This could be a sign of tight calves, poor ankle mobility or core instability. See: https://woman.thenest.com/proper-squat-technique-heels-coming-off-floor-20267.html"
                };
                jointErrorDict.AddOrUpdate("AnkleRight", list, (key, oldValue) => list);
            }

            if (skeleton.Joints[JointType.Head].Position.Z < (kneesZ - 0.13 * skeletonHeight))
            {
                List<string> list = new List<string>
                {
                    "Head coming too far forward.",
                    "Try keeping your chest upright and your upper back tight during the squat."
                };
                jointErrorDict.AddOrUpdate("Head", list, (key, oldValue) => list);
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

            if (skeleton.Joints[JointType.KneeLeft].Position.X - (0.03 * skeletonHeight) > (skeleton.Joints[JointType.AnkleLeft].Position.X ))
            {
                List<string> list = new List<string>
                {
                    "Left knee out of line.",
                    "Here are some exercises to help prevent this: https://barbend.com/how-to-prevent-knee-valgus/"
                };
                jointErrorDict.AddOrUpdate("KneeLeft", list, (key, oldValue) => list);
            }

            if (skeleton.Joints[JointType.KneeRight].Position.X + (0.03 * skeletonHeight) < (skeleton.Joints[JointType.AnkleRight].Position.X ))
            {
                List<string> list = new List<string>
                {
                    "Right knee out of line.",
                    "Here are some exercises to help prevent this: https://barbend.com/how-to-prevent-knee-valgus/"
                };
                jointErrorDict.AddOrUpdate("KneeRight", list, (key, oldValue) => list);
            }

            if ((skeleton.Joints[JointType.HipCenter].Position.Y - 0.2 * skeletonHeight) <= kneesY)
            {
                depthAchieved = true;
                if (jointErrorDict.ContainsKey("HipCentre"))
                {
                    List<string> ignored;
                    jointErrorDict.TryRemove("HipCentre", out ignored);
                }
            }
            else if (depthAchieved == false)
            {
                List<string> list = new List<string>
                {
                    "Didn't achieve parallel depth.",
                    "If you can't get your hips to or below your knees, consider lightening the weight until you can."
                };
                jointErrorDict.AddOrUpdate("HipCentre", list, (key, oldValue) => list);
            }

            if (CheckStartingPos(skeleton))
            {
                depthAchieved = false;
            }
        }
    }
}