using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App2
{
    class SkeletonPos
    {
        private KinectSensor sensor;
        private static Skeleton skeleton;

        internal void StartSkeletonStream(KinectSensor sensor1)
        {
            this.sensor = sensor1;
            sensor.SkeletonStream.Enable();
            sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
            sensor.Start();
        }

        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[3];
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
                            //SkeletonJointPrint(skeleton);
                            //CheckStandingPos(skeleton);
                            Squat.TrackSquat(skeleton);
                        }
                    }
                }
            }
        }

        //Used for testing purposes only
        private void SkeletonJointPrint(Skeleton skeleton)
        {
            foreach (Joint joint in skeleton.Joints)
                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    Debug.WriteLine(joint.JointType.ToString() + " position: " + joint.Position.X.ToString() + ", " + joint.Position.Y.ToString() + ", " + joint.Position.Z.ToString());
                }
                else
                {
                    Debug.WriteLine(joint.JointType.ToString() + "  inferred position: " + joint.Position.X.ToString() + ", " + joint.Position.Y.ToString() + ", " + joint.Position.Z.ToString());
                }
        }

        private void CheckStandingPos(Skeleton skeleton1)
        {
            //Checking if the skeleton is in a standing position, with head in line with neck, spine and bum,
            //with shoulder in line with elbow, wrist and hand and hip in line with knee, ankle and foot.

            Joint head = skeleton.Joints[JointType.Head];
            Joint neck = skeleton.Joints[JointType.ShoulderCenter];
            Joint rightShoulder = skeleton.Joints[JointType.ShoulderRight];
            Joint leftShoulder = skeleton.Joints[JointType.ShoulderLeft];
            Joint rightElbow = skeleton.Joints[JointType.ElbowRight];
            Joint leftElbow = skeleton.Joints[JointType.ElbowLeft];
            Joint rightHand = skeleton.Joints[JointType.HandRight];
            Joint leftHand = skeleton.Joints[JointType.HandLeft];
            Joint spine = skeleton.Joints[JointType.Spine];
            Joint waist = skeleton.Joints[JointType.HipCenter];
            Joint leftHip = skeleton.Joints[JointType.HipLeft];
            Joint rightHip = skeleton.Joints[JointType.HipRight];
            Joint leftKnee = skeleton.Joints[JointType.KneeLeft];
            Joint rightKnee = skeleton.Joints[JointType.KneeRight];
            Joint leftAnkle = skeleton.Joints[JointType.AnkleLeft];
            Joint rightAnkle = skeleton.Joints[JointType.AnkleRight];

            skeleton = skeleton1;

            if (waist.TrackingState == JointTrackingState.NotTracked) return;

            if (waist.Position.Z < 2.0f || waist.Position.Z > 5.0f)
            {
                return;
            }
            else
            {
                Debug.WriteLine("Upper X Margin: " + Math.Abs((head.Position.X + neck.Position.X + spine.Position.X + waist.Position.X) / 4 - Math.Abs(waist.Position.X)));
                Debug.WriteLine("Upper Z Margin: " + Math.Abs((head.Position.Z + neck.Position.Z + spine.Position.Z + waist.Position.Z) / 4 - Math.Abs(waist.Position.Z)));
                Debug.WriteLine("Left Arm X Margin: " + Math.Abs((leftShoulder.Position.X + leftElbow.Position.X + leftHand.Position.X) / 3 - Math.Abs(leftShoulder.Position.X)));
                Debug.WriteLine("Left Arm Z Margin: " + Math.Abs((leftShoulder.Position.Z + leftElbow.Position.Z + leftHand.Position.Z) / 3 - Math.Abs(leftShoulder.Position.Z)));
                Debug.WriteLine("Right Arm X Margin: " + Math.Abs((rightShoulder.Position.X + rightElbow.Position.X + rightHand.Position.X) / 3 - Math.Abs(rightShoulder.Position.X)));
                Debug.WriteLine("Right Arm Z Margin: " + Math.Abs((rightShoulder.Position.Z + rightElbow.Position.Z + rightHand.Position.Z) / 3 - Math.Abs(rightShoulder.Position.Z)));
                Debug.WriteLine("Right Leg X Margin: " + Math.Abs((rightHip.Position.X + rightKnee.Position.X + rightAnkle.Position.X) / 3 - Math.Abs(rightHip.Position.X)));
                Debug.WriteLine("Left Leg X Margin: " + Math.Abs((leftHip.Position.X + leftKnee.Position.X + leftAnkle.Position.X) / 3 - Math.Abs(leftHip.Position.X)));

                if ((Math.Abs((head.Position.X + neck.Position.X + spine.Position.X + waist.Position.X) / 4 - Math.Abs(waist.Position.X)) < 0.1f) &&
                    (Math.Abs((head.Position.Z + neck.Position.Z + spine.Position.Z + waist.Position.Z) / 4 - Math.Abs(waist.Position.Z)) < 0.1f) &&
                    (Math.Abs((leftShoulder.Position.X + leftElbow.Position.X + leftHand.Position.X) / 3 - Math.Abs(leftShoulder.Position.X)) < 0.1f) &&
                    (Math.Abs((leftShoulder.Position.Z + leftElbow.Position.Z + leftHand.Position.Z) / 3 - Math.Abs(leftShoulder.Position.Z)) < 0.1f) &&
                    (Math.Abs((rightShoulder.Position.X + rightElbow.Position.X + rightHand.Position.X) / 3 - Math.Abs(rightShoulder.Position.X)) < 0.1f) &&
                    (Math.Abs((rightShoulder.Position.Z + rightElbow.Position.Z + rightHand.Position.Z) / 3 - Math.Abs(rightShoulder.Position.Z)) < 0.1f) &&
                    (Math.Abs((rightHip.Position.X + rightKnee.Position.X + rightAnkle.Position.X) / 3 - Math.Abs(rightHip.Position.X)) < 0.1f) &&
                    (Math.Abs((leftHip.Position.X + leftKnee.Position.X + leftAnkle.Position.X) / 3 - Math.Abs(leftHip.Position.X)) < 0.1f))
                {
                    Squat.TrackSquat(skeleton);
                }
            }

        }
    }
}
