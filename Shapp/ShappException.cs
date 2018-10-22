using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shapp
{
    public class ShappException : Exception
    {
        public ShappException(string message)
           : base(message)
        {
        }
    }
}
