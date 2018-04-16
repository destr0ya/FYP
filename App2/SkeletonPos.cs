using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Collections.Concurrent;

namespace App2
{
    class SkeletonPos
    {
        private KinectSensor sensor;
        private Skeleton[] skeletons = new Skeleton[6];
        private ConcurrentDictionary<String, ColorImagePoint> jointPointDict = new ConcurrentDictionary<String, ColorImagePoint>();

        internal ConcurrentDictionary<String, ColorImagePoint> getJointPointDict()
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
                //right knee
                DepthImagePoint rightEDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.ElbowRight].Position);
                //left knee
                DepthImagePoint leftEDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.ElbowLeft].Position);
                //right hand
                DepthImagePoint rightHdDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandRight].Position);
                //left hand
                DepthImagePoint leftHdDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandLeft].Position);
                //spine
                DepthImagePoint spineDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.Spine].Position);
                //right hip
                DepthImagePoint rightHpDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HipRight].Position);
                //left hip
                DepthImagePoint leftHpDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HipLeft].Position);
                //hip centre
                DepthImagePoint centreHpDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HipCenter].Position);
                //right knee
                DepthImagePoint rightKDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.KneeRight].Position);
                //left knee
                DepthImagePoint leftKDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.KneeLeft].Position);
                //right ankle
                DepthImagePoint rightADepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.AnkleRight].Position);
                DepthImagePoint leftADepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.AnkleLeft].Position);



                //Map a depth point to a point on the color image
                //head
                ColorImagePoint headColorPoint =
                    depth.MapToColorImagePoint(headDepthPoint.X, headDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //left shoulder
                ColorImagePoint leftShoulderColorPoint =
                    depth.MapToColorImagePoint(leftSDepthPoint.X, leftSDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //right shoulder
                ColorImagePoint rightShoulderColorPoint =
                    depth.MapToColorImagePoint(rightSDepthPoint.X, rightSDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //right elbow
                ColorImagePoint rightEColorPoint =
                    depth.MapToColorImagePoint(rightEDepthPoint.X, rightEDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //left eblow
                ColorImagePoint leftEColorPoint =
                    depth.MapToColorImagePoint(leftEDepthPoint.X, leftEDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //right hand
                ColorImagePoint rightHdColorPoint =
                    depth.MapToColorImagePoint(rightHdDepthPoint.X, rightHdDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //left hand
                ColorImagePoint leftHdColorPoint =
                    depth.MapToColorImagePoint(leftHdDepthPoint.X, leftHdDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //spine
                ColorImagePoint spineColorPoint =
                    depth.MapToColorImagePoint(spineDepthPoint.X, spineDepthPoint.Y, 
                    ColorImageFormat.RawBayerResolution640x480Fps30);
                //right hip
                ColorImagePoint rightHpColorPoint =
                    depth.MapToColorImagePoint(rightHpDepthPoint.X, rightHpDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //left hip
                ColorImagePoint leftHpColorPoint =
                    depth.MapToColorImagePoint(leftHpDepthPoint.X, leftHpDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //centre hip
                ColorImagePoint centreHpColorPoint =
                    depth.MapToColorImagePoint(centreHpDepthPoint.X, centreHpDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //right knee
                ColorImagePoint rightKColorPoint =
                    depth.MapToColorImagePoint(rightKDepthPoint.X, rightKDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //left knee
                ColorImagePoint leftKColorPoint =
                    depth.MapToColorImagePoint(leftKDepthPoint.X, leftKDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //right ankle
                ColorImagePoint rightAColorPoint =
                    depth.MapToColorImagePoint(rightADepthPoint.X, rightADepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //left ankle
                ColorImagePoint leftAColorPoint =
                    depth.MapToColorImagePoint(leftADepthPoint.X, leftADepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                //Set Points
                jointPointDict.AddOrUpdate("Head", headColorPoint, (key, oldValue) => headColorPoint);
                jointPointDict.AddOrUpdate("ShoulderLeft", leftShoulderColorPoint, (key, oldValue) => leftShoulderColorPoint);
                jointPointDict.AddOrUpdate("ShoulderRight", rightShoulderColorPoint, (key, oldValue) => rightShoulderColorPoint);
                jointPointDict.AddOrUpdate("ElbowRight", rightEColorPoint, (key, oldValue) => rightEColorPoint);
                jointPointDict.AddOrUpdate("ElbowLeft", leftEColorPoint, (key, oldValue) => leftEColorPoint);
                jointPointDict.AddOrUpdate("HandRight", rightHdColorPoint, (key, oldValue) => rightHdColorPoint);
                jointPointDict.AddOrUpdate("HandLeft", leftHdColorPoint, (key, oldValue) => leftHdColorPoint);
                jointPointDict.AddOrUpdate("Spine", spineColorPoint, (key, oldValue) => spineColorPoint);
                jointPointDict.AddOrUpdate("HipRight", rightHpColorPoint, (key, oldValue) => rightHpColorPoint);
                jointPointDict.AddOrUpdate("HipLeft", leftHpColorPoint, (key, oldValue) => leftHpColorPoint);
                jointPointDict.AddOrUpdate("HipCentre", centreHpColorPoint, (key, oldValue) => centreHpColorPoint);
                jointPointDict.AddOrUpdate("KneeRight", rightKColorPoint, (key, oldValue) => rightKColorPoint);
                jointPointDict.AddOrUpdate("KneeLeft", leftKColorPoint, (key, oldValue) => leftKColorPoint);
                jointPointDict.AddOrUpdate("AnkleRight", rightAColorPoint, (key, oldValue) => rightAColorPoint);
                jointPointDict.AddOrUpdate("AnkleLeft", leftAColorPoint, (key, oldValue) => leftAColorPoint);
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
