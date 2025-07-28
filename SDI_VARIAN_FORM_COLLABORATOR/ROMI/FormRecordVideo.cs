using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace SDI_VARIAN_FORM_COLLABORATOR.ROMI
{
    public partial class FormRecordVideo: Form
    {
        private VideoCapture capture;
        private VideoWriter writer;
        private CancellationTokenSource cts;
        private Stopwatch stopwatch;
        private int targetFps = 30;
        private double frameIntervalMs;

        public FormRecordVideo()
        {
            InitializeComponent();
            frameIntervalMs = 1000.0 / targetFps;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cts = new CancellationTokenSource();
            stopwatch = new Stopwatch();
            Task.Run(() => RecordVideo(cts.Token));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
        }

        private void RecordVideo(CancellationToken token)
        {
            capture = new VideoCapture(0);
            if (!capture.IsOpened())
            {
                MessageBox.Show("Kamera tidak tersedia.");
                return;
            }

            int width = capture.FrameWidth;
            int height = capture.FrameHeight;
            writer = new VideoWriter("output.avi", FourCC.XVID, targetFps, new OpenCvSharp.Size(width, height));

            stopwatch.Start();
            long lastFrameTime = stopwatch.ElapsedMilliseconds;

            while (!token.IsCancellationRequested)
            {
                long currentTime = stopwatch.ElapsedMilliseconds;
                long elapsed = currentTime - lastFrameTime;

                if (elapsed >= frameIntervalMs)
                {
                    using (Mat frame = new Mat())
                    {
                        capture.Read(frame);
                        if (frame.Empty()) continue;

                        writer.Write(frame);

                        // Update preview UI
                        var image = BitmapConverter.ToBitmap(frame);
                        pictureBox1.Invoke(new Action(() =>
                        {
                            pictureBox1.Image?.Dispose();
                            pictureBox1.Image = image;
                        }));
                    }

                    lastFrameTime = currentTime;
                }
                else
                {
                    Thread.Sleep(1); // Small delay to reduce CPU usage
                }
            }

            stopwatch.Stop();
            capture.Release();
            writer.Release();

            MessageBox.Show("Perekaman selesai.");
        }
    }
}
