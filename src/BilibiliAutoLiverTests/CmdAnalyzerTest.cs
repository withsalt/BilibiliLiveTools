using System.Drawing;
using BilibiliAutoLiver.Utils;
using FlashCap;

namespace BilibiliLiverTests
{
    [TestClass()]
    public class CmdAnalyzerTest
    {
        [TestMethod()]
        public async Task Test()
        {
            List<string> cmds = new List<string>()
            {
                "ffmpeg -i ~/ -c copy -bsf:v h264_mp4toannexb -f mpegts 1.ts {URL}",
                "ffmpeg -i ~/ ~/test.mp4 -c copy -bsf:v h264_mp4toannexb -f mpegts 1.ts {URL}",
                "ffmpeg -i ~/test.mp4 -c copy -bsf:v h264_mp4toannexb -f mpegts 1.ts {URL}",
                "ffmpeg -i ~/test2.mp4 -i ~/test3.mp4 -i \"~/test4.mp4\" -c copy -bsf:v h264_mp4toannexb -f mpegts 2.ts {URL}",
                "ffmpeg -i \"concat:~/1.ts|~/2.ts\" -c copy -bsf:a aac_adtstoasc -movflags +faststart ts.mp4 {URL}",
                "ffmpeg -i video=~/test.mp4 -c copy -bsf:v h264_mp4toannexb -f mpegts 1.ts {URL}"
            };

            foreach (string cmd in cmds)
            {
                if (CmdAnalyzer.TryParse(cmd, false, "D:\\data\\", null, out var message, out var ffmpegCmd, out string cmdName, out string cmdArg))
                {
                    Console.WriteLine(cmd);
                    Console.WriteLine(ffmpegCmd);
                    Console.WriteLine();
                }
            }
        }

        [TestMethod()]
        public void ScreenParamsHelperTest()
        {
            List<CaptureDeviceDescriptor> devices = new CaptureDevices()?.EnumerateDescriptors().ToList();
            if (devices?.Any() != true)
            {
                throw new Exception($"找不到视频输入设备！");
            }
        }
    }
}
