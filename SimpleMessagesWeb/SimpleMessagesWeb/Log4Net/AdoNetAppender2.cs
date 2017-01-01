using log4net.Appender;
using System.Configuration;

namespace WebDemo.Log4Net
{
    public class AdoNetAppender2 : AdoNetAppender
    {
        public new string ConnectionString
        {
            get
            {
                return base.ConnectionString;
            }
            set
            {
                base.ConnectionString = ConfigurationManager.ConnectionStrings["MyConnStrName"].ConnectionString;
            }
        }
    }
}