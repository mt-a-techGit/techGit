using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMCore.driver.user_agent
{
    public class UserAgentProvider
    {
        UserAgentList iUserAgentList = new UserAgentList();
        public UserAgentProvider()
        {
            
        }
        public string getRandomUserAgent()
        {
            Random rnd = new Random();
            int index = rnd.Next(0, iUserAgentList.GetSize());
            return iUserAgentList.GetItem(index);
        }
    }
}
