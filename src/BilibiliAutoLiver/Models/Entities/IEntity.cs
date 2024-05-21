using System.ComponentModel;
using FreeSql.DataAnnotations;

namespace BilibiliAutoLiver.Models.Entities
{
    public interface IEntity
    {

    }


    public abstract class IEntity<TKey> : IEntity where TKey : struct
    {
        /// <summary>
        /// Id
        /// </summary>
        [Description("Id")]
        [Column(IsPrimary = true, IsIdentity = true, Position = 1)]
        public virtual TKey Id { get; set; }
    }
}
