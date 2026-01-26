namespace Asp.Tests;

using FluentAssertions;
using FunctionalDdd;
using FunctionalDdd.Asp.ModelBinding;
using FunctionalDdd.Asp.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text.Json.Serialization;
using Xunit;

/// <summary>
/// Tests for ScalarValueTypeHelper to ensure correct type detection and generic instantiation.
/// </summary>
public class ScalarValueTypeHelperTests
{
    #region Test Value Objects

    // Valid value object
    public class ValidVO : ScalarValueObject<ValidVO, string>, IScalarValue<ValidVO, string>
    {
        private ValidVO(string value) : base(value) { }
        public static Result<ValidVO> TryCreate(string? value, string? fieldName = null) =>
            string.IsNullOrEmpty(value) ? Error.Validation("Required", fieldName ?? "field") : new ValidVO(value);
    }

    // CRTP violation - TSelf doesn't match the class
    public class InvalidCRTP : ScalarValueObject<ValidVO, string>, IScalarValue<ValidVO, string>
    {
        private InvalidCRTP(string value) : base(value) { }
        public static Result<ValidVO> TryCreate(string? value, string? fieldName = null) =>
            ValidVO.TryCreate(value, fieldName);
    }

    // Doesn't implement the interface
    public class NotAValueObject
    {
        public string Value { get; set; } = "";
    }

    // Implements interface but not ScalarValueObject base class
    public class InterfaceOnly : IScalarValue<InterfaceOnly, int>
    {
        public int Value { get; }
        public InterfaceOnly(int value) => Value = value;
        public static Result<InterfaceOnly> TryCreate(int value, string? fieldName = null) =>
            new InterfaceOnly(value);
    }

    // Generic value object
    public class GenericVO<T> : ScalarValueObject<GenericVO<T>, T>, IScalarValue<GenericVO<T>, T>
        where T : IComparable
    {
        private GenericVO(T value) : base(value) { }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Required by IScalarValue interface pattern")]
        public static Result<GenericVO<T>> TryCreate(T? value, string? fieldName = null) =>
            value is null ? Error.Validation("Required", fieldName ?? "field") : new GenericVO<T>(value);
    }

    // Multiple interface implementations (edge case)
    public class MultiInterfaceVO : ScalarValueObject<MultiInterfaceVO, string>,
        IScalarValue<MultiInterfaceVO, string>,
        IComparable<MultiInterfaceVO>
    {
        private MultiInterfaceVO(string value) : base(value) { }
        public static Result<MultiInterfaceVO> TryCreate(string? value, string? fieldName = null) =>
            string.IsNullOrEmpty(value) ? Error.Validation("Required", fieldName ?? "field") : new MultiInterfaceVO(value);
        public int CompareTo(MultiInterfaceVO? other) => string.Compare(Value, other?.Value, StringComparison.Ordinal);
    }

    #endregion

    #region IsScalarValue Tests

    [Fact]
    public void IsScalarValue_ValidValueObject_ReturnsTrue()
    {
        // Act
        var result = ScalarValueTypeHelper.IsScalarValue(typeof(ValidVO));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsScalarValue_NotAValueObject_ReturnsFalse()
    {
        // Act
        var result = ScalarValueTypeHelper.IsScalarValue(typeof(NotAValueObject));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsScalarValue_String_ReturnsFalse()
    {
        // Act
        var result = ScalarValueTypeHelper.IsScalarValue(typeof(string));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsScalarValue_Int_ReturnsFalse()
    {
        // Act
        var result = ScalarValueTypeHelper.IsScalarValue(typeof(int));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsScalarValue_InterfaceOnlyImplementation_ReturnsTrue()
    {
        // Act
        var result = ScalarValueTypeHelper.IsScalarValue(typeof(InterfaceOnly));

        // Assert
        result.Should().BeTrue("interface implementation should be detected");
    }

    [Fact]
    public void IsScalarValue_GenericValueObject_ReturnsTrue()
    {
        // Act
        var result = ScalarValueTypeHelper.IsScalarValue(typeof(GenericVO<string>));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsScalarValue_MultiInterfaceValueObject_ReturnsTrue()
    {
        // Act
        var result = ScalarValueTypeHelper.IsScalarValue(typeof(MultiInterfaceVO));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsScalarValue_InvalidCRTP_ReturnsFalse()
    {
        // Act - CRTP violation: TSelf != declaring type
        var result = ScalarValueTypeHelper.IsScalarValue(typeof(InvalidCRTP));

        // Assert
        result.Should().BeFalse("CRTP pattern should be validated");
    }

    #endregion

    #region GetScalarValueInterface Tests

    [Fact]
    public void GetScalarValueInterface_ValidValueObject_ReturnsInterface()
    {
        // Act
        var interfaceType = ScalarValueTypeHelper.GetScalarValueInterface(typeof(ValidVO));

        // Assert
        interfaceType.Should().NotBeNull();
        interfaceType!.IsGenericType.Should().BeTrue();
        interfaceType.GetGenericTypeDefinition().Should().Be(typeof(IScalarValue<,>));
        interfaceType.GetGenericArguments()[0].Should().Be<ValidVO>();
        interfaceType.GetGenericArguments()[1].Should().Be<string>();
    }

    [Fact]
    public void GetScalarValueInterface_NotAValueObject_ReturnsNull()
    {
        // Act
        var interfaceType = ScalarValueTypeHelper.GetScalarValueInterface(typeof(NotAValueObject));

        // Assert
        interfaceType.Should().BeNull();
    }

    [Fact]
    public void GetScalarValueInterface_InvalidCRTP_ReturnsNull()
    {
        // Act
        var interfaceType = ScalarValueTypeHelper.GetScalarValueInterface(typeof(InvalidCRTP));

        // Assert
        interfaceType.Should().BeNull("CRTP validation should fail");
    }

    [Fact]
    public void GetScalarValueInterface_GenericVO_ReturnsCorrectInterface()
    {
        // Act
        var interfaceType = ScalarValueTypeHelper.GetScalarValueInterface(typeof(GenericVO<int>));

        // Assert
        interfaceType.Should().NotBeNull();
        interfaceType!.GetGenericArguments()[0].Should().Be<GenericVO<int>>();
        interfaceType.GetGenericArguments()[1].Should().Be<int>();
    }

    #endregion

    #region GetPrimitiveType Tests

    [Fact]
    public void GetPrimitiveType_ValidValueObject_ReturnsPrimitiveType()
    {
        // Act
        var primitiveType = ScalarValueTypeHelper.GetPrimitiveType(typeof(ValidVO));

        // Assert
        primitiveType.Should().Be<string>();
    }

    [Fact]
    public void GetPrimitiveType_NotAValueObject_ReturnsNull()
    {
        // Act
        var primitiveType = ScalarValueTypeHelper.GetPrimitiveType(typeof(NotAValueObject));

        // Assert
        primitiveType.Should().BeNull();
    }

    [Fact]
    public void GetPrimitiveType_GenericVO_ReturnsCorrectPrimitiveType()
    {
        // Act
        var primitiveType = ScalarValueTypeHelper.GetPrimitiveType(typeof(GenericVO<decimal>));

        // Assert
        primitiveType.Should().Be<decimal>();
    }

    [Fact]
    public void GetPrimitiveType_InterfaceOnly_ReturnsInt()
    {
        // Act
        var primitiveType = ScalarValueTypeHelper.GetPrimitiveType(typeof(InterfaceOnly));

        // Assert
        primitiveType.Should().Be<int>();
    }

    #endregion

    #region CreateGenericInstance Tests

    [Fact]
    public void CreateGenericInstance_JsonConverter_CreatesInstance()
    {
        // Act
        var converter = ScalarValueTypeHelper.CreateGenericInstance<JsonConverter>(
            typeof(ValidatingJsonConverter<,>),
            typeof(ValidVO),
            typeof(string));

        // Assert
        converter.Should().NotBeNull();
        converter.Should().BeOfType<ValidatingJsonConverter<ValidVO, string>>();
    }

    [Fact]
    public void CreateGenericInstance_ModelBinder_CreatesInstance()
    {
        // Act
        var binder = ScalarValueTypeHelper.CreateGenericInstance<IModelBinder>(
            typeof(ScalarValueModelBinder<,>),
            typeof(ValidVO),
            typeof(string));

        // Assert
        binder.Should().NotBeNull();
        binder.Should().BeOfType<ScalarValueModelBinder<ValidVO, string>>();
    }

    [Fact]
    public void CreateGenericInstance_WithGenericVO_CreatesInstance()
    {
        // Act
        var converter = ScalarValueTypeHelper.CreateGenericInstance<JsonConverter>(
            typeof(ValidatingJsonConverter<,>),
            typeof(GenericVO<int>),
            typeof(int));

        // Assert
        converter.Should().NotBeNull();
        converter.Should().BeOfType<ValidatingJsonConverter<GenericVO<int>, int>>();
    }

    [Fact]
    public void CreateGenericInstance_WrongPrimitiveType_ReturnsNull()
    {
        // Act - ValidVO uses string, but we're passing int
        var converter = ScalarValueTypeHelper.CreateGenericInstance<JsonConverter>(
            typeof(ValidatingJsonConverter<,>),
            typeof(ValidVO),
            typeof(int)); // Wrong primitive type

        // Assert - Should return null when types don't match constraints
        converter.Should().BeNull("generic type constraints are violated");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void IsScalarValue_Null_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ScalarValueTypeHelper.IsScalarValue(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetScalarValueInterface_Null_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ScalarValueTypeHelper.GetScalarValueInterface(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetPrimitiveType_Null_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ScalarValueTypeHelper.GetPrimitiveType(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsScalarValue_AbstractClass_ReturnsFalse()
    {
        // Act
        var result = ScalarValueTypeHelper.IsScalarValue(typeof(ScalarValueObject<,>));

        // Assert
        result.Should().BeFalse("abstract generic type definition shouldn't be detected");
    }

    [Fact]
    public void IsScalarValueObject_Interface_ReturnsFalse()
    {
        // Act
        var result = ScalarValueTypeHelper.IsScalarValue(typeof(IScalarValue<,>));

        // Assert
        result.Should().BeFalse("interface itself shouldn't be detected");
    }

    [Fact]
    public void CreateGenericInstance_NullGenericTypeDefinition_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ScalarValueTypeHelper.CreateGenericInstance<JsonConverter>(
            null!,
            typeof(ValidVO),
            typeof(string));

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateGenericInstance_NullValueObjectType_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ScalarValueTypeHelper.CreateGenericInstance<JsonConverter>(
            typeof(ValidatingJsonConverter<,>),
            null!,
            typeof(string));

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateGenericInstance_NullPrimitiveType_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ScalarValueTypeHelper.CreateGenericInstance<JsonConverter>(
            typeof(ValidatingJsonConverter<,>),
            typeof(ValidVO),
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullWorkflow_DetectAndCreateConverter()
    {
        // Arrange
        var type = typeof(ValidVO);

        // Act & Assert - Full workflow
        // 1. Check if it's a value object
        ScalarValueTypeHelper.IsScalarValue(type).Should().BeTrue();

        // 2. Get the interface
        var interfaceType = ScalarValueTypeHelper.GetScalarValueInterface(type);
        interfaceType.Should().NotBeNull();

        // 3. Get primitive type
        var primitiveType = ScalarValueTypeHelper.GetPrimitiveType(type);
        primitiveType.Should().Be<string>();

        // 4. Create converter
        var converter = ScalarValueTypeHelper.CreateGenericInstance<JsonConverter>(
            typeof(ValidatingJsonConverter<,>),
            type,
            primitiveType!);

        converter.Should().NotBeNull();
        converter.Should().BeOfType<ValidatingJsonConverter<ValidVO, string>>();
    }

    [Fact]
    public void FullWorkflow_NonValueObject_ReturnsNullAtEachStep()
    {
        // Arrange
        var type = typeof(NotAValueObject);

        // Act & Assert
        ScalarValueTypeHelper.IsScalarValue(type).Should().BeFalse();
        ScalarValueTypeHelper.GetScalarValueInterface(type).Should().BeNull();
        ScalarValueTypeHelper.GetPrimitiveType(type).Should().BeNull();
    }

    #endregion
}