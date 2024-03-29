using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation.Results;
using TinyCqrs.Classes;
using TinyCqrs.Interfaces;
using TinyCqrs.XUnitTests.Implementation;
using Microsoft.Extensions.DependencyInjection;
using TinyCqrs.Enums;
using Xunit;

namespace TinyCqrs.XUnitTests
{
    public class CqrsTests
    {
        private static int CountHandlers => 12;

        [Fact]
        public void CmdResult_AddError_Ok()
        {
            const string type = "Source";
            const string message = "New error message";
            
            var basic = new CmdResult<object>(type);
            basic.AddIssue(message);
            basic.AddIssue("New warning message", IssueType.Warning);

            basic.Type.Should().Be(type);
            basic.Issues.Count.Should().Be(2);
            basic.Issues[0].Message.Should().Be(message);

            basic.Success.Should().BeFalse();
        }

        [Fact]
        public void ConfigureCqrsObjects_ByTypeAssembly_Ok()
        {
            var services = new MockServiceCollection();
            services.ConfigureCqrsObjects(typeof(CoreCommandHandler));

            services.AddedItems.Count.Should().Be(CountHandlers, "decorators do not get included in the added items.");
        }
        
        [Fact]
        public void ConfigureCqrsObjects_ByTypeParameter_Ok()
        {
            var services = new MockServiceCollection();
            services.ConfigureCqrsObjects<CoreCommandHandler>();

            services.AddedItems.Count.Should().Be(CountHandlers, "decorators do not get included in the added items.");
        }
        
        [Fact]
        public void ConfigureCqrsObjects_ByExecutingAssembly_Ok()
        {
            var services = new MockServiceCollection();
            services.ConfigureCqrsObjects();

            services.AddedItems.Count.Should().Be(0, "RhLibs.Cqrs has no implementation types."); 
        }
        
        [Fact]
        public void ConfigureCqrsObjects_BySpecifiedAssembly_Ok()
        {
            var services = new MockServiceCollection();
            services.ConfigureCqrsObjects(Assembly.GetExecutingAssembly());

            services.AddedItems.Count.Should().Be(CountHandlers, "decorators do not get included in the added items.");
        }

        [Fact]
        public void TestForDuplicateConfigurations()
        {
            var services = new CqrsConfigurationTester(typeof(CoreCommandHandler));
            services.HasDuplicateConfigurations().Should().BeFalse();
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public async Task CmdHandlerAsync_TryCatch_Ok(bool throwError, bool isSuccessful)
        {
            var cmd = new MockCoreCommand(throwError);
            var handler = new CoreCommandHandlerAsync();

            var result = await handler.Execute(cmd);
            result.Success.Should().Be(isSuccessful);
        }

        [Fact]
        public void HandlerRegistrations_Ok()
        {
            var sc = new ServiceCollection();
            sc.ConfigureCqrsObjects(typeof(CoreCommandHandler));
            
            var provider = sc.BuildServiceProvider();

            var decoratedService = provider.GetService<ICmdHandler<MockCoreCommand>>();
            decoratedService.Should().BeOfType<DecoratorHandler>();

            var parent = (DecoratorHandler) decoratedService;

            if (parent == null)
                throw new Exception("Parent should not be null");

            parent.ChildHandler.Should().BeOfType<CoreCommandHandler>();

            var result = parent.Execute(new MockCoreCommand(false));
            result.Type.Should().Be("Core command handler");
        }
        
        [Theory]
        [InlineData(typeof(IQueryHandler<List<string>>), typeof(QueryHandler1))]
        [InlineData(typeof(IQueryHandler<Query, List<string>>), typeof(QueryHandler2))]
        
        [InlineData(typeof(IQueryHandlerAsync<List<string>>), typeof(QueryHandlerAsync1))]
        [InlineData(typeof(IQueryHandlerAsync<Query, List<string>>), typeof(QueryHandlerAsync2))]
        
        [InlineData(typeof(ICmdHandler<Cmd>), typeof(CmdHandler1))]
        [InlineData(typeof(ICmdHandler<Cmd, CustomResult>), typeof(CmdHandler2))]
        
        [InlineData(typeof(ICmdHandlerAsync<Cmd>), typeof(CmdHandlerAsync1))]
        [InlineData(typeof(ICmdHandlerAsync<Cmd, CustomResult>), typeof(CmdHandlerAsync2))]
        public void HandlerRegistrar_Types_Ok(Type serviceType, Type expectedType)
        {
            var sc = new ServiceCollection();
            sc.ConfigureCqrsObjects(typeof(QueryHandler1));
            
            var provider = sc.BuildServiceProvider();
            var service = provider.GetService(serviceType);

            service.GetType().Should().Be(expectedType);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task NextOnSuccessDecoratorAsync_Ok(bool throwError, bool isSuccessful)
        {
            var data = new MockCoreCommand(throwError);
            var handler = new CoreCommandHandlerAsync();
            var decorator = new DecoratorHandlerAsync(handler);

            var result = await decorator.Execute(data);
            result.Success.Should().Be(isSuccessful);
        }
        
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void NextOnSuccessDecorator_Ok(bool throwError, bool isSuccessful)
        {
            var data = new MockCoreCommand(throwError);
            var handler = new CoreCommandHandler();
            var decorator = new DecoratorHandler(handler);

            var result = decorator.Execute(data);
            result.Success.Should().Be(isSuccessful);
        }

        [Fact]
        public void GetPipeline_Ok()
        {
            var tester = new CqrsConfigurationTester(typeof(DecoratorHandlerAsync));
            
            tester.HandlerPipelineEquals(typeof(ICmdHandlerAsync<MockCoreCommand>), new[]
            {
                typeof(DecoratorHandlerAsync),
                typeof(CoreCommandHandlerAsync)
                
            }).Should().BeTrue();
        }

        [Fact]
        public void ConfigurationTester_ConstructByAssy_Ok()
        {
            var tester = new CqrsConfigurationTester(Assembly.GetExecutingAssembly());
            tester.IsAssemblyNull().Should().BeFalse();
        }
    }
}