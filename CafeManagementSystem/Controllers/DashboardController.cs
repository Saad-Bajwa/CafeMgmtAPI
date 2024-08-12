using CafeManagementSystem.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CafeManagementSystem.Controllers
{
    [RoutePrefix("api/dashboard")]
    public class DashboardController : ApiController
    {
        CafeMgmtSystemEntities db = new CafeMgmtSystemEntities();
        Response response = new Response();
        [HttpGet, Route("details")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetDetails()
        {
            try
            {
                var data = new
                {
                    category = db.Categories.Count(),
                    product = db.Products.Count(),
                    bills = db.Bills.Count(),
                    users = db.Users.Count()
                };
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }

        [HttpGet, Route("getAllOrdersData")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetAllOrdersData()
        {
            try
            {
                // Get the token from the request headers
                var token = Request.Headers.GetValues("authorization").First();

                // Validate the token and retrieve claims
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);

                // Check if the user role is not "admin"
                if (tokenClaim.Role != "admin")
                {
                    // If the user is not an admin, return only their orders
                    var userOrders = db.Bills
                        .Where(o => o.email == tokenClaim.Email)
                        .Select(o => new
                        {
                            Name = o.name,
                            TotalAmount = o.totalAmount
                        })
                        .ToList().
                        Select(u => new OrderDTO
                        {
                            Name = u.Name,
                            TotalAmount = Convert.ToDouble(u.TotalAmount)
                        });

                    return Request.CreateResponse(HttpStatusCode.OK, userOrders);
                }

                // If the user is an admin, return aggregated orders for all customers
                var aggregatedOrders = db.Bills.GroupBy(o => o.email).Select(g => new
                {
                    Name = g.Select(o => o.name).FirstOrDefault(),
                    TotalAmount = g.Sum(o => o.totalAmount)
                }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, aggregatedOrders);
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet, Route("getOrderCountByCustomer")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetOrderCountByCustomer()
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);

                if (tokenClaim.Role != "admin")
                {
                    var orders = db.Bills
                                    .Where(o => o.email == tokenClaim.Email)
                                    .Select(g => new
                                    {
                                        productDetails = g.productDetails,
                                        Name = g.name
                                    }).ToList();

                    var productDetails = orders
                        .SelectMany(order => JsonConvert.DeserializeObject<List<ProductDetail>>(order.productDetails))
                        .GroupBy(p => p.product)
                        .Select(g => new
                        {
                            Name = g.Key,
                            OrderCount = g.Sum(p => p.quantity)
                        })
                        .ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, productDetails);
                }
                else
                {
                    var result = db.Bills
                        .GroupBy(o => o.email)
                        .Select(g => new
                        {
                            Name = g.Select(o => o.name).FirstOrDefault(),
                            OrderCount = g.Count()
                        })
                        .ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error in processing request");
                throw ex;
            }
        }

    }
    public class OrderDTO
    {
        public string Name { get; set; }
        public double TotalAmount { get; set; }
    }

    public class ProductDetail
    {
        public string category { get; set; }
        public string product { get; set; }
        public double  price { get; set; }
        public int quantity { get; set; }
        public double amount { get; set; }
    }
}
