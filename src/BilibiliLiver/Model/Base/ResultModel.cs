namespace BilibiliLiver.Model.Base
{
    public class ResultModel<T> where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        public int Code { get; set; } = int.MinValue;

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public T Data { get; set; }
    }
}
