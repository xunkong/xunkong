using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xunkong.Web.Api.Models
{
    [Table("i18n")]
    [Index(nameof(zh_cn))]
    [Index(nameof(zh_tw))]
    [Index(nameof(de_de))]
    [Index(nameof(en_us))]
    [Index(nameof(es_es))]
    [Index(nameof(fr_fr))]
    [Index(nameof(id_id))]
    [Index(nameof(ja_jp))]
    [Index(nameof(ko_kr))]
    [Index(nameof(pt_pt))]
    [Index(nameof(ru_ru))]
    [Index(nameof(th_th))]
    [Index(nameof(vi_vn))]
    public class I18nModel
    {

        public long Id { get; set; }


        [MaxLength(255)]
        public string? zh_cn { get; set; }


        [MaxLength(255)]
        public string? zh_tw { get; set; }


        [MaxLength(255)]
        public string? de_de { get; set; }


        [MaxLength(255)]
        public string? en_us { get; set; }


        [MaxLength(255)]
        public string? es_es { get; set; }


        [MaxLength(255)]
        public string? fr_fr { get; set; }


        [MaxLength(255)]
        public string? id_id { get; set; }


        [MaxLength(255)]
        public string? ja_jp { get; set; }


        [MaxLength(255)]
        public string? ko_kr { get; set; }


        [MaxLength(255)]
        public string? pt_pt { get; set; }


        [MaxLength(255)]
        public string? ru_ru { get; set; }


        [MaxLength(255)]
        public string? th_th { get; set; }


        [MaxLength(255)]
        public string? vi_vn { get; set; }


    }
}

