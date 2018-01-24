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
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        SkeletonJointPrint(skeleton);
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
    }
}
