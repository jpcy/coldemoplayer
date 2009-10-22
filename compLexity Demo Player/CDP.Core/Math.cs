using System;

namespace CDP.Core
{
    public static class Math
    {
        public static uint LogBase2(uint value)
        {
            uint answer = 0;

            while ((value >>= 1) != 0)
            {
                answer++;
            }

            return answer;
        }
    }
}
