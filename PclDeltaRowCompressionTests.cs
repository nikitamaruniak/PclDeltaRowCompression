using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class PclDeltaRowCompressionTests
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Encode_throws_exception_when_input_and_seed_have_different_length() 
    {
        byte[] seed = new byte[1];
        byte[] input = new byte[2];
        PclDeltaRowCompression.Encode(input, seed);
    }
    
    [TestMethod]
    public void Encode_returns_empty_array_when_input_and_seed_are_empty()
    {
        byte[] seed = new byte[0];
        byte[] input = new byte[0];
        byte[] expected = new byte[0];
        Verify(input, seed, expected);
    }

    private void Verify(byte[] input, byte[] seed, byte[] expected)
    {
        CollectionAssert.AreEqual(
            expected,
            PclDeltaRowCompression.Encode(input, seed));
    }

    [TestMethod]
    public void Encode_returns_empty_array_when_input_and_seed_are_equal()
    {
        byte[] seed =  {1, 2, 3, 4};
        byte[] input = {1, 2, 3, 4};
        byte[] expected = new byte[0];
        Verify(input, seed, expected);
    }

    [TestMethod]
    public void Encode_input_is_completely_different_from_seed()
    {
        byte[] seed =  {1, 2, 3, 4};
        byte[] input = {5, 6, 7, 8};
        byte[] expected = {(4 - 1) << 5 | 0, 5, 6, 7, 8};
        Verify(input, seed, expected);
    }

    [TestMethod]
    public void Encode_one_replacement_at_the_end()
    {
        byte[] seed =  {0, 0, 1, 1};
        byte[] input = {0, 0, 2, 2};
        byte[] expected = {(2 - 1) << 5 | 2, 2, 2};
        Verify(input, seed, expected);
    }

    [TestMethod]
    public void Encode_one_replacement_at_the_start()
    {
        byte[] seed =  {1, 1, 0, 0};
        byte[] input = {2, 2, 0, 0};
        byte[] expected = {(2 - 1) << 5 | 0, 2, 2};
        Verify(input, seed, expected);
    }

    [TestMethod]
    public void Encode_one_replacement_in_the_middle()
    {
        byte[] seed =  {0, 0, 1, 1, 0, 0};
        byte[] input = {0, 0, 2, 2, 0, 0};
        byte[] expected = {(2 - 1) << 5 | 2, 2, 2};
        Verify(input, seed, expected);
    }

    [TestMethod]
    public void Encode_two_replacements()
    {
        byte[] seed =  {1, 1, 0, 0, 1, 1};
        byte[] input = {2, 2, 0, 0, 2, 2};
        byte[] expected = {
            (2 - 1) << 5 | 0, 2, 2,
            (2 - 1) << 5 | 2, 2, 2};
        Verify(input, seed, expected);
    }

    [TestMethod]
    public void Encode_replacements_can_not_be_longer_than_8_bytes()
    {
        byte[] seed =  {0, 1, 1, 1, 1, 1, 1, 1, 1, 1};
        byte[] input = {0, 2, 2, 2, 2, 2, 2, 2, 2, 2};
        byte[] expected = {
            (8 - 1) << 5 | 1, 2, 2, 2, 2, 2, 2, 2, 2,
            (1 - 1) << 5 | 0, 2};
        Verify(input, seed, expected);
    }

    [TestMethod]
    public void Encode_replacement_offset_longer_than_30()
    {
        byte[] seed = new byte[31 + 1 + 1];
        seed[seed.Length - 1] = 1;
        byte[] input = new byte[31 + 1 + 1];
        input[input.Length - 1] = 2;
        byte[] expected = { (1 - 1) << 5 | 31, 1, 2 };
        Verify(input, seed, expected);
    }

    [TestMethod]
    public void Encode_replacement_offset_longer_than_255()
    {
        byte[] seed = new byte[31 + 255 + 255 + 3 + 1];
        seed[seed.Length - 1] = 1;
        byte[] input = new byte[31 + 255 + 255 + 3 + 1];
        input[input.Length - 1] = 2;
        byte[] expected = { (1 - 1) << 5 | 31, 255, 255, 3, 2 };
        Verify(input, seed, expected);
    }
}
