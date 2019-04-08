﻿using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using SqlConnector;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer
{
   public static class DbFactory
    {
        public static IDbConnector GetDbConnector(ApplicationConfiguration configuration)
        {
            Db db = new Db(configuration);
            return db;
        }
        public static IDbConnector GetSqlConnector()
        {
            NamesManager db = new NamesManager();
            return db;
        }
        
    }
}
