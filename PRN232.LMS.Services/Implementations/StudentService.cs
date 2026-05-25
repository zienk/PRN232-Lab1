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
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResponseModel<object>> GetStudentsAsync(string? search, string? sort, int page, int size, string? fields, string? expand)
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
                (System.Linq.Expressions.Expression<Func<Student, bool>>)(s =>
                    s.FullName.Contains(parameters.Search) ||
                    s.Email.Contains(parameters.Search));

            var result = await _unitOfWork.Students.GetAllAsync(parameters, searchPredicate);

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

        public async Task<ResponseModel<StudentResponseModel>> GetStudentByIdAsync(int id)
        {
            var entity = await _unitOfWork.Students.GetByIdAsync(id, "enrollments");
            if (entity == null)
            {
                return ResponseModel<StudentResponseModel>.ErrorResponse($"Student with id {id} not found");
            }
            return ResponseModel<StudentResponseModel>.SuccessResponse(MapToResponseModel(entity));
        }

        public async Task<ResponseModel<StudentResponseModel>> CreateStudentAsync(StudentRequestModel model)
        {
            var entity = new Student
            {
                FullName = model.FullName,
                Email = model.Email,
                DateOfBirth = model.DateOfBirth
            };
            var created = await _unitOfWork.Students.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var fullEntity = await _unitOfWork.Students.GetByIdAsync(created.StudentId, "enrollments");
            return ResponseModel<StudentResponseModel>.SuccessResponse(MapToResponseModel(fullEntity ?? created), "Student created successfully");
        }

        public async Task<ResponseModel<StudentResponseModel>> UpdateStudentAsync(int id, StudentRequestModel model)
        {
            if (!await _unitOfWork.Students.ExistsAsync(id))
            {
                return ResponseModel<StudentResponseModel>.ErrorResponse($"Student with id {id} not found");
            }

            var entity = new Student
            {
                StudentId = id,
                FullName = model.FullName,
                Email = model.Email,
                DateOfBirth = model.DateOfBirth
            };

            var updated = await _unitOfWork.Students.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var fullEntity = await _unitOfWork.Students.GetByIdAsync(id, "enrollments");
            return ResponseModel<StudentResponseModel>.SuccessResponse(MapToResponseModel(fullEntity ?? updated), "Student updated successfully");
        }

        public async Task<ResponseModel<bool>> DeleteStudentAsync(int id)
        {
            if (!await _unitOfWork.Students.ExistsAsync(id))
            {
                return ResponseModel<bool>.ErrorResponse($"Student with id {id} not found");
            }

            var deleted = await _unitOfWork.Students.DeleteAsync(id);
            if (deleted)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return ResponseModel<bool>.SuccessResponse(deleted, "Student deleted successfully");
        }

        private static StudentResponseModel MapToResponseModel(Student entity) => new()
        {
            StudentId = entity.StudentId,
            FullName = entity.FullName,
            Email = entity.Email,
            DateOfBirth = entity.DateOfBirth,
            Enrollments = entity.Enrollments?.Select(e => new EnrollmentResponseModel
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status
            }).ToList()
        };
    }
}
