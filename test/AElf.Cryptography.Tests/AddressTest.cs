using System;
using System.Linq;
using AElf.Types;
using Shouldly;
using Xunit;

namespace AElf.Cryptography.Tests;

public class AddressTest
{
    [Fact]
    public void Generate_Address()
    {
        //Generate default
        var address1 = Address.FromBase58("2DZER7qHVwv3PUMFsHuQaQbE4wDFsCRzJsxLwYEk8rgM3HVn1S");
        var address2 = Address.FromBase58("xFqJD9R33mQBQPr1hCFUZMayXFQ577j34MPyUdXzbPpAYufG2");
        address1.ShouldNotBeSameAs(address2);

        //Generate from String
        var address3 = Address.FromBase58("z1NVbziJbekvcza3Zr4Gt4eAvoPBZThB68LHRQftrVFwjtGVM");
        address3.ShouldNotBe(null);

        //Generate from byte
        var bytes = Enumerable.Repeat((byte)0xEF, 32).ToArray();
        var address4 = Address.FromBytes(bytes);
        address4.ShouldNotBe(null);

        bytes = Enumerable.Repeat((byte)32, 20).ToArray();
        Should.Throw<ArgumentException>(() => { Address.FromBytes(bytes); });

        //Generate from public key
        var pk = CryptoHelper.GenerateKeyPair().PublicKey;
        var address5 = Address.FromPublicKey(pk);
        address5.ShouldNotBe(null);
        address5.ToByteArray().Length.ShouldBe(32);
    }

    [Fact]
    public void Get_Address_Info()
    {
        var pk = CryptoHelper.GenerateKeyPair().PublicKey;
        var address = Address.FromPublicKey(pk);
        var addressString = address.ToBase58();
        addressString.ShouldNotBe(string.Empty);
    }
    
    [Fact]
    public void StateKeyExtensions_ToStateKey_Test()
    {
        var statePath = new StatePath
        {
            Parts = { "1", "2", "3" }
        };

        var privateKey =
            ByteArrayHelper.HexStringToByteArray("5945c176c4269dc2aa7daf7078bc63b952832e880da66e5f2237cdf79bc59c5f");
        var keyPair = CryptoHelper.FromPrivateKey(privateKey);
        var address = Address.FromPublicKey(keyPair.PublicKey);
        var addressBase58String = address.ToBase58();
        var targetKeyString = string.Join("/", new[] { addressBase58String }.Concat(statePath.Parts));
        var stateKey1 = statePath.ToStateKey(address);
        var scopedStatePath = new ScopedStatePath
        {
            Path = statePath,
            Address = address
        };
        var stateKey2 = scopedStatePath.ToStateKey();

        stateKey1.ShouldBe(targetKeyString);
        stateKey2.ShouldBe(stateKey1);
    }
}