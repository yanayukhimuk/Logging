using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrainstormSessions.Api;
using BrainstormSessions.Controllers;
using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.Core.Model;
using Castle.Core.Configuration;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Core;
using Xunit;

namespace BrainstormSessions.Test.UnitTests
{
    public class LoggingTests : IDisposable
    {
        private Mock <ILogger<IdeasController>> _loggerIdeaMock;
        private Mock<ILogger<HomeController>> _loggerHomeMock;
        private Mock<ILogger<SessionController>> _loggerSessionMock;

        public LoggingTests()
        {
            _loggerIdeaMock = new Mock<ILogger<IdeasController>>();
            _loggerHomeMock = new Mock<ILogger<HomeController>>();
            _loggerSessionMock = new Mock<ILogger<SessionController>>();
        }

        public void Dispose()
        {
            _loggerHomeMock.Invocations.Clear();
            _loggerIdeaMock.Invocations.Clear();
            _loggerSessionMock.Invocations.Clear();
        }

        [Fact]
        public async Task HomeController_Index_LogInfoMessages()
        {
            // Arrange
            _loggerHomeMock.Invocations.Clear();
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            mockRepo.Setup(repo => repo.ListAsync())
                .ReturnsAsync(GetTestSessions());
            var controller = new HomeController(mockRepo.Object, _loggerHomeMock.Object);

            // Act
            var result = await controller.Index();

            // Assert
            CheckLoggerWasCalled(_loggerHomeMock, "Session list is executed", LogLevel.Information);
        }

        [Fact]
        public async Task HomeController_IndexPost_LogWarningMessage_WhenModelStateIsInvalid()
        {
            // Arrange
            _loggerHomeMock.Invocations.Clear();
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            mockRepo.Setup(repo => repo.ListAsync())
                .ReturnsAsync(GetTestSessions());
            var controller = new HomeController(mockRepo.Object, _loggerHomeMock.Object);
            controller.ModelState.AddModelError("SessionName", "Required");
            var newSession = new HomeController.NewSessionModel();

            // Act
            var result = await controller.Index(newSession);

            // Assert
            CheckLoggerWasCalled(_loggerHomeMock, "Expected Warn messages in the logs", LogLevel.Warning);
        }

        [Fact]
        public async Task IdeasController_CreateActionResult_LogErrorMessage_WhenModelStateIsInvalid()
        {
            // Arrange & Act
            _loggerIdeaMock.Invocations.Clear();
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            var logger = _loggerIdeaMock;  
            var controller = new IdeasController(mockRepo.Object, _loggerIdeaMock.Object);
            controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await controller.CreateActionResult(model: null);

            // Assert
            CheckLoggerWasCalled(_loggerIdeaMock, "Expected Error messages in the logs", LogLevel.Error);
        }

        [Fact]
        public async Task SessionController_Index_LogDebugMessages()
        {
            // Arrange
            _loggerSessionMock.Invocations.Clear();
            int testSessionId = 1;
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            mockRepo.Setup(repo => repo.GetByIdAsync(testSessionId))
                .ReturnsAsync(GetTestSessions().FirstOrDefault(
                    s => s.Id == testSessionId));
            var controller = new SessionController(mockRepo.Object, _loggerSessionMock.Object);

            // Act
            var result = await controller.Index(testSessionId);

            // Assert
            CheckLoggerWasCalled(_loggerSessionMock, "Expected message", LogLevel.Debug);
        }

        private List<BrainstormSession> GetTestSessions()
        {
            var sessions = new List<BrainstormSession>();
            sessions.Add(new BrainstormSession()
            {
                DateCreated = new DateTime(2016, 7, 2),
                Id = 1,
                Name = "Test One"
            });
            sessions.Add(new BrainstormSession()
            {
                DateCreated = new DateTime(2016, 7, 1),
                Id = 2,
                Name = "Test Two"
            });
            return sessions;
        }

        private void CheckLoggerWasCalled<T>(Mock<ILogger<T>> loggerMock, string message, LogLevel level)
        {
            loggerMock.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => string.Equals(message, o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
