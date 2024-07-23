using System.Drawing;
using System.Linq;

namespace BilibiliAutoLiver.Utils
{
    public static class ScreenParamsHelper
    {
        public static bool TryParse(string inputScreen, out string message, out Rectangle? rectangle)
        {
            message = null;
            rectangle = null;
            if (string.IsNullOrWhiteSpace(inputScreen))
            {
                message = "屏幕范围参数不能为空";
                return false;
            }
            string[] param = inputScreen.Split(',');
            if (param == null || param.Length != 4)
            {
                message = "Path参数不正确，示例：0,0,800,800（x, y, width, height）";
                return false;
            }
            int[] paramVal = param.Select(p => int.TryParse(p, out int oVal) ? oVal : -1).ToArray();
            if (paramVal[0] < 0)
            {
                message = "x参数不正确，x坐标不能小于0，示例：0,0,800,800（x, y, width, height）";
                return false;
            }
            if (paramVal[1] < 0)
            {
                message = "y参数不正确，y坐标不能小于0，示例：0,0,800,800（x, y, width, height）";
                return false;
            }
            if (paramVal[2] < 0)
            {
                message = "width参数不正确，width坐标不能小于0，示例：0,0,800,800（x, y, width, height）";
                return false;
            }
            if (paramVal[3] < 0)
            {
                message = "height参数不正确，height坐标不能小于0，示例：0,0,800,800（x, y, width, height）";
                return false;
            }
            rectangle = new Rectangle(paramVal[0], paramVal[1], paramVal[2], paramVal[3]);
            return true;
        }
    }
}
