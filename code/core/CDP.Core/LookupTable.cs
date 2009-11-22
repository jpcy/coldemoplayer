using System;
using System.Linq;

namespace CDP.Core
{
    public class LookupTable<T>
    {
        public T ErrorValue
        {
            get { return errorValue; }
        }

        private readonly T errorValue;
        private readonly Func<T, T, bool> equals;
        private readonly Func<T, T, bool> lessThan;
        private readonly Func<T, T, bool> greaterThanOrEqualTo;
        private readonly Func<T, T, T> add;
        private readonly Func<T, T, T> subtract;
        private readonly LookupElement<T>[] elements;

        public T this[string key]
        {
            get { return elements.First(e => e.Key == key).Value; }
        }

        public LookupTable(T errorValue, Func<T, T, bool> equals, Func<T, T, bool> lessThan, Func<T, T, bool> greaterThanOrEqualTo, Func<T, T, T> add, Func<T, T, T> subtract, params LookupElement<T>[] elements)
        {
            this.errorValue = errorValue;
            this.equals = equals;
            this.lessThan = lessThan;
            this.greaterThanOrEqualTo = greaterThanOrEqualTo;
            this.add = add;
            this.subtract = subtract;
            this.elements = elements;
        }

        public T Convert(T value, LookupTable<T> to)
        {
            LookupElement<T> startElement = (from e in elements
                                              orderby e.Value descending
                                              where e.IsStart && greaterThanOrEqualTo(value, e.Value)
                                              select e).FirstOrDefault();

            if (startElement != null)
            {
                // Find the element with the next highest value.
                LookupElement<T> nextElement = elements.Where(e => e != startElement && greaterThanOrEqualTo(e.Value, startElement.Value)).OrderBy(e => e.Value).FirstOrDefault();

                // Check for overflow.
                if (nextElement == null || !greaterThanOrEqualTo(value, nextElement.Value))
                {
                    T offset = subtract(value, startElement.Value);
                    return add(offset, to[startElement.Key]);
                }
            }

            LookupElement<T> element = elements.FirstOrDefault(e => equals(e.Value, value));

            if (element == null)
            {
                return value;
            }

            if (!to.elements.Any(e => e.Key == element.Key))
            {
                return errorValue;
            }

            return to[element.Key];
        }
    }

    public class LookupTable_short : LookupTable<short>
    {
        public LookupTable_short(params LookupElement<short>[] elements)
            : base(short.MaxValue, (a, b) => a == b,(a, b) => a < b, (a, b) => a >= b, (a, b) => (short)(a + b), (a, b) => (short)(a - b), elements)
        {
        }
    }

    public class LookupTable_int : LookupTable<int>
    {
        public LookupTable_int(params LookupElement<int>[] elements)
            : base(int.MaxValue, (a, b) => a == b, (a, b) => a < b, (a, b) => a >= b, (a, b) => a + b, (a, b) => a - b, elements)
        {
        }
    }

    public class LookupTable_uint : LookupTable<uint>
    {
        public LookupTable_uint(params LookupElement<uint>[] elements)
            : base(uint.MaxValue, (a, b) => a == b, (a, b) => a < b, (a, b) => a >= b, (a, b) => a + b, (a, b) => a - b, elements)
        {
        }
    }
}