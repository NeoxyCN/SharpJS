using System;
using System.IO;
using Xunit;
using SharpJS.Core;

namespace SharpJS.Tests
{
    public class GameApiTests
    {
        [Fact]
        public void GameApi_CanLog()
        {
            // Arrange
            var api = new GameApi();
            var output = new StringWriter();
            Console.SetOut(output);

            // Act
            api.Log("Test message");

            // Assert
            Assert.Contains("[MOD] Test message", output.ToString());
        }

        [Fact]
        public void GameApi_CanSetAndGetState()
        {
            // Arrange
            var api = new GameApi();

            // Act
            api.SetState("key1", "value1");
            api.SetState("key2", 42);
            var result1 = api.GetState("key1");
            var result2 = api.GetState("key2");

            // Assert
            Assert.Equal("value1", result1);
            Assert.Equal(42, result2);
        }

        [Fact]
        public void GameApi_ReturnsNullForMissingState()
        {
            // Arrange
            var api = new GameApi();

            // Act
            var result = api.GetState("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GameApi_CanRegisterAndTriggerEvents()
        {
            // Arrange
            var api = new GameApi();
            var eventFired = false;
            var eventData = "";

            api.On("test_event", (data) =>
            {
                eventFired = true;
                eventData = data;
            });

            // Act
            api.Emit("test_event", "test data");

            // Assert
            Assert.True(eventFired);
            Assert.Equal("test data", eventData);
        }

        [Fact]
        public void GameApi_CanSpawnEntity()
        {
            // Arrange
            var api = new GameApi();

            // Act
            var entityId = api.SpawnEntity("test_entity", 100, 200);

            // Assert
            Assert.NotNull(entityId);
            Assert.NotEmpty(entityId);
        }

        [Fact]
        public void GameApi_GetTimeReturnsValidTime()
        {
            // Arrange
            var api = new GameApi();

            // Act
            var time = api.GetTime();

            // Assert
            Assert.True(time > 0);
            Assert.True(time < 86400); // Less than 24 hours in seconds
        }
    }
}
