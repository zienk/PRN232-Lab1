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
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResponseModel<object>> GetCoursesAsync(string? search, string? sort, int page, int size, string? fields, string? expand)
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
                (System.Linq.Expressions.Expression<Func<Course, bool>>)(c =>
                    c.CourseName.Contains(parameters.Search));

            var result = await _unitOfWork.Courses.GetAllAsync(parameters, searchPredicate);

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

        public async Task<ResponseModel<CourseResponseModel>> GetCourseByIdAsync(int id)
        {
            var entity = await _unitOfWork.Courses.GetByIdAsync(id, "semester,subject");
            if (entity == null)
            {
                return ResponseModel<CourseResponseModel>.ErrorResponse($"Course with id {id} not found");
            }
            return ResponseModel<CourseResponseModel>.SuccessResponse(MapToResponseModel(entity));
        }

        public async Task<ResponseModel<CourseResponseModel>> CreateCourseAsync(CourseRequestModel model)
        {
            var entity = new Course
            {
                CourseName = model.CourseName,
                SemesterId = model.SemesterId,
                SubjectId = model.SubjectId
            };
            var created = await _unitOfWork.Courses.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var fullEntity = await _unitOfWork.Courses.GetByIdAsync(created.CourseId, "semester,subject");
            return ResponseModel<CourseResponseModel>.SuccessResponse(MapToResponseModel(fullEntity ?? created), "Course created successfully");
        }

        public async Task<ResponseModel<CourseResponseModel>> UpdateCourseAsync(int id, CourseRequestModel model)
        {
            if (!await _unitOfWork.Courses.ExistsAsync(id))
            {
                return ResponseModel<CourseResponseModel>.ErrorResponse($"Course with id {id} not found");
            }

            var entity = new Course
            {
                CourseId = id,
                CourseName = model.CourseName,
                SemesterId = model.SemesterId,
                SubjectId = model.SubjectId
            };

            var updated = await _unitOfWork.Courses.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var fullEntity = await _unitOfWork.Courses.GetByIdAsync(id, "semester,subject");
            return ResponseModel<CourseResponseModel>.SuccessResponse(MapToResponseModel(fullEntity ?? updated), "Course updated successfully");
        }

        public async Task<ResponseModel<bool>> DeleteCourseAsync(int id)
        {
            if (!await _unitOfWork.Courses.ExistsAsync(id))
            {
                return ResponseModel<bool>.ErrorResponse($"Course with id {id} not found");
            }

            var deleted = await _unitOfWork.Courses.DeleteAsync(id);
            if (deleted)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return ResponseModel<bool>.SuccessResponse(deleted, "Course deleted successfully");
        }

        private static CourseResponseModel MapToResponseModel(Course entity) => new()
        {
            CourseId = entity.CourseId,
            CourseName = entity.CourseName,
            SemesterId = entity.SemesterId,
            SubjectId = entity.SubjectId,
            Semester = entity.Semester != null ? new SemesterResponseModel
            {
                SemesterId = entity.Semester.SemesterId,
                SemesterName = entity.Semester.SemesterName,
                StartDate = entity.Semester.StartDate,
                EndDate = entity.Semester.EndDate
            } : null,
            Subject = entity.Subject != null ? new SubjectResponseModel
            {
                SubjectId = entity.Subject.SubjectId,
                SubjectCode = entity.Subject.SubjectCode,
                SubjectName = entity.Subject.SubjectName,
                Credit = entity.Subject.Credit
            } : null
        };
    }
}
