using System;

namespace HBYSClientApi.Helpers;

public class GeneralHelpers
{
    public static string GenerateIcmalOrProtocolNo(int value)
        {
            string yearPart = DateTime.Now.Year.ToString().Substring(2, 2);

            Span<char> valuePart = stackalloc char[7];
            bool isSuccess = value.ToString("D7").AsSpan().TryCopyTo(valuePart);

            if (!isSuccess)
            {
                valuePart.Clear();
                "0000000".AsSpan().CopyTo(valuePart);
            }

            Span<char> icmalNo = stackalloc char[9];
            yearPart.AsSpan().CopyTo(icmalNo);
            valuePart.CopyTo(icmalNo.Slice(2));

            return new string(icmalNo);
        }
}
