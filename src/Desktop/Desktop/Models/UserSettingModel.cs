using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Desktop.Models
{

    [Table("UserSettings")]
    internal class UserSettingModel
    {

        [Key]
        public string Key { get; set; }


        public string? Value { get; set; }


    }
}
