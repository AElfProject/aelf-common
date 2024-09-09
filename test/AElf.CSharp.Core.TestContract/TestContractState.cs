using AElf.Sdk.CSharp.State;

namespace AElf.CSharp.Core.TestContract;

public class TestContractState : ContractState
{
    public StringState StringInfo { get; set; }
}