using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;

namespace App2
{
    class SkeletonPos
    {
        private KinectSensor sensor;
        private Skeleton[] skeletons = new Skeleton[6];
        private Dictionary<String, ColorImagePoint> jointPointDict = new Dictionary<String, ColorImagePoint>();

        internal Dictionary<String, ColorImagePoint> getJointPointDict()
        {
            return jointPointDict;
        }

        internal void StartSkeletonStream(KinectSensor sensor1)
        {
            this.sensor = sensor1;
            sensor.SkeletonStream.Enable();
            sensor.DepthStream.Enable();
            sensor.AllFramesReady += this.AllFramesReady;
            sensor.Start();
        }

        private void AllFramesReady(object sender, AllFramesReadyEventArgs e)
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
                            GetCameraPoint(skeleton, e);
                        }
                    }
                }
            }
        }

        void GetCameraPoint(Skeleton first, AllFramesReadyEventArgs e)
        {
            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null)
                {
                    return;
                }

                //Map a joint location to a point on the depth map
                //head
                DepthImagePoint headDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.Head].Position);
                //left shoulder
                DepthImagePoint leftSDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.ShoulderLeft].Position);
                //right shoulder
                DepthImagePoint rightSDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.ShoulderRight].Position);


                //Map a depth point to a point on the color image
                //head
                ColorImagePoint headColorPoint =
                    depth.MapToColorImagePoint(headDepthPoint.X, headDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //left hand
                ColorImagePoint leftShoulderColorPoint =
                    depth.MapToColorImagePoint(leftSDepthPoint.X, leftSDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //right hand
                ColorImagePoint rightShoulderColorPoint =
                    depth.MapToColorImagePoint(rightSDepthPoint.X, rightSDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                //Set Points
                if (jointPointDict.ContainsKey("Head")) {
                    jointPointDict.Remove("Head");
                    jointPointDict.Add("Head", headColorPoint);
                } else
                {
                    jointPointDict.Add("Head", headColorPoint);
                }
                if (jointPointDict.ContainsKey("ShoulderLeft"))
                {
                    jointPointDict.Remove("ShoulderLeft");
                    jointPointDict.Add("ShoulderLeft", leftShoulderColorPoint);
                } else
                {
                    jointPointDict.Add("ShoulderLeft", leftShoulderColorPoint);
                }
                if (jointPointDict.ContainsKey("ShoulderRight"))
                {
                    jointPointDict.Remove("ShoulderRight");
                    jointPointDict.Add("ShoulderRight", rightShoulderColorPoint);
                } else
                {
                    jointPointDict.Add("ShoulderRight", rightShoulderColorPoint);
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

        //private void CheckStandingPos(Skeleton skeleton1)
        //{
            //Checking if the skeleton is in a standing position, with head in line with neck, spine and bum,
            //with shoulder in line with elbow, wrist and hand and hip in line with knee, ankle and foot.

         //   Joint head = skeleton.Joints[JointType.Head];
         //   Joint neck = skeleton.Joints[JointType.ShoulderCenter];
         //   Joint rightShoulder = skeleton.Joints[JointType.ShoulderRight];
         //   Joint leftShoulder = skeleton.Joints[JointType.ShoulderLeft];
         //   Joint rightElbow = skeleton.Joints[JointType.ElbowRight];
         //   Joint leftElbow = skeleton.Joints[JointType.ElbowLeft];
         //   Joint rightHand = skeleton.Joints[JointType.HandRight];
         //   Joint leftHand = skeleton.Joints[JointType.HandLeft];
         //   Joint spine = skeleton.Joints[JointType.Spine];
         //   Joint waist = skeleton.Joints[JointType.HipCenter];
         //   Joint leftHip = skeleton.Joints[JointType.HipLeft];
         //   Joint rightHip = skeleton.Joints[JointType.HipRight];
         //   Joint leftKnee = skeleton.Joints[JointType.KneeLeft];
         //   Joint rightKnee = skeleton.Joints[JointType.KneeRight];
         //   Joint leftAnkle = skeleton.Joints[JointType.AnkleLeft];
         //   Joint rightAnkle = skeleton.Joints[JointType.AnkleRight];

         //   skeleton = skeleton1;

         //   if (waist.TrackingState == JointTrackingState.NotTracked) return;

         //   if (waist.Position.Z < 2.0f || waist.Position.Z > 5.0f)
         //   {
         //       return;
         //   }
         //   else
          //  {
          //      Debug.WriteLine("Upper X Margin: " + Math.Abs((head.Position.X + neck.Position.X + spine.Position.X + waist.Position.X) / 4 - Math.Abs(waist.Position.X)));
           //     Debug.WriteLine("Upper Z Margin: " + Math.Abs((head.Position.Z + neck.Position.Z + spine.Position.Z + waist.Position.Z) / 4 - Math.Abs(waist.Position.Z)));
             //   Debug.WriteLine("Left Arm X Margin: " + Math.Abs((leftShoulder.Position.X + leftElbow.Position.X + leftHand.Position.X) / 3 - Math.Abs(leftShoulder.Position.X)));
              //  Debug.WriteLine("Left Arm Z Margin: " + Math.Abs((leftShoulder.Position.Z + leftElbow.Position.Z + leftHand.Position.Z) / 3 - Math.Abs(leftShoulder.Position.Z)));
            //    Debug.WriteLine("Right Arm X Margin: " + Math.Abs((rightShoulder.Position.X + rightElbow.Position.X + rightHand.Position.X) / 3 - Math.Abs(rightShoulder.Position.X)));
            //    Debug.WriteLine("Right Arm Z Margin: " + Math.Abs((rightShoulder.Position.Z + rightElbow.Position.Z + rightHand.Position.Z) / 3 - Math.Abs(rightShoulder.Position.Z)));
            //    Debug.WriteLine("Right Leg X Margin: " + Math.Abs((rightHip.Position.X + rightKnee.Position.X + rightAnkle.Position.X) / 3 - Math.Abs(rightHip.Position.X)));
            //    Debug.WriteLine("Left Leg X Margin: " + Math.Abs((leftHip.Position.X + leftKnee.Position.X + leftAnkle.Position.X) / 3 - Math.Abs(leftHip.Position.X)));

//                if ((Math.Abs((head.Position.X + neck.Position.X + spine.Position.X + waist.Position.X) / 4 - Math.Abs(waist.Position.X)) < 0.1f) &&
    //                (Math.Abs((head.Position.Z + neck.Position.Z + spine.Position.Z + waist.Position.Z) / 4 - Math.Abs(waist.Position.Z)) < 0.1f) &&
  //                  (Math.Abs((leftShoulder.Position.X + leftElbow.Position.X + leftHand.Position.X) / 3 - Math.Abs(leftShoulder.Position.X)) < 0.1f) &&
      //              (Math.Abs((leftShoulder.Position.Z + leftElbow.Position.Z + leftHand.Position.Z) / 3 - Math.Abs(leftShoulder.Position.Z)) < 0.1f) &&
          //          (Math.Abs((rightShoulder.Position.X + rightElbow.Position.X + rightHand.Position.X) / 3 - Math.Abs(rightShoulder.Position.X)) < 0.1f) &&
        //            (Math.Abs((rightShoulder.Position.Z + rightElbow.Position.Z + rightHand.Position.Z) / 3 - Math.Abs(rightShoulder.Position.Z)) < 0.1f) &&
            //        (Math.Abs((rightHip.Position.X + rightKnee.Position.X + rightAnkle.Position.X) / 3 - Math.Abs(rightHip.Position.X)) < 0.1f) &&
              //      (Math.Abs((leftHip.Position.X + leftKnee.Position.X + leftAnkle.Position.X) / 3 - Math.Abs(leftHip.Position.X)) < 0.1f))
                //{
                  //  Squat.TrackSquat(skeleton);
                //}
            //}

        //}
    }
}
