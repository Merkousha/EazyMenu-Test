using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Interfaces.Menus;
using EazyMenu.Application.Features.Menus.Commands.PublishMenu;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Domain.Aggregates.Menus;
using EazyMenu.Domain.ValueObjects;
using Moq;
using Xunit;

namespace EazyMenu.UnitTests.Application.Menus.Commands;

public sealed class PublishMenuCommandHandlerTests
{
    private readonly Mock<IMenuRepository> _menuRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IMenuPublicationWriter> _publicationWriter = new();
    private readonly Mock<IMenuRealtimeNotifier> _realtimeNotifier = new();

    [Fact]
    public async Task HandleAsync_WhenMenuExists_PublishesAndPersistsSnapshot()
    {
        // Arrange
        var tenantId = TenantId.New();
        var menu = Menu.Create(
            tenantId,
            LocalizedText.FromDictionary(new Dictionary<string, string>
            {
                [LocalizedText.DefaultCulture] = "منوی تست"
            }));
        menu.AddCategory(LocalizedText.FromDictionary(new Dictionary<string, string>
        {
            [LocalizedText.DefaultCulture] = "غذاها"
        }));

        _menuRepository
            .Setup(repository => repository.GetByIdAsync(tenantId, menu.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(menu);
        _menuRepository
            .Setup(repository => repository.UpdateAsync(menu, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publicationWriter
            .Setup(writer => writer.SaveAsync(It.IsAny<PublishedMenuDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _realtimeNotifier
            .Setup(notifier => notifier.PublishMenuChangedAsync(It.IsAny<MenuRealtimeNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new PublishMenuCommandHandler(
            _menuRepository.Object,
            _unitOfWork.Object,
            _publicationWriter.Object,
            _realtimeNotifier.Object);

        // Act
        var version = await handler.HandleAsync(
            new PublishMenuCommand(tenantId.Value, menu.Id.Value),
            CancellationToken.None);

        // Assert
        Assert.Equal(menu.PublishedVersion, version);
        Assert.Equal(1, version);

        _publicationWriter.Verify(writer => writer.SaveAsync(It.IsAny<PublishedMenuDto>(), It.IsAny<CancellationToken>()), Times.Once);
        _realtimeNotifier.Verify(notifier => notifier.PublishMenuChangedAsync(It.IsAny<MenuRealtimeNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        _menuRepository.Verify(repository => repository.UpdateAsync(menu, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
