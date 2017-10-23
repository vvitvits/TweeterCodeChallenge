using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweetController
{
   public static class HelperMethods
    {
        public static string ToShortDescriptiveString(System.DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                return string.Empty;
            }
            else if (date.Year == System.DateTime.Today.Year)
            {
                if (date.DayOfYear == System.DateTime.Today.DayOfYear)
                {
                    return string.Format("Today {0:h:mm tt}", date);
                }
                else if (date.DayOfYear == System.DateTime.Today.DayOfYear - 1)
                {
                    return string.Format("Yesterday {0:h:mm tt}", date);
                }
                return string.Format("{0:MMM %d, h:mm tt}", date);
            }
            return string.Format("{0:MM/dd/yy h:mm tt}", date);
        }
    }
}
