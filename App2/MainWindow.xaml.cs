using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Threading;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Diagnostics;
using Coding4Fun.Kinect.Wpf;
using System.Windows.Media.Imaging;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;

namespace App2
{
    /*
     * Emma Meehan - 14358201 - 4BCT 
     * April 2018
     * This class contains the logic for the main window. Contains all the buttons and redirection to the business logic
     * classes behind those buttons.
    */ 
    public partial class MainWindow : Window
    {
        private KinectSensor sensor;
        private String noKinectReady = "No Kinect connected.";
        ConcurrentDictionary<String, ColorImagePoint> dict = new ConcurrentDictionary<string, ColorImagePoint>();
        ConcurrentDictionary<String, List<String>> exerciseDict = new ConcurrentDictionary<string, List<string>>();
        private readonly AutoResetEvent _isStopping = new AutoResetEvent(false);
        bool startPosFound = false;
        ExerciseObj exerciseObj;

        private const float ClickHoldingRectThreshold = 0.05f;
        private Rect _clickHoldingLastRect;
        private readonly Stopwatch _clickHoldingTimer = new Stopwatch();

        private SpeechRecognitionEngine speechEngine;

        //Checks for Kinect sensor, activates colour stream. Populates grammar with the
        //word "finish" to trigger finish button. 
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                ColourStream colourStream = new ColourStream();
                Image.Source = colourStream.StartColourStream(sensor);
                Button0.Background = Brushes.Gray;
                this.sensor.DepthStream.Enable();
                this.sensor.SkeletonStream.Enable();

                RecognizerInfo ri = GetKinectRecognizer();
                if (null != ri)
                {
                    this.speechEngine = new SpeechRecognitionEngine(ri.Id);
                }
                var gb = new GrammarBuilder { Culture = ri.Culture };
                gb.Append("finish");
                
                var g = new Grammar(gb);
                speechEngine.LoadGrammar(g);
                speechEngine.SpeechRecognized += SpeechRecognized;

                speechEngine.SetInputToAudioStream(
                    sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));

                speechEngine.RecognizeAsync(RecognizeMode.Single);

                sensor.AllFramesReady += SensorAllFramesReady;
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = noKinectReady;
            }
        }

        //Stops sensor and threads when the window is closed. 
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
            Environment.Exit(Environment.ExitCode);
        }

        //Constantly calls SensorSkeletonFrameReady while there are frames ready. 
        void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            SensorSkeletonFrameReady(e);
        }

        //Main gesture recognition method. 
        void SensorSkeletonFrameReady(AllFramesReadyEventArgs e)
        {
            if (!startPosFound)
            {
                using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
                {
                    if (skeletonFrameData == null)
                    {
                        return;
                    }

                    var allSkeletons = new Skeleton[skeletonFrameData.SkeletonArrayLength];

                    skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                    foreach (Skeleton sd in allSkeletons)
                    {
                        // the first found/tracked skeleton moves the mouse cursor
                        if (sd.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            // make sure both hands are tracked
                            if (sd.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
                            {
                                BitmapImage image = new BitmapImage(new Uri("/Images/hand.png", UriKind.Relative));
                                CursorImage.Source = image;
                                var wristRight = sd.Joints[JointType.WristRight];
                                var scaledRightHand = wristRight.ScaleTo(1500, 1500);

                                var cursorX = (int)(scaledRightHand.Position.X + 10); 
                                var cursorY = (int)(scaledRightHand.Position.Y + 10);

                                Canvas.SetLeft(CursorImage, cursorX);
                                Canvas.SetTop(CursorImage, cursorY);

                                var leftClick = CheckForClickHold(scaledRightHand);
                                NativeMethods.SendMouseInput(cursorX, cursorY, (int)(SystemParameters.PrimaryScreenWidth), (int)(SystemParameters.PrimaryScreenHeight), leftClick);
                            }
                        }
                    }
                }
            }
        }

        //Checks if the user has their hand over a trigger for more than two seconds. 
        private bool CheckForClickHold(Joint hand)
        {
            var x = hand.Position.X;
            var y = hand.Position.Y;
            var screenwidth = 1500;
            var screenheight = 1500;
            var clickwidth = (int)(screenwidth * ClickHoldingRectThreshold);
            var clickheight = (int)(screenheight * ClickHoldingRectThreshold);

            var newClickHold = new Rect(x - clickwidth, y - clickheight, clickwidth * 2, clickheight * 2);

            if (_clickHoldingLastRect != Rect.Empty)
            {
                if (newClickHold.IntersectsWith(_clickHoldingLastRect))
                {
                    if ((int)_clickHoldingTimer.ElapsedMilliseconds > 2000)
                    {
                        _clickHoldingTimer.Stop();
                        _clickHoldingLastRect = Rect.Empty;
                        return true;
                    }

                    if (!_clickHoldingTimer.IsRunning)
                    {
                        _clickHoldingTimer.Reset();
                        _clickHoldingTimer.Start();
                    }
                    return false;
                }

                _clickHoldingTimer.Stop();
                _clickHoldingLastRect = newClickHold;
                return false;
            }

            _clickHoldingLastRect = newClickHold;
            if (!_clickHoldingTimer.IsRunning)
            {
                _clickHoldingTimer.Reset();
                _clickHoldingTimer.Start();
            }
            return false;
        }

        //Main speech recognition method. 
        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-IE".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        //Activates colour stream when this button is clicked. 
        private void ColourStreamClick(object sender, RoutedEventArgs e)
        {
            if (null == this.sensor)
            {
                this.statusBarText.Text = noKinectReady;
            }
            else
            {
                ColourStream colourStream = new ColourStream();
                Image.Source = colourStream.StartColourStream(sensor);
                Button0.Background = Brushes.Gray;
                Button1.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
                Button2.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
            }
        }

        //Activates depth stream when this button is clicked.
        private void DepthStreamClick(object sender, RoutedEventArgs e)
        {
            if (null == this.sensor)
            {
                this.statusBarText.Text = noKinectReady;
            }
            else
            {
                DepthStream depthStream = new DepthStream();
                Image.Source = depthStream.StartDepthStream(sensor);
                Button1.Background = Brushes.Gray;
                Button0.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
                Button2.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
            }
        }

        //Activates skeleton stream when this button is clicked. 
        private void SkeletonStreamClick(object sender, RoutedEventArgs e)
        {
            if (null == sensor)
            {
                this.statusBarText.Text = noKinectReady;
            }
            else
            {
                SkeletonStream skeletonStream = new SkeletonStream();
                Image.Source = skeletonStream.StartSkeletonStream(sensor);
                Button2.Background = Brushes.Gray;
                Button0.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
                Button1.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
            }
        }

        //Main thread for the system, occuring every 50 milliseconds. It handles two dictionaries simultaneously: 
        //dict and exerciseDict. dict contains the string name for the joint and it's ColorImagePoint, set in
        //SkeletonPos.cs. exerciseDict contains the string name for the joint, and whether it should be coloured
        //green or red. This is handled in the respective class for that exercise. 
        internal void GetDictionary(SkeletonPos skel, Exercise exercise)
        {
            TimeSpan waitInterval = TimeSpan.FromMilliseconds(50);
            List<String> keys = new List<String>();
            bool isEmpty;

            exerciseObj = new ExerciseObj(exercise.ToString());

            for (; !_isStopping.WaitOne(waitInterval);)
            {
                exerciseDict.Clear();
                using (var dictionaryEnum = dict.GetEnumerator())
                {
                    isEmpty = !dictionaryEnum.MoveNext();
                }

                if (isEmpty)
                {
                    foreach (KeyValuePair<String, ColorImagePoint> entry in skel.getJointPointDict())
                    {
                        dict.AddOrUpdate(entry.Key, entry.Value, (key, oldValue) => entry.Value);
                        keys.Add(entry.Key);
                    }
                    foreach (KeyValuePair<String, List<String>> entry in exercise.GetDictionary())
                    {
                        exerciseDict.AddOrUpdate(entry.Key, entry.Value, (key, oldValue) => entry.Value);
                    }
                }

                else
                {
                    foreach (var key in keys)
                    {
                        foreach (KeyValuePair<String, ColorImagePoint> realEntry in skel.getJointPointDict())
                        {
                            if (key == realEntry.Key)
                            {
                                dict[key] = realEntry.Value;
                            }
                        }
                        foreach (KeyValuePair<String, List<String>> realEntry in exercise.GetDictionary())
                        {
                            if (key == realEntry.Key)
                            {
                                exerciseDict[key] = realEntry.Value;
                            }
                        }
                    }
                }
                //If the user is tracked but the starting position isn't found, the joints are coloured in gold.
                foreach (KeyValuePair<String, ColorImagePoint> item in dict)
                    if (!exercise.CheckStartPosFound())
                    {
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                DrawDots(Brushes.Gold, item.Key, item.Value);
                            });
                        }
                    }
                    else if (exercise.CheckStartPosFound())
                    {
                        //If the exerciseDict contains that joint, it means that joint is in a bad position and is coloured red.
                        if (exerciseDict.ContainsKey(item.Key))
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                DrawDots(Brushes.Red, item.Key, item.Value);
                                exerciseObj.Add(item.Key, exerciseDict[item.Key]);
                            });
                        }
                        //Otherwise, joint is in an okay position and is coloured green as it remains so.
                        else
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                DisplayTextOnce("Starting Position Found - Go!");
                                DrawDots(Brushes.SpringGreen, item.Key, item.Value);
                            });
                        }

                    }
            }
        }

        private void DisplayText(String text)
        {
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, ea) => Thread.Sleep(TimeSpan.FromSeconds(2));
            backgroundWorker.RunWorkerCompleted += (s, ea) =>
            {
                this.animatedText.Text = "";
            };

            this.animatedText.Text = text;
            backgroundWorker.RunWorkerAsync();
        }

        private void DisplayTextOnce(String text)
        {
            if (!startPosFound)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (s, ea) => Thread.Sleep(TimeSpan.FromSeconds(2));
                backgroundWorker.RunWorkerCompleted += (s, ea) =>
                {
                    this.animatedText.Text = "";
                };

                this.animatedText.Text = text;
                backgroundWorker.RunWorkerAsync();
                startPosFound = true;
            }
        }

        //Triggered when squat button is clicked. Instantiates class for squat and begins checking. 
        //Activiates main thread with the squat exercise. 
        private void Squat(object sender, RoutedEventArgs e)
        {
            Squat squatMode = new Squat();
            SkeletonPos skeletonPos = new SkeletonPos();
            skeletonPos.StartSkeletonStream(sensor);

            DisplayText("Enter Starting Position");

            if (null == sensor)
            {
                this.statusBarText.Text = noKinectReady;
                return;
            }

            //Update dictionary
            Thread getDict = new Thread(() => GetDictionary(skeletonPos, squatMode));
            getDict.Start();

            squatMode.StartExercise(sensor);

            //Change of UI
            this.statusBarText.Text = "Squat mode activated";
            this.activityText.Text = "Please enter starting position.";
            this.DemoImage.Source = squatMode.ShowImage();
            SquatButton.Background = Brushes.Gray;
            DeadliftButton.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
            OHPButton.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
        }

        //Triggered when the overhead press button is clicked. Instantiates class for overhead press and begins checking. 
        //Activiates main thread with the overhead press exercise. 
        private void OverheadPress(object sender, RoutedEventArgs e)
        {
            OverheadPress overheadPress = new OverheadPress();
            SkeletonPos skeletonPos = new SkeletonPos();
            skeletonPos.StartSkeletonStream(sensor);

            DisplayText("Enter Starting Position");

            if (null == sensor)
            {
                this.statusBarText.Text = noKinectReady;
                return;
            }

            Thread getDict = new Thread(() => GetDictionary(skeletonPos, overheadPress));
            getDict.Start();

            overheadPress.StartExercise(sensor);

            this.statusBarText.Text = "Overhead press mode activated";
            this.activityText.Text = "Please enter starting position";
            this.DemoImage.Source = overheadPress.ShowImage();
            OHPButton.Background = Brushes.Gray; Button0.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
            SquatButton.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
            DeadliftButton.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));

        }

        //Triggered when deadlift button is clicked. Instantiates class for deadlift and begins checking. 
        //Activiates main thread with the deadlift exercise. 
        private void Deadlift(object sender, RoutedEventArgs e)
        {
            Deadlift deadlift = new Deadlift();
            SkeletonPos skeletonPos = new SkeletonPos();
            skeletonPos.StartSkeletonStream(sensor);

            DisplayText("Enter Starting Position");

            if (null == sensor)
            {
                this.statusBarText.Text = noKinectReady;
                return;
            }

            Thread getDict = new Thread(() => GetDictionary(skeletonPos, deadlift));
            getDict.Start();

            deadlift.StartExercise(sensor);

            this.statusBarText.Text = "Deadlift mode activated";
            this.activityText.Text = "Please enter starting position.";
            this.DemoImage.Source = deadlift.ShowImage();
            DeadliftButton.Background = Brushes.Gray;
            SquatButton.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
            OHPButton.Background = new SolidColorBrush(Color.FromRgb(110, 8, 178));
        }

        //Draws the dots for each joints at the particular point on screen, with the correct colour.
        private void DrawDots(Brush colour, String joint, ColorImagePoint point)
        {
            foreach (Object obj in Canvas.Children)
            {
                var ellipse = obj as Ellipse;
                if (ellipse != null && ellipse.Name.Contains(joint))
                {
                    ellipse.Fill = colour;
                    ellipse.Stroke = Brushes.Black;
                    Canvas.SetLeft(ellipse, (point.X + 50) * 2);
                    Canvas.SetTop(ellipse, point.Y * 2);
                }
            }
        }

        //Triggers finish button when speech is recognised. 
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.3;

            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                if (exerciseObj == null)
                {
                    throw new Exception("No exercise tracked");
                }
                else
                {
                    var results = new ResultsWindow(exerciseObj.GetContent());
                    results.Show();
                }
            }
        }

        //When finish button is triggered, creates results window. 
        private void Finished(object sender, RoutedEventArgs e)
        {
            if (exerciseObj == null)
            {
                throw new Exception("No exercise tracked");
            }
            else
            {
                //Open new window
                var results = new ResultsWindow(exerciseObj.GetContent());
                results.Show();
            }
        }
    }
}
