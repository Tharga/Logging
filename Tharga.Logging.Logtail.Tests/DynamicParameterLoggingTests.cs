using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Tharga.Logging.Logtail.Tests;

public class DynamicParameterLoggingTests
{
    [Fact]
    public void LogException()
    {
        //Arrange
        var categoryName = "A";
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        hostEnvironment.EnvironmentName.Returns("A");
        hostEnvironment.ApplicationName.Returns("B");
        var configuration = Substitute.For<IConfiguration>();
        configuration.GetSection(Arg.Any<string>()).Returns((IConfigurationSection)null);
        var loggingDefaultData = Substitute.For<ILoggingDefaultData>();
        loggingDefaultData.GetData().Returns(new Dictionary<string, object> { { "A", "a" } });
        var sut = new LogtailLogger(hostEnvironment, configuration, loggingDefaultData, categoryName);

        //Act
        sut.Log(LogLevel.Information, null, "AAA {a}", "a1");
        sut.Log(LogLevel.Information, "AAA {a}", "a1");
    }

    [Theory]
    [InlineData("ABC123 [a: 1, b: 2]")]
    [InlineData("ABC123   [ a : 1 , b : 2] ")]
    [InlineData("ABC123[a: 1, b: 2]")]
    [InlineData("ABC123[a:1,b:2]")]
    public void Extract(string message)
    {
        //Arrange
        var logEntry = new LogEntry
        {
            Message = message,
            Exception = null,
            LogLevel = LogLevel.Information,
            Data = new LogData()
        };

        //Act
        var extract = LogtailLogger.Unpack(logEntry);

        //Assert
        extract.Message.Should().Be("ABC123");
        extract.Data.Should().HaveCount(2);
        extract.Data.First().Key.Should().Be("a");
        extract.Data.First().Value.Should().Be("1");
        extract.Data.Last().Key.Should().Be("b");
        extract.Data.Last().Value.Should().Be("2");
    }

    [Theory]
    [InlineData("ABC123 [")]
    [InlineData("ABC123 []")]
    [InlineData("ABC123 [a]")]
    [InlineData("ABC123 [a:]")]
    [InlineData("ABC123 [a: ]")]
    [InlineData("ABC123  [ a : ]  ")]
    [InlineData("ABC123[:]")]
    [InlineData("ABC123[ :]")]
    [InlineData("ABC123[:a]")]
    [InlineData("ABC123[:a,:,a:,:a]")]
    [InlineData("ABC123[::::]")]
    [InlineData("ABC123[,,,,]")]
    [InlineData("ABC123[:,a:,:1,]")]
    [InlineData("ABC123")]
    [InlineData("ABC123 ")]
    public void ExtractBro(string message)
    {
        //Arrange
        var logEntry = new LogEntry
        {
            Message = message,
            Exception = null,
            LogLevel = LogLevel.Information,
            Data = new LogData()
        };

        //Act
        var extract = LogtailLogger.Unpack(logEntry);

        //Assert
        extract.Message.Should().Be("ABC123");
        extract.Data.Should().HaveCount(0);
    }

    [Theory]
    [InlineData("ABC123")]
    [InlineData("ABC123 ")]
    [InlineData("ABC123 [a:1]")]
    public void AppendData(string message)
    {
        //Arrange
        var logEntry = new LogEntry
        {
            Message = message.AppendLogData(new Dictionary<string, object> { { "a", "1" } }),
            Exception = null,
            LogLevel = LogLevel.Information,
            Data = new LogData()
        };

        //Act
        var extract = LogtailLogger.Unpack(logEntry);

        //Assert
        extract.Message.Should().Be("ABC123");
        extract.Data.Should().HaveCount(1);
        extract.Data.Single().Key.Should().Be("a");
        extract.Data.Single().Value.Should().Be("1");
    }

    [Theory]
    [InlineData("ABC123 [", "a,1")]
    [InlineData("ABC123", null)]
    [InlineData("ABC123", "")]
    [InlineData("ABC123", "a,")]
    [InlineData("ABC123", ",1")]
    public void IgnoreData(string message, string data)
    {
        //Arrange
        var p = data?.Split(",");
        var dictionary = p != null ? new Dictionary<string, object> { { p.First(), p.Last() } } : null;
        var logEntry = new LogEntry
        {
            Message = message.AppendLogData(dictionary),
            Exception = null,
            LogLevel = LogLevel.Information,
            Data = new LogData()
        };

        //Act
        var extract = LogtailLogger.Unpack(logEntry);

        //Assert
        extract.Message.Should().Be("ABC123");
        extract.Data.Should().HaveCount(0);
    }
}