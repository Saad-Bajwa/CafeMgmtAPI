using CafeManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CafeManagementSystem.Controllers
{
    [RoutePrefix("api/category")]
    public class CategoryController : ApiController
    {
        CafeMgmtSystemEntities db = new CafeMgmtSystemEntities();
        Response response = new Response();

        [HttpPost,Route("addCategory")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage AddCategory([FromBody] Category category)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if(tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                db.Categories.Add(category);
                db.SaveChanges();
                response.Message = "Category Added Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }

        [HttpGet,Route("getCategories")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetCategories()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, db.Categories.ToList());
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }
        [HttpPut,Route("updateCategory")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage UpdateCategory(Category category)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Category catObj = db.Categories.Find(category.id);
                if(catObj == null)
                {
                    response.Message = "Category not found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                catObj.name = category.name;
                db.Entry(catObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.Message = "Category updated Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }

        [HttpDelete,Route("deleteCategory/{id}")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage DeleteCategory(int id)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if(tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Category catObj = db.Categories.Find(id);
                if (catObj == null)
                {
                    response.Message = "Category not found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                db.Categories.Remove(catObj);
                var res = db.Products.Where(x=>x.categoryId == id).ToList();
                db.Products.RemoveRange(res);
                db.SaveChanges();
                response.Message = "Category Deleted Successfully";
                return Request.CreateResponse(HttpStatusCode.OK,response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }

        [HttpGet,Route("getCategoryById/{id}")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetCategoryById(int id)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Category catObj = db.Categories.Find(id);
                return Request.CreateResponse(HttpStatusCode.OK, catObj);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }

    }
}
