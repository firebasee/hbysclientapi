using HBYSClientApi.Helpers;

namespace HBYSClientApi.Parameters;

public record ProtocolOrIcmalNo
{
    public string Value { get; }
    public ProtocolOrIcmalNo(int value)
    {
        Value = GeneralHelpers.GenerateIcmalOrProtocolNo(value);
    }
}