using Microsoft.Kinect;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Media;

namespace App2
{
    // Interface for exercise, to be implemented by squat, overhead press and deadlift. 
    // Allows for the main thread to call an exercise without worrying which one it is.
    interface Exercise
    {
        ConcurrentDictionary<String, List<String>> GetDictionary();
        ImageSource ShowImage();

        void StartExercise(KinectSensor sensor);

        void StartSkeletonStream(KinectSensor sensor);

        float SetSkeletonHeight(Skeleton skeleton);

        bool CheckStartPosFound();
    }
}
