using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlConnector
{
    public class Names
    {
        [Key]      
        public string HebrewName { get; set; }
        public string EnglishName { get; set; }

    }
}
