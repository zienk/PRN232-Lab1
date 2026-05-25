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
    public class SemesterService : ISemesterService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SemesterService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResponseModel<object>> GetSemestersAsync(string? search, string? sort, int page, int size, string? fields, string? expand)
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
                (System.Linq.Expressions.Expression<Func<Semester, bool>>)(s =>
                    s.SemesterName.Contains(parameters.Search));

            var result = await _unitOfWork.Semesters.GetAllAsync(parameters, searchPredicate);

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

        public async Task<ResponseModel<SemesterResponseModel>> GetSemesterByIdAsync(int id)
        {
            var entity = await _unitOfWork.Semesters.GetByIdAsync(id, "courses");
            if (entity == null)
            {
                return ResponseModel<SemesterResponseModel>.ErrorResponse($"Semester with id {id} not found");
            }
            return ResponseModel<SemesterResponseModel>.SuccessResponse(MapToResponseModel(entity));
        }

        public async Task<ResponseModel<SemesterResponseModel>> CreateSemesterAsync(SemesterRequestModel model)
        {
            var entity = new Semester
            {
                SemesterName = model.SemesterName,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };
            var created = await _unitOfWork.Semesters.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var fullEntity = await _unitOfWork.Semesters.GetByIdAsync(created.SemesterId, "courses");
            return ResponseModel<SemesterResponseModel>.SuccessResponse(MapToResponseModel(fullEntity ?? created), "Semester created successfully");
        }

        public async Task<ResponseModel<SemesterResponseModel>> UpdateSemesterAsync(int id, SemesterRequestModel model)
        {
            if (!await _unitOfWork.Semesters.ExistsAsync(id))
            {
                return ResponseModel<SemesterResponseModel>.ErrorResponse($"Semester with id {id} not found");
            }

            var entity = new Semester
            {
                SemesterId = id,
                SemesterName = model.SemesterName,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };

            var updated = await _unitOfWork.Semesters.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var fullEntity = await _unitOfWork.Semesters.GetByIdAsync(id, "courses");
            return ResponseModel<SemesterResponseModel>.SuccessResponse(MapToResponseModel(fullEntity ?? updated), "Semester updated successfully");
        }

        public async Task<ResponseModel<bool>> DeleteSemesterAsync(int id)
        {
            if (!await _unitOfWork.Semesters.ExistsAsync(id))
            {
                return ResponseModel<bool>.ErrorResponse($"Semester with id {id} not found");
            }

            var deleted = await _unitOfWork.Semesters.DeleteAsync(id);
            if (deleted)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return ResponseModel<bool>.SuccessResponse(deleted, "Semester deleted successfully");
        }

        private static SemesterResponseModel MapToResponseModel(Semester entity) => new()
        {
            SemesterId = entity.SemesterId,
            SemesterName = entity.SemesterName,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
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
