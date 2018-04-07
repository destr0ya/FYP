using Microsoft.Kinect;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace App2
{
    interface Exercise
    {
        ConcurrentDictionary<String, String> GetDictionary();
        ImageSource ShowImage();

        void StartExercise(KinectSensor sensor);

        void StartSkeletonStream(KinectSensor sensor);

        float SetSkeletonHeight(Skeleton skeleton);

        bool CheckStartPosFound();
    }
}
