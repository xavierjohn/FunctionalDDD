namespace RailwayOrientedProgramming.Tests;
using System;
using FunctionalDDD.Results;
using FunctionalDDD.Results.Errors;
using Xunit;

public class MaybeExtensionTests
{
    [Fact]
    public void Convert_nullable_struct_to_result_pass()
    {
        // Arrange
        Maybe<DateTime> date = DateTime.Now;

        // Act
        var result = date.ToResult(Error.Validation("Date not set."));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(date.Value);
    }

    [Fact]
    public void Convert_nullable_struct_to_result_fail()
    {
        // Arrange
        Maybe<DateTime> date = default;

        // Act
        var result = date.ToResult(Error.Validation("Date not set."));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("Date not set."));

    }

    [Fact]
    public void Convert_nullable_class_to_result_pass()
    {
        // Arrange
        MyClass my = new();
        Maybe<MyClass> maybeMy = my;

        // Act
        var result = maybeMy.ToResult(Error.Validation("MyClass is not set."));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(my);
    }

    [Fact]
    public void Convert_nullable_class_to_result_fail()
    {
        // Arrange
        Maybe<MyClass> myClass = default;

        // Act
        var result = myClass.ToResult(Error.Validation("MyClass is not set."));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("MyClass is not set."));
    }

    // async class
    [Fact]
    public async Task Convert_task_nullable_class_to_result_pass()
    {
        // Arrange
        MyClass my = new();
        Maybe<MyClass> maybeMy = my;
        var myClassTask = Task.FromResult(maybeMy);

        // Act
        var result = await myClassTask.ToResultAsync(Error.Validation("MyClass is not set."));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(my);
    }

    [Fact]
    public async Task Convert_task_nullable_class_to_result_fail()
    {
        // Arrange
        Maybe<MyClass> my = default;
        var myClassTask = Task.FromResult(my);

        // Act
        var result = await myClassTask.ToResultAsync(Error.Validation("MyClass is not set."));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("MyClass is not set."));
    }

    [Fact]
    public async Task Convert_valuetask_nullable_class_to_result_pass()
    {
        // Arrange
        MyClass my = new();
        Maybe<MyClass> maybeMy = my;

        var myClassTask = ValueTask.FromResult(maybeMy);

        // Act
        var result = await myClassTask.ToResultAsync(Error.Validation("MyClass is not set."));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(my);
    }

    [Fact]
    public async Task Convert_valuetask_nullable_class_to_result_fail()
    {
        // Arrange
        Maybe<MyClass> my = null;
        var myClassTask = ValueTask.FromResult(my);

        // Act
        var result = await myClassTask.ToResultAsync(Error.Validation("MyClass is not set."));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("MyClass is not set."));
    }

    // async struct
    [Fact]
    public async Task Convert_task_nullable_struct_to_result_pass()
    {
        // Arrange
        var date = DateTime.Now;
        Maybe<DateTime> my = date;
        var myClassTask = Task.FromResult(my);

        // Act
        var result = await myClassTask.ToResultAsync(Error.Validation("Date is not set."));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(date);
    }

    [Fact]
    public async Task Convert_task_nullable_struct_to_result_fail()
    {
        // Arrange
        Maybe<DateTime> my = default;
        var myClassTask = Task.FromResult(my);

        // Act
        var result = await myClassTask.ToResultAsync(Error.Validation("Date is not set."));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("Date is not set."));
    }

    [Fact]
    public async Task Convert_valuetask_nullable_struct_to_result_pass()
    {
        // Arrange
        var date = DateTime.Now;
        Maybe<DateTime> my = date;
        var myClassTask = ValueTask.FromResult(my);

        // Act
        var result = await myClassTask.ToResultAsync(Error.Validation("Date is not set."));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(date);
    }

    [Fact]
    public async Task Convert_valuetask_nullable_struct_to_result_fail()
    {
        // Arrange
        Maybe<MyClass> my = null;
        var myClassTask = ValueTask.FromResult(my);

        // Act
        var result = await myClassTask.ToResultAsync(Error.Validation("Date is not set."));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("Date is not set."));
    }
}

