using AutoMapper;
using LibraryAPI.DatabaseAccess.CommentsRepository;
using LibraryAPI.DTOs;

namespace LibraryAPI.UseCases.Comments.GetById
{
    public class CommentGetByIdUseCase : ICommentGetByIdUseCase
    {
        private readonly ICommentsRepository _commentsRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentGetByIdUseCase> _logger;

        public CommentGetByIdUseCase(ICommentsRepository commentsRepository, IMapper mapper, ILogger<CommentGetByIdUseCase> logger)
        {
            _commentsRepository = commentsRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CommentDTO?> Run(Guid commentId)
        {
            var comment = await _commentsRepository.GetById(commentId);
            if (comment is null)
            {
                _logger.LogWarning($"Comment with ID {commentId} was not found.");
                return null;
            }

            var commentDTO = _mapper.Map<CommentDTO>(comment);
            return commentDTO;
        }
    }
}
