using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Implementations
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EnrollmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResponseModel<EnrollmentResponseModel>> GetEnrollmentsAsync(int? studentId, int? courseId, string? search, string? sort, int page, int size, string? fields, string? expand)
        {
            var parameters = new QueryParameters
            {
                Search = search,
                Sort = sort,
                Page = page,
                Size = size,
                Fields = fields,
                Expand = expand ?? "student,course"
            };

            System.Linq.Expressions.Expression<Func<Enrollment, bool>>? searchPredicate = null;
            if (studentId.HasValue && courseId.HasValue)
            {
                searchPredicate = e => e.StudentId == studentId.Value && e.CourseId == courseId.Value;
            }
            else if (studentId.HasValue)
            {
                searchPredicate = e => e.StudentId == studentId.Value;
            }
            else if (courseId.HasValue)
            {
                searchPredicate = e => e.CourseId == courseId.Value;
            }

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var keyword = parameters.Search;
                if (searchPredicate != null)
                {
                    var basePredicate = searchPredicate;
                    searchPredicate = e => e.Status.Contains(keyword) && (courseId.HasValue ? e.CourseId == courseId.Value : true) && (studentId.HasValue ? e.StudentId == studentId.Value : true);
                }
                else
                {
                    searchPredicate = e => e.Status.Contains(keyword);
                }
            }

            var result = await _unitOfWork.Enrollments.GetAllAsync(parameters, searchPredicate);

            var responses = result.Items.Select(MapToResponseModel).ToList();

            var pagination = new PaginationMetadataModel
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };

            return PagedResponseModel<EnrollmentResponseModel>.SuccessResponse(responses, pagination);
        }

        public async Task<ResponseModel<EnrollmentResponseModel>> GetEnrollmentByIdAsync(int id)
        {
            var entity = await _unitOfWork.Enrollments.GetByIdAsync(id, "student,course");
            if (entity == null)
            {
                return ResponseModel<EnrollmentResponseModel>.ErrorResponse($"Enrollment with id {id} not found");
            }
            return ResponseModel<EnrollmentResponseModel>.SuccessResponse(MapToResponseModel(entity));
        }

        public async Task<ResponseModel<EnrollmentResponseModel>> CreateEnrollmentAsync(EnrollmentRequestModel model)
        {
            var entity = new Enrollment
            {
                StudentId = model.StudentId,
                CourseId = model.CourseId,
                EnrollDate = model.EnrollDate,
                Status = model.Status
            };
            var created = await _unitOfWork.Enrollments.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var fullEntity = await _unitOfWork.Enrollments.GetByIdAsync(created.EnrollmentId, "student,course");
            return ResponseModel<EnrollmentResponseModel>.SuccessResponse(MapToResponseModel(fullEntity ?? created), "Enrollment created successfully");
        }

        public async Task<ResponseModel<EnrollmentResponseModel>> UpdateEnrollmentAsync(int id, EnrollmentRequestModel model)
        {
            if (!await _unitOfWork.Enrollments.ExistsAsync(id))
            {
                return ResponseModel<EnrollmentResponseModel>.ErrorResponse($"Enrollment with id {id} not found");
            }

            var entity = new Enrollment
            {
                EnrollmentId = id,
                StudentId = model.StudentId,
                CourseId = model.CourseId,
                EnrollDate = model.EnrollDate,
                Status = model.Status
            };

            var updated = await _unitOfWork.Enrollments.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var fullEntity = await _unitOfWork.Enrollments.GetByIdAsync(id, "student,course");
            return ResponseModel<EnrollmentResponseModel>.SuccessResponse(MapToResponseModel(fullEntity ?? updated), "Enrollment updated successfully");
        }

        public async Task<ResponseModel<bool>> DeleteEnrollmentAsync(int id)
        {
            if (!await _unitOfWork.Enrollments.ExistsAsync(id))
            {
                return ResponseModel<bool>.ErrorResponse($"Enrollment with id {id} not found");
            }

            var deleted = await _unitOfWork.Enrollments.DeleteAsync(id);
            if (deleted)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return ResponseModel<bool>.SuccessResponse(deleted, "Enrollment deleted successfully");
        }

        private static EnrollmentResponseModel MapToResponseModel(Enrollment entity) => new()
        {
            EnrollmentId = entity.EnrollmentId,
            StudentId = entity.StudentId,
            CourseId = entity.CourseId,
            EnrollDate = entity.EnrollDate,
            Status = entity.Status,
            Student = entity.Student != null ? new StudentResponseModel
            {
                StudentId = entity.Student.StudentId,
                FullName = entity.Student.FullName,
                Email = entity.Student.Email,
                DateOfBirth = entity.Student.DateOfBirth
            } : null,
            Course = entity.Course != null ? new CourseResponseModel
            {
                CourseId = entity.Course.CourseId,
                CourseName = entity.Course.CourseName,
                SemesterId = entity.Course.SemesterId,
                SubjectId = entity.Course.SubjectId
            } : null
        };
    }
}
