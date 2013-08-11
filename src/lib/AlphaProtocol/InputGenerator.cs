using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lib.AlphaProtocol
{
    public static class InputGenerator
    {
        public static UInt64[] Generate()
        {
            var samples = new UInt64[256];
            samples[0] = UInt64.MinValue;
            samples[1] = UInt64.MaxValue;
            samples[2] = UInt64.Parse("00000000FFFFFFFF", NumberStyles.AllowHexSpecifier);
            samples[3] = UInt64.Parse("FFFFFFFF00000000", NumberStyles.AllowHexSpecifier);
            samples[4] = UInt64.Parse("0F0F0F0F0F0F0F0F", NumberStyles.AllowHexSpecifier);
            samples[5] = UInt64.Parse("F0F0F0F0F0F0F0F0", NumberStyles.AllowHexSpecifier);
            samples[6] = UInt64.Parse("5555555555555555", NumberStyles.AllowHexSpecifier);
            samples[7] = UInt64.Parse("AAAAAAAAAAAAAAAA", NumberStyles.AllowHexSpecifier);
            var random = new Random(Environment.TickCount);
            for (int k = 1; k < 32; k++)
            {
                for (int i = 0; i < 4; i++)
                {
                    UInt64 sample = 0;
                    var onesCount = 0;
                    while (onesCount < k)
                    {
                        var onePosition = random.Next(64);
                        if ((sample & (((UInt64)1) << onePosition)) == 0)
                        {
                            sample += ((UInt64)1) << onePosition;
                            onesCount++;
                        }
                    }
                    samples[k * 8 + i * 2] = sample;
                    samples[k * 8 + i * 2 + 1] = sample ^ UInt64.MaxValue;
                }
            }
            return samples;
        }
    }
}
