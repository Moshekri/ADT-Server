using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdtSvrCmn.Objects;
using AdtSvrCmn.Interfaces;

namespace AdtSvrCmn.Objects
{
    public class TranslationManagerData
    {
        public ApplicationConfiguration Config { get; set; }
        public INormalizer   NameNormalyzer { get; set; }
        public IApplicationLogger Logger { get; set; }
    }
}
