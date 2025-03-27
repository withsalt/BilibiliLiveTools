using System.Drawing;
using System.Linq;
using SkiaSharp;

namespace BilibiliAutoLiver.Utils
{
    public static class ScreenParamsHelper
    {
        public static bool TryParse(string inputScreen, out string message, out SKRect? rectangle)
        {
            message = null;
            rectangle = null;
            if (string.IsNullOrWhiteSpace(inputScreen))
            {
                message = "屏幕范围参数不能为空";
                return false;
            }
            string[] param = inputScreen.Split(',', System.StringSplitOptions.RemoveEmptyEntries);
            if (param == null || (param.Length != 4 && param.Length != 2))
            {
                message = "Path参数不正确，示例：0,0,800,800（x, y, width, height）";
                return false;
            }
            int x, y, width, height;
            int[] paramVal = param.Select(p => int.TryParse(p, out int oVal) ? oVal : -1).ToArray();
            x = paramVal[0];
            y = paramVal[1];
            if (paramVal.Length > 2)
            {
                if (paramVal[2] < 0)
                {
                    message = "width参数不正确，width坐标不能小于0，示例：0,0,800,800（x, y, width, height）";
                    return false;
                }
                width = paramVal[2];
                if (paramVal[3] < 0)
                {
                    message = "height参数不正确，height坐标不能小于0，示例：0,0,800,800（x, y, width, height）";
                    return false;
                }
                height = paramVal[3];
            }
            else
            {
                width = 0;
                height = 0;
            }
            rectangle = SKRect.Create(new SKPoint(x, y), new SKSize(width, height));
            if (rectangle.Value.Width < 0 || rectangle.Value.Height < 0)
            {
                message = "width和height参数不正确，示例：0,0,800,800（x, y, width, height）";
                return false;
            }
            return true;
        }
    }
}
