using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Implementations
{
    public class SubjectService : ISubjectService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubjectService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResponseModel<object>> GetSubjectsAsync(string? search, string? sort, int page, int size, string? fields, string? expand)
        {
            var parameters = new QueryParameters
            {
                Search = search,
                Sort = sort,
                Page = page,
                Size = size,
                Fields = fields,
                Expand = expand
            };

            var searchPredicate = string.IsNullOrWhiteSpace(parameters.Search) ? null :
                (System.Linq.Expressions.Expression<Func<Subject, bool>>)(s =>
                    s.SubjectName.Contains(parameters.Search) ||
                    s.SubjectCode.Contains(parameters.Search));

            var result = await _unitOfWork.Subjects.GetAllAsync(parameters, searchPredicate);

            var responses = result.Items.Select(MapToResponseModel).ToList();
            var data = FieldSelector.SelectFieldsList(responses, parameters.Fields);

            var pagination = new PaginationMetadataModel
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };

            return PagedResponseModel<object>.SuccessResponse(data, pagination);
        }

        public async Task<ResponseModel<SubjectResponseModel>> GetSubjectByIdAsync(int id)
        {
            var entity = await _unitOfWork.Subjects.GetByIdAsync(id, "courses");
            if (entity == null)
            {
                return ResponseModel<SubjectResponseModel>.ErrorResponse($"Subject with id {id} not found");
            }
            return ResponseModel<SubjectResponseModel>.SuccessResponse(MapToResponseModel(entity));
        }

        public async Task<ResponseModel<SubjectResponseModel>> CreateSubjectAsync(SubjectRequestModel model)
        {
            var entity = new Subject
            {
                SubjectCode = model.SubjectCode,
                SubjectName = model.SubjectName,
                Credit = model.Credit
            };
            var created = await _unitOfWork.Subjects.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var fullEntity = await _unitOfWork.Subjects.GetByIdAsync(created.SubjectId, "courses");
            return ResponseModel<SubjectResponseModel>.SuccessResponse(MapToResponseModel(fullEntity ?? created), "Subject created successfully");
        }

        public async Task<ResponseModel<SubjectResponseModel>> UpdateSubjectAsync(int id, SubjectRequestModel model)
        {
            if (!await _unitOfWork.Subjects.ExistsAsync(id))
            {
                return ResponseModel<SubjectResponseModel>.ErrorResponse($"Subject with id {id} not found");
            }

            var entity = new Subject
            {
                SubjectId = id,
                SubjectCode = model.SubjectCode,
                SubjectName = model.SubjectName,
                Credit = model.Credit
            };

            var updated = await _unitOfWork.Subjects.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var fullEntity = await _unitOfWork.Subjects.GetByIdAsync(id, "courses");
            return ResponseModel<SubjectResponseModel>.SuccessResponse(MapToResponseModel(fullEntity ?? updated), "Subject updated successfully");
        }

        public async Task<ResponseModel<bool>> DeleteSubjectAsync(int id)
        {
            if (!await _unitOfWork.Subjects.ExistsAsync(id))
            {
                return ResponseModel<bool>.ErrorResponse($"Subject with id {id} not found");
            }

            var deleted = await _unitOfWork.Subjects.DeleteAsync(id);
            if (deleted)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return ResponseModel<bool>.SuccessResponse(deleted, "Subject deleted successfully");
        }

        private static SubjectResponseModel MapToResponseModel(Subject entity) => new()
        {
            SubjectId = entity.SubjectId,
            SubjectCode = entity.SubjectCode,
            SubjectName = entity.SubjectName,
            Credit = entity.Credit,
            Courses = entity.Courses?.Select(c => new CourseResponseModel
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                SemesterId = c.SemesterId,
                SubjectId = c.SubjectId
            }).ToList()
        };
    }
}
