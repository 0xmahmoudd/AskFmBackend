using AskFm.BLL.DTO;

namespace AskFm.BLL.Services;

public interface IThreadService
{
    public Task<ServiceResult<ThreadResponseDto>> AddThread(int askerId, CreateThreadDto createThreadDto);
    public Task<ServiceResult<ThreadResponseDto>> GetThreadById(int id);
    public Task<ServiceResult<PagedResponseDto<ThreadResponseDto>>> GetAllThreads(int askedId, int page, int pageSize);
    public Task<ServiceResult<ThreadResponseDto>> AnswerThread(int threadId, int userId, AnswerThreadDto answerDto);
    public Task<ServiceResult<PagedResponseDto<ThreadResponseDto>>> GetThreads(int page, int pageSize);
    public Task<ServiceResult<bool>> DeleteThread(int threadId, int userId);
    public Task<ServiceResult<PagedResponseDto<ThreadResponseDto>>> GetFeed(int userId, int page, int pageSize);
    public Task<ServiceResult<bool>> SaveThread(int threadId, int userId);
    public Task<ServiceResult<bool>> UnsaveThread(int threadId, int userId);
    public Task<ServiceResult<PagedResponseDto<ThreadResponseDto>>> GetSavedThreads(int userId, int page, int pageSize);
}
