using Microsoft.Kinect;
using System;
using System.Collections.Concurrent;

namespace App2
{
    //This class is called from MainWindow.xaml.cs to abstract ColorImagePoints from the skeleton it senses. 
    //Having ColorImagePoints makes it possible to plot a dot onto a joint at the correct x,y on the main
    //image being rendered, but also allows a sense of depth to be shown. The dot will grow or shrink slightly
    //depending on how far away it is from the sensor.
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
                DepthImagePoint headDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.Head].Position);
                //left shoulder
                DepthImagePoint leftSDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.ShoulderLeft].Position);
                //right shoulder
                DepthImagePoint rightSDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.ShoulderRight].Position);
                //right elbow
                DepthImagePoint rightEDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.ElbowRight].Position);
                //left elbow
                DepthImagePoint leftEDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.ElbowLeft].Position);
                //right hand
                DepthImagePoint rightHdDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.HandRight].Position);
                //left hand
                DepthImagePoint leftHdDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.HandLeft].Position);
                //spine
                DepthImagePoint spineDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.Spine].Position);
                //right hip
                DepthImagePoint rightHpDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.HipRight].Position);
                //left hip
                DepthImagePoint leftHpDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.HipLeft].Position);
                //hip centre
                DepthImagePoint centreHpDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.HipCenter].Position);
                //right knee
                DepthImagePoint rightKDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.KneeRight].Position);
                //left knee
                DepthImagePoint leftKDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.KneeLeft].Position);
                //right ankle
                DepthImagePoint rightADepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.AnkleRight].Position);
                //left ankle
                DepthImagePoint leftADepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.AnkleLeft].Position);

                //Map a depth point to a point on the color image
                //head
                ColorImagePoint headColorPoint = depth.MapToColorImagePoint(headDepthPoint.X, headDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                //left shoulder
                ColorImagePoint leftShoulderColorPoint = depth.MapToColorImagePoint(leftSDepthPoint.X, leftSDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                //right shoulder
                ColorImagePoint rightShoulderColorPoint = depth.MapToColorImagePoint(rightSDepthPoint.X, rightSDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                //right elbow
                ColorImagePoint rightEColorPoint = depth.MapToColorImagePoint(rightEDepthPoint.X, rightEDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                //left eblow
                ColorImagePoint leftEColorPoint = depth.MapToColorImagePoint(leftEDepthPoint.X, leftEDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                //right hand
                ColorImagePoint rightHdColorPoint = depth.MapToColorImagePoint(rightHdDepthPoint.X, rightHdDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                //left hand
                ColorImagePoint leftHdColorPoint = depth.MapToColorImagePoint(leftHdDepthPoint.X, leftHdDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                //spine
                ColorImagePoint spineColorPoint = depth.MapToColorImagePoint(spineDepthPoint.X, spineDepthPoint.Y, ColorImageFormat.RawBayerResolution640x480Fps30);
                //right hip
                ColorImagePoint rightHpColorPoint = depth.MapToColorImagePoint(rightHpDepthPoint.X, rightHpDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                //left hip
                ColorImagePoint leftHpColorPoint = depth.MapToColorImagePoint(leftHpDepthPoint.X, leftHpDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                //centre hip
                ColorImagePoint centreHpColorPoint = depth.MapToColorImagePoint(centreHpDepthPoint.X, centreHpDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                //right knee
                ColorImagePoint rightKColorPoint = depth.MapToColorImagePoint(rightKDepthPoint.X, rightKDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                //left knee
                ColorImagePoint leftKColorPoint = depth.MapToColorImagePoint(leftKDepthPoint.X, leftKDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                //right ankle
                ColorImagePoint rightAColorPoint = depth.MapToColorImagePoint(rightADepthPoint.X, rightADepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                //left ankle
                ColorImagePoint leftAColorPoint = depth.MapToColorImagePoint(leftADepthPoint.X, leftADepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);

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
    }
}
