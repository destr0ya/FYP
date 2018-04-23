using Microsoft.Kinect;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace App2
{
    //Displays DepthStream to screen when button is clicked. 
    //Creates an intensity level based on how far away the point is. 
    class DepthStream
    {
        private DepthImagePixel[] depthPixels;
        private byte[] colorPixels;
        private WriteableBitmap colorBitmap;

        internal ImageSource StartDepthStream(KinectSensor sensor)
        {
            sensor.ColorStream.Disable();
            sensor.SkeletonStream.Disable();
            sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

            this.depthPixels = new DepthImagePixel[sensor.DepthStream.FramePixelDataLength];
            this.colorPixels = new byte[sensor.DepthStream.FramePixelDataLength * sizeof(int)];
            this.colorBitmap = new WriteableBitmap(sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

            sensor.DepthFrameReady += this.SensorDepthFrameReady;

            try
            {
                sensor.Start();
                return this.colorBitmap;
            }
            catch (IOException)
            {
                sensor = null;
                return null;
            }
        }

        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                    // Get the min and max reliable depth for the current frame
                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;

                    // Convert the depth to RGB
                    int colorPixelIndex = 0;
                    for (int i = 0; i < this.depthPixels.Length; ++i)
                    {
                        // Get the depth for this pixel
                        short depth = depthPixels[i].Depth;
                        byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                        // Write out blue byte
                        this.colorPixels[colorPixelIndex++] = intensity;

                        // Write out green byte
                        this.colorPixels[colorPixelIndex++] = intensity;

                        // Write out red byte                        
                        this.colorPixels[colorPixelIndex++] = intensity;

                        ++colorPixelIndex;
                    }

                    // Write the pixel data into the bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }
    }
}
