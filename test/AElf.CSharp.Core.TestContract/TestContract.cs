namespace AElf.CSharp.Core.TestContract;

public class TestContract : TestContractContainer.TestContractBase
{
    public override StringOutput TestStringState(StringInput input)
    {
        if (string.IsNullOrEmpty(State.StringInfo.Value))
            State.StringInfo.Value = string.Empty;

        State.StringInfo.Value += input.StringValue;
        return new StringOutput
        {
            StringValue = State.StringInfo.Value
        };
    }
}