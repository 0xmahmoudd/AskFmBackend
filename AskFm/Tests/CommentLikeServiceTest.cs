using AskFm.BLL.Services;
using AskFm.DAL.Interfaces;
using AskFm.DAL.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using AskFm.BLL.DTO;
using AskFm.DAL;
using Microsoft.EntityFrameworkCore.Storage;


namespace Tests;

public class CommentLikeServiceTest
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<CommentLikeService>> _loggerMock;
    private readonly Mock<IRepository<Comment>> _mockCommentRepository;
    private readonly Mock<IRepository<ApplicationUser>> _mockUserRepository;
    private readonly Mock<IRepository<CommentLike>> _mockCommentLikeRepository;
    private readonly CommentLikeService _commentLikeService;
    private readonly Mock<IDbContextTransaction> _mockTransaction;
    
    public CommentLikeServiceTest()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCommentRepository = new Mock<IRepository<Comment>>();
        _mockCommentLikeRepository = new Mock<IRepository<CommentLike>>();
        _mockUserRepository = new Mock<IRepository<ApplicationUser>>();
        _loggerMock = new Mock<ILogger<CommentLikeService>>();
        _mockTransaction = new Mock<IDbContextTransaction>();
        _mockUnitOfWork.Setup(uow => uow.Comments).Returns(_mockCommentRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.CommentLikes).Returns(_mockCommentLikeRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).ReturnsAsync(_mockTransaction.Object);

        
        
        _commentLikeService = new CommentLikeService(_mockUnitOfWork.Object,  _loggerMock.Object);
    }
    
    [Fact]
    public async Task AddLike_WhenCommentIsNotDeleted_AddingNewLikeOnTheComment()
    {
        // Arrange 
        var commentId = 1;
        var userId = 5;
        var mockTransaction = new Mock<IDbContextTransaction>();
        
        var comment = new Comment 
        { 
            Id = commentId, 
            IsDeleted = false,
            Content = "Test comment"
        };
        
        var user = new ApplicationUser()
        {
            Id = userId,
            Comments = new List<Comment> { comment }
        };
        
        
        _mockCommentRepository.Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync(comment);
        
        
        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        // Act
        await _commentLikeService.AddLikeAsync(commentId, userId);
        
        // Assert
        _mockCommentLikeRepository.Verify(repo => repo.AddAsync(It.Is<CommentLike>(cl => 
            cl.CommentId == commentId && cl.UserId == userId)), Times.Once);
        
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task AddLike_WhenCommentNotFoundOrDeleted_ThrowException()
    {
        // Arrange
        var commentId = 1000;
        var userId = 5;
        var user = new ApplicationUser()
        {
            Name = "ziad"
        };
        _mockCommentRepository.Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync((Comment)null);
        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        var result = await _commentLikeService.AddLikeAsync(commentId, userId);
        
        // Assert
        Assert.False(result.success); 
        Assert.Contains($"Comment with id {commentId} not found", result.Errors);
        _mockCommentLikeRepository.Verify(repo => repo.AddAsync(It.IsAny<CommentLike>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Never);
        
    }
    
    
    [Fact]
    public async Task DeleteLike_WhenDeleteByAuthor_CommentIsDeleted()
    {
        var commentId = 1;
        var userId = 5;

        var commentLike = new CommentLike { CommentId = commentId, UserId = userId };

        _mockCommentLikeRepository.Setup(repo =>
                repo.FindAsync(
                    It.Is<Expression<Func<CommentLike, bool>>>(expr =>
                        ExpressionMatches(expr, userId, commentId)),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>()
                ))
            .Returns(Task.FromResult(commentLike));

        var likeCount = 1;

        ICollection<CommentLike> commentLikes = new List<CommentLike>();
        commentLikes.Add(commentLike);
        
        var comment = new Comment()
        {
            Id = commentId,
            Content = "Test comment",
            IsDeleted = false,
            CommentLikes = commentLikes,
            LikeCount = likeCount
        };
        
        _mockCommentRepository.Setup(repo => repo.GetByIdAsync(commentId))
            .Returns(Task.FromResult(comment));

        
        _mockCommentLikeRepository.Setup(repo => repo.RemoveAsync(commentLike))
            .Callback(() => commentLike.IsDeleted = true);
        
        
        // Act
        await _commentLikeService.DeleteLikeAsync(commentId, userId);
        
        
        
        // Assert 
        
        Assert.Empty(comment.CommentLikes);
        Assert.True(commentLike.IsDeleted);
        Assert.Equal(comment.LikeCount, likeCount - 1);
        _mockCommentLikeRepository.Verify(repo => repo.RemoveAsync(commentLike), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Once);

    }


    [Fact]
    public async Task GetLikesForComment_WhenCommentLikesIsNotEmpty_ReturnsCommentLikesList()
    {
        var commentId = 1;
        var userId = 4;
        

        ICollection<CommentLike> commentLikes = new List<CommentLike>()
        {
            { new CommentLike { CommentId = commentId, UserId = 4 }},
            { new CommentLike { CommentId = commentId, UserId = 5 } },
            { new CommentLike { CommentId = commentId, UserId = 6 } },
            { new CommentLike { CommentId = commentId, UserId = 8 } },
            { new CommentLike { CommentId = commentId, UserId = 9 } },
        };

        
        
        var comment = new Comment()
        {
            Id = commentId,
            Content = "Test comment",
            IsDeleted = false,
            CommentLikes = commentLikes
        };
        
        var expectedDtos = new List<CommentLikeDto>
        {
            new CommentLikeDto {CommentId = commentId, UserId = 4 },
            new CommentLikeDto {CommentId = commentId, UserId = 5 },
            new CommentLikeDto {CommentId = commentId, UserId = 6 },
            new CommentLikeDto {CommentId = commentId, UserId = 8 },
            new CommentLikeDto {CommentId = commentId, UserId = 9 }
        };
        
        _mockCommentRepository.Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync(comment);
        
        _mockCommentLikeRepository.Setup(repo => 
                repo.FindAllAsync(It.IsAny<Expression<Func<CommentLike, bool>>>(), It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(commentLikes);
        
        // Act
        var result = await _commentLikeService.GetLikesForCommentAsync(commentId);
        
        
        // Assert 
        Assert.True(result.success);
        Assert.NotNull(result.Data);
        Assert.Equal(5, result.Data.Count());

        Assert.Equivalent(expectedDtos, result.Data);
        
    }
    
    
    [Fact]
    public async Task GetLikesForComment_WhenCommentLikesIsEmpty_ReturnsEmptyList()
    {
        // Arrange
        var commentId = 1;
        var userId = 4;
        
        ICollection<CommentLike> commentLikes = new List<CommentLike>();
        
        var comment = new Comment()
        {
            Id = commentId,
            Content = "Test comment",
            IsDeleted = false,
            CommentLikes = commentLikes
        };
        
        _mockCommentRepository.Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync(comment);
        
        _mockCommentLikeRepository.Setup(repo => 
                repo.FindAllAsync(It.IsAny<Expression<Func<CommentLike, bool>>>(), It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(commentLikes);
        
        // Act
        var result = await _commentLikeService.GetLikesForCommentAsync(commentId);
        
        
        // Assert
        Assert.Empty(result.Data);
        
    }
    
    
    // helper
    private bool ExpressionMatches(Expression<Func<CommentLike, bool>> expr, int userId, int commentId)
    {
        var testItem = new CommentLike { UserId = userId, CommentId = commentId };
        var compiled = expr.Compile();
        return compiled(testItem);
    }

    
}