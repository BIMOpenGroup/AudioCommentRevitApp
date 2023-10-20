using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioComment.MyMessage
{
    [Serializable]
    class MyMessage
    {
        public int Id;
        public string Text;

        public override string ToString()
        {
            return string.Format("\"{0}\" (message ID = {1})", Text, Id);
        }
    }
}
