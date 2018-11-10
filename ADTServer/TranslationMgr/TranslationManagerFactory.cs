using AdtSvrCmn.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranslationManager;

namespace TranslationManager
{
    public static class TranslationManagerFactory
    {
        public static TranslationManger GetTranslationManager(TranslationManagerData data)
        {
            TranslationManger manager = new TranslationManger(data);
            return manager;
        }
    }
}
