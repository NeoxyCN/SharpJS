using System;
using Xunit;
using SharpJS.Core;

namespace SharpJS.Tests
{
    public class JsRuntimeTests
    {
        [Fact]
        public void JsRuntime_CanBeCreated()
        {
            // Arrange & Act
            using var runtime = new JsRuntime();

            // Assert
            Assert.NotNull(runtime);
        }

        [Fact]
        public void JsRuntime_CanEvaluateSimpleExpression()
        {
            // Arrange
            using var runtime = new JsRuntime();

            // Act
            var result = runtime.Eval<int>("1 + 1");

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public void JsRuntime_CanEvaluateStringExpression()
        {
            // Arrange
            using var runtime = new JsRuntime();

            // Act
            var result = runtime.Eval<string>("'Hello ' + 'World'");

            // Assert
            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void JsRuntime_CanRegisterAndUseObject()
        {
            // Arrange
            using var runtime = new JsRuntime();
            var testObj = new TestApiClass { Value = 42 };

            // Act
            runtime.RegisterObject("testApi", testObj);
            var result = runtime.Eval<int>("testApi.Value");

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public void JsRuntime_CanCallCSharpMethod()
        {
            // Arrange
            using var runtime = new JsRuntime();
            var testObj = new TestApiClass { Value = 10 };

            // Act
            runtime.RegisterObject("testApi", testObj);
            var result = runtime.Eval<int>("testApi.Add(5)");

            // Assert
            Assert.Equal(15, result);
        }

        [Fact]
        public void JsRuntime_ThrowsWhenDisposed()
        {
            // Arrange
            var runtime = new JsRuntime();
            runtime.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => runtime.Eval<int>("1 + 1"));
        }

        public class TestApiClass
        {
            public int Value { get; set; }

            public int Add(int amount)
            {
                return Value + amount;
            }
        }
    }
}
