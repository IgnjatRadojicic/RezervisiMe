using System;
using System.Collections.Generic;
using RezervisiMe.RezervisiMe.API.Models;

namespace RezervisiMe.RezervisiMe.API.Repositories.Interfaces
{
    public interface IRepository<T> where T : EntityBase
    {
        IEnumerable<T> GetAll();                  
        IEnumerable<T> GetAllIncludingDeleted();
        T GetById(Guid id);                      
        T Add(T entity);                        
        bool Update(T entity);                   
        bool SoftDelete(Guid id);                
    }
}