using Emgu.CV;
using Emgu.CV.Util;

namespace Diffusion.Video;

public class FrameExtractor
{
    public static Stream ExtractFrameToPNG(string path)
    {
        using (var capture = new VideoCapture(path)) 
        {
            int frameIndex = 0;

            //double totalFrames = capture.Get(CapProp.FrameCount);
            //int middleFrameIndex = (int)(totalFrames / 2);

            //capture.Set(CapProp.PosFrames, middleFrameIndex);

            Mat middleFrame = new Mat();

            capture.Read(middleFrame);

            var buffer = new VectorOfByte();

            // Encode as PNG into buffer
            CvInvoke.Imencode(".png", middleFrame, buffer);

            // Copy buffer to MemoryStream
            return new MemoryStream(buffer.ToArray(), writable: false);
        }
    }
}