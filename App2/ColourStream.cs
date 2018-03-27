using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace App2
{
    class ColourStream
    {
        private WriteableBitmap colorBitmap;
        private byte[] colorPixels;
        
        private ColorImageFormat colorImageFormat = ColorImageFormat.Undefined;
        private const float RenderWidth = 525.0f;
        private const float RenderHeight = 350.0f;

        internal ImageSource StartColourStream(KinectSensor sensor)
        {
            // Turn on the color stream to receive color frames
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

            // Allocate space to put the pixels we'll receive
            this.colorPixels = new byte[sensor.ColorStream.FramePixelDataLength];

            // This is the bitmap we'll display on-screen
            this.colorBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

            // Add an event handler to be called whenever there is new color frame data
            sensor.ColorFrameReady += this.SensorColorFrameReady;

            // Start the sensor!
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

        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
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
