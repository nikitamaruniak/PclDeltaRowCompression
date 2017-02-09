using System;
using System.Collections.Generic;
using System.Linq;

class PclDeltaRowCompression
{
    public static byte[] Encode(byte[] input, byte[] seed)
    {
        if (input.Length != seed.Length)
            throw new ArgumentException();

        return GetBytes(input, seed).ToArray();
    }

    private static IEnumerable<byte> GetBytes(
        byte[] input,
        byte[] seed)
    {
        IEnumerable<Difference> diffs =
            Compare(input, seed, MAX_DIFF_LENGTH);

        foreach (Difference diff in diffs)
        {
            foreach (byte cmdByte in GetCommandBytes(diff))
            {
                yield return cmdByte;
            }

            for (int i = diff.StartIndex; i < diff.StartIndex + diff.Length; i++)
            {
                yield return input[i];
            }
        }
    }

    private const int MAX_DIFF_LENGTH = 8;

    private struct Difference
    {
        public int Length;
        public int StartIndex;
        public int Offset;
    }

    private static IEnumerable<Difference> Compare(
        byte[] input,
        byte[] seed,
        int maxDiffLength)
    {
        int length = 0;
        int offset = 0;
        for (int i = 0; i < input.Length; i++)
        {
            bool isEqual = input[i] == seed[i];

            bool sequenceReady =
                isEqual && length > 0 ||
                !isEqual && length == maxDiffLength;

            if (sequenceReady)
            {
                yield return new Difference {
                    Length = length,
                    StartIndex = i - length,
                    Offset = offset
                };
                length = 0;
                offset = 0;
            }

            if (isEqual)
                offset++;
            else
                length++;
        }
        if (length > 0)
            yield return new Difference {
                Length = length,
                StartIndex = input.Length - length,
                Offset = offset
            };
    }

    private static IEnumerable<byte> GetCommandBytes(Difference r)
    {
        byte firstByte =
            (byte)((r.Length - 1) << 5 | Math.Min(r.Offset, 31));
        yield return firstByte;

        int remainder = r.Offset - 31;

        while (remainder >= 0)
        {
            byte nextByte = (byte)Math.Min(remainder, 255);
            remainder -= 255;
            yield return nextByte;
        }
    }
}
