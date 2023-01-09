using Exemple.Domain;
using Exemple.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using Example.Api.Models;
using Exemple.Domain.Models;
using Microsoft.Data.SqlClient;

namespace Example.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GradesController : ControllerBase
    {
        private ILogger<GradesController> logger;

        public GradesController(ILogger<GradesController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGrades([FromServices] IGradesRepository gradesRepository) =>
            await gradesRepository.TryGetExistingGrades().Match(
               Succ: GetAllGradesHandleSuccess,
               Fail: GetAllGradesHandleError
            );

        private ObjectResult GetAllGradesHandleError(Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return base.StatusCode(StatusCodes.Status500InternalServerError, "UnexpectedError");
        }

        private OkObjectResult GetAllGradesHandleSuccess(List<Exemple.Domain.Models.CalculatedSudentGrade> grades) =>
        Ok(grades.Select(grade => new
        {
            StudentRegistrationNumber = grade.CommandId,
            grade.Name,
            grade.Quantity,
            grade.Subtotal
        }));

        [HttpPost]
        public async Task<IActionResult> PublishGrades([FromServices]PublishGradeWorkflow publishGradeWorkflow, [FromBody]InputGrade[] grades)
        {
            var unvalidatedGrades = grades.Select(MapInputGradeToUnvalidatedGrade)
                                          .ToList()
                                          .AsReadOnly();
            PublishGradesCommand command = new(unvalidatedGrades);
            var result = await publishGradeWorkflow.ExecuteAsync(command);
            return result.Match<IActionResult>(
                whenExamGradesPublishFaildEvent: failedEvent => StatusCode(StatusCodes.Status500InternalServerError, failedEvent.Reason),
                whenExamGradesPublishScucceededEvent: successEvent => Ok()
            );
        }


        [HttpDelete]
        public async Task<IActionResult> CancelGrades([FromServices]PublishGradeWorkflow publishGradeWorkflow, [FromBody]InputGrade[] grades)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                builder.DataSource = "tcp:pssc2022.database.windows.net"; 
                builder.UserID = "denis";            
                builder.Password = "Pssc2022@";     
                builder.InitialCatalog = "Students";
            try 
            {
         
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {                    
                    connection.Open();       
                    String sql = "Delete FROM dbo.Command";

                    using (SqlCommand commanding = new SqlCommand(sql, connection))
                    {
                        commanding.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            return Ok();
        }

        [HttpGet("1")]
        public async Task<IActionResult> GetReceipt([FromServices] IGradesRepository gradesRepository, IStudentsRepository studentsRepository) =>
            await gradesRepository.TryGetExistingGrades().Match(
               Succ: GetReceiptsSuccess,
               Fail: GetReceiptsError
            );

        private ObjectResult GetReceiptsError(Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return base.StatusCode(StatusCodes.Status500InternalServerError, "UnexpectedError");
        }

        private OkObjectResult GetReceiptsSuccess(List<Exemple.Domain.Models.CalculatedSudentGrade> grades) =>
        Ok(grades.Select(grade => new
        {
            StudentRegistrationNumber = grade.CommandId,
            grade.Name,
            grade.Quantity,
            grade.Subtotal
        }));

        private static UnvalidatedStudentGrade MapInputGradeToUnvalidatedGrade(InputGrade grade) => new UnvalidatedStudentGrade(
            Name: grade.RegistrationNumber,
            Quantity: grade.Exam,
            Subtotal: grade.Activity);
    }
}
