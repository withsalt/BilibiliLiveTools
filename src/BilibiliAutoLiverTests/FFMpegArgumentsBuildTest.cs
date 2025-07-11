using System.Reflection;
using FFMpegCore;

namespace BilibiliLiverTests
{
    [TestClass()]
    public class FFMpegArgumentsBuildTest
    {

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
