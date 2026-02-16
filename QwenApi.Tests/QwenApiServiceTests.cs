using Moq;
using QwenApi.Apis;
using QwenApi.Services;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System;

namespace QwenApi.Tests
{
    public class QwenApiServiceTests
    {
        private readonly IQwenApiService _qwenApiService;

        public QwenApiServiceTests()
        {
            // 由于QwenApiService目前直接调用静态方法，这里我们直接使用实际的实现
            // 在实际项目中，我们应该进一步重构，使用依赖注入来隔离外部依赖
            _qwenApiService = new QwenApiService();
        }

        [Fact]
        public async Task GetModelsAsync_WhenCalled_ReturnsModelList()
        {
            // 注意：这个测试会实际调用API，需要配置正确的认证信息
            // 在实际项目中，我们应该使用Moq来模拟API调用
            var models = await _qwenApiService.GetModelsAsync();
            
            // 由于我们没有配置认证信息，这个测试可能会失败
            // 这里只是演示测试结构
            Assert.NotNull(models);
        }

        [Fact]
        public async Task GetSessionsAsync_WhenCalled_ReturnsSessionList()
        {
            // 注意：这个测试会实际调用API，需要配置正确的认证信息
            // 在实际项目中，我们应该使用Moq来模拟API调用
            var sessions = await _qwenApiService.GetSessionsAsync();
            
            // 由于我们没有配置认证信息，这个测试可能会失败
            // 这里只是演示测试结构
            Assert.NotNull(sessions);
        }
    }
}
