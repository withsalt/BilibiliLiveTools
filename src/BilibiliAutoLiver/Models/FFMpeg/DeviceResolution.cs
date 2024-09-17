namespace BilibiliAutoLiver.Models.FFMpeg
{
    public class DeviceResolution
    {
        public DeviceResolution(string type, string format, int width, int height)
        {
            this.Type = type;
            this.Format = format;
            this.Width = width;
            this.Height = height;
        }

        public string Type {  get; set; }

        public string Format {  get; set; }

        public int Width {  get; set; }

        public int Height {  get; set; }

        public override string ToString()
        {
            return $"{Width}x{Height}";
        }
    }
}
