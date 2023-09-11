namespace RailwayOrientedProgramming.Tests.Results.Extensions;

using FunctionalDDD.RailwayOrientedProgramming;

public class FinallyTests
{

    [Fact]
    public void Finally_executes_action_on_result()
    {
        // Arrange
        var result = Result.Success("Success");
        var actionExecuted = false;

        // Act
        var returned = result.Finally(Callback);

        // Assert
        actionExecuted.Should().BeTrue();

        string Callback(Result<string> result)
        {
            actionExecuted = true;
            result.IsSuccess.Should().BeTrue();
            return "Called";
        }
    }

    [Fact]
    public void Finally_executes_Okay()
    {
        // Arrange
        var result = Result.Success("Success");
        var okayActionExecuted = false;
        var errorActionExecuted = false;

        // Act
        var returned = result.Finally(OkayCallback, ErrorCallback);

        // Assert
        okayActionExecuted.Should().BeTrue();
        errorActionExecuted.Should().BeFalse();

        string OkayCallback(string str)
        {
            okayActionExecuted = true;
            return "Okay";
        }

        string ErrorCallback(Error e)
        {
            errorActionExecuted = true;
            return "Error";
        }
    }


    [Fact]
    public void Finally_executes_Error()
    {
        // Arrange
        var result = Result.Failure<string>(Error.Unexpected("Something is wrong."));
        var okayActionExecuted = false;
        var errorActionExecuted = false;

        // Act
        var returned = result.Finally(OkayCallback, ErrorCallback);

        // Assert
        okayActionExecuted.Should().BeFalse();
        errorActionExecuted.Should().BeTrue();

        string OkayCallback(string str)
        {
            okayActionExecuted = true;
            return "Okay";
        }

        string ErrorCallback(Error e)
        {
            errorActionExecuted = true;
            return "Error";
        }
    }

    [Fact]
    public async Task FinallyAsync_Task_executes_action_on_result()
    {
        // Arrange
        var result = Task.FromResult(Result.Success("Success"));
        var actionExecuted = false;

        // Act
        var returned = await result.FinallyAsync(Callback);

        // Assert
        actionExecuted.Should().BeTrue();

        string Callback(Result<string> result)
        {
            actionExecuted = true;
            result.IsSuccess.Should().BeTrue();
            return "Called";
        }
    }

    [Fact]
    public async Task FinallyAsync_Task_executes_Okay()
    {
        // Arrange
        var result = Task.FromResult(Result.Success("Success"));
        var okayActionExecuted = false;
        var errorActionExecuted = false;

        // Act
        var returned = await result.FinallyAsync(OkayCallback, ErrorCallback);

        // Assert
        okayActionExecuted.Should().BeTrue();
        errorActionExecuted.Should().BeFalse();

        string OkayCallback(string str)
        {
            okayActionExecuted = true;
            return "Okay";
        }

        string ErrorCallback(Error e)
        {
            errorActionExecuted = true;
            return "Error";
        }
    }


    [Fact]
    public async Task FinallyAsync_Task_executes_Error()
    {
        // Arrange
        var result = Task.FromResult(Result.Failure<string>(Error.Unexpected("Something is wrong.")));
        var okayActionExecuted = false;
        var errorActionExecuted = false;

        // Act
        var returned = await result.FinallyAsync(OkayCallback, ErrorCallback);

        // Assert
        okayActionExecuted.Should().BeFalse();
        errorActionExecuted.Should().BeTrue();

        string OkayCallback(string str)
        {
            okayActionExecuted = true;
            return "Okay";
        }

        string ErrorCallback(Error e)
        {
            errorActionExecuted = true;
            return "Error";
        }
    }

    [Fact]
    public async Task FinallyAsync_ValueTask_executes_action_on_result()
    {
        // Arrange
        var result = ValueTask.FromResult(Result.Success("Success"));
        var actionExecuted = false;

        // Act
        var returned = await result.FinallyAsync(Callback);

        // Assert
        actionExecuted.Should().BeTrue();

        string Callback(Result<string> result)
        {
            actionExecuted = true;
            result.IsSuccess.Should().BeTrue();
            return "Called";
        }
    }

    [Fact]
    public async Task FinallyAsync_ValueTask_executes_Okay()
    {
        // Arrange
        var result = ValueTask.FromResult(Result.Success("Success"));
        var okayActionExecuted = false;
        var errorActionExecuted = false;

        // Act
        var returned = await result.FinallyAsync(OkayCallback, ErrorCallback);

        // Assert
        okayActionExecuted.Should().BeTrue();
        errorActionExecuted.Should().BeFalse();

        string OkayCallback(string str)
        {
            okayActionExecuted = true;
            return "Okay";
        }

        string ErrorCallback(Error e)
        {
            errorActionExecuted = true;
            return "Error";
        }
    }


    [Fact]
    public async Task FinallyAsync_ValueTask_executes_Error()
    {
        // Arrange
        var result = ValueTask.FromResult(Result.Failure<string>(Error.Unexpected("Something is wrong.")));
        var okayActionExecuted = false;
        var errorActionExecuted = false;

        // Act
        var returned = await result.FinallyAsync(OkayCallback, ErrorCallback);

        // Assert
        okayActionExecuted.Should().BeFalse();
        errorActionExecuted.Should().BeTrue();

        string OkayCallback(string str)
        {
            okayActionExecuted = true;
            return "Okay";
        }

        string ErrorCallback(Error e)
        {
            errorActionExecuted = true;
            return "Error";
        }
    }
}
