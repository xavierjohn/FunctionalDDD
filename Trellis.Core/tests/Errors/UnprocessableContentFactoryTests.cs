namespace Trellis.Core.Tests.Errors;

using Trellis.Testing;

/// <summary>
/// Tests for the <see cref="Error.InvalidInput.ForField(string, string, string?)"/>,
/// <see cref="Error.InvalidInput.ForField(InputPointer, string, string?)"/>, and
/// <see cref="Error.InvalidInput.ForRule(string, string?)"/> static factories.
/// These exist to remove the verbose boilerplate of constructing single-violation 422 errors,
/// which are by far the most common shape (every primitive <c>TryCreate</c>, every value-object
/// invariant, every <c>RequiredEnum</c> failure produces one).
/// </summary>
public class UnprocessableContentFactoryTests
{
    // ── ForField(string, string, string?) ───────────────────────────────────

    [Fact]
    public void ForField_with_property_name_creates_single_field_violation()
    {
        var error = Error.InvalidInput.ForField("email", "invalid_format");

        error.Fields.Length.Should().Be(1);
        error.Fields[0].Field.Should().Be(InputPointer.ForProperty("email"));
        error.Fields[0].ReasonCode.Should().Be("invalid_format");
        error.Fields[0].Detail.Should().BeNull();
    }

    [Fact]
    public void ForField_with_property_name_and_detail_propagates_detail()
    {
        var error = Error.InvalidInput.ForField("email", "invalid_format", "must contain @");

        error.Fields.Length.Should().Be(1);
        error.Fields[0].Detail.Should().Be("must contain @");
    }

    [Fact]
    public void ForField_with_property_name_produces_empty_rules()
    {
        var error = Error.InvalidInput.ForField("name", "required");

        error.Rules.Length.Should().Be(0);
    }

    [Fact]
    public void ForField_escapes_property_name_via_InputPointer_ForProperty()
    {
        var error = Error.InvalidInput.ForField("a/b", "invalid");

        // ForProperty escapes '/' as "~1" per RFC 6901
        error.Fields[0].Field.Path.Should().Be("/a~1b");
    }

    [Fact]
    public void ForField_with_null_or_empty_property_falls_back_to_root_pointer()
    {
        var error = Error.InvalidInput.ForField(string.Empty, "object_invalid");

        error.Fields[0].Field.Should().Be(InputPointer.Root);
    }

    // ── ForField(InputPointer, string, string?) ──────────────────────────────

    [Fact]
    public void ForField_with_pointer_uses_pointer_directly()
    {
        var pointer = new InputPointer("/items/0/quantity");
        var error = Error.InvalidInput.ForField(pointer, "out_of_range");

        error.Fields.Length.Should().Be(1);
        error.Fields[0].Field.Should().Be(pointer);
        error.Fields[0].ReasonCode.Should().Be("out_of_range");
        error.Fields[0].Detail.Should().BeNull();
    }

    [Fact]
    public void ForField_with_pointer_and_detail_propagates_detail()
    {
        var pointer = new InputPointer("/items/0/quantity");
        var error = Error.InvalidInput.ForField(pointer, "out_of_range", "must be positive");

        error.Fields[0].Detail.Should().Be("must be positive");
    }

    [Fact]
    public void ForField_with_root_pointer_produces_object_level_violation()
    {
        var error = Error.InvalidInput.ForField(InputPointer.Root, "object_required");

        error.Fields[0].Field.Should().Be(InputPointer.Root);
    }

    // ── ForRule(string, string?) ─────────────────────────────────────────────

    [Fact]
    public void ForRule_creates_single_rule_violation_with_empty_fields()
    {
        var error = Error.InvalidInput.ForRule("passwords_must_match");

        error.Fields.Length.Should().Be(0);
        error.Rules.Length.Should().Be(1);
        error.Rules[0].ReasonCode.Should().Be("passwords_must_match");
        error.Rules[0].Detail.Should().BeNull();
    }

    [Fact]
    public void ForRule_with_detail_propagates_detail()
    {
        var error = Error.InvalidInput.ForRule("passwords_must_match", "Passwords do not match");

        error.Rules[0].Detail.Should().Be("Passwords do not match");
    }

    // ── Equality + Kind preserved ────────────────────────────────────────────

    [Fact]
    public void ForField_results_equal_manual_construction()
    {
        var fromFactory = Error.InvalidInput.ForField("email", "invalid_format", "must contain @");
        var manual = new Error.InvalidInput(EquatableArray.Create(
            new FieldViolation(InputPointer.ForProperty("email"), "invalid_format", Detail: "must contain @")));

        fromFactory.Equals(manual).Should().BeTrue();
        fromFactory.GetHashCode().Should().Be(manual.GetHashCode());
    }

    [Fact]
    public void ForRule_results_equal_manual_construction()
    {
        var fromFactory = Error.InvalidInput.ForRule("cancel_after_ship", "Cannot cancel after shipment");
        var manual = new Error.InvalidInput(
            EquatableArray<FieldViolation>.Empty,
            EquatableArray.Create(new RuleViolation("cancel_after_ship", Detail: "Cannot cancel after shipment")))
        { Detail = "Cannot cancel after shipment" };

        fromFactory.Equals(manual).Should().BeTrue();
        fromFactory.GetHashCode().Should().Be(manual.GetHashCode());
    }

    [Fact]
    public void Factory_results_have_correct_Kind()
    {
        Error.InvalidInput.ForField("x", "y").Kind.Should().Be("invalid-input");
        Error.InvalidInput.ForRule("x").Kind.Should().Be("invalid-input");
    }

    // ── Pluggability into Result ─────────────────────────────────────────────

    [Fact]
    public void ForField_can_be_used_as_failure_payload()
    {
        Result<int> result = Result.Fail<int>(Error.InvalidInput.ForField("age", "out_of_range", "must be >= 18"));

        result.IsFailure.Should().BeTrue();
        var err = result.UnwrapError();
        err.Should().BeOfType<Error.InvalidInput>();
        ((Error.InvalidInput)err).Fields[0].Detail.Should().Be("must be >= 18");
    }
}