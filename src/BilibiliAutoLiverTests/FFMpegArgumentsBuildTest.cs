using System.Reflection;
using FFMpegCore;

namespace BilibiliLiverTests
{
    [TestClass()]
    public class FFMpegArgumentsBuildTest
    {
        [TestMethod()]
        public void FFMpegArgumentsBuildTest1()
        {
            FFMpegArgumentProcessor processor = CreateFFMpegArguments()
                .OutputToUrl("rtmp://live-push.bilivideo.com/live-bvc/?streamname=live_313499407_25193107&key=91fda6bc743673533f5fa87adfde32ea&schedule=rtmp&pflag=4")
                .go;

            string text = processor.Arguments;
        
        }

        public FFMpegArguments CreateFFMpegArguments()
        {
            Type type = typeof(FFMpegArguments);
            if (type == null)
            {
                throw new InvalidOperationException("FFMpegArguments 类型未找到");
            }
            try
            {
                // 查找私有无参构造函数，BindingFlags 需包含 NonPublic 和 Instance
                ConstructorInfo constructor = type.GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    Type.EmptyTypes,
                    null);

                if (constructor == null)
                {
                    throw new InvalidOperationException("未找到私有无参构造函数。");
                }

                // 调用构造函数创建实例
                FFMpegArguments instance = (FFMpegArguments)constructor.Invoke(null);
                return instance;
            }
            catch (MissingMethodException)
            {
                throw new InvalidOperationException("FFMpegArguments 缺少无参构造函数");
            }
        }
    }
}
