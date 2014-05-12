using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace BLL.Class
{
    class dBFunctions
    {
        public static string ConnectionStringSQLite
        {
            get
            {
                  string database ="C:\\CMDB\\CM.db";
                  string connectionString = @"Data Source=" + Path.GetFullPath(database);
                return connectionString;
            }
        }
    }
}
