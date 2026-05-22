using Microsoft.AspNetCore.Mvc;
using Kursova2.Services.Microservices;
using Kursova2.Services.Strategies;
using System.Data;
using System;
using System.Collections.Generic;

namespace Kursova2.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LibraryController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IDataService _data;
        private readonly IAnalyticsService _analytics;
        private readonly IReservationService _reservation;
        private readonly ILoanService _loan;

        public LibraryController(IAuthService auth, IDataService data, IAnalyticsService analytics, IReservationService reservation, ILoanService loan)
        {
            _auth = auth;
            _data = data;
            _analytics = analytics;
            _reservation = reservation;
            _loan = loan;
        }

        // --- DATA MICROSERVICE ---
        [HttpGet("tables/{isAdmin}")]
        public IActionResult GetAvailableTables(bool isAdmin) => Ok(_data.GetAvailableTables(isAdmin));

        [HttpGet("table/{tableName}")]
        public IActionResult GetTableData(string tableName) => Ok(_data.GetTableData(tableName));

        [HttpGet("search/{tableName}/{columnName}/{keyword}")]
        public IActionResult SearchInTable(string tableName, string columnName, string keyword)
            => Ok(_data.SearchInTable(tableName, columnName, keyword));

        [HttpPost("table/save/{tableName}")]
        public IActionResult SaveChanges(string tableName, [FromBody] DataTable changes)
        {
            _data.SaveChanges(tableName, changes);
            return Ok();
        }

        // --- AUTH MICROSERVICE ---
        [HttpPost("auth/register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            try
            {
                _auth.RegisterReader(req.FullName, req.Email, req.Address, req.HomePhone, req.WorkPhone);
                return Ok(new { Message = "Вас успішно зареєстровано в системі!" });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [HttpGet("auth/reader/{name}")]
        public IActionResult GetReaderInfo(string name) => Ok(_auth.GetReaderInfo(name));

        // --- RESERVATION MICROSERVICE ---
        [HttpPost("reservation/reserve")]
        public IActionResult ReserveBook([FromBody] ReservationRequest req)
        {
            try
            {
                string result = _reservation.ReserveBook(req.BookId, req.ReaderName);
                return Ok(new { Message = result });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [HttpGet("reservation/user/{name}")]
        public IActionResult GetUserReservations(string name) => Ok(_reservation.GetUserReservations(name));

        [HttpPost("reservation/cancel/{id}")]
        public IActionResult CancelReservation(int id) => Ok(new { Message = _reservation.CancelReservation(id) });

        // --- LOAN MICROSERVICE ---
        [HttpPost("loan/return/{id}")]
        public IActionResult ReturnBook(int id) => Ok(new { Message = _loan.ReturnBook(id) });

        [HttpPost("loan/amnesty/{enable}")]
        public IActionResult SetAmnesty(bool enable)
        {
            if (enable) _loan.SetFineStrategy(new AmnestyFineStrategy());
            else _loan.SetFineStrategy(new StandardFineStrategy());
            return Ok();
        }

        // --- ANALYTICS MICROSERVICE ---
        [HttpGet("analytics/grouped-genre")]
        public IActionResult GetBooksGroupedByGenre() => Ok(_analytics.GetBooksGroupedByGenre());

        [HttpGet("analytics/rankings")]
        public IActionResult GetBookRankings() => Ok(_analytics.GetBookRankings());

        [HttpGet("analytics/authors")]
        public IActionResult GetBooksByAuthors() => Ok(_analytics.GetBooksByAuthors());

        [HttpGet("analytics/fines")]
        public IActionResult GetFinesSummary() => Ok(_analytics.GetFinesSummary());

        [HttpGet("analytics/user-fines/{name}")]
        public IActionResult GetUserFines(string name) => Ok(_analytics.GetUserFines(name));
    }

    // DTO классы для приема данных в формате JSON
    public class RegisterRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string HomePhone { get; set; } = string.Empty;
        public string WorkPhone { get; set; } = string.Empty;
    }

    public class ReservationRequest
    {
        public int BookId { get; set; }
        public string ReaderName { get; set; } = string.Empty;
    }
}