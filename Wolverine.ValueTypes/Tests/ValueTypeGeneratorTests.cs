using System.Linq;
using Shouldly;
using Xunit;

namespace NForza.Wolverine.ValueTypes.Tests;

public class ValueTypeGeneratorTests
{
    [Fact]
    public void GuidValue_GeneratesRecordStruct()
    {
        var source = @"
using NForza.Wolverine.ValueTypes;

namespace TestApp;

[GuidValue]
public partial record struct CustomerId;
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);
        diagnostics.ShouldBeEmpty();

        var recordSource = generated.FirstOrDefault(s => s.Contains("public partial record struct CustomerId"));
        recordSource.ShouldNotBeNull();
        recordSource.ShouldContain("IGuidValueType");
        recordSource.ShouldContain("IComparable<CustomerId>");
        recordSource.ShouldContain("Guid.NewGuid()");
        recordSource.ShouldContain("Guid.Empty");
        recordSource.ShouldContain("explicit operator Guid");
        recordSource.ShouldContain("explicit operator CustomerId");
        recordSource.ShouldContain("AsGuid()");
    }

    [Fact]
    public void GuidValue_GeneratesTryParse()
    {
        var source = @"
using NForza.Wolverine.ValueTypes;

namespace TestApp;

[GuidValue]
public partial record struct OrderId;
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);
        diagnostics.ShouldBeEmpty();

        var recordSource = generated.FirstOrDefault(s => s.Contains("public partial record struct OrderId"));
        recordSource.ShouldNotBeNull();
        recordSource.ShouldContain("static bool TryParse(string? s, out OrderId result)");
        recordSource.ShouldContain("Guid.TryParse(s, out var guid)");
    }

    [Fact]
    public void GuidValue_GeneratesJsonConverter()
    {
        var source = @"
using NForza.Wolverine.ValueTypes;

namespace TestApp;

[GuidValue]
public partial record struct CustomerId;
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);
        diagnostics.ShouldBeEmpty();

        var converterSource = generated.FirstOrDefault(s => s.Contains("class CustomerIdJsonConverter"));
        converterSource.ShouldNotBeNull();
        converterSource.ShouldContain("JsonConverter<CustomerId>");
        converterSource.ShouldContain("Guid.TryParse");
    }

    [Fact]
    public void StringValue_GeneratesRecordStruct()
    {
        var source = @"
using NForza.Wolverine.ValueTypes;

namespace TestApp;

[StringValue(1, 50)]
public partial record struct Name;
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);
        diagnostics.ShouldBeEmpty();

        var recordSource = generated.FirstOrDefault(s => s.Contains("public partial record struct Name(string Value)"));
        recordSource.ShouldNotBeNull();
        recordSource.ShouldContain("IStringValueType");
        recordSource.ShouldContain("Value.Length >= 1");
        recordSource.ShouldContain("Value.Length <= 50");
        recordSource.ShouldContain("static bool TryParse");
    }

    [Fact]
    public void StringValue_WithRegex_GeneratesValidation()
    {
        var source = @"
using NForza.Wolverine.ValueTypes;

namespace TestApp;

[StringValue(1, 50, ""^[A-Za-z ]*$"")]
public partial record struct PersonName;
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);
        diagnostics.ShouldBeEmpty();

        var recordSource = generated.FirstOrDefault(s => s.Contains("public partial record struct PersonName"));
        recordSource.ShouldNotBeNull();
        recordSource.ShouldContain("Regex.IsMatch");
    }

    [Fact]
    public void IntValue_GeneratesRecordStruct()
    {
        var source = @"
using NForza.Wolverine.ValueTypes;

namespace TestApp;

[IntValue(0, 100)]
public partial record struct Amount;
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);
        diagnostics.ShouldBeEmpty();

        var recordSource = generated.FirstOrDefault(s => s.Contains("public partial record struct Amount(int Value)"));
        recordSource.ShouldNotBeNull();
        recordSource.ShouldContain("IIntValueType");
        recordSource.ShouldContain("Value >= 0");
        recordSource.ShouldContain("Value <= 100");
        recordSource.ShouldContain("operator <(");
        recordSource.ShouldContain("operator >(");
        recordSource.ShouldContain("static bool TryParse");
    }

    [Fact]
    public void DoubleValue_GeneratesRecordStruct()
    {
        var source = @"
using NForza.Wolverine.ValueTypes;

namespace TestApp;

[DoubleValue(0.0, 99.9)]
public partial record struct Price;
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);
        diagnostics.ShouldBeEmpty();

        var recordSource = generated.FirstOrDefault(s => s.Contains("public partial record struct Price(double Value)"));
        recordSource.ShouldNotBeNull();
        recordSource.ShouldContain("IDoubleValueType");
        recordSource.ShouldContain("static bool TryParse");
    }

    [Fact]
    public void MultipleValueTypes_GeneratesWolverineExtension()
    {
        var source = @"
using NForza.Wolverine.ValueTypes;

namespace TestApp;

[GuidValue]
public partial record struct CustomerId;

[StringValue(1, 100)]
public partial record struct CustomerName;
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);
        diagnostics.ShouldBeEmpty();

        var extensionSource = generated.FirstOrDefault(s => s.Contains("WolverineValueTypeExtension"));
        extensionSource.ShouldNotBeNull();
        extensionSource.ShouldContain("IWolverineExtension");
        extensionSource.ShouldContain("CustomerIdJsonConverter");
        extensionSource.ShouldContain("CustomerNameJsonConverter");
    }

    [Fact]
    public void NoNamespace_GeneratesWithoutNamespace()
    {
        var source = @"
using NForza.Wolverine.ValueTypes;

[GuidValue]
public partial record struct GlobalId;
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);
        diagnostics.ShouldBeEmpty();

        var recordSource = generated.FirstOrDefault(s => s.Contains("public partial record struct GlobalId"));
        recordSource.ShouldNotBeNull();
        recordSource.Split(["public partial record"], System.StringSplitOptions.None)[0]
            .Substring(recordSource.IndexOf("using NForza") + 1)
            .ShouldNotContain("namespace");
    }

    [Fact]
    public void LongValue_GeneratesRecordStruct()
    {
        var source = @"
using NForza.Wolverine.ValueTypes;

namespace TestApp;

[LongValue(0, 1000000)]
public partial record struct Counter;
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);
        diagnostics.ShouldBeEmpty();

        var recordSource = generated.FirstOrDefault(s => s.Contains("public partial record struct Counter(long Value)"));
        recordSource.ShouldNotBeNull();
        recordSource.ShouldContain("ILongValueType");
        recordSource.ShouldContain("Value >= 0L");
        recordSource.ShouldContain("Value <= 1000000L");
        recordSource.ShouldContain("operator <(");
        recordSource.ShouldContain("operator >(");
        recordSource.ShouldContain("AsLong()");
        recordSource.ShouldContain("static bool TryParse");
        recordSource.ShouldContain("IParsable<Counter>");
    }

    [Fact]
    public void DecimalValue_GeneratesRecordStruct()
    {
        var source = @"
using NForza.Wolverine.ValueTypes;

namespace TestApp;

[DecimalValue]
public partial record struct Money;
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);
        diagnostics.ShouldBeEmpty();

        var recordSource = generated.FirstOrDefault(s => s.Contains("public partial record struct Money(decimal Value)"));
        recordSource.ShouldNotBeNull();
        recordSource.ShouldContain("IDecimalValueType");
        recordSource.ShouldContain("AsDecimal()");
        recordSource.ShouldContain("CultureInfo.InvariantCulture");
        recordSource.ShouldContain("static bool TryParse");
        recordSource.ShouldContain("IParsable<Money>");
    }

    [Fact]
    public void DateOnlyValue_GeneratesRecordStruct()
    {
        var source = @"
using NForza.Wolverine.ValueTypes;

namespace TestApp;

[DateOnlyValue]
public partial record struct BirthDate;
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);
        diagnostics.ShouldBeEmpty();

        var recordSource = generated.FirstOrDefault(s => s.Contains("public partial record struct BirthDate(DateOnly Value)"));
        recordSource.ShouldNotBeNull();
        recordSource.ShouldContain("IDateOnlyValueType");
        recordSource.ShouldContain("AsDateOnly()");
        recordSource.ShouldContain("Value != default");
        recordSource.ShouldContain("static bool TryParse");
        recordSource.ShouldContain("IParsable<BirthDate>");

        var converterSource = generated.FirstOrDefault(s => s.Contains("class BirthDateJsonConverter"));
        converterSource.ShouldNotBeNull();
        converterSource.ShouldContain("yyyy-MM-dd");
    }

    [Fact]
    public void DateTimeValue_GeneratesRecordStruct()
    {
        var source = @"
using NForza.Wolverine.ValueTypes;

namespace TestApp;

[DateTimeValue]
public partial record struct CreatedAt;
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);
        diagnostics.ShouldBeEmpty();

        var recordSource = generated.FirstOrDefault(s => s.Contains("public partial record struct CreatedAt(DateTime Value)"));
        recordSource.ShouldNotBeNull();
        recordSource.ShouldContain("IDateTimeValueType");
        recordSource.ShouldContain("AsDateTime()");
        recordSource.ShouldContain("Value != default");
        recordSource.ShouldContain("static bool TryParse");
        recordSource.ShouldContain("IParsable<CreatedAt>");
    }

    [Fact]
    public void DateTimeOffsetValue_GeneratesRecordStruct()
    {
        var source = @"
using NForza.Wolverine.ValueTypes;

namespace TestApp;

[DateTimeOffsetValue]
public partial record struct Timestamp;
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);
        diagnostics.ShouldBeEmpty();

        var recordSource = generated.FirstOrDefault(s => s.Contains("public partial record struct Timestamp(DateTimeOffset Value)"));
        recordSource.ShouldNotBeNull();
        recordSource.ShouldContain("IDateTimeOffsetValueType");
        recordSource.ShouldContain("AsDateTimeOffset()");
        recordSource.ShouldContain("Value != default");
        recordSource.ShouldContain("static bool TryParse");
        recordSource.ShouldContain("IParsable<Timestamp>");
    }

    [Fact]
    public void GuidValue_GeneratesIParsable()
    {
        var source = @"
using NForza.Wolverine.ValueTypes;

namespace TestApp;

[GuidValue]
public partial record struct OrderId;
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);
        diagnostics.ShouldBeEmpty();

        var recordSource = generated.FirstOrDefault(s => s.Contains("public partial record struct OrderId"));
        recordSource.ShouldNotBeNull();
        recordSource.ShouldContain("IParsable<OrderId>");
        recordSource.ShouldContain("static OrderId Parse(string s, IFormatProvider? provider)");
    }
}
