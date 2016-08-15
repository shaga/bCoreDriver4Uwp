using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bCoreDriver.Models
{
    enum ESliderType
    {
        Motor,
        Servo
    }

    class BcoreSliderParam
    {
        public ESliderType Type { get; private set; }
        public int Idx { get; private set; }

        public BcoreSliderParam(ESliderType type, int idx)
        {
            Type = type;
            Idx = idx;
        }
    }
}
