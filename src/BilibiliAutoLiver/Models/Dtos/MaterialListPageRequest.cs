using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class MaterialListPageRequest
    {
        private string field = "id";

        private string order = "desc";

        public string FileName { get; set; }

        public FileType FileType { get; set; }

        public int Page { get; set; }

        public int Limit { get; set; }

        public string Field
        {
            get
            {
                return field;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    field = "id";
                else
                    field = value.ToLower();
            }
        }

        public string Order
        {
            get
            {
                return order;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    order = "asc";
                else
                    order = value.ToLower();
            }
        }
    }
}
