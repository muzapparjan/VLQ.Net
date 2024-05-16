using System.Numerics;

namespace VariableLengthQuantity;

/**
 * <summary>Variable-Lenght Quantity</summary>
 * <see href="https://en.wikipedia.org/wiki/Variable-length_quantity"/>
 */
public static class VLQ
{
    /**
     * <summary>Read a VLQ from stream.</summary>
     * <param name="stream">Target stream to read from</param>
     * <returns>The first 7-bit encoded number read from the string</returns>
     */
    public static BigInteger Read(Stream stream)
    {
        var result = BigInteger.Zero;
        int slice;
        byte @byte;
        var firstByte = true;
        while (true)
        {
            slice = stream.ReadByte();
            if (slice < 0)
                throw new EndOfStreamException();
            @byte = (byte)slice;
            var offset = 6;
            if (firstByte)
            {
                while (offset >= 0 && (@byte & (1 << offset)) == 0)
                    offset--;
                if (offset < 0)
                    return 0;
                firstByte = false;
            }
            var part = (byte)(((byte)(@byte << (7 - offset))) >> (7 - offset));
            result = (result << (offset + 1)) | part;
            if (@byte >> 7 == 0)
                break;
        }
        return result;
    }
    /**
     * <summary>Write a variable-length number to the stream.</summary>
     * <param name="value">the value to encode and write to the stream</param>
     * <param name="stream">the stream to write to</param>
     */
    public static void Write(BigInteger value, Stream stream) => stream.Write(Encode(value));
    /**
     * <summary>Encode a value as variable-length with each group of length 7.</summary>
     * <param name="value">the value to encode</param>
     * <returns>the encoded byte sequence in big endian order</returns>
     */
    public static byte[] Encode(BigInteger value)
    {
        var bytes = new List<byte>();
        do
        {
            var slice = (byte)((byte.CreateTruncating(value)) & 0x7F);
            value >>= 7;
            if (bytes.Count > 0)
                slice |= 0x80;
            bytes.Insert(0, slice);
        } while (value > 0);
        return [.. bytes];
    }
    /**
     * <summary>Decode a byte sequence to a number.</summary>
     * <param name="bytes">Target byte sequence to encode</param>
     * <returns>The decoded number</returns>
     */
    public static BigInteger Decode(byte[] bytes) => Read(new MemoryStream(bytes));

    /**
     * <summary>Read a VLQ from stream.</summary>
     * <param name="stream">Target stream to read from</param>
     * <returns>The first 7-bit encoded number read from the string</returns>
     */
    public static T Read<T>(Stream stream) where T : INumberBase<T> => T.CreateChecked(Read(stream));
    /**
     * <summary>Write a variable-length number to the stream.</summary>
     * <param name="value">the value to encode and write to the stream</param>
     * <param name="stream">the stream to write to</param>
     */
    public static void Write<T>(T value, Stream stream) where T : INumberBase<T> => stream.Write(Encode(value));
    /**
     * <summary>Encode a value as variable-length with each group of length 7.</summary>
     * <param name="value">the value to encode</param>
     * <returns>the encoded byte sequence in big endian order</returns>
     */
    public static byte[] Encode<T>(T value) where T : INumberBase<T> => Encode(BigInteger.CreateChecked(value));
    /**
     * <summary>Decode a byte sequence to a number.</summary>
     * <param name="bytes">Target byte sequence to encode</param>
     * <returns>The decoded number</returns>
     */
    public static T Decode<T>(byte[] bytes) where T : INumberBase<T> => T.CreateChecked(Decode(bytes));
}