using FluentAssertions;
using GeldApp2.Application.Commands;
using GeldApp2.Application.Commands.Expense;
using GeldApp2.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GeldApp2.IntegrationTests
{
    public static class TestHelpers
    {
        public static CreateExpenseCommand AsCommand(this Expense expense, string accountName)
        {
            var cmd = new CreateExpenseCommand
            {
                AccountName = accountName,
                Amount = expense.Amount,
                CategoryName = expense.Category,
                SubcategoryName = expense.Subcategory,
                Created = expense.Created.LocalDateTime,
                Date = expense.Date,
                Details = expense.Details,
                Type = expense.Type,
            };

            return cmd;
        }

        public static StringContent AsContent(this object o)
        {
            return new StringContent(JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");
        }

        public static async Task<T> AsAsync<T>(this HttpResponseMessage resp)
        {
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var str = await resp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(str);
        }

        public static void ShouldBeOk(this HttpResponseMessage resp)
        {
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public static void ShouldBeForbidden(this HttpResponseMessage resp)
        {
            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        public static void IsUnauthorized(this HttpResponseMessage resp)
        {
            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        public static void ShouldFail(this HttpResponseMessage resp)
        {
            resp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        public static void ShouldClientFail(this HttpResponseMessage resp)
        {
            resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        public static async Task<JwtSecurityToken> GetJwtTokenAsync(this HttpResponseMessage resp)
        {
            var json = await resp.Content.ReadAsStringAsync();
            var tokenStr = JObject.Parse(json)["token"].ToString();
            var tokenService = new JwtSecurityTokenHandler();
            var token = tokenService.ReadJwtToken(tokenStr);
            return token;
        }
    }
}
