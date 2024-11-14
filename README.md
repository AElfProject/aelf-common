# AElf Common

## Overview

AElf Common is a foundational library for the AElf blockchain project. It contains essential types and helper classes that are used throughout the [AElf](https://github.com/AElfProject/AElf) system.

## Features

- **Common Types**: Basic data structures and types used across the AElf project, like `AElf.Types` and `AElf.Kernel.Types`.
- **Helper Classes**: Utility classes that provide common functionalities, like `AElf.Cryptography`.
- **Extensions**: Extension methods to enhance the functionality of existing types.
- **Infrastructure**: Database implementation of AElf, `AElf.Database`.

## Installation

To install AElf Common, add the following package to your project:

```bash
dotnet add package AElf.Common
```

## Usage

Here are some examples of how to use the types and helper classes provided by AElf Common:

### Example 1: Using a Common Type

```csharp
using AElf.Common;

public class Example
{
    public void UseCommonType()
    {
        var hash = HashHelper.ComputeFrom("example");
        Console.WriteLine(hash.ToHex());
        var address = Address.FromPublicKey(GenerateKeyPair().PublicKey);
        Console.WriteLine(address.ToBase58());
    }
}
```

### Example 2: Using a Helper Class

```csharp
using AElf.Common.Helpers;

public class Example
{
    public ECKeyPair GenerateKeyPair()
    {
        var ecKeyPair = CryptoHelper.GenerateKeyPair();
        Console.WriteLine(ecKeyPair.PublicKey.ToHex());
        return ecKeyPair;
    }
}
```

## Contributing

Contributions are welcome! Please read the [contributing guidelines](CONTRIBUTING.md) before submitting a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
