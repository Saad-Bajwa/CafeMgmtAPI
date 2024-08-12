using CafeManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CafeManagementSystem.Controllers
{
    [RoutePrefix("api/product")]
    public class ProductController : ApiController
    {
        CafeMgmtSystemEntities db = new CafeMgmtSystemEntities();
        Response response = new Response();
        [HttpPost,Route("addProduct")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage AddProduct(Product product)
        {
            try
            {
                var toekn = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(toekn);
                if(tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                product.status = "true";
                db.Products.Add(product);
                db.SaveChanges();
                response.Message = "Product Added Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }

        [HttpGet,Route("getAllProduct")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetAllProduct()
        {
            try
            {
                var result = from Products in db.Products
                             join Category in db.Categories
                             on Products.categoryId equals Category.id
                             select new
                             {
                                 Products.id,
                                 Products.name,
                                 Products.description,
                                 Products.price,
                                 Products.status,
                                 categoryId = Category.id,
                                 categoryName = Category.name
                             };
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }

        [HttpGet,Route("getProductByCategory/{id}")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetProductByCategory(int id)
        {
            try
            {
                var res = db.Products.Where(x => x.categoryId == id && x.status == "true").ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }
        [HttpGet,Route("getProductById/{id}")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetProductById(int id)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, db.Products.Find(id));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex);
                throw;
            }
        }

        [HttpPut,Route("updateProduct")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage UpdateProduct([FromBody] Product product)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Product proObj = db.Products.Find(product.id);
                if(proObj == null)
                {
                    response.Message = "Product Not Found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                proObj.name = product.name;
                proObj.description = product.description;
                proObj.categoryId = product.categoryId;
                proObj.price = product.price;
                db.Entry(proObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.Message = "Product updated Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }
        [HttpDelete,Route("deleteProduct/{Id}")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage DeleteProduct(int id)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Product pro = db.Products.Find(id);
                if(pro == null)
                {
                    response.Message = "Product Not Found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                db.Products.Remove(pro);
                db.SaveChanges();
                response.Message = "Product Deleted Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }
        [HttpPost,Route("updateProductStatus")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage UpdateProductStatus([FromBody] Product product)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Product pro = db.Products.Find(product.id);
                if (pro == null)
                {
                    response.Message = "Product Not Found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                pro.status = product.status;
                db.Entry(pro).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.Message = "Product Status Updated";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }

    }
}
